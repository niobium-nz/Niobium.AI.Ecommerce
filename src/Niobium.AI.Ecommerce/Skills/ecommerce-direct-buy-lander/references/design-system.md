# Design System Synthesis

## Page Type
Single-product ecommerce landing page or PDP hybrid for cold Meta paid traffic.

## Core UX Posture
- mobile-first
- fast to understand
- warm and trustworthy
- proof-heavy without feeling scammy
- one page, one product, one job
- direct purchase, not browsing

## What To Keep From The Provided UX Rules
- minimal header with logo and minimal secondary action
- warm, natural, premium DTC feel
- early trust, pricing, shipping, and returns clarity
- strong first viewport with product media and buy area
- proof close to CTA
- warm neutrals, soft corners, subtle shadows
- thumbnails instead of dot-only galleries
- sticky mobile purchase bar after first CTA scrolls away
- accordion style for FAQs and dense detail sections
- concise copy and large tap targets

## What To Override

### Replace cart patterns with direct-buy patterns
The page should not use cart language or cart flows.

Override these common ecommerce defaults:
- `Add to Cart` -> `Buy Now`
- sticky add-to-cart bar -> sticky buy-now bar
- quantity selector -> usually remove it entirely when bundle selection already captures the intended order size
- express checkout row -> replace with trust and payment reassurance, unless the user explicitly wants separate accelerated payment buttons that still preserve offer routing

### Remove unsupported urgency
Do not add limited-batch claims, scarcity chips, or countdown timers unless the input validates them.

### Delivery copy must be concrete
When possible, phrase delivery in terms of what the customer receives, not abstract speed labels.

Good:
- `Estimated AU delivery: 7-14 days`
- `Most orders arrive in about 7-14 days across Australia`

Avoid vague language such as:
- `Fast shipping`
- `Quick dispatch`

### Use a single dominant action per viewport
A direct-response page can still have section anchors or FAQ toggles, but there should only be one dominant purchase action visible at a time.

## Section Order Default
When the input does not specify another structure, use this mobile-first order:
1. hero media plus buy box with recommended offer preselected
2. fast proof or before-and-after block
3. offer stack and savings math
4. surface compatibility or use-case proof
5. how it works
6. rinse and reuse proof
7. testimonials or usage stories
8. shipping, support, guarantee, and FAQ
9. final CTA

## Visual Tokens
Use the brand colors from the input. When derived neutrals are needed, create them from the provided brand anchors.

Default style goals:
- dark text or CTA color from the primary brand tone
- warm off-white or cream background
- restrained accent usage reserved for offer emphasis and proof highlights
- rounded, premium cards
- section backgrounds alternating between warm base and white or a very light tint

## Typography
If the input requires system fonts, use a strong system stack and create distinction with:
- larger size jumps
- stronger weight contrast
- careful letter spacing
- tighter display line-height
- compact labels and chips
- disciplined section rhythm

If the input does not constrain fonts, choose a clean, warm sans stack that still respects performance.

## Component Guidance

### Header
Minimal. Do not include store navigation, category links, search, or social clutter.

### Hero
The first proof moment should be visible or understandable immediately.

Preferred hierarchy:
- headline
- subheadline
- offer summary
- three short benefit bullets
- offer cards or radio cards
- primary `Buy Now` CTA
- shipping and trust microcopy

### Offer Cards
Offer cards should:
- make the recommended offer visually dominant
- show the shopper-relevant use case, not internal margin logic
- keep savings math obvious
- stay tap-friendly on mobile

### Proof Sections
Favor concrete demonstrations:
- before and after on a familiar surface
- material close-up
- rinse-clean demo
- testimonial with specific use case

### FAQ And Detail Blocks
Use vertically collapsed sections or accordions. Avoid sending users to internal subpages for major product information.

## Trust System
Keep trust near the buy area and again near the final CTA.

Good trust signals:
- clear delivery window
- simple returns or support policy
- secure checkout reassurance
- visible contact email
- authentic testimonials or UGC-style proof

## Motion
Use subtle motion only:
- small hover lift
- fade or slide under 200ms
- sticky bar reveal
- accordion transitions

Avoid:
- autoplay video with sound
- parallax
- count-up gimmicks
- attention-seeking badges or timers
