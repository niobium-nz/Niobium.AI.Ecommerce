"""Offline contract/reference model. No network, LLM, host deployment or runtime requirement."""
from __future__ import annotations

from copy import deepcopy
import json
import math
from pathlib import Path
from typing import Any
from zoneinfo import ZoneInfo

from jsonschema import Draft202012Validator, FormatChecker
from resource_bindings import SchemaBindings

ROOT = Path(__file__).resolve().parents[1]
BINDINGS = SchemaBindings(ROOT)


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
    return BINDINGS.validator(path, fragment)


def check(value, definition):
    validator(fragment="#/$defs/" + definition).validate(value)


def skill_check(value, skill, direction):
    validator(f"urn:niobium:{skill}:schema:{direction}").validate(value)


def country(value):
    rows = load(BINDINGS.skill("ecommerce-normalize-products", "references/iso3166-1.json"))["countries"]
    codes = {row[key]: row["alpha_2"] for row in rows for key in ("alpha_2", "alpha_3")}
    return codes[value.upper()]


def validate_input(value):
    check(value, "input")
    ids = [ad["ad_archive_id"] for ad in value["ads"]]
    if len(ids) != len(set(ids)) or set(ids) != set(value["product"]["ad_archive_ids"]):
        raise ValueError("Duplicate or mismatched input IDs")


def validate_config(value):
    check(value, "claim_config_v1")
    ZoneInfo(value["timezone"])


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
    schema = load(BINDINGS.skill("ecommerce-score-products", "schemas/input.schema.json"))
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


def run_mock(value, normalized, responses, score, history=None, *, publish_ok=True,
             records_ok=True, finish_ok=True, claim_available=True, release_ok=True):
    """Deterministic expected-order model; not the real hosted agent or a skill implementation."""
    value = deepcopy(value)
    events, errors = [], []
    claims = dict(history or {})
    owned = ()
    phase = None
    published_artifact = None
    sid = None
    skip_registry = False
    product = value.get("product") if isinstance(value, dict) else None
    name = product.get("likely_product_name") if isinstance(product, dict) else None
    name = name if isinstance(name, str) and name.strip() else None
    status = "failed"
    stage = "input"
    try:
        events.append("validate_input")
        validate_input(value)
        skip_registry = value.get("override_duplicate_check", False)
        stage = "preflight"
        if not skip_registry:
            if not claim_available:
                raise ValueError("capability_unavailable")
            stage = "duplicate_check"
            events.append("claim_ids")
            ids = [ad["ad_archive_id"] for ad in value["ads"]]
            if any(claims.get(item) == "processed" for item in ids):
                raise ValueError("duplicate_found")
            if any(item in claims for item in ids):
                raise ValueError("duplicate_claim_held")
            owned = tuple(ids)
            phase = "held"
            claims.update(dict.fromkeys(owned, phase))
            events.append("verify_claim")
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
        sid = "15ae17b3-f461-4eba-8712-a10c7e691637"  # Fixed mock UUID, NEVER a runtime allocator.
        pending = artifact(value, reports, score, errors, sid)
        if not skip_registry:
            phase = "publishing"
            claims.update(dict.fromkeys(owned, phase))
            events.append("prepare_publication")
        events.append("publish")
        if not publish_ok:
            raise ValueError("publication_outcome_unknown")
        events.append("verify_publication")
        published_artifact = pending
        if skip_registry:
            status = "published_registry_skipped"
        else:
            stage = "registry"
            events.append("write_records")
            if not records_ok:
                raise ValueError("registry_update_failed")
            events.append("verify_records")
            if not finish_ok:
                raise ValueError("registry_outcome_unknown")
            claims.update(dict.fromkeys(owned, "processed"))
            events.append("verify_finalization")
            status = "partial" if errors else "succeeded"
    except Exception as exc:
        message = str(exc) if isinstance(exc, ValueError) else "Contract validation failed"
        codes = {"capability_unavailable", "duplicate_found", "duplicate_claim_held",
                 "publication_outcome_unknown", "registry_update_failed", "registry_outcome_unknown"}
        details = {"likely_product_name": name}
        if stage == "normalization" and isinstance(normalized, dict):
            section = normalized.get("normalization")
            if isinstance(section, dict):
                details["confidence0_to10"] = section.get("confidence0_to10")
        errors.append(diagnostic(message if message in codes else "stage_failed", stage, value, message, **details))
        status = "published_registry_pending" if published_artifact is not None else "failed"
    finally:
        if phase == "held":
            events.append("release_claim")
            if release_ok:
                for item in owned:
                    del claims[item]
            else:
                errors.append(diagnostic("duplicate_claim_outcome_unknown", "registry", value,
                                         "Prepublication claim release could not be verified"))
    published = published_artifact is not None
    filename = sid + ".json" if published else None
    result = {"likely_product_name": name, "signal_id": sid, "output_filename": filename,
              "output_file_id": "mock-file-id" if published else None,
              "output_path": f'discovery/{value["job"]["job_id"]}/{filename}' if published else None,
              "status": status, "published": published,
              "registry_updated": status in ("succeeded", "partial"), "errors": errors}
    check(result, "result")
    return result, published_artifact, claims, events
