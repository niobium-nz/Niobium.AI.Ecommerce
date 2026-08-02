---
name: ecommerce-direct-buy-lander
description: build high-converting, mobile-first ecommerce product landing pages for cold meta ad traffic using latest-stable next.js static export, tailwind css, customized shadcn/ui, immediate default offer prices with background quote refresh, live-quote in-site checkout, offer-option purchase mapping, cloudflare pages ci/cd, response-safe browser-side vendor integrations, transparent png logo preprocessing, sub-agent orchestration, and full warning-free test coverage. use when chatgpt must turn a structured product brief or input json into a direct-buy landing page code bundle with checkout, contact, track-order, order-status, policy pages, analytics wiring, multi-environment deployment, debugging configuration, and performance-safe static assets. optimize for profit-first bundle presentation, truthful claims only, buy now flow, preserved ad query params, and baymard-informed ecommerce ux without carts, fake urgency, stock shadcn styling, stale dependencies, runtime warnings, or generic template aesthetics.
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
- Show each required input `default_price` immediately on the landing page, then replace it with a successfully parsed live quote when available. Checkout, Stripe, and order totals must use a validated live quote only.
- Resolve the latest stable compatible direct dependencies from npm at generation time, declare them with caret ranges, and lock the exact installed versions; do not cross the declared caret range automatically or rely on remembered versions.
- Treat every lint, type, test, coverage, dev-server, browser-console, dependency, or build warning/error as unfinished work.
- Use the sub-agent role contracts in `agents/` when delegation is available, and execute the same roles sequentially otherwise.

## Workflow
1. Read the input JSON. Use `references/example_input.json` only as a shape reference when the live input is missing examples.
2. Validate that every input JSON field uses lower snake case, then validate top-level `short_product_name`, top-level `target_country`, positive-integer `vendor_integration.shipping_option_id`, both integration endpoints, `product_details.shipping_details`, every offer `default_price` in integer cents, and `pricing_economics_and_offers.offer_options_mapping` before coding.
3. Run `scripts/derive_offer_map.py <input-json>` and use its output as the offer-option map. Preserve the array order in `offer_options_mapping`; never sort offers by numeric option key.
4. Create a shared decision record and coordinate the role agents using `references/sub-agent-orchestration.md` and `agents/`.
5. Check `brand_system.logo_file`. If it is SVG, validate the black-foreground/white-background contract, map white to transparent alpha, map black to the selected theme foreground color, size it appropriately, and export transparent PNG assets. Use only the generated PNG assets in the real website. Follow `references/logo-processing.md`.
6. Identify the page's message-match anchor from `customer_segment.angle_and_trigger`, `segment_landing_page_adaptation`, and `mobile_first_landing_page_plan`.
7. Pick one clear art direction using `references/design-direction.md` and `references/design-system.md`, then apply the customer-copy and mobile-readability contract in `references/customer-facing-copy.md`.
8. Resolve latest stable compatible dependency versions, declare caret ranges, generate the reviewed install-script allowlist, and lock exact resolutions according to `references/dependency-policy.md` before scaffolding the project.
9. Build the project tree from `references/output-contract.md`, copying/adapting retained files in `templates/`, including `.vscode/launch.json`, `AGENTS.md`, `next.config.mjs`, `lib/env.ts`, `lib/utils.ts`, `lib/vendor-response.ts`, `lib/offer-pricing.ts`, `components/layout/home-link.tsx`, logo/dependency/runtime scripts, and retained regression tests. Do not weaken their contracts.
10. Wire environment variables, strict integer parsing, offer-option bootstrapping, Cloudflare deployment, and GitHub workflows according to `references/environment-and-deployment.md`.
11. Wire vendor quote/order/subscription/contact/track integrations according to `references/vendor-integrations.md`. Treat every vendor result as `Promise<Response>`, parse one JSON body only after transport handling, validate HTTP status and response shape, and keep all vendor monetary amounts in integer cents.
12. Localize checkout fields according to `references/country-checkout-field-rules.md`.
13. Apply checkout UX guidance from `references/checkout-principles.md` where it fits the static, in-site checkout flow.
14. Wire analytics, query-param persistence, and performance rules from `references/tracking-and-performance.md`.
15. Generate the policy pages with the shared header and footer, but keep their bodies simple.
16. Add unit, component, integration, E2E, asset, and runtime tests according to `references/quality-and-testing.md`. Add a regression test before fixing every discovered runtime defect.
17. Run `scripts/validate_bundle.py <project-dir> <input-json>` and fix every error and warning before finalizing.
18. Run dependency freshness and resolution/install-script health, project-boundary audit, lint, typecheck, 100% coverage, build, rendered-content audit, E2E against the static export, and warning-free dev runtime. Fix all failures and warnings; do not lower thresholds or suppress messages.
19. Return the complete code bundle plus the required summary: changes, generated/modified files, dependency versions, environment checklist, workflow behavior, Cloudflare behavior, route list, coverage and validation results, and unresolved issues if any.

