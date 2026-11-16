# Host capability bindings

Inspect the actual connected schemas at deployment and runtime. Logical names
in agent.yaml are not callable tools or a universal MCP schema. Missing semantics
must fail preflight, before paid search. Do not invent connector actions.

## ScrapeCreators

The authoring-session action was v1_facebook_adLibrary_search_ads, provider route
/v1/facebook/adLibrary/search/ads. Host prefixes may differ. The inspected schema
accepts query, sort_by, search_type, ad_type, country, status, media_type,
start_date and cursor. Use AGENT.md's eight page-one fields and only add a cursor
for permitted pagination. Do not use the unrelated Meta Ads account connector.

Request start_date is an impressions-window parameter, not the returned record's
actual start date. Verify actual is_active and date independently. Interpret the
real successful searchResults envelope, not example/default values. Three
attempts is a budget, not a promise of free requests; embed no pricing guarantee.

Source checked during authoring:
https://docs.scrapecreators.com/v1/facebook/adlibrary/search/ads/

## Google Drive

Example bindings exposed during authoring; verify their full semantics per host:

| Capability | Example action | Requirements |
| --- | --- | --- |
| Metadata | get_file_metadata | ID-based name, MIME, parent; request trash/add permission/checksum when exposed |
| Exact child lookup | search | Exact parent/name, all MIME types, no viewed-only restriction, complete pagination |
| Raw upload | upload_file | File bytes, explicit parent, exact filename, application/json |
| Complete readback | fetch raw-file mode | Full bytes, not a bounded text excerpt |

Example Drive-v3 filter:

```text
'<configured_folder_id>' in parents and name = '<job_id>.json' and trashed = false
```

The fields requested from Drive and the fields returned by a connector are not
necessarily identical. Some wrappers omit `trashed` and `capabilities.canAddChildren`.
Request them once where supported. Missing/null/non-boolean values are unknown:
warn with `drive_metadata_unavailable` and continue if identity and mandatory
actions are available. Explicit trash or add-permission denial still stops the
run. Do not equate `canShare`, `canEdit` or account ownership with add permission.
Apply this policy both at preflight and immediately before upload. A missing
file trash flag after upload also warns; exact ID/name/MIME/parent, complete bytes
and unique exact-name lookup remain mandatory for a verified save. Permission can
change after preflight, so even a positive flag is not a guarantee of a write.
An actual write denial fails the job, with no retry or success artifact.

Keep the server-side non-trashed search filter when supported. A broader complete
lookup that includes trashed children is safe but conservative: any exact-name
match still blocks a new write. Never drop a match because its trash flag is absent.
The exception for optional metadata does not permit incomplete collision searches,
missing parent/identity, unreadable content or unverified uploads.

Escape query literals using connector rules; do not interpolate ad-controlled
values. The inspected search wrapper exposes special_filter_query_str. Use an
empty keyword query, best_effort_fetch false and require_viewed_by_user false.
Do not impose a document/image/folder type filter; any same-named child blocks a
write. Follow actual opaque page tokens. If the wrapper cannot establish complete
all-types lookup, require a suitable authorized binding or fail preflight.

The inspected upload_file accepts file_uri, parent_folder_id, file_name and
mime_type. Create real local UTF-8 bytes and pass the actual file reference the
host requires. Never put JSON text/base64 in file_uri. Native create_file for
Docs/Sheets/Slides is not a raw JSON writer. Do not substitute a Google Doc.

Before raw readback, inspect metadata and actual fetch schema. Use its raw-file
option and authenticated full-file reference, not best-effort text. Retrieve
only this job's output or the matching file being reconciled after an uncertain
write. Request size/md5Checksum for binary verification when exposed; compare
against the exact upload bytes. Metadata alone cannot establish content equality.
Do not synthesize unobserved webViewLink values.

Drive names are not unique and lookup-then-upload is not atomic. UUIDs reduce
collision risk but do not create an atomic lock. Reconcile uncertain uploads
read-only without retrying, reservation files or cleanup. No folder creation,
sharing, delete, move or update_file capability is needed. Constrain writes to
the configured discovery folder wherever the host supports enforceable limits.

API references:
https://developers.google.com/workspace/drive/api/reference/rest/v3/files
https://developers.google.com/workspace/drive/api/guides/search-files
https://developers.google.com/workspace/drive/api/guides/manage-uploads

## Utilities and resource access

Require a real clock, IANA timezone conversion, UUIDv4 generation, lossless JSON,
local bytes/equivalent raw upload, and all host-selected skill resources. A real
Draft 2020-12 validator with date/date-time format checks and local reference
resolution is required for JSON Schema checks; JSON parsing is insufficient.
Python is one supported implementation, not a platform requirement. Installation
kits include an offline Python validator with its dependencies for the documented
Linux targets. Another host may supply an equivalent validator and run the same
valid/invalid fixtures. Do not attempt online dependency installation in the host.
`tests/contract_checks.py` is an offline reference, not a production connector
client or replacement for the clustering skill. See `skill-binding.md` for
resource resolution and `validation.md` for evidence levels and setup checks.
