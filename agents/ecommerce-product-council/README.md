# Product Council reference implementation

See [approved architecture](../../docs/product-council-sourcing-approval.md) and [sequence and component diagrams](../../docs/product-council-implementation-diagrams.md).

The Python standard-library module `council.py` implements deterministic five-specialist synthesis, one-winner selection, sourcing evidence handoff and evidence-bound human authorization.

`review_store.py` adds SQLite-backed durable review records and an append-only decision history. It binds review IDs to immutable sourcing evidence fingerprints, rejects stale evidence, deduplicates identical consecutive decisions, and exposes `authorize(review_id, current_handoff)` for a downstream onboarding adapter.

Example host integration:

```python
from review_store import ReviewStore

store = ReviewStore("reviews.sqlite3")
review = store.submit("sourcing-run-123", sourcing_handoff_payload)
# Render review["handoff"] and review["evidence_version"] to an authenticated reviewer.
store.decide("sourcing-run-123", "approve", authenticated_actor_id,
             "Reviewed supplier evidence and risks", review["evidence_version"])
# Immediately before an onboarding side effect:
store.authorize("sourcing-run-123", current_sourcing_handoff)
```

Run `cd agents/ecommerce-product-council && python -m unittest discover -v`.

**Production boundary:** The review store is not an authenticated hosted interface. The host must authenticate and authorize reviewers, manage the database and backups, and call `authorize` at the actual onboarding side-effect boundary. Live specialist research, supplier network integrations, hosted human-review UI, and production onboarding wiring remain pending. Do not use the reference code to claim real supplier verification or compliance clearance.
