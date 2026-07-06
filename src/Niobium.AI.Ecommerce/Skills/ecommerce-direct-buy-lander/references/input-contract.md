# Input Contract

## Purpose
Use the input JSON as a structured brief, not just as raw content. The agent should convert the brief into a direct-buy landing page and in-site checkout webapp that reflects the paid-ad angle, the strongest offer economics, the product's validated claims, and the explicit offer-option purchase mapping.

## Source-Of-Truth Hierarchy
1. Explicit user instructions in the current conversation.
2. Live input JSON.
3. This skill's rules and defaults.
4. The bundled example input as shape reference only.

## Required Top-Level Fields

### `shortProductName`
Required. A short, URL-safe product slug used to derive deterministic app names.

Rules:
- lowercase letters, numbers, and hyphens only
- no leading or trailing hyphen
- keep it short enough for Cloudflare Pages project names

App names derived from it:
- dev: `niobiumecomm-{shortProductName}-dev`
- test: `niobiumecomm-{shortProductName}-test`
- prod: `niobiumecomm-{shortProductName}`

### `targetCountry`
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

### `vendorIntegration`
Required for a working checkout and support flow. The generated project maps this block to shell-safe environment variables:

```json
{
  "tenantId": "TENANT_ID value",
  "googleRecaptchaSiteKey": "GOOGLE_RECAPTCHA_SITE_KEY value",
  "storeIntegrationEndpoint": "STORE_INTEGRATION_ENDPOINT value",
  "notificationIntegrationEndpoint": "NOTIFICATION_INTEGRATION_ENDPOINT value",
  "stripePublicKey": "STRIPE_PUBLIC_KEY value",
  "shippingOptionId": "SHIPPING_OPTION_ID value",
  "fallbackCoupon": "optional FALLBACK_COUPON value"
}
```

Do not add currency. Currency comes from quote responses only.

## What To Pull From Each Input Area

### `brandSystem`
Use the brand name, logo path, and colors directly.

Before coding the logo, check `brandSystem.logoFile`.

Treat the logo as SVG when either condition is true:
- the logo path extension is `.svg`, case-insensitive, ignoring query strings or hashes
- the asset file is available and its trimmed content starts with `<svg`

When the logo is SVG, assume the supplied artwork is a black/white monochrome source asset. The generated site workflow should apply the input color scheme to the logo, size it appropriately for website use, render/export website-ready PNG assets from the adjusted SVG, and use those PNG assets in the final site rather than embedding the original raw SVG directly in page markup.

SVG logo handling rules:
- Preserve the SVG `viewBox` and aspect ratio.
- Prepare the SVG as a source asset only; do not rely on serving the raw SVG directly in the final page UI when a PNG export can be produced.
- Replace solid black/white fills and strokes with `currentColor` or a CSS variable derived from the input palette during preprocessing. Preserve `fill="none"`, clipping paths, masks, and transparent regions.
- Use `primaryColor` for the normal logo on light surfaces.
- Use `secondaryColor`, white, or a derived light neutral for the logo on dark primary-color surfaces.
- Use `accentColor` only for a deliberate alternate mark, badge, or hover treatment; do not make the logo multicolor unless the design direction explicitly benefits from it.
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

Honor `fontStrategy` exactly. If the input says `system fonts only`, do not introduce hosted or custom fonts.

### `trackingSpec`
Use the platform IDs exactly as provided and map them to environment variables:
- `metaPixelId` -> `META_PIXEL_ID`
- `ga4Id` -> `GOOGLE_TAG`
- `microsoftClarity` -> `CLARITY_ID`

Preserve only the query params listed in `preserveQueryParams`. Additionally, pass through `coupon` only when the URL contains it.

Treat `trackEvents` as a source of landing-page vocabulary, but checkout-related event timing must follow `references/tracking-and-performance.md`.

### Deprecated `checkoutUrl`
The previous contract used `checkoutUrl` and `:offer-short-name`. That behavior is deprecated for this skill.

Generated projects must implement checkout inside the website at `/checkout`. Do not require, render, or route to an external checkout URL.

If a live input still contains `checkoutUrl`, ignore it for purchase routing and add a short migration note in the final output.

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
Use this block to decide visible offer order, highlight, and purchase mapping.

Required nested fields:
- `offerStack`
- `offerOptionsMapping`

The economics are mainly internal and should guide page emphasis, offer order, savings framing, and CTA copy. Do not surface internal CPA or margin math to customers unless the user explicitly asks for it.

Use the offer names and descriptions exactly or with only minimal copy compression.

Displayed price claims must come from vendor quote responses, not from the static input's old modeled `pricePoint` values. The old `pricePoint` strings may be used as copy-planning hints only until live quote data arrives.

### `pricingEconomicsAndOffers.offerStack`
`offerStack` contains shopper-facing marketing offer metadata keyed by source offer key:

```json
"offerStack": {
  "singleUnitOffer": {
    "name": "FurSweep Glove — Single",
    "pricePoint": "A$24.95",
    "description": "Entry option for one primary fur zone."
  }
}
```

The source offer key is used by `offerOptionsMapping[].sourceOfferKey`.

### `pricingEconomicsAndOffers.offerOptionsMapping`
Required. This array defines the visible sale options, their purchase option key, and the exact cart JSON for each option.

Example:

```json
"offerOptionsMapping": [
  {
    "sourceOfferKey": "singleUnitOffer",
    "offerOptionKey": "1",
    "optionConfiguration": [
      { "Listing": 1, "Option": "Default", "Quantity": 1 }
    ],
    "recommended": false
  },
  {
    "sourceOfferKey": "bestSellerBundle",
    "offerOptionKey": "2",
    "optionConfiguration": [
      { "Listing": 1, "Option": "Default", "Quantity": 2 },
      { "Listing": 2, "Option": "Default", "Quantity": 4 }
    ],
    "recommended": true
  }
]
```

Rules:
- Preserve the array order as the visible offer order. Do not sort by `offerOptionKey`.
- `sourceOfferKey` must exist in `offerStack`.
- `offerOptionKey` must be a positive integer or digit string and maps directly to `OFFER_OPTION__{offerOptionKey}`.
- `optionConfiguration` is the exact JSON array value assigned to the matching environment variable at deployment/build time.
- Each `optionConfiguration` item must contain only `Listing`, `Option`, and `Quantity`.
- `Listing` and `Quantity` must be positive integers.
- `Option` must be a non-empty string.
- Do not place labels, badges, savings text, product names, or ordering metadata inside `optionConfiguration`.
- Exactly one mapping should normally have `recommended: true`; if this is missing or ambiguous, stop and ask.

### `mobileFirstLandingPagePlan`
This is the most important structural guide.

Use it to derive:
- section order
- media priority
- FAQ topics
- friction points to pre-handle
- mobile interaction choices

If this block says `add-to-cart`, translate it to `Buy Now` and the in-site `/checkout?offer=<key>` flow.

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
- `contactEmail` -> `CONTACT_EMAIL`
- `facebookPage` -> `FACEBOOK_URL`
- `instagramPage` -> `INSTAGRAM_URL`
- legacy typo fallback: `InstrgramPage` may be read only if `instagramPage` is absent
- testimonials and reviewer locations

Testimonials can be edited for length, but do not change meaning.

### `assetLibrary`
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

If a live input omits required `shortProductName`, `targetCountry`, `vendorIntegration`, or `offerOptionsMapping`, stop and ask.
