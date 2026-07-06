---
name: ecommerce-direct-buy-lander
description: build high-converting, mobile-first ecommerce product landing pages for cold meta ad traffic using next.js static export, tailwind css, customized shadcn/ui, quote-driven in-site checkout, offer-option purchase mapping, cloudflare pages ci/cd, and browser-side vendor integrations. use when AI agent must turn a structured product brief or input json into a direct-buy landing page code bundle with checkout, contact, track-order, order-status, policy pages, analytics wiring, multi-environment deployment, and performance-safe static assets. optimize for profit-first bundle presentation, truthful claims only, buy now flow, preserved ad query params, and baymard-informed ecommerce ux without carts, fake urgency, stock shadcn styling, or generic template aesthetics.
---

# Ecommerce Direct Buy Lander

## Mission
Build a complete, static-export Next.js + Tailwind + shadcn project for a single-product, direct-buy ecommerce landing page with in-site checkout, support pages, policy pages, analytics, and Cloudflare Pages deployment from structured input.

The generated webapp is a frontend-only static export. It must use browser-side vendor JavaScript integrations for quote, order creation, Stripe Payment Element payment confirmation, marketing subscription, contact, and order tracking.

## Operating Principles
- Build the page and deployable app, not a strategy memo.
- Match the incoming paid-ad promise in the first viewport.
- Optimize for direct purchase, not browsing.
- Use only validated claims from the input.
- Keep the design distinctive but purposeful.
- Honor the input font strategy. If it says system fonts only, do not override it.
- Use `Buy Now` as the dominant purchase CTA everywhere.
- Keep the project static-export compatible and performance-safe.
- Customize shadcn primitives. Never ship stock defaults.
- Use quote responses as the source of truth for all displayed pricing.

## Workflow
1. Read the input JSON. Use `references/example_input.json` only as a shape reference when the live input is missing examples.
2. Validate top-level `shortProductName`, top-level `targetCountry`, and `pricingEconomicsAndOffers.offerOptionsMapping` before coding.
3. Run `scripts/derive_offer_map.py <input-json>` and use its output as the offer-option map. Preserve the array order in `offerOptionsMapping`; never sort offers by numeric option key.
4. Check `brandSystem.logoFile` before coding the header/footer logo. If the logo is SVG, treat it as a monochrome source asset, apply the input color scheme, size it appropriately, then render/export website-ready PNG logo assets from that adjusted SVG. Use the generated PNG assets in the real website while preserving aspect ratio.
5. Identify the page's message-match anchor from `customerSegment.angleAndTrigger`, `segmentLandingPageAdaptation`, and `mobileFirstLandingPagePlan`.
6. Pick one clear art direction using `references/design-direction.md` and `references/design-system.md`.
7. Build the project tree from `references/output-contract.md`.
8. Wire environment variables, offer-option bootstrapping, Cloudflare deployment, and GitHub workflows according to `references/environment-and-deployment.md`.
9. Wire vendor quote/order/subscription/contact/track integrations according to `references/vendor-integrations.md`.
10. Localize checkout fields according to `references/country-checkout-field-rules.md`.
11. Apply checkout UX guidance from `references/checkout-principles.md` where it fits the static, in-site checkout flow.
12. Wire analytics, query-param persistence, and performance rules from `references/tracking-and-performance.md`.
13. Generate the policy pages with the shared header and footer, but keep their bodies simple.
14. Run `scripts/validate_bundle.py <project-dir> <input-json>` and fix all errors before finalizing.
15. Run the generated project's lint and build commands when code execution is available. Lint must pass with zero warnings.
16. Return the complete code bundle plus the required summary: changes, generated/modified files, environment checklist, workflow behavior, Cloudflare behavior, route list, validation results, and unresolved issues if any.

## Input Handling
Treat the input JSON as the source of truth for:
- `shortProductName`, used to derive deterministic environment-specific `APP_NAME` values
- brand name, colors, logo path, SVG-logo status, logo recolor/export treatment, generated PNG logo asset plan, and font strategy
- product definition, validated claims, limits, and use cases
- offer names, descriptions, visual order, recommended highlight, and `offerOptionsMapping`
- target country and checkout field localization
- customer segment, ad angle, hero continuity, and objections
- shipping, refund, and support constraints
- analytics IDs and query params to preserve
- vendor integration values and deployment-safe environment variables
- legal policy content, testimonials, and asset paths
- contact email and social links for trust signals

