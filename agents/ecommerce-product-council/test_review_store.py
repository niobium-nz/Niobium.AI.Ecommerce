"""Offline regression tests for evidence-bound Drive persistence."""
from copy import deepcopy
import unittest

from council import council, sourcing_handoff
from review_store import ReviewStore, ReviewConflictError, ReviewIntegrityError, _seal, _digest, _semantic
from test_council import candidate


def handoff():
    result = council("batch-1", [candidate("a", 10), candidate("b", 10)])
    return sourcing_handoff(result, [{"product_id": "a", "status": "blocked",
                                     "evidence_references": [], "reason": "supplier not verified"}])


class MemoryDrive:
    """Create-only fake with independently stored metadata and serialized bodies."""

    def __init__(self):
        self.files = []
        self.before_create = None
        self.lose_response = False
        self.duplicate_create = False
        self.reverse_listing = False

    def list_files(self, folder_id, properties):
        files = [deepcopy(f) for f in self.files if f["folder_id"] == folder_id
                 and all(f["properties"].get(k) == v for k, v in properties.items())]
        return list(reversed(files)) if self.reverse_listing else files

    def read_json(self, file_id):
        return deepcopy(next(f["body"] for f in self.files if f["id"] == file_id))

    def create_json(self, folder_id, name, properties, body):
        if self.before_create:
            hook, self.before_create = self.before_create, None
            hook()
        file = {"id": str(len(self.files) + 1), "folder_id": folder_id, "name": name,
                "properties": deepcopy(properties), "body": deepcopy(body)}
        self.files.append(file)
        if self.duplicate_create:
            self.files.append({**deepcopy(file), "id": str(len(self.files) + 1)})
        if self.lose_response:
            self.lose_response = False
            raise TimeoutError("write committed but response lost")
        return file["id"]


