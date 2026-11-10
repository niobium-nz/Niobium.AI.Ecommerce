"""Offline contract/reference model. No network, LLM, host deployment or runtime requirement."""
from __future__ import annotations

from copy import deepcopy
import json
import math
from pathlib import Path
from typing import Any
from zoneinfo import ZoneInfo

from jsonschema import Draft202012Validator, FormatChecker
from referencing import Registry, Resource

ROOT = Path(__file__).resolve().parents[1]


def strict_loads(text: str) -> Any:
    def pairs(items):
        result = {}
        for key, value in items:
            if key in result:
                raise ValueError("Duplicate object key")
            result[key] = value
        return result
    def bad(value):
        raise ValueError("Non-JSON number: " + value)
    def finite(value):
        number = float(value)
        if not math.isfinite(number):
            raise ValueError("Number exceeds local representation")
        return number
    return json.loads(text, object_pairs_hook=pairs, parse_constant=bad, parse_float=finite)


def load(path: Path) -> Any:
    return strict_loads(path.read_text(encoding="utf-8"))


def validator(path: str = "schemas/contracts.schema.json", fragment: str = ""):
    resources = [(p.as_uri(), Resource.from_contents(load(p))) for p in ROOT.rglob("*.schema.json")]
    return Draft202012Validator({"$ref": (ROOT / path).as_uri() + fragment},
                               registry=Registry().with_resources(resources),
                               format_checker=FormatChecker())


def check(value, definition):
    validator(fragment="#/$defs/" + definition).validate(value)


def skill_check(value, skill, direction):
    validator(f"skills/{skill}/schemas/{direction}.schema.json").validate(value)


def country(value):
    rows = load(ROOT / "skills/ecommerce-normalize-products/references/iso3166-1.json")["countries"]
    codes = {row[key]: row["alpha_2"] for row in rows for key in ("alpha_2", "alpha_3")}
    return codes[value.upper()]


def validate_input(value):
    check(value, "input")
    ids = [ad["ad_archive_id"] for ad in value["ads"]]
    if len(ids) != len(set(ids)) or set(ids) != set(value["product"]["ad_archive_ids"]):
        raise ValueError("Duplicate or mismatched input IDs")


def validate_config(value):
    check(value, "config")
    ZoneInfo(value["timezone"])


def parse_registry(text, format_name):
    if format_name == "json_string_array":
        ids = strict_loads(text)
        if not isinstance(ids, list):
            raise ValueError("Expected JSON string array")
    elif format_name == "newline_ids":
        ids = [line[:-1] if line.endswith("\r") else line for line in text.split("\n")]
        ids = [line for line in ids if line.strip()]
        if any("\r" in line for line in ids):
            raise ValueError("Invalid newline registry record")
    else:
        raise ValueError("Unsupported registry format")
    if any(not isinstance(value, str) or not value.strip() for value in ids):
        raise ValueError("Registry IDs must be nonblank strings")
    return ids


def append_registry(text, format_name, ids):
    existing = parse_registry(text, format_name)
    if any(not isinstance(value, str) or not value.strip() for value in ids):
        raise ValueError("Invalid appended ID")
    missing = list(dict.fromkeys(value for value in ids if value not in set(existing)))
    if not missing:
        return text
    if format_name == "json_string_array":
        return json.dumps(existing + missing, ensure_ascii=False) + "\n"
    if any("\r" in value or "\n" in value for value in missing):
        raise ValueError("ID cannot be encoded as one line")
    newline = "\r\n" if "\r\n" in text else "\n"
    prefix = text + (newline if text and not text.endswith("\n") else "")
    return prefix + newline.join(missing) + newline


def normalization_input(value):
    p, j = value["product"], value["job"]
    return {"product_name": p["likely_product_name"], "category_name": p["category_guess"],
            "known_features": deepcopy(p["known_features"]), "country": j["source_country"]}


def validate_normalization(result, value):
    skill_check(result, "ecommerce-normalize-products", "output")
    if "error" in result:
        raise ValueError(result["error"])
    if result["normalization"]["confidence0_to10"] < 6:
        raise ValueError("Normalization confidence below 6")
    if not result["keyword_plan"]["recommended_mcp_queries"]:
        raise ValueError("Empty query plan")
    expected = normalization_input(value)
    for out, key in [("product_name_given", "product_name"),
                     ("category_name_provided", "category_name"),
                     ("known_features_provided", "known_features")]:
        if result[out] != expected[key]:
            raise ValueError("Normalization echo mismatch")


