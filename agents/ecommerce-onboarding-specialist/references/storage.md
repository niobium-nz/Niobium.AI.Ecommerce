# Google Drive publication and host capabilities

## Preflight

Resolve logical capabilities from the actual authorized host tool schemas. These
instructions are not callable tool names. Require candidates_folder_id to resolve
to one existing, non-trashed folder named exactly candidates with read/add access.
Do not find a replacement by name, use root, follow shortcuts, or create the root.
Verify complete exact-name child lookup across all MIME types/pages, folder create,
raw JSON upload with explicit parent and full content verification before browsing.
Read-only search or native Doc creation is insufficient. Keep host write approvals.

Use the profiler's real headed browser and visible inspection, with its redirect
scope guards, bounded navigation, popup handling and cleanup rules. Do not use a
scraper/headless client in its place. The optional strategy web research does not
replace profiling. The board reader must inspect supplied pixels without generating
an image. No paid ad tools, OneDrive registry or researcher/looper binding is needed.

## Folder and file resolution

Only after both strategies validate, allocate one lowercase UUIDv4 candidate_id.
Use a supplied signal_id as the exact subfolder name; otherwise use unlinked.
List direct children of the configured root by exact name across all types/pages.
One valid non-trashed folder may be reused. No matches permits creation of exactly
that child. Multiple matches, a same-named nonfolder/shortcut or incomplete search
stops. Never use a guessed sibling, signal_id (1), or overwrite an upstream artifact.

Verify created/reused folder metadata, parent and unique name match. Serialize
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
non-trashed state and unique exact-name result. Compare complete readback bytes or
an exposed provider checksum to the actual serialized upload bytes. A snippet or
successful upload response alone does not verify content. Metadata and bytes must
refer to the same file/version. Recheck the board's durable source identity before
writing; no extra board or profile sidecar file is created.

On timeout/uncertain upload, reconcile read-only using the same filename, candidate_id
and bytes. Success requires one unique exact matching artifact. Otherwise return
publication_outcome_unknown/verification_failed, candidate_id and sanitized attempted
path/observed ID in details; verified output fields stay null. Do not repeat the
upload or rerun skills. Do not delete a possibly published artifact or alter sharing.
Drive names are not unique and lookup-then-write is not atomic; never promise exactly
once. Operator reconciliation is required before retrying an ambiguous run.

## Browser cleanup

Prefer signed-out public browsing. Never log in, subscribe, buy or use webmail.
On success, failure or cancellation, close task-created surfaces and verify closure.
If any authorized account session was actually used, sign it out and verify where
possible before closure. Leave unrelated tabs/accounts and connector connections
untouched. Report failed/unverified cleanup rather than claiming cookies or sessions
were cleared merely by closing a tab. Preserve useful results with the limitation
only when the approved partial-profile gate still passes.

## API references checked during authoring (2026-11-12)

These describe APIs, not a guarantee that a particular connector exposes them:
https://developers.google.com/workspace/drive/api/guides/folder
https://developers.google.com/workspace/drive/api/guides/search-files
https://developers.google.com/workspace/drive/api/guides/manage-uploads
