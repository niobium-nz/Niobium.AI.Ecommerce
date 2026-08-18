# Environment And Deployment Contract

## Supported Environments
Generated projects must support:
- `dev`
- `test`
- `prod`

`dev` is local-only. Do not create a GitHub workflow for `dev`.

`test` and `prod` must be deployable side by side without conflict, using separate Cloudflare Pages projects and separate GitHub Environments.

## Deterministic APP_NAME
The input must contain top-level `short_product_name`.

Use this deterministic naming pattern:

```txt
# dev and test
ecom-{short_product_name}-{environment}

# prod
ecom-{short_product_name}
```

Examples:

```txt
ecom-hair-remover-dev
ecom-hair-remover-test
ecom-hair-remover
```

Treat `APP_NAME` as:
- Cloudflare Pages project name
- public app name passed to vendor frontend integrations

## Shell-Safe Environment Variables
Use only shell-safe names.

Required or supported semantic variables:

```txt
APP_NAME
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
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
DEV_ALLOWED_ORIGINS
```

Offer option variables must use the double-underscore format:

```txt
OFFER_OPTION__1
OFFER_OPTION__2
```

Do not use hyphenated names such as `TENANT-ID`, `GOOGLE-RECAPTCHA-SITE-KEY`, or `OFFER-OPTION--1`.

## Public Vs Deploy-Only Variables
Deploy-only variables:

```txt
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
```

These must never be included in frontend bundles, public config, static files, generated HTML, or `out/`.

`APP_NAME` is safe to expose and required by vendor frontend integrations.

All other app-facing variables above are safe to expose in the browser bundle because the generated app is frontend-only and the vendor libraries require them, except `DEV_ALLOWED_ORIGINS`, which is local-development configuration and must remain in Next.js config/runtime tooling rather than the public app config.

Do not define currency as an environment variable. Currency comes from quote responses.

Endpoint routing is fixed:
- `STORE_INTEGRATION_ENDPOINT` is browser-safe and must be passed as the last argument to `niobium.store.getQuote`, `niobium.store.makeOrder`, and `niobium.store.trackOrder`.
- `NOTIFICATION_INTEGRATION_ENDPOINT` is browser-safe and must be passed as the last argument to `niobium.notification.subscribe` and `niobium.notification.contactUs`.

## Input-To-Environment Mapping
Use these mappings:

```txt
short_product_name + environment -> APP_NAME
vendor_integration.tenant_id -> TENANT_ID
vendor_integration.google_recaptcha_site_key -> GOOGLE_RECAPTCHA_SITE_KEY
vendor_integration.store_integration_endpoint -> STORE_INTEGRATION_ENDPOINT
vendor_integration.notification_integration_endpoint -> NOTIFICATION_INTEGRATION_ENDPOINT
vendor_integration.stripe_public_key -> STRIPE_PUBLIC_KEY
vendor_integration.shipping_option_id (positive integer) -> SHIPPING_OPTION_ID (decimal environment string, strictly parsed back to number)
target_country -> TARGET_COUNTRY
vendor_integration.fallback_coupon -> FALLBACK_COUPON
pricing_economics_and_offers.offer_options_mapping[].option_configuration -> OFFER_OPTION__{offer_option_key}
tracking_spec.meta_pixel_id -> META_PIXEL_ID
tracking_spec.ga4_id -> GOOGLE_TAG
tracking_spec.microsoft_clarity -> CLARITY_ID
trust_signal.facebook_page -> FACEBOOK_URL
trust_signal.instagram_page -> INSTAGRAM_URL
trust_signal.contact_email -> CONTACT_EMAIL
DEV_ALLOWED_ORIGINS
```


## Shipping Option Integer Contract
`vendor_integration.shipping_option_id` must be a positive JSON integer.

Environment transport converts it to text, so `SHIPPING_OPTION_ID` must contain only ASCII decimal digits with no sign, whitespace, decimal point, exponent, or suffix. The generated public environment layer must validate the full string, convert it once to a JavaScript `number`, assert `Number.isSafeInteger(value) && value > 0`, and expose only the numeric value to application code.

