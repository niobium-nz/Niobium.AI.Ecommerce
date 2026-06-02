#!/usr/bin/env python3
"""Fast structural validator for direct-buy landing page bundles."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

REQUIRED_POLICY_ROUTES = [
    "privacy-policy/page.tsx",
    "terms/page.tsx",
    "returns-policy/page.tsx",
    "shipping-policy/page.tsx",
]

FORBIDDEN_PATTERNS = {
    "cart_language": re.compile(r"add\s+to\s+cart", re.IGNORECASE),
    "newsletter": re.compile(r"newsletter|waitlist", re.IGNORECASE),
    "countdown": re.compile(r"countdown|ends in|hours left|minutes left", re.IGNORECASE),
}


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


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


def collect_text_files(project_dir: Path) -> list[Path]:
    exts = {".ts", ".tsx", ".js", ".jsx", ".mjs", ".md", ".css"}
    return [
        path
        for path in project_dir.rglob("*")
        if path.is_file() and path.suffix.lower() in exts and "node_modules" not in path.parts
    ]


def check_exists(path: Path, rel: str, errors: list[str]) -> None:
    if not (path / rel).exists():
        errors.append(f"missing required file: {rel}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project_dir", type=Path, help="Path to generated project")
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    project_dir = args.project_dir.resolve()
    input_data = load_json(args.input_json.resolve())
    errors: list[str] = []
    warnings: list[str] = []

    if not project_dir.exists():
        print("ERROR: project directory does not exist", file=sys.stderr)
        return 2

    app_dir = find_app_dir(project_dir)
    if app_dir is None:
        errors.append("missing app directory (expected app/ or src/app/)")
    else:
        if not (app_dir / "page.tsx").exists():
            errors.append("missing landing page route: app/page.tsx")
        for rel in REQUIRED_POLICY_ROUTES:
            if not (app_dir / rel).exists():
                errors.append(f"missing policy route: {rel}")

    config_candidates = [project_dir / "next.config.mjs", project_dir / "next.config.js"]
    config_path = next((p for p in config_candidates if p.exists()), None)
    if config_path is None:
        errors.append("missing Next.js config file")
    else:
        config_text = read_text(config_path)
        if "output: 'export'" not in config_text and 'output: "export"' not in config_text:
            errors.append("next config does not enable static export")

    package_json = project_dir / "package.json"
    if not package_json.exists():
        errors.append("missing package.json")
    else:
        package_text = read_text(package_json)
        if "next" not in package_text:
            warnings.append("package.json does not clearly include next.js")

    all_text = "\n".join(read_text(path) for path in collect_text_files(project_dir))

    if "buy now" not in all_text.lower():
        warnings.append("no visible 'Buy Now' string found; check CTA wording")

    for name, pattern in FORBIDDEN_PATTERNS.items():
        if pattern.search(all_text):
            errors.append(f"forbidden pattern found: {name}")

    preserve_params = input_data.get("trackingSpec", {}).get("preserveQueryParams", [])
    if preserve_params:
        if not any(str(param) in all_text for param in preserve_params):
            warnings.append("preserved query params are not obvious in source; verify CTA and footer link handling")

    ids = {
        "ga4": input_data.get("trackingSpec", {}).get("ga4Id"),
        "meta": input_data.get("trackingSpec", {}).get("metaPixelId"),
        "clarity": input_data.get("trackingSpec", {}).get("microsoftClarity"),
    }
    for label, value in ids.items():
        if value and str(value) not in all_text and label not in all_text.lower():
            warnings.append(f"could not confirm {label} integration from source text")

    for route in ["/privacy-policy", "/terms", "/returns-policy", "/shipping-policy"]:
        if route not in all_text:
            warnings.append(f"footer link not obvious for route: {route}")

    if ":offer-short-name" not in input_data.get("checkoutUrl", ""):
        warnings.append("input checkout URL does not contain :offer-short-name token")
    else:
        if ":offer-short-name" in all_text:
            warnings.append("raw :offer-short-name token still appears in project; verify runtime replacement")

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