Do not invent claims, certifications, timelines, savings, prices, discounts, stock status, use cases, or vendor request shapes that are not supported by the input, quote response, or this skill's contracts.

You may infer layout, hierarchy, and concise conversion copy from validated facts.

If a provided asset path is unavailable, preserve the slot and render a graceful fallback instead of pretending the asset exists.

If the input conflicts with itself, prioritize in this order:
1. explicit user hard constraints
2. truthfulness and legal clarity
3. quote/order/checkout-routing and analytics requirements
4. deployment-secret safety
5. direct-response conversion quality
6. visual-system defaults

## Non-Negotiable Build Rules
- Output a static site only:
  - Next.js App Router
  - TypeScript
  - Tailwind CSS
  - customized shadcn/ui primitives
  - `output: 'export'`
- Browser-side/client-side implementation only for runtime integrations.
- No server runtime.
- No API routes.
- No server actions.
- No middleware dependency.
- Do not create a `/cart` route, mini-cart, cart drawer, wishlist, waitlist, countdown timer, or fake scarcity pattern.
- Do not use `Add to Cart` copy in shopper-facing UI.
- Implement in-site checkout at `/checkout`; do not hand off to an external checkout URL.
- Use a single dominant purchase CTA: `Buy Now`.
- Permit offer selection, but every visible sale option must map to an explicit `offerOptionKey` from `pricingEconomicsAndOffers.offerOptionsMapping`.
- The landing page must navigate to `/checkout?offer=<offerOptionKey>` and preserve only allowed query params, plus `coupon` when present.
- If `/checkout` is opened without a valid `offer` query param, show a user-facing error and do not silently select a fallback offer.
- Preselect the mapping marked `recommended: true` unless the input explicitly instructs otherwise.
- The marketing email subscription form is required near the footer. It is the only allowed email capture and must use the vendor subscription library.
- Keep policy pages simple and styled like the same site.
- Use SVG or CSS icons when possible. Avoid heavy icon dependencies if they hurt bundle size.
- Centralize brand logo rendering in one component. If `brandSystem.logoFile` is an SVG, assume the supplied logo is monochrome black/white, recolor it from the input palette, size it for the website, render/export PNG variants for actual site use, and use those PNG assets consistently across header, footer, checkout, and policy layouts.
- Keep all major product content on the main page. Do not hide key proof in subpages or secondary routes.
- Prefer vertically collapsed sections or accordions over tabs or mobile subpages when content is long.
- Use thumbnails for image galleries, not dot-only controls.

## Offer-Option Mapping Rules
`pricingEconomicsAndOffers.offerOptionsMapping` is required. Each mapping must contain:

```json
{
  "sourceOfferKey": "bestSellerBundle",
  "offerOptionKey": "2",
  "optionConfiguration": [
    { "Listing": 1, "Option": "Default", "Quantity": 2 }
  ],
  "recommended": true
}
```

Rules:
- `sourceOfferKey` must match a key inside `pricingEconomicsAndOffers.offerStack`.
- `offerOptionKey` is the direct numeric key used by the webapp and by the environment variable name. `offerOptionKey: "2"` maps to `OFFER_OPTION__2` and `/checkout?offer=2`.
- `optionConfiguration` is the exact JSON array value for the matching `OFFER_OPTION__n` environment variable.
- `optionConfiguration` items must contain only `Listing`, `Option`, and `Quantity`.
- UI labels, badges, descriptions, product names, and ordering metadata must stay in the marketing offer data, not in `OFFER_OPTION__n` values.
- The visible offer order is the array order. Do not sort numerically.
- Exactly one mapping should normally be marked `recommended: true`; if not, stop and ask rather than guessing the highlighted offer.
- At runtime, missing or invalid expected offer-option environment config must throw a visible JavaScript error. Do not silently fall back to another offer.