Never pass the raw env string to quote or order vendor calls. Both `getQuote` and `makeOrder` must receive numeric shipping IDs.

## Offer Option Environment Generation
The source of truth is `pricing_economics_and_offers.offer_options_mapping`.

For every mapping:
- read `offer_option_key`
- validate lower-snake-case input cart fields `listing`, `option`, and `quantity`
- transform each item to vendor wire keys `Listing`, `Option`, and `Quantity`
- serialize the transformed array as compact JSON
- set `OFFER_OPTION__{offer_option_key}` to that compact vendor JSON

Example:

```json
{
  "offer_option_key": "2",
  "option_configuration": [
    { "listing": 1, "option": "Default", "quantity": 2 },
    { "listing": 2, "option": "Default", "quantity": 4 }
  ]
}
```

Produces:

```txt
OFFER_OPTION__2=[{"Listing":1,"Option":"Default","Quantity":2},{"Listing":2,"Option":"Default","Quantity":4}]
```

The generated project must include `scripts/export-offer-env.mjs` and call it in workflows before build. This script should append the values to `$GITHUB_ENV` in GitHub Actions so the build and deploy steps consume normal environment variables.

## Build-Time Public Config
Static-export Next.js cannot read runtime server env at request time. Generated projects must use a safe build-time public environment layer, such as `scripts/generate-public-env.mjs` writing a generated TypeScript or JSON config.

That generated public config may include app-facing values, but must explicitly exclude:

```txt
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
```

Recommended generated files:
- `lib/public-env.ts`
- or `lib/generated/public-env.ts`

## Local Development
Local values may come from `.env`, `.env.local`, or a generated local env file.

When branch/environment detection does not identify `test` or `prod`, local commands should default to `dev`.

Required local behavior:
- `scripts/export-offer-env.mjs` writes offer option values to a local generated env file or logs shell export commands.
- `scripts/generate-public-env.mjs` resolves `APP_NAME` as `ecom-{short_product_name}-dev` if `APP_NAME` is absent.
- `.env.example` documents required local values and notes that Cloudflare secrets are needed only for deploy.
- `.env.example` may include optional `DEV_ALLOWED_ORIGINS` as a comma-separated list of extra development hostnames/IP addresses.
- `next.config.mjs` automatically includes localhost and detected non-internal LAN IPv4 addresses in `allowedDevOrigins`, merges explicitly configured `DEV_ALLOWED_ORIGINS`, and does not use a permissive wildcard.
- `next.config.mjs` sets `logging.browserToTerminal` to at least `"warn"` so client warnings/errors appear in the dev terminal.
- `npm run dev` binds to `0.0.0.0` so local devices can test the site without a cross-origin warning.
- `.vscode/launch.json` uses the retained Next.js full-stack `node-terminal` profile, runs `npm run dev`, and opens the URL matched by `serverReadyAction` through `debugWithChrome`.
- Normal React DevTools suggestions and HMR connection messages are informational. Application warnings/errors remain fatal; the debugger follows the same full-stack development command used by the project.

## Self-Contained Project Paths
Every generated project must be runnable after checkout on a clean CI runner without access to the original skill input directory or generation machine.

- Copy the input SVG logo into `source-assets/logo.svg` before any generated task references it.
- Store only project-relative paths in generated config and manifests.
- Do not emit absolute Windows, Linux, macOS, workspace, temporary, or `file://` paths.
- Do not use dependencies with local `file:` references.
- Do not create symlinks that resolve outside the generated project.
- Run `npm run project:boundaries` in the quality gate.

## npm Scripts
Generated projects must provide:

```txt
npm run prepare:app
npm run dev
npm run deps:check
npm run deps:health
npm run lint
npm run typecheck
npm run test
npm run test:coverage
npm run serve:static
npm run test:e2e
npm run test:runtime
npm run quality
npm run build
npm run deploy
```

