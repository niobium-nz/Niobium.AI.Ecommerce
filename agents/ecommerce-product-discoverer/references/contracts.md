# Contracts and failure semantics

## Inputs and configuration

Input is exactly three strings: source_country, target_country, keyword.
Schema validation is supplemented by assigned-country membership in the bundled
249-country snapshot. Preserve source strings; uppercase alpha-2 values are
working parameters/provenance only. discovery_folder_id and timezone are deployment
configuration, never extra public inputs. The example's null folder ID is
intentionally invalid until configured. Never infer a folder ID from its name.

## Original evidence and projection

Keep complete original searchResults records, including unknown fields. Activity
and date gates use their actual supplied values. Build a separate recursive
projection using the bundled skill input schema. Report unknown omitted paths;
never coerce or discard invalid recognized fields to make a record pass.
Validate the entire projected batch before one clustering invocation.

The bundled skill owns the unchanged ten-field product contract. One ad can
support several distinct products. Check exact nonempty IDs, source membership,
schema, duplicate clusters, coverage and type after invocation. Quarantine whole
invalid clusters; do not repair their ID lists. A top-level skill error is not [].

## Business lifecycle and handoff boundary

`discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding`

Discovery outputs are discoveries only. The competitive research agent receives
discoveries and produces candidates. Product council reviews those candidates; only
approved candidates become listings for product onboarding. Saving a discovery,
a successful job status, ad age or High cluster confidence cannot promote it.
No discovery is sent directly to council or onboarding. This agent neither executes
competitive research nor invokes downstream agents; persistence is its handoff.
The manifest's handoff_target identifies a role, not a configured callable agent.

The nested product object remains the clustering skill's evidence descriptor.
Any internal "offering candidate" terminology inside that unchanged skill refers
to extraction/grouping only, never to this pipeline's post-research candidates.

## Saved JSON array

Each record contains schema_version, job_id, inputs, product, ads, provenance,
lifecycle_stage, research_status and profitability_status. Original inputs,
product and ads remain unchanged. Ads follow product.ad_archive_ids order;
every ad must qualify. Metadata stays outside source objects.

Version 2.0.0 requires lifecycle_stage = discovery and research_status =
pending_competitive_research; profitability_status remains unverified.
Remove the former review_status field rather than marking council review as
pending. The schema rejects candidate/listing stages, completed research,
council-review fields and the former version 1.0.0 shape. This is a breaking
handoff-contract change; update consumers before deployment. Do not rewrite
existing stored artifacts or promote historical discoveries during this update.

Provenance includes run_started_at, timezone, cutoff_date, normalized countries,
search_attempts, successful_pages, stop_reason, run_status and search_exhaustive.
Complete means the bounded plan completed without execution/integrity loss,
NOT an exhaustive market census. search_exhaustive is always false. Later search
failure, conflicting original IDs, invalid clusters and missing coverage cause
partial output if intact products remain. Ordinary date/activity/service filters
and Low-confidence notices alone do not cause partial status. Both complete and
partial records remain discoveries awaiting competitive research.

An empty array has no per-record context. The caller must retain final result
and original job input together; do not create unrequested Drive sidecar files.
An uncertain product classification or ad claim remains uncertain, not approved.

## Result contract

Every result has job_id, status, artifact, warnings, errors. These execution
statuses do not describe or advance the discovery/candidate/listing lifecycle.
Diagnostics contain code, message, details, using lower snake case codes. Put indexes, cluster IDs,
source IDs, omitted paths and observed unverified file IDs in details, not full
ad payloads or credentials. Empty diagnostics are []. Only UUID-generation
failure may use job_id null.

| Status | Artifact | Meaning |
| --- | --- | --- |
| succeeded | Verified; record_count > 0 | Bounded complete run; warnings allowed |
| partial | Verified; record_count > 0 | Usable output survived execution/integrity loss |
| empty | Verified; record_count = 0 | Valid outcome with no retained products |
| stopped | null | Same job filename exists; no write |
| failed | null | Other failure, including uncertain persistence |

A failed/partial retrieval with no usable products must not save []. A clean
empty search, all-ineligible sample or service-only result saves []. All-corrupt
clusters fail without a file. Partial status may have no errors when integrity
warnings alone caused it; later search failure must retain its error. A late
collision preserves preceding errors/warnings. A failed write overrides earlier
success/partial plans. Filename must equal job_id + .json; verified artifact
requires exact MIME, parent, complete bytes and a unique exact-name match.

## Diagnostic vocabulary

Preflight: uuid_unavailable, configuration_invalid, folder_invalid,
job_file_exists, collision_check_failed, input_invalid, same_source_and_target,
clock_unavailable, capability_unavailable.
Retrieval: search_failed, search_response_invalid, search_sample_bounded,
conflicting_ad_records, ad_not_active, ad_status_unverified, ad_too_recent,
ad_date_unverified.
Clustering: projection_fields_omitted, cluster_input_invalid, resource_limit,
clustering_failed, cluster_traceability_failed, cluster_schema_invalid,
duplicate_cluster_id, cluster_coverage_missing, service_cluster_excluded,
low_confidence_product, no_valid_products.
Persistence: handoff_invalid, upload_failed, upload_outcome_unknown,
artifact_verification_failed. Unexpected failures: internal_error.
