# Scoring rules

Preserve the supplied model: seven integer-banded components totaling a maximum
of 100 points, plus component and final-score ceilings. Read `scoring-model.json`
for exact values and original high/low criteria adapted to the target country.
Do not multiply by additional percentages or turn a prior Scout rating into a
mechanical subscore. Score test priority, not profitability or a purchase guarantee.

## Selecting bands

Assign each subscore from its supported band. Prefer the lower band for an
in-between judgment. Use the same evidence pattern consistently; do not add new
quantitative review-count, seller-count, currency or ad-age thresholds. The source
provides discrete bands and qualitative high/low guidance, not a fully calibrated
classifier for every intermediate band. State uncertainty rather than claim that
judgment is mathematically deterministic.

Reward easy-to-demonstrate products, substantiated target-market buyer-interest
proxies, differentiation, workable retail-price positioning, transferable creatives,
manageable operations and low apparent regulatory/claim risk. Penalize commodity
pricing, education/setup friction, weak transferability and unresolved downside.

Treat supplied features and ad claims as claims to verify. Distinguish popularity
of a broad category from exact form-factor demand. Several listings show availability,
not sales. Ad longevity, likes and promotions are weak evidence absent corroboration.
Never treat zero exact-query ads as proof of either zero demand or no competitors.

## Component constraints

Use the following reporting prefixes in `caps_applied`, followed by `: ` and a
short evidence-grounded explanation. These are implementation labels, not new rules.

| Prefix | Apply when | Ceiling |
|---|---|---|
| `COMMODITY_COMPETITION_6` | Highly commoditized with many near-identical low-price local sellers | competition_advantage 6 |
| `COMMODITY_PRICING_4` | Same commodity condition | pricing_headroom 4 |
| `OPS_COMPLEXITY_4` | Strong fragility, bulk, leakage, fit or installation burden | ops_simplicity 4 |
| `UNPROVEN_DEMAND_COMPETITION_6` | Competition appears absent and demand remains unproven | competition_advantage 6 |

Publish component values after ceilings, even where the uncapped judgment was higher.
Record an applicable cap even when the chosen component is already lower. Apply
both commodity limits together. Do not offset these penalties with invented bonuses.

## Final constraints and precedence

| Prefix | Apply when | Final ceiling |
|---|---|---|
| `PROHIBITED_OR_RESTRICTED_15` | Appears prohibited, clearly restricted, counterfeit or likely illegal in the target country | 15 |
| `UNRESOLVED_COMPLIANCE_35` | Public-source risk screening cannot reasonably resolve compliance, or serious legal/platform-policy risk remains | 35 |
| `THIN_EVIDENCE_60` | Web evidence is thin or tools fail materially, but a score can be returned | 60 |

Compute `sum_of_components` after component limits, then
`final_score = min(sum_of_components, applicable_final_caps)`.
When no final ceiling applies, use the component sum. Do not add an output field
for the sum. List all applicable ceilings; apply the strictest numerical limit.
A ceiling is not a target or a floor. A sum of 12 with a 35 ceiling remains 12.

Map the final score to the five priority bands in `scoring-model.json`. Do not
apply the priority band to the uncapped sum. For example, synthetic subscores of
20, 12, 9, 8, 8, 8, 15 sum to 80 and map to STRONG_CANDIDATE without caps; an
applicable 35-point compliance ceiling changes the final result to DO_NOT_TEST.
The numbers illustrate arithmetic only, not a real product assessment.

## Evidence confidence and claims

Use integer 0-100 confidence to express evidence quality separately from score.
Reduce it for sparse, stale, contradictory, indirect, cross-country or partially
failed evidence. Do not invent a probabilistic interpretation or precise formula.
A product may be confidently unsuitable. A promising product may have weak evidence.

Do not claim legal clearance, guaranteed policy approval, verified margins or
copyright/patent freedom from a web scan. For high-risk categories require current
applicable official-source validation before assigning strong safety scores; missing
such evidence leaves risk unresolved and invokes the 35 ceiling as appropriate.
