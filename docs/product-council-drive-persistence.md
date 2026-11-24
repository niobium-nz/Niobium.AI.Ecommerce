# Product Council Google Drive persistence

## Scope and unchanged contract

`agents/ecommerce-product-council/review_store.py` replaces the SQLite persistence boundary, not the domain in `council.py`. Discovery -> research -> Council selection -> sourcing -> human review -> onboarding remains unchanged. Selection never grants approval. Pending, rejected, stale, mismatched, conflicted or invalid evidence must not authorize onboarding.

The constructor is now `ReviewStore(client, folder_id)`. The injected `DriveClient` has only `list_files`, `read_json` and `create_json`; there is no update or delete operation. The optional `GoogleDriveClient(service, shared_drive_id=None)` accepts an already-authenticated Drive v3 service. It does not obtain credentials or authenticate reviewers.

## Durable representation

One JSON manifest records `schema_version`, `kind`, `review_id`, `selected_product_id`, `evidence_version`, the complete `handoff`, `created_at` and a SHA-256 `fingerprint`. The schema version is the string `"1"`. A review ID cannot be rebound to changed evidence; use a new review ID and obtain new human approval.

Each human action is a separate JSON event containing the review/product/evidence identifiers, `parent_event_ids`, the unchanged domain `decision` fields, deterministic `event_id`, and a full-record fingerprint. Normal decisions have zero or one parent. An explicit human resolution has multiple parents, acknowledging all current conflicting heads. Existing decision fields retain their names and values; the public returned decision adds `event_id` and `parent_event_ids`.

Compact Drive `properties` contain `store=niobium-council`, `schema`, `kind`, the hashed review ID, evidence version and (for decisions) event ID. These are indexing hints, not authority. Public custom properties allow authorized clients using different OAuth applications to share the index; they do not make a file publicly accessible. Do not substitute client-private `appProperties` without a separately coordinated storage migration.

JSON canonicalization uses sorted keys, compact separators, UTF-8 and no non-finite numbers. Full-record fingerprints cover timestamps as well as payloads. Event identity excludes its own ID, checksum and generated timestamp, but includes the review, product, evidence, human action, authenticated actor, rationale and sorted parents. Two physical files with the same logical event ID are deduplicated after both have been validated. A stable timestamp representative is used only for display, never to choose approval over rejection.

On every read, the store recalculates the evidence fingerprint, record checksum and semantic event ID; revalidates domain decision fields; checks index/body agreement; and rejects missing parents or cycles. Identical manifest retries are accepted, but different evidence under one review ID is an integrity error. Unknown schemas, malformed records and unreadable files fail closed.

## Human workflow and concurrency

`submit(review_id, handoff)` persists the sourcing handoff. `get(review_id)` returns the verified dossier, decision history and `head_event_id`. The UI displays all evidence and retains that head along with the evidence version. `decide(..., expected_head_event_id=displayed_head)` appends an authenticated human action. Explicit `None` means the reviewer saw an empty history. Omitting the keyword preserves the previous API's call shape, but cannot detect a UI that became stale before the call; production hosts should always supply it.

Identical consecutive decisions are idempotent. Retrying a lost successful response with its original parent returns the same current event. The same action after an intervening decision is a new event. Retrying an old approval after a later rejection fails the head check rather than restoring the approval. Following an indeterminate create error, reload and retry the same human operation with its original displayed head; never generate a fresh approval silently.

Two different events written from the same head create a conflict, even when both say approve. `get`, ordinary `decide` and `authorize` raise `ReviewConflictError`. There is no timestamp winner or automatic branch deletion. The exception carries `head_event_ids`; `inspect(review_id)` returns a verified but non-authorizing dossier with all competing decisions and `conflicted=True`. `pending()` includes conflicted records so the host can surface them.

After a human reloads and reviews the full conflict, the host may record a fresh decision:

