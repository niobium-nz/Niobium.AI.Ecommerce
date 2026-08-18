# Input Contract

## Purpose
Use the input JSON as a structured brief, not just as raw content. The agent should convert the brief into a direct-buy landing page and in-site checkout webapp that reflects the paid-ad angle, the strongest offer economics, the product's validated claims, and the explicit offer-option purchase mapping.

## Source-Of-Truth Hierarchy
1. Explicit user instructions in the current conversation.
2. Live input JSON.
3. This skill's rules and defaults.
4. The bundled example input as shape reference only.

## JSON Field Naming
Every input JSON object key must use lower snake case and match:

```txt
^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$
```

This rule applies recursively to top-level fields, nested fields, dynamic offer keys, segment keys, and asset identifiers. Vendor wire formats are separate: the skill may transform lower-snake-case input fields into vendor-required property names only at the vendor/environment boundary.

This is the current schema, not a compatibility layer. Reject obsolete input fields such as `checkout_url`, `price_point`, `subscription_integration_endpoint`, `contact_us_integration_endpoint` instead of silently translating them.

## Required Top-Level Fields

### `short_product_name`
Required. A short, URL-safe product slug used to derive deterministic app names.

Rules:
- lowercase letters, numbers, and hyphens only
- no leading or trailing hyphen
- keep it short enough for Cloudflare Pages project names

App names derived from it:
- dev: `ecom-{short_product_name}-dev`
- test: `ecom-{short_product_name}-test`
- prod: `ecom-{short_product_name}`

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
  "shipping_option_id": 101,
  "fallback_coupon": "optional FALLBACK_COUPON value"
}
```

`shipping_option_id` is required and must be a positive JSON integer within the JavaScript safe-integer range. Do not accept a string, decimal, zero, negative value, boolean, exponent-form value, unsafe integer, or numeric-looking token. Environment variables are strings by transport, so serialize this integer as the decimal value of `SHIPPING_OPTION_ID`, validate it strictly, and convert it back to a JavaScript `number` before passing it to any vendor function.

`store_integration_endpoint` and `notification_integration_endpoint` are required non-empty strings. The store endpoint is the final argument to `getQuote`, `makeOrder`, and `trackOrder`. The notification endpoint is the final argument to `subscribe` and `contactUs`.

Do not add a currency environment variable. Each visible offer supplies a required display-only `default_price.currency` for immediate landing-page rendering, while the live quote response remains authoritative for refreshed landing pricing and all checkout/payment pricing.

## What To Pull From Each Input Area

### `brand_system`
Use the brand name, logo path, and colors directly.

Before coding the logo, check `brand_system.logo_file`.

Treat the logo as SVG when either condition is true:
- the logo path extension is `.svg`, case-insensitive, ignoring query strings or hashes
- the asset file is available and its trimmed content starts with `<svg`

When the logo is SVG, the source contract is strict:
- foreground signs are black only: `#000`, `#000000`, or `black`
- background is white only: `#fff`, `#ffffff`, or `white`
- `none` or `transparent` is allowed where transparency already exists
- omitted SVG fill is treated as the default black foreground

Do not guess when another visible color, gradient, external image, script, external stylesheet, or remote resource is present. The generated logo preparation script must fail clearly.

The SVG source file must be locally available to the generation workflow. A missing file or remote-only URL is a blocking input error because the colors, transparency conversion, output dimensions, and PNG content cannot otherwise be verified.

The SVG is a preprocessing source asset only. The final site must use generated transparent PNG assets and must not inline, mask, or directly display the raw SVG.

SVG logo conversion rules:
- Preserve the SVG `viewBox` and aspect ratio.
- Convert every source white background pixel to alpha transparency.
- Convert every source black foreground pixel to the selected theme foreground color.
- Preserve antialiased edges by converting grey edge pixels into partial alpha rather than leaving grey or white halos.
- Use `primary_color` for the normal light-surface variant.
- Use `secondary_color` or another explicitly selected high-contrast palette color for an inverse/dark-surface variant.
- White source pixels always become transparent; they never become the inverse foreground color.
- Export RGBA PNGs without flattening them onto an opaque background.
- Verify that an alpha channel exists, transparent pixels exist when the source has white background, no opaque white rectangle remains, the foreground color is correct, and dimensions preserve aspect ratio.
- Keep the original SVG out of shopper-facing component paths. Use the generated PNGs in headers, footers, checkout, contact, track-order, order-status, and policy pages.
- Follow `references/logo-processing.md` for the complete transformation and test contract.