Expected behavior:
- `npm run prepare:app`: prepares local offer env values, transparent logo PNGs, and browser-safe public configuration.
- `npm run dev`: runs `prepare:app`, then starts `next dev --hostname 0.0.0.0` with dynamic `allowedDevOrigins`.
- `npm run deps:check`: verifies every direct dependency is exact and matches the npm stable `latest` tag.
- `npm run deps:health`: verifies the lockfile, peer graph, package engines, and dry-run install are warning-free.
- `npm run lint`: runs ESLint with zero-warning enforcement.
- `npm run typecheck`: runs `tsc --noEmit`.
- `npm run test`: runs Vitest once, not watch mode.
- `npm run test:coverage`: runs Vitest with 100% statement, branch, function, and line thresholds.
- `npm run serve:static`: serves the built `out/` directory on the fixed local E2E port without downloading packages at runtime.
- `npm run test:e2e`: runs Playwright against that built static export for all required routes and flows with browser error listeners.
- `npm run test:runtime`: starts the dev server, visits localhost and a LAN origin when available, verifies clickable home navigation on every non-home route, and fails on terminal/browser warnings or runtime errors while ignoring only normal info/log messages.
- `npm run quality`: runs every freshness, static, test, runtime, and build gate required by `references/quality-and-testing.md`.
- `npm run build`: runs `prepare:app`, then `next build` to produce `out/`.
- `npm run deploy`: deploys `out/` to Cloudflare Pages using active environment variables.

Do not call the project complete unless `npm run quality` exits successfully with no warnings.

## Required Workflows
Create two workflows:

```txt
.github/workflows/test.yml
.github/workflows/prod.yml
```

Both workflows must use the Node version declared by the generated project, install from the committed lockfile with `npm ci --strict-allow-scripts`, treat install warnings as failures through `npm run deps:health`, install required Playwright browser dependencies, run `node scripts/export-offer-env.mjs`, and execute the complete warning-free quality gate before any deployment.

### Test Workflow
The test workflow must be available for every non-main branch, whether work reaches it by push or pull request, and must support manual execution.

Required trigger block:

```yaml
on:
  push:
    branches-ignore:
      - main
  pull_request:
    branches-ignore:
      - main
  workflow_dispatch:
```

Requirements:
- Do not restrict the workflow to `feature/*` branches.
- Do not add conditions such as `startsWith(github.head_ref, 'feature/')`.
- Use the GitHub Environment named `test`.
- All non-main branches share the test Cloudflare Pages project selected by the test environment variables.
- Install with `npm ci --strict-allow-scripts`.
- Install the Playwright Chromium runtime.
- Run `npm run quality` before `npm run deploy`.
- A warning, failed gate, or unreviewed install script blocks deployment.

### Prod Workflow
Trigger on:
- pull requests targeting `main`
- pushes to `main`

For pull requests targeting `main`, run validation only:

```bash
npm ci --strict-allow-scripts
npx playwright install --with-deps chromium
node scripts/export-offer-env.mjs
npm run quality
```

For pushes to `main`, use GitHub Environment `prod` and run:

```bash
npm ci --strict-allow-scripts
npx playwright install --with-deps chromium
node scripts/export-offer-env.mjs
npm run quality
npm run deploy
```

`npm run deploy` must not take an environment name argument. The workflow selects the GitHub Environment and the script consumes whatever variables are available at execution time.

## Cloudflare Pages Deploy Script
Generated projects must include:

```txt
scripts/deploy-cloudflare-pages.mjs
```

The script must use:

```txt
APP_NAME
CLOUDFLARE_ACCOUNT_ID
CLOUDFLARE_API_TOKEN
```

Required behavior:
1. Validate required deploy env variables.
2. Confirm `out/` exists, or build only if the script clearly documents that behavior.
3. Check whether a Cloudflare Pages project named `APP_NAME` exists.
4. If absent, create/provision it.
5. Deploy `out/` to that project.
6. Configure the custom domain `<APP_NAME>.listings.niobium.co.nz`.
7. Create or update the required Cloudflare DNS record automatically.
8. Avoid logging secrets.

No additional Cloudflare Pages deployment configuration is required when creating the project because deployment is handled by GitHub workflow and Wrangler/API.
