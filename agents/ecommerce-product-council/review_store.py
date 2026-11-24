"""Append-only Drive review records; human authentication belongs to the host.

Drive is not a transaction manager or a tamper-proof audit system. See
../../docs/product-council-drive-persistence.md for the consistency boundary.
"""
from __future__ import annotations

from datetime import datetime, timezone
from hashlib import sha256
import heapq
import json
from typing import Any, Protocol

from council import evidence_version, human_decision, assert_onboarding_authorized

_NAMESPACE = "niobium-council"
_SCHEMA = "1"
_UNSET = object()


class ReviewIntegrityError(ValueError):
    """Stored evidence, metadata, or decision history is inconsistent."""


class ReviewConflictError(ValueError):
    """The displayed head is stale or multiple decisions need human resolution."""

    def __init__(self, message: str, head_event_ids: list[str]):
        super().__init__(message)
        self.head_event_ids = tuple(sorted(head_event_ids))


class DriveClient(Protocol):
    """Host adapter: complete, uncached folder reads and create-only JSON writes.

    list_files returns every matching non-trashed file, including all pages,
    with its id and properties. Errors/incomplete reads must never become [].
    Properties are Drive's cross-application `properties`, not appProperties.
    """

    def list_files(self, folder_id: str, properties: dict[str, str]) -> list[dict[str, Any]]: ...
    def read_json(self, file_id: str) -> dict[str, Any]: ...
    def create_json(self, folder_id: str, name: str, properties: dict[str, str],
                    body: dict[str, Any]) -> str: ...


def _json(value: Any) -> str:
    return json.dumps(value, sort_keys=True, separators=(",", ":"), ensure_ascii=False, allow_nan=False)


def _digest(value: Any) -> str:
    return sha256(_json(value).encode("utf-8")).hexdigest()


def _text(value: Any, label: str) -> str:
    if not isinstance(value, str) or not value.strip():
        raise ValueError(f"{label} required")
    return value


def _timestamp(value: Any) -> None:
    if datetime.fromisoformat(_text(value, "timestamp")).tzinfo is None:
        raise ValueError("timestamp must include timezone")


def _handoff_version(handoff: Any) -> str:
    if not isinstance(handoff, dict):
        raise ValueError("complete sourcing handoff required")
    selected = _text(handoff.get("selected_product_id"), "selected product")
    if not isinstance(handoff.get("supplier_findings"), list) or not isinstance(handoff.get("council_result"), dict):
        raise ValueError("complete sourcing handoff required")
    if handoff["council_result"].get("selected_product_id") != selected:
        raise ValueError("council selected product mismatch")
    return evidence_version(handoff)


def _properties(body: dict[str, Any]) -> dict[str, str]:
    result = {"store": _NAMESPACE, "schema": _SCHEMA, "kind": body["kind"],
              "review": _digest(body["review_id"]), "evidence": body["evidence_version"]}
    if body["kind"] == "decision":
        result["event"] = body["event_id"]
    return result


def _seal(body: dict[str, Any]) -> dict[str, Any]:
    return {**body, "fingerprint": _digest(body)}


def _semantic(body: dict[str, Any]) -> dict[str, Any]:
    """Logical event identity excludes only storage checksum, ID and timestamp."""
    return {k: ({d: v for d, v in value.items() if d != "timestamp"} if k == "decision" else value)
            for k, value in body.items() if k not in ("fingerprint", "event_id")}


def _decision(event: dict[str, Any]) -> dict[str, Any]:
    return {**event["decision"], "event_id": event["event_id"],
            "parent_event_ids": list(event["parent_event_ids"])}


def _same_action(left: dict[str, Any], right: dict[str, Any]) -> bool:
    return all(left.get(key) == right.get(key) for key in
               ("selected_product_id", "evidence_version", "action", "actor", "rationale"))