Logo sizing rules:
- Keep the header logo compact and tap-safe: approximately `28-34px` tall on mobile and `32-40px` tall on desktop.
- Clamp wide wordmarks with a sensible max width, usually `160-180px` in the header and up to `200px` in the footer.
- Use `width: auto`, preserve aspect ratio, and avoid stretching.
- Export at least 2x the largest intended CSS size, but do not preserve an arbitrary oversized vector coordinate size.
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
- structured shipping details
- recommended product name

Use `fulfillment_and_refund_assumptions` for internal copy judgment. Do not expose internal economics or fulfillment origin directly to shoppers.

`product_details.shipping_details` is required and must contain:

```json
{
  "tracked": true,
  "carrier_delivery_estimate": "7 - 14 business days",
  "tracking_message": "Tracking details are emailed after dispatch."
}
```

Rules:
- `tracked` must be a boolean. Mention tracked delivery only when it is `true`.
- `carrier_delivery_estimate` must be a non-empty, operationally supported customer-facing ETA.
- `tracking_message` is optional, but when supplied it must remain truthful.
- Customer-facing copy may say `Tracked delivery`, show the carrier ETA, and explain that tracking details are emailed after dispatch.
- Never use the words `oversea` or `overseas` in shopper-facing copy, and do not emphasize fulfillment origin. Do not falsely imply local dispatch or a domestic warehouse.

### `pricing_economics_and_offers`
Use this block to decide visible offer order, highlight, and purchase mapping.

Required nested fields:
- `offer_stack`
- `offer_options_mapping`

The economics are mainly internal and should guide page emphasis, offer order, savings framing, and CTA copy. Do not surface internal CPA or margin math to customers unless the user explicitly asks for it.

Use the offer names and descriptions exactly or with only minimal copy compression.

Every visible offer must define a structured `default_price`. The landing page renders this price immediately, without waiting for a vendor request, then starts quote requests in the background after hydration. A successful quote replaces the displayed amount and currency when they differ. A failed landing quote keeps the default price visible and shows only a non-blocking, user-friendly live-pricing notice when useful.

The default price is display-only. Never use it to initialize Stripe, create an order, calculate tax/shipping/discount, or bypass a live checkout quote. Checkout must obtain and validate a successful live quote before enabling payment.

### `pricing_economics_and_offers.offer_stack`
`offer_stack` contains shopper-facing marketing offer metadata keyed by source offer key:

```json
"offer_stack": {
  "single_unit_offer": {
    "name": "FurSweep Glove  -  Single",
    "default_price": {
      "amount_cents": 2495,
      "currency": "AUD"
    },
    "description": "Entry option for one primary fur zone."
  }
}
```

The source offer key is used by `offer_options_mapping[].source_offer_key`.

`default_price` rules:
- required for every offer declared in `offer_stack`, including every offer referenced by `offer_options_mapping`
- `amount_cents` must be a positive safe JSON integer and represents cents, not dollars
- `currency` must be a three-letter uppercase ISO currency code
- it is safe app-facing configuration and belongs in `config/offer-options.json`, not in `OFFER_OPTION__n`
- it is the immediate landing-page display fallback only; quote response data supersedes it when available

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
- Each input `option_configuration` item must contain only `listing`, `option`, and `quantity`.
- `listing` and `quantity` must be positive integers.
- `option` must be a non-empty string.
- At deployment/build time, transform each item to the vendor wire keys `Listing`, `Option`, and `Quantity`; compact JSON of that transformed array is the exact matching environment-variable value.
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
Required trust fields:
- `contact_email`
- `facebook_page`
- `instagram_page`
- `privacy_policy`
- `terms`
- `returns_policy`
- `shipping_policy`
- `testimonials`

`testimonials` must be an array with at least three genuine entries. Each entry must contain non-empty `name` and `testimonial` fields; optional location/rating/media may be used only when supplied and truthful.

