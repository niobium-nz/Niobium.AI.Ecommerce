# Candidate storage and verification

Bind the configured `commerce_folder_id` and its direct child `discovery` by ID.
Require folder MIME and exact identity. Apply the optional metadata policy below.
Use no root or name fallback. Before research when duplicate checking is enabled,
bind the sibling records folder and claim index described in duplicate-records.md.
Explicit duplicate-check override skips that index and the per-ID records.
Both modes enforce the same destination and readback rules.

## Google Drive publication

After a valid score, generate one UUIDv4 signal_id with a trusted host facility.
Use the validated upstream UUID as the exact job folder name. Search direct
children of the configured discovery folder in either mode. Check the exact
child name across all file types and every exposed page.
Reuse one verified folder with no explicit trash indication. Zero matches
permits creation of the exact folder. A nonfolder match, multiple matches or
known incomplete lookup is an error.
Do not overwrite discovery/{job_id}.json: it is the upstream ARRAY file, while
this agent writes inside a job_id FOLDER.

Create a missing job folder only when ready to publish. In normal mode, use
the duplicate adapter's `claim_job_folder` operation with the same claim ID
and input. One index entry per job authorizes one create attempt. Verify the
exact owner's receipt and recheck exact folder absence before creating. If another run already holds it, look
up the expected folder again and reuse it only after verification. Stop if
it remains unresolved. Preserve the entry across interruption and resolve a
failed or uncertain create before an operator clears it. In override mode
reuse a verified folder or use a pre-provisioned destination when concurrent
creation is possible. Verify new folder metadata and one observed exact-name
match. Reconcile an uncertain create by listing. Never retry the create blindly,
use a numbered folder or change an existing folder's sharing.

Check exact {signal_id}.json absence, all MIME types, before writing. Any collision
stops; do not regenerate IDs or overwrite. Serialize the validated artifact as raw
UTF-8 application/json. In normal mode, atomically enter publishing and verify
the claim receipt with the exact path identifiers and artifact SHA-256 before
any upload. Upload once to the verified folder. Verify returned ID,
name, parent, MIME, trash state when exposed, full content/checksum and no observed
competing exact-name match. A snippet or success response alone is insufficient. On ambiguous upload,
reconcile read-only using the same ID/name/bytes; never retry the write. If still
uncertain, return failed with publication_outcome_unknown and the attempted path,
signal ID, observed file ID if available and claim ID if applicable. Keep the
publishing claim for the affected ad IDs. Do not delete or recreate the file.
Drive names are not unique; list-then-create is not an atomic uniqueness guarantee.

## Binding references

Bind the actual authorized host tools for exact metadata/search, raw upload
and byte readback. These references describe the provider operations.

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
skipping exact identity, full readback or claim verification.
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
This exception concerns search enumeration only. Folder identity, full file
contents and authoritative claim results still require verification.
