# Output Contract

## Required Deliverable
Return a complete, buildable Next.js App Router project that exports to static files with `next build` and produces deployable output in `out/`.

The project must contain the landing page, in-site checkout, contact page, track-order page, order-status page, and policy pages. It must not contain a cart UI or server runtime.

## Required Routes
- `/`
- `/checkout`
- `/contact`
- `/track-order`
- `/order-status`
- `/privacy-policy`
- `/terms`
- `/returns-policy`
- `/shipping-policy`

Do not create:
- `/cart`
- API routes
- server actions
- middleware-dependent rewrites or redirects
- any route that requires a server at runtime

## Recommended Project Tree

```text
project-root/
  app/
    globals.css
    layout.tsx
    page.tsx
    checkout/page.tsx
    contact/page.tsx
    track-order/page.tsx
    order-status/page.tsx
    privacy-policy/page.tsx
    terms/page.tsx
    returns-policy/page.tsx
    shipping-policy/page.tsx
    not-found.tsx
  components/
    brand/
      site-logo.tsx
    checkout/
      checkout-form.tsx
      checkout-shell.tsx
      coupon-box.tsx
      order-summary.tsx
      payment-element.tsx
      shipping-fields.tsx
      billing-fields.tsx
    forms/
      contact-form.tsx
      subscription-form.tsx
      track-order-form.tsx
    layout/
      site-header.tsx
      site-footer.tsx
      home-link.tsx
      sticky-buy-bar.tsx
      policy-shell.tsx
    sections/
      hero.tsx
      purchase-moment.tsx
      offer-stack.tsx
      emotional-driver.tsx
      how-it-works.tsx
      core-promise.tsx
      use-cases.tsx
      testimonials.tsx
      faq.tsx
      final-cta.tsx
    ui/
      ...customized shadcn primitives
  config/
    offer-options.json
    site-input-summary.json
  lib/
    site-data.ts
    env.ts
    public-env.ts
    offers.ts
    quote.ts
    order.ts
    checkout-fields.ts
    checkout-validation.ts
    coupon.ts
    query-params.ts
    tracking.ts
    vendor-scripts.ts
    vendor-response.ts
    offer-pricing.ts
    utils.ts
  scripts/
    export-offer-env.mjs
    generate-public-env.mjs
    prepare-logo-assets.mjs
    check-dependency-freshness.mjs
    check-dependency-health.mjs
    check-dev-runtime.mjs
    check-project-boundaries.mjs
    check-customer-facing-copy.mjs
    deploy-cloudflare-pages.mjs
  tests/
    unit/
    components/
    integration/
    e2e/
    fixtures/
    setup.ts
  .github/workflows/
    test.yml
    prod.yml
  .vscode/
    launch.json
  source-assets/
    logo.svg
  public/
    assets/
      ...copied or organized media assets
      logo-primary.png
      logo-inverse.png
  .env.example
  .npmrc
  .gitignore
  .nvmrc
  AGENTS.md
  components.json
  next.config.mjs
  package.json
  package-lock.json
  tsconfig.json
  postcss.config.mjs
  eslint.config.mjs
  vitest.config.mts
  playwright.config.ts
  README.md
```

The project may use `src/` if the user explicitly prefers it, but the route and file responsibilities must remain equivalent. Copy every available input-provided local asset into `source-assets/` or `public/assets/`, rewrite all references to project-relative locations, and never retain a machine-specific or out-of-project path. Copy/adapt `.vscode/launch.json`, `AGENTS.md`, `next.config.mjs`, `lib/env.ts`, `lib/utils.ts`, `lib/vendor-response.ts`, `lib/offer-pricing.ts`, `components/layout/home-link.tsx`, the retained unit/E2E regressions, `scripts/prepare-logo-assets.mjs`, `scripts/check-dependency-freshness.mjs`, `scripts/check-dependency-health.mjs`, `scripts/check-project-boundaries.mjs`, `scripts/check-customer-facing-copy.mjs`, and `scripts/check-dev-runtime.mjs` from `templates/`, changing only project-specific details without weakening their contracts.

