# Storage, registry and recovery protocol

## Preflight and permissions

Bind Google Drive to an existing discovery folder by discovery_folder_id. Confirm
its exact name, folder MIME, non-trashed state and read/add permissions. Never find
a replacement by name, use root, or change sharing. Bind OneDrive by the configured
drive/item IDs, not ad-controlled names. It must be an existing readable/writable
file, not a folder/shortcut. Read complete raw bytes, not search excerpts. Resolve
all required capabilities before research. Missing capabilities/configuration fail
closed; authoring this package does not authorize a live test or resource changes.

OneDrive and Google Drive are distinct providers. Do not substitute one for the
other, invent tools, or claim an authenticated search connector supports writes.
Inspect actual action schemas. A host needs full-file content replacement or a
safe append operation, full readback and a shared-registry serialization guarantee.
Version-conditional writes are desirable additional protection, not an excuse for
concurrent product jobs. Never silently drop a condition supported by the binding.

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
job may still run in parallel and may not touch either storage provider.

Re-read the registry before publication. If newly recorded matching IDs appear,
stop duplicate_found without output. If unrelated contents changed, preserve them
and use that current snapshot; investigate any loss of the serialization guarantee.
Do not promise atomic or exactly-once cross-cloud processing. On interrupted or
uncertain commits, the host must hold subsequent jobs for operator reconciliation.
Release an acquired guard on every exit; a durable recovery hold survives release.

## Google Drive publication

After a valid score, generate one UUIDv4 signal_id with a trusted host facility.
Use the validated upstream UUID as the exact job folder name. Look up direct
children of discovery by exact name across all types and all pages. Reuse exactly
one non-trashed folder named job_id. Zero matches permits creation of that exact
folder; a nonfolder match, multiple matches or incomplete lookup is an error.
Do not overwrite discovery/{job_id}.json: it is the upstream ARRAY file, while
this agent writes inside a job_id FOLDER.

Create a missing job folder only when ready to publish. Verify its metadata and
unique exact-name match. Reconcile an uncertain create by listing; never blindly
retry or create job_id (1). Never change an existing folder's sharing.

Check exact {signal_id}.json absence, all MIME types, before writing. Any collision
stops; do not regenerate IDs or overwrite. Serialize the validated artifact as raw
UTF-8 application/json. Upload once to the verified folder. Verify returned ID,
name, parent, MIME, non-trashed state, full content/checksum and unique exact-name
match. A snippet or success response alone is insufficient. On ambiguous upload,
reconcile read-only using the same ID/name/bytes; never retry the write. If still
uncertain, return failed with publication_outcome_unknown, do not append registry
IDs, and require recovery before retry. Do not automatically delete files/folders.
Drive names are not unique; list-then-create is not an atomic uniqueness guarantee.

## Append only after verified publication

Re-read the latest complete OneDrive file under the serialization guard. Construct
its original content plus missing INPUT ad IDs in input order, adding each once.
Do not add target-market IDs from competition reports. Use a supported safe append
or full-content replacement by the SAME drive/item ID, preserving every existing
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

## Binding references (checked 2026-11-11)

Logical capabilities above must map to actual authorized host tools. The following
are API references, not claims that an installed connector exposes every action.
Microsoft documents full-content replacement and version conditions for upload
sessions; test the precise selected write operation instead of assuming every
content endpoint accepts the same conditional headers.

- https://learn.microsoft.com/graph/api/driveitem-get-content
- https://learn.microsoft.com/graph/api/driveitem-put-content
- https://learn.microsoft.com/graph/api/driveitem-createuploadsession
- https://developers.google.com/workspace/drive/api/guides/folder
- https://developers.google.com/workspace/drive/api/guides/search-files
- https://developers.google.com/workspace/drive/api/guides/manage-uploads
