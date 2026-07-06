#!/usr/bin/env python3
"""Fast structural validator for ecommerce direct-buy landing page bundles."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

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
]

FORBIDDEN_PATTERNS = {
    "add_to_cart_language": re.compile(r"add\s+to\s+cart", re.IGNORECASE),
    "waitlist": re.compile(r"\bwaitlist\b", re.IGNORECASE),
    "countdown": re.compile(r"countdown|ends in|hours left|minutes left", re.IGNORECASE),
    "server_action_directive": re.compile(r"['\"]use server['\"]"),
}

SUPPORTED_COUNTRIES = {"US", "UK", "CA", "AU", "SG", "NZ", "IE"}


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


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return ""




def strip_asset_suffix(value: str) -> str:
    return re.split(r"[?#]", value, maxsplit=1)[0]


def detect_svg_logo_input(input_data: dict[str, Any], input_json_dir: Path) -> bool:
    brand = input_data.get("brandSystem")
    logo_file = brand.get("logoFile") if isinstance(brand, dict) else None
    if not isinstance(logo_file, str) or not logo_file.strip():
        return False
    asset_path = strip_asset_suffix(logo_file.strip())
    if asset_path.lower().endswith(".svg"):
        return True
    if re.match(r"^[a-z][a-z0-9+.-]*://", asset_path, re.IGNORECASE):
        return False
    candidate = Path(asset_path)
    if not candidate.is_absolute():
        candidate = input_json_dir / candidate
    try:
        if candidate.is_file():
            return candidate.read_text(encoding="utf-8", errors="ignore")[:512].lstrip().startswith("<svg")
    except OSError:
        return False
    return False

def collect_text_files(project_dir: Path) -> list[Path]:
    exts = {".ts", ".tsx", ".js", ".jsx", ".mjs", ".md", ".css", ".json", ".yml", ".yaml", ".svg"}
    ignored_parts = {"node_modules", ".next", "out", ".git"}
    return [
        path
        for path in project_dir.rglob("*")
        if path.is_file() and path.suffix.lower() in exts and not any(part in ignored_parts for part in path.parts)
    ]


def check_exists(path: Path, rel: str, errors: list[str]) -> None:
    if not (path / rel).exists():
        errors.append(f"missing required file: {rel}")


def validate_input_contract(input_data: dict[str, Any], errors: list[str]) -> list[str]:
    expected_offer_keys: list[str] = []

    short_product = input_data.get("shortProductName")
    if not isinstance(short_product, str) or not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", short_product.strip()):
        errors.append("input shortProductName is missing or not a lowercase hyphen slug")

    target_country = input_data.get("targetCountry")
    if not isinstance(target_country, str) or target_country.upper() not in SUPPORTED_COUNTRIES:
        errors.append(f"input targetCountry must be one of {sorted(SUPPORTED_COUNTRIES)}")

    pricing = input_data.get("pricingEconomicsAndOffers", {})
    offer_stack = pricing.get("offerStack") if isinstance(pricing, dict) else None
    mappings = pricing.get("offerOptionsMapping") if isinstance(pricing, dict) else None
    if not isinstance(offer_stack, dict) or not offer_stack:
        errors.append("input missing pricingEconomicsAndOffers.offerStack")
    if not isinstance(mappings, list) or not mappings:
        errors.append("input missing pricingEconomicsAndOffers.offerOptionsMapping")
        return expected_offer_keys

    seen: set[str] = set()
    recommended_count = 0
    for index, mapping in enumerate(mappings):
        if not isinstance(mapping, dict):
            errors.append(f"offerOptionsMapping[{index}] must be an object")
            continue
        source = mapping.get("sourceOfferKey")
        if not isinstance(source, str) or not source:
            errors.append(f"offerOptionsMapping[{index}].sourceOfferKey missing")
        elif isinstance(offer_stack, dict) and source not in offer_stack:
            errors.append(f"offerOptionsMapping[{index}].sourceOfferKey not found in offerStack: {source}")
        raw_key = mapping.get("offerOptionKey")
        key = str(raw_key).strip() if isinstance(raw_key, (str, int)) else ""
        if not re.fullmatch(r"[1-9][0-9]*", key):
            errors.append(f"offerOptionsMapping[{index}].offerOptionKey must be positive integer/digit string")
        elif key in seen:
            errors.append(f"duplicate offerOptionKey in input: {key}")
        else:
            seen.add(key)
            expected_offer_keys.append(key)
        if mapping.get("recommended") is True:
            recommended_count += 1
        elif mapping.get("recommended") is not False:
            errors.append(f"offerOptionsMapping[{index}].recommended must be boolean")
        config = mapping.get("optionConfiguration")
        if not isinstance(config, list) or not config:
            errors.append(f"offerOptionsMapping[{index}].optionConfiguration must be non-empty array")
        else:
            for item_index, item in enumerate(config):
                if not isinstance(item, dict) or set(item.keys()) != {"Listing", "Option", "Quantity"}:
                    errors.append(f"offerOptionsMapping[{index}].optionConfiguration[{item_index}] must contain only Listing, Option, Quantity")
    if recommended_count != 1:
        errors.append(f"offerOptionsMapping must contain exactly one recommended=true mapping; found {recommended_count}")
    return expected_offer_keys


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project_dir", type=Path, help="Path to generated project")
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    project_dir = args.project_dir.resolve()
    input_json_path = args.input_json.resolve()
    input_data = load_json(input_json_path)
    svg_logo_input = detect_svg_logo_input(input_data, input_json_path.parent)
    errors: list[str] = []
    warnings: list[str] = []

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

    for rel in REQUIRED_WORKFLOWS + REQUIRED_SCRIPTS:
        check_exists(project_dir, rel, errors)

    for rel in ["README.md", ".env.example"]:
        check_exists(project_dir, rel, warnings)

    config_candidates = [project_dir / "next.config.mjs", project_dir / "next.config.js", project_dir / "next.config.ts"]
    config_path = next((p for p in config_candidates if p.exists()), None)
    if config_path is None:
        errors.append("missing Next.js config file")
    else:
        config_text = read_text(config_path)
        if "output: 'export'" not in config_text and 'output: "export"' not in config_text:
            errors.append("next config does not enable static export")

    package_json = project_dir / "package.json"
    package_data: dict[str, Any] = {}
    if not package_json.exists():
        errors.append("missing package.json")
    else:
        package_text = read_text(package_json)
        try:
            package_data = json.loads(package_text)
        except json.JSONDecodeError:
            errors.append("package.json is not valid JSON")
        scripts = package_data.get("scripts", {}) if isinstance(package_data, dict) else {}
        for script_name in ["lint", "build", "deploy"]:
            if script_name not in scripts:
                errors.append(f"package.json missing script: {script_name}")
        lint_script = str(scripts.get("lint", ""))
        if "--max-warnings=0" not in lint_script:
            errors.append("lint script must enforce zero warnings with --max-warnings=0 or equivalent")
        build_script = str(scripts.get("build", ""))
        if "next build" not in build_script:
            errors.append("build script must run next build")
        deploy_script = str(scripts.get("deploy", ""))
        if "deploy-cloudflare-pages" not in deploy_script:
            errors.append("deploy script must use scripts/deploy-cloudflare-pages.mjs")

    text_files = collect_text_files(project_dir)
    all_text = "\n".join(read_text(path) for path in text_files)
    lower_text = all_text.lower()


    if svg_logo_input:
        colorization_markers = [
            "currentColor",
            "mask-image",
            "WebkitMask",
            "--logo",
            "logoColor",
            "fill: var(",
            "stroke: var(",
            "primaryColor",
            "secondaryColor",
        ]
        png_export_markers = [
            "logo-primary.png",
            "logo-inverse.png",
            "prepare-logo-assets",
            "public/assets/logo",
            "resvg",
            "sharp",
            "png",
        ]
        sizing_markers = [
            "viewBox",
            "max-w",
            "maxWidth",
            "height:",
            "width:",
            "aspectRatio",
        ]
        if not any(marker in all_text for marker in colorization_markers):
            errors.append(
                "input SVG logo detected; project must recolor the assumed monochrome logo from the input palette before export"
            )
        if not any(marker in all_text for marker in png_export_markers):
            errors.append(
                "input SVG logo detected; project must export and use PNG logo assets rather than relying only on raw SVG rendering"
            )
        if not any(marker in all_text for marker in sizing_markers):
            warnings.append("input SVG logo detected; explicit logo sizing/aspect-ratio handling is not obvious")

    if "buy now" not in lower_text:
        warnings.append("no visible 'Buy Now' string found; check CTA wording")

    for name, pattern in FORBIDDEN_PATTERNS.items():
        if pattern.search(all_text):
            errors.append(f"forbidden pattern found: {name}")

    for required_snippet in [
        "/checkout?offer=",
        "StartCheckoutForm",
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
    ]:
        if required_snippet not in all_text:
            warnings.append(f"required implementation marker not obvious: {required_snippet}")

    for key in expected_offer_keys:
        env_name = f"OFFER_OPTION__{key}"
        if env_name not in all_text:
            errors.append(f"expected offer option env variable not found in project source: {env_name}")
        if f"offer={key}" not in all_text and f"offerOptionKey: \"{key}\"" not in all_text and f"offerOptionKey\": \"{key}\"" not in all_text:
            warnings.append(f"offer option key {key} not obvious in checkout routing/source")

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
            warnings.append(f"environment variable not obvious in source/docs: {env_name}")

    deploy_public_leaks = []
    for path in text_files:
        if path.name == ".env.example" or ".github" in path.parts or "scripts" in path.parts or path.name == "README.md":
            continue
        text = read_text(path)
        if "CLOUDFLARE_API_TOKEN" in text or "CLOUDFLARE_ACCOUNT_ID" in text:
            deploy_public_leaks.append(str(path.relative_to(project_dir)))
    if deploy_public_leaks:
        errors.append("Cloudflare deploy-only variables appear in frontend/public source files: " + ", ".join(deploy_public_leaks))

    preserve_params = input_data.get("trackingSpec", {}).get("preserveQueryParams", [])
    if preserve_params:
        if not any(str(param) in all_text for param in preserve_params):
            warnings.append("preserved query params are not obvious in source; verify CTA and footer link handling")

    for route in ["/contact", "/track-order", "/privacy-policy", "/terms", "/returns-policy", "/shipping-policy"]:
        if route not in all_text:
            warnings.append(f"footer/internal link not obvious for route: {route}")

    if errors:
        print("VALIDATION FAILED")
        for item in errors:
            print(f"ERROR: {item}")
        for item in warnings:
            print(f"WARN: {item}")
        return 1

    print("VALIDATION PASSED")
    for item in warnings:
        print(f"WARN: {item}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
