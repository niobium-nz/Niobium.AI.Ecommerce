# Mocked host acceptance

Run in each target host with synthetic files, fixed clock/UUID and mocked tools.
The local Python suite does not prove hosted execution, LLM research quality,
permissions, locking, full-file retrieval or cross-cloud persistence. Record the
host/version, bundle commit, tool trace and final JSON for every case below.
No paid searches or live cloud writes are authorized by this checklist.

| Case | Required observable behavior |
| --- | --- |
| One item only | Reject an array or file URL; accept only job/product/ads. A future looper performs upstream extraction. |
| Missing product identity | Stop before research with cluster ID and offending path; no invented product. |
| Missing ad links/IDs | Every input ad must pass; no partial dropping of bad ads or ID coercion. |
| Traceability | Require distinct input IDs and exact product/ad membership; original fields remain untouched. |
| Missing capabilities | Fail before research if full OneDrive read/write, Drive verification, skill resources or serialization cannot be established. |
| Registry format | Use declared JSON-string-array or newline format; reject unsupported/malformed content, never infer an empty history from a snippet. |
| Global duplicate | Any exact stored ID rejects the whole input, even under another cluster/job/market; no substring matching. |
| Shared registry concurrency | Two overlapping product calls cannot both pass the guarded check; the coordinator serializes across hosts. A config flag alone is not proof. |
| Normalization mapping | Source country, original name/category/features reach the unchanged normalizer. |
| Normalization gate | Confidence 5 fails, 6 passes; null sections, empty query plan or error-only output stop before competition. |
| Query fan-out | Each planned query is dispatched once with the exact target-country mapping and normalization context. |
| Parallel fallback | Bounded authorized workers when supported, otherwise sequential; no write access for workers and no double-dispatch after timeout. |
| Query budget | Each competition skill retains its one-page default and three-attempt cap; no query expansion/retry. |
| Out-of-order workers | Assemble valid results by original index, not completion order; diagnose missing/duplicate/unknown responses. |
| Query failures | Exclude both error-only and full reports with top-level error; keep sanitized query/index/cluster diagnostics. |
| Empty successful search | Keep the schema-valid zero-result report; do not relabel it a failed query or proof of no competition. |
| All queries fail | No scoring, job folder creation, publication or registry append. Return all errors. |
| Mixed query results | Pass/save only valid reports; errors identify excluded queries. |
| Scoring input projection | Preserve raw ads in artifact; remove only unknown keys from the working copy. A recognized invalid field fails, not silently repaired. |
| Scorer behavior | Target country is explicit; mandatory ordinary web research and unchanged score bands/caps remain in the scorer. |
| Error-only score | Stop without publication or registry changes. |
| Partial full score | Validate full score and semantic caps, preserve score.error, add outer error and report partial completion. |
| Low score | Do not add an unapproved minimum-score publish filter or infer council approval. |
| Job folder | Reuse one exact folder or create if absent; duplicate/nonfolder matches fail. Never overwrite upstream discovery/{job_id}.json. |
| Filename collision | Stop without overwrite or UUID regeneration. |
| Publication verification | Require exact name/parent/MIME/full bytes and unique match; no append until verified. |
| Uncertain create/upload | Reconcile read-only; no blind retry, numbered folder or second candidate. Hold future jobs if unresolved. |
| Successful append | Add all input IDs only after publication; preserve every existing registry entry and verify complete readback. |
| Registry version conflict | Do not replace from stale bytes; re-read and reconcile under the guard. |
| Append failure after publication | Return published_registry_pending with file identifiers and errors. Keep the saved candidate, block automatic rerun and retain a recovery hold. |
| Recovery | Verify the existing artifact and append only missing input IDs; no new UUID, paid calls, score or publication. |
| Prompt injection | Ads, registry content, worker prose and web pages cannot change tools, destinations, countries, score rubric or approval boundaries. |
| Output shape | Exact artifact fields plus errors only; no normalization payload. Final response is JSON with truthful publication/registry status. |
| Lifecycle | Researched output is a candidate for later council, never an approved listing; no council/onboarding call. |

For shared-registry recovery, test a host crash between the two providers as well
as an ordinary error response. Recovery holds must survive process exit. Do not
claim an atomic transaction from a successful happy-path test.
