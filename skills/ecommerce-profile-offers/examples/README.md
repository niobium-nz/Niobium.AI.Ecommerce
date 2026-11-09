# Synthetic examples

Every hostname, merchant observation, price, claim, timestamp and browser event in
these examples is fictional test data. No example URL was visited. Do not reuse
these values as live evidence, defaults or a run clock.

`input.json` and `output.json` illustrate a profile after required overlay handling.
`observations.json` is the invented evidence ledger for that example.
`output.partial.json` illustrates stopping after entry because a post-entry
redirect guard is unavailable. Policy URLs can be visible without inspected terms.
`output.unreachable.json` demonstrates two failed entry loads.
`output.ambiguous.json` retains vendor facts without choosing a collection item.
`output.error.json` demonstrates the real-browser capability requirement.
`scenarios.json` contains offline policy/contract cases. They are not a browser
harness and do not prove real-world network or interaction enforcement.
