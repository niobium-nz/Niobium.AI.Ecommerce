#!/usr/bin/env python3
"""Validate and print ecommerce offer-option and asset/environment mappings.

Generated projects use explicit `pricing_economics_and_offers.offer_options_mapping`
entries. Input fields use lower snake case. Each visible offer maps to an
`OFFER_OPTION__n` environment variable whose value is the compact JSON
serialization of the vendor wire cart shape (`Listing`, `Option`, `Quantity`)
derived from lower-snake-case `option_configuration` items.

The script also validates the positive-integer shipping option contract, required
per-offer default prices expressed in integer cents, neutral shipping-details
metadata, and - when an SVG logo file is locally available - the black-foreground /
white-background source contract.
"""

from __future__ import annotations

import argparse
import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any

SUPPORTED_COUNTRIES = {"US", "UK", "CA", "AU", "SG", "NZ", "IE"}
SHORT_PRODUCT_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
INPUT_CART_ITEM_KEYS = {"listing", "option", "quantity"}
COLOR_PROPERTIES = {"fill", "stroke", "color", "stop-color", "flood-color", "lighting-color"}
BLACK_VALUES = {"#000", "#000000", "black", "rgb(0,0,0)", "rgb(0 0 0)"}
WHITE_VALUES = {"#fff", "#ffffff", "white", "rgb(255,255,255)", "rgb(255 255 255)"}
TRANSPARENT_VALUES = {"none", "transparent"}
FORBIDDEN_SVG_TAGS = {"script", "image", "lineargradient", "radialgradient", "pattern"}
MAX_SAFE_INTEGER = 9_007_199_254_740_991
CURRENCY_RE = re.compile(r"^[A-Z]{3}$")
FORBIDDEN_ORIGIN_WORDING_RE = re.compile(r"\boverseas?\b", re.IGNORECASE)
OBSOLETE_INPUT_FIELDS = {
    "checkout_url",
    "price_point",
    "subscription_integration_endpoint",
    "contact_us_integration_endpoint",
}


def strip_asset_suffix(value: str) -> str:
    return re.split(r"[?#]", value, maxsplit=1)[0]


def local_name(value: str) -> str:
    return value.rsplit("}", 1)[-1].lower()


def normalize_color(value: str) -> str:
    normalized = re.sub(r"\s+", " ", value.strip().lower())
    normalized = re.sub(r"\s*,\s*", ",", normalized)
    return normalized


def validate_svg_color(value: str, context: str) -> str:
    normalized = normalize_color(value)
    if not normalized or normalized in TRANSPARENT_VALUES:
        return "transparent"
    if "url(" in normalized:
        raise ValueError(f"SVG logo uses unsupported paint reference at {context}: {value!r}")
    if normalized in BLACK_VALUES:
        return "black"
    if normalized in WHITE_VALUES:
        return "white"
    raise ValueError(
        f"SVG logo uses unsupported visible color at {context}: {value!r}; "
        "only black foreground, white background, none, and transparent are allowed"
    )


