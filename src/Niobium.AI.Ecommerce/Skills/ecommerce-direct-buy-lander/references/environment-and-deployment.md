# Environment And Deployment Contract

## Supported Environments
Generated projects must support:
- `dev`
- `test`
- `prod`

`dev` is local-only. Do not create a GitHub workflow for `dev`.

`test` and `prod` must be deployable side by side without conflict, using separate Cloudflare Pages projects and separate GitHub Environments.

## Deterministic APP_NAME
The input must contain top-level `shortProductName`.

Use this deterministic naming pattern:

```txt
# dev and test
niobiumecomm-{shortProductName}-{environment}

# prod
niobiumecomm-{shortProductName}
```

Examples:

```txt
niobiumecomm-hair-remover-dev
niobiumecomm-hair-remover-test
niobiumecomm-hair-remover
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

All other app-facing variables above are safe to expose in the browser bundle because the generated app is frontend-only and the vendor libraries require them.

Do not define currency as an environment variable. Currency comes from quote responses.

## Input-To-Environment Mapping
Use these mappings:

```txt
shortProductName + environment -> APP_NAME
vendorIntegration.tenantId -> TENANT_ID
vendorIntegration.googleRecaptchaSiteKey -> GOOGLE_RECAPTCHA_SITE_KEY
vendorIntegration.storeIntegrationEndpoint -> STORE_INTEGRATION_ENDPOINT
vendorIntegration.notificationIntegrationEndpoint -> NOTIFICATION_INTEGRATION_ENDPOINT
vendorIntegration.stripePublicKey -> STRIPE_PUBLIC_KEY
vendorIntegration.shippingOptionId -> SHIPPING_OPTION_ID
targetCountry -> TARGET_COUNTRY
vendorIntegration.fallbackCoupon -> FALLBACK_COUPON
pricingEconomicsAndOffers.offerOptionsMapping[].optionConfiguration -> OFFER_OPTION__{offerOptionKey}
trackingSpec.metaPixelId -> META_PIXEL_ID
trackingSpec.ga4Id -> GOOGLE_TAG
trackingSpec.microsoftClarity -> CLARITY_ID
trustSignal.facebookPage -> FACEBOOK_URL
trustSignal.instagramPage -> INSTAGRAM_URL
trustSignal.contactEmail -> CONTACT_EMAIL
```

Legacy fallback: `trustSignal.InstrgramPage` may be used only if `trustSignal.instagramPage` is absent.

## Offer Option Environment Generation
The source of truth is `pricingEconomicsAndOffers.offerOptionsMapping`.

For every mapping:
- read `offerOptionKey`
- serialize `optionConfiguration` as compact JSON
- set `OFFER_OPTION__{offerOptionKey}` to that compact JSON

Example:

```json
{
  "offerOptionKey": "2",
  "optionConfiguration": [
    { "Listing": 1, "Option": "Default", "Quantity": 2 },
    { "Listing": 2, "Option": "Default", "Quantity": 4 }
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

Recommended local behavior:
- `scripts/export-offer-env.mjs` writes offer option values to a local generated env file or logs shell export commands.
- `scripts/generate-public-env.mjs` resolves `APP_NAME` as `niobiumecomm-{shortProductName}-dev` if `APP_NAME` is absent.
- `.env.example` documents required local values and notes that Cloudflare secrets are needed only for deploy.

## npm Scripts
Generated projects must provide:

```txt
npm run lint
npm run build
npm run deploy
```

Expected behavior:
- `npm run lint`: runs ESLint with zero-warning enforcement.
- `npm run build`: generates public env safely and runs `next build` to produce `out/`.
- `npm run deploy`: deploys `out/` to Cloudflare Pages using active environment variables.

Do not add `npm test` yet.

## Required Workflows
Create two workflows:

```txt
.github/workflows/test.yml
.github/workflows/prod.yml
```

Both workflows should install dependencies and run `node scripts/export-offer-env.mjs` before the required npm commands.

### Test Workflow
Trigger only on pull requests from `feature/*` branches.

Use GitHub Environment `test`.

All `feature/*` branches share one test Cloudflare Pages project and one test deployment environment.

Required commands:

```bash
npm run lint
npm run build
npm run deploy
```

### Prod Workflow
Trigger on:
- pull requests targeting `main`
- pushes to `main`

For pull requests targeting `main`, run validation only:

```bash
npm run lint
npm run build
```

For pushes to `main`, use GitHub Environment `prod` and run:

```bash
npm run lint
npm run build
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
