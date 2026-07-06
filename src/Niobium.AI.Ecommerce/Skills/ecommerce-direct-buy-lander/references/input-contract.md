# Input Contract

## Purpose
Use the input JSON as a structured brief, not just as raw content. The agent should convert the brief into a direct-buy landing page and in-site checkout webapp that reflects the paid-ad angle, the strongest offer economics, the product's validated claims, and the explicit offer-option purchase mapping.

## Source-Of-Truth Hierarchy
1. Explicit user instructions in the current conversation.
2. Live input JSON.
3. This skill's rules and defaults.
4. The bundled example input as shape reference only.

## Required Top-Level Fields

### `short_product_name`
Required. A short, URL-safe product slug used to derive deterministic app names.

Rules:
- lowercase letters, numbers, and hyphens only
- no leading or trailing hyphen
- keep it short enough for Cloudflare Pages project names

App names derived from it:
- dev: `niobiumecomm-{short_product_name}-dev`
- test: `niobiumecomm-{short_product_name}-test`
- prod: `niobiumecomm-{short_product_name}`

### `target_country`
Required. Must be one of:

```txt
US
UK
CA
AU
SG
NZ
IE
```

Use this value to set `TARGET_COUNTRY`. The checkout page must not ask the customer to select a country.

### `vendor_integration`
Required for a working checkout and support flow. The generated project maps this block to shell-safe environment variables:

```json
{
  "tenant_id": "TENANT_ID value",
  "google_recaptcha_site_key": "GOOGLE_RECAPTCHA_SITE_KEY value",
  "store_integration_endpoint": "STORE_INTEGRATION_ENDPOINT value",
  "notification_integration_endpoint": "NOTIFICATION_INTEGRATION_ENDPOINT value",
  "stripe_public_key": "STRIPE_PUBLIC_KEY value",
  "shipping_option_id": "SHIPPING_OPTION_ID value",
  "fallback_coupon": "optional FALLBACK_COUPON value"
}
```

Do not add currency. Currency comes from quote responses only.

## What To Pull From Each Input Area

### `brand_system`
Use the brand name, logo path, and colors directly.

Before coding the logo, check `brand_system.logo_file`.

Treat the logo as SVG when either condition is true:
- the logo path extension is `.svg`, case-insensitive, ignoring query strings or hashes
- the asset file is available and its trimmed content starts with `<svg`

When the logo is SVG, assume the supplied artwork is a black/white monochrome source asset. The generated site workflow should apply the input color scheme to the logo, size it appropriately for website use, render/export website-ready PNG assets from the adjusted SVG, and use those PNG assets in the final site rather than embedding the original raw SVG directly in page markup.

SVG logo handling rules:
- Preserve the SVG `viewBox` and aspect ratio.
- Prepare the SVG as a source asset only; do not rely on serving the raw SVG directly in the final page UI when a PNG export can be produced.
- Replace solid black/white fills and strokes with `currentColor` or a CSS variable derived from the input palette during preprocessing. Preserve `fill="none"`, clipping paths, masks, and transparent regions.
- Use `primary_color` for the normal logo on light surfaces.
- Use `secondary_color`, white, or a derived light neutral for the logo on dark primary-color surfaces.
- Use `accent_color` only for a deliberate alternate mark, badge, or hover treatment; do not make the logo multicolor unless the design direction explicitly benefits from it.
- Do not hardcode black or white as the final visible logo color unless those colors are actually the selected brand palette for that surface.
- Export optimized PNG files from the recolored/sized SVG for the actual website, ideally at least a standard/light-surface variant and an inverse/dark-surface variant when both are needed.
- The generated site should reference the derived PNG logo assets in headers, footers, checkout, and policy pages.
- If the SVG cannot be safely parsed or transformed, document the fallback clearly and still avoid inventing a new logo.

Logo sizing rules:
- Keep the header logo compact and tap-safe: approximately `28-34px` tall on mobile and `32-40px` tall on desktop.
- Clamp wide wordmarks with a sensible max width, usually `160-180px` in the header and up to `200px` in the footer.
- Use `width: auto`, preserve aspect ratio, and avoid stretching.
- Provide explicit width/height or CSS dimensions to avoid layout shift.
- Ensure the logo remains legible in the checkout, contact, track-order, order-status, and policy layouts.

For non-SVG logos, preserve the provided image asset and do not attempt color replacement. Still size it explicitly and render a graceful text-brand fallback if the asset is unavailable.

Honor `font_strategy` exactly. If the input says `system fonts only`, do not introduce hosted or custom fonts.

### `tracking_spec`
Use the platform IDs exactly as provided and map them to environment variables:
- `meta_pixel_id` -> `META_PIXEL_ID`
- `ga4_id` -> `GOOGLE_TAG`
- `microsoft_clarity` -> `CLARITY_ID`

Preserve only the query params listed in `preserve_query_params`. Additionally, pass through `coupon` only when the URL contains it.

Treat `track_events` as a source of landing-page vocabulary, but checkout-related event timing must follow `references/tracking-and-performance.md`.

### `product_details`
This is the truth boundary for claims.

Pull:
- working product definition
- core problem solved
- primary use cases
- materials or construction summary
- fulfillment and refund assumptions
- recommended product name

Use `fulfillment_and_refund_assumptions` for internal copy judgment. Do not expose internal economics directly to shoppers.

