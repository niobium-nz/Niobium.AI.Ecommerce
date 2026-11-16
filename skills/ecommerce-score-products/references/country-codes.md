# Country-code resolution

Read `iso3166-1.json` without running code. Accept a case-insensitive assigned
alpha-2 or alpha-3 code in that snapshot and resolve to uppercase alpha-2 for
`target_country` in the result and internal comparisons. For example GBR -> GB,
USA -> US, JPN -> JP, NZL -> NZ, AUS -> AU. Never default to any market.

Reject full names, numeric codes, aliases such as UK, region groups, surrounding
whitespace and subdivisions. Do not silently repair an unsupported code. Use the
same conversion for each full Scout report; preserve the original supplied report.

The lookup is copied unchanged from the finalized Competition Scout package. Its
embedded metadata identifies pycountry 24.6.1, extraction on 2026-11-01, and 249
entries. It is a pinned snapshot, not a live registry verification or a runtime
Python dependency. The input pattern and output enumeration use the same snapshot.
If a code is genuinely newly assigned, a maintainer must verify and update the
lookup and schemas together; do not invent a mapping during scoring.

Country names are lookup labels only. They do not establish local authorities,
marketplaces, laws, language, currency, shipping conditions or market demand.
Discover and verify those during country-targeted research.
