# Synthetic examples

Use `input.json` and `output.json` as a paired demonstration. `mock-search-response.json`
is synthetic data with reserved `.example` domains and fake IDs, not retrieved ads.
`search-request.json` illustrates the exact request for a frozen run date of
2026-11-04 and must not be reused as today's cutoff. It is not a runnable command.
`output.partial.json`, `output.empty.json` and `output.error.json` show the three
exception/empty pathways. `scenarios.json` covers arithmetic, filters, pagination,
response checks and behavioral review cases; it is not an executable skill engine.

These files contain the provider's response keys where demonstrating its envelope,
including `searchResults` and `searchResultsCount`. Only the skill's input/output
contracts require lower snake case; do not rename real provider fields in transit.

The main example stops after one page by choice even though a synthetic next cursor
exists. The partial example instead attempts a second page and encounters a synthetic
failure. They are different run scenarios, not contradictory records of one run.
