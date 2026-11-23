import unittest
from council import council, sourcing_handoff, evidence_version, human_decision, assert_onboarding_authorized

def candidate(pid, score):
    return {"product_id": pid, "specialist_assessments": {name: {"score": score, "pros": [], "cons": [],
            "risks": ["unresolved risk"], "uncertainties": [], "evidence_references": [],
            "supplier_validation_questions": [], "recommended_mitigations": []}
            for name in ("commercial", "market", "competitive", "risk", "operations")}}

class CouncilTests(unittest.TestCase):
    def test_all_bad_still_selects_one(self):
        result = council("batch", [candidate("b", 2), candidate("a", 1)])
        self.assertEqual(result["selected_product_id"], "b")
        self.assertIsNone(result["approved_for_onboarding"])
        self.assertEqual(len(result["candidate_rankings"]), 2)

    def test_tie_is_deterministic(self):
        self.assertEqual(council("batch", [candidate("z", 5), candidate("a", 5)])["selected_product_id"], "a")

    def test_invalid_batch(self):
        with self.assertRaises(ValueError):
            council("batch", [])
        with self.assertRaises(ValueError):
            council("batch", [candidate("a", 1), candidate("a", 2)])

    def test_gate_and_stale_evidence(self):
        result = council("batch", [candidate("a", 1)])
        handoff = sourcing_handoff(result, [{"product_id": "a", "status": "blocked", "evidence_references": []}])
        version = evidence_version(handoff)
        with self.assertRaises(ValueError):
            assert_onboarding_authorized(handoff, None)
        pending = human_decision(handoff, "request_more_information", "owner", "need quote", version)
        with self.assertRaises(ValueError):
            assert_onboarding_authorized(handoff, pending)
        approved = human_decision(handoff, "approve", "owner", "reviewed risks", version)
        assert_onboarding_authorized(handoff, approved)
        changed = sourcing_handoff(result, [{"product_id": "a", "status": "verified", "evidence_references": ["quote"]}])
        with self.assertRaises(ValueError):
            assert_onboarding_authorized(changed, approved)

    def test_sourcing_cannot_substitute(self):
        result = council("batch", [candidate("a", 10)])
        with self.assertRaises(ValueError):
            sourcing_handoff(result, [{"product_id": "other", "status": "verified", "evidence_references": []}])

if __name__ == "__main__":
    unittest.main()