def inspect_svg_source(path: Path) -> dict[str, Any]:
    raw = path.read_text(encoding="utf-8", errors="strict")
    lowered = raw.lower()
    if "<!doctype" in lowered or "<!entity" in lowered:
        raise ValueError("SVG logo must not contain a DOCTYPE or entity declaration")

    try:
        root = ET.fromstring(raw)
    except ET.ParseError as exc:
        raise ValueError(f"SVG logo is not valid XML: {exc}") from exc

    color_counts = {"black": 0, "white": 0, "transparent": 0}
    checked_colors: list[dict[str, str]] = []

    for element_index, element in enumerate(root.iter()):
        tag = local_name(element.tag)
        if tag in FORBIDDEN_SVG_TAGS:
            raise ValueError(f"SVG logo contains unsupported <{tag}> content")

        for attr_name, attr_value in element.attrib.items():
            attr = local_name(attr_name)
            value = str(attr_value).strip()
            if attr in {"href", "src"} and value and not (attr == "href" and value.startswith("#")):
                raise ValueError(f"SVG logo contains unsupported linked resource at {tag}[{attr}]")
            if attr in COLOR_PROPERTIES:
                kind = validate_svg_color(value, f"{tag}[{attr}]")
                color_counts[kind] += 1
                checked_colors.append({"context": f"{tag}[{attr}]", "value": value, "kind": kind})
            elif attr == "style":
                for declaration in value.split(";"):
                    if ":" not in declaration:
                        continue
                    property_name, property_value = declaration.split(":", 1)
                    property_name = property_name.strip().lower()
                    if property_name in COLOR_PROPERTIES:
                        kind = validate_svg_color(property_value, f"{tag}[style:{property_name}]")
                        color_counts[kind] += 1
                        checked_colors.append(
                            {
                                "context": f"{tag}[style:{property_name}]",
                                "value": property_value.strip(),
                                "kind": kind,
                            }
                        )

        if tag == "style" and element.text:
            css = element.text
            for match in re.finditer(
                r"(?P<property>fill|stroke|color|stop-color|flood-color|lighting-color)\s*:\s*(?P<value>[^;}]+)",
                css,
                re.IGNORECASE,
            ):
                property_name = match.group("property").lower()
                property_value = match.group("value").strip()
                kind = validate_svg_color(property_value, f"style[{property_name}]#{element_index}")
                color_counts[kind] += 1
                checked_colors.append(
                    {
                        "context": f"style[{property_name}]#{element_index}",
                        "value": property_value,
                        "kind": kind,
                    }
                )

    view_box = root.attrib.get("viewBox") or root.attrib.get("viewbox")
    return {
        "inspected": True,
        "source_file": path.name,
        "view_box": view_box,
        "color_counts": color_counts,
        "checked_colors": checked_colors,
        "source_contract_valid": True,
    }


def resolve_local_logo_path(logo_file: str, input_json: Path | None) -> Path | None:
    asset_path = strip_asset_suffix(logo_file)
    if re.match(r"^[a-z][a-z0-9+.-]*://", asset_path, re.IGNORECASE):
        return None
    candidate = Path(asset_path)
    if not candidate.is_absolute() and input_json is not None:
        candidate = input_json.resolve().parent / candidate
    return candidate if candidate.is_file() else None


def detect_svg_logo(data: dict[str, Any], input_json: Path | None = None) -> dict[str, Any]:
    brand = data.get("brand_system")
    logo_file = brand.get("logo_file") if isinstance(brand, dict) else None
    if not isinstance(logo_file, str) or not logo_file.strip():
        return {
            "path": logo_file if isinstance(logo_file, str) else None,
            "is_svg": False,
            "svg_colorization_required": False,
            "white_to_transparent_required": False,
            "website_asset_format": "original",
            "source_inspection": {"inspected": False, "reason": "logo path missing"},
        }

    logo_file = logo_file.strip()
    asset_path = strip_asset_suffix(logo_file)
    local_path = resolve_local_logo_path(logo_file, input_json)
    is_svg = asset_path.lower().endswith(".svg")

    if not is_svg and local_path is not None:
        try:
            head = local_path.read_text(encoding="utf-8", errors="ignore")[:512].lstrip()
            is_svg = head.startswith("<svg") or "<svg" in head[:128]
        except OSError:
            pass

    source_inspection: dict[str, Any]
    if is_svg and local_path is not None:
        source_inspection = inspect_svg_source(local_path)
    elif is_svg:
        raise ValueError(
            "SVG logo source must be a locally available file so its black/white colors can be validated "
            "and converted to transparent PNG assets; remote or missing SVG logo paths are not allowed"
        )
    else:
        source_inspection = {"inspected": False, "reason": "logo is not SVG"}

    source_name = local_path.name if local_path is not None else Path(asset_path).name
    project_source_path = "source-assets/logo.svg" if is_svg else f"public/assets/{source_name}"
    return {
        "input_source_name": source_name,
        "project_source_path": project_source_path,
        "is_svg": is_svg,
        "svg_colorization_required": is_svg,
        "white_to_transparent_required": is_svg,
        "source_foreground": "#000",
        "source_background": "#fff",
        "output_has_alpha": is_svg,
        "website_asset_format": "png" if is_svg else "original",
        "website_asset_paths": ["/assets/logo-primary.png", "/assets/logo-inverse.png"] if is_svg else [f"/assets/{source_name}"],
        "source_inspection": source_inspection,
        "expected_treatment": (
            "Copy the validated SVG into source-assets/logo.svg, map white to transparent alpha, map black to theme color, "
            "preserve antialiased edges as partial alpha, size for website placements, and export optimized transparent PNG assets."
            if is_svg
            else "Copy the supplied logo into public/assets and render it with explicit dimensions; do not reference its original external path."
        ),
    }


