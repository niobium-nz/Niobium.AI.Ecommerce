# Google Drive publication and host capabilities

## Preflight

Resolve logical capabilities from the actual authorized host tool schemas. These
instructions are not callable tool names. Require candidates_folder_id to resolve
to one existing folder named exactly candidates with matching ID/type; apply the
optional metadata policy below when trash/add flags are unavailable.
Do not find a replacement by name, use root, follow shortcuts, or create the root.
Verify correctly scoped exact-name child lookup across all MIME types and exposed pages, folder create,
raw JSON upload with explicit parent and full content verification before browsing.
Read-only search or native Doc creation is insufficient. Keep host write approvals.

Use the profiler's localized Firecrawl workflow and its bounded link queue.
The board reader must inspect supplied pixels without generating
an image. No paid ad tools, OneDrive registry or researcher/looper binding is needed.

## Folder and file resolution

Only after both strategies validate, allocate one lowercase UUIDv4 candidate_id.
Use a supplied signal_id as the exact subfolder name; otherwise use unlinked.
List direct children of the configured root by exact name across all types and exposed pages.
One valid folder with no explicit trash indication may be reused. No matches permits creation of exactly
that child. Multiple matches, a same-named nonfolder/shortcut or known incomplete search
stops. Never use a guessed sibling, signal_id (1), or overwrite an upstream artifact.

Verify created/reused folder metadata, parent and one observed name match. Serialize
folder resolution/create per parent/name through a host guard when concurrent
runs are possible; a Boolean flag or model promise is not a distributed lock.
Without that capability, deployment must serialize these runs. On uncertain create,
reconcile via read-only lookup; never blindly repeat it or delete a folder.

Check {candidate_id}.json absence across all MIME types immediately before writing.
Any collision stops; do not regenerate ID or overwrite. Serialize the entire
schema-valid object to UTF-8 application/json, not a Doc, Sheet, Markdown or wrapper.
Use actual bytes and a real local/authenticated file reference as the upload schema
requires; JSON/base64 text must not masquerade as a file URI. Upload exactly once.

## Verify or report uncertainty

Read returned file metadata and require exact name, parent, application/json,
trash state when exposed and no observed competing exact-name result. Compare complete readback bytes or
an exposed provider checksum to the actual serialized upload bytes. A snippet or
successful upload response alone does not verify content. Metadata and bytes must
refer to the same file/version. Recheck the board's durable source identity before
writing; no extra board or profile sidecar file is created.

On timeout/uncertain upload, reconcile read-only using the same filename, candidate_id
and bytes. Success requires one identified, fully verified artifact and no observed
competing match. Otherwise return
publication_outcome_unknown/verification_failed, candidate_id and sanitized attempted
path/observed ID in details; verified output fields stay null. Do not repeat the
upload or rerun skills. Do not delete a possibly published artifact or alter sharing.
Drive names are not unique and lookup-then-write is not atomic; never promise exactly
once. Operator reconciliation is required before retrying an ambiguous run.

## Retrieval cleanup

Ordinary Firecrawl scrapes are single requests. If a run opened an Interact
session, stop that specific session and check the result on success, failure or
cancellation. Record unverified cleanup in profile diagnostics. Keep unrelated
sessions and the persistent Firecrawl connection intact.

## API references checked during authoring (2026-11-12)

These describe APIs, not a guarantee that a particular connector exposes them:
https://developers.google.com/workspace/drive/api/guides/folder
https://developers.google.com/workspace/drive/api/guides/search-files
https://developers.google.com/workspace/drive/api/guides/manage-uploads

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

A successful exact parent/name search with no completeness metadata records
`drive_lookup_completeness_unavailable` in host notes and continues using observed
matches. It does not prove exhaustive absence or global uniqueness. Follow all
returned tokens, even for empty pages; known truncation, malformed responses,
errors or an unusable token remain failures. Do not narrow to a file type merely
to obtain a pagination flag. A reused child folder needs one observed valid match;
zero matches permit its normal create-only path. An observed competing match stops.
For a new artifact, verify the actual returned ID, metadata and complete bytes.
If it is missing from post-upload search, warn `drive_created_file_not_listed`
without re-uploading. An uncertain create/upload without a returned ID still needs
one observed fully verified match. Missing search evidence never excuses incomplete
content, wrong parents, Firecrawl session cleanup failures or missing caller approval.
