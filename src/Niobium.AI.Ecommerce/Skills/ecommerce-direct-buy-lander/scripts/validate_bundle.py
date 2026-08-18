#!/usr/bin/env python3
"""Strict structural validator for ecommerce direct-buy landing page bundles.

The validator is intentionally warning-free: every detected gap is a validation
error. Generated projects must also run their executable quality suite; this
script verifies that the required files, scripts, contracts, and guardrails are
present before completion.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

REQUIRED_ROUTES = [
    "page.tsx",
    "checkout/page.tsx",
    "contact/page.tsx",
    "track-order/page.tsx",
    "order-status/page.tsx",
    "privacy-policy/page.tsx",
    "terms/page.tsx",
    "returns-policy/page.tsx",
    "shipping-policy/page.tsx",
]

REQUIRED_WORKFLOWS = [
    ".github/workflows/test.yml",
    ".github/workflows/prod.yml",
]

REQUIRED_SCRIPTS = [
    "scripts/deploy-cloudflare-pages.mjs",
    "scripts/generate-public-env.mjs",
    "scripts/export-offer-env.mjs",
    "scripts/prepare-logo-assets.mjs",
    "scripts/check-dependency-freshness.mjs",
    "scripts/check-dependency-health.mjs",
    "scripts/check-dev-runtime.mjs",
    "scripts/check-project-boundaries.mjs",
    "scripts/check-customer-facing-copy.mjs",
]

REQUIRED_PROJECT_FILES = [
    "README.md",
    ".env.example",
    ".nvmrc",
    "AGENTS.md",
    ".vscode/launch.json",
    "package.json",
    "package-lock.json",
    ".npmrc",
    ".gitignore",
    "tsconfig.json",
    "eslint.config.mjs",
    "playwright.config.ts",
    "config/offer-options.json",
    "config/site-input-summary.json",
    "config/testimonials.json",
    "config/legal-content-manifest.json",
]

REQUIRED_PACKAGE_SCRIPTS = [
    "prepare:app",
    "dev",
    "deps:check",
    "deps:health",
    "deps:scripts",
    "project:boundaries",
    "test:content",
    "lint",
    "typecheck",
    "test",
    "test:coverage",
    "serve:static",
    "test:e2e",
    "test:runtime",
    "quality",
    "build",
    "deploy",
]

REQUIRED_DIRECT_PACKAGES = [
    "next",
    "react",
    "react-dom",
    "typescript",
    "@types/node",
    "@types/react",
    "@types/react-dom",
    "tailwindcss",
    "@tailwindcss/postcss",
    "postcss",
    "eslint",
    "vitest",
    "@vitest/coverage-v8",
    "@testing-library/react",
    "@testing-library/jest-dom",
    "@testing-library/user-event",
    "jsdom",
    "@playwright/test",
    "serve",
    "@stripe/stripe-js",
    "@stripe/react-stripe-js",
    "sharp",
    "wrangler",
]

FORBIDDEN_PATTERNS = {
    "add_to_cart_language": re.compile(r"add\s+to\s+cart", re.IGNORECASE),
    "waitlist": re.compile(r"\bwaitlist\b", re.IGNORECASE),
    "countdown": re.compile(r"countdown|ends in|hours left|minutes left", re.IGNORECASE),
    "server_action_directive": re.compile(r"['\"]use server['\"]"),
    "fulfillment_origin_wording": re.compile(r"\boverseas?\b", re.IGNORECASE),
    "em_dash": re.compile("\u2014"),
    "ambiguous_coupon_label": re.compile(r"\bactive coupon\b", re.IGNORECASE),
    "owner_facing_checkout_copy": re.compile(r"\ba focused,?\s+guest checkout\b|\bguest checkout\b", re.IGNORECASE),
    "conversion_meta_copy": re.compile(r"\bconversion[- ]focused\b|\bconversion rate\b", re.IGNORECASE),
    "friction_meta_copy": re.compile(r"\blow[- ]friction\b|\breduce friction\b", re.IGNORECASE),
    "operator_meta_copy": re.compile(r"\boffer stack\b|\bmessage match(?:ed)?\b|\bpurchase flow\b|\bwebsite owner\b|\bsite owner\b|\bbusiness operator\b", re.IGNORECASE),
}

SUPPORTED_COUNTRIES = {"US", "UK", "CA", "AU", "SG", "NZ", "IE"}
CARET_STABLE_SEMVER = re.compile(r"^\^\d+\.\d+\.\d+(?:\+[0-9A-Za-z.-]+)?$")
LOWER_SNAKE_KEY_RE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
STORE_ENDPOINT_RE = re.compile(r"\b(?:STORE_INTEGRATION_ENDPOINT|storeIntegrationEndpoint|store_integration_endpoint)\b")
NOTIFICATION_ENDPOINT_RE = re.compile(
    r"\b(?:NOTIFICATION_INTEGRATION_ENDPOINT|notificationIntegrationEndpoint|notification_integration_endpoint)\b"
)
MAX_SAFE_INTEGER = 9_007_199_254_740_991
CURRENCY_RE = re.compile(r"^[A-Z]{3}$")
FORBIDDEN_ORIGIN_WORDING_RE = re.compile(r"\boverseas?\b", re.IGNORECASE)
POLICY_FIELDS = {
    "privacy_policy": "content/policies/privacy-policy.md",
    "terms": "content/policies/terms.md",
    "returns_policy": "content/policies/returns-policy.md",
    "shipping_policy": "content/policies/shipping-policy.md",
}
CANONICAL_VENDOR_SCRIPT_URLS = [
    "https://assets.store.niobium.co.nz/quote.js",
    "https://assets.store.niobium.co.nz/order.js",
    "https://assets.notification.niobium.co.nz/subscribe.js",
    "https://assets.notification.niobium.co.nz/contact-us.js",
    "https://assets.store.niobium.co.nz/track.js",
]

OBSOLETE_INPUT_FIELDS = {
    "checkout_url",
    "price_point",
    "subscription_integration_endpoint",
    "contact_us_integration_endpoint",
}


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        data = json.load(f)
    if not isinstance(data, dict):
        raise ValueError("input JSON root must be an object")
    return data


def find_app_dir(project_dir: Path) -> Path | None:
    direct = project_dir / "app"
    if direct.is_dir():
        return direct
    src = project_dir / "src" / "app"
    if src.is_dir():
        return src
    return None


def find_source_dir(project_dir: Path, name: str) -> Path | None:
    direct = project_dir / name
    if direct.is_dir():
        return direct
    src = project_dir / "src" / name
    if src.is_dir():
        return src
    return None


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return ""


def strip_asset_suffix(value: str) -> str:
    return re.split(r"[?#]", value, maxsplit=1)[0]


def detect_svg_logo_input(input_data: dict[str, Any], input_json_dir: Path) -> tuple[bool, str | None, Path | None]:
    brand = input_data.get("brand_system")
    logo_file = brand.get("logo_file") if isinstance(brand, dict) else None
    if not isinstance(logo_file, str) or not logo_file.strip():
        return False, None, None
    asset_path = strip_asset_suffix(logo_file.strip())
    logo_name = Path(asset_path).name
    if re.match(r"^[a-z][a-z0-9+.-]*://", asset_path, re.IGNORECASE):
        return asset_path.lower().endswith(".svg"), logo_name, None
    candidate = Path(asset_path)
    if not candidate.is_absolute():
        candidate = input_json_dir / candidate
    candidate = candidate.resolve()
    is_svg = asset_path.lower().endswith(".svg")
    try:
        if candidate.is_file() and not is_svg:
            head = candidate.read_text(encoding="utf-8", errors="ignore")[:512].lstrip()
            is_svg = head.startswith("<svg") or "<svg" in head[:128]
    except OSError:
        pass
    return is_svg, logo_name, candidate if candidate.is_file() else None


def collect_text_files(project_dir: Path) -> list[Path]:
    exts = {
        ".ts",
        ".tsx",
        ".js",
        ".jsx",
        ".mjs",
        ".mts",
        ".md",
        ".css",
        ".json",
        ".yml",
        ".yaml",
        ".svg",
        ".html",
        ".txt",
        ".toml",
    }
    ignored_parts = {"node_modules", ".next", "out", ".git", "coverage", "test-results", "playwright-report"}
    return [
        path
        for path in project_dir.rglob("*")
        if path.is_file() and path.suffix.lower() in exts and not any(part in ignored_parts for part in path.parts)
    ]


def collect_source_text(paths: Iterable[Path]) -> str:
    return "\n".join(read_text(path) for path in paths)


def check_exists(path: Path, rel: str, errors: list[str]) -> None:
    if not (path / rel).exists():
        errors.append(f"missing required file: {rel}")


def validate_lower_snake_keys(value: Any, errors: list[str], path: str = "$") -> None:
    if isinstance(value, dict):
        for key, child in value.items():
            if not isinstance(key, str) or not LOWER_SNAKE_KEY_RE.fullmatch(key):
                errors.append(f"input JSON field must use lower snake case at {path}: {key!r}")
            if key in OBSOLETE_INPUT_FIELDS:
                errors.append(f"obsolete input field is not supported at {path}: {key}")
            validate_lower_snake_keys(child, errors, f"{path}.{key}")
    elif isinstance(value, list):
        for index, child in enumerate(value):
            validate_lower_snake_keys(child, errors, f"{path}[{index}]")


def _extract_call_argument_blocks(text: str, callee: str) -> list[str]:
    """Extract argument text for JavaScript/TypeScript calls to a known callee."""
    blocks: list[str] = []
    offset = 0
    while True:
        found = text.find(callee, offset)
        if found < 0:
            return blocks
        cursor = found + len(callee)
        while cursor < len(text) and text[cursor].isspace():
            cursor += 1
        if cursor >= len(text) or text[cursor] != "(":
            offset = found + len(callee)
            continue
        start = cursor + 1
        cursor += 1
        depth = 1
        quote: str | None = None
        escaped = False
        line_comment = False
        block_comment = False
        while cursor < len(text):
            char = text[cursor]
            nxt = text[cursor + 1] if cursor + 1 < len(text) else ""
            if line_comment:
                if char == "\n":
                    line_comment = False
                cursor += 1
                continue
            if block_comment:
                if char == "*" and nxt == "/":
                    block_comment = False
                    cursor += 2
                else:
                    cursor += 1
                continue
            if quote:
                if escaped:
                    escaped = False
                elif char == "\\":
                    escaped = True
                elif char == quote:
                    quote = None
                cursor += 1
                continue
            if char == "/" and nxt == "/":
                line_comment = True
                cursor += 2
                continue
            if char == "/" and nxt == "*":
                block_comment = True
                cursor += 2
                continue
            if char in {"'", '"', "`"}:
                quote = char
                cursor += 1
                continue
            if char == "(":
                depth += 1
            elif char == ")":
                depth -= 1
                if depth == 0:
                    blocks.append(text[start:cursor])
                    offset = cursor + 1
                    break
            cursor += 1
        else:
            return blocks


def _split_top_level_arguments(block: str) -> list[str]:
    arguments: list[str] = []
    start = 0
    stack: list[str] = []
    pairs = {")": "(", "]": "[", "}": "{"}
    quote: str | None = None
    escaped = False
    line_comment = False
    block_comment = False
    cursor = 0
    while cursor < len(block):
        char = block[cursor]
        nxt = block[cursor + 1] if cursor + 1 < len(block) else ""
        if line_comment:
            if char == "\n":
                line_comment = False
            cursor += 1
            continue
        if block_comment:
            if char == "*" and nxt == "/":
                block_comment = False
                cursor += 2
            else:
                cursor += 1
            continue
        if quote:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == quote:
                quote = None
            cursor += 1
            continue
        if char == "/" and nxt == "/":
            line_comment = True
            cursor += 2
            continue
        if char == "/" and nxt == "*":
            block_comment = True
            cursor += 2
            continue
        if char in {"'", '"', "`"}:
            quote = char
        elif char in "([{":
            stack.append(char)
        elif char in ")]}" and stack and stack[-1] == pairs[char]:
            stack.pop()
        elif char == "," and not stack:
            arguments.append(block[start:cursor].strip())
            start = cursor + 1
        cursor += 1
    tail = block[start:].strip()
    if tail or arguments:
        arguments.append(tail)
    return arguments


def require_final_endpoint_argument(
    source_text: str,
    callee: str,
    endpoint_pattern: re.Pattern[str],
    endpoint_name: str,
    errors: list[str],
) -> None:
    calls = _extract_call_argument_blocks(source_text, callee)
    if not calls:
        errors.append(f"application source does not contain a callable {callee} integration")
        return
    for index, block in enumerate(calls):
        arguments = _split_top_level_arguments(block)
        if not arguments or not endpoint_pattern.search(arguments[-1]):
            errors.append(f"{callee} call #{index + 1} must pass {endpoint_name} as its final argument")


def _extract_call_spans(text: str, callee: str) -> list[tuple[int, int]]:
    """Return [start, end) spans for syntactic calls to a known JavaScript/TypeScript callee."""
    spans: list[tuple[int, int]] = []
    offset = 0
    while True:
        found = text.find(callee, offset)
        if found < 0:
            return spans
        cursor = found + len(callee)
        while cursor < len(text) and text[cursor].isspace():
            cursor += 1
        if cursor >= len(text) or text[cursor] != "(":
            offset = found + len(callee)
            continue
        cursor += 1
        depth = 1
        quote: str | None = None
        escaped = False
        line_comment = False
        block_comment = False
        while cursor < len(text):
            char = text[cursor]
            nxt = text[cursor + 1] if cursor + 1 < len(text) else ""
            if line_comment:
                if char == "\n":
                    line_comment = False
                cursor += 1
                continue
            if block_comment:
                if char == "*" and nxt == "/":
                    block_comment = False
                    cursor += 2
                else:
                    cursor += 1
                continue
            if quote:
                if escaped:
                    escaped = False
                elif char == "\\":
                    escaped = True
                elif char == quote:
                    quote = None
                cursor += 1
                continue
            if char == "/" and nxt == "/":
                line_comment = True
                cursor += 2
                continue
            if char == "/" and nxt == "*":
                block_comment = True
                cursor += 2
                continue
            if char in {"'", '"', "`"}:
                quote = char
                cursor += 1
                continue
            if char == "(":
                depth += 1
            elif char == ")":
                depth -= 1
                if depth == 0:
                    spans.append((found, cursor + 1))
                    offset = cursor + 1
                    break
            cursor += 1
        else:
            return spans


def require_vendor_calls_wrapped(source_text: str, callee: str, errors: list[str]) -> None:
    vendor_spans = _extract_call_spans(source_text, callee)
    wrapper_spans = _extract_call_spans(source_text, "callVendorJson")
    for index, (vendor_start, _vendor_end) in enumerate(vendor_spans):
        if not any(wrapper_start < vendor_start < wrapper_end for wrapper_start, wrapper_end in wrapper_spans):
            errors.append(
                f"{callee} call #{index + 1} must be awaited through callVendorJson so raw Response status/JSON handling is applied"
            )


def validate_default_price(value: Any, label: str, errors: list[str]) -> None:
    if not isinstance(value, dict):
        errors.append(f"{label} must be an object")
        return
    amount_cents = value.get("amount_cents")
    currency = value.get("currency")
    if (
        isinstance(amount_cents, bool)
        or not isinstance(amount_cents, int)
        or amount_cents <= 0
        or amount_cents > MAX_SAFE_INTEGER
    ):
        errors.append(f"{label}.amount_cents must be a positive safe integer in cents")
    if not isinstance(currency, str) or not CURRENCY_RE.fullmatch(currency):
        errors.append(f"{label}.currency must be an uppercase three-letter currency code")


def validate_shipping_details(input_data: dict[str, Any], errors: list[str]) -> None:
    product_details = input_data.get("product_details")
    shipping = product_details.get("shipping_details") if isinstance(product_details, dict) else None
    if not isinstance(shipping, dict):
        errors.append("input product_details.shipping_details must be an object")
        return
    if not isinstance(shipping.get("tracked"), bool):
        errors.append("input product_details.shipping_details.tracked must be boolean")
    eta = shipping.get("carrier_delivery_estimate")
    if not isinstance(eta, str) or not eta.strip():
        errors.append("input product_details.shipping_details.carrier_delivery_estimate must be non-empty")
    tracking_message = shipping.get("tracking_message")
    if tracking_message is not None and (not isinstance(tracking_message, str) or not tracking_message.strip()):
        errors.append("input product_details.shipping_details.tracking_message must be omitted or non-empty")
    for field_name, field_value in (
        ("carrier_delivery_estimate", eta),
        ("tracking_message", tracking_message),
    ):
        if isinstance(field_value, str) and FORBIDDEN_ORIGIN_WORDING_RE.search(field_value):
            errors.append(
                f"input product_details.shipping_details.{field_name} must not emphasize fulfillment origin"
            )


def validate_input_contract(input_data: dict[str, Any], errors: list[str]) -> list[str]:
    expected_offer_keys: list[str] = []
    validate_lower_snake_keys(input_data, errors)

    short_product = input_data.get("short_product_name")
    if not isinstance(short_product, str) or not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", short_product.strip()):
        errors.append("input short_product_name is missing or not a lowercase hyphen slug")

    target_country = input_data.get("target_country")
    if not isinstance(target_country, str) or target_country.upper() not in SUPPORTED_COUNTRIES:
        errors.append(f"input target_country must be one of {sorted(SUPPORTED_COUNTRIES)}")

    validate_shipping_details(input_data, errors)

    vendor = input_data.get("vendor_integration")
    shipping_option_id = vendor.get("shipping_option_id") if isinstance(vendor, dict) else None
    if (
        isinstance(shipping_option_id, bool)
        or not isinstance(shipping_option_id, int)
        or shipping_option_id <= 0
        or shipping_option_id > MAX_SAFE_INTEGER
    ):
        errors.append(
            "input vendor_integration.shipping_option_id must be a positive JSON integer within the JavaScript safe-integer range"
        )
    if isinstance(vendor, dict):
        for endpoint_key in ("store_integration_endpoint", "notification_integration_endpoint"):
            endpoint_value = vendor.get(endpoint_key)
            if not isinstance(endpoint_value, str) or not endpoint_value.strip():
                errors.append(f"input vendor_integration.{endpoint_key} must be a non-empty string")
        removed_input_fields = (
            "subscription" + "_integration_endpoint",
            "contact_us" + "_integration_endpoint",
        )
        for removed_key in removed_input_fields:
            if removed_key in vendor:
                errors.append(f"removed per-form endpoint field must not be used: vendor_integration.{removed_key}")
    else:
        errors.append("input vendor_integration must be an object")


    trust = input_data.get("trust_signal")
    if not isinstance(trust, dict):
        errors.append("input trust_signal must be an object")
    else:
        for trust_key in ("contact_email", "facebook_page", "instagram_page", *POLICY_FIELDS.keys()):
            trust_value = trust.get(trust_key)
            if not isinstance(trust_value, str) or not trust_value.strip():
                errors.append(f"input trust_signal.{trust_key} must be a non-empty string")
        testimonials = trust.get("testimonials")
        if not isinstance(testimonials, list) or len(testimonials) < 3:
            errors.append("input trust_signal.testimonials must contain at least three entries")
        else:
            for testimonial_index, testimonial in enumerate(testimonials):
                label = f"input trust_signal.testimonials[{testimonial_index}]"
                if not isinstance(testimonial, dict):
                    errors.append(f"{label} must be an object")
                    continue
                if not isinstance(testimonial.get("name"), str) or not testimonial["name"].strip():
                    errors.append(f"{label}.name must be non-empty")
                if not isinstance(testimonial.get("testimonial"), str) or not testimonial["testimonial"].strip():
                    errors.append(f"{label}.testimonial must be non-empty")

    pricing = input_data.get("pricing_economics_and_offers", {})
    offer_stack = pricing.get("offer_stack") if isinstance(pricing, dict) else None
    mappings = pricing.get("offer_options_mapping") if isinstance(pricing, dict) else None
    if not isinstance(offer_stack, dict) or not offer_stack:
        errors.append("input missing pricing_economics_and_offers.offer_stack")
    else:
        for offer_key, offer_value in offer_stack.items():
            if not isinstance(offer_value, dict):
                errors.append(f"input offer_stack.{offer_key} must be an object")
                continue
            validate_default_price(
                offer_value.get("default_price"),
                f"input pricing_economics_and_offers.offer_stack.{offer_key}.default_price",
                errors,
            )
    if not isinstance(mappings, list) or not mappings:
        errors.append("input missing pricing_economics_and_offers.offer_options_mapping")
        return expected_offer_keys

    seen: set[str] = set()
    recommended_count = 0
    for index, mapping in enumerate(mappings):
        if not isinstance(mapping, dict):
            errors.append(f"offer_options_mapping[{index}] must be an object")
            continue
        source = mapping.get("source_offer_key")
        if not isinstance(source, str) or not source:
            errors.append(f"offer_options_mapping[{index}].source_offer_key missing")
        elif isinstance(offer_stack, dict) and source not in offer_stack:
            errors.append(f"offer_options_mapping[{index}].source_offer_key not found in offer_stack: {source}")
        raw_key = mapping.get("offer_option_key")
        if isinstance(raw_key, bool):
            key = ""
        else:
            key = str(raw_key).strip() if isinstance(raw_key, (str, int)) else ""
        if not re.fullmatch(r"[1-9][0-9]*", key):
            errors.append(f"offer_options_mapping[{index}].offer_option_key must be positive integer/digit string")
        elif key in seen:
            errors.append(f"duplicate offer_option_key in input: {key}")
        else:
            seen.add(key)
            expected_offer_keys.append(key)
        if mapping.get("recommended") is True:
            recommended_count += 1
        elif mapping.get("recommended") is not False:
            errors.append(f"offer_options_mapping[{index}].recommended must be boolean")
        config = mapping.get("option_configuration")
        if not isinstance(config, list) or not config:
            errors.append(f"offer_options_mapping[{index}].option_configuration must be non-empty array")
        else:
            for item_index, item in enumerate(config):
                label = f"offer_options_mapping[{index}].option_configuration[{item_index}]"
                if not isinstance(item, dict) or set(item.keys()) != {"listing", "option", "quantity"}:
                    errors.append(f"{label} must contain only listing, option, quantity")
                    continue
                listing = item.get("listing")
                quantity = item.get("quantity")
                option = item.get("option")
                if isinstance(listing, bool) or not isinstance(listing, int) or listing <= 0:
                    errors.append(f"{label}.listing must be a positive integer")
                if isinstance(quantity, bool) or not isinstance(quantity, int) or quantity <= 0:
                    errors.append(f"{label}.quantity must be a positive integer")
                if not isinstance(option, str) or not option.strip():
                    errors.append(f"{label}.option must be a non-empty string")
    if recommended_count != 1:
        errors.append(f"offer_options_mapping must contain exactly one recommended=true mapping; found {recommended_count}")
    return expected_offer_keys


def find_config(project_dir: Path, names: list[str]) -> Path | None:
    return next((project_dir / name for name in names if (project_dir / name).exists()), None)


def require_markers(text: str, markers: list[str], label: str, errors: list[str]) -> None:
    for marker in markers:
        if marker not in text:
            errors.append(f"{label} missing required marker: {marker}")


def validate_launch_json(project_dir: Path, errors: list[str]) -> None:
    path = project_dir / ".vscode" / "launch.json"
    if not path.exists():
        return
    try:
        data = json.loads(read_text(path))
    except json.JSONDecodeError:
        errors.append(".vscode/launch.json is not valid JSON")
        return
    expected = {
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
    }
    if data != expected:
        errors.append(
            ".vscode/launch.json must exactly use the supported Next.js full-stack node-terminal debug configuration"
        )


def validate_dependency_ranges(package_data: dict[str, Any], errors: list[str]) -> None:
    dependencies: dict[str, str] = {}
    for section in ("dependencies", "devDependencies"):
        values = package_data.get(section, {})
        if not isinstance(values, dict):
            errors.append(f"package.json {section} must be an object")
            continue
        for name, version in values.items():
            if not isinstance(version, str) or not CARET_STABLE_SEMVER.fullmatch(version):
                errors.append(
                    f"direct dependency {name} must use a stable caret range such as ^4.113.0; found {version!r}"
                )
            dependencies[name] = version
    for package in REQUIRED_DIRECT_PACKAGES:
        if package not in dependencies:
            errors.append(f"package.json missing required direct dependency: {package}")


def validate_allow_scripts(package_data: dict[str, Any], lock_data: dict[str, Any], errors: list[str]) -> None:
    allow_scripts = package_data.get("allowScripts")
    if not isinstance(allow_scripts, dict):
        errors.append("package.json must contain an allowScripts object")
        return
    required: list[str] = []
    packages = lock_data.get("packages") if isinstance(lock_data, dict) else None
    if isinstance(packages, dict):
        for lock_path, entry in packages.items():
            if not isinstance(entry, dict) or entry.get("hasInstallScript") is not True:
                continue
            version = entry.get("version")
            if not isinstance(version, str) or "node_modules/" not in lock_path:
                continue
            package_name = entry.get("name")
            if not isinstance(package_name, str) or not package_name:
                package_name = lock_path.rsplit("node_modules/", 1)[-1]
            required.append(f"{package_name}@{version}")
    for key in required:
        if allow_scripts.get(key) not in {True, False}:
            errors.append(f"allowScripts missing reviewed install-script decision: {key}")
    workerd_keys = [key for key in required if key.startswith("workerd@")]
    if workerd_keys and not any(allow_scripts.get(key) is True for key in workerd_keys):
        errors.append("resolved workerd install script must be explicitly approved in allowScripts")


def resolve_input_local_file(input_json_path: Path, raw: str) -> Path:
    cleaned = strip_asset_suffix(raw.strip())
    if re.match(r"^[a-z][a-z0-9+.-]*://", cleaned, re.IGNORECASE):
        raise ValueError("remote URLs are not accepted for binding legal policy content")
    path = Path(cleaned)
    if not path.is_absolute():
        path = input_json_path.parent / path
    return path.resolve()


def validate_testimonial_contract(
    project_dir: Path,
    input_data: dict[str, Any],
    source_text: str,
    test_text: str,
    errors: list[str],
) -> None:
    expected = input_data.get("trust_signal", {}).get("testimonials", [])
    testimonial_path = project_dir / "config" / "testimonials.json"
    try:
        actual = json.loads(read_text(testimonial_path))
    except json.JSONDecodeError:
        actual = None
        errors.append("config/testimonials.json is not valid JSON")
    if actual != expected:
        errors.append("config/testimonials.json must preserve every input testimonial and field exactly, in input order")

    required_source_markers = [
        'data-testimonials="true"',
        'data-testimonials-total',
        'data-testimonials-visible',
        'data-testimonial="true"',
        'data-load-more-testimonials="true"',
        'visibleCount',
        '.slice(0, visibleCount)',
        '{item.name}',
        '{item.testimonial}',
    ]
    require_markers(source_text, required_source_markers, "testimonial implementation", errors)
    if re.search(r"(?:item\.testimonial|testimonial\.testimonial|testimonialText)\s*\.\s*(?:slice|substring)\s*\(", source_text, re.IGNORECASE):
        errors.append("testimonial text must not be shortened, truncated, or paraphrased")

    home_path = next(
        (
            candidate
            for candidate in [project_dir / "app" / "page.tsx", project_dir / "src" / "app" / "page.tsx"]
            if candidate.is_file()
        ),
        None,
    )
    if home_path is not None:
        home_text = read_text(home_path)
        import_match = re.search(
            r'''import\s+([A-Za-z_$][\w$]*)\s+from\s+["'][^"']*config/testimonials\.json["']''',
            home_text,
        )
        if import_match is None:
            errors.append("home page must import config/testimonials.json as the sole testimonial data source")
        else:
            identifier = re.escape(import_match.group(1))
            if not re.search(
                rf"<Testimonials\b[^>]*\btestimonials\s*=\s*\{{\s*{identifier}\s*\}}",
                home_text,
                re.DOTALL,
            ):
                errors.append("home page must pass the complete imported testimonial array to <Testimonials>")

    for marker in [
        "data-load-more-testimonials",
        "testimonialCount",
        "while",
        "toHaveCount",
        "testimonials.json",
        "testimonial.name",
        "testimonial.testimonial",
    ]:
        if marker not in test_text:
            errors.append(f"tests missing testimonial completeness/load-more marker: {marker}")


def validate_legal_policy_contract(
    project_dir: Path,
    input_json_path: Path,
    input_data: dict[str, Any],
    source_text: str,
    test_text: str,
    errors: list[str],
) -> None:
    trust = input_data.get("trust_signal")
    if not isinstance(trust, dict):
        return
    manifest_path = project_dir / "config" / "legal-content-manifest.json"
    try:
        manifest = json.loads(read_text(manifest_path))
    except json.JSONDecodeError:
        manifest = None
        errors.append("config/legal-content-manifest.json is not valid JSON")
    if not isinstance(manifest, dict):
        manifest = {}
        errors.append("config/legal-content-manifest.json must be an object keyed by policy input field")

    legal_helper = find_config(project_dir, ["lib/legal-content.ts", "src/lib/legal-content.ts"])
    legal_helper_text = read_text(legal_helper) if legal_helper is not None else ""
    require_markers(
        legal_helper_text,
        ["readFileSync", "process.cwd()", "content/policies", "utf8", "readPolicySource"],
        "byte-bound legal content helper",
        errors,
    )

    app_root = next(
        (candidate for candidate in [project_dir / "app", project_dir / "src" / "app"] if candidate.is_dir()),
        None,
    )
    route_by_field = {
        "privacy_policy": "privacy-policy/page.tsx",
        "terms": "terms/page.tsx",
        "returns_policy": "returns-policy/page.tsx",
        "shipping_policy": "shipping-policy/page.tsx",
    }

    for field, relative in POLICY_FIELDS.items():
        raw = trust.get(field)
        if not isinstance(raw, str) or not raw.strip():
            continue
        try:
            source_path = resolve_input_local_file(input_json_path, raw)
        except ValueError as exc:
            errors.append(f"input trust_signal.{field}: {exc}")
            continue
        if not source_path.is_file():
            errors.append(f"input legal policy source does not exist: trust_signal.{field}={raw}")
            continue
        generated_path = project_dir / relative
        if not generated_path.is_file():
            errors.append(f"missing generated legal policy file: {relative}")
            continue
        expected_bytes = source_path.read_bytes()
        actual_bytes = generated_path.read_bytes()
        if actual_bytes != expected_bytes:
            errors.append(f"{relative} must be a byte-for-byte copy of input trust_signal.{field}")
        expected_hash = hashlib.sha256(expected_bytes).hexdigest()
        entry = manifest.get(field)
        if not isinstance(entry, dict):
            errors.append(f"legal manifest missing object entry: {field}")
        else:
            if entry.get("project_path") != relative or entry.get("sha256") != expected_hash:
                errors.append(f"legal manifest entry {field} must bind {relative} to the exact input SHA-256")
        if relative not in legal_helper_text:
            errors.append(f"legal content helper must map the exact policy file: {relative}")
        if app_root is not None:
            route_path = app_root / route_by_field[field]
            route_text = read_text(route_path)
            bound_call = re.search(
                rf'''readPolicySource\s*\(\s*["']{re.escape(field)}["']\s*\)''',
                route_text,
            )
            source_marker = re.search(
                rf'''data-policy-source\s*=\s*["']{re.escape(field)}["']''',
                route_text,
            )
            if bound_call is None or source_marker is None:
                errors.append(
                    f"policy route must render exact source through readPolicySource({field}): {route_by_field[field]}"
                )
        if relative not in test_text or expected_hash not in test_text or "readPolicySource" not in test_text:
            errors.append(f"tests must verify exact legal policy bytes/hash and source rendering for {relative}")


def validate_canonical_integration_contract(source_text: str, errors: list[str]) -> None:
    require_markers(
        source_text,
        [
            "next/script",
            "https://www.googletagmanager.com/gtag/js?id=",
            "window.dataLayer",
            "gtag('config'",
            "https://connect.facebook.net/en_US/fbevents.js",
            "fbq('init'",
            "fbq('track', 'PageView'",
            "https://www.clarity.ms/tag/",
        ] + CANONICAL_VENDOR_SCRIPT_URLS,
        "canonical third-party script integration",
        errors,
    )
    if re.search(r"\b(?:loadExternalScript|injectScript|ensureScript|appendVendorScript|createScriptTag)\b", source_text):
        errors.append("custom client-side script loaders are forbidden; render canonical snippets with next/script")
    if not re.search(r'''import\s+Script\s+from\s+["']next/script["']''', source_text):
        errors.append("canonical third-party snippets must import the default Script component from next/script")
    if "<Script" not in source_text:
        errors.append("canonical third-party snippets must render Next.js <Script> elements")
    if re.search(
        r"fetch\s*\([^)]*(?:STORE_INTEGRATION_ENDPOINT|storeIntegrationEndpoint|store_integration_endpoint|NOTIFICATION_INTEGRATION_ENDPOINT|notificationIntegrationEndpoint|notification_integration_endpoint)",
        source_text,
        re.DOTALL,
    ):
        errors.append("do not fetch integration endpoints directly; pass them only to the documented vendor globals")
    if not re.search(r'''from\s+["']@stripe/stripe-js["']''', source_text) or not re.search(
        r'''from\s+["']@stripe/react-stripe-js["']''', source_text
    ):
        errors.append("Stripe Payment Element must use the official @stripe/stripe-js and @stripe/react-stripe-js packages")
    require_markers(
        source_text,
        [
            "loadStripe",
            "Elements",
            "PaymentElement",
            "useStripe",
            "useElements",
            "elements.submit",
            "stripe.confirmPayment",
            "mode",
            "amount",
            "currency",
        ],
        "canonical Stripe React integration",
        errors,
    )


def validate_checkout_contract(source_text: str, test_text: str, errors: list[str]) -> None:
    markers = [
        'data-checkout-order-summary="true"',
        'data-checkout-coupon="true"',
        'data-coupon-toggle="true"',
        'data-checkout-shipping-form="true"',
        'data-checkout-payment="true"',
    ]
    require_markers(source_text, markers, "checkout information hierarchy", errors)
    positions = [source_text.find(marker) for marker in markers]
    if all(position >= 0 for position in positions):
        summary, coupon, _toggle, shipping, payment = positions
        if not (summary < shipping and summary < payment):
            errors.append("checkout order summary must appear before shipping and payment in source/DOM order")
        if not (summary <= coupon < shipping):
            errors.append("compact coupon UI must be embedded in the order-summary region, not above checkout")
    for marker in [
        "data-checkout-order-summary",
        "data-checkout-coupon",
        "data-checkout-shipping-form",
        "data-checkout-payment",
        "boundingBox",
        "toBeLessThan",
    ]:
        if marker not in test_text:
            errors.append(f"tests missing checkout hierarchy/compact-coupon marker: {marker}")


def validate_local_script_coverage(
    project_dir: Path,
    package_data: dict[str, Any],
    vitest_text: str,
    test_text: str,
    errors: list[str],
) -> None:
    scripts = package_data.get("scripts") if isinstance(package_data, dict) else None
    if not isinstance(scripts, dict):
        return
    local_paths: set[str] = set()
    for command in scripts.values():
        if not isinstance(command, str):
            continue
        patterns = [
            r"(?:^|[;&|]\s*|\s)(?:node|tsx|ts-node)\s+([\w./-]+\.(?:mjs|js|cjs|mts|ts))",
            r"(?:^|[;&|]\s*|\s)(\.?/?scripts/[\w./-]+\.(?:mjs|js|cjs|mts|ts))",
        ]
        for pattern in patterns:
            for match in re.finditer(pattern, command):
                local_paths.add(match.group(1).lstrip("./"))
    if local_paths and not re.search(
        r'''include\s*:\s*\[[^\]]*["']scripts/\*\*(?:/\*)?["']''',
        vitest_text,
        re.DOTALL,
    ):
        errors.append("Vitest coverage include must explicitly cover scripts/** used by package.json")
    if re.search(r"exclude\s*:\s*\[[^\]]*scripts", vitest_text, re.DOTALL):
        errors.append("Vitest coverage must not exclude local scripts referenced by package.json")
    for relative in sorted(local_paths):
        if not (project_dir / relative).is_file():
            errors.append(f"package.json references missing local script: {relative}")
        if relative not in test_text and Path(relative).name not in test_text:
            errors.append(f"local package script lacks explicit test coverage reference: {relative}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project_dir", type=Path, help="Path to generated project")
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    project_dir = args.project_dir.resolve()
    input_json_path = args.input_json.resolve()
    input_data = load_json(input_json_path)
    svg_logo_input, logo_name, input_logo_path = detect_svg_logo_input(input_data, input_json_path.parent)
    errors: list[str] = []

    expected_offer_keys = validate_input_contract(input_data, errors)

    if not project_dir.exists():
        print("ERROR: project directory does not exist", file=sys.stderr)
        return 2

    app_dir = find_app_dir(project_dir)
    if app_dir is None:
        errors.append("missing app directory (expected app/ or src/app/)")
    else:
        for rel in REQUIRED_ROUTES:
            if not (app_dir / rel).exists():
                errors.append(f"missing required route: app/{rel}")
        if (app_dir / "cart").exists():
            errors.append("forbidden cart route exists: app/cart")
        if (app_dir / "api").exists():
            errors.append("forbidden API route directory exists: app/api")

    if (project_dir / "pages" / "api").exists():
        errors.append("forbidden API route directory exists: pages/api")
    if (project_dir / "middleware.ts").exists() or (project_dir / "src" / "middleware.ts").exists():
        errors.append("middleware dependency is not allowed")

    for rel in REQUIRED_WORKFLOWS + REQUIRED_SCRIPTS + REQUIRED_PROJECT_FILES:
        check_exists(project_dir, rel, errors)

    for label, candidates in [
        ("vendor response helper", ["lib/vendor-response.ts", "src/lib/vendor-response.ts"]),
        ("offer pricing helper", ["lib/offer-pricing.ts", "src/lib/offer-pricing.ts"]),
        ("byte-bound legal content helper", ["lib/legal-content.ts", "src/lib/legal-content.ts"]),
        ("money helper", ["lib/utils.ts", "src/lib/utils.ts"]),
        ("visible home-link component", ["components/layout/home-link.tsx", "src/components/layout/home-link.tsx"]),
        ("testimonial load-more component", ["components/sections/testimonials.tsx", "src/components/sections/testimonials.tsx"]),
        ("canonical third-party scripts component", ["components/integrations/third-party-scripts.tsx", "src/components/integrations/third-party-scripts.tsx"]),
    ]:
        if find_config(project_dir, candidates) is None:
            errors.append(f"missing required {label}: expected one of {candidates}")

    vitest_text = ""
    vitest_path = find_config(project_dir, ["vitest.config.mts", "vitest.config.ts", "vitest.config.mjs", "vitest.config.js"])
    if vitest_path is None:
        errors.append("missing Vitest config")
    else:
        vitest_text = read_text(vitest_path)
        require_markers(
            vitest_text,
            ["coverage", "provider", "v8", "statements", "branches", "functions", "lines"],
            "Vitest config",
            errors,
        )
        for metric in ["statements", "branches", "functions", "lines"]:
            if not re.search(rf"{metric}\s*:\s*100\b", vitest_text):
                errors.append(f"Vitest coverage threshold for {metric} must be 100")
        if not re.search(r"perFile\s*:\s*true\b", vitest_text):
            errors.append("Vitest coverage thresholds must set perFile: true so every local script reaches 100%")

    tests_dir = project_dir / "tests"
    test_files = [] if not tests_dir.is_dir() else [
        path for path in tests_dir.rglob("*") if path.is_file() and re.search(r"\.(test|spec)\.(ts|tsx|js|jsx|mjs)$", path.name)
    ]
    if not test_files:
        errors.append("tests/ must contain unit/component/integration/E2E test files")
    e2e_files = [path for path in test_files if "e2e" in path.parts or ".spec." in path.name]
    if not e2e_files:
        errors.append("tests/ must contain Playwright E2E tests")

    config_path = find_config(project_dir, ["next.config.mjs", "next.config.js", "next.config.ts"])
    if config_path is None:
        errors.append("missing Next.js config file")
    else:
        config_text = read_text(config_path)
        if "output: 'export'" not in config_text and 'output: "export"' not in config_text:
            errors.append("next config does not enable static export")
        require_markers(config_text, ["allowedDevOrigins", "DEV_ALLOWED_ORIGINS", "networkInterfaces", "browserToTerminal"], "next config", errors)
        if re.search(r"allowedDevOrigins\s*:\s*\[\s*['\"]\*['\"]", config_text):
            errors.append("next config must not use wildcard allowedDevOrigins")

    validate_launch_json(project_dir, errors)

    playwright_path = project_dir / "playwright.config.ts"
    if playwright_path.exists():
        playwright_text = read_text(playwright_path)
        require_markers(playwright_text, ["webServer", "baseURL", "npm run serve:static"], "Playwright config", errors)
        if re.search(r"next\s+dev|npm\s+run\s+dev", playwright_text):
            errors.append("Playwright E2E must serve the built static out/ directory, not run next dev")

    package_json = project_dir / "package.json"
    package_data: dict[str, Any] = {}
    if package_json.exists():
        try:
            package_data = json.loads(read_text(package_json))
        except json.JSONDecodeError:
            errors.append("package.json is not valid JSON")
        if package_data:
            scripts = package_data.get("scripts", {})
            if not isinstance(scripts, dict):
                errors.append("package.json scripts must be an object")
                scripts = {}
            for script_name in REQUIRED_PACKAGE_SCRIPTS:
                if script_name not in scripts:
                    errors.append(f"package.json missing script: {script_name}")
            if "--max-warnings=0" not in str(scripts.get("lint", "")):
                errors.append("lint script must enforce zero warnings with --max-warnings=0")
            if "tsc" not in str(scripts.get("typecheck", "")) or "--noEmit" not in str(scripts.get("typecheck", "")):
                errors.append("typecheck script must run tsc --noEmit")
            if "vitest run" not in str(scripts.get("test", "")):
                errors.append("test script must run Vitest once, not watch mode")
            if "--coverage" not in str(scripts.get("test:coverage", "")):
                errors.append("test:coverage script must enable coverage")
            serve_script = str(scripts.get("serve:static", ""))
            if "serve out" not in serve_script or "4173" not in serve_script:
                errors.append("serve:static script must serve out/ on the fixed E2E port 4173")
            if "playwright test" not in str(scripts.get("test:e2e", "")):
                errors.append("test:e2e script must run Playwright")
            if "check-dev-runtime" not in str(scripts.get("test:runtime", "")):
                errors.append("test:runtime script must use scripts/check-dev-runtime.mjs")
            if "check-dependency-freshness" not in str(scripts.get("deps:check", "")):
                errors.append("deps:check script must use scripts/check-dependency-freshness.mjs")
            if "check-dependency-health" not in str(scripts.get("deps:health", "")):
                errors.append("deps:health script must use scripts/check-dependency-health.mjs")
            deps_scripts = str(scripts.get("deps:scripts", ""))
            if "approve-scripts" not in deps_scripts or "--allow-scripts-pending" not in deps_scripts or "--json" not in deps_scripts:
                errors.append("deps:scripts must inspect pending install scripts with npm approve-scripts --allow-scripts-pending --json")
            if "check-project-boundaries" not in str(scripts.get("project:boundaries", "")):
                errors.append("project:boundaries must use scripts/check-project-boundaries.mjs")
            if "check-customer-facing-copy" not in str(scripts.get("test:content", "")):
                errors.append("test:content must use scripts/check-customer-facing-copy.mjs")
            quality_script = str(scripts.get("quality", ""))
            for required_fragment in [
                "deps:check",
                "deps:health",
                "deps:scripts",
                "project:boundaries",
                "lint",
                "typecheck",
                "test:coverage",
                "build",
                "test:content",
                "test:e2e",
                "test:runtime",
            ]:
                if required_fragment not in quality_script:
                    errors.append(f"quality script missing gate: {required_fragment}")
            prepare_script = str(scripts.get("prepare:app", ""))
            for required_fragment in ["export-offer-env", "prepare-logo-assets", "generate-public-env"]:
                if required_fragment not in prepare_script:
                    errors.append(f"prepare:app script missing required step: {required_fragment}")
            build_script = str(scripts.get("build", ""))
            if "prepare:app" not in build_script or "next build" not in build_script:
                errors.append("build script must run prepare:app before next build")
            if "deploy-cloudflare-pages" not in str(scripts.get("deploy", "")):
                errors.append("deploy script must use scripts/deploy-cloudflare-pages.mjs")
            dev_script = str(scripts.get("dev", ""))
            if "prepare:app" not in dev_script or "next dev" not in dev_script or "--hostname 0.0.0.0" not in dev_script:
                errors.append("dev script must run prepare:app before next dev --hostname 0.0.0.0")
            validate_dependency_ranges(package_data, errors)
            lock_path = project_dir / "package-lock.json"
            try:
                lock_data = json.loads(read_text(lock_path)) if lock_path.exists() else {}
            except json.JSONDecodeError:
                lock_data = {}
                errors.append("package-lock.json is not valid JSON")
            validate_allow_scripts(package_data, lock_data, errors)
            engines = package_data.get("engines", {})
            if not isinstance(engines, dict) or not isinstance(engines.get("node"), str):
                errors.append("package.json must declare engines.node")

    npmrc_text = read_text(project_dir / ".npmrc")
    if not re.search(r"^\s*strict-allow-scripts\s*=\s*true\s*$", npmrc_text, re.MULTILINE | re.IGNORECASE):
        errors.append(".npmrc must set strict-allow-scripts=true")

    absolute_local_path_patterns = [
        re.compile(r"file:///+(?:[A-Za-z]:[\\/]|(?:home|Users|mnt|tmp|private|workspace|github/workspace|root|opt)/)", re.IGNORECASE),
        re.compile(r"(?<![A-Za-z0-9_])(?:[A-Za-z]:[\\/]|/(?:home|Users|mnt|tmp|private|workspace|github/workspace|root|opt)/)"),
    ]
    for file_path in project_dir.rglob("*"):
        if file_path.is_symlink():
            try:
                if not file_path.resolve().is_relative_to(project_dir):
                    errors.append(f"project symlink escapes project root: {file_path.relative_to(project_dir)}")
            except OSError:
                errors.append(f"project contains broken symlink: {file_path.relative_to(project_dir)}")
        if not file_path.is_file() or any(part in {"node_modules", ".next", "out", ".git", "coverage"} for part in file_path.parts):
            continue
        if file_path.suffix.lower() not in {".ts", ".tsx", ".js", ".jsx", ".mjs", ".mts", ".json", ".yml", ".yaml", ".md", ".css", ".html", ".txt", ".toml"}:
            continue
        file_text = read_text(file_path)
        if any(pattern.search(file_text) for pattern in absolute_local_path_patterns):
            errors.append(f"absolute local filesystem path found in project file: {file_path.relative_to(project_dir)}")

    if svg_logo_input:
        project_logo_path = project_dir / "source-assets" / "logo.svg"
        if not project_logo_path.is_file():
            errors.append("SVG logo source must be copied into the self-contained project at source-assets/logo.svg")
        elif input_logo_path is None:
            errors.append("input SVG logo source must be locally available for deterministic project copying")
        else:
            try:
                if project_logo_path.read_bytes() != input_logo_path.read_bytes():
                    errors.append("source-assets/logo.svg must be a byte-for-byte in-project copy of the validated input SVG")
            except OSError as exc:
                errors.append(f"could not compare in-project SVG logo copy: {exc}")
        site_input_path = project_dir / "config" / "site-input-summary.json"
        try:
            site_input = json.loads(read_text(site_input_path))
        except json.JSONDecodeError:
            site_input = {}
            errors.append("config/site-input-summary.json is not valid JSON")
        configured_logo = site_input.get("brand_system", {}).get("logo_file") if isinstance(site_input, dict) else None
        if configured_logo != "source-assets/logo.svg":
            errors.append("generated site config must reference the in-project SVG source as source-assets/logo.svg")

    all_files = collect_text_files(project_dir)
    all_text = collect_source_text(all_files)
    lower_text = all_text.lower()

    source_dirs = [path for path in [app_dir, find_source_dir(project_dir, "components"), find_source_dir(project_dir, "lib")] if path]
    source_files = [
        path
        for directory in source_dirs
        for path in directory.rglob("*")
        if path.is_file() and path.suffix.lower() in {".ts", ".tsx", ".js", ".jsx", ".css"}
    ]
    source_text = collect_source_text(source_files)

    if "buy now" not in lower_text:
        errors.append("no visible 'Buy Now' string found")
    if "carrier_delivery_estimate" not in all_text or "tracked" not in all_text:
        errors.append("shipping_details tracked/carrier_delivery_estimate are not obvious in generated config/source")
    for marker in ["data-testimonials", "data-testimonial", "Coupon applied to this order", "data-coupon-applied"]:
        if marker not in source_text:
            errors.append(f"customer-facing implementation missing required marker: {marker}")
    if re.search(r"(?<![A-Za-z0-9_-])text-(?:5xl|6xl|7xl|8xl|9xl)\b", source_text):
        errors.append("unprefixed text-5xl or larger class found; mobile headings must stay within the responsive type limits")
    if "clamp(" not in source_text and "clamp(" not in all_text:
        errors.append("responsive typography must use clamp() or an equivalent explicit fluid type implementation")
    if "text-wrap" not in source_text and "text-balance" not in source_text:
        errors.append("heading implementation must include balanced wrapping")

    for name, pattern in FORBIDDEN_PATTERNS.items():
        if pattern.search(source_text):
            errors.append(f"forbidden pattern found in application source: {name}")

    required_markers = [
        "/checkout?offer=",
        "StartCheckoutForm",
        "OfferSelect",
        "InitiatePurchase",
        "PurchaseSuccess",
        "PurchaseFailed",
        "redirect_status",
        "niobium.store.getQuote",
        "niobium.store.makeOrder",
        "niobium.notification.subscribe",
        "niobium.notification.contactUs",
        "niobium.store.trackOrder",
        "stripe.confirmPayment",
        "callVendorJson",
        "formatMoneyFromCents",
        "default_price",
        "amount_cents",
        "data-home-link",
    ]
    require_markers(all_text, required_markers, "project implementation", errors)
    removed_offer_event = "Bundle" + "Select"
    if removed_offer_event in all_text:
        errors.append("removed bundle-selection analytics event found; use OfferSelect")
    removed_endpoint_envs = (
        "SUBSCRIPTION" + "_INTEGRATION_ENDPOINT",
        "CONTACT_US" + "_INTEGRATION_ENDPOINT",
    )
    for removed_env in removed_endpoint_envs:
        if removed_env in all_text:
            errors.append(f"removed per-form environment variable found: {removed_env}")

    require_final_endpoint_argument(
        source_text,
        "niobium.store.getQuote",
        STORE_ENDPOINT_RE,
        "STORE_INTEGRATION_ENDPOINT",
        errors,
    )
    require_final_endpoint_argument(
        source_text,
        "niobium.store.makeOrder",
        STORE_ENDPOINT_RE,
        "STORE_INTEGRATION_ENDPOINT",
        errors,
    )
    require_final_endpoint_argument(
        source_text,
        "niobium.store.trackOrder",
        STORE_ENDPOINT_RE,
        "STORE_INTEGRATION_ENDPOINT",
        errors,
    )
    require_final_endpoint_argument(
        source_text,
        "niobium.notification.subscribe",
        NOTIFICATION_ENDPOINT_RE,
        "NOTIFICATION_INTEGRATION_ENDPOINT",
        errors,
    )
    require_final_endpoint_argument(
        source_text,
        "niobium.notification.contactUs",
        NOTIFICATION_ENDPOINT_RE,
        "NOTIFICATION_INTEGRATION_ENDPOINT",
        errors,
    )

    for callee in [
        "niobium.store.getQuote",
        "niobium.store.makeOrder",
        "niobium.store.trackOrder",
        "niobium.notification.subscribe",
        "niobium.notification.contactUs",
    ]:
        require_vendor_calls_wrapped(source_text, callee, errors)

    vendor_response_file = find_config(project_dir, ["lib/vendor-response.ts", "src/lib/vendor-response.ts"])
    vendor_response_text = read_text(vendor_response_file) if vendor_response_file else ""
    require_markers(
        vendor_response_text,
        [
            "Promise<Response>",
            "response.ok",
            "response.status",
            "response.json()",
            "callVendorJson",
            "VendorResponseError",
            "invalid_json",
            "invalid_body",
        ],
        "vendor Response helper",
        errors,
    )
    if vendor_response_text.count("response.json()") != 1:
        errors.append("vendor Response helper must consume response.json() exactly once in its parser")

    for key in expected_offer_keys:
        env_name = f"OFFER_OPTION__{key}"
        if env_name not in all_text:
            errors.append(f"expected offer option env variable not found in project source: {env_name}")
        if (
            f"offer={key}" not in all_text
            and f'offer_option_key: "{key}"' not in all_text
            and f'offer_option_key": "{key}"' not in all_text
        ):
            errors.append(f"offer option key {key} not obvious in checkout routing/source")

    for env_name in [
        "APP_NAME",
        "TENANT_ID",
        "GOOGLE_RECAPTCHA_SITE_KEY",
        "STORE_INTEGRATION_ENDPOINT",
        "NOTIFICATION_INTEGRATION_ENDPOINT",
        "STRIPE_PUBLIC_KEY",
        "SHIPPING_OPTION_ID",
        "TARGET_COUNTRY",
        "META_PIXEL_ID",
        "GOOGLE_TAG",
        "CLARITY_ID",
        "FACEBOOK_URL",
        "INSTAGRAM_URL",
        "CONTACT_EMAIL",
    ]:
        if env_name not in all_text:
            errors.append(f"environment variable not obvious in source/docs: {env_name}")

    env_file = find_config(project_dir, ["lib/env.ts", "src/lib/env.ts", "lib/public-env.ts", "src/lib/public-env.ts"])
    env_text = read_text(env_file) if env_file else ""
    require_markers(env_text, ["SHIPPING_OPTION_ID", "Number.isSafeInteger"], "environment integer parser", errors)
    if not re.search(r"\^\[1-9\].*\\d\*\$|\^\[1-9\]\[0-9\]\*\$", env_text):
        errors.append("environment parser must validate the full positive-integer SHIPPING_OPTION_ID string")

    vendor_boundary_files = [
        path
        for path in [
            project_dir / "lib" / "quote.ts",
            project_dir / "lib" / "order.ts",
            project_dir / "src" / "lib" / "quote.ts",
            project_dir / "src" / "lib" / "order.ts",
        ]
        if path.exists()
    ]
    vendor_boundary_text = collect_source_text(vendor_boundary_files)
    if re.search(r"shippingId\s*:\s*(?:process\.env\.)?SHIPPING_OPTION_ID\b", vendor_boundary_text):
        errors.append("vendor order payload passes raw SHIPPING_OPTION_ID string instead of parsed number")
    if "shippingId" not in vendor_boundary_text or not re.search(r"shipping_option_id|shippingId\s*:\s*[A-Za-z_$]", vendor_boundary_text):
        errors.append("numeric shippingId vendor boundary is not obvious")

    utils_file = find_config(project_dir, ["lib/utils.ts", "src/lib/utils.ts"])
    utils_text = read_text(utils_file) if utils_file else ""
    require_markers(
        utils_text,
        ["formatMoneyFromCents", "Number.isSafeInteger", "Intl.NumberFormat", "currency", "/ 100"],
        "cent-based money formatter",
        errors,
    )
    if not re.search(r"\[A-Z\]\{3\}|currency.*trim|normalizedCurrency", utils_text, re.IGNORECASE | re.DOTALL):
        errors.append("formatMoneyFromCents must validate currency before using currency style")
    if not re.search(r"amountCents\s*<\s*0|value\s*<\s*0", utils_text):
        errors.append("cent-based money formatter must reject negative cent amounts")
    if re.search(r"\bformatMoney\s*\(", source_text):
        errors.append("application source uses a major-unit formatMoney helper; use formatMoneyFromCents")

    offer_pricing_file = find_config(project_dir, ["lib/offer-pricing.ts", "src/lib/offer-pricing.ts"])
    offer_pricing_text = read_text(offer_pricing_file) if offer_pricing_file else ""
    require_markers(
        offer_pricing_text,
        [
            "default_price.amount_cents",
            "createImmediateOfferPrice",
            "refreshOfferPriceInBackground",
            'source: "default"',
            'source: "quote"',
            "quote.total",
        ],
        "landing default/background quote pricing helper",
        errors,
    )

    offer_config_text = read_text(project_dir / "config" / "offer-options.json")
    require_markers(
        offer_config_text,
        ["default_price", "amount_cents", "currency", "offer_option_key"],
        "offer options config",
        errors,
    )

    home_link_file = find_config(
        project_dir,
        ["components/layout/home-link.tsx", "src/components/layout/home-link.tsx"],
    )
    home_link_text = read_text(home_link_file) if home_link_file else ""
    require_markers(home_link_text, ['data-home-link="true"', 'href="/"', "Back to home"], "home link", errors)

    if "console.warn(" in source_text or "console.error(" in source_text:
        errors.append("application source contains console.warn/console.error; handled failures must remain user-facing and warning-free")

    if svg_logo_input:
        prepare_path = project_dir / "scripts" / "prepare-logo-assets.mjs"
        prepare_text = read_text(prepare_path)
        require_markers(
            prepare_text,
            ["sharp", "#000", "#fff", "transparent", "alpha", "logo-primary.png", "logo-inverse.png"],
            "SVG logo preparation script",
            errors,
        )
        if ".flatten(" in prepare_text or "removeAlpha" in prepare_text:
            errors.append("SVG logo preparation must not flatten or remove the PNG alpha channel")
        if not re.search(r"luminance|255\s*-|1\s*-.*lum|alpha.*coverage", prepare_text, re.IGNORECASE | re.DOTALL):
            errors.append("SVG logo preparation must convert white/antialiased pixels to transparency using luminance/coverage")
        if logo_name:
            for path in source_files:
                if logo_name in read_text(path):
                    errors.append(f"raw SVG logo is referenced by shopper-facing source: {path.relative_to(project_dir)}")
        site_logo_file = next(
            (
                path
                for path in [
                    project_dir / "components" / "brand" / "site-logo.tsx",
                    project_dir / "src" / "components" / "brand" / "site-logo.tsx",
                ]
                if path.exists()
            ),
            None,
        )
        if site_logo_file is not None and re.search(r"(?:src|href)\s*=.*\.svg|url\([^)]*\.svg", read_text(site_logo_file), re.IGNORECASE):
            errors.append("site-logo component appears to render the source SVG directly")
        if "logo-primary.png" not in source_text:
            errors.append("shopper-facing source must use generated logo-primary.png")

    test_text = collect_source_text(test_files)
    checkout_sources: list[Path] = []
    for candidate in [project_dir / "app" / "checkout", project_dir / "src" / "app" / "checkout", project_dir / "components" / "checkout", project_dir / "src" / "components" / "checkout"]:
        if candidate.is_dir():
            checkout_sources.extend(path for path in candidate.rglob("*") if path.is_file() and path.suffix.lower() in {".ts", ".tsx", ".js", ".jsx"})
    checkout_text = collect_source_text(checkout_sources)
    validate_testimonial_contract(project_dir, input_data, source_text, test_text, errors)
    validate_legal_policy_contract(project_dir, input_json_path, input_data, source_text, test_text, errors)
    validate_canonical_integration_contract(source_text, errors)
    validate_checkout_contract(checkout_text, test_text, errors)
    validate_local_script_coverage(project_dir, package_data, vitest_text, test_text, errors)
    for marker in ["page.on", "console", "pageerror", "requestfailed"]:
        if marker not in test_text:
            errors.append(f"Playwright tests missing browser error gate marker: {marker}")
    for route in ["/", "/checkout", "/contact", "/track-order", "/order-status", "/privacy-policy", "/terms", "/returns-policy", "/shipping-policy"]:
        if route not in test_text:
            errors.append(f"tests do not obviously cover required route: {route}")
    for marker in [
        "formatMoneyFromCents",
        "SHIPPING_OPTION_ID",
        "shippingId",
        "STORE_INTEGRATION_ENDPOINT",
        "NOTIFICATION_INTEGRATION_ENDPOINT",
        "OfferSelect",
        "logo-primary.png",
        "transparent",
        "response.ok",
        "response.status",
        "response.json",
        "VendorResponseError",
        "400",
        "429",
        "500",
        "amount_cents",
        "2495",
        "24.95",
        "refreshOfferPriceInBackground",
        "data-home-link",
        "pathname",
        "data-testimonials",
        "data-testimonial",
        "Coupon applied to this order",
        "320",
        "360",
        "390",
        "430",
        "horizontal overflow",
        "project boundary",
    ]:
        if marker not in test_text:
            errors.append(f"tests missing required regression/contract marker: {marker}")

    runtime_script = read_text(project_dir / "scripts" / "check-dev-runtime.mjs")
    require_markers(
        runtime_script,
        [
            "0.0.0.0",
            "networkInterfaces",
            "console",
            "pageerror",
            "requestfailed",
            "warning",
            "outdated",
            "cross-origin",
            "source-map",
            "new Response",
            "data-home-link",
            "waitForURL",
            "React DevTools",
            "HMR",
            "classifyBrowserConsoleMessage",
            "external-diagnostic",
            "--disable-extensions",
            "data-testimonials",
            "data-testimonials-total",
            "data-load-more-testimonials",
            "MOBILE_VIEWPORTS",
            "320",
            "360",
            "390",
            "430",
            "interactiveDefects",
            "EXPECTED_INFORMATIONAL_DEV_MESSAGES",
            "ObjectMultiplex",
            "MaxListenersExceededWarning",
            "private-token",
        ],
        "dev runtime check",
        errors,
    )

    freshness_script = read_text(project_dir / "scripts" / "check-dependency-freshness.mjs")
    require_markers(
        freshness_script,
        ["dist-tags.latest", "dependencies", "devDependencies", "npm", "latest", "CARET_STABLE_SEMVER", "latest stable compatible", "queryLatestStableWithinRange", "isCaretCompatible"],
        "dependency freshness check",
        errors,
    )

    health_script = read_text(project_dir / "scripts" / "check-dependency-health.mjs")
    require_markers(
        health_script,
        ["npm", "ci", "--dry-run", "--strict-allow-scripts", "approve-scripts", "--allow-scripts-pending", "--json", "allowScripts", "workerd", "engines", "warn"],
        "dependency resolution health check",
        errors,
    )
    if "npm ls" not in health_script and not re.search(r"[\"']ls[\"']", health_script):
        errors.append("dependency resolution health check must run npm ls --all")
    if "--legacy-peer-deps" in health_script or "--force" in health_script:
        errors.append("dependency health check must not use --legacy-peer-deps or --force")

    boundary_script = read_text(project_dir / "scripts" / "check-project-boundaries.mjs")
    require_markers(
        boundary_script,
        ["findAbsoluteLocalReferences", "findEscapingRelativeReferences", "isSymbolicLink", "file:", "link:", "Project boundary check failed"],
        "project boundary check",
        errors,
    )
    content_script = read_text(project_dir / "scripts" / "check-customer-facing-copy.mjs")
    require_markers(
        content_script,
        ["data-testimonials", "data-testimonial", "data-home-link", "Coupon applied to this order", "active coupon", "em dash"],
        "customer-facing built HTML check",
        errors,
    )

    deploy_public_leaks = []
    for path in all_files:
        if path.name == ".env.example" or ".github" in path.parts or "scripts" in path.parts or path.name == "README.md":
            continue
        file_text = read_text(path)
        if "CLOUDFLARE_API_TOKEN" in file_text or "CLOUDFLARE_ACCOUNT_ID" in file_text:
            deploy_public_leaks.append(str(path.relative_to(project_dir)))
    if deploy_public_leaks:
        errors.append("Cloudflare deploy-only variables appear in frontend/public source files: " + ", ".join(deploy_public_leaks))

    preserve_params = input_data.get("tracking_spec", {}).get("preserve_query_params", [])
    if preserve_params and not all(str(param) in all_text for param in preserve_params):
        errors.append("one or more preserved query params are not obvious in source")

    for route in ["/contact", "/track-order", "/privacy-policy", "/terms", "/returns-policy", "/shipping-policy"]:
        if route not in source_text:
            errors.append(f"footer/internal link not obvious for route: {route}")


    test_workflow_path = project_dir / ".github" / "workflows" / "test.yml"
    test_workflow_text = read_text(test_workflow_path)
    for marker in ["push:", "pull_request:", "branches-ignore:", "- main", "workflow_dispatch:", "environment: test"]:
        if marker not in test_workflow_text:
            errors.append(f"test workflow missing required trigger/environment marker: {marker}")
    if "feature/" in test_workflow_text or "startsWith(github.head_ref" in test_workflow_text:
        errors.append("test workflow must not be limited to feature branches or use a feature-branch condition")
    if test_workflow_text.count("branches-ignore:") < 2 or test_workflow_text.count("- main") < 2:
        errors.append("test workflow must ignore main independently for both push and pull_request")
    if re.search(r"^\s*if\s*:", test_workflow_text, re.MULTILINE):
        errors.append("test workflow must not add a branch condition that narrows the declared non-main triggers")

    workflow_text = collect_source_text([project_dir / rel for rel in REQUIRED_WORKFLOWS if (project_dir / rel).exists()])
    for marker in ["npm ci --strict-allow-scripts", "playwright install", "npm run quality", "npm run deploy"]:
        if marker not in workflow_text:
            errors.append(f"GitHub workflows missing required quality/deploy marker: {marker}")

    if errors:
        print("VALIDATION FAILED")
        for item in errors:
            print(f"ERROR: {item}")
        return 1

    print("VALIDATION PASSED")
    print("No structural warnings or errors detected.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