## File Responsibilities

### `app/layout.tsx`
- global shell
- analytics script insertion
- metadata
- shared header and footer except where a distraction-reduced checkout shell is intentionally used
- a linked brand logo and a visible text `Home`/`Back to home` route on every non-home page; checkout must include the text route even when its shell is simplified
- no blocking, render-heavy logic
- no server-only runtime dependency

### `app/page.tsx`
Assemble the landing page sections in the chosen order.

Must include:
- message-matched hero
- visible offer selector using `offer_options_mapping` order
- immediate prices from each mapped offer's required `default_price.amount_cents` and `default_price.currency`
- non-blocking background quote calls for all visible offers after first render
- per-offer replacement with validated quote `total`/`currency` only after a successful parsed 2xx JSON response; retain the default price on quote failure
- a usable `Buy Now` CTA before landing quotes settle, linking to `/checkout?offer=<offer_option_key>`
- `coupon` pass-through only when present in the landing-page URL
- marketing email subscription form near the footer or in the footer

### `app/checkout/page.tsx`
Must implement browser-side checkout.

Requirements:
- read `offer` query param
- validate it against expected offer-option mappings
- show a user-facing error if missing or invalid
- load and validate a live quote for selected cart and coupon before enabling payment; never use the landing default price for Stripe or order creation
- allow coupon entry/change with immediate quote refresh
- collect localized shipping fields based on `TARGET_COUNTRY`
- hide country selection
- show optional phone field always
- collect optional order notes
- include marketing subscription checkbox checked by default
- hide billing fields by default behind a same-as-shipping choice
- mount Stripe Payment Element using the validated quote total in integer cents and quote currency
- call `elements.submit()` when checkout is submitted
- call vendor `makeOrder` only after local and Stripe Element validation pass
- use `orderResponse.instruction` as Stripe `clientSecret`
- call `stripe.confirmPayment` with `return_url` derived from `window.location.origin` and `/order-status`
- parse every vendor `Response` with the shared status/JSON helper before reading body fields
- show loading, disabled, retry, and safe user-facing error states for quote, coupon, order, and Stripe flows

### `app/order-status/page.tsx`
Must rely only on Stripe `redirect_status` from the URL query string.

Do not call Stripe.js or vendor APIs to verify payment or order details.

Status behavior:
- success: say the order is being processed and email updates will follow; do not claim fulfillment is complete
- failure: explain that payment/order could not be completed and link to contact
- missing/unknown/uncertain: say status cannot be confirmed from this page and link to contact

Fire `PurchaseSuccess` or `PurchaseFailed` only from this route based on `redirect_status`.

The page must show a visible text link back to `/` near the top of the usable content.

### `app/contact/page.tsx`
Render a standalone contact form with:
- name
- email
- message

Use the vendor contact library client-side, parse its raw `Response`, and show validation, loading, success, and safe error states. Show a visible text link back to `/`.

### `app/track-order/page.tsx`
Render one tracking form with a toggle/radio selection for:
- preferred: email + numeric order number
- alternative: email + first name

Use the vendor track library client-side and parse its raw `Response`. First-name matching should be case-insensitive. Display successful tracking responses clearly and do not describe the interaction as message posting. Show a visible text link back to `/`. If the input says delivery is tracked, the result may reinforce that fact and show carrier ETA/status; do not emphasize fulfillment origin or use the prohibited origin wording defined by the input contract.

### Policy Pages
Each policy page should:
- reuse the shared visual language
- preserve allowed query params in footer/internal links
- render the provided body content with simple, readable styling
- avoid heavy conversion logic
- display a visible, keyboard-accessible text link to `/` in the first usable viewport; do not rely on a logo-only return path

### `components/layout/home-link.tsx`
Use the retained template as the shared return path on every non-home route.

Required behavior:
- render an actual anchor/link with `href="/"`
- use visible text such as `Home` or `Back to home`
- include a stable `data-home-link="true"` marker for runtime and E2E verification
- remain visible and keyboard-accessible in header, checkout shell, order status, contact, tracking, and policy layouts
- preserve only allowed query parameters when the site contract requires it, while keeping the destination pathname `/`
- a clickable logo may supplement this link but must not replace it

