# Product Council reference implementation

See [approved architecture](../../docs/product-council-sourcing-approval.md), [sequence and component diagrams](../../docs/product-council-implementation-diagrams.md), and [Drive persistence and operations](../../docs/product-council-drive-persistence.md).

The Python standard-library module `council.py` implements deterministic five-specialist synthesis, one-winner selection, sourcing evidence handoff and evidence-bound human authorization. Its domain contract is unchanged.

`review_store.py` uses an injected `DriveClient` to persist one immutable JSON review manifest and one JSON file per human decision. It binds review IDs to sourcing evidence fingerprints, verifies stored content on reads, deduplicates semantic retries, rejects stale heads and evidence, and blocks onboarding while concurrent decisions remain unresolved. It never updates or deletes Drive objects.

`google_drive_client.py` is an optional Drive v3 adapter for ordinary folders and Shared Drives. The host supplies an already-authenticated service; only this adapter's upload operation requires `google-api-python-client`. A connector/MCP host can instead implement the three-method `DriveClient` protocol without installing the SDK. Every adapter must exhaust pagination, report incomplete reads as errors, and use the documented cross-application custom properties.

## Host integration

```python
from google_drive_client import GoogleDriveClient
from review_store import ReviewStore

# drive_service, review_folder_id and authenticated_actor_id come from the host.
# Omit shared_drive_id for My Drive; no credentials belong in this module.
client = GoogleDriveClient(drive_service, shared_drive_id=shared_drive_id)
store = ReviewStore(client, review_folder_id)
review = store.submit("sourcing-run-123", sourcing_handoff_payload)
# Display the full dossier, evidence version and head to an authenticated human.
decision = store.decide(
    "sourcing-run-123", "approve", authenticated_actor_id,
    "Reviewed supplier evidence and risks", review["evidence_version"],
    expected_head_event_id=review["head_event_id"],  # None is an explicit empty head.
)
# Re-read persisted state immediately before the real onboarding side effect.
store.authorize("sourcing-run-123", current_sourcing_handoff)
```

`get()` raises `ReviewConflictError` for competing decision heads. Use `inspect()` to display all verified competing decisions, then have an authorized human call `resolve(..., expected_head_event_ids=displayed_heads)`. Resolution appends a new decision referencing every displayed head; it never erases the conflicting events. `pending()` includes unanswered, more-information and conflicted reviews. Neither `inspect()` nor a cached decision is an onboarding authorization.

Run offline tests from the repository root:

```sh
python -B -m unittest discover -s agents/ecommerce-product-council -v
```

## Production boundary

The host must authenticate and authorize human reviewers, supply Drive credentials and folder configuration, enforce least-privilege access and retention, and call `authorize` at the actual side-effect boundary. A caller-provided actor string is not authentication. Drive access does not establish human approval.

Drive does not offer SQLite-style transaction isolation. The implementation detects conflicting histories visible to its reads; an unseen concurrent write or deletion cannot be ruled out by a file listing. Serialize review writes and onboarding through a host coordinator when strict ordering is required. Hashes detect inconsistent records, not a privileged editor rewriting content and hashes together. See the design document for these limits and the rollout checklist.

This replaces `ReviewStore(database_path)` with `ReviewStore(client, folder_id)`. Existing SQLite files are not modified or automatically imported. Archive them and explicitly re-review migrated handoffs as described in the design document. Live specialist research, supplier integrations, hosted human-review UI and production onboarding wiring remain host work. Do not claim supplier verification or compliance clearance from reference tests.
