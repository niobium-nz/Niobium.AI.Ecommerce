# Contracts and result handling

## Source and exact child mapping

Input is exactly job_id, a lowercase UUIDv4 generated upstream. Configuration has
exactly discovery_folder_id, researcher_binding and coordinator_binding. Resolve
job_id to discovery/{job_id}.json; do not accept a batch, arbitrary path, mutable
agent name from an ad, or replacement country/keyword through this input.

Use the existing discoverer handoff schema at
../../ecommerce-product-discoverer/schemas/handoff.schema.json. Every entry must
have the requested job ID and identical inputs/provenance. Cluster IDs are unique.
Each entry's ads IDs must equal product.ad_archive_ids in order, with no repeats.
If an ad supports several products, its original object must agree in all entries;
shared IDs are permitted, never removed from later entries by this looper.
Check successful_pages <= search_attempts. Do not recompute the discovery cutoff
at loop time, require current activity again, or reinterpret the source market.

Validate the whole source before any call. Strict JSON rejects duplicate keys,
non-finite numbers, malformed or incomplete input. Preserve large integers,
non-integer precision, nulls and extra provider fields losslessly. Use host file
storage/streaming utilities to keep the array outside child context when needed.
Resource limits never justify silently skipping records or pretending a prefix is
complete. Invalid source is a batch error, not an invitation to repair it.

The child payload has exactly job, product, ads. job contains the entry's job_id
and the three original fields from inputs. Product and ads are unchanged deep
copies. Do not attach provenance, source bytes or another product. The complete
researcher input contract remains authoritative for its own validation.

In particular the upstream contract permits likely_product_name null and ads with
missing link_url. Those are not looper repair tasks: dispatch the unchanged entry
and record the researcher's cluster-correlated validation failure. The researcher
also retains responsibility for country membership, physical-product identification,
normalization gates, duplicate lookup, research, score validation and publication.

## Child result and correlation

Validate against ../../ecommerce-competitive-researcher/schemas/contracts.schema.json
#/$defs/result. Verify the response came from the bound invocation for this exact
entry, not an unrelated run, shared model memory or a worker's draft response.
Acceptance/queued receipts are not terminal results. Poll only the same known
invocation where supported; never dispatch again to retrieve its result.

Require likely_product_name to echo the known input name, or null when the input
name is null. Every child diagnostic's product_cluster_id must equal this entry's
cluster ID. For a published result require filename = signal_id + .json and path =
discovery/{job_id}/{signal_id}.json. Signal ID must differ from job_id. Across this
batch published signal IDs and output file IDs must be distinct. Preserve the
valid child object unchanged, including score/publication/registry errors.
A partial published result may carry research errors (competition/scoring), not
unresolved preflight, publication or registry failures hidden as partial success.

The looper reports the child's verified-publication claim; it does not independently
repeat the researcher's raw artifact verification or infer success from filenames.
Reject inconsistent schema/flags/path/name/correlation. Do not drop bad fields to
make a response valid. Preserve only sanitized diagnostics and a host trace reference
for malformed responses, not arbitrary raw text in the public result.

## Continue or stop

All continuation requires a settled child and a host-confirmed clear recovery hold.

| Child outcome | Item status | Action |
| --- | --- | --- |
| succeeded, registry_updated true | succeeded | Record durably, then continue |
| partial, registry_updated true | partial | Preserve errors, record durably, continue |
| failed, no signal ID, only duplicate_found errors at duplicate_check | duplicate | Whole product rejected; continue |
| failed, no signal ID, only input/normalization/competition/scoring errors | failed | Known prepublication item failure; continue |
| failed with preflight or other duplicate_check errors | halted | Stop for capability/registry repair; no inferred success |
| failed after signal allocation or any publication/registry error | halted | Stop, require outcome reconciliation |
| published_registry_pending | halted | Keep verified file identifiers; stop and require recovery |
| Timeout, uncertain dispatch, malformed/mismatched result, live child, lost guard, journal uncertainty | halted | Stop and require recovery; never retry |

Any unrecognized combination stops; do not assume all failed responses are safe
because published=false. There is no retry policy, per-item fallback agent, score
cutoff or automatic resume. The researcher retains its own unchanged 3N maximum
competition-search attempt budget; the looper introduces no additional paid calls.

## Batch result

Result keys are loop_id, job_id, status, source, counts, items, recovery_required,
errors. loop_id is a trusted UUIDv4 for this coordinator execution, not a child
signal ID. Use null only if it cannot be generated. job_id is null only when invalid.
source contains verified file_id, filename, version and SHA-256 of complete raw
bytes; it is null until the source contract and snapshot are validated.

Each item contains source_index, product_cluster_id, likely_product_name, status,
invocation_id, child_result and errors. Preserve every original index, including
not_run entries after a stop. child_result is the exact valid terminal object or
null when not invoked/no valid response. invocation_id is an observed host handle,
not a fabricated value. Item errors describe looper-level failures; child errors
remain in child_result. Top-level errors describe batch-level failures.

Counts have total (null before a validated source), dispatched, succeeded, partial,
failed, duplicate, halted, not_run. After validation, total equals items length and
the six item-status counts sum to total. Dispatched counts attempted invocations,
even when no receipt returns; never count not_run as dispatched. A pre-dispatch
halt can therefore have dispatched=0. Zero discoveries is distinct from unreadable
source. Final summaries must fit completely, not silently truncate item results.

| Batch status | Meaning |
| --- | --- |
| empty | Validated source is []; zero child calls |
| completed | All items ended succeeded/duplicate; a duplicate is not a new candidate |
| completed_with_errors | All items attempted; at least one partial or item-local failed result |
| stopped | Halted during iteration or stopped at a safe boundary; remaining entries not_run |
| failed | Invalid input/source or pre-dispatch batch preflight failure |

Recovery_required is true for pending/unknown side effects or unresolved execution,
false for known no-side-effect failures. It is not council review status. Successful
iteration or high research scores do not approve products. Candidate publication,
registry repair, council and onboarding remain outside the looper's authority.