### `components/brand/site-logo.tsx`
Centralize all logo rendering here so header, footer, checkout, contact, order, and policy pages use one consistent brand treatment.

Required behavior:
- Read generated logo asset paths and explicit display dimensions from the generated site data/config.
- Check whether the source logo was SVG using the input contract rules.
- For SVG sources, use only transparent PNG outputs from `scripts/prepare-logo-assets.mjs`.
- Source black (`#000`) foreground must be mapped to the chosen theme color; source white (`#fff`) background must become transparent alpha.
- Provide normal/light-surface and inverse/dark-surface PNG variants when both are needed.
- Never inline, mask, fetch, or display the raw logo SVG in shopper-facing markup or CSS.
- Size the PNG responsively with explicit width and height or equivalent constraints to avoid layout shift and preserve aspect ratio.
- For non-SVG logos, render the supplied static image without color replacement.
- For a non-SVG image that is unavailable, render a styled text fallback using the brand name. A missing SVG source is a blocking generation error and must not reach this component.

### `components/forms/subscription-form.tsx`
Required near the landing page footer or inside the footer.

Fields:
- email only

Must validate email, load the vendor subscription script client-side, disable during submit, parse the returned raw `Response`, show a spinner/loading state, show success only after a parsed 2xx JSON response, and show a user-facing retry error on network, HTTP, or body failures.

### `config/offer-options.json`
Generated from `pricing_economics_and_offers.offer_options_mapping`. It should preserve input array order, retain lower-snake-case input cart fields, include the mapped offer's validated `default_price` for immediate landing display, and contain only safe app-facing metadata needed for deterministic vendor-wire environment export.

Do not include Cloudflare secrets.

### `scripts/export-offer-env.mjs`
Reads `config/offer-options.json`, validates each lower-snake-case `option_configuration`, converts its `listing`/`option`/`quantity` fields to vendor wire keys `Listing`/`Option`/`Quantity`, and sets the resulting `OFFER_OPTION__n` values.

Required behavior:
- in GitHub Actions, append `OFFER_OPTION__n=<compact vendor-wire JSON>` to `$GITHUB_ENV`
- locally, write or update a local env file used by build scripts, without overwriting hand-edited secrets
- preserve mapping order in output logs
- fail on duplicate, missing, or invalid offer option keys

### `scripts/prepare-logo-assets.mjs`
Runs before tests/build when the input logo is SVG.

Required behavior:
- detect and load the source SVG using the input-contract rules
- securely reject scripts, remote resources, external stylesheets/images, gradients or visible colors outside black/white/none/transparent
- interpret omitted foreground fill as black
- preserve `viewBox` and aspect ratio
- rasterize to RGBA at dimensions appropriate to actual website placements, normally 2x display size
- map white source pixels to alpha `0`
- map black source pixels to the selected theme foreground RGB
- convert antialiased greys to partial alpha so no white/grey halo remains
- export optimized transparent PNG assets into `public/assets/`
- never flatten onto an opaque background
- verify alpha channel, transparent pixels, foreground color, dimensions, aspect ratio, and absence of an opaque white background
- expose pure importable functions so all branches can be unit-tested
- fail clearly if validation or export is impossible
- avoid exposing or embedding deploy secrets

Follow `references/logo-processing.md`.

### `scripts/generate-public-env.mjs`
Creates the frontend-safe runtime config before `next build`.

May use generated TypeScript, JSON, or another static-export-safe approach.

It may expose:
- `APP_NAME`
- app-facing vendor variables
- analytics IDs
- contact/social URLs
- expected offer option keys and cart values

It must never expose:
- `CLOUDFLARE_ACCOUNT_ID`
- `CLOUDFLARE_API_TOKEN`

