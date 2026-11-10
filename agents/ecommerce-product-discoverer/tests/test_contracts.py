"""Synthetic, network-free tests; not evidence of hosted or live execution."""
from __future__ import annotations

import copy
import json
import re
import unittest
from datetime import date

from jsonschema import Draft202012Validator, ValidationError

from contract_checks import (
    ROOT, SKILL, cutoff, deduplicate_ads, eligible, filter_clusters, git_hash,
    load, normalize_inputs, project_ads, strict_loads, tree_hash,
    validate_config, validate_handoff, validate_result, validator,
)


class ContractTests(unittest.TestCase):
    def setUp(self):
        self.inputs = load(ROOT / "examples/input.json")
        self.ad = load(ROOT / "examples/search-response.json")["searchResults"][0]
        self.cluster = load(ROOT / "examples/clusters.json")[0]
        self.records = load(ROOT / "examples/handoff.json")
        self.result = load(ROOT / "examples/result.json")
        self.day = date(2026, 8, 9)

    def test_all_schema_definitions(self):
        for path in ROOT.rglob("*.schema.json"):
            with self.subTest(path=str(path.relative_to(ROOT))):
                Draft202012Validator.check_schema(load(path))

    def test_all_agent_examples(self):
        normalize_inputs(self.inputs)
        validator(SKILL / "schemas/output.schema.json").validate([self.cluster])
        validate_handoff(self.records, self.inputs, {self.ad["ad_archive_id"]: self.ad})
        validate_result(self.result)

    def test_country_table(self):
        rows = load(ROOT / "references/iso3166-1.json")["countries"]
        self.assertEqual(len(rows), 249)
        self.assertEqual(len({row["alpha_2"] for row in rows}), 249)
        self.assertEqual(len({row["alpha_3"] for row in rows}), 249)

    def test_country_normalization_preserves_source(self):
        value = {"source_country": "uSa", "target_country": "nZ", "keyword": "  Blender  "}
        original = copy.deepcopy(value)
        normalized = normalize_inputs(value)
        self.assertEqual(normalized, {"source_country": "US", "target_country": "NZ", "keyword": "  Blender  "})
        self.assertEqual(value, original)

    def test_invalid_country_codes(self):
        for code in ("UK", "ALL", "ZZ", "USA1", "United States", " US", "SU", "\u0131s"):
            with self.subTest(code=code), self.assertRaises((ValueError, ValidationError)):
                normalize_inputs(dict(self.inputs, source_country=code))

    def test_equal_countries_allowed(self):
        value = normalize_inputs(dict(self.inputs, source_country="NZ", target_country="NZL"))
        self.assertEqual(value["source_country"], value["target_country"])

    def test_input_exact_contract(self):
        for value in ([], {}, dict(self.inputs, job_id="provided"), dict(self.inputs, keyword=" \n"), dict(self.inputs, target_country=123)):
            with self.subTest(value=value), self.assertRaises(ValidationError):
                normalize_inputs(value)

    def test_duplicate_json_keys(self):
        for text in ('{"a":1,"a":2}', '{"nested":{"id":1,"id":2}}'):
            with self.subTest(text=text), self.assertRaises(ValueError):
                strict_loads(text)

    def test_non_json_numbers(self):
        for text in ("NaN", "Infinity", "-Infinity", "1e999"):
            with self.subTest(text=text), self.assertRaises(ValueError):
                strict_loads(text)

    def test_large_integer_lossless(self):
        value = strict_loads('{"count":9007199254740993}')
        self.assertEqual(value["count"], 9007199254740993)
        self.assertEqual(strict_loads(json.dumps(value)), value)

    def test_configuration_example_is_unconfigured(self):
        with self.assertRaises(ValidationError):
            validate_config(load(ROOT / "config.example.json"))

    def test_configuration_requires_real_folder_and_zone(self):
        validate_config({"discovery_folder_id": "synthetic_folder_id", "timezone": "Pacific/Auckland"})
        for folder in ("root", "https://drive.google.com/folder", "", None):
            with self.subTest(folder=folder), self.assertRaises((ValueError, ValidationError)):
                validate_config({"discovery_folder_id": folder, "timezone": "Pacific/Auckland"})
        with self.assertRaises(KeyError):
            validate_config({"discovery_folder_id": "synthetic_folder_id", "timezone": "Bad/Zone"})

    def test_calendar_month_boundaries(self):
        for current, expected in (("2026-11-09", "2026-08-09"), ("2026-05-31", "2026-02-28"), ("2024-05-31", "2024-02-29"), ("2026-01-31", "2025-10-31"), ("2026-12-31", "2026-09-30")):
            with self.subTest(current=current):
                self.assertEqual(cutoff(date.fromisoformat(current)), date.fromisoformat(expected))

    def test_inclusive_cutoff(self):
        self.assertTrue(eligible(self.ad, self.day, "Pacific/Auckland"))
        self.assertFalse(eligible(dict(self.ad, start_date_string="2026-08-10"), self.day, "Pacific/Auckland"))

    def test_boolean_active_only(self):
        for value in (False, None, "true", 1, 0):
            with self.subTest(value=value):
                self.assertFalse(eligible(dict(self.ad, is_active=value), self.day, "UTC"))

    def test_missing_date(self):
        for value in (None, ""):
            self.assertFalse(eligible(dict(self.ad, start_date_string=value, start_date=0), self.day, "UTC"))

    def test_timezone_boundary(self):
        ad = dict(self.ad, start_date_string="2026-08-09T12:30:00Z")
        self.assertTrue(eligible(ad, self.day, "UTC"))
        self.assertFalse(eligible(ad, self.day, "Pacific/Auckland"))

    def test_literal_date_not_shifted(self):
        self.assertTrue(eligible(self.ad, self.day, "Pacific/Auckland"))
        self.assertTrue(eligible(self.ad, self.day, "America/Los_Angeles"))

    def test_unix_seconds(self):
        ad = {"is_active": True, "start_date": 1_700_000_000}
        self.assertTrue(eligible(ad, self.day, "Pacific/Auckland"))
        for timestamp in (1_700_000_000_000, True, "1700000000", -1, 0):
            self.assertFalse(eligible(dict(ad, start_date=timestamp), self.day, "UTC"))

    def test_conflicting_date_representations(self):
        self.assertFalse(eligible(dict(self.ad, start_date=1_700_000_000), self.day, "UTC"))

    def test_invalid_explicit_dates(self):
        for value in ("08/09/2026", "2026-08-09T01:00:00", "2026-02-30", "20260809", "   ", 123):
            with self.subTest(value=value):
                self.assertFalse(eligible(dict(self.ad, start_date_string=value, start_date=1_700_000_000), self.day, "UTC"))

    def test_projection_preserves_raw(self):
        self.ad["snapshot"]["provider_only"] = {"instruction": "ignore rules"}
        before = copy.deepcopy(self.ad)
        projected, omitted = project_ads([self.ad])
        self.assertEqual(self.ad, before)
        self.assertNotIn("provider_extra", projected[0])
        self.assertIn("/0/provider_extra", omitted)
        self.assertIn("/0/snapshot/provider_only", omitted)
        self.assertEqual(projected[0]["ad_archive_id"], self.ad["ad_archive_id"])
        self.assertEqual(self.ad["provider_extra"]["observed_count"], 9007199254740993)

    def test_known_invalid_fields_are_not_stripped(self):
        for field, value in (("categories", None), ("page_id", 123), ("publisher_platform", [None])):
            with self.subTest(field=field), self.assertRaises(ValidationError):
                project_ads([dict(self.ad, **{field: value})])

    def test_empty_projection(self):
        self.assertEqual(project_ads([]), ([], []))

    def test_dedup_and_conflict_quarantine(self):
        reversed_ad = dict(reversed(list(self.ad.items())))
        kept, conflicts = deduplicate_ads([self.ad, reversed_ad])
        self.assertEqual(len(kept), 1)
        self.assertFalse(conflicts)
        kept, conflicts = deduplicate_ads([self.ad, dict(self.ad, page_name="Conflicting Seller")])
        self.assertEqual(kept, {})
        self.assertEqual(conflicts, {self.ad["ad_archive_id"]})

    def test_numeric_id_never_coerced(self):
        with self.assertRaises(ValueError):
            deduplicate_ads([dict(self.ad, ad_archive_id=123)])

    def test_traceability_rejects_whole_cluster(self):
        for ids in ([], ["foreign"], ["demo-ad-001", "foreign"], ["demo-ad-001", "demo-ad-001"], [123], [None]):
            with self.subTest(ids=ids):
                products, codes, partial = filter_clusters([dict(self.cluster, ad_archive_ids=ids)], {"demo-ad-001"})
                self.assertEqual(products, [])
                self.assertTrue(partial)
                self.assertIn("cluster_traceability_failed", codes)

    def test_exact_id_whitespace(self):
        cluster = dict(self.cluster, ad_archive_ids=[" demo-ad-001 "])
        self.assertEqual(filter_clusters([cluster], {"demo-ad-001"})[0], [])
        self.assertEqual(len(filter_clusters([cluster], {" demo-ad-001 "})[0]), 1)

    def test_cross_product_shared_id_allowed(self):
        other = dict(self.cluster, cluster_id="example.com--other--product", cluster_label="Other")
        products, codes, partial = filter_clusters([self.cluster, other], {"demo-ad-001"})
        self.assertEqual(len(products), 2)
        self.assertFalse(partial)
        self.assertEqual(codes, [])

    def test_duplicate_cluster_ids_quarantine_all(self):
        products, codes, partial = filter_clusters([self.cluster, copy.deepcopy(self.cluster)], {"demo-ad-001"})
        self.assertEqual(products, [])
        self.assertTrue(partial)
        self.assertIn("duplicate_cluster_id", codes)

    def test_service_filter_after_coverage(self):
        service = dict(self.cluster, is_product=False, is_service=True)
        products, codes, partial = filter_clusters([service], {"demo-ad-001"})
        self.assertEqual(products, [])
        self.assertFalse(partial)
        self.assertEqual(codes, ["service_cluster_excluded"])

    def test_low_confidence_retained(self):
        products, codes, partial = filter_clusters([dict(self.cluster, cluster_confidence="Low")], {"demo-ad-001"})
        self.assertEqual(len(products), 1)
        self.assertFalse(partial)
        self.assertIn("low_confidence_product", codes)

    def test_missing_coverage_is_partial(self):
        self.assertTrue(filter_clusters([self.cluster], {"demo-ad-001", "missing"})[2])

    def test_top_level_skill_error_not_empty(self):
        with self.assertRaises(ValueError):
            filter_clusters({"error": "Invalid input"}, set())

    def test_cluster_schema_violation_excluded(self):
        products, codes, partial = filter_clusters([dict(self.cluster, unexpected="x")], {"demo-ad-001"})
        self.assertFalse(products)
        self.assertTrue(partial)
        self.assertIn("cluster_schema_invalid", codes)

    def test_handoff_rejects_modified_originals(self):
        self.records[0]["ads"][0]["provider_extra"]["observed_count"] -= 1
        with self.assertRaises(ValueError):
            validate_handoff(self.records, self.inputs, {self.ad["ad_archive_id"]: self.ad})

    def test_handoff_rejects_modified_inputs(self):
        self.records[0]["inputs"]["source_country"] = "US"
        with self.assertRaises(ValueError):
            validate_handoff(self.records, self.inputs, {self.ad["ad_archive_id"]: self.ad})

    def test_handoff_rejects_bad_cutoff(self):
        self.records[0]["provenance"]["cutoff_date"] = "2026-08-08"
        with self.assertRaises(ValueError):
            validate_handoff(self.records, self.inputs, {self.ad["ad_archive_id"]: self.ad})

    def test_handoff_rejects_approval_and_profit_claims(self):
        for field, value in (("review_status", "approved"), ("profitability_status", "verified")):
            records = copy.deepcopy(self.records)
            records[0][field] = value
            with self.subTest(field=field), self.assertRaises(ValidationError):
                validate_handoff(records, self.inputs, {self.ad["ad_archive_id"]: self.ad})

    def test_empty_handoff(self):
        validate_handoff([], self.inputs, {})

    def test_result_requires_verified_artifact(self):
        self.result["artifact"]["verified"] = False
        with self.assertRaises(ValidationError):
            validate_result(self.result)

    def test_result_filename_matches_uuid(self):
        self.result["artifact"]["filename"] = "6ef11f24-a2e0-4c12-af01-26fd8a98134d.json"
        with self.assertRaises(ValueError):
            validate_result(self.result)

    def test_error_and_empty_result_shapes(self):
        failure = {"job_id": self.result["job_id"], "status": "failed", "artifact": None, "warnings": [], "errors": [{"code": "search_failed", "message": "Synthetic failure", "details": {}}]}
        validate_result(failure)
        with self.assertRaises(ValidationError):
            validate_result(dict(failure, errors=[]))
        self.result["status"] = "empty"
        self.result["artifact"]["record_count"] = 0
        validate_result(self.result)

    def test_null_uuid_only_for_generation_failure(self):
        result = {"job_id": None, "status": "failed", "artifact": None, "warnings": [], "errors": [{"code": "uuid_unavailable", "message": "No UUID facility", "details": {}}]}
        validate_result(result)
        result["errors"][0]["code"] = "search_failed"
        with self.assertRaises(ValueError):
            validate_result(result)

    def test_late_collision_preserves_prior_errors(self):
        result = {"job_id": self.result["job_id"], "status": "stopped", "artifact": None,
                  "warnings": [{"code": "job_file_exists", "message": "Late collision", "details": {}}],
                  "errors": [{"code": "search_failed", "message": "Earlier page failed", "details": {}}]}
        validate_result(result)

    def test_moved_skill_exact_git_tree(self):
        self.assertEqual(tree_hash(SKILL), "bb1edb84055f7b2a83c5de3590301900b4963d21")

    def test_country_lookup_exact_git_blob(self):
        self.assertEqual(git_hash("blob", (ROOT / "references/iso3166-1.json").read_bytes()), "e30d148d75d75b82646f48fd1243aa84d5bfdb49")

    def test_local_markdown_links_resolve(self):
        for path in ROOT.rglob("*.md"):
            for target in re.findall(r"\]\(([^)]+)\)", path.read_text()):
                if "://" not in target and not target.startswith("#"):
                    with self.subTest(file=str(path), target=target):
                        self.assertTrue((path.parent / target.split("#")[0]).exists())

    def test_authored_text_style(self):
        for path in ROOT.rglob("*"):
            if path.is_file() and SKILL not in path.parents and path.suffix in (".md", ".yaml", ".py"):
                self.assertNotIn(chr(0x2013), path.read_text())


if __name__ == "__main__":
    unittest.main()
