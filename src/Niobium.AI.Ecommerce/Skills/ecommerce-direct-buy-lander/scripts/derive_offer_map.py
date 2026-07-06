#!/usr/bin/env python3
"""Validate and print ecommerce offer-option mappings.

The skill no longer derives alphabet-only offer short names or checkout URLs.
Generated projects use explicit `pricing_economics_and_offers.offer_options_mapping`
entries. Each visible offer maps to an `OFFER_OPTION__n` environment variable
whose value is the compact JSON serialization of `option_configuration`.
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any

SUPPORTED_COUNTRIES = {"US", "UK", "CA", "AU", "SG", "NZ", "IE"}
SHORT_PRODUCT_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
CART_ITEM_KEYS = {"listing", "option", "quantity"}


def strip_asset_suffix(value: str) -> str:
    return re.split(r"[?#]", value, maxsplit=1)[0]


def detect_svg_logo(data: dict[str, Any], input_json: Path | None = None) -> dict[str, Any]:
    brand = data.get("brand_system")
    logo_file = brand.get("logo_file") if isinstance(brand, dict) else None
    if not isinstance(logo_file, str) or not logo_file.strip():
        return {
            "path": logo_file if isinstance(logo_file, str) else None,
            "is_svg": False,
            "svg_colorization_required": False,
            "website_asset_format": "original",
        }

    logo_file = logo_file.strip()
    asset_path = strip_asset_suffix(logo_file)
    is_svg = asset_path.lower().endswith(".svg")

    if not is_svg and input_json is not None and not re.match(r"^[a-z][a-z0-9+.-]*://", asset_path, re.IGNORECASE):
        candidate = Path(asset_path)
        if not candidate.is_absolute():
            candidate = input_json.resolve().parent / candidate
        try:
            if candidate.is_file():
                head = candidate.read_text(encoding="utf-8", errors="ignore")[:512].lstrip()
                is_svg = head.startswith("<svg")
        except OSError:
            pass

    return {
        "path": logo_file,
        "is_svg": is_svg,
        "svg_colorization_required": is_svg,
        "website_asset_format": "png" if is_svg else "original",
        "expected_treatment": (
            "Assume monochrome black/white SVG; recolor the source from the input palette, size it for website placements, "
            "export optimized PNG assets for actual site use, and preserve viewBox/aspect ratio in the preprocessing step."
            if is_svg
            else "Render as a normal static asset with explicit dimensions; do not recolor."
        ),
    }


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        data = json.load(f)
    if not isinstance(data, dict):
        raise ValueError("input JSON root must be an object")
    return data


def require_short_product_name(data: dict[str, Any]) -> str:
    value = data.get("short_product_name")
    if not isinstance(value, str) or not value.strip():
        raise ValueError("missing required top-level short_product_name")
    value = value.strip()
    if not SHORT_PRODUCT_RE.fullmatch(value):
        raise ValueError("short_product_name must be lowercase letters/numbers/hyphens, with no leading or trailing hyphen")
    return value


def require_target_country(data: dict[str, Any]) -> str:
    value = data.get("target_country")
    if not isinstance(value, str) or not value.strip():
        raise ValueError("missing required top-level target_country")
    value = value.strip().upper()
    if value not in SUPPORTED_COUNTRIES:
        raise ValueError(f"target_country must be one of {sorted(SUPPORTED_COUNTRIES)}")
    return value


def normalize_offer_option_key(raw: Any, index: int) -> str:
    if isinstance(raw, int):
        if raw <= 0:
            raise ValueError(f"offer_options_mapping[{index}].offer_option_key must be a positive integer")
        return str(raw)
    if isinstance(raw, str):
        value = raw.strip()
        if not re.fullmatch(r"[1-9][0-9]*", value):
            raise ValueError(f"offer_options_mapping[{index}].offer_option_key must be a positive integer string")
        return value
    raise ValueError(f"offer_options_mapping[{index}].offer_option_key must be a positive integer or digit string")


def validate_cart_item(item: Any, mapping_index: int, item_index: int) -> dict[str, Any]:
    label = f"offer_options_mapping[{mapping_index}].option_configuration[{item_index}]"
    if not isinstance(item, dict):
        raise ValueError(f"{label} must be an object")
    keys = set(item.keys())
    if keys != CART_ITEM_KEYS:
        extra = sorted(keys - CART_ITEM_KEYS)
        missing = sorted(CART_ITEM_KEYS - keys)
        raise ValueError(f"{label} must contain only listing, option, quantity; extra={extra}, missing={missing}")
    listing = item.get("listing")
    quantity = item.get("quantity")
    option = item.get("option")
    if not isinstance(listing, int) or listing <= 0:
        raise ValueError(f"{label}.listing must be a positive integer")
    if not isinstance(quantity, int) or quantity <= 0:
        raise ValueError(f"{label}.quantity must be a positive integer")
    if not isinstance(option, str) or not option.strip():
        raise ValueError(f"{label}.option must be a non-empty string")
    return {"listing": listing, "option": option.strip(), "quantity": quantity}


def build_offer_option_map(data: dict[str, Any], input_json: Path | None = None) -> dict[str, Any]:
    short_product_name = require_short_product_name(data)
    target_country = require_target_country(data)

    pricing = data.get("pricing_economics_and_offers")
    if not isinstance(pricing, dict):
        raise ValueError("input JSON does not contain pricing_economics_and_offers")

    offer_stack = pricing.get("offer_stack")
    if not isinstance(offer_stack, dict) or not offer_stack:
        raise ValueError("input JSON does not contain pricing_economics_and_offers.offer_stack")

    mappings = pricing.get("offer_options_mapping")
    if not isinstance(mappings, list) or not mappings:
        raise ValueError("input JSON must contain non-empty pricing_economics_and_offers.offer_options_mapping")

    used_option_keys: set[str] = set()
    recommended_count = 0
    visible_offers: list[dict[str, Any]] = []

    for index, mapping in enumerate(mappings):
        if not isinstance(mapping, dict):
            raise ValueError(f"offer_options_mapping[{index}] must be an object")

        source_key = mapping.get("source_offer_key")
        if not isinstance(source_key, str) or not source_key.strip():
            raise ValueError(f"offer_options_mapping[{index}].source_offer_key is required")
        source_key = source_key.strip()
        if source_key not in offer_stack:
            raise ValueError(f"offer_options_mapping[{index}].source_offer_key '{source_key}' does not exist in offer_stack")

        option_key = normalize_offer_option_key(mapping.get("offer_option_key"), index)
        if option_key in used_option_keys:
            raise ValueError(f"duplicate offer_option_key '{option_key}' in offer_options_mapping")
        used_option_keys.add(option_key)

        option_configuration = mapping.get("option_configuration")
        if not isinstance(option_configuration, list) or not option_configuration:
            raise ValueError(f"offer_options_mapping[{index}].option_configuration must be a non-empty array")
        cart = [validate_cart_item(item, index, item_index) for item_index, item in enumerate(option_configuration)]

        recommended = mapping.get("recommended")
        if not isinstance(recommended, bool):
            raise ValueError(f"offer_options_mapping[{index}].recommended must be a boolean")
        if recommended:
            recommended_count += 1

        offer_value = offer_stack[source_key]
        if not isinstance(offer_value, dict):
            raise ValueError(f"offer_stack.{source_key} must be an object")

        env_var_name = f"OFFER_OPTION__{option_key}"
        env_var_value = json.dumps(cart, separators=(",", ":"), ensure_ascii=True)
        visible_offers.append(
            {
                "source_offer_key": source_key,
                "offer_option_key": option_key,
                "env_var_name": env_var_name,
                "env_var_value": env_var_value,
                "recommended": recommended,
                "name": offer_value.get("name", source_key),
                "description": offer_value.get("description"),
                "price_point_hint": offer_value.get("price_point"),
                "option_configuration": cart,
            }
        )

    if recommended_count != 1:
        raise ValueError(f"offer_options_mapping must contain exactly one recommended=true mapping; found {recommended_count}")

    return {
        "short_product_name": short_product_name,
        "target_country": target_country,
        "app_names": {
            "dev": f"niobiumecomm-{short_product_name}-dev",
            "test": f"niobiumecomm-{short_product_name}-test",
            "prod": f"niobiumecomm-{short_product_name}",
        },
        "brand_name": data.get("brand_system", {}).get("brand_name"),
        "logo": detect_svg_logo(data, input_json),
        "visible_offers": visible_offers,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    data = load_json(args.input_json)
    result = build_offer_option_map(data, args.input_json)
    print(json.dumps(result, indent=2, ensure_ascii=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
