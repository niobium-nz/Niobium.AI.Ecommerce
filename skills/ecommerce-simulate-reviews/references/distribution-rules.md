# Percentage allocation

Use the fixed policy in `references/distribution-policy.json`. It is a bundled reference, not a runtime override interface.

## Total versus category shares

Keep exactly 30 items per successful run. The user's change replaces category-specific absolute rules with percentages; it does not add a review-count input or remove the original batch size.

Length: short = 20%, medium = 40%, long = 40%.

Rating: 10 = 66 2/3% (exact fraction 2/3), 9 = 23 1/3% (exact fraction 7/30), 8 = 10% (exact fraction 1/10).

Compute each quota as `30 * exact_share`. All quotas are integers for this batch size. Do not round 66 2/3% to 66.67% or 23 1/3% to 23.33% before multiplication. The schema's integer `contains` limits are compiled from these percentages; the example's counts are derived checks, not separate policy settings. A future batch-size change would require an explicitly revised contract and allocation policy.

## Word bands

Short: 10 to 25 words inclusive. Medium: 26 to 60. Long: 61 to 100. Every `review_text` must also independently contain at least 30 characters.

Count nonempty tokens separated by whitespace. Count `can't` and `easy-to-use` once each. Do not add punctuation-only tokens to inflate a count. Strip leading and trailing whitespace; count only the review body, never JSON keys, the boolean, the video prompt, or an external display label.

## Independent axes

Allocate lengths and ratings independently before drafting. Vary focus on ease of use, supported outcome, subjective value, emotion, and relevant subject/object behavior. Use only angles actually supported by the supplied product and selected audience. Do not infer facts from the rating quota.

Keep ratings scripted. The intentionally positive distribution is an internal creative-test setting, not measured sentiment, predicted average satisfaction, a representative review sample, or an input for public star ratings. Do not compute or display a public aggregate from it.

## Checks beyond JSON Schema

The output schema enforces length bands by whitespace-token patterns, exact batch size, scripted-rating quotas, required fields, and the constant true boolean. It cannot prove English quality, truthful grounding, distinct ideas, faithful video pairing, or correct human-visible disclosure. Review these separately. Compare normalized review bodies to catch duplicate wording even when ratings or prompts differ.
