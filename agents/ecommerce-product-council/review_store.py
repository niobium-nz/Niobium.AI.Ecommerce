"""SQLite-backed review queue and evidence-bound onboarding authorization.

This module is a local persistence boundary, not a hosted UI or authentication
service. The caller must authenticate the reviewer and enforce permissions.
"""
from __future__ import annotations

from datetime import datetime, timezone
import json
import sqlite3
from pathlib import Path
from typing import Any

from council import evidence_version, human_decision, assert_onboarding_authorized


class ReviewStore:
    def __init__(self, database: str | Path):
        self.database = str(database)
        self._initialize()

    def _connect(self):
        connection = sqlite3.connect(self.database, timeout=30)
        connection.row_factory = sqlite3.Row
        connection.execute("PRAGMA busy_timeout=30000")
        return connection

    def _initialize(self):
        with self._connect() as db:
            db.execute("""CREATE TABLE IF NOT EXISTS reviews (
                review_id TEXT PRIMARY KEY,
                product_id TEXT NOT NULL,
                handoff_json TEXT NOT NULL,
                evidence_version TEXT NOT NULL,
                created_at TEXT NOT NULL
            )""")
            db.execute("""CREATE TABLE IF NOT EXISTS decisions (
                decision_id INTEGER PRIMARY KEY AUTOINCREMENT,
                review_id TEXT NOT NULL REFERENCES reviews(review_id),
                decision_json TEXT NOT NULL,
                created_at TEXT NOT NULL
            )""")
            db.execute("CREATE INDEX IF NOT EXISTS decisions_review_idx ON decisions(review_id, decision_id)")

    def submit(self, review_id: str, handoff: dict[str, Any]) -> dict[str, Any]:
        if not isinstance(review_id, str) or not review_id.strip():
            raise ValueError("review_id required")
        if not isinstance(handoff, dict) or not isinstance(handoff.get("selected_product_id"), str):
            raise ValueError("selected product required")
        if not isinstance(handoff.get("supplier_findings"), list) or not isinstance(handoff.get("council_result"), dict):
            raise ValueError("complete sourcing handoff required")
        version = evidence_version(handoff)
        payload = json.dumps(handoff, sort_keys=True, ensure_ascii=False, allow_nan=False)
        now = datetime.now(timezone.utc).isoformat()
        with self._connect() as db:
            db.execute("BEGIN IMMEDIATE")
            previous = db.execute("SELECT evidence_version FROM reviews WHERE review_id=?", (review_id,)).fetchone()
            if previous:
                if previous["evidence_version"] != version:
                    raise ValueError("review ID already bound to different evidence; create a new review")
            else:
                db.execute("INSERT INTO reviews VALUES (?,?,?,?,?)",
                           (review_id, handoff["selected_product_id"], payload, version, now))
        return self.get(review_id)

    def get(self, review_id: str) -> dict[str, Any]:
        with self._connect() as db:
            row = db.execute("SELECT * FROM reviews WHERE review_id=?", (review_id,)).fetchone()
            if row is None:
                raise KeyError(review_id)
            history = db.execute("SELECT decision_json FROM decisions WHERE review_id=? ORDER BY decision_id",
                                 (review_id,)).fetchall()
        decisions = [json.loads(item["decision_json"]) for item in history]
        return {"review_id": review_id, "selected_product_id": row["product_id"],
                "handoff": json.loads(row["handoff_json"]),
                "evidence_version": row["evidence_version"],
                "decisions": decisions,
                "approved_for_onboarding": decisions[-1]["approved_for_onboarding"] if decisions else None}

    def decide(self, review_id: str, action: str, actor: str, rationale: str,
               version: str) -> dict[str, Any]:
        with self._connect() as db:
            db.execute("BEGIN IMMEDIATE")
            row = db.execute("SELECT handoff_json, evidence_version FROM reviews WHERE review_id=?",
                             (review_id,)).fetchone()
            if row is None:
                raise KeyError(review_id)
            handoff = json.loads(row["handoff_json"])
            if version != row["evidence_version"]:
                raise ValueError("stale evidence version")
            decision = human_decision(handoff, action, actor, rationale, version)
            previous = db.execute("SELECT decision_json FROM decisions WHERE review_id=? ORDER BY decision_id DESC LIMIT 1",
                                  (review_id,)).fetchone()
            if previous:
                last = json.loads(previous["decision_json"])
                if all(last.get(k) == decision.get(k) for k in ("action", "actor", "rationale", "evidence_version")):
                    return last
            db.execute("INSERT INTO decisions (review_id, decision_json, created_at) VALUES (?,?,?)",
                       (review_id, json.dumps(decision, sort_keys=True), decision["timestamp"]))
            return decision

    def authorize(self, review_id: str, current_handoff: dict[str, Any]) -> dict[str, Any]:
        record = self.get(review_id)
        if record["evidence_version"] != evidence_version(current_handoff):
            raise ValueError("review evidence is stale")
        if record["selected_product_id"] != current_handoff.get("selected_product_id"):
            raise ValueError("product mismatch")
        latest = record["decisions"][-1] if record["decisions"] else None
        assert_onboarding_authorized(current_handoff, latest)
        return latest

    def pending(self) -> list[dict[str, Any]]:
        with self._connect() as db:
            ids = [row["review_id"] for row in db.execute("SELECT review_id FROM reviews ORDER BY created_at, review_id")]
        return [record for rid in ids if (record := self.get(rid))["approved_for_onboarding"] is None]
