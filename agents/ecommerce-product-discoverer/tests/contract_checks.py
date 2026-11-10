"""Offline reference checks only. No connectors, network, or LLM invocation."""
from __future__ import annotations

import calendar
import copy
import hashlib
import json
import math
import re
from collections import Counter
from datetime import date, datetime, timezone
from pathlib import Path
from typing import Any
from zoneinfo import ZoneInfo

from jsonschema import Draft202012Validator, FormatChecker
from referencing import Registry, Resource

ROOT = Path(__file__).resolve().parents[1]
SKILL = ROOT / "skills/ecommerce-cluster-ads"


def strict_loads(text: str) -> Any:
    def pairs(items: list[tuple[str, Any]]) -> dict[str, Any]:
        out: dict[str, Any] = {}
        for key, value in items:
            if key in out:
                raise ValueError(f"Duplicate JSON key: {key}")
            out[key] = value
        return out

    def bad_number(value: str) -> None:
        raise ValueError(f"Non-JSON number: {value}")

    def finite_float(value: str) -> float:
        number = float(value)
        if not math.isfinite(number):
            raise ValueError("Numeric value exceeds local representation limits")
        return number

    return json.loads(text, object_pairs_hook=pairs, parse_constant=bad_number, parse_float=finite_float)


def load(path: Path) -> Any:
    return strict_loads(path.read_text(encoding="utf-8"))


def validator(path: Path, fragment: str = "") -> Draft202012Validator:
    resources = [(p.as_uri(), Resource.from_contents(load(p))) for p in ROOT.rglob("*.schema.json")]
    return Draft202012Validator(
        {"$ref": path.resolve().as_uri() + fragment},
        registry=Registry().with_resources(resources), format_checker=FormatChecker(),
    )


def normalize_inputs(value: Any) -> dict[str, str]:
    validator(ROOT / "schemas/input.schema.json").validate(value)
    table = load(ROOT / "references/iso3166-1.json")["countries"]
    codes = {row[key]: row["alpha_2"] for row in table for key in ("alpha_2", "alpha_3")}
    result = copy.deepcopy(value)
    for field in ("source_country", "target_country"):
        code = value[field].upper()
        if code not in codes:
            raise ValueError(f"Unassigned country code: {field}")
        result[field] = codes[code]
    return result


def validate_config(value: Any) -> None:
    validator(ROOT / "schemas/config.schema.json").validate(value)
    ZoneInfo(value["timezone"])


def cutoff(run_date: date) -> date:
    year, month0 = divmod(run_date.year * 12 + run_date.month - 1 - 3, 12)
    month = month0 + 1
    return date(year, month, min(run_date.day, calendar.monthrange(year, month)[1]))


def eligible(ad: dict[str, Any], cutoff_date: date, timezone_name: str) -> bool:
    if ad.get("is_active") is not True:
        return False
    zone = ZoneInfo(timezone_name)
    values: list[date] = []
    try:
        if "start_date_string" in ad and ad["start_date_string"] not in (None, ""):
            value = ad["start_date_string"]
            if not isinstance(value, str):
                return False
            if re.fullmatch(r"\d{4}-\d{2}-\d{2}", value):
                values.append(date.fromisoformat(value))
            elif re.fullmatch(r"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})", value):
                parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
                values.append(parsed.astimezone(zone).date())
            else:
                return False
        value = ad.get("start_date")
        if value is not None:
            if type(value) is not int:
                return False
            if value != 0:
                if not 0 < value < 100_000_000_000:
                    return False
                values.append(datetime.fromtimestamp(value, timezone.utc).astimezone(zone).date())
    except (ValueError, OverflowError, OSError):
        return False
    return bool(values) and len(set(values)) == 1 and values[0] <= cutoff_date


def project_ads(ads: list[dict[str, Any]]) -> tuple[list[dict[str, Any]], list[str]]:
    """Remove only unknown keys; invalid known values must still fail schema."""
    schema = load(SKILL / "schemas/input.schema.json")
    omitted: list[str] = []

    def resolve(node: dict[str, Any]) -> dict[str, Any]:
        while "$ref" in node:
            current = schema
            for part in node["$ref"][2:].split("/"):
                current = current[part]
            node = current
        if "anyOf" in node:
            node = resolve(next((part for part in node["anyOf"] if part.get("type") != "null"), {}))
        return node

    def walk(value: Any, node: dict[str, Any], path: str) -> Any:
        node = resolve(node)
        if isinstance(value, dict) and "properties" in node:
            out = {}
            for key, item in value.items():
                child_path = f"{path}/{key}"
                if key not in node["properties"]:
                    omitted.append(child_path)
                else:
                    out[key] = walk(item, node["properties"][key], child_path)
            return out
        if isinstance(value, list) and "items" in node:
            return [walk(item, node["items"], f"{path}/{index}") for index, item in enumerate(value)]
        return copy.deepcopy(value)

    projected = walk(ads, schema, "")
    validator(SKILL / "schemas/input.schema.json").validate(projected)
    return projected, omitted