def competition_inputs(normalized, value):
    return [{"query": q, "country": value["job"]["target_country"],
             "category_name": value["product"]["category_guess"],
             "notes": deepcopy(normalized["notes_for_downstream_agent"]),
             "avoid_or_exclusion_terms": deepcopy(normalized["keyword_plan"]["avoid_or_exclusion_terms"]),
             "product_interpretations": deepcopy(normalized["product_interpretations"])}
            for q in normalized["keyword_plan"]["recommended_mcp_queries"]]


def diagnostic(code, stage, value, message, **details):
    product = value.get("product", {}) if isinstance(value, dict) else {}
    cluster = product.get("cluster_id") if isinstance(product, dict) else None
    cluster = cluster if isinstance(cluster, str) and cluster.strip() else None
    return {"code": code, "stage": stage, "product_cluster_id": cluster,
            "message": str(message), "details": details}


def filter_reports(tasks, responses, value):
    valid, errors = [], []
    # responses are indexed mock completions, deliberately allowed to arrive out of order.
    for index, task in enumerate(tasks):
        matches = [r for i, r in responses if type(i) is int and i == index]
        try:
            if len(matches) != 1:
                raise ValueError("Missing or duplicate worker response")
            report = matches[0]
            skill_check(report, "ecommerce-analyze-competition", "output")
            if "error" in report:
                raise ValueError(report["error"])
            if report["query"] != task["query"] or report["country"] != task["country"]:
                raise ValueError("Query or country echo mismatch")
            valid.append(deepcopy(report))
        except Exception as exc:
            # Mock diagnostics use a generic schema failure message, not raw validator payloads.
            message = str(exc) if isinstance(exc, ValueError) else "Invalid competition response"
            errors.append(diagnostic("competition_response_rejected", "competition", value,
                                     message, query_index=index, query=task["query"]))
    for index, _ in responses:
        if type(index) is not int or not 0 <= index < len(tasks):
            errors.append(diagnostic("competition_response_unexpected", "competition", value,
                                     "Unexpected response index", query_index=str(index)))
    return valid, errors


def project_ads(ads):
    schema = load(ROOT / "skills/ecommerce-score-products/schemas/input.schema.json")
    omitted = []
    def resolve(node):
        if "$ref" in node:
            return resolve(schema["$defs"][node["$ref"].split("/")[-1]])
        if "anyOf" in node:
            return resolve(next(x for x in node["anyOf"] if x.get("type") != "null"))
        return node
    def walk(value, node, path):
        node = resolve(node)
        if isinstance(value, dict) and "properties" in node:
            result = {}
            for key, item in value.items():
                if key in node["properties"]:
                    result[key] = walk(item, node["properties"][key], path + "/" + key)
                else:
                    omitted.append(path + "/" + key)
            return result
        if isinstance(value, list) and "items" in node:
            return [walk(item, node["items"], path + "/" + str(i)) for i, item in enumerate(value)]
        return deepcopy(value)
    return walk(ads, schema["properties"]["ads"], "/ads"), omitted


def scoring_input(value, valid_reports):
    ads, _ = project_ads(value["ads"])
    result = {"target_country": value["job"]["target_country"], "product": deepcopy(value["product"]),
              "ads": ads, "competition_signals": deepcopy(valid_reports)}
    skill_check(result, "ecommerce-score-products", "input")
    return result


def validate_score(score, value):
    skill_check(score, "ecommerce-score-products", "output")
    if "final_score" not in score:
        raise ValueError("No score")
    if score["target_country"] != country(value["job"]["target_country"]):
        raise ValueError("Wrong score market")
    if "error" in score and not any(c.startswith("THIN_EVIDENCE_60:") for c in score["caps_applied"]):
        raise ValueError("Partial research requires its documented evidence cap")
    # Arithmetic assertion only. Real evidence/risk/cap selection remains in the skill.
    ceilings = [sum(score["subscores"].values())]
    for prefix, maximum in [("PROHIBITED_OR_RESTRICTED_15:", 15),
                            ("UNRESOLVED_COMPLIANCE_35:", 35), ("THIN_EVIDENCE_60:", 60)]:
        if any(item.startswith(prefix) for item in score["caps_applied"]):
            ceilings.append(maximum)
    if score["final_score"] != min(ceilings):
        raise ValueError("Score arithmetic mismatch")


