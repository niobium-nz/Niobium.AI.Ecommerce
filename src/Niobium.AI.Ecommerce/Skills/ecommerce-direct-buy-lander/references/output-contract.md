# Output Contract

## Required Deliverable
Return a complete, buildable Next.js App Router project that exports to static files with `next build` and produces deployable output in `out/`.

The project must contain the landing page and the four policy pages, but no cart and no checkout page.

## Required Routes
- `/`
- `/privacy-policy`
- `/terms`
- `/returns-policy`
- `/shipping-policy`

Do not create:
- `/cart`
- `/checkout`
- API routes
- server actions
- rewrites or redirect logic that requires a server

## Recommended Project Tree

```text
project-root/
  app/
    globals.css
    layout.tsx
    page.tsx
    privacy-policy/page.tsx
    terms/page.tsx
    returns-policy/page.tsx
    shipping-policy/page.tsx
    not-found.tsx
  components/
    layout/
      site-header.tsx
      site-footer.tsx
      sticky-buy-bar.tsx
    sections/
      hero.tsx
      social-proof.tsx
      offer-stack.tsx
      surface-proof.tsx
      how-it-works.tsx
      rinse-clean.tsx
      use-cases.tsx
      testimonials.tsx
      faq.tsx
      final-cta.tsx
    ui/
      ...customized shadcn primitives
  lib/
    site-data.ts
    offers.ts
    checkout.ts
    query-params.ts
    tracking.ts
    utils.ts
  public/
    assets/
      ...copied or organized media assets
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
- shared header and footer
- no blocking, render-heavy logic

### `app/page.tsx`
Assemble the landing page sections in the chosen order.

### Policy Pages
Each policy page should:
- reuse the shared header and footer
- preserve allowed query params in footer links
- render the provided body content with simple, readable styling
- avoid heavy conversion logic

### `components/sections/*`
Keep sections focused and reusable. Each section should do one job:
- increase desire
- reduce risk
- remove friction

### `lib/site-data.ts`
Map the raw input into a presentation-friendly structure for the page.

### `lib/offers.ts`
Store the offer map, selected-offer helpers, and short-name logic if not generated ahead of time.

### `lib/checkout.ts`
Build the final checkout URL by replacing `:offer-short-name` and merging whitelisted query params.

### `lib/query-params.ts`
Read and preserve only the allowed tracking params.

### `lib/tracking.ts`
Expose tiny helpers for the required analytics events.

## Build Expectations
- `next.config.mjs` must set `output: 'export'`.
- Do not rely on server-only features.
- Do not rely on the default Next.js image optimizer in static export mode.
- Keep the landing page static and client-light.
- The final bundle should be deployable to any static host.

## CTA Rules
- Every purchase CTA says `Buy Now` or a very close variant that still clearly means immediate purchase.
- The CTA must lead to checkout, not to cart.
- The selected offer should be reflected in the outgoing checkout URL.

## Footer Requirements
The footer should include the trust-policy links and contact reassurance.

At minimum include links to:
- privacy policy
- terms
- returns policy
- shipping policy

## README Requirements
The output project should include a short `README.md` with:
- install command
- dev command
- build command
- export output location
- where to place assets if they are external to the generated repo
- any unresolved TODOs caused by missing input

## Acceptance Checklist
Before finalizing, confirm:
- static export is configured
- no cart language remains in primary flows
- policy pages exist
- footer links to those pages
- CTA uses offer-aware checkout URL
- query params are preserved correctly
- analytics IDs are wired behind guards
- major above-the-fold media has explicit dimensions
- hero media is not lazy-loaded
- below-the-fold media is lazy-loaded
