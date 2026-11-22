# Storage, registry and recovery protocol

## Provision one canonical registry

The `duplicate_history` section of [agent.yaml](../agent.yaml) pins the file
`processed-ad-ids.json` directly inside the configured Google Drive discovery
folder. On initial setup, search direct children for that exact name across all
file types and apply the search completeness rules below. If there are no matches,
upload UTF-8 `[]` as `application/json` once. Verify the ID, exact name, parent,
MIME type and full contents. Stop on an observed collision or known incomplete
search. Reconcile uncertain writes without uploading another copy.

Reuse a verified existing registry and retain all its entries. Record its Drive
file ID as `registry_file_id` in the private `deployment-config.json`. Keep that
ID across installs and updates that share this history. If a binding goes missing
during redeployment, resolve the file under the configured discovery folder and
verify it before restoring the binding. Keep live IDs, sharing links, registry
contents and credentials out of Git. An explicitly bound existing registry can
keep its verified format, including legacy UTF-8 newline-separated IDs.

## Preflight and permissions

Bind Google Drive to an existing discovery folder by `discovery_folder_id`.
Confirm its exact ID/name and folder MIME. Bind the registry by `registry_file_id`
and verify its exact pinned name, discovery parent and raw-file MIME matching the
declared format (`application/json` for JSON or `text/plain` for legacy newline
data). Apply the optional metadata policy below to both resources and stop on an
observed exact-name registry collision. Do not replace the configured ID with a
search result, use root, create a missing registry during a job or change sharing.

Read the registry's complete raw bytes. Inspect actual action schemas for full
readback and safe append or in-place raw-content replacement by file ID. Resolve
those capabilities and the shared-registry serialization guarantee before research.
Use any version condition supported by the selected write tool as additional
protection. The condition alone does not serialize product jobs.

## Registry format and duplicate semantics

registry_format is REQUIRED deployment configuration, never inferred from bytes:

- json_string_array: one JSON array of nonblank string ad IDs; [] is empty. Preserve
  existing order/values; numeric IDs, nested objects, nulls and malformed JSON fail.
- newline_ids: UTF-8, one exact ID per LF or CRLF line. Empty file is empty. Ignore
  blank-only lines; do not trim nonblank IDs or split by punctuation. Appended IDs
  may not contain CR/LF. Preserve existing bytes and newline style when appending.

These supported bindings do not claim an unknown existing file already has a
particular format. Configure the actual format before deployment or provide an
explicit adapter; never guess or overwrite an unsupported registry structure.

Compare complete string IDs exactly, case-sensitively, across ALL stored records.
No substring matching, numeric conversion, country scoping or per-job reset.
Existing repeated IDs are harmless for lookup; preserve them. ANY hit rejects the
whole product. An unreadable/truncated registry never counts as empty. A resource
limit is a failure, not permission to check only a prefix of the file.

## Serialize product jobs, not keyword workers

The host/coordinator must enforce exclusive processing per configured registry
from the initial duplicate check through publication and registry verification.
This may be a serialized execution queue or a shared lock respected by EVERY writer,
including other hosts. A config value alone is not a lock. No such mechanism means
capability_unavailable before research. Never create ad hoc registry reservation
records or mark IDs processed before publication. Keyword subagents within the one
job may still run in parallel and may not access storage.

Re-read the registry before publication. If newly recorded matching IDs appear,
stop duplicate_found without output. If unrelated contents changed, preserve them
and use that current snapshot; investigate any loss of the serialization guarantee.
Candidate upload and registry update are separate Drive operations. On interrupted
or uncertain commits, the host must hold subsequent jobs for operator reconciliation.
Release an acquired guard on every exit; a durable recovery hold survives release.

## Google Drive publication

After a valid score, generate one UUIDv4 signal_id with a trusted host facility.
Use the validated upstream UUID as the exact job folder name. Look up direct
children of discovery by exact name across all types and every exposed page. Reuse exactly
one folder named job_id with no explicit trash indication. Zero matches permits creation of that exact
folder; a nonfolder match, multiple matches or known incomplete lookup is an error.
Do not overwrite discovery/{job_id}.json: it is the upstream ARRAY file, while
this agent writes inside a job_id FOLDER.

