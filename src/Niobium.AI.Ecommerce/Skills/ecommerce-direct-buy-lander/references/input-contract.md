# Input Contract

## Purpose
Use the input JSON as a structured brief, not just as raw content. The agent should convert the brief into a direct-buy landing page that reflects the paid-ad angle, the strongest offer economics, and the product's validated claims.

## Source-Of-Truth Hierarchy
1. Explicit user instructions in the current conversation.
2. Live input JSON.
3. This skill's rules and defaults.
4. The bundled example input as shape reference only.

## What To Pull From Each Input Area

### `brandSystem`
Use the brand name, logo path, and colors directly.

Honor `fontStrategy` exactly. If the input says `system fonts only`, do not introduce hosted or custom fonts.

### `trackingSpec`
Use the platform IDs exactly as provided.

Preserve only the query params listed in `preserveQueryParams`.

Treat `trackEvents` as the required landing-page event vocabulary.

### `checkoutUrl`
This is the only purchase destination. The landing page does not own checkout.

Replace `:offer-short-name` with the selected offer's short name.

Do not introduce cart or add-to-cart routing.

### `productDetails`
This is the truth boundary for claims.

Pull:
- working product definition
- core problem solved
- primary use cases
- materials or construction summary
- fulfillment and refund assumptions
- recommended product name

Use `fulfillmentAndRefundAssumptions` for internal copy judgment. Do not expose internal economics directly to shoppers.

### `pricingEconomicsAndOffers`
Use this block to decide which offer should be visually dominant.

The economics are mainly internal and should guide page emphasis, offer order, savings framing, and CTA copy. Do not surface internal CPA or margin math to customers unless the user explicitly asks for it.

Use the offer names, prices, and descriptions exactly or with only minimal copy compression.

Default selection should follow `recommendedPrimaryOffer`.

### `mobileFirstLandingPagePlan`
This is the most important structural guide.

Use it to derive:
- section order
- media priority
- FAQ topics
- friction points to pre-handle
- mobile interaction choices

If this block conflicts with a generic ecommerce pattern, prefer this block.

### `customerSegment`
This drives ad-message continuity.

Use:
- `segmentSummary` for emotional framing and objection handling
- `angleAndTrigger` for hero hook, first proof block, and first CTA framing
- `creativeHandoffs` for visual direction
- `segmentLandingPageAdaptation` for section emphasis and FAQ weighting

The first viewport should usually reflect the named angle and trigger.

### `trustSignal`
Use these to build trust honestly:
- policy content and links
- contact email
- social links when useful in footer only
- testimonials and reviewer locations

Testimonials can be edited for length, but do not change meaning.

### `assetLibrary`
Use the provided asset plan to decide what each section needs. If a listed asset file is only a placeholder, keep the section structure and use a fallback visual treatment rather than claiming the final media exists.

## Offer Short Name Rules
Offer short names must be:
- alphabetic only
- lowercase in code and URLs
- stable across the same input
- tied to the selected offer, not to cart state

Preferred derivation order:
1. unique descriptive words in the offer name after removing product-name tokens and generic words like `pack`, `bundle`, `offer`, `glove`, `mitt`
2. pack-size fallback such as `single`, `twopack`, `threepack`
3. sanitized offer key fallback

Example from the bundled sample:
- `FurSweep Glove - Single` -> `single`
- `Daily Reset 2-Pack` -> `dailyreset`
- `Whole-Home 3-Pack` -> `wholehome`

If two offers would collide, append a pack-size word to the shorter duplicate.

## Copy Guardrails
- Never invent performance claims or social proof.
- Never add countdowns or scarcity unless the input validates them.
- Do not imply local fulfillment if the input says overseas fulfillment.
- If refund terms are uncertain, use the most supportable version.
- If using testimonial excerpts, keep them faithful.

## Page-Decision Defaults
If a live input omits a decision, use these defaults:
- visual mood: warm, natural, premium, direct-response
- headline style: short, outcome-led, concrete
- first proof block: before and after result on the primary surface
- offer emphasis: recommended primary offer first, single offer last
- CTA copy: `Buy Now`
- policy routes: `/privacy-policy`, `/terms`, `/returns-policy`, `/shipping-policy`