def deduplicate_ads(ads: list[dict[str, Any]]) -> tuple[dict[str, dict[str, Any]], set[str]]:
    originals: dict[str, dict[str, Any]] = {}
    conflicts: set[str] = set()
    for ad in ads:
        key = ad.get("ad_archive_id")
        if not isinstance(key, str) or not key.strip():
            raise ValueError("Nonblank string ad_archive_id required")
        if key in originals and json.dumps(ad, sort_keys=True) != json.dumps(originals[key], sort_keys=True):
            conflicts.add(key)
        else:
            originals.setdefault(key, copy.deepcopy(ad))
    return {key: ad for key, ad in originals.items() if key not in conflicts}, conflicts


def filter_clusters(clusters: Any, eligible_ids: set[str]) -> tuple[list[dict[str, Any]], list[str], bool]:
    if not isinstance(clusters, list):
        raise ValueError("Clustering failure or non-array response")
    check = validator(SKILL / "schemas/output.schema.json", "#/$defs/cluster")
    counts = Counter(item.get("cluster_id") for item in clusters if isinstance(item, dict) and isinstance(item.get("cluster_id"), str))
    intact, codes = [], []
    partial = False
    for item in clusters:
        ids = item.get("ad_archive_ids") if isinstance(item, dict) else None
        if not isinstance(ids, list) or not ids or any(not isinstance(key, str) or not key.strip() or key not in eligible_ids for key in ids) or len(set(ids)) != len(ids):
            codes.append("cluster_traceability_failed")
            partial = True
            continue
        if not check.is_valid(item):
            codes.append("cluster_schema_invalid")
            partial = True
            continue
        if counts[item["cluster_id"]] > 1:
            codes.append("duplicate_cluster_id")
            partial = True
            continue
        intact.append(item)
    covered = {key for item in intact for key in item["ad_archive_ids"]}
    if eligible_ids - covered:
        codes.append("cluster_coverage_missing")
        partial = True
    products = []
    for item in intact:
        if item["is_service"]:
            codes.append("service_cluster_excluded")
        else:
            products.append(copy.deepcopy(item))
            if item["cluster_confidence"] == "Low":
                codes.append("low_confidence_product")
    return sorted(products, key=lambda item: item["cluster_id"]), codes, partial


def validate_handoff(records: Any, inputs: dict[str, str], originals: dict[str, Any]) -> None:
    validator(ROOT / "schemas/handoff.schema.json").validate(records)
    normalized = normalize_inputs(inputs)
    cluster_ids = []
    for record in records:
        if record["inputs"] != inputs:
            raise ValueError("Original inputs changed")
        if record["job_id"] != records[0]["job_id"] or record["provenance"] != records[0]["provenance"]:
            raise ValueError("Mixed job provenance")
        provenance = record["provenance"]
        zone = ZoneInfo(provenance["timezone"])
        instant = datetime.fromisoformat(provenance["run_started_at"].replace("Z", "+00:00"))
        expected = cutoff(instant.astimezone(zone).date())
        if expected.isoformat() != provenance["cutoff_date"]:
            raise ValueError("Wrong cutoff")
        if provenance["successful_pages"] > provenance["search_attempts"]:
            raise ValueError("Impossible page counts")
        if any(provenance[field] != normalized[field] for field in ("source_country", "target_country")):
            raise ValueError("Wrong normalized countries")
        if record["product"]["ad_archive_ids"] != [ad["ad_archive_id"] for ad in record["ads"]]:
            raise ValueError("Ad membership or order changed")
        for ad in record["ads"]:
            if ad != originals.get(ad["ad_archive_id"]):
                raise ValueError("Original ad changed")
            if not eligible(ad, expected, provenance["timezone"]):
                raise ValueError("Ineligible handoff ad")
        cluster_ids.append(record["product"]["cluster_id"])
    if cluster_ids != sorted(set(cluster_ids)):
        raise ValueError("Duplicate or unsorted clusters")


def validate_result(result: Any) -> None:
    validator(ROOT / "schemas/result.schema.json").validate(result)
    if result["job_id"] is None and not (result["status"] == "failed" and any(e["code"] == "uuid_unavailable" for e in result["errors"])):
        raise ValueError("Null job ID is only allowed for UUID failure")
    if result["artifact"] and result["artifact"]["filename"] != result["job_id"] + ".json":
        raise ValueError("Filename must match job ID")


def git_hash(kind: str, data: bytes) -> str:
    return hashlib.sha1(kind.encode() + b" " + str(len(data)).encode() + b"\0" + data).hexdigest()


def tree_hash(directory: Path) -> str:
    parts = []
    for child in sorted(directory.iterdir(), key=lambda p: p.name + ("/" if p.is_dir() else "")):
        mode, digest = ("40000", tree_hash(child)) if child.is_dir() else ("100644", git_hash("blob", child.read_bytes()))
        parts.append(mode.encode() + b" " + child.name.encode() + b"\0" + bytes.fromhex(digest))
    return git_hash("tree", b"".join(parts))
