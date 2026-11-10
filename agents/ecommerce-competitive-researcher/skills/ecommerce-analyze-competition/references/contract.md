# JSON contract

## Input

Accept one object, never a list or a query batch. Require `query` and `country`.
Use lower snake case keys. Keep `category_name` nullable/optional. Default omitted
`notes`, `avoid_or_exclusion_terms` and `product_interpretations` to empty arrays;
reject explicit null arrays. Require nonblank string items in the two text arrays.
Interpretation items can contain nullable `interpreted_product_type`, nullable
`interpreted_archetype`, `why_this_interpretation` (default `[]`) and nullable
`confidence` (`High`, `Medium`, `Low`). Do not let confidence labels create evidence.
Reject unknown fields rather than hiding a misspelled filter or accepting a budget
override. Do not silently accept fenced commentary as JSON or coerce types.

Accept country codes in the bundled assigned-code snapshot, case-insensitively.
Keep the supplied query and country strings unchanged in a report; send the
country's uppercase alpha-2 equivalent to the search tool. Record that mapping
in limitations when it differs from the input. An unavailable country at the
provider is a tool failure, not permission to substitute `ALL` or another market.

## Report

Always emit `query`, `country`, `raw_ads_discovered`, `exclusion_filtering` and
`competition_signal` for a normal or partial report. A normal report necessarily
has `raw_ads_discovered.tool_call_made: true`. Do not output the legacy MCP fields.

| Path | Meaning |
| --- | --- |
| `raw_ads_discovered.raw_ads_count` | Exact distinct retrieved ad count before scope/age/exclusion filtering, or `-1` |
| `raw_ads_discovered.distinct_advertisers_count` | Distinct advertiser identifiers or explicitly labeled page-name identities in the raw retrieved set, or `-1` |
| `raw_ads_discovered.notable_raw_patterns` | Raw-sample observations, clearly not eligible-competitor totals |
| `raw_ads_discovered.snippets` | Short exact source excerpts with source references |
| `raw_ads_discovered.limitations` | Coverage, run date/timezone, cutoff, request/age distinction, attempts/pages, stopping reason, sort bias, missing fields and failure limitations |
| `exclusion_filtering.exclusion_terms_provided` | Whether the input has at least one exclusion term |
| `exclusion_filtering.filtering_possible` | Whether exclusion filtering is assessable for the whole raw sample; true when no exclusions or no records need checking |
| `exclusion_filtering.excluded_ads_count` | Exact count of ads matching supplied exclusion terms only, or `-1` if unassessable |
| `exclusion_filtering.in_scope_ads_count` | Exact count passing literal exclusions, age/activity and stated scope in the retrieved sample, or `-1` if unresolved |
| `exclusion_filtering.top_matched_exclusion_terms` | Only actual input terms observed to match; no invented expansions |
| `exclusion_filtering.scope_note` | Explain scope, notes, filtering coverage and reconciliation with age/activity/unknown rejections |
| `competition_signal.rating0_to10` | Integer 0 to 10, or `-1` for unknown; qualitative sample-specific competition |
| `competition_signal.evidence_signals` | Eligible-subset observations with record provenance and exact/lower-bound labeling |
| `competition_signal.inference` | Explicit inferences, including any weak demand proxy, never observed sales or profitability |
| `competition_signal.confidence0_to10` | Integer 0 to 10, or `-1` when unassignable |
| `competition_signal.justification` | Explain the score, uncertainty and effect of operator notes |
| `competition_signal.operational_definition` | State the population assessed, evidence weighting, scale convention and limits |

Use numeric `-1`, not `"unknown"`, for unknown numeric fields. Use explanatory
strings for other unknowns. Include all report fields even if a list is empty.
Keep zero separate from unknown. Do not emit null numeric fields or omit the
competition section to avoid expressing uncertainty.

## Error channel

Use only `{"error":"..."}` when validation or preflight fails, the tool is missing,
or no usable first response exists. A recognizable explicit empty result is usable.
A missing/null `searchResults` collection is not an empty result.

For a failed later request after usable data, emit the complete report plus a
nonblank top-level `error`. Omit `error` on clean success; do not emit null or an
empty error. Sanitize credentials, tokens and internal traces out of error text.
Partial page coverage without a failure is a limitation, not an error.

## Cross-field checks beyond JSON Schema

Schema validation does not execute the agent, enforce budgets or prove evidence.
Check runtime query equality and country mapping. Check each count against its
ledger, not against a provider estimate. For fully resolved records:

`raw = excluded_by_terms + other_rejected_after_terms + in_scope`.

Add `unresolved_after_terms` to that decomposition when records are unverified;
then use `in_scope_ads_count: -1` if they could alter the exact eligible total.
Never count one row in two disjoint rejection buckets. Describe overlapping
exclusion matches separately. Keep both exclusions and age restrictions visible.
