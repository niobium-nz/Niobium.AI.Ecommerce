"""Network-free contract and mocked orchestration regressions."""
from copy import deepcopy as cp
import hashlib
import json
import unittest
from jsonschema import Draft202012Validator, ValidationError

from reference_model import (ROOT, load, validator, check, skill_check, strict_loads,
    validate_input, validate_config, parse_registry, append_registry, normalization_input,
    validate_normalization, competition_inputs, filter_reports, scoring_input,
    validate_score, artifact, run_mock)


class ResearcherTests(unittest.TestCase):
    def setUp(self):
        self.f = load(ROOT / "examples/fixtures.json")
        self.input = cp(self.f["input"])
        self.normal = cp(self.f["normalization"])
        self.report = cp(self.f["competition_report"])
        self.score = cp(self.f["score"])

    def run_case(self, **kwargs):
        return run_mock(self.input, self.normal, [(0, self.report)], self.score, **kwargs)

    def test_schema_definitions(self):
        for p in ROOT.rglob("*.schema.json"):
            with self.subTest(path=str(p)):
                Draft202012Validator.check_schema(load(p))

    def test_dependency_schema_hashes(self):
        for path, expected in load(ROOT / "dependencies.json")["contracts_used"].items():
            data = (ROOT / path).read_bytes()
            with self.subTest(path=path):
                self.assertEqual(hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest(), expected)

    def test_fixtures(self):
        validate_input(self.input)
        validate_normalization(self.normal, self.input)
        skill_check(self.report, "ecommerce-analyze-competition", "output")
        validate_score(self.score, self.input)
        check(self.f["artifact"], "artifact")
        check(self.f["result"], "result")

    def test_configuration_requires_real_ids(self):
        config = load(ROOT / "config.example.json")
        with self.assertRaises(ValidationError): validate_config(config)
        for field in ("discovery_folder_id", "onedrive_drive_id", "onedrive_item_id"):
            config[field] = "mock-id"
        validate_config(config)

    def test_configuration_rejects_root_bad_zone_and_null_format(self):
        config = load(ROOT / "config.example.json")
        for k in ("discovery_folder_id", "onedrive_drive_id", "onedrive_item_id"): config[k] = "mock-id"
        for field, value in (("discovery_folder_id", "root"), ("registry_format", None),
                             ("max_parallel_queries", 0), ("registry_concurrency", "concurrent")):
            with self.subTest(field=field), self.assertRaises(ValidationError):
                validate_config(dict(config, **{field: value}))
        with self.assertRaises(KeyError): validate_config(dict(config, timezone="Invalid/Zone"))

    def test_batch_and_url_rejected(self):
        for value in ([self.input], "https://drive.google.com/file/mock", {**self.input, "input_file": "mock"}):
            with self.subTest(value=value), self.assertRaises(ValidationError): validate_input(value)

    def test_job_validation(self):
        for field, value in (("job_id", "../escape"), ("source_country", "UK"),
                             ("target_country", "New Zealand"), ("keyword", "  ")):
            data = cp(self.input); data["job"][field] = value
            with self.subTest(field=field), self.assertRaises(ValidationError): validate_input(data)

    def test_alpha2_and_alpha3_allowed(self):
        self.input["job"].update(source_country="us", target_country="nzl")
        validate_input(self.input)

    def test_product_validation(self):
        for field, value in (("likely_product_name", None), ("likely_product_name", " "),
                             ("is_product", False), ("is_service", True), ("cluster_id", "")):
            data = cp(self.input); data["product"][field] = value
            with self.subTest(field=field), self.assertRaises(ValidationError): validate_input(data)

    def test_every_ad_requires_link(self):
        for snapshot in ({}, {"link_url": None}, {"link_url": " "}, None):
            data = cp(self.input); data["ads"][0]["snapshot"] = snapshot
            with self.subTest(snapshot=snapshot), self.assertRaises(ValidationError): validate_input(data)

    def test_empty_ads_rejected(self):
        self.input["ads"] = []
        with self.assertRaises(ValidationError): validate_input(self.input)

    def test_input_duplicates_and_traceability_rejected(self):
        self.input["ads"].append(cp(self.input["ads"][0]))
        with self.assertRaises(ValueError): validate_input(self.input)
        self.input["ads"].pop(); self.input["product"]["ad_archive_ids"] = ["foreign"]
        with self.assertRaises(ValueError): validate_input(self.input)

    def test_duplicate_json_keys_and_nonfinite_rejected(self):
        for text in ('{"a":1,"a":2}', "NaN", "Infinity", "1e999"):
            with self.subTest(text=text), self.assertRaises(ValueError): strict_loads(text)

    def test_registry_exact_not_substring(self):
        stored = parse_registry('["1234","00123","AbC"]', "json_string_array")
        self.assertNotIn("123", stored); self.assertNotIn("abc", stored)
        self.assertIn("00123", stored)

    def test_registry_preserves_nonblank_id_whitespace(self):
        self.assertEqual(parse_registry('[" id "]', "json_string_array"), [" id "])
        self.assertEqual(parse_registry(" id \r\n\r\n", "newline_ids"), [" id "])

    def test_registry_rejects_unsupported_or_malformed(self):
        for text in ('[1]', '[null]', '[""]', '{}', '', '["x",'):
            with self.subTest(text=text), self.assertRaises((ValueError, json.JSONDecodeError)):
                parse_registry(text, "json_string_array")
        with self.assertRaises(ValueError): parse_registry("id", "auto")

    def test_registry_append_preserves_all_existing_ids(self):
        updated = append_registry('["old","old","001"]', "json_string_array", ["new", "new", "001"])
        self.assertEqual(json.loads(updated), ["old", "old", "001", "new"])

    def test_registry_plaintext_append_preserves_prefix(self):
        original = "old\r\nother"
        updated = append_registry(original, "newline_ids", ["new"])
        self.assertEqual(updated, "old\r\nother\r\nnew\r\n")
        self.assertEqual(append_registry(updated, "newline_ids", ["new"]), updated)
        with self.assertRaises(ValueError): append_registry(original, "newline_ids", ["bad\nid"])

    def test_normalization_mapping_uses_source(self):
        value = normalization_input(self.input)
        self.assertEqual(value["country"], "USA")
        self.assertEqual(value["known_features"], self.input["product"]["known_features"])
        skill_check(value, "ecommerce-normalize-products", "input")

    def test_confidence_six_passes_five_fails(self):
        self.normal["normalization"]["confidence0_to10"] = 6
        validate_normalization(self.normal, self.input)
        self.normal["normalization"]["confidence0_to10"] = 5
        with self.assertRaises(ValueError): validate_normalization(self.normal, self.input)

    def test_confidence_wrong_alias_rejected(self):
        self.normal["normalization"]["confidence_0_to_10"] = 8
        del self.normal["normalization"]["confidence0_to10"]
        with self.assertRaises(ValidationError): validate_normalization(self.normal, self.input)

    def test_empty_plan_and_null_sections_fail(self):
        for field in ("normalization", "keyword_plan"):
            result = cp(self.normal); result[field] = None
            with self.subTest(field=field), self.assertRaises(ValidationError): validate_normalization(result, self.input)
        self.normal["keyword_plan"]["recommended_mcp_queries"] = []
        with self.assertRaises(ValueError): validate_normalization(self.normal, self.input)

    def test_error_only_normalizer_stops(self):
        self.normal = {"error": "Cannot identify item"}
        result, out, _, events = self.run_case()
        self.assertEqual(result["status"], "failed"); self.assertIsNone(out)
        self.assertNotIn("score", events)

    def test_no_extra_normalization_status_gate(self):
        self.normal["status"] = "needs_more_info"
        validate_normalization(self.normal, self.input)

    def test_normalization_echo_mismatch(self):
        self.normal["product_name_given"] = "Wrong item"
        with self.assertRaises(ValueError): validate_normalization(self.normal, self.input)

    def test_competition_mapping_uses_target_and_notes(self):
        task = competition_inputs(self.normal, self.input)[0]
        self.assertEqual(task["country"], "NZL")
        self.assertEqual(task["query"], "portable blender")
        self.assertEqual(task["notes"], self.normal["notes_for_downstream_agent"])
        skill_check(task, "ecommerce-analyze-competition", "input")

    def test_error_only_and_partial_reports_excluded(self):
        tasks = competition_inputs(self.normal, self.input)
        for report in ({"error": "failed"}, dict(self.report, error="later page failed")):
            valid, errors = filter_reports(tasks, [(0, report)], self.input)
            self.assertEqual(valid, []); self.assertEqual(errors[0]["details"]["query_index"], 0)
            self.assertEqual(errors[0]["product_cluster_id"], self.input["product"]["cluster_id"])

    def test_zero_result_report_retained(self):
        valid, errors = filter_reports(competition_inputs(self.normal, self.input), [(0, self.report)], self.input)
        self.assertEqual(valid, [self.report]); self.assertEqual(errors, [])

    def test_wrong_query_country_and_legacy_alias_rejected(self):
        for report in (dict(self.report, query="other"), dict(self.report, country="US"),
                       {**self.report, "raw_ads_discovered": {**self.report["raw_ads_discovered"], "mcp_error": "legacy"}}):
            valid, errors = filter_reports(competition_inputs(self.normal, self.input), [(0, report)], self.input)
            self.assertFalse(valid); self.assertTrue(errors)

    def test_parallel_completion_order_is_not_output_order(self):
        self.normal["keyword_plan"]["recommended_mcp_queries"].append("second query")
        tasks = competition_inputs(self.normal, self.input)
        second = dict(self.report, query="second query")
        valid, errors = filter_reports(tasks, [(1, second), (0, self.report)], self.input)
        self.assertEqual([r["query"] for r in valid], ["portable blender", "second query"])
        self.assertFalse(errors)

    def test_missing_duplicate_and_unexpected_workers_diagnosed(self):
        tasks = competition_inputs(self.normal, self.input)
        for responses in ([], [(0, self.report), (0, self.report)], [(8, self.report)]):
            valid, errors = filter_reports(tasks, responses, self.input)
            self.assertFalse(valid); self.assertTrue(errors)

    def test_ads_projection_and_original_preservation(self):
        before = cp(self.input)
        value = scoring_input(self.input, [self.report])
        self.assertEqual(value["target_country"], "NZL")
        self.assertNotIn("provider_extra", value["ads"][0])
        self.assertEqual(self.input, before)
        self.assertEqual(self.input["ads"][0]["provider_extra"]["observed_count"], 9007199254740993)

    def test_recognized_invalid_ad_field_not_stripped(self):
        self.input["ads"][0]["categories"] = None
        with self.assertRaises(ValidationError): scoring_input(self.input, [self.report])

    def test_scorer_target_country_required(self):
        payload = scoring_input(self.input, [self.report]); del payload["target_country"]
        with self.assertRaises(ValidationError): skill_check(payload, "ecommerce-score-products", "input")

    def test_score_wrong_market_sum_or_band_rejected(self):
        for field, value in (("target_country", "US"), ("final_score", 34), ("priority_band", "TEST_FIRST")):
            score = dict(self.score, **{field: value})
            with self.subTest(field=field), self.assertRaises((ValueError, ValidationError)):
                validate_score(score, self.input)

    def test_artifact_exact_fields_and_originals(self):
        value = artifact(self.input, [self.report], self.score, [], self.f["artifact"]["signal_id"])
        self.assertEqual(set(value), {"job_id", "signal_id", "keyword", "source_country", "target_country", "product", "ads", "competition_signals", "score", "errors"})
        self.assertEqual(value["ads"], self.input["ads"])
        value["normalized_product"] = self.normal
        with self.assertRaises(ValidationError): check(value, "artifact")

    def test_artifact_rejects_no_reports_or_error_reports(self):
        for reports in ([], [{"error": "failed"}], [dict(self.report, error="partial")]):
            with self.subTest(reports=reports), self.assertRaises(ValidationError):
                artifact(self.input, reports, self.score, [], self.f["artifact"]["signal_id"])

    def test_complete_order_and_input_ids_only(self):
        result, out, history, events = self.run_case(registry='["old"]')
        self.assertEqual(result["status"], "succeeded")
        self.assertEqual(json.loads(history), ["old", "demo-ad-001"])
        self.assertLess(events.index("verify_publication"), events.index("append_registry"))
        self.assertLess(events.index("verify_registry"), events.index("release_guard"))
        self.assertEqual(out["product"], self.input["product"])

    def test_duplicate_global_stops_before_research(self):
        result, out, history, events = self.run_case(registry='["demo-ad-001"]')
        self.assertEqual(result["status"], "failed"); self.assertIsNone(out)
        self.assertNotIn("normalize", events); self.assertNotIn("append_registry", events)
        self.assertEqual(result["errors"][0]["product_cluster_id"], self.input["product"]["cluster_id"])

    def test_all_query_failures_stop_without_score_or_publication(self):
        self.report = {"error": "search unavailable"}
        result, out, history, events = self.run_case()
        self.assertEqual(result["status"], "failed"); self.assertIsNone(out)
        self.assertNotIn("score", events); self.assertNotIn("publish", events)
        self.assertNotIn("append_registry", events); self.assertEqual(history, "[]")

    def test_mixed_reports_publish_only_valid_subset_and_errors(self):
        self.normal["keyword_plan"]["recommended_mcp_queries"].append("other query")
        result, out, _, _ = run_mock(self.input, self.normal,
            [(1, {"error": "failed"}), (0, self.report)], self.score)
        self.assertEqual(result["status"], "partial")
        self.assertEqual(out["competition_signals"], [self.report])
        self.assertEqual(out["errors"][0]["details"]["query"], "other query")

    def test_no_score_stops_without_publication_or_registry_write(self):
        self.score = {"error": "Research unavailable"}
        result, out, _, events = self.run_case()
        self.assertEqual(result["status"], "failed"); self.assertIsNone(out)
        self.assertNotIn("publish", events); self.assertNotIn("append_registry", events)

    def test_complete_score_with_error_is_published_partial(self):
        self.score["error"] = "One research query failed"
        self.score["caps_applied"].append("THIN_EVIDENCE_60: Partial research failure.")
        result, out, _, events = self.run_case()
        self.assertEqual(result["status"], "partial")
        self.assertEqual(out["score"]["error"], self.score["error"])
        self.assertEqual(out["errors"][0]["code"], "score_partial_failure")
        self.assertIn("append_registry", events)

    def test_low_score_is_not_a_new_publish_gate(self):
        result, out, _, _ = self.run_case()
        self.assertEqual(out["score"]["priority_band"], "DO_NOT_TEST")
        self.assertEqual(result["status"], "succeeded")

    def test_late_duplicate_stops_publication(self):
        result, out, _, events = self.run_case(late_registry='["demo-ad-001"]')
        self.assertIsNone(out); self.assertEqual(result["status"], "failed")
        self.assertNotIn("publish", events); self.assertNotIn("append_registry", events)

    def test_unrelated_registry_updates_preserved(self):
        result, _, history, _ = self.run_case(registry='["old"]', late_registry='["old","other"]')
        self.assertEqual(result["status"], "succeeded")
        self.assertEqual(json.loads(history), ["old", "other", "demo-ad-001"])

    def test_failed_publication_never_marks_ids_processed(self):
        result, out, history, events = self.run_case(publish_ok=False)
        self.assertFalse(result["published"]); self.assertIsNone(result["output_filename"])
        self.assertIsNone(out); self.assertEqual(history, "[]")
        self.assertNotIn("append_registry", events)

    def test_registry_failure_keeps_published_artifact_and_no_rerun(self):
        result, out, history, events = self.run_case(registry_ok=False)
        self.assertEqual(result["status"], "published_registry_pending")
        self.assertTrue(result["published"]); self.assertFalse(result["registry_updated"])
        self.assertIsNotNone(result["output_filename"]); self.assertIsNotNone(out)
        self.assertEqual(history, "[]"); self.assertEqual(events.count("publish"), 1)
        self.assertEqual(events.count("score"), 1)

    def test_registry_failure_preserves_prior_research_errors(self):
        self.score["error"] = "Partial research"
        self.score["caps_applied"].append("THIN_EVIDENCE_60: Partial research failure.")
        result, out, _, _ = self.run_case(registry_ok=False)
        self.assertEqual(len(out["errors"]), 1)
        self.assertEqual(len(result["errors"]), 2)
        self.assertEqual(out["score"]["error"], "Partial research")

    def test_result_rejects_false_clean_success(self):
        for change in ({"registry_updated": False}, {"published": False}, {"output_filename": None}):
            result = {**self.f["result"], **change}
            with self.subTest(change=change), self.assertRaises(ValidationError): check(result, "result")

    def test_malformed_product_returns_correlated_error_not_exception(self):
        self.input["product"] = "not an object"
        result, out, _, events = self.run_case()
        self.assertEqual(result["status"], "failed")
        self.assertIsNone(result["errors"][0]["product_cluster_id"])
        self.assertIsNone(out); self.assertNotIn("read_registry", events)

    def test_confidence_failure_exposes_available_value(self):
        self.normal["normalization"]["confidence0_to10"] = 5
        result, _, _, events = self.run_case()
        self.assertEqual(result["errors"][-1]["details"]["confidence0_to10"], 5)
        self.assertNotIn("score", events)

    def test_partial_score_cannot_omit_required_evidence_cap(self):
        self.score["error"] = "Partial research"
        with self.assertRaises(ValueError): validate_score(self.score, self.input)

    def test_manifest_lists_all_three_skills(self):
        text = (ROOT / "agent.yaml").read_text()
        for name in load(ROOT / "dependencies.json")["moved_skills"]:
            self.assertIn("skills/" + name + "/SKILL.md", text)
        self.assertIn("output_stage: candidate", text)
        self.assertIn("invokes_downstream_agents: false", text)

    def test_compact_entrypoint_and_style(self):
        self.assertLess(len((ROOT / "AGENT.md").read_text().splitlines()), 125)
        for p in ROOT.rglob("*"):
            if p.is_file() and "skills" not in p.relative_to(ROOT).parts and p.suffix in (".md", ".yaml", ".py"):
                self.assertNotIn(chr(0x2013), p.read_text())


if __name__ == "__main__":
    unittest.main()
