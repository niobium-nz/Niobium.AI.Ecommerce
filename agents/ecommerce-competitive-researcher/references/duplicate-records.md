# Per-ID duplicate records

## Storage and setup

Use the configured `commerce_folder_id` with two direct children, `discovery`
and `processed-ad-ids`. Bind both by ID. Keep one raw JSON record per exact
input ad ID in the processed folder. Use namespace `researcher_ads` globally
across products, jobs and markets. Each distinct workflow that needs its own
history binds a separate namespace and records folder. Workflows without
persistent deduplication do not acquire claims.

Bind the native Google spreadsheet `Claim index` inside the processed folder
by `claim_spreadsheet_id`. Verify exact name, ID, parent and MIME before use.
Bind its `Claims` grid tab by `claim_sheet_id`. Verify `Claims!A1:A5` contains
these exact string values in order:

```text
held
publishing
processed
released
ecommerce_duplicate_claims_v1
```

Only the duplicate protocol writes managed named ranges. Every writer of this
history must use that same index and namespace. Do not share, rename, move,
manually edit or recreate the resources during runs. Confirm the host can execute
raw Sheets batch updates with custom range IDs and read an exact named range by
name. A missing capability or an invalid index is a preflight failure. Drive
supports repeated filenames, so creating or looking up a filename alone cannot
serve as the claim. Sheets rejects an existing named range ID and applies every
request in a batch atomically. The index uses those guarantees without a shared
whole-file read/replace or a lock held across unrelated product jobs.

## Claim and advance

Use the optional packaged `offline-validation-support/duplicate_claims.py` or
an equivalent implementation. From verified input, generate a trusted UUIDv4
claim ID and use exact `ads[].ad_archive_id` strings. For the Python adapter:

```sh
python3 duplicate_claims.py claim --namespace researcher_ads --sheet-id <sheet-id> \
  --input <validated-input.json> --claim-id <claim-uuid>
```

Apply `requests` from the plan in one raw Sheets `spreadsheets.batchUpdate`
action against the verified spreadsheet. Do not split any product's batch. Read
the exact `receipt` named range using the Sheets values action and check its
literal state and coordinates on the verified tab. For the initial claim, also
verify the returned add replies match every expected named range ID, name and
range when the response supplies them. The receipt binds claim ID, job and exact
key set. A verified receipt in the expected state proves the corresponding
atomic batch. A timeout is reconciled against that same receipt. Never substitute
another claim ID or infer success from a missing or malformed read.

If acquisition fails, inspect the exact named ranges for the input IDs. Return
`duplicate_found` only for an observed `processed` ID, and reject the whole
product. A `held` or `publishing` key is contention or pending recovery. Report
`duplicate_claim_held` with the observed state and affected IDs. A provider error
without evidence of either is `duplicate_claim_unavailable` or
`duplicate_claim_outcome_unknown`. Record the claim ID and job ID for recovery.
Missing keys and unsuccessful reads never authorize research. For a known
successful claim with a confirmed failure before publication, apply `release`
and verify the `released` receipt. It frees all owned IDs as one batch.

After scoring and full candidate validation, allocate the signal ID and compute
SHA-256 of the exact UTF-8 bytes to upload. Apply `prepare` with the same inputs
plus `--signal-id` and `--artifact-sha256`. Confirm the receipt and
`publication_receipt` both resolve to `publishing` before attempting the
candidate upload. The publication receipt binds the claim to the signal ID
and payload digest. Write once and follow storage.md for exact readback. Claims
stay held across interruption. An absent previous-phase fence causes any stale
transition to fail atomically.

## Save the individual records and finish

After verifying the candidate, save each `records[].filename` from the plan
inside the verified processed folder as raw UTF-8 `application/json`. Write
the exact input `ad_archive_id`, `job_id`, `signal_id`, and verified
`output_file_id`. Safe IDs are used directly as the filename stem. Other IDs
use a reserved SHA-256 filename and retain the original ID in the JSON. Never
record competition-report IDs. Before each write, check the exact filename
across all MIME types and exposed pages. Reuse an existing file only if exact
name, parent, MIME and complete JSON match the expected record. A collision with
another record or a non-file stops the operation. After writing, verify metadata
and full readback. Reconcile an uncertain write by exact path and bytes without
a second upload. Apply the identity and lookup evidence rules in storage.md.

When all records are verified, apply `finish` using the same signal ID and SHA-256.
Verify the receipt reads `processed`. Only then report `registry_updated: true`.
If the candidate is verified but any record write or finalization is unresolved,
return `published_registry_pending`, retaining the candidate IDs and adding a
registry-stage error with the claim ID. Keep the owned keys in `publishing` until
verified. Other products with disjoint ad IDs can proceed. Explicit duplicate
override bypasses all index/record operations and can intentionally reread any
ad. It still enforces the candidate destination and exact upload readback.

## Reconcile a stopped claim

Use the original validated input, job ID and claim ID. Find a hard-interrupted
run's claim and publication receipts in the index's named ranges or the host
trace. A `held` claim can be released when the run is known to have stopped
before publication. A `publishing` receipt records the signal ID and expected
artifact SHA-256. Reconcile the exact candidate path and bytes. If verified,
verify existing individual records, create only definitively missing records,
and finish the same claim. If the upload was definitively rejected or never
attempted, the stopped run's owner can use `release_publishing` with the exact
signal and SHA-256. Leave the claim in place when the write outcome is unknown.
A lookup that finds no file does not alone prove that an upload failed. Never
expire claims on a timer, clear a different owner's claim, repeat research or
mint a replacement candidate during reconciliation.

## Migrate a previous history

Stop old writers before migration. Read the complete trusted legacy registry
using its deployed format, JSON string array or UTF-8 newline IDs. Keep exact
strings and existing decisions, and preserve the old file as a read-only backup.
Reconcile unresolved old writes before cutover. For each historical ID, create
or verify its individual record with `ad_archive_id` and
`source: "legacy_registry"`. Do not fabricate candidate IDs for migrated records.
Use `seed --ids <verified-legacy-file> --legacy-format <format>` to generate
create-only processed ranges, in bounded batches if needed. On recovery, verify
already migrated records and exact processed ranges, then submit only missing
ranges. Verify every old ID against both the index and its record before making
the new deployment active. An empty legacy registry has nothing to seed.

- [Drive file names and identity](https://developers.google.com/workspace/drive/api/reference/rest/v3/files)
- [Google Sheets named range requests](https://developers.google.com/workspace/sheets/api/reference/rest/v4/spreadsheets/request#addnamedrangerequest)
- [Atomic spreadsheet batch updates](https://developers.google.com/workspace/sheets/api/reference/rest/v4/spreadsheets/batchUpdate)
