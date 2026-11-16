# Contract guide

## Contents
- Scope and approved changes
- Input validation
- Output shape
- Cross-field requirements
- Errors and unknowns

## Scope and approved changes

Use JSON Schema Draft 2020-12. Treat the bundled schemas as the runtime contract;
no original source-language model is needed. Preserve the source strategy's
product details, economics and offers, mobile-first plan, and nested segments.
Use lower snake case keys and original string/list/integer value categories.

Implement these user-approved adaptations: require `currency`; require the original
business inputs instead of guessing missing costs; retain tax and fees as explicit
strings; add three product-level name fields; make unsuitable bundle objects
nullable; and replace generated UUIDs with readable plan-local asset IDs. Use the
established JSON-only `error` object for invalid/materially ambiguous requests.
Use the country-code convention already agreed in this project.

## Input validation

Require all seven fields declared required in the input schema. Costs must be
finite nonnegative JSON numbers, not quoted currency strings or booleans. Keep an
explicit zero; do not infer free shipping beyond its inclusion in given COGS.
Accept optional competitor arrays as absent or arrays of nonblank strings, not
null. Accept the optional selling-point object as absent, null, or an object;
its twelve lists default to empty when omitted. Do not accept unknown fields.

Retain product identity and supplier costs as user-provided data. Competitor claims
are references, not verified claims. If a name is pure branding and no other input
identifies the product, return an error asking for descriptive product information
in the existing input fields. Do not fetch private context or guess the category.

Check country membership and conversion using the bundled lookup. Check currency
semantically, not only for three letters. Tax/fee grammar is intentionally not a
closed mini-language. Accept any clear wording with a uniquely interpretable
amount/rate, basis, inclusion convention and order/unit scope. State the parsed
interpretation in `given_inputs`; if two interpretations materially change costs,
return an error specifying the missing detail. A schema pass alone cannot establish
that a free-text instruction is financially unambiguous.

## Output shape

Return exactly these top-level fields on success:
`product_details`, `pricing_economics_and_offers`,
`mobile_first_landing_page_plan`, `customer_segments`.

Under `product_details`, add `suggested_product_names` (5 to 10),
`recommended_primary_product_name`, and
`recommended_primary_product_name_rationale`. Retain all other model fields.

Emit the optional anchor price and diagnostics as their declared object/string
or JSON null. Emit both bundle keys even when their values are null. Retain a
non-null `single_unit_offer`. Do not add top-level currency, status, citations,
sources, country, blockers or a numeric economics table: use the existing fields.
Keep economic values as explanatory strings, including currency and calculation
context. Use actual JSON null, never the string "null", for nullable values.

Preserve the twelve selling-point lists: positioning, differentiators, proof_points,
target_customer, objection_handling, comparison_claims, calls_to_action,
offers_and_guarantees, risk_reducers, social_proof, urgency_scarcity,
compliance_language. Interpret all as competitor-reference material.

## Cross-field requirements

Make the primary product name a member of the proposed list and repeat it exactly
in each shared-context snapshot. Each angle's recommended name must be one of that
angle's alternatives. Do not use the competitor name as a recommendation. State
that name availability and trademark clearance remain unverified.

Keep shared country/currency, prices, shipping, refund framing and core offer
consistent across the plan. Repeat country/currency in `given_inputs` and include
currency in every monetary presentation. Give each segment enough context to be
copied alone. Keep number fields as integers and ratings between 1 and 5.

Order segments by expected profit potential, explain meaningful differences and
permitted ties. Normally return 3 to 5 segments; record why a different number is
appropriate. Keep at least one concrete angle for each segment without padding.

List the same landing-page section names in section order and section asset plans.
Define every planned asset once, use unique readable `lp-...-NN` IDs, and describe
it sufficiently to avoid cross-reference-only handoffs. References may repeat an
existing ID when they mean the same asset; materially different assets need new IDs.
Repeat a brief description beside asset references inside stand-alone segments.

## Errors and unknowns

Return only `{"error":"..."}` for malformed/invalid input, missing currency/costs,
ambiguous tax/fee bases, unidentifiable products or inability to complete essential
calculations. Do not mix that object with a partial strategy. Tell the caller what
specific correction is required without generic follow-up questions.

Do not treat low expected profitability as invalid input. Return an honest strategy
and explain why the base case should not launch until its identified dependency
changes. Mark undefined metrics in the existing string fields and explain why.

Use explicit descriptions such as "Not supplied; confirm before publishing" for
unknown facts in required string fields, and `[]` for unavailable lists. Never
turn missing materials into an invented composition. Optional web unavailability
and competitor contradictions are limitations recorded in existing fields, not
permission to invent facts. Follow higher-priority safety rules for disallowed work.
