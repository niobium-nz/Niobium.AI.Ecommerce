# Country-code lookup

Use `iso3166-1.json` as a readable lookup of alpha-2/alpha-3 pairs. Match without
case sensitivity. Do not accept a code merely because it has two or three letters.
Do not accept numeric codes, country names, historic codes, user-assigned codes,
subdivision identifiers or reserved aliases such as UK instead of GB.

This is the same pinned lookup bundled with Ecommerce Expand Keywords: 249
records, originally extracted from pycountry 24.6.1 on 2026-11-01 according to its
embedded metadata. This package reuses that supplied snapshot; it does not claim
a fresh download or independent verification of today's ISO registry. No pycountry
runtime is included or required. Country labels are lookup labels, not a rule for
language, spelling, political status or market evidence.

A code absent from the snapshot is unverified by this package. Return a correction
request in the JSON error channel rather than guessing an alias. For an asserted
new assignment, maintainers should verify an official registry and update BOTH
the data and the input schema's assigned-code pattern before accepting it. Do not
silently expand the runtime contract from a casual web result.

The snapshot does not create a mandatory network dependency. Country resolution
is used to scope any research that is actually needed, not to demand research for
well-specified products. Use only region parameters actually supported by the host.
