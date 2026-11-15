# Example fixtures

All product names, facts, audiences, viewpoints, ratings and scenes in these files are invented test data. They are not real reviews, market findings, endorsements, or observed customer satisfaction. The product reference has not been inspected and no video has been generated.

Use `input.json` as the complete source for `output.json`. The fixture contains one fictional desktop cable-holder product and one selected segment. Prices, shipping assumptions, forecasts and proof requests are not promoted to facts in the drafts.

The success fixture contains the full fixed-size batch. `output-plan.json` is a build-time audit aid that records derived categories, word counts and scripted ratings; it is not a runtime output extension. Percentages, rather than these sample counts, define the policy.

Every success item carries `is_fictional: true`; its review body has no disclosure prefix. Any human-facing preview must display the fictional/internal-use notice separately. Every paired video prompt requests the notice throughout a later video and requires the actual downstream product reference.

`error.json` illustrates the only failure shape. `behavioral-cases.json` contains review scenarios and expected behaviors; these text cases are not evidence that an agent was executed against them. The separate delivered validation report distinguishes automated schema/fixture checks from semantic review.