```python
conflict = store.inspect(review_id)
# Display evidence and every competing decision; obtain a NEW human action.
resolved = store.resolve(
    review_id, human_action, authenticated_actor_id, human_rationale,
    conflict["evidence_version"],
    expected_head_event_ids=conflict["head_event_ids"],
)
```

`resolve` requires the exact displayed set of current heads and appends a new event with all of them as parents. Competing history remains intact. A stale resolution is rejected; two genuinely racing resolutions produce another conflict. Resolution is not automatic approval: reject and request_more_information remain valid outcomes.

## Consistency and integrity limits

Google Drive is not a transactional database. The adapter exhausts pagination, uses Shared Drive flags where appropriate, rejects `incompleteSearch`, propagates API errors, and refuses malformed/repeating pagination. Post-write reads detect visible conflicts and refuse to return an unpersisted local approval. None of these checks provides a multi-file snapshot, compare-and-swap lock or linearizable read.

An unseen concurrent event can appear after a read, and a decision can change between authorization and an external onboarding side effect. For strict ordering, the host must serialize decision writers and onboarding through a trusted coordinator. Always call `authorize(review_id, current_handoff)` immediately before the real side effect, under that coordination when required. Neither `inspect`, a prior `decide` result nor a previously cached authorization is sufficient.

Application-level append-only behavior is not write-once storage. A privileged editor can modify/delete files or indexing metadata, hide a rejecting leaf, or recompute hashes after editing. Hashes detect inconsistent data, not a malicious administrator, and are not signatures proving human identity. Use restricted write access, independent audit/backup retention, and operational reconciliation. If regulatory immutability or stronger distributed consistency is required, this Drive-only boundary is insufficient without additional infrastructure.

## Host configuration and acceptance

The host must supply an authenticated Drive service or equivalent connector adapter, a dedicated review folder ID, and optionally a Shared Drive ID. Prefer organizational ownership where available. Choose the narrowest OAuth scope compatible with the actual credential/folder model; validate visibility with the same application identity used in production. Credentials and secrets stay outside this repository. Only the SDK adapter's upload path needs the optional `google-api-python-client` dependency; domain tests use the standard library.

Human identity is a separate boundary: authenticate the person, verify their approval permission, and pass their trusted identity into `decide` or `resolve`. Never accept a free-form actor from an unauthenticated request. Agent Drive credentials alone do not authorize human decisions.

Before enabling onboarding, use an isolated non-production folder to verify creation, restart/reload, all-page enumeration, duplicate retries, rejection, changed evidence, conflicting writers, explicit resolution, permission loss and the actual onboarding guard. Confirm the service identity sees every review file, including files created by any other participating client. No live Drive folder, credential setup, production data migration or onboarding side effect is exercised by the offline suite.

## SQLite cutover

There is no implicit database import and no fallback to SQLite. Stop legacy writers, back up the old database and retain its historical audit records. Explicitly export and submit the required sourcing handoffs to Drive, verify their fingerprints, and have authorized humans review and decide again before enabling onboarding. Do not infer approval merely because a legacy record was copied. A historical decision-preserving migration requires a separate audited importer and reconciliation plan. This change does not modify or delete any existing database or Drive data.

## Validation

Run `python -B -m unittest discover -s agents/ecommerce-product-council -v` from the repository root. The suite covers the unchanged Council behavior, persistence/retry semantics, invalid evidence, stale heads, competing and resolved histories, corrupted records, missing parents, pending enumeration, folder isolation, and the Drive v3 adapter's requests, pagination and error handling. The Agent packages workflow runs it on the existing Python matrix in addition to the package checks.

## Drive API references

- [Search files, custom properties and query escaping](https://developers.google.com/workspace/drive/api/guides/search-files).
- [Shared Drive parameters and incomplete search handling](https://developers.google.com/workspace/drive/api/guides/enable-shareddrives).

The JSON schema, event identity and conflict-resolution behavior above are this repository's application contract, not guarantees supplied by Google Drive.
