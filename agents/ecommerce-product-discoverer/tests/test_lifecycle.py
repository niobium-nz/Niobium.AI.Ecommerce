"""Offline discovery-stage regressions; no hosted agents or connectors are invoked."""
from __future__ import annotations

import copy
import json
from pathlib import Path
import re
import unittest

from jsonschema import Draft202012Validator, FormatChecker, ValidationError
from referencing import Registry, Resource

ROOT = Path(__file__).resolve().parents[1]


class DiscoveryLifecycleTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        schema_path = ROOT / "schemas/handoff.schema.json"
        cls.schema = json.loads(schema_path.read_text(encoding="utf-8"))
        resources = [
            (path.as_uri(), Resource.from_contents(json.loads(path.read_text(encoding="utf-8"))))
            for path in ROOT.rglob("*.schema.json")
        ]
        cls.check = Draft202012Validator(
            {"$ref": schema_path.as_uri()},
            registry=Registry().with_resources(resources),
            format_checker=FormatChecker(),
        )
        cls.example = json.loads((ROOT / "examples/handoff.json").read_text(encoding="utf-8"))

    def setUp(self):
        self.records = copy.deepcopy(self.example)

    def test_discovery_schema_definition(self):
        Draft202012Validator.check_schema(self.schema)

    def test_example_is_discovery_awaiting_research(self):
        self.check.validate(self.records)
        self.assertGreater(len(self.records), 0)
        for record in self.records:
            self.assertEqual(record["schema_version"], "2.0.0")
            self.assertEqual(record["lifecycle_stage"], "discovery")
            self.assertEqual(record["research_status"], "pending_competitive_research")
            self.assertEqual(record["profitability_status"], "unverified")
            self.assertNotIn("review_status", record)

    def test_lifecycle_fields_are_required(self):
        for field in ("schema_version", "lifecycle_stage", "research_status", "profitability_status"):
            records = copy.deepcopy(self.example)
            del records[0][field]
            with self.subTest(field=field), self.assertRaises(ValidationError):
                self.check.validate(records)

    def test_discovery_cannot_emit_candidates_or_listings(self):
        for value in ("candidate", "listing", "approved", "Discovery", None):
            self.records[0]["lifecycle_stage"] = value
            with self.subTest(value=value), self.assertRaises(ValidationError):
                self.check.validate(self.records)

    def test_discovery_cannot_claim_research_complete_or_council_ready(self):
        for value in ("completed", "researched", "pending_product_council", "approved", None):
            self.records[0]["research_status"] = value
            with self.subTest(value=value), self.assertRaises(ValidationError):
                self.check.validate(self.records)

    def test_review_status_is_rejected_even_with_discovery_markers(self):
        for value in ("pending_product_council", "approved", "not_submitted", None):
            self.records[0]["review_status"] = value
            with self.subTest(value=value), self.assertRaises(ValidationError):
                self.check.validate(self.records)

    def test_legacy_v1_handoff_is_rejected(self):
        record = self.records[0]
        record["schema_version"] = "1.0.0"
        record["review_status"] = "pending_product_council"
        del record["lifecycle_stage"]
        del record["research_status"]
        with self.assertRaises(ValidationError):
            self.check.validate(self.records)

    def test_new_fields_cannot_masquerade_as_v1(self):
        self.records[0]["schema_version"] = "1.0.0"
        with self.assertRaises(ValidationError):
            self.check.validate(self.records)

    def test_profitability_stays_unverified(self):
        for value in ("verified", "profitable", "approved", None):
            self.records[0]["profitability_status"] = value
            with self.subTest(value=value), self.assertRaises(ValidationError):
                self.check.validate(self.records)

    def test_complete_and_partial_runs_remain_discoveries(self):
        for run_status in ("complete", "partial"):
            self.records[0]["provenance"]["run_status"] = run_status
            with self.subTest(run_status=run_status):
                self.check.validate(self.records)
                self.assertEqual(self.records[0]["lifecycle_stage"], "discovery")
                self.assertEqual(self.records[0]["research_status"], "pending_competitive_research")

    def test_cluster_confidence_never_promotes_a_discovery(self):
        for confidence in ("High", "Medium", "Low"):
            self.records[0]["product"]["cluster_confidence"] = confidence
            with self.subTest(confidence=confidence):
                self.check.validate(self.records)
                self.assertEqual(self.records[0]["lifecycle_stage"], "discovery")

    def test_empty_handoff_stays_an_empty_array(self):
        self.check.validate([])
        with self.assertRaises(ValidationError):
            self.check.validate({"lifecycle_stage": "discovery", "discoveries": []})

    def test_source_claims_do_not_control_envelope_stage(self):
        self.records[0]["ads"][0]["provider_claim"] = {
            "lifecycle_stage": "listing", "review_status": "approved"
        }
        self.check.validate(self.records)
        self.assertEqual(self.records[0]["lifecycle_stage"], "discovery")
        self.assertNotIn("review_status", self.records[0])
        self.assertEqual(self.records[0]["ads"][0]["provider_claim"]["review_status"], "approved")

    def test_manifest_matches_handoff_contract(self):
        # These are deliberately simple top-level manifest scalars, not a YAML parser.
        manifest = (ROOT / "agent.yaml").read_text(encoding="utf-8")
        for key, value in (("version", "2.0.0"), ("output_stage", "discovery"),
                           ("handoff_target", "competitive_research")):
            with self.subTest(key=key):
                self.assertRegex(manifest, re.compile(rf"^{key}: {re.escape(value)}$", re.MULTILINE))
        self.assertNotIn("approval_boundary:", manifest)

    def test_host_instructions_preserve_research_boundary(self):
        lifecycle = "discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding"
        for name in ("AGENT.md", "README.md", "references/contracts.md"):
            with self.subTest(name=name):
                self.assertIn(lifecycle, (ROOT / name).read_text(encoding="utf-8"))
        for name in ("adapters/chatgpt-workspace-agent.md", "adapters/claude-cowork.md"):
            text = (ROOT / name).read_text(encoding="utf-8")
            with self.subTest(name=name):
                self.assertIn("pending_competitive_research", text)
                self.assertIn("not product council", text)
                self.assertIn("Do not add downstream invocation", text)

    def test_previous_stage_name_is_rejected(self):
        self.records[0]["lifecycle_stage"] = "signal"
        with self.assertRaises(ValidationError):
            self.check.validate(self.records)

    def test_discovery_folder_configuration(self):
        schema = json.loads((ROOT / "schemas/config.schema.json").read_text(encoding="utf-8"))
        Draft202012Validator.check_schema(schema)
        check = Draft202012Validator(schema)
        check.validate({"discovery_folder_id": "test-folder-id", "timezone": "Pacific/Auckland"})
        for value in (
            {"signal_folder_id": "test-folder-id", "timezone": "Pacific/Auckland"},
            {"discovery_folder_id": "test-folder-id", "signal_folder_id": "old-id", "timezone": "Pacific/Auckland"},
        ):
            with self.subTest(value=value), self.assertRaises(ValidationError):
                check.validate(value)

    def test_config_example_uses_new_key_without_inventing_an_id(self):
        config = json.loads((ROOT / "config.example.json").read_text(encoding="utf-8"))
        self.assertEqual(config, {"discovery_folder_id": None, "timezone": "Pacific/Auckland"})

    def test_storage_name_and_filename_are_consistent(self):
        manifest = (ROOT / "agent.yaml").read_text(encoding="utf-8")
        self.assertRegex(manifest, re.compile(r"^  folder_name: discovery$", re.MULTILINE))
        self.assertRegex(manifest, re.compile(r"^  folder_id_configuration_key: discovery_folder_id$", re.MULTILINE))
        self.assertIn('filename: "{job_id}.json"', manifest)
        instructions = (ROOT / "AGENT.md").read_text(encoding="utf-8")
        self.assertIn("`discovery`, folder MIME type", instructions)
        self.assertIn("parent discovery_folder_id", instructions)
        self.assertIn("discovery/{job_id}.json", (ROOT / "README.md").read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
