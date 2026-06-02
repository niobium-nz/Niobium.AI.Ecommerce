#!/usr/bin/env python3
"""Derive stable alphabet-only offer short names and checkout URLs from input JSON."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any

DIGIT_WORDS = {
    "0": "zero",
    "1": "one",
    "2": "two",
    "3": "three",
    "4": "four",
    "5": "five",
    "6": "six",
    "7": "seven",
    "8": "eight",
    "9": "nine",
    "10": "ten",
}

PACK_NUMBER_WORDS = {
    "one": "onepack",
    "two": "twopack",
    "three": "threepack",
    "four": "fourpack",
    "five": "fivepack",
    "six": "sixpack",
    "seven": "sevenpack",
    "eight": "eightpack",
    "nine": "ninepack",
    "ten": "tenpack",
}

GENERIC_TOKENS = {
    "offer",
    "offers",
    "bundle",
    "bundles",
    "pack",
    "packs",
    "unit",
    "units",
    "glove",
    "gloves",
    "mitt",
    "mitts",
    "best",
    "seller",
    "bestseller",
}

COUNT_TOKENS = set(PACK_NUMBER_WORDS) | {"single"}


def load_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def camel_tokens(name: str) -> list[str]:
    spaced = re.sub(r"([a-z])([A-Z])", r"\1 \2", name)
    return [tok.lower() for tok in re.findall(r"[A-Za-z]+", spaced)]


def normalize_words(text: str) -> list[str]:
    working = text.lower()
    for digit, word in DIGIT_WORDS.items():
        working = re.sub(rf"(?<![a-z]){re.escape(digit)}(?![a-z])", f" {word} ", working)
    return [tok for tok in re.findall(r"[a-z]+", working) if tok]


def product_tokens(data: dict[str, Any]) -> set[str]:
    product_details = data.get("productDetails", {})
    names = []
    recommended = product_details.get("recommendedPrimaryProductName")
    if isinstance(recommended, str):
        names.append(recommended)
    suggested = product_details.get("suggestedProductNames", [])
    if isinstance(suggested, list):
        names.extend(name for name in suggested if isinstance(name, str))

    tokens: set[str] = set()
    for name in names:
        tokens.update(normalize_words(name))
    return tokens


def derive_short_name(offer_key: str, offer_name: str, blocked_tokens: set[str], used: set[str]) -> str:
    words = normalize_words(offer_name)
    key_words = camel_tokens(offer_key)

    descriptor_tokens = [
        tok
        for tok in words
        if tok not in blocked_tokens and tok not in GENERIC_TOKENS and tok not in COUNT_TOKENS
    ]

    if descriptor_tokens:
        candidate = "".join(descriptor_tokens[:2])
    else:
        if "single" in words or "single" in key_words:
            candidate = "single"
        else:
            count_token = next((tok for tok in words if tok in PACK_NUMBER_WORDS), None)
            if count_token:
                candidate = PACK_NUMBER_WORDS[count_token]
            else:
                fallback = [tok for tok in key_words if tok not in {"offer", "offers", "bundle", "bundles"}]
                candidate = "".join(fallback) or "offer"

    candidate = re.sub(r"[^a-z]", "", candidate)
    if not candidate:
        candidate = "offer"

    if candidate in used:
        count_token = next((tok for tok in words if tok in PACK_NUMBER_WORDS), None)
        suffix = PACK_NUMBER_WORDS[count_token] if count_token else "alt"
        merged = re.sub(r"[^a-z]", "", f"{candidate}{suffix}")
        candidate = merged if merged not in used else re.sub(r"[^a-z]", "", f"{candidate}{offer_key}")

    used.add(candidate)
    return candidate


def replace_offer_token(checkout_url: str, short_name: str) -> str:
    return checkout_url.replace(":offer-short-name", short_name)


def preferred_offer_key(data: dict[str, Any], offers: dict[str, Any]) -> str | None:
    target = str(data.get("pricingEconomicsAndOffers", {}).get("recommendedPrimaryOffer", "")).lower()
    if not target:
        return None
    for key, value in offers.items():
        name = str(value.get("name", "")).lower()
        price = str(value.get("pricePoint", "")).lower()
        combined = f"{name} {price}".strip()
        if target in combined or combined in target:
            return key
    for key, value in offers.items():
        name = str(value.get("name", "")).lower()
        if name and name in target:
            return key
    return None


def build_offer_map(data: dict[str, Any]) -> dict[str, Any]:
    offer_stack = data.get("pricingEconomicsAndOffers", {}).get("offerStack", {})
    if not isinstance(offer_stack, dict) or not offer_stack:
        raise ValueError("input JSON does not contain pricingEconomicsAndOffers.offerStack")

    blocked = product_tokens(data)
    checkout_url = str(data.get("checkoutUrl", "")).strip()
    used: set[str] = set()
    offers_out: list[dict[str, Any]] = []

    preferred_key = preferred_offer_key(data, offer_stack)

    for offer_key, offer_value in offer_stack.items():
        if not isinstance(offer_value, dict):
            continue
        name = str(offer_value.get("name", offer_key)).strip()
        short_name = derive_short_name(offer_key, name, blocked, used)
        offers_out.append(
            {
                "key": offer_key,
                "name": name,
                "price": offer_value.get("pricePoint"),
                "description": offer_value.get("description"),
                "short_name": short_name,
                "checkout_url": replace_offer_token(checkout_url, short_name) if checkout_url else None,
                "recommended": offer_key == preferred_key,
            }
        )

    return {
        "brand_name": data.get("brandSystem", {}).get("brandName"),
        "checkout_url_template": checkout_url,
        "recommended_offer_key": preferred_key,
        "offers": offers_out,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input_json", type=Path, help="Path to input JSON")
    args = parser.parse_args()

    data = load_json(args.input_json)
    result = build_offer_map(data)
    print(json.dumps(result, indent=2, ensure_ascii=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
