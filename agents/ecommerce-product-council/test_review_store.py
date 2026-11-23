"""Regression tests for durable human authorization."""
import tempfile
import unittest
from pathlib import Path

from council import council, sourcing_handoff
from review_store import ReviewStore


def handoff():
    assessment = {
        "score": 10, "pros": [], "cons": ["weak economics"],
        "risks": ["unknown certification"], "uncertainties": [],
        "evidence_references": [], "supplier_validation_questions": [],
        "recommended_mitigations": [],
    }
    candidates = [
        {"product_id": "a", "specialist_assessments": {
            key: dict(assessment) for key in ("commercial", "market", "competitive", "risk", "operations")}},
        {"product_id": "b", "specialist_assessments": {
            key: dict(assessment) for key in ("commercial", "market", "competitive", "risk", "operations")}},
    ]
    result = council("batch-1", candidates)
    assert result["selected_product_id"] == "a"
    return sourcing_handoff(result, [{"product_id": "a", "status": "blocked",
                                     "evidence_references": [], "reason": "supplier not verified"}])


class ReviewStoreTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.store = ReviewStore(Path(self.temp.name) / "review.sqlite3")
        self.data = handoff()
        self.record = self.store.submit("review-1", self.data)

    def tearDown(self):
        self.temp.cleanup()

    def test_pending_denies_and_approval_allows(self):
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)
        version = self.record["evidence_version"]
        decision = self.store.decide("review-1", "approve", "human-1", "Accept risk", version)
        self.assertTrue(decision["approved_for_onboarding"])
        self.assertEqual(self.store.authorize("review-1", self.data), decision)

    def test_reject_and_more_information_block(self):
        version = self.record["evidence_version"]
        self.store.decide("review-1", "reject", "human-1", "No supplier", version)
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)
        self.store.decide("review-1", "request_more_information", "human-1", "Need quote", version)
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", self.data)

    def test_changed_evidence_invalidates_approval(self):
        version = self.record["evidence_version"]
        self.store.decide("review-1", "approve", "human-1", "Accept", version)
        changed = sourcing_handoff(self.data["council_result"],
                                   [{"product_id": "a", "status": "unverified", "evidence_references": []}])
        with self.assertRaises(ValueError):
            self.store.authorize("review-1", changed)
        with self.assertRaises(ValueError):
            self.store.submit("review-1", changed)

    def test_idempotent_replay_and_persistence(self):
        version = self.record["evidence_version"]
        first = self.store.decide("review-1", "approve", "human-1", "Accept", version)
        second = self.store.decide("review-1", "approve", "human-1", "Accept", version)
        self.assertEqual(first, second)
        reopened = ReviewStore(Path(self.temp.name) / "review.sqlite3")
        self.assertEqual(len(reopened.get("review-1")["decisions"]), 1)
        self.assertEqual(reopened.authorize("review-1", self.data), first)

    def test_wrong_version_denied(self):
        with self.assertRaises(ValueError):
            self.store.decide("review-1", "approve", "human-1", "Accept", "wrong")


if __name__ == "__main__":
    unittest.main()
