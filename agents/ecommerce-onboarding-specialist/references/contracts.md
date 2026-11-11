# Contracts and skill mappings

## Invocation and approval

Accept one object, not a batch or the researcher's candidate object. Exactly:
landing_page_url, target_country, costs, product_reference_board, optional job_id,
optional signal_id. Both omitted and explicit null correlations mean unlinked;
empty strings and malformed UUIDs are errors, never requests for fresh upstream IDs.
The agent only generates candidate_id. Retain original target_country for output;
case-insensitive assigned alpha-2 codes are allowed, but alpha-3 codes are not.
Normalize working country/currency values only as the strategy skill prescribes.

The host/caller invokes onboarding only after its own approval decision. No council
is implemented, no new approval JSON field is invented, and neither URL possession
nor generated strategies demonstrate approval. Keep the requested storage names
without relabeling the business lifecycle. Website deployment is a separate action.

Strict JSON rejects duplicate keys, NaN/Infinity, lossy numeric conversion and extra
properties. Validate the profiler's public URL restrictions and denylist before
navigation. Never access private-network targets, authenticated stores, checkout,
cart or account pages. Do not repair invalid URLs, IDs or financial inputs silently.

## Costs

costs has exactly cogs_per_unit, extra_unit_cogs_per_order, sales_tax,
payment_processing_fees and currency. The first-unit and incremental-unit costs
include shipping and use the declared currency. Both numbers must be finite and
nonnegative; booleans and quoted numbers are invalid. Preserve explicitly supplied
zero. Tax/fee strings must specify enough rate/amount, price inclusion, basis and
per-order/per-unit treatment for one unambiguous interpretation. Explicit none/0
is valid. A bare tax percentage is not sufficient. Currency must denote a monetary
currency, not simply any three letters; use trusted currency data or verify it.
Never infer business currency from country, page currency or user location.
Do not convert amounts. Delegate the financial interpretation and arithmetic to
the unchanged strategy skill and its economics reference; a schema pass alone is
not proof of financially sound input or a viable offer.

## Caller-supplied reference board

The public board field is the draft-prompts skill's original string locator.
Open actual pixels through an authorized image reader. A filename/URL alone is
not inspected evidence. It must be a clear multi-view reference for one complete
product/SKU compatible with the profiled item. The board is appearance evidence,
not proof of composition, performance, certificates or saleable branding.

For a public HTTP(S) locator, preserve the caller's durable URL only after checking
it resolves to the actual board. Do not persist expiring bearer URLs, access tokens
or credentials. For a local path, require board_handoff_binding: a trusted host
resolver for an already-existing durable HTTP(S) location of the SAME bytes. Verify
content identity (full bytes/checksum), authorization and downstream accessibility;
map relative paths only within an explicitly authorized workspace. The binding is
not a model-invented URL, not an upload permission, and not a new business input.
No mapping means stop with board_handoff_unavailable and ask the caller for a durable
board URL. Never create/copy/alter a board or change sharing as an implicit fallback.

Pass the readable local/URL locator to the prompt skill; save the verified durable
reference in the artifact's product_reference_board. Keep the board stable through
inspection/publication; a changed/unverifiable source requires clarification, not
silently using another asset. Downstream must retrieve the same board separately.
Its location must not leak into rendered copy or generated prompt strings.

## Exact skill calls

| Skill | Input |
| --- | --- |
| ecommerce-profile-offers | {landing_page_url: input.landing_page_url} |
| ecommerce-develop-strategy | cogs_per_unit, extra_unit_cogs_per_order, sales_tax, payment_processing_fees, currency from input.costs; target_market_country from input.target_country; competitor_used_product_name from profile.product.name; competitor_claims from profile.product.key_claims; ingredients_or_materials from profile.product.ingredients_or_materials; competitor_marketing_how_it_wins from profile.product.how_it_wins |
| ecommerce-draft-prompts | Entire strategy success object, copied unchanged, plus root product_reference_board |

Preserve arrays/nulls exactly. No country or browser controls go into profiler input.
The correct selling-point path is profile.product.how_it_wins. No extra dots,
wind/wins aliases, market_strategy wrapper or dropped strategy section are allowed.
Keep the full skill outputs as marketing_strategy and image_strategy, without
inserting the board into the saved marketing_strategy or altering nested fields.

## Validation gates and limitations

Validate each whole skill response against its actual bundled schema, then apply
its semantic checklist. Profile.url must exactly echo the original URL. A missing
product or blank name cannot be repaired from the URL or the board. Partial profile
errors may proceed only when the supported identity is adequate and limitations
do not make downstream assumptions materially ambiguous. Unknown materials stay
unknown; nullable how_it_wins is passed as null. A brand-only/ambiguous item stops.

Record profiler.error as profile_partial_failure and retain blockers in details.
Retain each ordinary relevant blocker as a warning (profile_limitation), not as an
invented tool failure. Those diagnostics travel in the artifact and final result.
Do not inject warning strings into competitor_claims to smuggle extra skill inputs.
Review strategy and prompts against these limitations and the board before writing.

A marketing error-only/invalid/incomplete response stops without publication.
A full strategy must satisfy the existing economics, currency/country consistency,
name-membership, shared-context, truthful proof and unique asset-reference checks.
No new profitability threshold is imposed. A proposed plan is not launch approval.

Image error-only, invalid JSON or needs_clarification stops; no partial prompt set
is saved. An ok response with [] prompts is valid after actual board inspection.
Check unique asset IDs, source-ID fidelity, positive integer terminal aspect ratio,
orientation agreement and no embedded reference URL/path. Evidence and commercial
truth checks remain the prompt skill's responsibility, not a mere regex check.

## Artifact and terminal result

Artifact: exactly job_id, signal_id, candidate_id, target_country,
product_reference_board, marketing_strategy, image_strategy, errors.
Do not add a full profile, raw costs, approval metadata or invented generated assets.
The saved strategies equal the validated outputs. Errors are accumulated, never
replaced by the latest one. Diagnostics: code, stage, severity, message, details.
Use stages input/preflight/profile/marketing/image_strategy/publication/cleanup.
Use details for recoverable IDs and relevant blockers, never credentials or
unnecessary full pages. Include upstream IDs/candidate_id in the outer result.

Terminal result also has status, published, output_file_id and output_path. No
verified file ID/path before readback. candidate_id is null until both strategies
pass and a trusted UUID is allocated; retain it on later publication failure.

| Status | Published | Meaning |
| --- | --- | --- |
| succeeded | true | Complete verified handoff, empty errors |
| partial | true | Complete verified handoff with disclosed limitations/errors |
| needs_clarification | false | Usable correction request, no handoff |
| failed | false | Validation, capability, skill or persistence failure |

When no signal_id exists, path uses the literal unlinked folder, while JSON retains
null. Optional job_id does not change the path. All supplied identifiers, target
country and strategy objects must match the input/results; check these beyond
schema. No successful terminal result means a website was implemented or deployed.