Render every testimonial in the normal home-page document flow with the selectors required by `references/customer-facing-copy.md`. Preload the defined subset and load the remainder on demand. Do not omit customer feedback because an image is missing; use a well-designed text treatment.

Use only `instagram_page` for Instagram. There is no compatibility alias for alternative spellings.

### `asset_library`
Use the provided asset plan to decide what each section needs. Copy every available local asset into `source-assets/` or `public/assets/` inside the generated project and rewrite generated references to those project-relative locations. If a listed local asset is missing or only a placeholder, keep the section structure and use an in-project fallback visual treatment rather than claiming the final media exists. Never preserve an absolute, machine-specific, `file:` URL, or escaping relative path from the input in generated source, configuration, tests, scripts, manifests, package tasks, or static output.

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

`SHIPPING_OPTION_ID` must contain only the decimal representation of the positive integer from `vendor_integration.shipping_option_id`. Application config must expose it as `number`, not `string`, after strict validation. Never pass the raw environment string to `niobium.store.getQuote` or `niobium.store.makeOrder`.

Deploy-only variables:

```txt
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
```

Deploy-only variables must never be copied to public config, bundled JavaScript, static HTML, or `out/`.

## Customer-Facing Copy And Mobile Guardrails
Every visible string must address a potential customer, not describe the website to its owner, developer, designer, or operator. Apply `references/customer-facing-copy.md` across every route.

The generated UI must contain no Unicode em dash. Coupon-applied state must say `Coupon applied to this order`. All required mobile widths and testimonial selectors are mandatory acceptance criteria.

## Copy Guardrails
- Never invent performance claims or social proof unless the input provided them.
- Never add countdowns or scarcity unless the input validates them.
- Never use `oversea` or `overseas` in shopper-facing copy and never emphasize fulfillment origin.
- Do not falsely claim local dispatch, a domestic warehouse, or a shipping origin that the input does not support.
- When `shipping_details.tracked` is true, it is appropriate to say `Tracked delivery` and show the supported carrier ETA.
- If refund terms are uncertain, use the most supportable version.
- If using testimonial excerpts, keep them faithful.
- Render each offer's required default price immediately on the landing page, then replace it with validated quote pricing when the background quote returns.
- Checkout prices, Stripe amounts, and order totals must always come from the live quote response.

## Page-Decision Defaults
If a live input omits a non-critical decision, use these defaults:
- visual mood: warm, natural, premium, direct-response
- headline style: short, outcome-led, concrete
- first proof block: before and after result on the primary surface
- offer emphasis: recommended mapping highlighted in place; do not move it unless the mapping order itself places it first
- CTA copy: `Buy Now`
- policy routes: `/privacy-policy`, `/terms`, `/returns-policy`, `/shipping-policy`

If a live input omits required `short_product_name`, `target_country`, `vendor_integration`, positive-integer `vendor_integration.shipping_option_id`, `product_details.shipping_details`, `offer_options_mapping`, or any mapped offer's valid `default_price`, stop and ask.


## Binding Legal-Content Inputs
`trust_signal.privacy_policy`, `trust_signal.terms`, `trust_signal.returns_policy`, and `trust_signal.shipping_policy` are required local UTF-8 file paths. Remote URLs, missing files, or undecodable files are blocking errors.

Copy each source file byte-for-byte into the generated project:
- `privacy_policy` -> `content/policies/privacy-policy.md`
- `terms` -> `content/policies/terms.md`
- `returns_policy` -> `content/policies/returns-policy.md`
- `shipping_policy` -> `content/policies/shipping-policy.md`

Record each project path, byte length, and SHA-256 in `config/legal-content-manifest.json`. Copy/adapt `templates/lib/legal-content.ts`, and require every policy route to call `readPolicySource` with its matching input field. Legal words, punctuation, capitalization, order, and spelling are immutable. Only the page shell, typography, spacing, headings derived from the source itself, and responsive layout may change.

## Testimonial Data Contract
Copy `trust_signal.testimonials` unchanged to `config/testimonials.json`. Import that exact file directly from the home route and pass the complete array to `<Testimonials>`. Preserve array order and every supplied field, including names, locations, media references, ratios, ratings, and testimonial text. The home page must make all entries reachable without navigation to a separate page.
