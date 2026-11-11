# Economics and consistency rules

## Contents
- Modeling boundary
- Parse tax and fees
- Offer economics
- Blended economics
- Direct-purchase funnel
- Edge cases and output placement

## Modeling boundary

Use supplied business inputs and one explicit currency. These are commercial
scenario calculations, not tax/legal advice, evidence of actual campaign results,
or a claim of full net profit. Do not choose rates from the country. Preserve the
source convention that "landed cost" includes product/fulfillment cost plus the
modeled tax and processing fee. Clearly describe that accounting convention.

Use shipping-included COGS; do not add another shipping charge or unit-by-unit
parcel cost. Assume overseas fulfillment typically 1 to 2 weeks. Model return
shipping as zero because low-cost refunded items are kept; do not thereby assume
refunds, chargebacks, payment losses, replacements or overhead are zero. Where
such amounts are absent, disclose exclusions or explicitly label a sensitivity.
Do not insert unprovided app fees or supplier costs as observed facts.

## Parse tax and fees

For each offer define:
- `n`: positive integer number of the SAME input cost unit, not guessed item count
  inside a product pack. State what one input unit represents.
- `P`: recommended displayed offer price in input currency.
- `R`: total amount the customer pays, including modeled sales tax.
- `T`: modeled tax liability included in the source cost convention.
- `B`: explicitly specified payment-processing percentage base.
- `F`: percentage fee on B plus the explicitly specified fixed fee.

Use the input's tax instruction, not an assumed country's rules. For simple cases:

| Explicit instruction | Customer total R | Tax amount T |
|---|---|---|
| No tax / zero | P | 0 |
| Rate t of tax-exclusive price, included in displayed price | P | P * t / (1 + t) |
| Rate t of tax-exclusive price, added at checkout | P * (1 + t) | P * t |
| Fixed tax k per order, included in displayed price | P | k |
| Fixed tax k per order, added at checkout | P + k | k |
| Rate t of total customer charge, included in price | P | P * t |

Treat these as arithmetic examples, not an exhaustive grammar or real rates. For
multiple taxes, thresholds, caps, different bases or per-unit charges, calculate
only what the input clearly specifies. Do not substitute a simpler case if it
changes the result. Require clarification through `error` when the calculation
cannot be resolved unambiguously.

For `f% of total customer charge plus k per order`, use `F = f * R + k`.
For an explicit pre-tax base, use that base instead; do not automatically use R.
Apply a fixed per-order fee once per order, not once per unit. Multiply a fee by n
only if its instruction explicitly specifies a per-unit fee. Record the interpreted
bases and actual computed amounts for each offer in `landed_cost_model`.

## Offer economics

Use:

`C_n = cogs_per_unit + (n - 1) * extra_unit_cogs_per_order`

`L_n = C_n + T_n + F_n`

`M_n = R_n - L_n`

Here M_n is contribution before advertising and any explicitly excluded costs.
Equivalently, M_n = (R_n - T_n) - C_n - F_n. Do not use net-of-tax revenue and
subtract T_n a second time. When tax is added at checkout, do not confuse the
pre-tax displayed P with gross customer cash R. Show both values transparently.

Recommend a price scenario with clear value logic, not the lowest possible price
by default. Explain uncertainty, source-backed anchors when available and the main
price/conversion dependency. A compare-at price needs genuine support; do not use
an invented inflated anchor. Describe proposed test prices as hypotheses.

Make multi-unit bundles only for plausible additional usage, backup ownership,
gifting or sharing. Do not invent an accessory with no supplied cost. Permit each
bundle slot to be null independently. Explain a higher incremental cost honestly;
do not change it or claim an economy that does not exist. "Best seller" is an
internal offer slot, not observed popularity. Name public bundles without that claim.

## Blended economics

Choose an explicit hypothetical order mix `w_i >= 0` with `sum(w_i) = 1` across
non-null offers. Set absent offer weights to zero. For a single-offer plan use 1.

`AOV_gross = sum(w_i * R_i)`

`L_blended = sum(w_i * L_i)`

`M_blended = sum(w_i * M_i) = AOV_gross - L_blended`

`break_even_blended_CPA = M_blended`

`target_blended_CPA = M_blended - profit_buffer`

State whether the buffer is a currency amount or a defined percentage of margin
or revenue. It is a modeled assumption, not a fixed standard. If revenue mixes,
weights, conversion assumptions or offer composition differ by segment/angle,
recompute and label the relevant economics rather than copying incompatible CPA.
When one shared base case is used, say it is not a measured segment forecast.

If displaying ROAS, state its revenue basis and use the same mix and CPA basis:
`ROAS_gross = AOV_gross / CPA`. Do not compare a gross-revenue target against a
net-revenue report without disclosing the distinction. CTA alone is permitted by
`recommended_cta_or_roas_target`; do not invent a ROAS target to fill the field.

## Direct-purchase funnel

Convert percentages to fractions before multiplication:

`l = LPVs / clicks`

`c = checkout starts / LPVs`

`p = purchases / checkout starts`

`q = l * c * p = purchases / clicks`

`estimated_blended_CPA = CPC / q = spend / purchases`

`cost_per_LPV = CPC / l`

`cost_per_checkout_start = CPC / (l * c)`

Keep rates within [0, 1]; base-case positive-conversion scenarios need each rate
above zero. Do not accidentally interpret purchases per checkout start as purchases
per LPV. Use a normalized cohort such as 1000 clicks to independently cross-check
LPVs, checkout starts, purchases, spend and CPA.

Disclose the assumed CPC and all three conditional conversion rates; none are
observed performance without evidence. Show contribution less estimated CPA and
whether estimated CPA meets target and break-even. Describe a meaningful downside
sensitivity without changing the baseline silently. Never call checkout-start cost
CPA. Diagnostics remain secondary metrics.

## Edge cases and output placement

Use unrounded intermediate calculations and round only for display to appropriate
currency precision. Show any rounding convention; do not force two decimals for
all currencies. Use consistent rounded displays, but do not sum prematurely rounded
figures and present an unexplained mismatch. Check both formulas for contribution.

If q is zero, state that estimated CPA is undefined because no purchases are modeled;
do not emit JSON NaN or Infinity. If break-even contribution or target CPA is zero
or negative, state that no positive paid acquisition budget meets that guardrail.
Do not hide negative results by clamping them to a plausible positive target.
Economic infeasibility is a strategy finding, not an input-schema error.

Put input cost/currency/tax/fee facts in `given_inputs`. Put rates, proposed prices,
order mix, buffer, exclusions and research limitations in `modeled_assumptions`.
Use `landed_cost_model` for per-offer calculations and blended totals; retain the
existing string-valued model fields. Use `funnel_logic_explanation` for formulas,
CPA feasibility, sensitivity and major dependencies. Keep enough context in each
segment snapshot for it to stand alone without the rest of the report.