## Environment And Deployment Rules
- Support `dev`, `test`, and `prod`.
- `dev` is local-only. There is no GitHub workflow for `dev`.
- `test` and `prod` must be deployable side by side without conflict, using separate Cloudflare Pages projects.
- Use shell-safe environment variable names only.
- Use deterministic app names:
  - dev/test: `niobiumecomm-{shortProductName}-{environment}`
  - prod: `niobiumecomm-{shortProductName}`
- Treat `APP_NAME` as both the Cloudflare Pages project name and the public app name for frontend vendor calls.
- `CLOUDFLARE_ACCOUNT_ID` and `CLOUDFLARE_API_TOKEN` are deploy-only and must never appear in frontend bundles, generated public config files, or static files.
- `OFFER_OPTION__n` variables must be generated from `offerOptionsMapping[].optionConfiguration` at workflow/deploy time and made available to the build/runtime config layer.
- Currency is never an environment variable. It must come from the quote response.

See `references/environment-and-deployment.md` for the required scripts, workflow behavior, and Cloudflare Pages deployment behavior.

## Required Routes
Generate these routes at minimum:
- `/`
- `/checkout`
- `/contact`
- `/track-order`
- `/order-status`
- `/privacy-policy`
- `/terms`
- `/returns-policy`
- `/shipping-policy`

The policy route convention above is confirmed for this skill. Footer navigation must link to contact, track order, all policy pages, and the subscription form area.

## UX Rules
- Above the fold must answer:
  - what this is
  - who the target audience is
  - why it matters now
  - what visual language the target audience expects
  - what the main offer is
  - what delivery reality looks like
  - why the page is trustworthy
  - what happens when the user taps `Buy Now`
- Surface shipping or delivery timing near the first CTA and again in the FAQ.
- Prefer an estimated delivery date only when the business can state it truthfully. Otherwise use a transparent delivery window.
- Keep copy blocks short for mobile paid traffic.
- Repeat the main CTA roughly every 1 to 1.5 mobile screens without creating competing actions.
- Keep proof close to CTA blocks: demo/result, testimonial or review, support or guarantee, transparent shipping or returns, and secure checkout reassurance.
- Keep major details on-page and easy to scan.
- Avoid heavy carousels, sliders, or tab systems that bury content or hurt performance.
- Checkout must be distraction-reduced but visually continuous with the landing page.
- Checkout must mark required and optional fields clearly.
- All async vendor calls must show loading, disabled, success, and user-facing error states.

## Design Direction Rules
- Start from ad-message continuity. The landing page should feel like the next frame after the Meta ad click.
- Favor one clear art direction. Use scale contrast, layered surfaces, asymmetry, or texture intentionally. Do not default to a bland centered SaaS layout.
- Choose a deliberate aesthetic direction before coding.
- Use anti-generic heuristics.
- Strengthen composition, color, and typography guidance.
- Customize shadcn components rather than shipping stock styling.
- Reject generic newsletter/waitlist patterns. The required footer-area marketing subscription form is allowed only because it is vendor-backed and collects email only.
- Reject dual-CTA frameworks that split purchase attention.
- If the input says `system fonts only`, use system fonts and create distinction with spacing, hierarchy, weight, composition, icon treatment, and section rhythm.
- If the logo asset is SVG, preserve its shape/viewBox while replacing black/white visual output with brand colors from the input. Use the primary color on light surfaces, a light/secondary color on dark surfaces, and an accent variant only when it improves brand integration. After recoloring and sizing the SVG, export optimized PNG assets and use those PNGs in the shipped website instead of embedding the raw SVG directly in page markup.

## Analytics Rules
- Wire GA4, Meta Pixel, and Microsoft Clarity from environment variables derived from the input IDs:
  - `trackingSpec.metaPixelId` -> `META_PIXEL_ID`
  - `trackingSpec.ga4Id` -> `GOOGLE_TAG`
  - `trackingSpec.microsoftClarity` -> `CLARITY_ID`