### `scripts/check-dependency-freshness.mjs`
Must inspect all direct dependencies/devDependencies, require stable caret ranges, read exact lockfile resolutions, query npm for the latest stable release accepted by each complete caret range, and fail when the lockfile is behind that release. It must report but not automatically cross a newer incompatible release.

It must not modify package files silently and must not use `--force` or `--legacy-peer-deps` as a resolution strategy.

### `scripts/check-dependency-health.mjs`
Must validate the installed/resolved dependency graph without modifying package versions.

Required behavior:
- validate version-qualified `package.json.allowScripts` decisions against every lockfile entry with `hasInstallScript: true`
- require a reviewed true decision for the resolved workerd install script when applicable
- run `npm ci --dry-run --strict-allow-scripts --no-audit --no-fund` or an equivalent strict lockfile resolution check
- fail on any npm warning, pending install-script approval, peer override, engine mismatch, deprecation, or non-zero exit
- run `npm ls --all` and fail on invalid, extraneous, missing, or peer-invalid packages
- verify the running Node version satisfies `package.json#engines.node`
- never use `--force` or `--legacy-peer-deps`

### `scripts/check-dev-runtime.mjs`
Must start the Next.js dev server, visit every required route at 320px, 360px, 390px, and 430px through localhost and a detected LAN IPv4 origin when available, capture server/browser output, and fail on first-party/source-less/unexpected warnings, cross-origin notices, deprecations, outdated-package notices, page errors, unhandled rejections, hydration errors, or first-party request failures. Run a clean browser with extensions disabled. Only the narrowly source-and-message-matched external diagnostics defined in `references/quality-and-testing.md` may be reported as external diagnostics rather than application defects.

It must terminate the child process cleanly in all paths.

### `scripts/deploy-cloudflare-pages.mjs`
Must deploy `out/` to Cloudflare Pages with Wrangler/API.

Required behavior:
1. Verify `APP_NAME`, `CLOUDFLARE_ACCOUNT_ID`, and `CLOUDFLARE_API_TOKEN` are present.
2. Use existing `out/` or run the build only if the script is explicitly designed to do so without violating the npm workflow order.
3. Check whether the Cloudflare Pages project named by `APP_NAME` exists.
4. Create/provision the project if it does not exist.
5. Deploy `out/` to that project.
6. Configure the custom domain `<APP_NAME>.listings.niobium.co.nz`.
7. Create or update the required Cloudflare DNS record automatically.
8. Never print secrets.

### `.github/workflows/test.yml`
Triggered by pushes to every branch except `main`, pull requests whose base branch is not `main`, and manual dispatch. Use `branches-ignore: [main]` semantics for both push and pull_request and do not add any feature-branch-only condition.

Must use GitHub Environment `test`. After `npm ci --strict-allow-scripts`, Playwright browser installation, and offer-env preparation, run:

```bash
npm run quality
npm run deploy
```

`npm run quality` includes dependency freshness/health, zero-warning lint, typecheck, 100% unit/component coverage, static build, E2E, and warning-free dev-runtime checks. Deployment must occur only after it passes.

### `.github/workflows/prod.yml`
Triggered by:
- pull requests targeting `main`
- pushes to `main`

For PRs to `main`, install dependencies/browsers, prepare offer env values, and run validation only:

```bash
npm run quality
```

For pushes to `main`, use GitHub Environment `prod`, perform the same setup, and run:

```bash
npm run quality
npm run deploy
```

`npm run deploy` must not take an environment argument.

### `lib/env.ts`
- validate all public build-time values centrally
- parse `SHIPPING_OPTION_ID` from its full decimal string into a safe positive integer
- expose `shipping_option_id` to application code as `number`
- reject whitespace, signs, decimals, exponent notation, unsafe integers, zero, negatives, and missing values
- never expose Cloudflare deployment secrets

### `lib/offers.ts`
- expose visible offer metadata in input order
- map `offer_option_key` to `OFFER_OPTION__n`
- parse expected cart JSON from public config/environment
- throw visible runtime errors for missing or invalid expected offer options
- never silently fall back to another offer

