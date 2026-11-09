# Search action and cost controls

## Binding and payload

Use the connected ScrapeCreators Facebook Ad Library keyword-search capability.
The authoring session exposed `scrapecreators.v1_facebook_adLibrary_search_ads`.
The provider route is `/v1/facebook/adLibrary/search/ads`. Host prefixes may vary;
inspect the action schema rather than blindly invoking a similarly named tool.
The user explicitly selected this action instead of the unrelated Meta Ads
account-management/search tool exposed earlier. Do not fall back to that tool.

Discover definitions without invoking paid searches. If all eight approved fields
are not accepted, return an error before spending. Use the connected action only;
do not create a direct HTTP client, request API keys, embed credentials or configure
a new server. Respect host approval/permission gates. Do not treat this package's
creation or an example payload as authorization to run a research job.

Pass the query unchanged, a single uppercase alpha-2 country and the computed
`start_date`. Always set `sort_by=total_impressions`,
`search_type=keyword_exact_phrase`, `ad_type=all`, `status=ACTIVE`, `media_type=ALL`.
Omit `cursor` for page one; use the exact opaque response cursor for page two/three.
Keep every other argument identical across pages. Do not add `active_since`,
`end_date`, `language`, `trim`, `limit`, `page`, `max_pages` or unsupported fields.

## Response interpretation

The documented example envelope has `success`, `searchResults`,
`searchResultsCount` and `cursor`; credits metadata can also appear. Use the actual
response and inspect any host content wrapper. Never invent a collection or cursor.
If `success` is false, treat it as failure even if an example/default collection
exists. A successful, explicit `searchResults: []` is a valid empty result.

Read `searchResults` for records. Keep `searchResultsCount` only as a separately
labeled provider-reported count if useful; it is not the number of records examined,
not an exclusion-filtered count and not the size of the verified older-ad subset.
Do not sum it across pagination. Do not multiply by `collation_count`.
Use `ad_archive_id` as the actual ad identity, not `collation_id`.

The request `start_date` is documented as an impressions-window parameter. The
returned ad's own `start_date` is a separate per-record value. Passing the former
does not prove the latter predates it. The user's requested parameter is retained;
minimum-age assessment is an additional local evidence check, not a replacement
request. Do not silently invert the date or add an end-date filter.

## Sequential budget

Maintain `attempts` (including failures), successful pages, cursor history and a
set of identified ad IDs. Make at most THREE keyword-search attempts per run.
One page is the default; three is a ceiling, not a target. Do not parallelize calls.
A failed attempt ends retrieval immediately. Do not retry, even if the provider
might not charge for that error. Do not bypass the cap through a different endpoint.

Continue only if all conditions hold: attempts < 3; previous response is usable;
next cursor is nonblank and not used before; page is nonempty and adds new data;
and another page could materially improve an unresolved scope/diversity judgment.
Do not paginate simply to reach a count or to manufacture a high score.

Stop on an empty page, absent/null cursor, explicit end-of-pagination indicator,
repeated cursor, all-duplicate page, sufficient bounded evidence, systematic missing
fields unlikely to improve, a failure, or the third attempt. A missing cursor is
not permission to guess a page number. Do not issue company, detail, transcription,
credit-balance, enrichment or other paid calls. Do not use web/browser searches to
bypass this one-query evidence boundary.

After later failure, keep the successfully observed data with a top-level `error`.
If no usable data exists, use the error-only shape. Missing fields within usable
records are uncertainty, not permission to spend on enrichment.

## Provenance of integration details

Action schema inspected in the authoring session on 2026-11-04; no paid search was
executed. Public provider documentation also checked on that date:
`https://docs.scrapecreators.com/v1/facebook/adlibrary/search/ads/`.
This is documentation verification, not live-response verification. Recheck the
actual action schema if the provider changes; stop rather than silently dropping
required parameters. No pricing guarantee is embedded in the skill.
