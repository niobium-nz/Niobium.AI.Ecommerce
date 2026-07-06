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
    utils.ts
  scripts/
    export-offer-env.mjs
    generate-public-env.mjs
    prepare-logo-assets.mjs
    deploy-cloudflare-pages.mjs
  .github/workflows/
    test.yml
    prod.yml
  public/
    assets/
      ...copied or organized media assets
      logo-primary.png
      logo-inverse.png
  .env.example
  components.json
  next.config.mjs
  package.json
  tsconfig.json
  postcss.config.mjs
  eslint.config.mjs
  README.md
```

The project may use `src/` if the user explicitly prefers it, but the route and file responsibilities must remain equivalent.

## File Responsibilities

### `app/layout.tsx`
- global shell
- analytics script insertion
- metadata
- shared header and footer except where a distraction-reduced checkout shell is intentionally used
- no blocking, render-heavy logic
- no server-only runtime dependency

### `app/page.tsx`
Assemble the landing page sections in the chosen order.

Must include:
- message-matched hero
- visible offer selector using `offerOptionsMapping` order
- quote-driven pricing states for all visible offers
- `Buy Now` CTA linking to `/checkout?offer=<offerOptionKey>`
- `coupon` pass-through only when present in the landing-page URL
- marketing email subscription form near the footer or in the footer

### `app/checkout/page.tsx`
Must implement browser-side checkout.

Requirements:
- read `offer` query param
- validate it against expected offer-option mappings
- show a user-facing error if missing or invalid
- load quote for selected cart and coupon
- allow coupon entry/change with immediate quote refresh
- collect localized shipping fields based on `TARGET_COUNTRY`
- hide country selection
- show optional phone field always
- collect optional order notes
- include marketing subscription checkbox checked by default
- hide billing fields by default behind a same-as-shipping choice
- mount Stripe Payment Element using quote total and quote currency
- call `elements.submit()` when checkout is submitted
- call vendor `makeOrder` only after local and Stripe Element validation pass
- use `orderResponse.instruction` as Stripe `clientSecret`
- call `stripe.confirmPayment` with `return_url` derived from `window.location.origin` and `/order-status`
- show loading, disabled, retry, and error states for quote, coupon, order, and Stripe flows

### `app/order-status/page.tsx`
Must rely only on Stripe `redirect_status` from the URL query string.

Do not call Stripe.js or vendor APIs to verify payment or order details.

Status behavior:
- success: say the order is being processed and email updates will follow; do not claim fulfillment is complete
- failure: explain that payment/order could not be completed and link to contact
- missing/unknown/uncertain: say status cannot be confirmed from this page and link to contact

Fire `PurchaseSuccess` or `PurchaseFailed` only from this route based on `redirect_status`.

### `app/contact/page.tsx`
Render a standalone contact form with:
- name
- email
- message

Use the vendor contact library client-side and show validation, loading, success, and error states.

### `app/track-order/page.tsx`
Render one tracking form with a toggle/radio selection for:
- preferred: email + numeric order number
- alternative: email + first name

Use the vendor track library client-side. First-name matching should be case-insensitive. Display successful tracking responses clearly and do not describe the interaction as message posting.

### Policy Pages
Each policy page should:
- reuse the shared visual language
- preserve allowed query params in footer/internal links
- render the provided body content with simple, readable styling
- avoid heavy conversion logic

### `components/brand/site-logo.tsx`
Centralize all logo rendering here so header, footer, checkout, contact, order, and policy pages use one consistent brand treatment.

Required behavior:
- Read the logo path and brand colors from the generated site data/config derived from `brandSystem`.
- Check whether the logo is SVG using the input contract rules.
- For SVG logos, assume the source is black/white monochrome, recolor it during preprocessing, and export website-ready PNG assets while preserving `viewBox` and aspect ratio in the source transformation step.
- Provide at least normal/light-surface and inverse/dark-surface variants when both are needed, typically as PNG outputs such as `logo-primary.png` and `logo-inverse.png`.
- Use the generated PNG assets in the actual site UI instead of embedding the raw SVG directly in page markup.
- Size the logo responsively with explicit dimensions or CSS constraints to avoid layout shift.
- For non-SVG logos, render the asset through a standard static-export-safe image approach without color replacement.
- If the asset is unavailable, render a styled text fallback using the brand name.

### `components/forms/subscription-form.tsx`
Required near the landing page footer or inside the footer.

Fields:
- email only

Must validate email, load the vendor subscription script client-side, disable during submit, show a spinner/loading state, show success, and show a user-facing retry error on failure.

### `config/offer-options.json`
Generated from `pricingEconomicsAndOffers.offerOptionsMapping`. It should preserve input array order and contain only safe app-facing offer mapping metadata and exact cart values needed for workflow env export.

Do not include Cloudflare secrets.

### `scripts/export-offer-env.mjs`
Reads `config/offer-options.json` and sets `OFFER_OPTION__n` values from each mapping's `optionConfiguration`.

Required behavior:
- in GitHub Actions, append `OFFER_OPTION__n=<compact JSON>` to `$GITHUB_ENV`
- locally, write or update a local env file used by build scripts, without overwriting hand-edited secrets
- preserve mapping order in output logs
- fail on duplicate, missing, or invalid offer option keys

### `scripts/prepare-logo-assets.mjs`
Runs before `next build` when the input logo is SVG.

Required behavior:
- detect whether `brandSystem.logoFile` is SVG using the input-contract rules
- load the source SVG
- apply the selected brand color treatment and appropriate size constraints for website placements
- export optimized PNG logo assets into `public/assets/` for actual site use
- produce at least the variants the site needs for light and dark surfaces
- fail clearly if export is impossible, or emit a documented fallback path that still keeps the final site on explicit image assets
- avoid exposing or embedding deploy secrets

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
Triggered by pull requests from `feature/*` branches only.

Must use GitHub Environment `test` and run:

```bash
npm run lint
npm run build
npm run deploy
```

It may include setup steps before those commands, including dependency install and `node scripts/export-offer-env.mjs`.

### `.github/workflows/prod.yml`
Triggered by:
- pull requests targeting `main`
- pushes to `main`

For PRs to `main`, run validation only:

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

`npm run deploy` must not take an environment argument.

### `lib/offers.ts`
- expose visible offer metadata in input order
- map `offerOptionKey` to `OFFER_OPTION__n`
- parse expected cart JSON from public config/environment
- throw visible runtime errors for missing or invalid expected offer options
- never silently fall back to another offer

### `lib/quote.ts`
- load/call `niobium.store.getQuote`
- accept selected cart and coupon
- return quote response typed from `references/vendor-integrations.md`
- provide helpers for price display and topmost listing ID by highest `lineTotal`

### `lib/order.ts`
- load/call `niobium.store.makeOrder`
- build consignee, shipping, billing, notes, marketingSubscription, culture, and timeZone payloads
- derive billing values from shipping when billing is same as shipping
- return `orderResponse.instruction` for Stripe `clientSecret`

### `lib/checkout-fields.ts`
- define field labels and required/optional rules for supported `TARGET_COUNTRY` values
- implement lightweight validation only
- never render a country selector

### `lib/query-params.ts`
- preserve only input-whitelisted tracking params
- pass through `coupon` only when present
- build root-domain-safe internal URLs; no subfolder support is required

### `lib/tracking.ts`
Expose tiny helpers for required analytics events and guard missing IDs.

Checkout event payloads must include only:
- offer option
- order total
- currency
- country
- topmost listing ID with highest line total

## `package.json` Script Requirements
Must include:

```json
{
  "scripts": {
    "lint": "eslint --max-warnings=0 .",
    "build": "node scripts/generate-public-env.mjs && next build",
    "deploy": "node scripts/deploy-cloudflare-pages.mjs"
  }
}
```

Equivalent commands are allowed if:
- lint has zero-warning enforcement
- build produces static export in `out/`
- deploy uses only active environment variables

Do not add `npm test` yet.

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
- The CTA must lead to `/checkout?offer=<offerOptionKey>`, not to cart.
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
- where to place assets if they are external to the generated repo
- any unresolved TODOs caused by missing input

## Acceptance Checklist
Before finalizing, confirm:
- static export is configured
- no cart route or shopper-facing `Add to Cart` language remains
- all required routes exist
- footer links to required pages and subscription area
- CTA uses `/checkout?offer=<offerOptionKey>`
- `offerOptionsMapping` order is preserved
- `OFFER_OPTION__n` values are generated from `optionConfiguration`
- missing/invalid offer option config throws runtime errors
- quote behavior exists on landing and checkout
- displayed prices are quote-derived
- coupon priority rules are implemented
- Stripe Payment Element deferred-intent flow is implemented
- order creation uses selected cart items and selected coupon
- order status relies only on `redirect_status`
- contact, subscription, and track-order vendor flows include async UI states
- checkout fields follow the country rules document
- analytics IDs are wired behind guards
- checkout analytics event timing follows `references/tracking-and-performance.md`
- SVG logos are detected, recolored from the input palette, converted into appropriately sized PNG assets, and used without stretching when `brandSystem.logoFile` is SVG
- major above-the-fold media has explicit dimensions
- hero media is not lazy-loaded
- below-the-fold media is lazy-loaded
- `npm run lint` passes with zero warnings
- `npm run build` passes
- validator passes
