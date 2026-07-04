#!/usr/bin/env python3
"""Validate and print ecommerce offer-option mappings.

The skill no longer derives alphabet-only offer short names or checkout URLs.
Generated projects use explicit `pricingEconomicsAndOffers.offerOptionsMapping`
entries. Each visible offer maps to an `OFFER_OPTION__n` environment variable
whose value is the compact JSON serialization of `optionConfiguration`.
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any

SUPPORTED_COUNTRIES = {"US", "UK", "CA", "AU", "SG", "NZ", "IE"}
SHORT_PRODUCT_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
CART_ITEM_KEYS = {"Listing", "Option", "Quantity"}


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        data = json.load(f)
    if not isinstance(data, dict):
        raise ValueError("input JSON root must be an object")
    return data


def require_short_product_name(data: dict[str, Any]) -> str:
    value = data.get("shortProductName")
    if not isinstance(value, str) or not value.strip():
        raise ValueError("missing required top-level shortProductName")
    value = value.strip()
    if not SHORT_PRODUCT_RE.fullmatch(value):
        raise ValueError("shortProductName must be lowercase letters/numbers/hyphens, with no leading or trailing hyphen")
    return value


def require_target_country(data: dict[str, Any]) -> str:
    value = data.get("targetCountry")
    if not isinstance(value, str) or not value.strip():
        raise ValueError("missing required top-level targetCountry")
    value = value.strip().upper()
    if value not in SUPPORTED_COUNTRIES:
        raise ValueError(f"targetCountry must be one of {sorted(SUPPORTED_COUNTRIES)}")
    return value


def normalize_offer_option_key(raw: Any, index: int) -> str:
    if isinstance(raw, int):
        if raw <= 0:
            raise ValueError(f"offerOptionsMapping[{index}].offerOptionKey must be a positive integer")
        return str(raw)
    if isinstance(raw, str):
        value = raw.strip()
        if not re.fullmatch(r"[1-9][0-9]*", value):
            raise ValueError(f"offerOptionsMapping[{index}].offerOptionKey must be a positive integer string")
        return value
    raise ValueError(f"offerOptionsMapping[{index}].offerOptionKey must be a positive integer or digit string")


def validate_cart_item(item: Any, mapping_index: int, item_index: int) -> dict[str, Any]:
    label = f"offerOptionsMapping[{mapping_index}].optionConfiguration[{item_index}]"
    if not isinstance(item, dict):
        raise ValueError(f"{label} must be an object")
    keys = set(item.keys())
    if keys != CART_ITEM_KEYS:
        extra = sorted(keys - CART_ITEM_KEYS)
        missing = sorted(CART_ITEM_KEYS - keys)
        raise ValueError(f"{label} must contain only Listing, Option, Quantity; extra={extra}, missing={missing}")
    listing = item.get("Listing")
    quantity = item.get("Quantity")
    option = item.get("Option")
    if not isinstance(listing, int) or listing <= 0:
        raise ValueError(f"{label}.Listing must be a positive integer")
    if not isinstance(quantity, int) or quantity <= 0:
        raise ValueError(f"{label}.Quantity must be a positive integer")
    if not isinstance(option, str) or not option.strip():
        raise ValueError(f"{label}.Option must be a non-empty string")
    return {"Listing": listing, "Option": option.strip(), "Quantity": quantity}


def build_offer_option_map(data: dict[str, Any]) -> dict[str, Any]:
    short_product_name = require_short_product_name(data)
    target_country = require_target_country(data)

    pricing = data.get("pricingEconomicsAndOffers")
    if not isinstance(pricing, dict):
        raise ValueError("input JSON does not contain pricingEconomicsAndOffers")

    offer_stack = pricing.get("offerStack")
    if not isinstance(offer_stack, dict) or not offer_stack:
        raise ValueError("input JSON does not contain pricingEconomicsAndOffers.offerStack")

    mappings = pricing.get("offerOptionsMapping")
    if not isinstance(mappings, list) or not mappings:
        raise ValueError("input JSON must contain non-empty pricingEconomicsAndOffers.offerOptionsMapping")

    used_option_keys: set[str] = set()
    recommended_count = 0
    visible_offers: list[dict[str, Any]] = []

    for index, mapping in enumerate(mappings):
        if not isinstance(mapping, dict):
            raise ValueError(f"offerOptionsMapping[{index}] must be an object")

        source_key = mapping.get("sourceOfferKey")
        if not isinstance(source_key, str) or not source_key.strip():
            raise ValueError(f"offerOptionsMapping[{index}].sourceOfferKey is required")
        source_key = source_key.strip()
        if source_key not in offer_stack:
            raise ValueError(f"offerOptionsMapping[{index}].sourceOfferKey '{source_key}' does not exist in offerStack")

        option_key = normalize_offer_option_key(mapping.get("offerOptionKey"), index)
        if option_key in used_option_keys:
            raise ValueError(f"duplicate offerOptionKey '{option_key}' in offerOptionsMapping")
        used_option_keys.add(option_key)

        option_configuration = mapping.get("optionConfiguration")
        if not isinstance(option_configuration, list) or not option_configuration:
            raise ValueError(f"offerOptionsMapping[{index}].optionConfiguration must be a non-empty array")
        cart = [validate_cart_item(item, index, item_index) for item_index, item in enumerate(option_configuration)]

        recommended = mapping.get("recommended")
        if not isinstance(recommended, bool):
            raise ValueError(f"offerOptionsMapping[{index}].recommended must be a boolean")
        if recommended:
            recommended_count += 1

        offer_value = offer_stack[source_key]
        if not isinstance(offer_value, dict):
            raise ValueError(f"offerStack.{source_key} must be an object")

        env_var_name = f"OFFER_OPTION__{option_key}"
        env_var_value = json.dumps(cart, separators=(",", ":"), ensure_ascii=True)
        visible_offers.append(
            {
                "sourceOfferKey": source_key,
                "offerOptionKey": option_key,
                "envVarName": env_var_name,
                "envVarValue": env_var_value,
                "recommended": recommended,
                "name": offer_value.get("name", source_key),
                "description": offer_value.get("description"),
                "pricePointHint": offer_value.get("pricePoint"),
                "optionConfiguration": cart,
            }
        )

    if recommended_count != 1:
        raise ValueError(f"offerOptionsMapping must contain exactly one recommended=true mapping; found {recommended_count}")

    return {
        "shortProductName": short_product_name,
        "targetCountry": target_country,
        "appNames": {
            "dev": f"niobiumecomm-{short_product_name}-dev",
            "test": f"niobiumecomm-{short_product_name}-test",
            "prod": f"niobiumecomm-{short_product_name}",
        },
        "brandName": data.get("brandSystem", {}).get("brandName"),
        "visibleOffers": visible_offers,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    data = load_json(args.input_json)
    result = build_offer_option_map(data)
    print(json.dumps(result, indent=2, ensure_ascii=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