LOWER_SNAKE_KEY_RE = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")


def validate_lower_snake_keys(value: Any, path: str = "$") -> None:
    if isinstance(value, dict):
        for key, child in value.items():
            if not isinstance(key, str) or not LOWER_SNAKE_KEY_RE.fullmatch(key):
                raise ValueError(f"input JSON field must use lower snake case at {path}: {key!r}")
            if key in OBSOLETE_INPUT_FIELDS:
                raise ValueError(f"obsolete input field is not supported at {path}: {key}")
            validate_lower_snake_keys(child, f"{path}.{key}")
    elif isinstance(value, list):
        for index, child in enumerate(value):
            validate_lower_snake_keys(child, f"{path}[{index}]")


def require_shipping_details(data: dict[str, Any]) -> dict[str, Any]:
    product_details = data.get("product_details")
    if not isinstance(product_details, dict):
        raise ValueError("missing required top-level product_details object")
    shipping = product_details.get("shipping_details")
    if not isinstance(shipping, dict):
        raise ValueError("product_details.shipping_details must be an object")

    tracked = shipping.get("tracked")
    eta = shipping.get("carrier_delivery_estimate")
    tracking_message = shipping.get("tracking_message")
    if not isinstance(tracked, bool):
        raise ValueError("product_details.shipping_details.tracked must be a boolean")
    if not isinstance(eta, str) or not eta.strip():
        raise ValueError("product_details.shipping_details.carrier_delivery_estimate must be a non-empty string")
    if tracking_message is not None and (not isinstance(tracking_message, str) or not tracking_message.strip()):
        raise ValueError("product_details.shipping_details.tracking_message must be omitted or a non-empty string")
    for field_name, field_value in (
        ("carrier_delivery_estimate", eta),
        ("tracking_message", tracking_message),
    ):
        if isinstance(field_value, str) and FORBIDDEN_ORIGIN_WORDING_RE.search(field_value):
            raise ValueError(
                f"product_details.shipping_details.{field_name} must focus on tracking/ETA and must not emphasize fulfillment origin"
            )

    return {
        "tracked": tracked,
        "carrier_delivery_estimate": eta.strip(),
        "tracking_message": tracking_message.strip() if isinstance(tracking_message, str) else None,
    }


def require_default_price(offer: Any, offer_path: str) -> dict[str, Any]:
    if not isinstance(offer, dict):
        raise ValueError(f"{offer_path} must be an object")
    value = offer.get("default_price")
    if not isinstance(value, dict):
        raise ValueError(f"{offer_path}.default_price must be an object")
    amount_cents = value.get("amount_cents")
    currency = value.get("currency")
    if (
        isinstance(amount_cents, bool)
        or not isinstance(amount_cents, int)
        or amount_cents <= 0
        or amount_cents > MAX_SAFE_INTEGER
    ):
        raise ValueError(f"{offer_path}.default_price.amount_cents must be a positive safe integer in cents")
    if not isinstance(currency, str) or not CURRENCY_RE.fullmatch(currency):
        raise ValueError(f"{offer_path}.default_price.currency must be an uppercase three-letter currency code")
    return {"amount_cents": amount_cents, "currency": currency}



def require_testimonials(data: dict[str, Any]) -> list[dict[str, Any]]:
    trust = data.get("trust_signal")
    if not isinstance(trust, dict):
        raise ValueError("missing required top-level trust_signal object")
    for key in ("contact_email", "facebook_page", "instagram_page"):
        value = trust.get(key)
        if not isinstance(value, str) or not value.strip():
            raise ValueError(f"trust_signal.{key} must be a non-empty string")
    testimonials = trust.get("testimonials")
    if not isinstance(testimonials, list) or len(testimonials) < 3:
        raise ValueError("trust_signal.testimonials must contain at least three customer feedback entries")
    normalized: list[dict[str, Any]] = []
    for index, item in enumerate(testimonials):
        label = f"trust_signal.testimonials[{index}]"
        if not isinstance(item, dict):
            raise ValueError(f"{label} must be an object")
        name = item.get("name")
        testimonial = item.get("testimonial")
        if not isinstance(name, str) or not name.strip():
            raise ValueError(f"{label}.name must be a non-empty string")
        if not isinstance(testimonial, str) or not testimonial.strip():
            raise ValueError(f"{label}.testimonial must be a non-empty string")
        normalized.append(item)
    return normalized

