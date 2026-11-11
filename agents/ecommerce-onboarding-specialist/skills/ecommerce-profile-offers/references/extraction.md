# Evidence-only offer extraction

## Contents

- Evidence discipline and field sources
- Price, variants and media
- Key claims
- How-it-wins mapping
- Vendor signals and blockers

## Evidence discipline and field sources

Use only the accepted real-browser pages and their visible page evidence. Do not
supplement missing product/vendor details from model knowledge, search, competitor
sites, social destinations or another workflow. A title, badge, claim or review
can establish what the merchant says, not the truth of that assertion. Preserve
qualifiers, timeframes and conditions. Do not independently certify compliance,
safety, patents, test results, security, customer counts or authentic urgency.

Inspect the offer stack in this order: hero headline/subheadline; benefit bullets
near CTA; why-us/why-it-works/how-it-works; offer modules; guarantee/returns;
review/testimonial blocks; already-visible FAQ content; comparison sections;
legible badges; checkout-adjacent banners without entering checkout. Prioritize
specific, quantified purchase-zone evidence. Record source URL and page zone in
the execution ledger for each filled claim. Never output a hidden reasoning trace.

Read all relevant visible portions using scrolling, after mandatory overlay handling.
Do not infer hidden text from CSS, alt labels unrelated to a displayed badge or
private application state. Use read-only current-page link observations to capture
actual URLs, not to manufacture external fetches. Do not click disallowed controls
to reveal facts. Keep unavailable fields null/[] and explain meaningful gaps.

Resolve explicit conflicting product name/price/variants in favor of the PDP, while
retaining a concise conflict blocker. Keep advertorial-only claims as merchant
claims and add `Claims not corroborated on PDP` when not corroborated. A missing
PDP is not evidence that a claim is false. Do not infer audience from a person's
appearance or a product image.

## Price, variants and media

Record the evidenced primary one-click purchase price in `product.price`
as a number and the explicitly established currency separately. Keep sale or
regular-price alternatives, subscriptions, instalments, compare-at prices and
bundle terms in `bundle_offers` or the appropriate how-it-wins list. Do not calculate
an unsupported discount or substitute a recurring instalment for a full price.
For an exclusively recurring primary offer, preserve its interval/conditions in
`bundle_offers` rather than implying that its displayed price is a one-off charge.
If only a range/from-price or ambiguous selection is shown, leave the single price
null unless a default specific purchase price is actually evidenced, and add a
blocker. An ambiguous `$` symbol does not establish which dollar currency applies.
Use an observed ISO currency code when established; do not guess from location.

Record visible variant labels without selecting them. Record ingredients/materials
only when stated, not inferred from color/texture. For images/videos record actual
product-relevant HTTP/HTTPS URLs exposed by the approved page or current-page browser
observations. Resolve relative URLs against the observed page base. CDN/media URLs
may be recorded if displayed by the page, but never opened/downloaded separately.
Do not collect tracking pixels, unrelated logos/ads, invent hidden video URLs,
click play or navigate an embedded external player. Omit unusable blob/data URLs
and explain a material media gap rather than inventing a durable media URL.

## Key claims

Target five to twelve concise, deduplicated customer-facing benefits when supported.
Never pad to reach the target. Prefer transformation, time-to-result, convenience
or savings actually stated. Translate a feature into a benefit only when the page
itself provides that mapping. Otherwise retain feature wording and add `Benefits
not explicitly stated; only features found`. Avoid turning promotional copy into
claims of proven demand, profitability or superiority.

Use brief quotations only where necessary (for example CTA labels). Prefer concise
faithful paraphrases for other claims, while preserving numerical terms and scope.
Respect applicable source quotation limits; do not repeat identical merchant copy
across many lists to evade those limits. Do not paste long testimonials or policy
passages. The package examples are invented source text, not extracted copyrighted
merchant content.

## How-it-wins mapping

Keep all twelve lists separate. Prefer three to ten entries per populated list
when available, with no minimum and no invented values. Deduplicate within lists
and avoid redundant copy across lists. A fact may support different dimensions
only where the separate downstream purpose is useful.

| Field | Required kind of visible evidence |
| --- | --- |
| `positioning` | Explicit category or identity framing: premium, first/only, intended audience, at-home/professional framing, etc. Attribute superiority language to the page. |
| `differentiators` | Explicit comparative/unique wording: patented, proprietary, exclusive, unlike, only, named superiority. A generic feature alone is not a differentiator. |
| `proof_points` | Quantified reviews, customers/units, named tests/certifications/media, stated guarantee terms, described before/after or UGC references. Record as page claims. |
| `target_customer` | Explicit written audience/use segment. Capture broad 'everyone' wording when stated, not a guessed segment from imagery. |
| `objection_handling` | Visible answers about compatibility, safety, fit, shipping cost/time, returns, setup, tools or subscription requirements. |
| `comparison_claims` | Explicit versus language or a visible comparison table. Do not create a competitor comparison. |
| `calls_to_action` | Exact observed primary button text and short surrounding microcopy, primary price-zone CTA first. Never activate a transactional CTA. |
| `offers_and_guarantees` | Explicit discounts, bundles, bonuses, shipping thresholds, guarantee durations and payment flexibility, retaining terms. |
| `risk_reducers` | Stated returns, money-back/warranty, support, policy visibility or cancellation terms. Claim secure payment only when the page says it. |
| `social_proof` | Stated ratings/counts, verified-buyer labels, short testimonial meaning, named UGC/influencer references and community counts. Do not verify externally. |
| `urgency_scarcity` | Visible timers, deadline offers, stock counts, limited editions, shipping cutoffs or price-change deadlines. Record displayed state, not future validity. |
| `compliance_language` | Visible disclaimers, results-vary text, regulated-claim caveats, age statements and subscription terms. Do not interpret as legal approval. |

Use these source-prescribed blockers when applicable:
- `No explicit differentiators found; page uses generic benefits`
- `Proof badges present but not legible/textless`
- `Target customer implied by imagery but not explicitly stated`
- `No explicit comparison claims found`
- `Discount/offer shown without clear terms`
- `Urgency language present but no concrete scarcity/timer mechanism`

A word such as Hurry can be recorded minimally but is not a stock count or a timer.
An elapsed timer is merely its displayed value at retrieval. Omit illegible badges;
do not identify an unreadable certification by resemblance.

## Vendor signals and blockers

Derive brand only from page evidence, not a guess from hostname. Derive domain
from accepted scope. Detect platform only from explicit observable identifiers;
otherwise use `Unknown`. A platform name in an unrelated review or ad is not a
platform fingerprint. Do not imply that a familiar platform makes a vendor safe.

Record visible shipping, returns and privacy URLs only after scope/denylist checks.
Do not add a terms property; retain relevant terms language in existing fields.
A policy link can be stored without visiting it, but do not extract its terms
until a permitted real-browser visit actually shows them.

Trust signals may capture stated support email/phone, external social links,
contact details, guarantee/identity/security messages and quantified proof with
`source_url` pointing to the in-scope page where observed. Use concise `type` labels
such as `support_email`, `support_phone`, `social_link`, `merchant_identity` or
`review_claim`; these examples are not a restrictive enum. Do not collect unrelated
personal information, customers' contact details or account data.

Use blockers for missing/uncertain fields, conflicting statements, unsupported
currency, inaccessible hidden controls, unexpected redirects and normal crawl
limits. Deduplicate blockers and group related missing fields when it improves
readability. Avoid declaring absence across an entire site after a partial browse;
use wording such as not observed in the inspected pages.
