from __future__ import annotations

import importlib.util
import json
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def load_module(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"unable to load {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


derive = load_module("derive_offer_map", ROOT / "scripts" / "derive_offer_map.py")
validator = load_module("validate_bundle", ROOT / "scripts" / "validate_bundle.py")


class DeriveOfferMapTests(unittest.TestCase):
    def setUp(self) -> None:
        self.example_path = ROOT / "references" / "example_input.json"
        self.example = json.loads(self.example_path.read_text(encoding="utf-8"))

    def test_example_uses_integer_shipping_option(self) -> None:
        result = derive.build_offer_option_map(self.example, self.example_path)
        self.assertEqual(result["shipping_option_id"], 101)
        self.assertEqual(result["shipping_option_environment"]["value"], "101")
        self.assertEqual(result["shipping_option_environment"]["vendor_input_type"], "number")

    def test_example_json_fields_are_all_lower_snake_case(self) -> None:
        def walk(value, path: str = "$") -> None:
            if isinstance(value, dict):
                for key, child in value.items():
                    self.assertRegex(key, r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$", path)
                    walk(child, f"{path}.{key}")
            elif isinstance(value, list):
                for index, child in enumerate(value):
                    walk(child, f"{path}[{index}]")

        walk(self.example)

    def test_cart_input_is_lower_snake_and_env_cart_is_vendor_wire_shape(self) -> None:
        mapping = self.example["pricing_economics_and_offers"]["offer_options_mapping"][0]
        self.assertEqual(set(mapping["option_configuration"][0]), {"listing", "option", "quantity"})
        result = derive.build_offer_option_map(self.example, self.example_path)
        env_cart = json.loads(result["visible_offers"][0]["env_var_value"])
        self.assertEqual(set(env_cart[0]), {"Listing", "Option", "Quantity"})

    def test_integration_endpoints_are_required_and_reported(self) -> None:
        result = derive.build_offer_option_map(self.example, self.example_path)
        self.assertEqual(
            result["integration_endpoints"],
            {
                "store": self.example["vendor_integration"]["store_integration_endpoint"],
                "notification": self.example["vendor_integration"]["notification_integration_endpoint"],
            },
        )
        for key in ("store_integration_endpoint", "notification_integration_endpoint"):
            data = json.loads(json.dumps(self.example))
            data["vendor_integration"][key] = ""
            with self.subTest(key=key):
                with self.assertRaisesRegex(ValueError, key):
                    derive.build_offer_option_map(data, self.example_path)


    def test_testimonial_rendering_and_legal_policy_bindings_are_reported(self) -> None:
        result = derive.build_offer_option_map(self.example, self.example_path)
        self.assertEqual(result["testimonial_count"], len(self.example["trust_signal"]["testimonials"]))
        self.assertEqual(result["testimonials"], self.example["trust_signal"]["testimonials"])
        self.assertEqual(result["testimonial_rendering"]["initial_count"], 6)
        self.assertTrue(result["testimonial_rendering"]["load_more_required"])
        self.assertEqual(len(result["legal_policies"]), 4)
        for policy in result["legal_policies"]:
            self.assertRegex(policy["sha256"], r"^[0-9a-f]{64}$")
            self.assertTrue(policy["project_path"].startswith("content/policies/"))

    def test_missing_legal_policy_source_is_rejected(self) -> None:
        data = json.loads(json.dumps(self.example))
        data["trust_signal"]["terms"] = "./assets/missing-terms.md"
        with self.assertRaisesRegex(ValueError, "source file does not exist"):
            derive.build_offer_option_map(data, self.example_path)

    def test_camel_case_input_field_is_rejected(self) -> None:
        data = json.loads(json.dumps(self.example))
        data["vendor_integration"]["shippingOptionId"] = data["vendor_integration"].pop("shipping_option_id")
        with self.assertRaisesRegex(ValueError, "lower snake case"):
            derive.build_offer_option_map(data, self.example_path)

    def test_shipping_option_rejects_string_and_boolean(self) -> None:
        for invalid in ("101", True, 0, -1, 1.5, None, derive.MAX_SAFE_INTEGER + 1):
            data = json.loads(json.dumps(self.example))
            data["vendor_integration"]["shipping_option_id"] = invalid
            with self.subTest(invalid=invalid):
                with self.assertRaisesRegex(ValueError, "positive JSON integer"):
                    derive.build_offer_option_map(data, self.example_path)


    def test_default_prices_are_required_and_reported_in_integer_cents(self) -> None:
        result = derive.build_offer_option_map(self.example, self.example_path)
        offer_stack = self.example["pricing_economics_and_offers"]["offer_stack"]
        for visible in result["visible_offers"]:
            with self.subTest(source_offer_key=visible["source_offer_key"]):
                self.assertEqual(
                    visible["default_price"],
                    offer_stack[visible["source_offer_key"]]["default_price"],
                )
                self.assertIsInstance(visible["default_price"]["amount_cents"], int)
                self.assertGreater(visible["default_price"]["amount_cents"], 0)

        data = json.loads(json.dumps(self.example))
        first_offer = next(iter(data["pricing_economics_and_offers"]["offer_stack"].values()))
        first_offer.pop("default_price")
        with self.assertRaisesRegex(ValueError, r"default_price must be an object"):
            derive.build_offer_option_map(data, self.example_path)

    def test_default_price_rejects_invalid_cent_amounts(self) -> None:
        invalid_amounts = ("2495", True, 0, -1, 24.95, None, derive.MAX_SAFE_INTEGER + 1)
        for invalid in invalid_amounts:
            data = json.loads(json.dumps(self.example))
            first_offer = next(iter(data["pricing_economics_and_offers"]["offer_stack"].values()))
            first_offer["default_price"]["amount_cents"] = invalid
            with self.subTest(invalid=invalid):
                with self.assertRaisesRegex(ValueError, "positive safe integer in cents"):
                    derive.build_offer_option_map(data, self.example_path)

    def test_default_price_rejects_invalid_currency_codes(self) -> None:
        for invalid in ("aud", "AU", "AUD ", "USDD", 123, None):
            data = json.loads(json.dumps(self.example))
            first_offer = next(iter(data["pricing_economics_and_offers"]["offer_stack"].values()))
            first_offer["default_price"]["currency"] = invalid
            with self.subTest(invalid=invalid):
                with self.assertRaisesRegex(ValueError, "uppercase three-letter currency code"):
                    derive.build_offer_option_map(data, self.example_path)

    def test_shipping_details_are_required_and_reported(self) -> None:
        result = derive.build_offer_option_map(self.example, self.example_path)
        self.assertEqual(result["shipping_details"], self.example["product_details"]["shipping_details"])

        invalid_cases = [
            ("tracked", "yes", "must be a boolean"),
            ("carrier_delivery_estimate", "", "non-empty string"),
            ("tracking_message", "", "omitted or a non-empty string"),
        ]
        for field, invalid, message in invalid_cases:
            data = json.loads(json.dumps(self.example))
            data["product_details"]["shipping_details"][field] = invalid
            with self.subTest(field=field, invalid=invalid):
                with self.assertRaisesRegex(ValueError, message):
                    derive.build_offer_option_map(data, self.example_path)

    def test_shipping_copy_rejects_fulfillment_origin_emphasis(self) -> None:
        for field in ("carrier_delivery_estimate", "tracking_message"):
            data = json.loads(json.dumps(self.example))
            data["product_details"]["shipping_details"][field] = "Tracked overseas delivery in 7 days"
            with self.subTest(field=field):
                with self.assertRaisesRegex(ValueError, "must not emphasize fulfillment origin"):
                    derive.build_offer_option_map(data, self.example_path)

    def test_obsolete_price_point_input_is_rejected(self) -> None:
        data = json.loads(json.dumps(self.example))
        first_offer = next(iter(data["pricing_economics_and_offers"]["offer_stack"].values()))
        first_offer["price_point"] = "$24.95"
        with self.assertRaisesRegex(ValueError, "obsolete input field"):
            derive.build_offer_option_map(data, self.example_path)

    def test_svg_black_white_contract_is_accepted(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            svg_path = Path(temp) / "logo.svg"
            svg_path.write_text(
                '<svg viewBox="0 0 100 40" xmlns="http://www.w3.org/2000/svg">'
                '<rect width="100" height="40" fill="#fff"/>'
                '<path d="M10 10h80v20H10z" fill="#000"/>'
                '</svg>',
                encoding="utf-8",
            )
            result = derive.inspect_svg_source(svg_path)
            self.assertTrue(result["source_contract_valid"])
            self.assertEqual(result["color_counts"]["black"], 1)
            self.assertEqual(result["color_counts"]["white"], 1)
            self.assertEqual(result["view_box"], "0 0 100 40")


    def test_svg_internal_use_reference_is_allowed(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            svg_path = Path(temp) / "logo.svg"
            svg_path.write_text(
                '<svg xmlns="http://www.w3.org/2000/svg"><defs><path id="mark" fill="#000" d="M0 0h10v10z"/></defs><use href="#mark"/></svg>',
                encoding="utf-8",
            )
            result = derive.inspect_svg_source(svg_path)
            self.assertTrue(result["source_contract_valid"])

    def test_svg_logo_must_be_locally_available(self) -> None:
        data = json.loads(json.dumps(self.example))
        data["brand_system"]["logo_file"] = "./assets/missing-logo.svg"
        with self.assertRaisesRegex(ValueError, "locally available file"):
            derive.build_offer_option_map(data, self.example_path)

    def test_svg_unsupported_visible_color_fails(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            svg_path = Path(temp) / "logo.svg"
            svg_path.write_text(
                '<svg xmlns="http://www.w3.org/2000/svg"><path fill="#f00" d="M0 0h10v10z"/></svg>',
                encoding="utf-8",
            )
            with self.assertRaisesRegex(ValueError, "unsupported visible color"):
                derive.inspect_svg_source(svg_path)

    def test_svg_external_image_and_gradient_fail(self) -> None:
        fixtures = {
            "image": '<svg xmlns="http://www.w3.org/2000/svg"><image href="https://example.com/a.png"/></svg>',
            "gradient": '<svg xmlns="http://www.w3.org/2000/svg"><linearGradient id="g"/></svg>',
        }
        for name, source in fixtures.items():
            with self.subTest(name=name), tempfile.TemporaryDirectory() as temp:
                svg_path = Path(temp) / "logo.svg"
                svg_path.write_text(source, encoding="utf-8")
                with self.assertRaisesRegex(ValueError, "unsupported"):
                    derive.inspect_svg_source(svg_path)


class ValidatorAndDocumentationTests(unittest.TestCase):
    def test_caret_semver_rule_accepts_stable_compatible_ranges_only(self) -> None:
        self.assertIsNotNone(validator.CARET_STABLE_SEMVER.fullmatch("^16.2.10"))
        for invalid in ("16.2.10", "~16.2.10", "latest", "^16.3.0-rc.1", "*"):
            with self.subTest(invalid=invalid):
                self.assertIsNone(validator.CARET_STABLE_SEMVER.fullmatch(invalid))

    def test_required_new_contract_documents_exist(self) -> None:
        required = [
            "references/logo-processing.md",
            "references/quality-and-testing.md",
            "references/dependency-policy.md",
            "references/sub-agent-orchestration.md",
            "references/customer-facing-copy.md",
            "agents/orchestrator.md",
            "agents/customer-experience.md",
            "agents/platform-dependencies.md",
            "agents/brand-assets.md",
            "agents/commerce-checkout.md",
            "agents/quality-runtime.md",
            "agents/deployment.md",
            "templates/AGENTS.md",
            "templates/.vscode/launch.json",
            "templates/.npmrc",
            "templates/.github/workflows/test.yml",
            "templates/.github/workflows/prod.yml",
            "templates/next.config.mjs",
            "templates/styles/mobile-typography.css",
            "templates/lib/env.ts",
            "templates/lib/utils.ts",
            "templates/lib/vendor-response.ts",
            "templates/lib/offer-pricing.ts",
            "templates/components/layout/home-link.tsx",
            "templates/tests/unit/vendor-response.test.ts",
            "templates/tests/unit/utils.test.ts",
            "templates/tests/unit/offer-pricing.test.ts",
            "templates/tests/e2e/navigation.spec.ts",
            "templates/tests/e2e/mobile-customer-ui.spec.ts",
            "templates/tests/unit/dev-runtime-classification.test.mjs",
            "templates/tests/unit/project-boundaries.test.mjs",
            "templates/scripts/prepare-logo-assets.mjs",
            "templates/scripts/check-dependency-freshness.mjs",
            "templates/scripts/check-dependency-health.mjs",
            "templates/scripts/check-dev-runtime.mjs",
            "templates/scripts/check-project-boundaries.mjs",
            "templates/scripts/check-customer-facing-copy.mjs",
            "references/assets/logo.svg",
        ]
        for relative in required:
            with self.subTest(relative=relative):
                self.assertTrue((ROOT / relative).is_file())

    def test_skill_mentions_all_requested_changes(self) -> None:
        skill = (ROOT / "SKILL.md").read_text(encoding="utf-8")
        for marker in [
            "white to transparent alpha",
            ".vscode/launch.json",
            "positive-integer `vendor_integration.shipping_option_id`",
            "100% coverage",
            "caret ranges",
            "sub-agent",
            "STORE_INTEGRATION_ENDPOINT",
            "NOTIFICATION_INTEGRATION_ENDPOINT",
            "OfferSelect",
            "PaymentElement",
            "default_price.amount_cents",
            "formatMoneyFromCents",
            "Back to home",
            "serverReadyAction",
        ]:
            with self.subTest(marker=marker):
                self.assertIn(marker, skill)


    def test_retained_runtime_templates_cover_reported_regressions(self) -> None:
        next_config = (ROOT / "templates" / "next.config.mjs").read_text(encoding="utf-8")
        env_template = (ROOT / "templates" / "lib" / "env.ts").read_text(encoding="utf-8")
        utils_template = (ROOT / "templates" / "lib" / "utils.ts").read_text(encoding="utf-8")
        for marker in ["allowedDevOrigins", "networkInterfaces", "DEV_ALLOWED_ORIGINS", "browserToTerminal"]:
            self.assertIn(marker, next_config)
        for marker in ["SHIPPING_OPTION_ID", "Number.isSafeInteger", "/^[1-9]\\d*$/"]:
            self.assertIn(marker, env_template)
        for marker in [
            "Number.isSafeInteger",
            "formatMoneyFromCents",
            "amountCents < 0",
            "/ 100",
            "normalizedCurrency",
            "Intl.NumberFormat",
            "catch",
        ]:
            self.assertIn(marker, utils_template)

    def test_vendor_response_and_default_pricing_templates_are_executable_contracts(self) -> None:
        vendor = (ROOT / "templates" / "lib" / "vendor-response.ts").read_text(encoding="utf-8")
        for marker in [
            "Promise<Response>",
            "response.ok",
            "response.status",
            "response.json()",
            "callVendorJson",
            "VendorResponseError",
            "invalid_json",
            "invalid_body",
        ]:
            self.assertIn(marker, vendor)
        self.assertEqual(vendor.count("response.json()"), 1)

        pricing = (ROOT / "templates" / "lib" / "offer-pricing.ts").read_text(encoding="utf-8")
        for marker in [
            "amount_cents",
            "createImmediateOfferPrice",
            "applyLiveQuotePrice",
            "refreshOfferPriceInBackground",
            'source: "default"',
            'source: "quote"',
        ]:
            self.assertIn(marker, pricing)

        home_link = (ROOT / "templates" / "components" / "layout" / "home-link.tsx").read_text(
            encoding="utf-8"
        )
        for marker in ['href="/"', 'data-home-link="true"', "Back to home"]:
            self.assertIn(marker, home_link)

    def test_retained_logo_pipeline_has_transparent_pixel_transform(self) -> None:
        script = (ROOT / "templates" / "scripts" / "prepare-logo-assets.mjs").read_text(encoding="utf-8")
        for marker in [
            "sourceCoverage",
            "1 - luminance",
            "logo-primary.png",
            "logo-inverse.png",
            "ensureAlpha",
            "palette: false",
            "opaque white background",
            "atomicWrite",
        ]:
            with self.subTest(marker=marker):
                self.assertIn(marker, script)

    def test_retained_launch_template_is_valid_full_stack_debug_config(self) -> None:
        path = ROOT / "templates" / ".vscode" / "launch.json"
        self.assertTrue(path.is_file())
        data = json.loads(path.read_text(encoding="utf-8"))
        self.assertEqual(
            data,
            {
                "version": "0.2.0",
                "configurations": [
                    {
                        "name": "Next.js: debug full stack",
                        "type": "node-terminal",
                        "request": "launch",
                        "command": "npm run dev",
                        "serverReadyAction": {
                            "pattern": "- Local:.+(https?://.+)",
                            "uriFormat": "%s",
                            "action": "debugWithChrome",
                        },
                    }
                ],
            },
        )

    def test_retained_dev_runtime_gate_covers_server_and_browser_failures(self) -> None:
        script = (ROOT / "templates" / "scripts" / "check-dev-runtime.mjs").read_text(encoding="utf-8")
        for marker in [
            "networkInterfaces",
            "0.0.0.0",
            "console",
            "pageerror",
            "requestfailed",
            "cross-origin",
            "outdated",
            "TypeError",
            "@playwright/test",
            "new Response",
            "data-home-link",
            "waitForURL",
            "React DevTools",
            "[HMR] connected",
            "source map",
            "MOBILE_VIEWPORTS",
            "320",
            "360",
            "390",
            "430",
            "ObjectMultiplex",
            "MaxListenersExceededWarning",
            "private-token",
            "--disable-extensions",
        ]:
            self.assertIn(marker, script)

    def test_retained_dependency_gate_templates_are_executable_contracts(self) -> None:
        freshness = (ROOT / "templates" / "scripts" / "check-dependency-freshness.mjs").read_text(encoding="utf-8")
        health = (ROOT / "templates" / "scripts" / "check-dependency-health.mjs").read_text(encoding="utf-8")
        for marker in ["dist-tags.latest", "CARET_STABLE_SEMVER", "queryLatestStableWithinRange", "isCaretCompatible", "latest stable compatible"]:
            self.assertIn(marker, freshness)
        for marker in ["--dry-run", "--strict-allow-scripts", "approve-scripts", "--allow-scripts-pending", "--json", "allowScripts", "workerd", "engines.node", "UNHEALTHY_OUTPUT"]:
            self.assertIn(marker, health)

    def test_dependency_health_gate_is_required(self) -> None:
        output = (ROOT / "references" / "output-contract.md").read_text(encoding="utf-8")
        validator = (ROOT / "scripts" / "validate_bundle.py").read_text(encoding="utf-8")
        for marker in ["deps:health", "check-dependency-health.mjs", "npm ci --dry-run"]:
            self.assertIn(marker, output)
        self.assertIn("check-dependency-health.mjs", validator)


    def test_customer_experience_contract_is_enforced(self) -> None:
        copy_doc = (ROOT / "references" / "customer-facing-copy.md").read_text(encoding="utf-8")
        mobile_test = (ROOT / "templates" / "tests" / "e2e" / "mobile-customer-ui.spec.ts").read_text(encoding="utf-8")
        for marker in ["320", "360", "390", "430", "Coupon applied to this order", "data-testimonials", "data-load-more-testimonials", "customer-facing"]:
            self.assertIn(marker, copy_doc + mobile_test)
        self.assertNotIn(chr(0x2014), copy_doc)

    def test_test_workflow_targets_all_non_main_branches_and_manual_runs(self) -> None:
        workflow = (ROOT / "templates" / ".github" / "workflows" / "test.yml").read_text(encoding="utf-8")
        for marker in ["push:", "pull_request:", "workflow_dispatch:", "branches-ignore:", "- main", "environment: test", "npm ci --strict-allow-scripts"]:
            self.assertIn(marker, workflow)
        self.assertGreaterEqual(workflow.count("branches-ignore:"), 2)
        self.assertGreaterEqual(workflow.count("- main"), 2)
        self.assertNotIn("feature/", workflow)
        self.assertNotIn("startsWith(github.head_ref", workflow)


    def test_mobile_audits_cover_all_semantic_headlines_and_accurate_line_counts(self) -> None:
        runtime = (ROOT / "templates" / "scripts" / "check-dev-runtime.mjs").read_text(encoding="utf-8")
        e2e = (ROOT / "templates" / "tests" / "e2e" / "mobile-customer-ui.spec.ts").read_text(encoding="utf-8")
        for marker in ["h1,h2,h3,h4,h5,h6", "[role=heading]", "[data-headline=true]", "getClientRects", "lineTops"]:
            self.assertIn(marker, runtime)
            self.assertIn(marker, e2e)

    def test_sub_agent_index_includes_customer_experience(self) -> None:
        index = (ROOT / "agents" / "README.md").read_text(encoding="utf-8")
        self.assertIn("customer-experience.md", index)

    def test_retained_workflows_use_current_action_majors(self) -> None:
        corpus = "\n".join(
            (ROOT / "templates" / ".github" / "workflows" / name).read_text(encoding="utf-8")
            for name in ("test.yml", "prod.yml")
        )
        self.assertIn("actions/checkout@v7", corpus)
        self.assertIn("actions/setup-node@v7", corpus)

    def test_skill_package_has_no_legacy_typo_or_literal_em_dash(self) -> None:
        text_files = [
            path for path in ROOT.rglob("*")
            if path.is_file() and path.suffix.lower() in {".md", ".py", ".ts", ".tsx", ".js", ".mjs", ".json", ".yml", ".yaml", ".css"}
        ]
        corpus = "\n".join(path.read_text(encoding="utf-8", errors="ignore") for path in text_files)
        legacy_typo = "instr" + "gram_page"
        self.assertNotIn(legacy_typo, corpus)
        self.assertNotIn(chr(0x2014), corpus)

    def test_logo_derivation_uses_only_project_local_output_paths(self) -> None:
        result = derive.build_offer_option_map(json.loads((ROOT / "references" / "example_input.json").read_text()), ROOT / "references" / "example_input.json")
        logo = result["logo"]
        self.assertEqual(logo["project_source_path"], "source-assets/logo.svg")
        self.assertNotIn("/tmp/", json.dumps(logo))
        self.assertNotIn("/home/", json.dumps(logo))
        self.assertNotIn("file:///", json.dumps(logo))

    def test_removed_endpoint_names_and_bundle_event_are_absent(self) -> None:
        text_files = [
            path
            for path in ROOT.rglob("*")
            if path.is_file() and path.suffix.lower() in {".md", ".py", ".ts", ".tsx", ".js", ".mjs", ".json", ".yml", ".yaml"}
        ]
        corpus = "\n".join(path.read_text(encoding="utf-8", errors="ignore") for path in text_files)
        removed_identifiers = (
            "SUBSCRIPTION" + "_INTEGRATION_ENDPOINT",
            "CONTACT_US" + "_INTEGRATION_ENDPOINT",
            "Bundle" + "Select",
        )
        for removed in removed_identifiers:
            self.assertNotIn(removed, corpus)



if __name__ == "__main__":
    unittest.main()