## Input Handling
Treat the input JSON as the source of truth for:
- `short_product_name`, used to derive deterministic environment-specific `APP_NAME` values
- brand name, colors, logo path, SVG-logo status, logo recolor/export treatment, generated PNG logo asset plan, and font strategy
- product definition, validated claims, limits, and use cases
- offer names, descriptions, required immediate-display `default_price` values in integer cents, visual order, recommended highlight, and `offer_options_mapping`
- target country and checkout field localization
- customer segment, ad angle, hero continuity, and objections
- shipping, refund, support, `shipping_details.tracked`, truthful carrier delivery estimate, and optional tracking message constraints
- analytics IDs and query params to preserve
- vendor integration values, positive-integer shipping option ID, and deployment-safe environment variables
- legal policy content, at least three required testimonials, and self-contained asset paths
- contact email and social links for trust signals

Do not invent claims, certifications, timelines, savings, prices, discounts, stock status, use cases, or vendor request shapes that are not supported by the input, quote response, or this skill's contracts.

You may infer layout, hierarchy, and concise conversion copy from validated facts.

Copy every available input-provided local asset into the generated project and rewrite all generated references to project-relative paths. If a non-logo local asset is unavailable, preserve the content slot with a graceful in-project fallback, but do not retain its machine-specific or out-of-project path in source, config, tests, scripts, manifests, or build tasks. An SVG logo is different: its source file must be locally available for validation and PNG conversion, or completion must stop with a clear missing-asset error.

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
- Permit offer selection, but every visible sale option must map to an explicit `offer_option_key` from `pricing_economics_and_offers.offer_options_mapping`.
- The landing page must navigate to `/checkout?offer=<offer_option_key>` and preserve only allowed query params, plus `coupon` when present.
- If `/checkout` is opened without a valid `offer` query param, show a user-facing error and do not silently select a fallback offer.
- Preselect the mapping marked `recommended: true` unless the input explicitly instructs otherwise.
- The marketing email subscription form is required near the footer. It is the only allowed email capture and must use the vendor subscription library.
- Keep policy pages simple and styled like the same site. Every non-home route must provide an immediately visible, keyboard-accessible text link back to `/`; a linked brand logo may be provided in addition but never as the only return path.
- Use SVG or CSS icons when possible. Avoid heavy icon dependencies if they hurt bundle size.
- Centralize brand logo rendering in one component. If `brand_system.logo_file` is SVG, assume black (`#000`) foreground and white (`#fff`) background, convert white to transparent alpha, recolor black foreground marks from the input palette, size the output for the website, export transparent PNG variants, and use those PNG assets consistently across header, footer, checkout, and policy layouts. Never display the raw SVG.
- Keep all major product content on the main page. Do not hide key proof in subpages or secondary routes.
- Prefer vertically collapsed sections or accordions over tabs or mobile subpages when content is long.
- Use thumbnails for image galleries, not dot-only controls.

## Offer-Option Mapping Rules
`pricing_economics_and_offers.offer_options_mapping` is required. Each mapping must contain:

```json
{
  "source_offer_key": "best_seller_bundle",
  "offer_option_key": "2",
  "option_configuration": [
    { "listing": 1, "option": "Default", "quantity": 2 }
  ],
  "recommended": true
}
```

