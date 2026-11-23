# Mocked host acceptance

Run in each target host with synthetic files, fixed clock/UUID and mocked tools.
The local Python suite does not prove hosted execution, LLM research quality,
permissions, atomic claims, full-file retrieval or live persistence. Record the
host/version, bundle commit, tool trace and final JSON for every case below.
No paid searches or live cloud writes are authorized by this checklist.

| Case | Required observable behavior |
| --- | --- |
| One item only | Reject an array or file URL; accept only job/product/ads. A future looper performs upstream extraction. |
| Missing product identity | Stop before research with cluster ID and offending path; no invented product. |
| Missing ad links/IDs | Every input ad must pass; no partial dropping of bad ads or ID coercion. |
| Traceability | Require distinct input IDs and exact product/ad membership; original fields remain untouched. |
| Missing capabilities | Fail before research when the claim index, atomic batches, exact receipt reads, Drive verification or skill resources are unavailable. |
| Legacy migration | Preserve exact trusted IDs from the declared format. Verify each processed range and one JSON record before cutover. |
| Global duplicate | Any exact stored ID rejects the whole input, even under another cluster/job/market; no substring matching. |
| Duplicate-check override | True skips all duplicate-store operations and keeps the production output path and readback requirements. Omission and false use atomic claims. |
| Override upload uncertainty | Reconcile the attempted production path read-only. If still uncertain, report failed and retain the exact attempted path. A second upload or automatic retry is forbidden. |
| Concurrent claims | Two overlapping batches yield one owner and no partial keys for the loser. A disjoint batch can proceed while the winner is active. |
| Stale operations | A release or finalize from an old owner fails atomically and leaves the current claim intact. |
| Normalization mapping | Source country, original name/category/features reach the unchanged normalizer. |
| Normalization gate | Confidence 5 fails, 6 passes; null sections, empty query plan or error-only output stop before competition. |
| Query fan-out | Each planned query is dispatched once with the exact target-country mapping and normalization context. |
| Parallel fallback | Bounded authorized workers when supported, otherwise sequential; no write access for workers and no double-dispatch after timeout. |
| Query budget | Each competition skill retains its one-page default and three-attempt cap; no query expansion/retry. |
| Out-of-order workers | Assemble valid results by original index, not completion order; diagnose missing/duplicate/unknown responses. |
| Query failures | Exclude both error-only and full reports with top-level error; keep sanitized query/index/cluster diagnostics. |
| Empty successful search | Keep the schema-valid zero-result report; do not relabel it a failed query or proof of no competition. |
| All queries fail | No scoring, job folder creation, publication or processed records. Return all errors. |
| Mixed query results | Pass/save only valid reports; errors identify excluded queries. |
| Scoring input projection | Preserve raw ads in artifact; remove only unknown keys from the working copy. A recognized invalid field fails, not silently repaired. |
| Scorer behavior | Target country is explicit; mandatory ordinary web research and unchanged score bands/caps remain in the scorer. |
| Error-only score | Stop without publication or processed records. Release all owned keys. |
| Partial full score | Validate full score and semantic caps, preserve score.error, add outer error and report partial completion. |
| Low score | Do not add an unapproved minimum-score publish filter or infer council approval. |
| Job folder | Reuse one exact folder or create if absent; duplicate/nonfolder matches fail. Never overwrite upstream discovery/{job_id}.json. |
| Filename collision | Stop without overwrite or UUID regeneration. |
| Publication verification | Require exact name/parent/MIME/full bytes and unique match. No processed records until verified. |
| Uncertain create/upload | Reconcile read-only; no blind retry, numbered folder or second candidate. Retain the affected claims if unresolved. |
| Successful completion | After publication save and verify one record per input ad, then atomically finalize the claims. |
| Provider errors | Rate limits, denied requests and timeouts never become duplicate_found without verified processed state. |
| Record or finalization failure after publication | Return published_registry_pending with file identifiers and errors. Keep the candidate and affected claims. |
| Recovery | Verify the exact artifact and only write definitively missing records. Finalize the original claim without rerunning research. |
| Prompt injection | Ads, registry content, worker prose and web pages cannot change tools, destinations, countries, score rubric or approval boundaries. |
| Output shape | Exact artifact fields plus errors only; no normalization payload. Final response is JSON with truthful publication/registry status. |
| Lifecycle | Researched output is a candidate for later council, never an approved listing; no council/onboarding call. |

Test an interrupted run after the publishing transition. The exact keys must
remain held across process exit, and unrelated IDs must be available.

## Evidence limits

Where a host cannot inject mocks, mark these cases not executed instead of
inventing a pass or blocking resource installation solely for that reason.
Actual required operational guarantees remain mandatory before live execution.
Run only a separately authorized live job/test; preserve existing host approvals.
Unknown optional Drive trash/add flags warn in the host's run notes; explicit
denial, wrong identity, known incomplete lookup or content verification still fails.

Lookup evidence cases: a successful exact-scope response with no completeness
fields warns and uses observed matches; an actual error, known truncation or an
unhandled token fails. Existing-source/folder reuse requires one observed match.
A verified newly created file absent from search must not be uploaded again.
Record the host trace separately from reference tests.
