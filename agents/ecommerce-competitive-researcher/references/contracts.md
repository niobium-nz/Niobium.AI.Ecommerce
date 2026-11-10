# Contracts and skill boundaries

## One upstream record, not a file or batch

The future looper reads discovery/{job_id}.json, selects one array entry and maps:

```text
job.job_id         <- entry.job_id
job.source_country <- entry.inputs.source_country
job.target_country <- entry.inputs.target_country
job.keyword        <- entry.inputs.keyword
product            <- entry.product (unchanged)
ads                <- entry.ads (unchanged)
```

The public input has exactly job, product and ads. Upstream provenance is not
silently repurposed as target-market evidence. No looper is built here.

Validate strict JSON, including duplicate keys and non-JSON numbers. Job ID is the
upstream lowercase UUIDv4. Countries are assigned alpha-2/alpha-3 codes from the
bundled normalizer schema/lookup; do not infer location or rewrite saved strings.
The keyword is nonblank. Product retains its ten upstream fields, must be physical
(is_product true/is_service false), have a nonblank cluster_id/likely_product_name
and nonempty distinct ad_archive_ids. Ads is nonempty; each ID and snapshot.link_url
is a nonblank string. Reject missing, null or wrong-type values, duplicate input
IDs and product/ad membership mismatches. Do not check link reachability as a new
qualification rule. Preserve original ads and their ordering/unknown provider keys.
Error details always carry the recoverable product.cluster_id, or null for malformed
input, plus affected IDs/paths. Never fabricate a missing correlation value.

## Skill ownership and mappings

Read each bundled SKILL.md and all its required references when invoking it. Its
schema and substantive rules remain authoritative. The orchestrator performs the
additional agreed gates, error exclusion, mapping and persistence only.

| Skill | Mapping |
| --- | --- |
| ecommerce-normalize-products | product_name <- likely_product_name; category_name <- category_guess; known_features <- known_features; country <- job.source_country |
| ecommerce-analyze-competition | query <- each recommended_mcp_queries element; country <- job.target_country; category_name <- original category_guess; notes <- notes_for_downstream_agent; exclusions <- keyword_plan.avoid_or_exclusion_terms; product_interpretations <- product_interpretations |
| ecommerce-score-products | target_country <- job.target_country; product <- original product; ads <- schema-compatible input copy; competition_signals <- ordered valid reports |

Normalization: validate the entire result and exact product/category/features echoes.
Reject error-only/malformed output. Then require normalization.confidence0_to10 >= 6
and at least one recommended_mcp_queries item. Null sections also fail. Do not use
confidence_0_to_10, treat list count as an object property, or impose status == ok
as an additional gate. Preserve ambiguous interpretations/notes when the approved
gates pass. No normalized_product field is saved.

Ads projection: upstream deliberately preserves provider fields that the scorer's
additionalProperties:false schema cannot accept. Copy only recognized keys
recursively using the scorer input schema; preserve their exact values and array
members. Report omitted paths in working diagnostics. Do not mutate originals,
coerce recognized values, discard invalid recognized fields or change ad identity.
Validate the complete scoring input before invocation. If a recognized value is
invalid, stop rather than silently repairing it. The saved ads are ALWAYS originals.

## Fan-out and error collection

Give each query its original zero-based index. Invoke all queries once; do not cap,
expand, translate, trim or reorder the plan. With N queries, the competition skill's
maximum is 3N attempted paid searches; its default remains one page per query.
Disclose this bounded budget before execution where the host requires approval.
Never claim costs or infer that failed calls are free. Do not retry failed workers.

Use at most max_parallel_queries authorized workers, each with the full competition
skill/resources, trusted run-time context and required search access. Freeze one
trusted job clock/timezone context; workers use that basis for their skill's cutoff.
Do not add unsupported fields to skill input. Workers cannot publish or modify the
registry. Missing subagent capability means sequential execution, not failed work.
If a worker may already have executed, an ambiguous dispatch outcome is a query
failure, not permission to run it again. Await all launched workers before scoring;
reassemble by index, not completion order. Missing/duplicate/wrong-index responses
must be diagnosed, never silently dropped or accepted twice.

Each valid report must pass the competition schema and correlate to its exact
query and target country (including the skill's country echo). A top-level error
ALWAYS excludes that report, including otherwise usable partial reports. Legacy
mcp_error is not an accepted alias. Schema errors and query/country mismatch also
exclude the response. Diagnostics identify stage, original query index/text,
cluster ID and sanitized cause. An explicit valid zero-result report is retained;
missing evidence is not zero competition. Preserve valid report bodies unchanged.

If the valid subset is empty, stop with all_competition_queries_failed and query
errors; do not invoke the scorer or create output/registry entries. Otherwise give
only that subset to the scorer. Overlapping queries are not independent market
counts; the scorer's existing compatibility rules govern interpretation.

## Score, artifact and terminal result

The scorer must perform its required current target-country web research. Its
complete result is score, including score.error on partial research failure.
Error-only/no score, schema failure, country mismatch or a failed semantic
sum/cap/priority check prevents publication. Delegate the substantive rubric to
the skill and check against its scoring-model/rules; do not invent new weights.
A valid full score with error is preserved, with score_partial_failure in outer
errors. Low scores are not operational errors and do not introduce a publish cutoff.

The artifact has EXACTLY ten fields: job_id, signal_id, keyword, source_country,
target_country, product, ads, competition_signals, score, errors. Job correlations,
product and ads equal input. It has at least one valid report and a complete score.
It has no normalization payload, lifecycle metadata or unrequested envelope.
Its business role is a candidate awaiting council, never an approved listing.

errors is always an array of {code, stage, product_cluster_id, message, details}.
Do not include secrets, raw connector traces or full unrelated source payloads.
Use details for likely_product_name, confidence0_to10, query_index, query, ad IDs,
paths and observed file IDs. Final result errors include all accumulated errors.
The artifact's errors are the research/validation diagnostics known at publication;
a later registry failure is returned in the terminal result, not hidden by rewriting
the already published JSON. The caller must retain that result for recovery.

| Result status | Verified artifact | Registry IDs verified | Meaning |
| --- | --- | --- | --- |
| failed | No | No | Validation, research, scoring or publication failed |
| succeeded | Yes | Yes | No research errors |
| partial | Yes | Yes | Usable candidate with recorded research errors |
| published_registry_pending | Yes | No | Publication succeeded; registration needs reconciliation |

signal_id is allocated only after a valid score. Keep it on subsequent failures.
output_filename/output_file_id/output_path are non-null only for verified output.
For verified publication, output_filename must equal signal_id + .json and
output_path must equal discovery/{job.job_id}/{signal_id}.json. Check these
cross-field equalities, not just schema types. For uncertain publication, put
attempted path/observed ID in error details instead.
No automatic rerun after uncertain publication or registration. A completed job
status never means product council approval. Output JSON only during execution.
