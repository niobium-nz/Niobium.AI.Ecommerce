# Release checklist

Before returning, check:

- One valid input object, one target country from the snapshot, one product, and one selected customer segment; no accidental cross-segment blending.
- An identified product with supported use and sufficient audience context, with proposals and assumptions kept separate from facts.
- Exactly 30 output objects, no partial result or output wrapper, and exactly the four allowed keys in every item.
- `is_fictional` is true in every item; no disclosure prefix in `review_text`; no claimed genuine or verified reviewers.
- Length shares 20% short, 40% medium, 40% long; rating shares exactly 2/3 at 10, 7/30 at 9, and 1/10 at 8. Compute from percentages, not rounded decimals.
- Every body satisfies its word band, has at least 30 characters, contains readable English, and is not duplicate or formulaic filler.
- Each rating is scripted, does not imply measured satisfaction, and has not caused invented faults or unsupported praise.
- Every video prompt is nonempty, closely paired, detailed, and explicitly fictional; it requests continuous visible disclosure and the real downstream product reference.
- No invented specifications, visual details, fulfillment events, returns, public proof, certifications, performance numbers, medical outcomes, or unapproved claims.
- No web research, paid calls, publication, actual image/video generation, or hidden code dependency.

Fix failures before responding. If safe, grounded completion of the whole batch is not possible, emit only a nonblank `error` object. A semantic check is still needed even if the JSON Schema passes.
