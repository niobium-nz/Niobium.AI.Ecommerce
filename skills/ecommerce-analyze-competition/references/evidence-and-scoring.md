# Evidence, counts and qualitative scoring

## Identify records without fabricating evidence

Deduplicate identical `ad_archive_id` values across pages. Never merge different
ad IDs because their copy, page or collation ID matches. When duplicates disagree
about age, status or scope, keep the conflict and withhold that identity from the
verified subset until resolved by already returned evidence. Do not choose the
oldest start just to qualify an ad. Treat IDs as opaque strings.

For missing IDs, keep source-page/row references for audit; do not create fake ad
IDs. If exact distinct-ad counts cannot be established, output `-1` and describe
identified unique ads plus unidentified returned rows as lower bounds/row counts.
Retain uncertainty instead of silently dropping the rows.

Prefer page IDs for advertiser diversity. Reconcile `page_id` with snapshot page
ID only when consistent. If only names exist, compare normalized page names and
explicitly label the result as distinct page-name identities, not unique businesses.
Do not collapse different page IDs sharing a name. Missing identities or unresolved
name/ID collisions make an exact advertiser count unknown. Describe any verified
minimum. Do not assume that pages are economically independent businesses.

## Exclusions and other scope gates

Use only supplied exclusion terms, as case-insensitive literal substrings. Inspect
available page name, snapshot title, body.text, captions, link_description, card
body/title/caption/description and the landing hostname. A positive match in ANY
available allowed field excludes the entire ad. Do not match JSON keys, timestamps,
IDs, image/video URLs or tracking parameters. Do not translate or stem terms.

Do not "correct" a match based on negation: `no roller required` still matches
`roller`. An ad matching both `roller` and `lint roller` counts once as excluded;
both terms can appear in the top matched terms. Deduplicate equivalent terms for
matching, retain the first supplied spelling for reporting, and break tied term
counts in original input order.

A detected match is established even if other fields are missing. A missing or
empty creative does not establish a nonmatch. If available fields are too sparse
to test exclusions, mark the affected row unresolved, set `filtering_possible`
false for the batch and `excluded_ads_count` to `-1` if that exact count is unknown.
Include the mandatory filtering-warning phrase from the entrypoint. With no
exclusions, or an explicitly empty retrieved set, filtering is not required:
set `filtering_possible` true and `excluded_ads_count` 0.

Within records that did not match a term, apply age/activity and explicit scope
notes. Do not infer new product features or tighten to an unprovided archetype.
Withhold clearly out-of-scope or unverified records and explain why. Interpretations
are labels, not new queries or proof that an ad is relevant.

Use disjoint buckets for reconciliation: term matches; other definite rejections
among remaining rows; unresolved remaining rows; confirmed eligible remaining rows.
If all are resolved, raw equals the first, second and fourth buckets summed.
If unknown rows could change the eligible total, publish in_scope_ads_count as -1
and report confirmed eligible evidence separately as a lower bound. A raw missing-ID
uncertainty need not invalidate known evidence, but it prevents unsupported exact
unique-ad totals. Never use raw advertiser diversity as eligible diversity.

## Evidence and inference

Record observed eligible-ad count, eligible advertiser identity count, repeated
creatives/offers and explicit reseller/affiliate cues when present. Attribute
snippets to source IDs or page/row references and copy text faithfully. A claim
about an affiliate/reseller is an inference unless explicitly established in text.
A sort order is not an actual impressions number. Missing numeric fields stay
unknown. Do not extrapolate counts to unvisited pages or the entire market.

Reflect notes concretely (for example, retaining glove/mitt offers while withholding
rollers) in `scope_note` or `justification`. If no notes were given, do not claim they
influenced the result. Flag heavy exclusions and broad scope. Only recommend an
actual next keyword when notes allow it, and never run it in the same execution.

## Heuristic rubric (a convention, not calibrated market measurement)

Use the full `rating0_to10` integer scale without a fabricated ads-to-score formula.
Explain your chosen point in the operational definition and justification.
- `-1`: evidence cannot support a useful competition assessment, including an empty
  result without adequate coverage, wholly unverified ages or unresolved scope.
- `0`: no eligible competitors observed in an assessable bounded sample; never
  claim no competitors exist in the country. Prefer `-1` if sampling is too weak.
- `1` to `3`: limited observed eligible competitor presence, such as a small set
  or heavy concentration in one advertiser.
- `4` to `6`: a meaningful set of distinct eligible advertisers with relevant offers.
- `7` to `10`: substantial, clearly diverse eligible competition within the observed
  sample, with corroborating offer repetition across distinct advertiser identities.
  High ad volume from a single advertiser alone is insufficient.

Use confidence `0` to `3` for major field/age/scope uncertainty or partial tool
failure; `4` to `6` for useful but bounded/biased samples; `7` to `10` only for strong
consistency and evidence completeness WITHIN THE RETRIEVED SAMPLE, never population
representativeness. Use `-1` if confidence itself cannot be assigned. Do not raise
confidence solely because all three pages were consumed.

Mention the exact phrase, selected country, active/age conditions, impression-sort
selection, one-to-three-page budget and exclusions in the operational definition
or limitations. Keep numerical scores explicitly qualitative and sample-relative.

Put weak-demand commentary only in `inference`, marked as a weak advertising proxy.
Do not create a demand_score key. Age and activity can be consistent with sustained
advertiser interest but cannot prove consumer demand, revenue, conversions, market
share, profitability or uninterrupted ad delivery. Sparse or empty findings do not
prove a market opportunity. No model knowledge substitutes for missing tool evidence.
