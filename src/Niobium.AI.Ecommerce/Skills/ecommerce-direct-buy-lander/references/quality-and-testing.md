# Quality, Testing, And Warning-Free Runtime Contract

## Completion Standard
A generated project is not complete until all quality commands pass and both terminal and browser runs are free of unhandled errors and warnings.

Do not silence, filter, suppress, or ignore an application warning merely to pass. Fix the root cause. Normal `console.log`/`console.info` development messages such as the React DevTools suggestion and `[HMR] connected` are informational, not warnings. A debugger's attempt to load malformed or missing third-party `node_modules` source maps must be prevented through launch configuration rather than counted as an application runtime defect.

## Required Test Layers
Use all of these layers:

### Unit And Component Tests
Use the latest stable versions of:
- Vitest
- `@vitest/coverage-v8`
- React Testing Library
- `@testing-library/jest-dom`
- `@testing-library/user-event`
- jsdom

Tests must cover production logic and components, including browser-only integrations through mocks.

### End-To-End Tests
Use the latest stable Playwright release.

Run at least Chromium in every CI job. Run Firefox and WebKit as well when the environment supports them without compromising the required workflow runtime.

### Static And Runtime Checks
Run:
- dependency freshness and lockfile/peer/engine health checks
- ESLint with zero warnings
- TypeScript with `tsc --noEmit`
- unit/component coverage
- static production build
- E2E tests against the built static export
- warning-free dev-runtime smoke check

## Coverage Target
Configure V8 coverage thresholds to exactly `100` for:
- statements
- branches
- functions
- lines

Apply coverage to testable production modules under:
- `app/**/*.{ts,tsx}`
- `components/**/*.{ts,tsx}`
- `lib/**/*.{ts,tsx}`
- importable logic under `scripts/**/*.{js,mjs,ts}`

Allowed exclusions are narrow and mechanical only:
- `.d.ts` declarations
- generated public-env files
- test files and test fixtures
- static assets
- framework/config files that contain no executable application logic

Do not exclude a difficult file or branch merely to reach the threshold. Refactor hard-to-test logic into pure functions and test it.

## Required Unit And Component Coverage Areas
At minimum cover:
- recursive lower-snake-case validation for every input JSON object key
- lower-snake-case cart input conversion to vendor wire keys `Listing`, `Option`, and `Quantity`
- offer mapping parsing and missing/invalid env errors
- raw vendor `Response` parsing for quote, order, tracking, subscription, and contact: network rejection, Response-shape validation, `response.ok`, representative 4xx/5xx statuses, exactly-once `response.json()`, malformed JSON on 2xx, non-JSON bodies on non-2xx, empty JSON, and body-shape validation
- coupon priority
- checkout field localization and validation for every supported country
- strict positive-integer parsing for `SHIPPING_OPTION_ID`
- order payload construction, including numeric `shippingId`
- billing same-as-shipping derivation
- consignee construction
- query-param preservation
- analytics event timing and payload filtering, including `OfferSelect` on visible-offer changes
- order-status interpretation for success, failure, missing, and unknown values
- vendor script loader success, duplicate-load, timeout, and failure paths
- final-argument routing of `STORE_INTEGRATION_ENDPOINT` for quote/order/tracking and `NOTIFICATION_INTEGRATION_ENDPOINT` for subscription/contact
- rejection of missing, empty, swapped, or non-final integration endpoint arguments
- every async UI loading, disabled, success, retry, and error state
- logo source validation, white-to-transparent conversion, color application, sizing, and PNG verification
- cent amount validation, cents-to-major display conversion, money formatting, and every utility branch
- immediate `default_price` rendering, non-blocking background quote replacement, unchanged-price behavior, partial quote success, and default-price retention on quote failure
- tracked/ETA fulfillment copy rules and rejection of prohibited shopper-facing origin wording
- visible/clickable home navigation from every non-home route
- `next.config` origin discovery/normalization, wildcard rejection, and browser-to-terminal logging configuration
- dependency freshness/health script success and failure paths
- logo preparation script I/O, cache/stale-output behavior, and atomic-write failures
- Cloudflare Pages project/domain/DNS command or API request construction through mocks, without live deployment calls
- workflow trigger/environment/command ordering through YAML/static contract tests
- project-boundary tests proving source/config/test/script/manifests contain no machine-specific absolute paths, escaping relative paths, external symlinks, or local `file:`/`link:` package dependencies