Create a missing job folder only when ready to publish. Verify its metadata and
one observed exact-name match. Reconcile an uncertain create by listing; never blindly
retry or create job_id (1). Never change an existing folder's sharing.

Check exact {signal_id}.json absence, all MIME types, before writing. Any collision
stops; do not regenerate IDs or overwrite. Serialize the validated artifact as raw
UTF-8 application/json. Upload once to the verified folder. Verify returned ID,
name, parent, MIME, trash state when exposed, full content/checksum and no observed
competing exact-name match. A snippet or success response alone is insufficient. On ambiguous upload,
reconcile read-only using the same ID/name/bytes; never retry the write. If still
uncertain, return failed with publication_outcome_unknown, do not append registry
IDs, and require recovery before retry. Do not automatically delete files/folders.
Drive names are not unique; list-then-create is not an atomic uniqueness guarantee.

## Append only after verified publication

Re-read the latest complete Google Drive registry under the serialization guard. Construct
its original content plus missing INPUT ad IDs in input order, adding each once.
Do not add target-market IDs from competition reports. Use a supported safe append
or full-content replacement by the SAME Drive file ID, preserving every existing
record; verify full readback contains the original sequence plus all expected IDs.
For newline format preserve the original byte prefix as well. Never truncate,
reinitialize, rename or delete the registry, even when it is missing or malformed.

If the binding supports an enforced version precondition, apply it to the read
version. On a conflict or uncertain write, read again and reconcile; no blind
retry of stale bytes. If the whole expected update is verified, registration can
complete. Otherwise return published_registry_pending with registry_update_failed
or registry_outcome_unknown. Keep the verified Drive artifact identifiers and all
research errors. Do not roll back the artifact or repeat scoring/publication.

Recovery is outside normal invocation: an operator/host uses the saved artifact,
input IDs and error details to verify publication and append ONLY missing IDs under
the same guard. It must resolve the hold before another job can reuse those IDs.
No new signal_id, artifact, research or paid call is needed. This package does not
pretend a plain ID-history file is a distributed transaction log.

## Binding references

Logical capabilities above must map to actual authorized host tools. The following
are API references. Check that the installed tool exposes the precise read and
in-place content update operations.

- https://developers.google.com/workspace/drive/api/reference/rest/v3/files/get
- https://developers.google.com/workspace/drive/api/reference/rest/v3/files/update
- https://developers.google.com/workspace/drive/api/guides/folder
- https://developers.google.com/workspace/drive/api/guides/search-files
- https://developers.google.com/workspace/drive/api/guides/manage-uploads

## Optional provider metadata

Request trash state and, for a write destination, add-child capability when the
binding exposes them. Explicit trashed=true, canAddChildren=false, access denial,
or unreadable/mismatched identity stops the operation. Missing/null/non-boolean
trash/add flags are unknown: record drive_metadata_unavailable in host execution
notes and continue if mandatory capabilities and identity are available. Do not
infer add permission from sharing/edit flags, repeat unsupported requests or write
a probe file during setup. Unknown permission can still cause a real write denial.

Reapply the policy before a write and when verifying returned resources. Exact
ID/name/type/parent, all-type collision lookup with no observed competing match and full
content verification remain mandatory. A real denial or unverified save is a
failure; never retry an ambiguous write. Missing optional flags do not authorize
skipping stable-version reads, serialization, child settlement or recovery holds.
Keep warnings in the host run notes when the unchanged result contract has no
warning field; do not disguise them as research errors or alter business payloads.

## Search completeness

Use successful, well-formed exact parent/name searches across all file types and
follow every exposed continuation token. An explicit incomplete/truncated result,
unhandled/repeated token, wrong scope or actual error fails; missing completeness
metadata alone does not. Record `drive_lookup_completeness_unavailable` in host
notes and use observed matches without claiming global uniqueness. A wrapper
response containing only results is not required to provide a complete=true flag.
Zero matches permit normal create-only behavior; a reused job folder needs exactly
one observed valid match. Reject observed competing matches and fully verify IDs,
parents and bytes. For a new artifact whose returned ID and full bytes verify,
an empty post-upload search warns `drive_created_file_not_listed`; never re-upload.
An uncertain create/upload without an ID requires one observed verified match.
This exception concerns search enumeration only, never registry completeness,
serialization, recovery holds, folder identity or complete file contents.
