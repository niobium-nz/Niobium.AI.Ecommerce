# Tracking, Query Params, Static Export, And Performance

## Tracking Integration
The generated static site must support:
- GA4
- Meta Pixel
- Microsoft Clarity

Analytics IDs come from environment variables derived from the input:

```txt
META_PIXEL_ID
GOOGLE_TAG
CLARITY_ID
```

### Script Placement
Use a shared layout-level integration so analytics can load on all routes.

For Next.js, prefer `next/script` for third-party tracking. Analytics scripts are good candidates for the `afterInteractive` strategy.

Vendor quote/order/contact/track/subscription scripts should load only on the pages/components that need them, not globally unless the implementation has a clear performance reason.

### Event Wiring
Centralize event helpers in `lib/tracking.ts`.

At minimum support these events:
- `PageView`
- `CTAClick`
- `OfferSelect`
- `VideoPlay`
- `StartCheckoutForm`
- `InitiatePurchase`
- `PurchaseSuccess`
- `PurchaseFailed`

Do not emit `AddToCart`.

Do not trigger `PurchaseSuccess` or `PurchaseFailed` from `/checkout`.

### PageView
Trigger `PageView` to both GA4 and Meta Pixel when any page loads. Guard missing IDs.

### Landing-Page Events
- `CTAClick`: fire when any `Buy Now` control is pressed.
- `OfferSelect`: fire when the visible selected offer changes.
- `VideoPlay`: fire when a demo video begins playback.

The landing page should not emit final purchase-completion events.

### StartCheckoutForm
Trigger `StartCheckoutForm` only once per checkout page session.

Trigger it when the customer first touches any checkout field, including:
- shipping field interaction
- billing field interaction
- coupon field interaction
- order notes interaction
- payment section interaction

For Stripe Payment Element, interaction with the Payment Element container or supported Stripe Element change/focus events may count as payment-field interaction.

### InitiatePurchase
Trigger `InitiatePurchase` when the checkout form is submitted and Stripe Payment Element validation begins.

This event replaces the old broad `Purchase` event name to avoid confusion.

### PurchaseSuccess And PurchaseFailed
Trigger final status events only from `/order-status`, based only on the URL query string `redirect_status`.

- If `redirect_status` indicates success, trigger `PurchaseSuccess`.
- Otherwise, trigger `PurchaseFailed`.

Do not verify payment amount, currency, quote ID, order ID, selected offer, or PaymentIntent status from this page.

### Checkout Event Payload
For checkout-related events, include only:
- offer option
- order total
- currency
- country
- topmost listing ID with the highest line total

The topmost listing ID means the listing ID from the quote line with the highest `lineTotal`.

Do not include customer PII in analytics payloads.

### Guarding
If a platform ID is missing, skip that integration cleanly. Never break the build over missing analytics.

## Query Param Preservation
Preserve only the query params listed in the input, for example:
- `utm_source`
- `utm_campaign`
- `utm_content`
- `fbclid`

Preserve them across:
- CTA links to checkout
- footer links to policy/support pages
- internal in-page stateful links if present

Do not preserve arbitrary or unknown query params.

Exception: pass through `coupon` only when the current URL contains it. Do not invent a coupon query param.

## Coupon Rules
Coupon priority on checkout:
1. Manual coupon entered on checkout page.
2. Coupon from landing page query string.
3. `FALLBACK_COUPON`, only if it has a value.

When coupon changes on checkout:
- block payment interaction temporarily
- show a spinner/loading state
- re-call quote immediately
- refresh displayed price breakdown
- show user-facing errors if quote refresh fails

## Static Export Constraints
The project must use Next.js static export.

Required:
- `output: 'export'`
- routes that can be fully rendered as static files
- no server actions
- no API routes
- no middleware dependency
- no rewrites or redirects that require a server
- no cookie-dependent rendering

The deployable build output is `out/`.

The site is always deployed at the root of its domain. Internal URLs do not need subfolder-safe handling.

### Browser-Only Runtime Integrations
All vendor interactions run in the browser:
- quote calls
- order creation calls
- Stripe Elements mounting/submission/confirmation
- subscription calls
- contact calls
- track-order calls

Do not introduce server endpoints to hide or proxy these calls.

### Images In Static Export
Do not rely on the default Next.js image optimizer in a static export.

Prefer:
- standard `img` elements with explicit width and height
- or another static-export-compatible image approach

## Core Web Vitals Targets
Use these as the non-negotiable quality targets for mobile performance:
- LCP: `<= 2.5s`
- INP: `<= 200ms`
- CLS: `<= 0.1`

Treat these targets as 75th-percentile goals, not just lab-only ideals.

## Project-Level Performance Budgets
These are the skill's internal guardrails for PageSpeed-oriented output:
- initial route JavaScript should stay lean and avoid unnecessary client hydration
- do not lazy-load the main hero image or hero poster
- only one above-the-fold media asset should be eagerly prioritized
- all media must reserve space with explicit dimensions or aspect-ratio boxes
- below-the-fold media must lazy-load
- no autoplay hero video with sound
- no heavy carousel library for a simple media gallery
- avoid adding custom fonts when the input says system fonts only
- keep third-party scripts limited to required analytics and vendor integrations
- load vendor scripts page-locally and after interaction where compatible with UX

## Async UI States Checklist
Every async vendor or payment action must show clear UI states:
- quote loading and quote error
- offer-selection quote refresh
- coupon quote refresh
- subscription submit
- contact submit
- track-order submit
- order creation
- Stripe Elements submit/validation
- Stripe confirmPayment failure before redirect

Requirements:
- show spinner/loading state
- disable relevant buttons while the request is in progress
- prevent double submission
- show success when appropriate
- show user-facing errors on failure
- suggest retrying where appropriate
- do not rely only on console errors

## Mobile Performance Checklist
Before finalizing, confirm:
- the first viewport renders without waiting on analytics
- the hero media is dimensioned and not lazy-loaded
- the sticky buy bar does not shift layout when shown
- accordions animate cheaply and do not relayout the whole page unnecessarily
- policy pages reuse the same shell without loading landing-page-only interactive code
- checkout loads only the vendor scripts needed for checkout
