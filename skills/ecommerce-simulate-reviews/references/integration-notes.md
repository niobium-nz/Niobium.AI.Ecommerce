# Integration notes

## Inputs and outputs

Pass `product_details` and exactly one entry selected from an upstream strategy's `customer_segments` as `customer_segment`, together with `target_country`. Do not pass the full strategy root. Accept the approved product-name additions without requiring them from older callers. Preserve source facts and do not edit the upstream strategy.

The output is a JSON array of fictional draft/prompt records, not a replacement for a customer-review database. Every item contains `is_fictional: true`. Store and display that provenance independently from `review_text`; the review body has no embedded label. Use a separate visible internal-use fiction notice in human-facing displays and preserve it on exports. Never present these records as actual customers' opinions or merge their ratings into public aggregates.

Require a nonempty `video_prompt` in all successful items despite the legacy model's nullable field. Replace the legacy empty-array failure with `{"error":"..."}`. No successful result contains errors, and no error contains a partial batch.

## Runtime

No scripts, browser, research connector, advertising account, code interpreter, or media generator is required to execute this skill. It is a text-generation and validation workflow driven by the supplied context. The optional plugin wrapper carries the same skill resources and does not install, authorize, or publish anything by itself.

Read schemas as constraints without code when necessary. A software consumer can validate them with any compatible JSON Schema Draft 2020-12 implementation. Structural checks do not establish whether a claim is true or an internal UI displays disclosures correctly. Reject duplicate JSON keys before schema validation because a parser might otherwise overwrite one silently.

## Boundaries and provenance

The country-code lookup is an offline factual snapshot extracted from pycountry 24.6.1, not an assurance of live ISO updates. It contains only code/name data; no Python runtime or library is bundled or required. Country changes require updating both the lookup and schema pattern together.

The user-approved adaptations are: a structural true-only fictional boolean instead of a review-body prefix; percentage-based category allocation; approved length bands; a nonempty video prompt for every item; error-only failure responses; and previously approved upstream field compatibility. The total remains 30 because no count change or count field was requested.

The examples are synthetic fixtures. The bundled behavioral cases are scenario specifications, not evidence of end-to-end model execution. Do not promise identical prose across model runs, factual outcomes from imagined reactions, or that downstream media generators will obey every instruction.