class ReviewStore:
    """Evidence-bound human review using an injected DriveClient and folder ID."""

    def __init__(self, client: DriveClient, folder_id: str):
        self.client = client
        self.folder_id = _text(folder_id, "folder_id")

    def _files(self, review_id: str | None = None) -> list[dict[str, Any]]:
        properties = {"store": _NAMESPACE}
        if review_id is not None:
            properties["review"] = _digest(_text(review_id, "review_id"))
        return self.client.list_files(self.folder_id, properties)

    def _read(self, file: dict[str, Any]) -> dict[str, Any]:
        try:
            body = self.client.read_json(file["id"])
            unsigned = {k: v for k, v in body.items() if k != "fingerprint"}
            if body["fingerprint"] != _digest(unsigned) or body["schema_version"] != _SCHEMA:
                raise ValueError("invalid fingerprint or unsupported schema")
            if body["kind"] not in ("review", "decision"):
                raise ValueError("unknown object kind")
            _text(body["review_id"], "review_id")
            if any(file["properties"].get(k) != v for k, v in _properties(body).items()):
                raise ValueError("index metadata disagrees with JSON")
            return body
        except (KeyError, TypeError, ValueError, AttributeError) as exc:
            raise ReviewIntegrityError(f"invalid Drive record {file.get('id', '?')}: {exc}") from exc

    def _write(self, body: dict[str, Any]) -> None:
        properties = _properties(body)
        identity = properties.get("event", properties["evidence"])
        self.client.create_json(self.folder_id,
                                f"{body['kind']}-{properties['review']}-{identity}.json",
                                properties, body)

    def submit(self, review_id: str, handoff: dict[str, Any]) -> dict[str, Any]:
        """Create an immutable review manifest; retrying identical evidence is safe."""
        _text(review_id, "review_id")
        # Detach caller-owned containers before hashing or persisting them.
        handoff = json.loads(_json(handoff))
        version = _handoff_version(handoff)
        if self._files(review_id):
            existing = self.get(review_id)
            if existing["evidence_version"] != version:
                raise ValueError("review ID already bound to different evidence; create a new review")
            return existing
        self._write(_seal({"schema_version": _SCHEMA, "kind": "review", "review_id": review_id,
                           "selected_product_id": handoff["selected_product_id"],
                           "evidence_version": version, "handoff": handoff,
                           "created_at": datetime.now(timezone.utc).isoformat()}))
        return self.get(review_id)

    def _load(self, review_id: str) -> tuple[dict[str, Any], dict[str, dict[str, Any]], list[str], list[str]]:
        files = self._files(review_id)
        if not files:
            raise KeyError(review_id)
        manifest = None
        events: dict[str, dict[str, Any]] = {}
        bodies = [self._read(file) for file in files]
        try:
            for body in bodies:
                if body["review_id"] != review_id:
                    raise ValueError("review ID mismatch")
                if body["kind"] != "review":
                    continue
                if set(body) != {"schema_version", "kind", "review_id", "selected_product_id",
                                 "evidence_version", "handoff", "created_at", "fingerprint"}:
                    raise ValueError("invalid manifest fields")
                if _handoff_version(body["handoff"]) != body["evidence_version"]:
                    raise ValueError("sourcing evidence fingerprint mismatch")
                if body["selected_product_id"] != body["handoff"]["selected_product_id"]:
                    raise ValueError("selected product mismatch")
                _timestamp(body["created_at"])
                if manifest and manifest["evidence_version"] != body["evidence_version"]:
                    raise ValueError("conflicting review manifests")
                if manifest is None or body["created_at"] < manifest["created_at"]:
                    manifest = body
            if manifest is None:
                raise ValueError("decision history has no review manifest")
            for body in bodies:
                if body["kind"] != "decision":
                    continue
                if set(body) != {"schema_version", "kind", "review_id", "selected_product_id",
                                 "evidence_version", "parent_event_ids", "decision", "event_id", "fingerprint"}:
                    raise ValueError("invalid decision event fields")
                if any(body[key] != manifest[key] for key in ("selected_product_id", "evidence_version")):
                    raise ValueError("decision is for different evidence or product")
                parents = body["parent_event_ids"]
                if not isinstance(parents, list) or any(not isinstance(p, str) or len(p) != 64 for p in parents):
                    raise ValueError("invalid event parents")
                if parents != sorted(set(parents)):
                    raise ValueError("event parents must be sorted and unique")
                decision = body["decision"]
                _timestamp(decision["timestamp"])
                expected = human_decision(manifest["handoff"], decision["action"], decision["actor"],
                                          decision["rationale"], manifest["evidence_version"],
                                          timestamp=decision["timestamp"])
                if _json(decision) != _json(expected) or body["event_id"] != _digest(_semantic(body)):
                    raise ValueError("decision event fingerprint or authorization fields mismatch")
                previous = events.get(body["event_id"])
                if previous is None or decision["timestamp"] < previous["decision"]["timestamp"]:
                    events[body["event_id"]] = body
            # Causal order, never timestamp order. Multiple leaves mean a conflict.
            children: dict[str, list[str]] = {eid: [] for eid in events}
            remaining = {eid: len(event["parent_event_ids"]) for eid, event in events.items()}
            for eid, event in events.items():
                for parent in event["parent_event_ids"]:
                    if parent not in events:
                        raise ValueError("missing parent decision")
                    children[parent].append(eid)
            ready = [eid for eid, count in remaining.items() if count == 0]
            heapq.heapify(ready)
            ordered = []
            while ready:
                eid = heapq.heappop(ready)
                ordered.append(eid)
                for child in children[eid]:
                    remaining[child] -= 1
                    if remaining[child] == 0:
                        heapq.heappush(ready, child)
            if len(ordered) != len(events):
                raise ValueError("cyclic decision history")
            return manifest, events, ordered, sorted(eid for eid in events if not children[eid])
        except (KeyError, TypeError, ValueError, AttributeError) as exc:
            raise ReviewIntegrityError(f"invalid review {review_id}: {exc}") from exc

    def inspect(self, review_id: str) -> dict[str, Any]:
        """Verified dossier for display, including unresolved competing decisions.

        This diagnostic method never authorizes onboarding. A conflicted review
        has no effective decision and must be explicitly resolved by a human.
        """
        manifest, events, ordered, heads = self._load(review_id)
        latest = _decision(events[heads[0]]) if len(heads) == 1 else None
        return {"review_id": review_id, "selected_product_id": manifest["selected_product_id"],
                "handoff": manifest["handoff"], "evidence_version": manifest["evidence_version"],
                "created_at": manifest["created_at"], "decisions": [_decision(events[eid]) for eid in ordered],
                "head_event_ids": heads, "head_event_id": heads[0] if len(heads) == 1 else None,
                "conflicted": len(heads) > 1,
                "approved_for_onboarding": latest["approved_for_onboarding"] if latest else None}

    def get(self, review_id: str) -> dict[str, Any]:
        record = self.inspect(review_id)
        if record["conflicted"]:
            raise ReviewConflictError("concurrent decisions require human resolution", record["head_event_ids"])
        return record

    def _append(self, record: dict[str, Any], decision: dict[str, Any], parents: list[str]) -> dict[str, Any]:
        body = {"schema_version": _SCHEMA, "kind": "decision", "review_id": record["review_id"],
                "selected_product_id": record["selected_product_id"], "evidence_version": record["evidence_version"],
                "parent_event_ids": sorted(parents), "decision": decision}
        body["event_id"] = _digest(_semantic(body))
        self._write(_seal(body))
        # Detect visible races and incomplete read-after-write visibility. Never
        # return a locally fabricated approval when persistence cannot be read.
        after = self.get(record["review_id"])
        for stored in after["decisions"]:
            if stored["event_id"] == body["event_id"]:
                return stored
        raise ReviewIntegrityError("new decision is not visible; reload before retrying")

    def decide(self, review_id: str, action: str, actor: str, rationale: str, version: str,
               *, expected_head_event_id: Any = _UNSET) -> dict[str, Any]:
        """Append a human action. Hosts should always supply the displayed head.

        Explicit None means the reviewer saw no decisions. Omitting the argument
        preserves the old API but cannot detect a stale UI before this call.
        """
        record = self.get(review_id)
        decision = human_decision(record["handoff"], action, actor, rationale, version)
        head = record["head_event_id"]
        latest = record["decisions"][-1] if head else None
        if expected_head_event_id is not _UNSET and expected_head_event_id != head:
            parents = [] if expected_head_event_id is None else [expected_head_event_id]
            # A lost successful response may be retried with the original head.
            if latest and latest["parent_event_ids"] == parents and _same_action(latest, decision):
                return latest
            raise ReviewConflictError("stale decision head; reload the review", record["head_event_ids"])
        if latest and _same_action(latest, decision):
            return latest
        return self._append(record, decision, [head] if head else [])

    def resolve(self, review_id: str, action: str, actor: str, rationale: str, version: str,
                *, expected_head_event_ids: list[str]) -> dict[str, Any]:
        """Record a fresh human decision acknowledging every displayed conflict head."""
        if (not isinstance(expected_head_event_ids, list) or len(expected_head_event_ids) < 2
                or any(not isinstance(eid, str) for eid in expected_head_event_ids)
                or len(set(expected_head_event_ids)) != len(expected_head_event_ids)):
            raise ValueError("at least two distinct conflict heads required")
        parents = sorted(expected_head_event_ids)
        record = self.inspect(review_id)
        decision = human_decision(record["handoff"], action, actor, rationale, version)
        if not record["conflicted"] and record["decisions"]:
            latest = record["decisions"][-1]
            if latest["parent_event_ids"] == parents and _same_action(latest, decision):
                return latest
        if parents != record["head_event_ids"]:
            raise ReviewConflictError("conflict heads changed; reload all decisions", record["head_event_ids"])
        return self._append(record, decision, parents)

    def authorize(self, review_id: str, current_handoff: dict[str, Any]) -> dict[str, Any]:
        """Re-read persisted evidence and history immediately before onboarding."""
        record = self.get(review_id)
        if record["evidence_version"] != evidence_version(current_handoff):
            raise ValueError("review evidence is stale")
        if record["selected_product_id"] != current_handoff.get("selected_product_id"):
            raise ValueError("product mismatch")
        latest = record["decisions"][-1] if record["decisions"] else None
        assert_onboarding_authorized(current_handoff, latest)
        return latest

    def pending(self) -> list[dict[str, Any]]:
        """Include unanswered, more-information and conflicted reviews for humans."""
        ids = {self._read(file)["review_id"] for file in self._files()}
        records = [self.inspect(rid) for rid in ids]
        return sorted((r for r in records if r["approved_for_onboarding"] is None),
                      key=lambda r: (r["created_at"], r["review_id"]))