Rules:
- `source_offer_key` must match a key inside `pricing_economics_and_offers.offer_stack`.
- `offer_option_key` is the direct numeric key used by the webapp and by the environment variable name. `offer_option_key: "2"` maps to `OFFER_OPTION__2` and `/checkout?offer=2`.
- Input `option_configuration` items must contain only lower-snake-case `listing`, `option`, and `quantity` fields.
- At build/deploy time, transform each input item into the vendor wire shape `{ "Listing": ..., "Option": ..., "Quantity": ... }`; the compact transformed array is the exact `OFFER_OPTION__n` environment value.
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
  - dev/test: `niobiumecomm-{short_product_name}-{environment}`
  - prod: `niobiumecomm-{short_product_name}`
- Treat `APP_NAME` as both the Cloudflare Pages project name and the public app name for frontend vendor calls.
- `CLOUDFLARE_ACCOUNT_ID` and `CLOUDFLARE_API_TOKEN` are deploy-only and must never appear in frontend bundles, generated public config files, or static files.
- `OFFER_OPTION__n` variables must be generated from `offer_options_mapping[].option_configuration` at workflow/deploy time by converting lower-snake-case cart items into the vendor wire keys `Listing`, `Option`, and `Quantity`, then made available to the build/runtime config layer.
- Currency is never an environment variable. The landing page uses each offer's input `default_price.currency` until a successful quote replaces it; checkout and payment use the validated live quote currency.
- `vendor_integration.shipping_option_id` is a positive integer. `SHIPPING_OPTION_ID` is its decimal environment representation and must be strictly parsed and validated into a JavaScript `number` before any vendor call.
- Map `vendor_integration.store_integration_endpoint` to `STORE_INTEGRATION_ENDPOINT` and pass it as the last argument to `getQuote`, `makeOrder`, and `trackOrder`.
- Map `vendor_integration.notification_integration_endpoint` to `NOTIFICATION_INTEGRATION_ENDPOINT` and pass it as the last argument to `subscribe` and `contactUs`.
- Local development must configure `allowedDevOrigins` dynamically and forward browser warnings/errors to the terminal.
- Exact current dependencies, lockfile, and dependency freshness checks are mandatory.

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

The policy route convention above is confirmed for this skill. Footer navigation must link to contact, track order, all policy pages, and the subscription form area. Every route except `/` must also expose a visible text return path to `/` in its first usable viewport; the checkout shell is not exempt.

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
- Surface the supplied carrier delivery estimate near the first CTA and again in the FAQ. Mention tracked delivery only when `shipping_details.tracked` is true, and never emphasize fulfillment origin or use `oversea`/`overseas` in shopper-facing copy.
- Prefer an estimated delivery date only when the business can state it truthfully. Otherwise use a transparent delivery window.
- Keep copy blocks short for mobile paid traffic. Every visible word must address a potential customer, not the website owner or operator; follow `references/customer-facing-copy.md`.
- Do not use the Unicode em dash character in customer-facing source or rendered HTML. Use a spaced hyphen (` - `) or rewrite the sentence.
- Render at least three supplied testimonials in a visible `data-testimonials="true"` home-page section; do not hide all feedback behind an interaction.
- Use responsive/fluid headings and validate 320px, 360px, 390px, and 430px widths. A short heading of at most six words and 42 characters must fit within two lines at every required mobile width.
- Repeat the main CTA roughly every 1 to 1.5 mobile screens without creating competing actions.
- Keep proof close to CTA blocks: demo/result, testimonial or review, support or guarantee, transparent shipping or returns, and secure checkout reassurance.
- Keep major details on-page and easy to scan.
- Avoid heavy carousels, sliders, or tab systems that bury content or hurt performance.
- Checkout must be distraction-reduced but visually continuous with the landing page. Use customer language such as `Complete your order`; never render internal labels such as `A focused, guest checkout.`
- When a coupon is present, label it `Coupon applied to this order`; never use `Active coupon`.
- Checkout must mark required and optional fields clearly.
- All async vendor calls must show loading, disabled, success, and user-facing error states.
- Every vendor method returns a raw `Response`. Catch network rejection, parse the response body as JSON exactly once, verify `response.ok`/2xx status, validate the expected body, and display safe operation-specific errors without exposing raw backend details.
- Treat every vendor monetary amount - including quote totals, lines, tax, shipping, discounts, and order amounts - as integer cents. Never format or divide them as though they were already major currency units; convert exactly once for display only.
- Render each offer's required `default_price.amount_cents` and `default_price.currency` immediately on the home page. Start quote calls in the background after load and replace only the corresponding displayed price after a valid successful quote. A failed landing quote must retain the default price and must not block the CTA.
- Never use the words `oversea` or `overseas` in shopper-facing copy. When `product_details.shipping_details.tracked` is true, state that the package is tracked and show the supplied carrier delivery estimate without emphasizing fulfillment origin. Never falsely claim local fulfillment.
- Every non-home page must show a conspicuous `Home` or `Back to home` text link whose destination is `/`; test the link by clicking it on every required route.

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
- If the logo asset is SVG, treat black (`#000`) as the foreground and white (`#fff`) as the background. Preserve shape/viewBox, convert white to transparent alpha, replace black foreground marks with the theme color, preserve antialiased edges as partial alpha, and export appropriately sized transparent PNG assets. Use those PNGs in the shipped website instead of embedding the raw SVG.