### `pricing_economics_and_offers`
Use this block to decide visible offer order, highlight, and purchase mapping.

Required nested fields:
- `offer_stack`
- `offer_options_mapping`

The economics are mainly internal and should guide page emphasis, offer order, savings framing, and CTA copy. Do not surface internal CPA or margin math to customers unless the user explicitly asks for it.

Use the offer names and descriptions exactly or with only minimal copy compression.

Displayed price claims must come from vendor quote responses, not from the static input's old modeled `price_point` values. The old `price_point` strings may be used as copy-planning hints only until live quote data arrives.

### `pricing_economics_and_offers.offer_stack`
`offer_stack` contains shopper-facing marketing offer metadata keyed by source offer key:

```json
"offer_stack": {
  "single_unit_offer": {
    "name": "FurSweep Glove — Single",
    "price_point": "A$24.95",
    "description": "Entry option for one primary fur zone."
  }
}
```

The source offer key is used by `offer_options_mapping[].source_offer_key`.

### `pricing_economics_and_offers.offer_options_mapping`
Required. This array defines the visible sale options, their purchase option key, and the exact cart JSON for each option.

Example:

```json
"offer_options_mapping": [
  {
    "source_offer_key": "single_unit_offer",
    "offer_option_key": "1",
    "option_configuration": [
      { "listing": 1, "option": "Default", "quantity": 1 }
    ],
    "recommended": false
  },
  {
    "source_offer_key": "best_seller_bundle",
    "offer_option_key": "2",
    "option_configuration": [
      { "listing": 1, "option": "Default", "quantity": 2 },
      { "listing": 2, "option": "Default", "quantity": 4 }
    ],
    "recommended": true
  }
]
```

Rules:
- Preserve the array order as the visible offer order. Do not sort by `offer_option_key`.
- `source_offer_key` must exist in `offer_stack`.
- `offer_option_key` must be a positive integer or digit string and maps directly to `OFFER_OPTION__{offer_option_key}`.
- `option_configuration` is the exact JSON array value assigned to the matching environment variable at deployment/build time.
- Each `option_configuration` item must contain only `listing`, `option`, and `quantity`.
- `listing` and `quantity` must be positive integers.
- `option` must be a non-empty string.
- Do not place labels, badges, savings text, product names, or ordering metadata inside `option_configuration`.
- Exactly one mapping should normally have `recommended: true`; if this is missing or ambiguous, stop and ask.

### `mobile_first_landing_page_plan`
This is the most important structural guide.

Use it to derive:
- section order
- media priority
- FAQ topics
- friction points to pre-handle
- mobile interaction choices

If this block says `add-to-cart`, translate it to `Buy Now` and the in-site `/checkout?offer=<key>` flow.

### `customer_segment`
This drives ad-message continuity.

Use:
- `segment_summary` for emotional framing and objection handling
- `angle_and_trigger` for hero hook, first proof block, and first CTA framing
- `creative_handoffs` for visual direction
- `segment_landing_page_adaptation` for section emphasis and FAQ weighting

The first viewport should usually reflect the named angle and trigger.

### `trust_signal`
Use these to build trust honestly:
- policy content and links
- `contact_email` -> `CONTACT_EMAIL`
- `facebook_page` -> `FACEBOOK_URL`
- `instagram_page` -> `INSTAGRAM_URL`
- testimonials and reviewer locations

Testimonials can be edited for length, but do not change meaning.

### `asset_library`
Use the provided asset plan to decide what each section needs. If a listed asset file is only a placeholder, keep the section structure and use a fallback visual treatment rather than claiming the final media exists.

## Environment Variable Mapping
Generated projects should create a browser-safe public config at build time from these semantic source variables:

```txt
APP_NAME
TENANT_ID
GOOGLE_RECAPTCHA_SITE_KEY
STORE_INTEGRATION_ENDPOINT
NOTIFICATION_INTEGRATION_ENDPOINT
STRIPE_PUBLIC_KEY
SHIPPING_OPTION_ID
TARGET_COUNTRY
FALLBACK_COUPON
OFFER_OPTION__1
OFFER_OPTION__2
OFFER_OPTION__3
META_PIXEL_ID
GOOGLE_TAG
CLARITY_ID
FACEBOOK_URL
INSTAGRAM_URL
CONTACT_EMAIL
```

Deploy-only variables:

```txt
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
```

Deploy-only variables must never be copied to public config, bundled JavaScript, static HTML, or `out/`.

## Copy Guardrails
- Never invent performance claims or social proof unless the input provided them.
- Never add countdowns or scarcity unless the input validates them.
- Do not imply local fulfillment if the input says overseas fulfillment.
- If refund terms are uncertain, use the most supportable version.
- If using testimonial excerpts, keep them faithful.
- Do not hardcode prices. Use loading, pending, or quote-derived values instead.

## Page-Decision Defaults
If a live input omits a non-critical decision, use these defaults:
- visual mood: warm, natural, premium, direct-response
- headline style: short, outcome-led, concrete
- first proof block: before and after result on the primary surface
- offer emphasis: recommended mapping highlighted in place; do not move it unless the mapping order itself places it first
- CTA copy: `Buy Now`
- policy routes: `/privacy-policy`, `/terms`, `/returns-policy`, `/shipping-policy`

If a live input omits required `short_product_name`, `target_country`, `vendor_integration`, or `offer_options_mapping`, stop and ask.