## Money Formatting Contract
All monetary values returned by vendor quote/order integrations are integer cents. Input offer defaults use the same unit through `default_price.amount_cents`.

Use the retained contract in `templates/lib/utils.ts`:

```ts
export function formatMoneyFromCents(
  amountCents: number | null | undefined,
  currency: string | null | undefined,
  locale = "en",
): string {
  if (!Number.isSafeInteger(amountCents) || (amountCents ?? -1) < 0) return " - ";

  const normalizedCurrency = currency?.trim().toUpperCase();
  if (!normalizedCurrency || !/^[A-Z]{3}$/.test(normalizedCurrency)) return " - ";

  try {
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency: normalizedCurrency,
    }).format((amountCents as number) / 100);
  } catch {
    return " - ";
  }
}
```

Rules:
- Never treat a vendor amount as dollars/major units.
- Never divide a cent amount before Stripe, order construction, quote comparison, or internal analytics payload construction.
- Convert exactly once, inside a display boundary or an analytics adapter that explicitly requires major units.
- The landing page can format required input `default_price` immediately. Checkout must render a pending/error state until a valid live quote exists.
- Tests must include zero, one cent, non-round amounts, large safe integers, fractional/negative/unsafe values, undefined/null, empty/malformed/lowercase/unsupported/valid currency codes, and ensure `2495` renders as `24.95` major units rather than `2,495`.

## Vendor Response Contract Tests
Mock all vendor globals with real `Response` objects. Every integration wrapper must prove that it:
- catches a rejected `Promise<Response>` as a network failure
- calls `response.json()` exactly once
- checks `response.ok` and the 2xx status range
- rejects non-2xx statuses even when their body is valid JSON
- rejects malformed, empty, or unexpected JSON bodies
- maps failures to safe operation-specific shopper messages
- never renders raw response bodies, stack traces, endpoint URLs, or backend error details
- returns the parsed/validated JSON only after all transport checks pass

Cover at least statuses `400`, `401`, `403`, `404`, `409`, `422`, `429`, and `500`, plus a successful 2xx response for each operation.

## Playwright Coverage Areas
E2E tests must exercise every required route and primary branch:
- immediate landing default prices before any quote resolves
- landing-page quote success, unchanged price, changed-price replacement, partial success, malformed/non-2xx response, network failure, and retention of the default price without blocking `Buy Now`
- visible offer order, recommended selection, and `OfferSelect` emission
- CTA routing with offer and optional coupon
- checkout missing/invalid offer error
- coupon priority and quote refresh
- checkout field rules for the active target country
- Stripe Element interaction, validation failure, order failure, confirm failure, and success redirect
- numeric `shippingId` in the vendor order payload
- contact, subscription, and both track-order modes with the correct final integration endpoint argument
- order-status success, failure, missing, and unknown states
- analytics timing
- footer/internal navigation
- a visible `Home`/`Back to home` text link on every non-home route; click it and assert the final pathname is `/`
- truthful tracked/ETA copy when enabled, and absence of prohibited origin wording in shopper-facing pages
- responsive mobile and desktop smoke passes

Mock vendor globals and Stripe deterministically. Do not call live vendor, Stripe, recaptcha, analytics, or deployment services from automated tests.

## Browser Error Gate
Every Playwright page/context must install listeners before the first navigation and fail on:
- first-party `console.warn` or `console.error`
- source-less warning/error messages
- unexpected external warning/error messages
- `pageerror`
- unhandled promise rejection
- React hydration errors
- failed first-party resource requests

Run Playwright with its clean bundled browser. VS Code client debugging must launch an isolated browser profile with extensions disabled.