## Analytics Rules
- Wire GA4, Meta Pixel, and Microsoft Clarity from environment variables derived from the input IDs:
  - `tracking_spec.meta_pixel_id` -> `META_PIXEL_ID`
  - `tracking_spec.ga4_id` -> `GOOGLE_TAG`
  - `tracking_spec.microsoft_clarity` -> `CLARITY_ID`
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
- Format only validated integer-cent amounts. The landing page may use a valid input default currency immediately; live quote currency supersedes it. Convert cents to major units exactly once inside `formatMoneyFromCents` and never call `Intl.NumberFormat` with an invalid currency.
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
- reusable `components/`, including a shared visible home-return link used on every non-home route
- `lib/` helpers for offers, quotes, order creation, raw `Response` parsing, cent-based money, checkout fields, tracking, query params, vendor scripts, and content mapping
- `scripts/` for build-time public env generation, offer-env export, and Cloudflare Pages deployment
- `.github/workflows/` for test and prod
- `.vscode/launch.json` for Next.js client-side debugging, copied from the retained template in this skill
- `tests/`, Vitest, and Playwright configuration with 100% coverage thresholds
- scripts for app preparation, dependency freshness/health, and warning-free local runtime checks
- `source-assets/` and `public/` for copied input assets and generated transparent PNG assets, with no generated reference to an external local filesystem path
- current stable direct dependencies declared with major-compatible caret ranges and a committed `package-lock.json` containing exact resolved versions
- configuration files needed to install, lint, typecheck, test, build, export, and deploy
- a short `README.md` with install, build, deploy, environment, route, and assumptions notes

When code execution is possible, create the files. When it is not, emit the full file tree and file contents.

## Failure Mode
- If crucial content is missing, stop and ask when the missing information affects vendor request shape, offer-option mapping, analytics input shape, policy route conventions, or deployment/build conventions.
- If non-critical content is missing, still output a compilable project with narrowly-scoped `TODO` comments only where the input truly lacks required data.
- If a non-logo local asset path is provided but unavailable, show a graceful in-project fallback and report the missing logical asset without copying its external machine path into the project. If an SVG logo source is unavailable, fail clearly and request/provide the actual local source before completion.
- If an analytics ID is missing, guard the integration and keep the build working.
- If any generated project command emits a warning, fails a test, reports incomplete coverage, or produces a browser/runtime error, keep fixing it before completion.
- Do not suppress first-party application, dependency, Next.js, React, browser, or test warnings to make the gate pass. Classify only narrowly identified browser-extension diagnostics and the known Google reCAPTCHA `private-token` feature diagnostic by verified external source; all first-party and unexpected external warnings remain fatal. Configure VS Code client debugging with `skipFiles` and `resolveSourceMapLocations` so source maps resolve only from workspace application code and `node_modules` maps are excluded; a malformed third-party framework source map must not be requested. Normal informational development messages such as React DevTools suggestions and HMR connection notices are not warnings; actual `console.warn`, `console.error`, page errors, request failures, and server warnings remain fatal.
- If a requested design choice conflicts with conversion clarity, trust, static-export constraints, or truthfulness, choose the safer direct-response implementation and explain the tradeoff briefly.