class ReviewStoreTests(unittest.TestCase):
    def setUp(self):
        self.drive = MemoryDrive()
        self.store = ReviewStore(self.drive, "reviews")
        self.data = handoff()
        self.record = self.store.submit("review-1", self.data)
        self.version = self.record["evidence_version"]

    def decide(self, action="approve", actor="human-1", rationale="Reviewed risks", **kwargs):
        return self.store.decide("review-1", action, actor, rationale, self.version, **kwargs)

    def race(self):
        self.drive.before_create = lambda: self.decide("reject", actor="human-2", expected_head_event_id=None)
        with self.assertRaises(ReviewConflictError):
            self.decide(expected_head_event_id=None)
        return self.store.inspect("review-1")

    def test_pending_denies_and_approval_allows(self):
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)
        decision = self.decide(expected_head_event_id=None)
        self.assertTrue(decision["approved_for_onboarding"])
        self.assertEqual(self.store.authorize("review-1", self.data), decision)

    def test_reject_and_more_information_block(self):
        self.decide("reject")
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)
        self.decide("request_more_information")
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)

    def test_changed_evidence_invalidates_approval(self):
        self.decide()
        changed = deepcopy(self.data)
        changed["supplier_findings"][0]["status"] = "unverified"
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", changed)
        with self.assertRaises(ValueError):
            self.store.submit("review-1", changed)

    def test_idempotent_replay_and_reopened_store(self):
        first = self.decide()
        self.assertEqual(self.decide(), first)
        reopened = ReviewStore(self.drive, "reviews")
        self.assertEqual(len(reopened.get("review-1")["decisions"]), 1)
        self.assertEqual(reopened.authorize("review-1", self.data), first)

    def test_wrong_version_denied(self):
        with self.assertRaises(ValueError):
            self.store.decide("review-1", "approve", "human", "Reviewed", "wrong")
        self.assertEqual(len(self.drive.files), 1)

    def test_same_action_after_intervening_action_is_new_event(self):
        first = self.decide()
        self.decide("reject")
        third = self.decide()
        self.assertNotEqual(first["event_id"], third["event_id"])
        self.assertEqual(len(self.store.get("review-1")["decisions"]), 3)

    def test_stale_head_is_rejected_without_write(self):
        first = self.decide()
        with self.assertRaises(ReviewConflictError):
            self.decide("reject", expected_head_event_id=None)
        self.assertEqual(len(self.drive.files), 2)
        self.decide("reject", expected_head_event_id=first["event_id"])
        with self.assertRaises(ReviewConflictError):
            self.decide(expected_head_event_id=first["event_id"])

    def test_original_head_retry_returns_same_event(self):
        first = self.decide(expected_head_event_id=None)
        self.assertEqual(self.decide(expected_head_event_id=None), first)
        self.assertEqual(len(self.drive.files), 2)

    def test_old_approval_retry_does_not_override_later_rejection(self):
        self.decide(expected_head_event_id=None)
        self.decide("reject")
        with self.assertRaises(ReviewConflictError):
            self.decide(expected_head_event_id=None)
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)

    def test_lost_response_is_idempotent(self):
        self.drive.lose_response = True
        with self.assertRaises(TimeoutError):
            self.decide(expected_head_event_id=None)
        decision = self.decide(expected_head_event_id=None)
        self.assertEqual(len(self.drive.files), 2)
        self.assertEqual(self.store.authorize("review-1", self.data), decision)

    def test_duplicate_physical_files_are_logically_deduplicated(self):
        self.drive.duplicate_create = True
        self.decide()
        self.assertEqual(len(self.drive.files), 3)
        self.assertEqual(len(self.store.get("review-1")["decisions"]), 1)
        self.drive.files.append(deepcopy(self.drive.files[0]))
        self.assertEqual(self.store.submit("review-1", self.data)["evidence_version"], self.version)
        self.assertEqual(len(self.store.get("review-1")["decisions"]), 1)

    def test_concurrent_identical_retry_has_one_logical_event(self):
        self.drive.before_create = lambda: self.decide(expected_head_event_id=None)
        decision = self.decide(expected_head_event_id=None)
        self.assertEqual(len(self.drive.files), 3)
        self.assertEqual(len(self.store.get("review-1")["decisions"]), 1)
        self.assertEqual(self.store.authorize("review-1", self.data), decision)

    def test_concurrent_conflicting_decisions_fail_closed(self):
        record = self.race()
        self.assertTrue(record["conflicted"])
        self.assertIsNone(record["approved_for_onboarding"])
        self.assertEqual(len(record["head_event_ids"]), 2)
        for operation in (lambda: self.store.get("review-1"),
                          lambda: self.store.authorize("review-1", self.data), self.decide):
            with self.assertRaises(ReviewConflictError):
                operation()
        self.assertTrue(self.store.pending()[0]["conflicted"])

    def test_human_resolves_all_conflict_heads_and_retains_audit(self):
        record = self.race()
        decision = self.store.resolve("review-1", "approve", "human-3", "Reviewed both decisions",
                                      self.version, expected_head_event_ids=record["head_event_ids"])
        self.assertEqual(decision["parent_event_ids"], record["head_event_ids"])
        self.assertEqual(len(self.store.get("review-1")["decisions"]), 3)
        self.assertEqual(self.store.authorize("review-1", self.data), decision)
        retry = self.store.resolve("review-1", "approve", "human-3", "Reviewed both decisions",
                                   self.version, expected_head_event_ids=record["head_event_ids"])
        self.assertEqual(retry, decision)
        self.assertEqual(len(self.drive.files), 4)

    def test_resolution_requires_exact_current_heads(self):
        record = self.race()
        for heads in (record["head_event_ids"][:1], record["head_event_ids"] + ["x" * 64],
                      [record["head_event_ids"][0]] * 2):
            with self.assertRaises(ValueError):
                self.store.resolve("review-1", "approve", "human", "Reviewed", self.version,
                                   expected_head_event_ids=heads)
        self.assertEqual(len(self.drive.files), 3)

    def test_concurrent_resolutions_conflict_again(self):
        record = self.race()
        def resolve(action):
            return self.store.resolve("review-1", action, "human-3", "Reviewed", self.version,
                                      expected_head_event_ids=record["head_event_ids"])
        self.drive.before_create = lambda: resolve("reject")
        with self.assertRaises(ReviewConflictError):
            resolve("approve")
        with self.assertRaises(ReviewConflictError):
            self.store.authorize("review-1", self.data)
        self.assertEqual(len(self.store.inspect("review-1")["head_event_ids"]), 2)

    def test_manifest_tampering_is_detected(self):
        self.drive.files[0]["body"]["handoff"]["supplier_findings"] = []
        with self.assertRaises(ReviewIntegrityError):
            self.store.get("review-1")

    def test_evidence_fingerprint_is_recalculated(self):
        body = self.drive.files[0]["body"]
        body["handoff"]["supplier_findings"] = []
        self.drive.files[0]["body"] = _seal({k: v for k, v in body.items() if k != "fingerprint"})
        with self.assertRaises(ReviewIntegrityError):
            self.store.get("review-1")

    def test_decision_tampering_is_detected(self):
        self.decide()
        self.drive.files[-1]["body"]["decision"]["rationale"] = "Changed"
        with self.assertRaises(ReviewIntegrityError):
            self.store.authorize("review-1", self.data)

    def test_semantic_event_id_is_recalculated(self):
        self.decide()
        body = self.drive.files[-1]["body"]
        body["decision"]["actor"] = "someone-else"
        self.drive.files[-1]["body"] = _seal({k: v for k, v in body.items() if k != "fingerprint"})
        with self.assertRaises(ReviewIntegrityError):
            self.store.get("review-1")

    def test_forged_approval_flag_is_not_accepted(self):
        self.decide("reject")
        file = self.drive.files[-1]
        body = file["body"]
        body["decision"]["approved_for_onboarding"] = True
        body["event_id"] = _digest(_semantic(body))
        file["properties"]["event"] = body["event_id"]
        file["body"] = _seal({k: v for k, v in body.items() if k != "fingerprint"})
        with self.assertRaises(ReviewIntegrityError):
            self.store.authorize("review-1", self.data)

    def test_metadata_mismatch_is_not_ignored(self):
        self.decide()
        self.drive.files[-1]["properties"]["schema"] = "999"
        with self.assertRaises(ReviewIntegrityError):
            self.store.authorize("review-1", self.data)

    def test_missing_parent_is_not_treated_as_new_approval(self):
        self.decide("reject")
        self.decide()
        del self.drive.files[1]
        with self.assertRaises(ReviewIntegrityError):
            self.store.authorize("review-1", self.data)

    def test_conflicting_manifest_retry_is_rejected(self):
        changed = deepcopy(self.data)
        changed["supplier_findings"] = []
        other = MemoryDrive()
        ReviewStore(other, "reviews").submit("review-1", changed)
        file = deepcopy(other.files[0])
        file["id"] = "conflicting-manifest"
        self.drive.files.append(file)
        with self.assertRaises(ReviewIntegrityError):
            self.store.get("review-1")

    def test_pending_queue_and_folder_isolation(self):
        self.store.submit("approved", self.data)
        self.store.decide("approved", "approve", "human", "Reviewed", self.version)
        self.store.submit("rejected", self.data)
        self.store.decide("rejected", "reject", "human", "Reviewed", self.version)
        self.store.submit("more-info", self.data)
        self.store.decide("more-info", "request_more_information", "human", "Need quote", self.version)
        ReviewStore(self.drive, "other-folder").submit("unrelated", self.data)
        self.assertEqual({r["review_id"] for r in self.store.pending()}, {"review-1", "more-info"})

    def test_listing_order_and_caller_mutation_do_not_change_history(self):
        self.decide("reject")
        decision = self.decide()
        self.drive.reverse_listing = True
        self.record["handoff"]["supplier_findings"] = []
        self.assertEqual(self.store.authorize("review-1", self.data), decision)

    def test_unknown_review_and_invalid_input(self):
        with self.assertRaises(KeyError):
            self.store.get("missing")
        for rid, data in ((" ", self.data), ("new", {}), ("new", {**self.data, "selected_product_id": ""}),
                          ("new", {**self.data, "cost": float("nan")})):
            with self.assertRaises(ValueError):
                self.store.submit(rid, data)
        for action, actor, rationale in (("auto-approve", "human", "Reviewed"),
                                         ("approve", "", "Reviewed"), ("approve", "human", "")):
            with self.assertRaises(ValueError):
                self.decide(action, actor, rationale)

    def test_listing_failure_propagates_and_never_authorizes(self):
        self.decide()
        def fail(*args):
            raise PermissionError("Drive credentials revoked")
        self.drive.list_files = fail
        with self.assertRaises(PermissionError):
            self.store.authorize("review-1", self.data)

    def test_invisible_write_does_not_return_local_approval(self):
        listing = self.drive.list_files
        self.drive.list_files = lambda *args: [f for f in listing(*args) if f["body"]["kind"] == "review"]
        with self.assertRaises(ReviewIntegrityError):
            self.decide()


if __name__ == "__main__":
    unittest.main()