### `lib/vendor-response.ts`
Copy/adapt `templates/lib/vendor-response.ts` as the sole shared transport boundary for vendor methods returning `Promise<Response>`.

Required behavior:
- catch rejected fetch/network promises
- verify the value is Response-like
- call `response.json()` exactly once
- check both `response.ok` and 2xx status before accepting the body
- reject malformed, empty, or structurally invalid JSON
- expose operation-specific, user-safe messages for quote, order, tracking, subscription, and contact failures
- retain HTTP status and machine-readable failure kind for tests/telemetry without rendering raw backend bodies to shoppers

### `lib/offer-pricing.ts`
Copy/adapt `templates/lib/offer-pricing.ts` for landing-page price state only.

Required behavior:
- create each offer's first usable display state from its validated `default_price.amount_cents` and `default_price.currency`
- start quote requests after hydration without blocking the offer selector, price, or `Buy Now` CTA
- replace the displayed amount/currency only after a successful status-checked, JSON-parsed, body-validated quote
- keep the default price visible when the background quote fails, while allowing a restrained non-blocking live-price notice when useful
- preserve whether the current value comes from `default` or `quote` so tests and accessible UI can distinguish the states
- never use this landing-only fallback helper for checkout totals, Stripe Elements amount, order submission, or purchase analytics

### `lib/quote.ts`
- load/call `niobium.store.getQuote`, whose return type is `Promise<Response>`
- pass `STORE_INTEGRATION_ENDPOINT` as the last argument
- accept selected cart and coupon
- route the raw response through `callVendorJson`, then validate the quote body
- preserve every amount as an integer number of cents
- provide helpers for landing quote replacement, checkout price display, and topmost listing ID by highest `lineTotal`

### `lib/order.ts`
- load/call `niobium.store.makeOrder`, whose return type is `Promise<Response>`
- pass `STORE_INTEGRATION_ENDPOINT` as the last argument
- build consignee, shipping, billing, notes, marketingSubscription, culture, and timeZone payloads
- derive billing values from shipping when billing is same as shipping
- parse/check the raw response with `callVendorJson`, validate the JSON object, and return its non-empty `instruction` as Stripe `clientSecret`
- preserve any returned monetary fields as integer cents

### `lib/checkout-fields.ts`
- define field labels and required/optional rules for supported `TARGET_COUNTRY` values
- implement lightweight validation only
- never render a country selector

### `lib/query-params.ts`
- preserve only input-whitelisted tracking params
- pass through `coupon` only when present
- build root-domain-safe internal URLs; no subfolder support is required

### `lib/utils.ts`
- provide defensive helpers with complete tests
- use `formatMoneyFromCents`, not a major-unit `formatMoney` helper
- accept only non-negative safe integer cents and a valid non-empty three-letter currency code
- divide cents by 100 exactly once inside the display formatter
- never mutate/convert the cent amount used by Stripe, quote comparison, order payloads, or analytics-internal fields
- never instantiate a currency formatter with missing/invalid currency; catch unsupported currency/locale formatter errors and return a safe placeholder

### `lib/tracking.ts`
Expose tiny helpers for required analytics events and guard missing IDs.

Checkout event payloads must include only:
- offer option
- order total
- currency
- country
- topmost listing ID with highest line total

## `package.json` Script Requirements
At minimum:

```json
{
  "scripts": {
    "prepare:app": "node scripts/export-offer-env.mjs && node scripts/prepare-logo-assets.mjs && node scripts/generate-public-env.mjs",
    "dev": "npm run prepare:app && next dev --hostname 0.0.0.0",
    "deps:check": "node scripts/check-dependency-freshness.mjs",
    "deps:scripts": "npm approve-scripts --allow-scripts-pending --json",
    "deps:health": "node scripts/check-dependency-health.mjs",
    "project:boundaries": "node scripts/check-project-boundaries.mjs",
    "test:content": "node scripts/check-customer-facing-copy.mjs",
    "lint": "eslint --max-warnings=0 .",
    "typecheck": "tsc --noEmit",
    "test": "vitest run",
    "test:coverage": "vitest run --coverage",
    "serve:static": "serve out --listen 4173 --no-clipboard",
    "test:e2e": "playwright test",
    "test:runtime": "node scripts/check-dev-runtime.mjs",
    "build": "npm run prepare:app && next build",
    "deploy": "node scripts/deploy-cloudflare-pages.mjs",
    "quality": "npm run deps:check && npm run deps:scripts && npm run deps:health && npm run project:boundaries && npm run lint && npm run typecheck && npm run test:coverage && npm run build && npm run test:content && npm run test:e2e && npm run test:runtime"
  }
}
```

