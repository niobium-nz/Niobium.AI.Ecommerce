# Design System Synthesis

## Page Type
Single-product ecommerce landing page or PDP hybrid for cold Meta paid traffic.

## Core UX Posture
- mobile-first
- fast to understand
- warm and trustworthy
- proof-heavy without feeling scammy
- one main selling page, one product, one job
- direct purchase through in-site checkout, not browsing

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

### Replace cart patterns with direct-buy checkout patterns
The page should not use cart language or cart UI. It should use visible marketing offer options that route to `/checkout?offer=<offerOptionKey>`. The underlying vendor APIs may use a `cart` parameter, but shoppers must not see cart concepts.

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
2. purchase moment or trigger
3. offer stack and savings math
4. emotional driver
5. how it works
6. core promise
7. testimonials or usage stories
8. shipping, support, guarantee, and FAQ focused on objections relief
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

### Logo
Use the brand logo as part of the color system, not as an unstyled dropped-in asset.

If `brandSystem.logoFile` is SVG, treat it as a monochrome black/white source logo, recolor it from the input palette, then export a website-ready PNG version for actual use in the site:
- primary brand color on light header/footer surfaces
- secondary, white, or a derived light neutral on dark brand surfaces
- accent color only for a deliberate alternate state, not as the default wordmark color

Implementation should preserve the SVG viewBox/aspect ratio during preprocessing and use `currentColor`, CSS variables, or a mask-based approach to recolor the source. Then render/export optimized PNG assets and use those PNGs in the live website. Size the logo deliberately: compact in the header, slightly more generous in the footer, and never stretched. If the file is not SVG, keep it as an image asset and size it explicitly without attempting recoloring.

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
- before and after
- material close-up
- use case demo
- testimonial with specific use case

### FAQ And Detail Blocks
Avoid sending users to internal subpages for major product information by optionally using vertically collapsed sections or accordions. 

## Trust System
Keep trust near the buy area and again near the final CTA.

Good trust signals:
- clear delivery window
- simple returns or support policy
- secure in-site checkout reassurance
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


## Checkout Continuity
The `/checkout` page should feel like the next purchase step from the landing page, not a generic payment form.

Requirements:
- reduced navigation and fewer distractions than the landing page
- same typography, card style, radius, and trust tone as the landing page
- visible order summary using quote-derived price data
- clear secure-payment reassurance near the Stripe Payment Element
- required/optional field labels that match the target country
- billing fields collapsed behind a same-as-shipping choice
- coupon field visible enough for users who have one, but not visually dominant over payment completion

## Required Subscription Form
The landing page must include a small marketing email subscription form near the footer or in the footer.

It should:
- collect email only
- avoid becoming a competing hero CTA
- use loading, disabled, success, and retry-error states
- use the vendor subscription integration rather than a generic newsletter/waitlist pattern
