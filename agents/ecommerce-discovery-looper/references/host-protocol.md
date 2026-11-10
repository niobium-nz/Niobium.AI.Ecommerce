# Host protocol: read, coordinate, dispatch

## Required capabilities

The names in agent.yaml describe capabilities, not callable tool APIs. Inspect the
actual host action schemas. Bind read-only Google Drive metadata, complete exact
child search and raw file download, plus a trusted UUID facility and lossless JSON.
Bind the real researcher invocation/terminal-result capability and the shared
coordinator. Reject deployment if any required semantics are unavailable. Do not
invent a native agent-call tool, HTTP endpoint, background job or credentials.

The researcher deployment must use the same discovery_folder_id and its existing
OneDrive registry configuration. Verify the actual registry scope and coordinator
at deployment and before dispatch. A local sequential loop is not a global lock.
The researcher must have its full AGENT.md, references, schemas, three skills,
research tools and write permissions. The looper itself needs no OneDrive access,
paid-ad search, browsing or Google Drive write permission.

## Read one stable upstream artifact

Verify configured folder metadata: exact name discovery, folder MIME, not trashed,
not a shortcut, accessible by the configured ID. No root or name-based fallback.
List exact filename {job_id}.json among direct children, across MIME types and all
pages, with viewed-only restrictions disabled. Require exactly one match. Neither
zero results nor duplicate names permits choosing a nearby file or a job subfolder.

Read metadata before downloading: id, name, parents, mimeType, trashed, version and
size, with checksum when available. Require application/json and the configured
parent. Download full raw bytes using the binding's authenticated file reference,
not excerpts, search snippets, native-document export or inline base64 guesses.
Read metadata again and require unchanged identity, name, parent, type, version,
size and checksum. Compare actual byte length to declared size and checksum when
provided. A binding offering an immutable revision read can supply equivalent
verified guarantees. Compute SHA-256 of the complete bytes for the journal/result.
If stability/completeness cannot be established, fail before any researcher call.

Freeze the validated source snapshot. Never re-read a changing file between items
and merge new entries into an in-progress loop. Later source edits require a new
explicit run after any outstanding recovery has been resolved.

## Shared serialization and durable dispatch records

Use ONE verified coordinator domain for all product jobs that share the registry,
including independent researcher invocations and other hosts. Its admission/guard
handoff must cooperate with the researcher's existing serialization protocol, not
hold a separate non-reentrant lock while the child tries to acquire the same lock.
Global serialization covers duplicate check through publication and ID verification.
A child may still fan out its keyword searches internally; product concurrency is 1.

Before EACH dispatch, check pending recovery holds and capacity, then durably record
loop ID, source ID/version/digest, source index, cluster ID, exact child payload or
lossless recoverable reference, researcher deployment/version and registry scope.
The dispatch intent must exist before sending. Record any returned invocation ID.
An open intent with unknown completion is a recovery condition, not permission for
another attempt. Bind retries off in the transport/coordinator as well as prompts.

Wait for the exact invocation to become terminal and quiescent; a visible answer
without that guarantee is insufficient. Validate and durably record the result
before advancing. A task timeout, lost receipt, cancellation or crash may leave a
child running or a candidate published. Mark/retain a durable hold and stop further
product jobs until the operator/coordinator establishes the outcome. Do not claim
cancellation succeeded without confirmation. Release owned execution resources
without clearing unresolved intents/holds; never strand or clear another run's lock.

If journal writes or holding fail, stop all dispatch and report the inability to
establish the hold. A flag in the result is not a successfully persisted hold.
Admission must reject unresolved intents even if the looper's process disappears.
For known preflight failures with no child side effects, report the infrastructure
failure without inventing a published artifact.

## Recovery and outputs

Record a valid published_registry_pending response with its original file identifiers
and child errors. Registry recovery is owned by the existing researcher protocol:
verify the prior artifact and append only missing input IDs under the shared guard.
The looper must not rerun normalization/research/scoring or republish that product.
An uncertain researcher invocation is reconciled by its durable intent and actual
host record, never by creating another signal ID. Resuming remaining items is a
separate explicit coordinator/operator action on the pinned source and journal;
there is no public automatic resume flag in this package.

Keep loop summaries/journals in durable host task storage. Do not create new Drive
sidecars or repurpose the OneDrive ad-history file as a coordinator log. The child
alone publishes discovery/{job_id}/{signal_id}.json and updates processed IDs.
Cross-provider writes are not atomic; no exactly-once claim is made by this package.

## Integration references checked 2026-11-11

These describe provider primitives, not proof that a particular connector exposes
them or that a deployment has passed the host acceptance checklist.

- https://developers.google.com/workspace/drive/api/reference/rest/v3/files
- https://developers.google.com/workspace/drive/api/guides/search-files
- https://developers.google.com/workspace/drive/api/guides/manage-downloads