The exact command order may be equivalent, but no gate may be removed. `package.json` must also contain reviewed version-qualified `allowScripts` decisions and `.npmrc` must set `strict-allow-scripts=true`.

## `next.config.mjs` Development Requirements
In addition to `output: "export"`:
- build `allowedDevOrigins` from localhost, detected non-internal LAN IPv4 addresses, and optional `DEV_ALLOWED_ORIGINS`
- never use a permissive wildcard
- set `logging.browserToTerminal` to at least `"warn"`
- keep configuration static-export compatible

## `.vscode/launch.json`
Include a valid VS Code client-side browser debug configuration equivalent to:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Next.js: debug client-side",
      "type": "chrome",
      "request": "launch",
      "url": "http://localhost:3000",
      "webRoot": "${workspaceFolder}",
      "sourceMaps": true,
      "smartStep": true,
      "skipFiles": [
        "<node_internals>/**",
        "${workspaceFolder}/node_modules/**",
        "**/node_modules/**"
      ],
      "resolveSourceMapLocations": [
        "${workspaceFolder}/**",
        "!**/node_modules/**"
      ]
    }
  ]
}
```

Additional server/full-stack configurations are allowed, but the client-side configuration is mandatory. Source maps must remain enabled for workspace application code while `node_modules` source maps are excluded, preventing the debugger from trying to resolve malformed or missing third-party Next.js/React maps. Normal React DevTools suggestions and HMR connection notices are informational rather than warning-gate failures.

## Test Configuration
- `vitest.config.mts` must use V8 coverage with 100% statements, branches, functions, and lines.
- Coverage exclusions must be narrow and mechanical only.
- `playwright.config.ts` must use `webServer.command: "npm run serve:static"`, use a base URL on the matching fixed port, retain failure artifacts, and run route/flow tests.
- Browser listeners must be installed before navigation and fail on warnings/errors/page errors/request failures; informational `console.log`/`console.info` messages such as DevTools suggestions or HMR connectivity are not failures.
- Tests must mock vendor methods with real `Response` objects, Stripe, recaptcha, analytics, and deployment integrations.
- Tests must cover network rejection, representative non-2xx statuses, malformed/empty JSON, cent conversion, immediate default pricing, background quote replacement/retention, and click-through home navigation from every non-home route.
- Every runtime defect fixed during generation requires a regression test.

## Build Expectations
- `next.config.mjs` must set `output: 'export'`.
- Do not rely on server-only features.
- Do not rely on the default Next.js image optimizer in static export mode.
- Keep the landing page static and client-light where possible.
- Runtime vendor integrations are browser-only client components.
- The final bundle must be deployable as static files at the root of its domain.
- Internal URLs do not need subfolder-safe handling.

## CTA Rules
- Every purchase CTA says `Buy Now` or a very close variant that still clearly means immediate purchase.
- The CTA must lead to `/checkout?offer=<offer_option_key>`, not to cart.
- The selected offer and coupon pass-through should be reflected in the checkout URL.

## Footer Requirements
The footer should include trust-policy links, social trust signals, contact reassurance, track-order link, and the subscription form area.

At minimum include links to:
- contact page
- track order page
- privacy policy
- terms
- returns policy
- shipping policy

Any page that displays social links must read from `FACEBOOK_URL` and `INSTAGRAM_URL`. Any page that displays support email must read from `CONTACT_EMAIL`.

## README Requirements
The output project should include a short `README.md` with:
- install command
- dev command
- lint command
- build command
- deploy command
- static export output location
- required environment variables by environment
- offer-option mapping summary
- route list
- in-project locations and provenance for every copied source asset
- any unresolved TODOs caused by missing input

## Acceptance Checklist
Before finalizing, confirm:
- every visible string is written for potential customers, not the owner/operator/developer
- built HTML contains no Unicode em dash
- headings pass 320px, 360px, 390px, and 430px viewport checks without overflow; short headings fit within two lines
- at least three supplied testimonials render visibly with required data attributes
- checkout uses `Coupon applied to this order` for an applied coupon and never renders `Active coupon`
- all project files/tasks use self-contained project-relative resources; source SVG is copied to `source-assets/logo.svg`
- direct dependencies use caret ranges, lockfile resolutions are current within declared caret ranges, and reviewed install scripts are represented in `allowScripts`
- test workflow runs on all non-main pushes and non-main pull requests and supports manual dispatch
- static export is configured
- no cart route or shopper-facing `Add to Cart` language remains
- all required routes exist
- footer links to required pages and subscription area
- CTA uses `/checkout?offer=<offer_option_key>`
- `offer_options_mapping` order is preserved
- `OFFER_OPTION__n` values are generated by converting lower-snake-case `option_configuration` items to vendor wire cart keys
- missing/invalid offer option config throws runtime errors
- every mapped offer has a valid integer-cent `default_price`; landing renders it immediately and starts a non-blocking background quote refresh
- quote behavior exists on landing and checkout; landing failure retains default price while checkout failure blocks payment safely
- landing prices render immediately from validated `default_price` values in cents, then update only from successfully parsed live quotes; checkout/payment values are live-quote-only
- coupon priority rules are implemented
- Stripe Payment Element deferred-intent flow is implemented
- order creation uses selected cart items and selected coupon
- order status relies only on `redirect_status`
- quote, order, contact, subscription, and track-order flows treat vendor results as raw `Response` objects, check HTTP status, parse JSON once, validate bodies, expose safe user errors, and use the correct final integration-endpoint argument
- checkout fields follow the country rules document
- analytics IDs are wired behind guards
- checkout analytics event timing follows `references/tracking-and-performance.md`, including `OfferSelect` with no predecessor bundle-selection event name
- `STORE_INTEGRATION_ENDPOINT` is the final argument to quote, order, and track-order calls
- `NOTIFICATION_INTEGRATION_ENDPOINT` is the final argument to subscription and contact calls
- SVG logos are validated as black-foreground/white-background sources; white becomes transparent alpha, black becomes the selected theme color, correctly sized transparent PNGs are generated, and the raw SVG is not used by the site
- tracked delivery and carrier ETA are shown only from `product_details.shipping_details`; shopper-facing source never emphasizes fulfillment origin or uses the prohibited origin wording
- every non-home route has a visible `Home`/`Back to home` text link to `/`, and automated tests click it successfully
- major above-the-fold media has explicit dimensions
- hero media is not lazy-loaded
- below-the-fold media is lazy-loaded
- `shipping_option_id` is an input integer and vendor `shippingId` is a JavaScript number
- `.vscode/launch.json` includes client-side Next.js debugging, enables workspace source maps, skips `node_modules`, and restricts source-map resolution to application code
- `scripts/prepare-logo-assets.mjs` is based on `templates/scripts/prepare-logo-assets.mjs` and preserves the white-to-transparent alpha transform
- `next.config.mjs` handles LAN `allowedDevOrigins` and forwards browser warnings/errors
- direct dependency declarations use stable caret ranges and exact lockfile resolutions are current within the declared caret ranges according to `npm run deps:check`
- `npm run lint` passes with zero warnings
- `npm run typecheck` passes
- unit/component coverage is 100% for statements, branches, functions, and lines
- Playwright covers every required route and critical flow, including clicking a visible text home link from every non-home route
- `npm run test:runtime` passes with no terminal/browser warnings or runtime errors
- `npm run build` passes
- validator passes with no warnings