def require_integration_endpoints(data: dict[str, Any]) -> tuple[str, str]:
    vendor = data.get("vendor_integration")
    if not isinstance(vendor, dict):
        raise ValueError("missing required top-level vendor_integration object")
    values: list[str] = []
    for key in ("store_integration_endpoint", "notification_integration_endpoint"):
        value = vendor.get(key)
        if not isinstance(value, str) or not value.strip():
            raise ValueError(f"vendor_integration.{key} must be a non-empty string")
        values.append(value.strip())
    return values[0], values[1]


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


def require_shipping_option_id(data: dict[str, Any]) -> int:
    vendor = data.get("vendor_integration")
    if not isinstance(vendor, dict):
        raise ValueError("missing required top-level vendor_integration object")
    value = vendor.get("shipping_option_id")
    if (
        isinstance(value, bool)
        or not isinstance(value, int)
        or value <= 0
        or value > MAX_SAFE_INTEGER
    ):
        raise ValueError(
            "vendor_integration.shipping_option_id must be a positive JSON integer within the JavaScript safe-integer range"
        )
    return value


def normalize_offer_option_key(raw: Any, index: int) -> str:
    if isinstance(raw, bool):
        raise ValueError(f"offer_options_mapping[{index}].offer_option_key must not be boolean")
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
    """Validate lower-snake-case input and return the vendor wire cart item."""
    label = f"offer_options_mapping[{mapping_index}].option_configuration[{item_index}]"
    if not isinstance(item, dict):
        raise ValueError(f"{label} must be an object")
    keys = set(item.keys())
    if keys != INPUT_CART_ITEM_KEYS:
        extra = sorted(keys - INPUT_CART_ITEM_KEYS)
        missing = sorted(INPUT_CART_ITEM_KEYS - keys)
        raise ValueError(
            f"{label} must contain only listing, option, quantity; extra={extra}, missing={missing}"
        )
    listing = item.get("listing")
    quantity = item.get("quantity")
    option = item.get("option")
    if isinstance(listing, bool) or not isinstance(listing, int) or listing <= 0:
        raise ValueError(f"{label}.listing must be a positive integer")
    if isinstance(quantity, bool) or not isinstance(quantity, int) or quantity <= 0:
        raise ValueError(f"{label}.quantity must be a positive integer")
    if not isinstance(option, str) or not option.strip():
        raise ValueError(f"{label}.option must be a non-empty string")
    return {"Listing": listing, "Option": option.strip(), "Quantity": quantity}


def build_offer_option_map(data: dict[str, Any], input_json: Path | None = None) -> dict[str, Any]:
    validate_lower_snake_keys(data)
    short_product_name = require_short_product_name(data)
    target_country = require_target_country(data)
    shipping_option_id = require_shipping_option_id(data)
    store_integration_endpoint, notification_integration_endpoint = require_integration_endpoints(data)
    shipping_details = require_shipping_details(data)
    testimonials = require_testimonials(data)

    pricing = data.get("pricing_economics_and_offers")
    if not isinstance(pricing, dict):
        raise ValueError("input JSON does not contain pricing_economics_and_offers")

    offer_stack = pricing.get("offer_stack")
    if not isinstance(offer_stack, dict) or not offer_stack:
        raise ValueError("input JSON does not contain pricing_economics_and_offers.offer_stack")

    default_prices: dict[str, dict[str, Any]] = {}
    for offer_key, offer_value in offer_stack.items():
        default_prices[offer_key] = require_default_price(
            offer_value,
            f"pricing_economics_and_offers.offer_stack.{offer_key}",
        )

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
                "default_price": default_prices[source_key],
                "option_configuration": cart,
            }
        )

    if recommended_count != 1:
        raise ValueError(f"offer_options_mapping must contain exactly one recommended=true mapping; found {recommended_count}")

    return {
        "short_product_name": short_product_name,
        "target_country": target_country,
        "shipping_details": shipping_details,
        "testimonial_count": len(testimonials),
        "shipping_option_id": shipping_option_id,
        "integration_endpoints": {
            "store": store_integration_endpoint,
            "notification": notification_integration_endpoint,
        },
        "shipping_option_environment": {
            "name": "SHIPPING_OPTION_ID",
            "value": str(shipping_option_id),
            "application_type": "number",
            "vendor_input_type": "number",
        },
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
