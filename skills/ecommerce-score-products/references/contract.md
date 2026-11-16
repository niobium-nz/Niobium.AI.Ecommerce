# JSON contract

## Input

Accept exactly one object with `target_country`, `product`, and `ads` required.
Allow `competition_signals` to be omitted, defaulting to `[]`. Require arrays,
not null or a single object, for all list fields. Empty ads and reports are valid.
All property names use lower snake case. Preserve supplied source strings and
record identifiers; country normalization is an internal conversion plus output echo.
Reject malformed JSON, duplicate object keys, unknown properties and type coercion.
Treat quoted numbers, booleans-as-numbers, null lists and unsupported country codes
as invalid. Do not require a language runtime or a schema validator to use the skill.

### Product

Require `cluster_id`, `cluster_label`, `cluster_confidence`, `is_product`,
`is_service`. Accept confidence labels `High`, `Medium`, `Low`. Require a physical
product classification: true/false, respectively. Permit nullable or omitted
`landing_page_domain`, `likely_product_name`, `category_guess`. Default omitted
`known_features` and `ad_archive_ids` to empty arrays. Do not invent missing IDs.

A structurally valid product can still fail semantic identification. Use label,
name, features and relevant ad copy to identify form factor and job. An opaque SKU
without enough evidence requires an error, not a guessed score. Reject explicit
service-only/nonphysical offers even when mislabeled as products.

### Ads

Accept every supplied model field through the `ad`, `snapshot`, `snapshot_body`,
`image`, `video`, and `card` definitions in the input schema. Nullable source
strings may be omitted or null. Metadata booleans/integers may be omitted, but if
present must match their types. Optional list fields default to `[]`.

Do not borrow the Transformer's mandatory-ad-ID validation rule. The Scorer accepts
missing/null `ad_archive_id` as supplied evidence with weaker attribution. A missing
boolean/date is unknown, not a verified false/zero observation. Do not guess units
or a country from an integer date, page name, ad language or URL. Do not fetch media
just to fill fields; inspect only content actually available through permitted tools.

When product.ad_archive_ids are present, prefer ads with matching source IDs. Record
missing referenced records as missing evidence rather than fabricating them. Keep
unmatched ads out of product-specific proof unless explicit supplied cues establish
relevance. For multi-product ads, isolate the selected item's cues. Deduplicate
repeated source IDs/identical creative observations for evidence; do not count duplicate
records as independent creative testing. Treat contradictory duplicates cautiously.

### Competition reports

Validate each item against `competition-scout-output.schema.json`. This is the
finalized upstream response schema, copied without changes. Accept full reports,
partial reports with `error`, and error-only objects. Do not add or accept the old
`mcp_call_made` or `mcp_error` aliases. The upstream country remains its original
alpha-2/alpha-3 representation; normalize only for matching to `target_country`.

## Output

Return either an error-only object, or a complete scored result. Do not emit a
partially populated success object. Normal fields are:

| Field | Contract |
|---|---|
| `score_version` | Exactly `Ecommerce-PaidSocial-v1` |
| `target_country` | Assigned uppercase alpha-2 code matching the input market |
| `product_name` | Supported supplied product name, or actual null if unknown |
| `normalized_product_type` | Nonblank generic physical-product type |
| `final_score` | Integer 0-100 after all caps |
| `priority_band` | One of the five uppercase values defined by the score |
| `subscores` | Exactly seven fields and their fixed integer bands |
| `caps_applied` | String array identifying applicable caps and evidence |
| `top_reasons` | Up to three concise strings, supported and attributed |
| `key_risks` | Up to three concise strings; combine related limitations if needed |
| `evidence_confidence` | Integer 0-100, evidence quality rather than product quality |
| `sources_checked` | Distinct actual source domains or source names; not invented sources |
| `error` | Nonblank string only on partial operational research failure |

For error-only responses, permit exactly `{"error":"Actionable reason"}`.
Do not add `message`, an invalid score, an empty score object or a version field.
A low score, source disagreement, upstream error or absence of evidence is not by
itself a current-run tool failure. Keep those in limitations/risks and confidence.

## Semantic checks beyond JSON Schema

The schema enforces types, ranges, bands, priority thresholds and the ceilings of
caps recorded with their reporting prefixes. It cannot establish evidence support,
compute a sum across fields or choose caps from product facts. Before returning:

1. Confirm the target-country output equals the normalized input country.
2. Confirm seven capped components are present and add them exactly once.
3. Confirm `final_score = min(component sum, every applicable final ceiling)`.
4. Confirm all material caps are selected from the evidence, not just the ones
   needed to make the output look consistent. Publish capped component values.
5. Confirm priority matches the capped score, confidence reflects research quality,
   and all relevant failures and uncertainty fit within the existing text fields.
6. Confirm no source-market count is represented as a target-market count and no
   missing/failed report is represented as verified absence of competitors.
7. Confirm no prohibited source file or executable is needed at runtime.
