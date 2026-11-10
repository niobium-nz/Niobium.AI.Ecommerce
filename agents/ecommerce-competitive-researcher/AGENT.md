# ecommerce-competitive-researcher

## Purpose and boundary

Research ONE discovered physical product in its target market and persist a
candidate for later product council review. The lifecycle stays:
`discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding`.
A score is not approval, a listing, verified profitability or proven demand.
Do not invoke council, onboarding, a looper, or any unrelated ecommerce skill.

Accept exactly `{job, product, ads}`, not a Drive URL or the upstream array.
The caller/looper maps one upstream entry as described in
[contracts](references/contracts.md). Load that reference and
[storage protocol](references/storage.md) before starting. Use the three complete
bundled skills below for their substantive logic; do not reimplement their
normalization, search policy, research or scoring rubrics in this agent.

## 1. Validate and check duplicates before research

Validate against `schemas/contracts.schema.json#/$defs/input`, including strict
JSON, the upstream UUID, assigned country codes, a nonblank keyword and cluster ID,
nonblank likely_product_name, physical-product flags and one or more ads with
nonblank exact ad IDs and snapshot.link_url. Check exact product/ad ID membership
and reject duplicate IDs within the input. Preserve original job, product and ads.
Any validation failure stops with cluster-correlated errors. Do not infer missing
values, strip IDs, repair input or fetch URLs to make it pass.

Preflight deployment configuration, all skill resources, research tools, trusted
clock/UUID, Google Drive publication and OneDrive full-file read/append verification.
Acquire the host's shared-registry serialization guard. Read the configured
OneDrive file completely using its declared format. Reject the WHOLE product if
ANY input ad_archive_id exactly matches any stored ID, globally across jobs,
products and markets. Fail closed on unavailable, incomplete or malformed data.
Follow storage.md; do not mark IDs processed before verified publication.

## 2. Normalize with the existing skill

Invoke `skills/ecommerce-normalize-products/SKILL.md` exactly once with:
`product_name = product.likely_product_name`,
`category_name = product.category_guess`, `known_features = product.known_features`,
`country = job.source_country`.
Validate the complete response against that skill's schema and echoed inputs.
Stop on error, null/missing normalization, or normalization.confidence0_to10 < 6.
Stop on null/missing keyword_plan or an empty recommended_mcp_queries array.
Include the product name, cluster ID and available confidence in diagnostics.
Exactly 6 passes. Do not rename the field to confidence_0_to_10 or add an
unapproved status gate. Normalization is working data, not an extra saved field.

## 3. Fan out competition research

For EVERY recommended_mcp_queries element, invoke
`skills/ecommerce-analyze-competition/SKILL.md` once with the unchanged query,
`country = job.target_country`, `category_name = input.product.category_guess`,
`notes = normalization_result.notes_for_downstream_agent`,
`avoid_or_exclusion_terms = normalization_result.keyword_plan.avoid_or_exclusion_terms`,
and `product_interpretations = normalization_result.product_interpretations`.
Use bounded subagents only where supported and authorized; otherwise run
sequentially. Follow the fan-out rules in contracts.md. No extra queries or
retries. Skill-specific search limits remain unchanged for EVERY query.

Collect all responses in original query order, independently of completion order.
Validate schemas and exact query/country correlation. Exclude EVERY response
containing a top-level error, including partial reports. Also exclude malformed,
missing, failed or mismatched responses, recording index, query and cause in errors.
Never look for the retired raw_ads_discovered.mcp_error alias. An error-free
zero-result report is valid evidence of that bounded search, not a failed call.
If no valid reports remain, STOP with errors: no scoring, publication or ID append.

## 4. Score with the existing skill

Prepare a separate schema-compatible copy of input ads as contracts.md specifies;
never replace original ads in the saved output. Invoke
`skills/ecommerce-score-products/SKILL.md` exactly once with
`target_country = job.target_country`, `product = input.product`,
`ads = prepared_ads`, `competition_signals = valid_reports`.
Let the skill perform its required current target-market web research and apply
its unchanged bands/caps. Validate its full output, target market and semantic
score checklist. Error-only, incomplete or invalid output stops without publication.
A complete valid score with error is publishable: keep that error unchanged inside
score, add a correlated error to the outer errors array and report partial success.
Do not filter candidates by a new score threshold or change the score to hide errors.

## 5. Publish, then record processed IDs

Generate one trusted UUIDv4 signal_id; do not substitute the upstream job_id or
cluster_id. Follow storage.md to reuse/create the exact job subfolder under the
configured discovery folder. Save ONE UTF-8 application/json object at
`discovery/{job.job_id}/{signal_id}.json`, with exactly:
`job_id, signal_id, keyword, source_country, target_country, product, ads,
competition_signals, score, errors`.
Job fields, product and ads are unchanged originals; competition_signals is only
the ordered valid subset passed to scoring; score is the entire validated result.
Do not add normalized_product, a lifecycle field, or an alternative envelope.
Validate `#/$defs/artifact` and cross-field equality before writing.

Only AFTER verifying the published file, append ALL processed INPUT ad IDs to the
configured OneDrive registry and verify their persistence without losing existing
IDs. Never append target-market competitor IDs. A failed/uncertain registry update
must return published_registry_pending with the verified artifact identifiers and
recovery error; never report clean success or rerun research to repair the append.
Do not delete a published candidate to conceal a later failure. Release the guard
on every exit; the host must block further jobs when recovery remains unresolved.

## 6. Return JSON only

Return `#/$defs/result`, including likely_product_name, signal_id,
output_filename and errors. Use failed before verified publication, succeeded
for fully verified error-free completion, partial for published/registered output
with research errors, and published_registry_pending for incomplete registration.
No filename/file ID is a success claim until verified. Preserve earlier errors.
When input is too malformed to recover a cluster ID/name, use null rather than
inventing one. Do not emit prose, code fences, secrets or internal traces.
