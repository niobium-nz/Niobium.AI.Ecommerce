# JSON contract

## Input

Use JSON Schema Draft 2020-12 in `schemas/input.schema.json`. Accept one object:

| Field | Type | Required/default |
| --- | --- | --- |
| `product_name` | Nonblank string | Required |
| `country` | Assigned alpha-2/alpha-3 string | Required; case-insensitive |
| `category_name` | String or null | Optional; omitted means null |
| `known_features` | Array of nonblank strings | Optional; omitted means [] |

Treat empty/whitespace category as no useful hint; preserve the original echo.
Treat omitted and empty feature arrays identically. A nonempty feature array is
the authority on permitted functionality. Reject explicit null for a feature list.
Preserve its order and duplicates in `known_features_provided`; deduplication applies
to derived keywords and scope lists, not to the input echo. Schema `default` is
an annotation, not an instruction that every validator inserts values.

## Normal response

Emit exactly these ten top-level fields, with all nested section objects present:
`status`, `product_name_given`, `category_name_provided`, `known_features_provided`,
`normalization`, `competitive_set_definition`, `product_interpretations`,
`web_research`, `keyword_plan`, `notes_for_downstream_agent`.

Use `ok`, `needs_more_info`, or `ambiguous` for status. Use actual JSON null for
unknown nullable scalars. Normal sections are objects, not null placeholders;
unknown lists are []. Do not emit literal "null" to mean missing data.

| Section | Fields |
| --- | --- |
| `normalization` | `brand_model_tokens`, `base_category_inferred`, `observed_from_input`, `observed_from_web`, `assumptions`, `confidence0_to10` |
| `competitive_set_definition` | `archetype_phrase`, `archetype_explanation`, `form_factor`, `job_to_be_done`, `primary_target`, `qualifiers`, `must_include_tokens`, `near_miss_classes`, `avoid_terms` |
| Each `product_interpretations` item | `interpreted_product_type`, `interpreted_archetype`, `why_this_interpretation`, `confidence` |
| `web_research` | `used`, `queries`, `key_takeaways`, `source_domains` |
| `keyword_plan` | `tier_a_archetype_terms`, `tier_b_close_archetype_synonyms`, `tier_c_brand_model_terms`, `tier_d_broad_context_terms`, `avoid_or_exclusion_terms`, `recommended_mcp_queries` |

Use the exact numeric field name `confidence0_to10` (digits remain attached to
the adjacent word). Use an integer 0 through 10, not a string, fraction, or
probability. Interpretation confidence is `High`, `Medium`, or `Low`.
These are language-neutral JSON property spellings, not serializer settings.

For `ok`, require a nonblank archetype, form factor and job plus two to five
semantic scope tokens. For `ambiguous`, include two or three plausible
interpretations. For `needs_more_info`, include an actionable request in the
notes. Do not add status-specific keys not in the schema.

## Errors

Return only `{"error":"Actionable description of the failure."}` for invalid
input or operational failure. Do not combine `error` with a normal result, emit
partial results, add an error status, or wrap this object in an array. A blank
error string is invalid. Exact English wording of examples is illustrative.

A research failure is an error when research is necessary. Sparse or ambiguous
but structurally valid content is normally a structured clarification outcome.
An opaque SKU with no clues can use `needs_more_info` without pretending to have
looked it up. If the agent instead needs external verification to proceed and
cannot perform it, use `error`, not invented research.

## Checks beyond JSON Schema

Before returning a normal result, verify all of the following in addition to types:

- Echo the input values exactly; use null/[] only for omitted optional fields.
- Keep observations, source attribution and inference distinct; do not enlarge
  the functional scope of a nonempty feature list.
- Include both form factor and job in Tier A; retain equivalent form/job scope
  in Tier B. Never infer mechanism or material without input/web support.
- Keep every term within a supported interpretation; document ambiguity mappings
  in notes without inventing nested keyword fields.
- Remove duplicate/near-duplicate keywords within/across tiers. Preserve brand/model
  spelling and distinguishing numbers; do not confuse a brand token with a feature.
- Keep tiers at or below maxima A=12, B=10, C=8, D=6. Permit fewer than suggested
  minima, including empty C. Do not treat the count targets as evidence.
- Make recommendations the ordered deduplicated A+B+C concatenation, appending D
  only if A and B are both empty. Do not count broad D terms as resolved evidence.
- Keep exclusions aligned with near-miss classes, without blanket exclusions
  that unintentionally eliminate the canonical archetype itself.
- When web research was not attempted, make used=false, all research lists empty,
  and observed_from_web empty. When used=true, log only actual actions and evidence.
- Describe confidence as evidence judgment. Do not claim market demand or strength
  of competition, and do not label plausible alternatives as verified products.

JSON Schema enforces structure and some local conditions. It cannot establish
research truth, brand identity, feature compatibility, semantic equivalence,
source correspondence or interpretation quality. The host/consumer should validate
results independently before production use. Self-checking is not a guarantee.
