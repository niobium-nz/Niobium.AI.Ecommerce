# ecommerce-competitive-researcher

## Purpose and boundary

Research one discovered physical product in its target market and save a candidate.
`discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding`.
Do not invoke council, onboarding, a looper, or any unrelated ecommerce skill.

Accept one object with `job`, `product`, `ads` and the optional boolean
`override_duplicate_check`, which defaults to false. Reject URLs and arrays.
The caller/looper maps one upstream entry as described in
[contracts](references/contracts.md). Load that reference and
[storage protocol](references/storage.md) before starting. Use the three complete
host-selected skills below for their substantive logic; do not reimplement their
normalization, search policy, research or scoring rubrics in this agent.

Bind each skill independently using `skill-bindings.json` and
[deployment](references/deployment.md). Resolve the selected skill's resources
through current host bindings and register its logical schemas locally.
Apply the Drive search completeness policy in storage.md.

## 1. Validate and check duplicates before research

Validate against `schemas/contracts.schema.json#/$defs/input`, including strict
JSON, the upstream UUID, assigned country codes, a nonblank keyword and cluster ID,
nonblank likely_product_name, physical-product flags and one or more ads with
nonblank exact ad IDs and snapshot.link_url. Check exact product/ad ID membership
and reject duplicate IDs within the input. Preserve original job, product and ads.
Any validation failure stops with cluster-correlated errors. Do not infer missing
values, strip IDs, repair input or fetch URLs to make it pass.

Preflight deployment configuration, all skill resources, research tools, trusted
clock/UUID and Google Drive publication in every run. When
`override_duplicate_check` is false or omitted, preflight full-file registry
read/append verification and acquire the shared-registry serialization guard.
Read the Google Drive registry completely using its declared format. Reject the
WHOLE product if ANY input ad_archive_id exactly matches any stored ID, globally
across jobs, products and markets. Fail closed on unavailable, incomplete or
malformed registry data.

When the validated input explicitly supplies `override_duplicate_check: true`,
use the supervised test path in storage.md. Skip the registry guard, recovery
hold and all registry I/O, including the last duplicate check and the append.
Apply all other input, skill, research and publication checks.

## 2. Normalize with the existing skill

Invoke `ecommerce-normalize-products` exactly once with:
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
`ecommerce-analyze-competition` once with the unchanged query,
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
`ecommerce-score-products` exactly once with
`target_country = job.target_country`, `product = input.product`,
`ads = prepared_ads`, `competition_signals = valid_reports`.
Let the skill perform its required current target-market web research and apply
its unchanged bands/caps. Validate its full output, target market and semantic
score checklist. Error-only, incomplete or invalid output stops without publication.
A complete valid score with error is publishable: keep that error unchanged inside
score, add a correlated error to the outer errors array and report partial success.
Do not filter candidates by a new score threshold or change the score to hide errors.

## 5. Publish, then record processed IDs

Generate one trusted UUIDv4 signal_id distinct from job_id and cluster_id.
Follow storage.md to save one UTF-8 application/json object at
`discovery/{job.job_id}/{signal_id}.json` for a normal run or
`discovery/_test/{job.job_id}/{signal_id}.json` for an override run. Use these fields:
`job_id, signal_id, keyword, source_country, target_country, product, ads,
competition_signals, score, errors`.
Job fields, product and ads are unchanged originals; competition_signals is only
the ordered valid subset passed to scoring; score is the entire validated result.
Do not add normalized_product, a lifecycle field, or an alternative envelope.
Validate `#/$defs/artifact` and cross-field equality before writing.

After verifying normal publication, append only input ad IDs to the registry.
Verify persistence and preserve existing entries. A failed or uncertain update
returns published_registry_pending with verified file identifiers and the recovery
error. Follow storage.md for reconciliation. Release the guard on every exit.
Keep further normal runs on hold during unresolved recovery. End an override run
after verifying its test file and leave the registry and any hold untouched.

## 6. Return JSON only

Return `#/$defs/result`, including likely_product_name, signal_id,
output_filename and errors. Use failed before verified publication, succeeded
for fully verified error-free completion, partial for published/registered output
with research errors, and published_registry_pending for incomplete registration.
For a verified test file, return test_published with registry_updated=false
and retain any research errors. Verification failures still return failed.
No filename/file ID is a success claim until verified. Preserve earlier errors.
When input is too malformed to recover a cluster ID/name, use null rather than
inventing one. Do not emit prose, code fences, secrets or internal traces.
