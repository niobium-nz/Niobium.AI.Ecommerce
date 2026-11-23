"""Deterministic council synthesis, sourcing handoff and human approval reference.

No external calls, supplier fabrication or automatic onboarding.
"""
from __future__ import annotations
from dataclasses import dataclass, asdict
from datetime import datetime, timezone
from hashlib import sha256
import json
from typing import Any

WEIGHTS = {"commercial": 30, "market": 25, "competitive": 20, "risk": 15, "operations": 10}
SCORING_VERSION = "council-v1"


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def council(batch_id: str, candidates: list[dict[str, Any]], weights: dict[str, int] | None = None) -> dict[str, Any]:
    """Synthesize independent specialist assessments; select exactly one product."""
    weights = dict(WEIGHTS if weights is None else weights)
    _require(bool(batch_id and batch_id.strip()), "batch_id required")
    _require(set(weights) == set(WEIGHTS) and sum(weights.values()) == 100 and all(type(v) is int and v >= 0 for v in weights.values()), "invalid weights")
    _require(isinstance(candidates, list) and bool(candidates), "nonempty candidate batch required")
    ids: set[str] = set()
    ranked = []
    for candidate in candidates:
        pid = candidate.get("product_id")
        _require(isinstance(pid, str) and bool(pid.strip()) and pid not in ids, "missing or duplicate product_id")
        ids.add(pid)
        assessments = candidate.get("specialist_assessments")
        _require(isinstance(assessments, dict) and set(assessments) == set(weights), "all five specialist assessments required")
        for dimension, assessment in assessments.items():
            _require(isinstance(assessment, dict), "assessment must be object")
            score = assessment.get("score")
            _require(type(score) in (int, float) and 0 <= score <= 100, "score must be 0..100")
            for field in ("pros", "cons", "risks", "uncertainties", "evidence_references", "supplier_validation_questions", "recommended_mitigations"):
                _require(isinstance(assessment.get(field), list), f"{dimension}.{field} must be list")
        score = sum(weights[k] * assessments[k]["score"] for k in weights) / 100
        ranked.append({"product_id": pid, "weighted_score": round(score, 4),
                       "specialist_assessments": assessments,
                       "commercial_assumptions_to_verify": candidate.get("commercial_assumptions_to_verify", []),
                       "dissenting_opinions": candidate.get("dissenting_opinions", [])})
    ranked.sort(key=lambda x: (-x["weighted_score"], x["product_id"]))
    for index, candidate in enumerate(ranked, 1):
        candidate["rank"] = index
    winner = ranked[0]
    return {"research_batch_id": batch_id, "scoring_version": SCORING_VERSION,
            "weights": weights, "selected_product_id": winner["product_id"],
            "selection_rank": 1, "selection_reason": "Highest relative weighted score; ties broken by product ID. Not onboarding approval.",
            "candidate_rankings": ranked, "approved_for_onboarding": None}


def sourcing_handoff(result: dict[str, Any], supplier_findings: list[dict[str, Any]]) -> dict[str, Any]:
    """Preserve complete council dossier and supplier evidence; never change selection."""
    selected = result["selected_product_id"]
    _require(any(x["product_id"] == selected for x in result["candidate_rankings"]), "selected product absent")
    _require(isinstance(supplier_findings, list), "supplier findings must be list")
    for finding in supplier_findings:
        _require(finding.get("product_id") == selected, "supplier finding for wrong product")
        _require(finding.get("status") in ("verified", "unverified", "blocked"), "finding status invalid")
        _require(isinstance(finding.get("evidence_references"), list), "supplier provenance required")
    return {"selected_product_id": selected, "council_result": result, "supplier_findings": supplier_findings}


def evidence_version(handoff: dict[str, Any]) -> str:
    payload = json.dumps(handoff, sort_keys=True, separators=(",", ":"), ensure_ascii=False, allow_nan=False)
    return sha256(payload.encode("utf-8")).hexdigest()


def human_decision(handoff: dict[str, Any], action: str, actor: str, rationale: str,
                   version: str, *, timestamp: str | None = None) -> dict[str, Any]:
    _require(action in ("approve", "reject", "request_more_information"), "invalid action")
    _require(isinstance(actor, str) and bool(actor.strip()), "human actor required")
    _require(isinstance(rationale, str) and bool(rationale.strip()), "rationale required")
    _require(version == evidence_version(handoff), "stale evidence version")
    return {"selected_product_id": handoff["selected_product_id"],
            "approved_for_onboarding": {"approve": True, "reject": False, "request_more_information": None}[action],
            "action": action, "actor": actor, "rationale": rationale,
            "evidence_version": version, "timestamp": timestamp or datetime.now(timezone.utc).isoformat()}


def assert_onboarding_authorized(handoff: dict[str, Any], decision: dict[str, Any] | None) -> None:
    _require(isinstance(decision, dict) and decision.get("approved_for_onboarding") is True,
             "explicit human onboarding approval required")
    _require(decision.get("action") == "approve" and bool(decision.get("actor")) and bool(decision.get("rationale")),
             "valid human approval record required")
    _require(decision.get("selected_product_id") == handoff["selected_product_id"], "product mismatch")
    _require(decision.get("evidence_version") == evidence_version(handoff), "approval is stale")
