# Country-code lookup

Read `iso3166-1.json` as a case-insensitive alpha-2/alpha-3 lookup. It is a reused
pinned snapshot from the previously supplied product-normalizer package: 249 entries,
with embedded metadata naming pycountry 24.6.1 and extraction on 2026-11-01. This
package does not claim a fresh ISO registry download or verification of later changes.
No country library or script is required at runtime.

Accept only assigned codes in the snapshot; reject numeric codes, country names,
`ALL`, aliases such as `UK`, whitespace and subdivision codes. Send uppercase
alpha-2 to the search action (for example AUS -> AU, GBR -> GB, USA -> US). Echo the
input spelling in the report country field and explain a conversion in limitations.
Do not infer a target country from the user's location. A valid assigned code does
not guarantee that the provider supports that market. Do not silently substitute
another country if a provider rejects it.

For newly assigned codes, a maintainer must update BOTH the snapshot and schema
pattern after checking an authoritative registry. Do not guess a mapping during a
paid research run. Country names in this file are lookup labels, not market evidence.