Do not broadly suppress third-party diagnostics. A warning may be classified as a known external diagnostic only when both its message and verified external source match a narrow allow rule. The retained runtime checker may classify these exact known cases:
- browser-extension `ObjectMultiplex` liveness diagnostics from extension/content-script sources
- browser-extension `MaxListenersExceededWarning` from extension/content-script sources
- Google reCAPTCHA `private-token` feature diagnostic from Google reCAPTCHA script sources

The same text from first-party, source-less, or unexpected sources remains fatal. React DevTools suggestions and HMR connection messages are informational only when emitted as log/info, not warning/error.

## Warning-Free Local Dev Gate
Generate `scripts/check-dev-runtime.mjs` from the retained `templates/scripts/check-dev-runtime.mjs`, adapting project-specific mocks only without weakening route coverage or failure detection.

It must:
1. start `npm run dev` bound to `0.0.0.0`
2. wait for the server to become ready
3. visit every required route at 320px, 360px, 390px, and 430px using localhost and at least one detected LAN IPv4 address when available
4. run browser console/page-error checks plus all semantic heading/data-headline size and line-count checks, overflow, testimonial, coupon-label, and minimum interactive-target checks
5. capture server stdout and stderr
6. fail on Next.js warnings, cross-origin warnings, deprecations, outdated-package notices, source-map resolution errors emitted by the server/debugger, unhandled errors, or runtime exceptions
7. verify every non-home route contains a visible `a[data-home-link="true"]` whose pathname is `/`, click it, and confirm home navigation succeeds
8. treat normal React DevTools and HMR informational messages as non-failures while preserving all warning/error gates
9. shut the dev server down cleanly on success or failure

Set `NODE_OPTIONS` for the check so unhandled rejections and deprecations fail rather than becoming ignorable warnings where supported.

## Next.js Dev Configuration
`next.config.mjs` must:
- set `output: "export"`
- build `allowedDevOrigins` from localhost, detected non-internal LAN IPv4 addresses, and optional comma-separated `DEV_ALLOWED_ORIGINS`
- avoid a permissive wildcard
- set `logging.browserToTerminal` to at least `"warn"` so client warnings/errors are visible during development

This must handle local access through an address such as `192.168.x.x` without producing the Next.js cross-origin warning.

`.vscode/launch.json` must keep workspace source maps enabled but set `skipFiles` and `resolveSourceMapLocations` so `node_modules` maps are not resolved. This avoids the known class of malformed/missing framework source-map lookups while preserving debugging of application TypeScript/TSX.

## Required Scripts
At minimum, preserve these gates:

```json
{
  "scripts": {
    "deps:check": "node scripts/check-dependency-freshness.mjs",
    "deps:scripts": "npm approve-scripts --allow-scripts-pending --json",
    "deps:health": "node scripts/check-dependency-health.mjs",
    "project:boundaries": "node scripts/check-project-boundaries.mjs",
    "lint": "eslint --max-warnings=0 .",
    "typecheck": "tsc --noEmit",
    "test:coverage": "vitest run --coverage",
    "build": "npm run prepare:app && next build",
    "test:content": "node scripts/check-customer-facing-copy.mjs",
    "test:e2e": "playwright test",
    "test:runtime": "node scripts/check-dev-runtime.mjs",
    "quality": "npm run deps:check && npm run deps:scripts && npm run deps:health && npm run project:boundaries && npm run lint && npm run typecheck && npm run test:coverage && npm run build && npm run test:content && npm run test:e2e && npm run test:runtime"
  }
}
```

Configure Playwright's `webServer` to serve the already-built `out/` directory. The separate runtime gate owns `next dev` warning checks.

## Final Evidence
The final response for a generated project must report actual command results for:
- dependency freshness and resolution health
- lint
- typecheck
- coverage percentages
- E2E tests
- warning-free dev-runtime check, including route-by-route clickable home navigation and no third-party `node_modules` source-map lookup failure
- build
- structural validator

Do not claim a command passed unless it was run successfully. If execution is unavailable, clearly state that the files were generated but the command could not be executed.