- Fire at minimum:
  - `PageView` on every page load to GA4 and Meta Pixel
  - `CTAClick` when any `Buy Now` control is pressed
  - `OfferSelect` when the selected visible offer changes
  - `VideoPlay` when a demo video begins playback
  - `StartCheckoutForm` once per checkout page session when the customer first touches any checkout field, coupon field, notes field, billing field, or payment section
  - `InitiatePurchase` when the checkout form is submitted and Stripe Payment Element validation begins
  - `PurchaseSuccess` only from `/order-status` when Stripe `redirect_status` indicates success
  - `PurchaseFailed` only from `/order-status` for failure, missing, unknown, or uncertain status
- Do not trigger final purchase success/failure events from `/checkout`.
- For checkout events, include only offer option, order total, currency, country, and topmost listing ID with the highest quote `lineTotal`.
- Keep tracking code lightweight and isolated in layout and utility files.
- Preserve only whitelisted query params across internal links and checkout links. Also pass through `coupon` only when it is present.

## Performance Rules
- Build toward the Core Web Vitals targets and project budgets in `references/tracking-and-performance.md`.
- Treat the hero media as the likely LCP asset:
  - compress it aggressively
  - size it explicitly
  - preload only the single LCP image or poster when needed
  - never lazy-load the first-viewport hero image
- Lazy-load below-the-fold images, iframes, and non-critical video.
- Keep vendor scripts client-side, non-blocking, and loaded only where needed.
- Do not add custom fonts when the input specifies system fonts.
- Avoid heavy animation libraries unless the motion is essential and still stays within performance budget.
- Do not rely on Next.js default image optimization in a static export. Use standard `img` elements with explicit dimensions, or a static-export-compatible image strategy.

## Output Requirements
Follow `references/output-contract.md`.

Unless the user explicitly asks for a different framework, return a complete Next.js App Router project with:
- `app/` routes for the landing page, checkout, contact, track order, order status, and policy pages
- reusable `components/`
- `lib/` helpers for offers, quotes, order creation, checkout fields, tracking, query params, vendor scripts, and content mapping
- `scripts/` for build-time public env generation, offer-env export, and Cloudflare Pages deployment
- `.github/workflows/` for test and prod
- `public/` for user-provided assets
- configuration files needed to install, lint, build, export, and deploy
- a short `README.md` with install, build, deploy, environment, route, and assumptions notes

When code execution is possible, create the files. When it is not, emit the full file tree and file contents.

## Failure Mode
- If crucial content is missing, stop and ask when the missing information affects vendor request shape, offer-option mapping, analytics input shape, policy route conventions, or deployment/build conventions.
- If non-critical content is missing, still output a compilable project with narrowly-scoped `TODO` comments only where the input truly lacks required data.
- If an asset path is provided but the file itself is unavailable, preserve the path contract and show a graceful fallback component.
- If an analytics ID is missing, guard the integration and keep the build working.
- If a requested design choice conflicts with conversion clarity, trust, static-export constraints, or truthfulness, choose the safer direct-response implementation and explain the tradeoff briefly.

## References
- `references/input-contract.md` - how to interpret the input JSON and derive page decisions
- `references/output-contract.md` - required project tree and file responsibilities
- `references/environment-and-deployment.md` - environment names, APP_NAME, workflows, and Cloudflare Pages deployment
- `references/vendor-integrations.md` - quote, order, subscription, contact, track order, and Stripe integration contracts
- `references/country-checkout-field-rules.md` - supported countries and checkout field rules
- `references/checkout-principles.md` - checkout UX principles to apply where compatible
- `references/design-system.md` - visual and UX rules synthesized from the provided design system and ecommerce product-page guidance
- `references/design-direction.md` - anti-generic heuristics and visual-direction process
- `references/tracking-and-performance.md` - analytics wiring, query-param persistence, static export constraints, and performance budgets
- `references/example_input.json` - example input schema

## Scripts
- `scripts/derive_offer_map.py <input-json>` - validate and print the visible offer-option map, environment variable names, and exact `OFFER_OPTION__n` JSON values
- `scripts/validate_bundle.py <project-dir> <input-json>` - run a fast structural check on the generated project
