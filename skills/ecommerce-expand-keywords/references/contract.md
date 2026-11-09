# JSON contract

Use `schemas/input.schema.json` and `schemas/output.schema.json` as the structural contract. Both use JSON Schema Draft 2020-12. These are language-agnostic schemas, not programming-language classes or an API request configuration. Read this document for normalization and conversational behavior that a structural schema does not express.

## Input

| JSON property | JSON type and omission behavior |
| --- | --- |
| `category_focus` | Required nonblank string. |
| `country` | Required string containing an assigned ISO 3166-1 alpha-2 or alpha-3 letter code. |
| `seed_keywords` | Optional array of strings; omitted means `[]`. Explicit `null` is invalid. |
| `optional_constraints` | Optional array of objects; omitted means `[]`. Explicit `null` is invalid. |

Allow only these properties in each constraint object:

| JSON property | JSON type |
| --- | --- |
| `target_audience` | String or null; optional. |
| `price_tier` | String or null; optional. |
| `use_case` | String or null; optional. |
| `gender` | String or null; optional. |
| `seasonality` | String or null; optional. |
| `platform` | String or null; optional. |

Keep the constraint values free text. An absent, null, or whitespace-only value imposes no restriction. An empty constraint object adds no restriction for that alternative segment. Do not invent a meaning for an unfamiliar nonblank value.

Reject unknown keys, duplicate JSON object keys, non-JSON numbers such as NaN/Infinity, wrong JSON types, and non-object top-level input. Do not coerce types, rename keys, or convert null collections to empty collections. JSON Schema `default` annotations describe the workflow's defaults; a generic schema validator does not insert values.

Permit empty seed strings as input strings, then discard them during normalization. Permit redundant seeds and normalize them; do not reject them merely for duplication.

Accept code letters case-insensitively, without surrounding whitespace: `NZ`, `NZL`, `nz`, and `nZl` resolve to New Zealand. `554`, `New Zealand`, `UK`, `en-NZ`, and `NZ-AUK` do not meet this contract. Use `GB` or `GBR` for the United Kingdom, not the reserved alias `UK`. Validate membership, not just string length, using `references/iso3166-1.json`.

The bundled country-code snapshot controls the optional offline validator and input schema. Do not silently accept a newly assigned code absent from that snapshot; verify the registry change and update the snapshot and schema together before accepting it. Explain a lookup limitation rather than declaring a verified new code nonexistent.

## Success

Return exactly two properties:

- `category_focus`: concise nonempty category phrase.
- `optimized_keywords`: ordered array of 25 to 60 nonempty strings, with no duplicates or easy near-duplicates.

Do not include `country`, constraints, metrics, source lists, or `error` on success. The usual 1 to 5 words per keyword is a preference, not an absolute validator rule.

## Error

Return exactly three properties:

```json
{
  "category_focus": "",
  "optimized_keywords": [],
  "error": "country must be an assigned ISO 3166-1 alpha-2 or alpha-3 letter code."
}
```

Use a nonempty `error` string, not a Boolean, object, array, null, or numeric code. Keep `category_focus` as an empty string and `optimized_keywords` as an empty array. Do not mix partial success with an error.

Use this shape for any actual input-validation or processing failure. When input needs correction, describe the correction inside `error`. Do not add `status`, `message`, `details`, `questions`, or any other key.

## Semantic clarification

Preserve the authorized plain-text exception when meaning, not the JSON contract, needs clarification. Ask at most three specific questions and stop without keywords or a JSON object. Keep the established input context for the follow-up, but do not infer an answer. Do not classify two legitimate alternative segments as contradictory solely because their attribute values differ.

## Validation without a programming-language dependency

Read and apply these rules directly when the host cannot execute code. The country lookup and both schemas are plain JSON resources. No script is needed to read their values or follow the workflow.

Use any JSON Schema Draft 2020-12 implementation for optional machine validation. The bundled Python helper is one optional implementation aid, not the definition of the contract. It also checks deterministic phrase hygiene beyond JSON Schema `uniqueItems`, including case and hyphen/spacing variants. Perform the same hygiene review without code; never silently waive a rule because the helper cannot run.

Keep semantic relevance, genuine segment conflicts, near-duplicate meaning, local spelling, research completion, and source independence as explicit agent checks. A schema validator cannot establish these. Downstream software should validate received JSON independently before consuming it.
