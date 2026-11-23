from copy import deepcopy
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from duplicate_claims import make_plan, read_legacy_ids, record_filename


JOB = "96d59e56-bddf-443d-ac57-d6b633d053d2"
SIGNAL = "409a267a-deed-48ff-9578-1ebb08beff2c"
FIRST = "b83ac34f-339f-4b8f-9ffe-2eae6958e64d"
SECOND = "93cbb539-5339-4505-b7f6-50c5e36cc3c0"
SHA = "a" * 64


class NamedRangeIndex:
    """Model the provider's create/update preconditions and atomic batches."""

    def __init__(self):
        self.ranges = {}

    def apply(self, plan):
        prospective = deepcopy(self.ranges)
        for request in plan["requests"]:
            if "addNamedRange" in request:
                item = request["addNamedRange"]["namedRange"]
                key = item["namedRangeId"]
                if key in prospective:
                    raise ValueError("Range ID already exists")
                prospective[key] = dict(item)
            elif "updateNamedRange" in request:
                item = request["updateNamedRange"]["namedRange"]
                key = item["namedRangeId"]
                if key not in prospective:
                    raise ValueError("Range ID missing")
                prospective[key]["range"] = item["range"]
            else:
                prospective.pop(request["deleteNamedRange"]["namedRangeId"], None)
        self.ranges = prospective


class DuplicateClaimTests(unittest.TestCase):
    def plan(self, operation, ids, owner=FIRST, digest=SHA, namespace="researcher_ads"):
        return make_plan(operation, namespace, ids, 42, attempt_id=owner, job_id=JOB,
                         signal_id=SIGNAL, artifact_sha256=digest)

    def test_only_overlap_blocks_and_atomic_batch_does_not_leave_free_id(self):
        index = NamedRangeIndex()
        first = self.plan("claim", ["001"])
        second = self.plan("claim", ["002", "001"], SECOND)
        index.apply(first)
        original = deepcopy(index.ranges)
        with self.assertRaises(ValueError):
            index.apply(second)
        self.assertEqual(index.ranges, original)
        index.apply(self.plan("claim", ["002"], SECOND))

    def test_stale_release_cannot_clear_another_attempt(self):
        index = NamedRangeIndex()
        index.apply(self.plan("claim", ["001"]))
        release = self.plan("release", ["001"])
        index.apply(release)
        index.apply(self.plan("claim", ["001"], SECOND))
        snapshot = deepcopy(index.ranges)
        for stale in (release, self.plan("claim", ["001"])):
            with self.assertRaises(ValueError):
                index.apply(stale)
            self.assertEqual(index.ranges, snapshot)

    def test_publishing_blocks_stale_release_and_wrong_finalization(self):
        index = NamedRangeIndex()
        index.apply(self.plan("claim", ["001"]))
        index.apply(self.plan("prepare", ["001"]))
        snapshot = deepcopy(index.ranges)
        for stale in (self.plan("release", ["001"]), self.plan("finish", ["001"], digest="b" * 64)):
            with self.assertRaises(ValueError):
                index.apply(stale)
            self.assertEqual(index.ranges, snapshot)
        done = self.plan("finish", ["001"])
        index.apply(done)
        key = done["records"][0]["range"]
        self.assertEqual(index.ranges[key]["range"]["startRowIndex"], 2)
        with self.assertRaises(ValueError):
            index.apply(self.plan("claim", ["001"], SECOND))

    def test_job_folder_creation_is_exclusive_even_for_disjoint_ads(self):
        index = NamedRangeIndex()
        first = self.plan("claim_job_folder", ["001"])
        second = self.plan("claim_job_folder", ["002"], SECOND)
        self.assertNotEqual(first["receipt"], second["receipt"])
        index.apply(first)
        with self.assertRaises(ValueError):
            index.apply(second)

    def test_exact_id_encoding_and_safe_names(self):
        ids = ["001", "1", "1234", "AbC", "abc", " id ", "../x", "é", "e\N{COMBINING ACUTE ACCENT}"]
        records = self.plan("claim", ids)["records"]
        self.assertEqual([record["id"] for record in records], ids)
        self.assertEqual(len({record["range"] for record in records}), len(ids))
        self.assertEqual(records[0]["filename"], "001.json")
        self.assertEqual(records[3]["filename"], "AbC.json")
        self.assertTrue(record_filename("researcher_ads", "../x").startswith("_sha256_"))
        self.assertNotEqual(records[0]["range"], self.plan("claim", ["001"], namespace="another")["records"][0]["range"])

    def test_verified_legacy_ids_seed_exactly_once(self):
        with TemporaryDirectory() as directory:
            path = Path(directory) / "legacy"
            path.write_bytes(b'["001","001"," id ","Ad"]')
            ids = read_legacy_ids(path, "json_string_array")
            self.assertEqual(ids, ["001", " id ", "Ad"])
            path.write_bytes(b"001\r\n id \r\n\r\nAd\n")
            self.assertEqual(read_legacy_ids(path, "newline_ids"), ids)
            path.write_bytes(b"[12]")
            with self.assertRaises(ValueError):
                read_legacy_ids(path, "json_string_array")
        index = NamedRangeIndex()
        index.apply(self.plan("seed", ids))
        with self.assertRaises(ValueError):
            index.apply(self.plan("claim", ["001"]))
        index.apply(self.plan("claim", ["1"]))


if __name__ == "__main__":
    unittest.main()