## References
- `references/input-contract.md` - how to interpret the input JSON and derive page decisions
- `references/output-contract.md` - required project tree and file responsibilities
- `references/environment-and-deployment.md` - environment names, APP_NAME, workflows, and Cloudflare Pages deployment
- `references/vendor-integrations.md` - quote, order, subscription, contact, track order, and Stripe integration contracts
- `references/country-checkout-field-rules.md` - supported countries and checkout field rules
- `references/checkout-principles.md` - checkout UX principles to apply where compatible
- `references/customer-facing-copy.md` - shopper-facing language, mobile typography, testimonial, coupon, and rendered-content rules
- `references/design-system.md` - visual and UX rules synthesized from the provided design system and ecommerce product-page guidance
- `references/design-direction.md` - anti-generic heuristics and visual-direction process
- `references/tracking-and-performance.md` - analytics wiring, query-param persistence, static export constraints, and performance budgets
- `references/logo-processing.md` - constrained SVG validation, transparent PNG conversion, theme recoloring, sizing, and verification
- `references/dependency-policy.md` - current stable package resolution, major-compatible caret ranges, exact lockfile resolutions, install-script approval, and freshness checks
- `references/quality-and-testing.md` - 100% coverage, E2E, warning-free dev/browser runtime, and completion gates
- `references/sub-agent-orchestration.md` - role delegation, ownership, handoffs, and merge order
- `agents/` - coordinator and focused sub-agent role instructions
- `templates/.npmrc` - retained strict install-script approval configuration
- `templates/.vscode/launch.json` - retained Next.js client-side VS Code debug template
- `templates/AGENTS.md` - retained generated-project role ownership and handoff template
- `templates/next.config.mjs` - retained static-export, LAN-origin, and browser-warning configuration
- `templates/lib/env.ts` - retained strict positive-integer environment parser
- `templates/lib/utils.ts` - retained integer-cent validation and defensive display formatter
- `templates/lib/vendor-response.ts` - retained raw `Response` status/JSON parser and user-safe vendor error contract
- `templates/lib/offer-pricing.ts` - retained immediate default-price and non-blocking live-quote refresh state helpers
- `templates/components/layout/home-link.tsx` - retained visible, accessible return-to-home link
- `templates/styles/mobile-typography.css` - retained narrow-phone heading, balanced-wrap, and minimum control-height safeguards
- `templates/tests/e2e/mobile-customer-ui.spec.ts` - retained all-route mobile readability/customer-copy/testimonial/coupon tests
- `templates/tests/unit/dev-runtime-classification.test.mjs` - retained source-aware external diagnostic tests
- `templates/tests/unit/project-boundaries.test.mjs` - retained project-path boundary tests
- `templates/scripts/prepare-logo-assets.mjs` - retained secure black/white SVG to transparent themed PNG implementation
- `templates/scripts/check-dependency-freshness.mjs` - retained caret-range and latest-compatible-with-caret-range npm version gate
- `templates/scripts/check-dependency-health.mjs` - retained lockfile, peer, engine, approved install-script, and warning health gate
- `templates/scripts/check-dev-runtime.mjs` - retained localhost/LAN browser and dev-server warning/error gate
- `templates/scripts/check-project-boundaries.mjs` - retained self-contained project path and symlink audit
- `templates/scripts/check-customer-facing-copy.mjs` - retained built-HTML customer-copy/testimonial/coupon audit
- `templates/.github/workflows/test.yml` - retained all-non-main test workflow trigger contract
- `templates/.github/workflows/prod.yml` - retained main validation/deployment workflow contract
- `references/example_input.json` - example input schema

## Scripts
- `scripts/derive_offer_map.py <input-json>` - validate and print the visible offer-option map, environment variable names, and exact `OFFER_OPTION__n` JSON values
- `scripts/validate_bundle.py <project-dir> <input-json>` - run a strict structural check; warnings fail validation
- `python -m unittest discover -s tests` - validate this skill package itself
