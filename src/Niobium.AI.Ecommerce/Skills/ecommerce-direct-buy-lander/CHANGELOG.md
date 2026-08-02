# Current Skill Contract

This package is maintained as one current specification. Generated projects must implement the requirements below directly; they must not carry compatibility shims for superseded input fields, endpoint names, event names, or pricing assumptions.

## Vendor transport and error handling

- Every Niobium vendor method is treated as returning `Promise<Response>`.
- Shared transport code catches rejected network promises, verifies a Response-like value, calls `response.json()` exactly once, checks `response.ok` and 2xx status, validates the parsed body, and converts failures into operation-specific user-friendly messages.
- Raw backend response bodies, stack traces, and transport details are not rendered to shoppers.
- `STORE_INTEGRATION_ENDPOINT` is the final argument to `getQuote`, `makeOrder`, and `trackOrder`.
- `NOTIFICATION_INTEGRATION_ENDPOINT` is the final argument to `subscribe` and `contactUs`.

## Money and landing pricing

- All vendor monetary fields are integer cents.
- Every mapped offer defines `default_price.amount_cents` and `default_price.currency` in the input.
- The home page renders those validated defaults immediately and starts quote requests after hydration.
- A successful background quote can replace a displayed default; a failed landing quote keeps the default price and CTA usable.
- Checkout, Stripe, order submission, and purchase analytics require a valid live quote and never fall back to input defaults.
- Display formatting divides cents by 100 exactly once; internal quote/order/Stripe values remain cents.

## Navigation and fulfillment wording

- Every non-home route contains a visible text `Home` or `Back to home` link to `/` in the first usable viewport, in addition to any linked logo.
- Runtime and E2E tests click the link on every required non-home route and verify navigation reaches `/`.
- When input confirms tracking, shopper copy may say the package is tracked and may show the supplied carrier delivery estimate.
- Shopper-facing copy does not emphasize fulfillment origin, use `oversea`/`overseas`, or invent local-dispatch claims.

## Customer-facing mobile experience

- Every visible word is written for a potential customer; internal conversion, operator, merchandising, and website-building labels are not rendered or shipped in application source.
- The Unicode em dash character is prohibited in generated customer-facing source and HTML.
- Every mobile route is tested at 320, 360, 390, and 430 CSS pixels for heading size, short-heading line count, overflow, and actionable control dimensions.
- At least three supplied testimonials render visibly on the home page.
- Applied checkout coupons use the unambiguous label `Coupon applied to this order`.

## Debugging and runtime quality

- `.vscode/launch.json` keeps source maps for workspace code but excludes `node_modules` using `skipFiles` and `resolveSourceMapLocations`.
- Informational React DevTools prompts and `[HMR] connected` messages are not treated as defects.
- Browser warnings/errors, page exceptions, hydration errors, first-party request failures, malformed framework source-map lookups, cross-origin warnings, deprecations, and outdated dependency notices fail the runtime gate. Known browser-extension liveness/listener messages and the Google reCAPTCHA `private-token` diagnostic are classified only by verified external source; the same text from first-party or unknown sources remains fatal.
- Local development is tested through localhost and an available LAN address.

## Input, assets, and typing

- Every JSON input field uses lower snake case.
- `vendor_integration.shipping_option_id` is a positive JSON integer and is passed as a JavaScript number to vendor methods.
- SVG logos are constrained to black foreground and white background; white becomes transparent alpha, black becomes the selected theme color, and the generated site uses optimized RGBA PNG assets.
- Every available local input asset is copied into the project and all generated references are project-relative; machine-specific paths, escaping relative paths, external symlinks, and local `file:`/`link:` dependencies are rejected.
- `OfferSelect` is the offer-selection analytics event.

## Quality and delivery

- Generated projects use current stable caret ranges resolved at generation time, with exact reproducible versions stored in the committed lockfile. Routine freshness may update within the declared caret range but does not cross it automatically.
- Every dependency install script receives a version-qualified `allowScripts` decision; strict installs fail on unreviewed scripts and the resolved `workerd` script is explicitly approved when required.
- The test workflow runs for pushes to every non-main branch, pull requests whose base is non-main, and manual dispatch; it has no feature-branch-only condition.
- Lint, type checking, 100% testable-code coverage thresholds, static build, static-export E2E, dependency health, and warning-free development-runtime checks must all pass before completion.
- Focused agent roles cover orchestration, platform/dependencies, customer experience, brand assets, commerce/checkout, quality/runtime, and deployment; the same responsibilities are followed sequentially when native sub-agents are unavailable.