def artifact(value, reports, score, errors, signal_id):
    result = {**deepcopy(value["job"]), "signal_id": signal_id,
              "product": deepcopy(value["product"]), "ads": deepcopy(value["ads"]),
              "competition_signals": deepcopy(reports), "score": deepcopy(score), "errors": deepcopy(errors)}
    check(result, "artifact")
    return result


def run_mock(value, normalized, responses, score, registry='[]', *, publish_ok=True,
             registry_ok=True, late_registry=None):
    """Deterministic expected-order model; not the real hosted agent or a skill implementation."""
    value = deepcopy(value)
    events, errors = [], []
    published_artifact = None
    sid = None
    product = value.get("product") if isinstance(value, dict) else None
    name = product.get("likely_product_name") if isinstance(product, dict) else None
    name = name if isinstance(name, str) and name.strip() else None
    status = "failed"
    stage = "input"
    try:
        events.append("validate_input")
        validate_input(value)
        stage = "duplicate_check"
        events.extend(["acquire_guard", "read_registry"])
        ids = [ad["ad_archive_id"] for ad in value["ads"]]
        if set(ids) & set(parse_registry(registry, "json_string_array")):
            raise ValueError("duplicate_found")
        stage = "normalization"
        events.append("normalize")
        skill_check(normalization_input(value), "ecommerce-normalize-products", "input")
        validate_normalization(normalized, value)
        stage = "competition"
        tasks = competition_inputs(normalized, value)
        for task in tasks:
            skill_check(task, "ecommerce-analyze-competition", "input")
            events.append("competition:" + task["query"])
        reports, query_errors = filter_reports(tasks, responses, value)
        errors.extend(query_errors)
        if not reports:
            raise ValueError("all_competition_queries_failed")
        stage = "scoring"
        scoring_input(value, reports)
        events.append("score")
        validate_score(score, value)
        if "error" in score:
            errors.append(diagnostic("score_partial_failure", stage, value, score["error"]))
        stage = "publication"
        events.append("recheck_registry")
        current = registry if late_registry is None else late_registry
        if set(ids) & set(parse_registry(current, "json_string_array")):
            raise ValueError("duplicate_found")
        sid = "15ae17b3-f461-4eba-8712-a10c7e691637"  # Fixed mock UUID, NEVER a runtime allocator.
        pending = artifact(value, reports, score, errors, sid)
        events.append("publish")
        if not publish_ok:
            raise ValueError("publication_outcome_unknown")
        events.append("verify_publication")
        published_artifact = pending
        stage = "registry"
        events.extend(["reread_registry", "append_registry"])
        if not registry_ok:
            raise ValueError("registry_update_failed")
        registry = append_registry(current, "json_string_array", ids)
        events.append("verify_registry")
        status = "partial" if errors else "succeeded"
    except Exception as exc:
        message = str(exc) if isinstance(exc, ValueError) else "Contract validation failed"
        details = {"likely_product_name": name}
        if stage == "normalization" and isinstance(normalized, dict):
            section = normalized.get("normalization")
            if isinstance(section, dict):
                details["confidence0_to10"] = section.get("confidence0_to10")
        errors.append(diagnostic("stage_failed", stage, value, message, **details))
        status = "published_registry_pending" if published_artifact is not None else "failed"
    finally:
        if "acquire_guard" in events:
            events.append("release_guard")
    published = published_artifact is not None
    filename = sid + ".json" if published else None
    result = {"likely_product_name": name, "signal_id": sid, "output_filename": filename,
              "output_file_id": "mock-file-id" if published else None,
              "output_path": f'discovery/{value["job"]["job_id"]}/{filename}' if published else None,
              "status": status, "published": published,
              "registry_updated": status in ("succeeded", "partial"), "errors": errors}
    check(result, "result")
    return result, published_artifact, registry, events
