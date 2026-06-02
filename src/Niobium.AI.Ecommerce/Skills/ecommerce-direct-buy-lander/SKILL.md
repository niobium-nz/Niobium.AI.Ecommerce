---
name: ecommerce-direct-buy-lander
description: build high-converting, mobile-first ecommerce product landing pages for cold meta ad traffic using next.js static export, tailwind css, and customized shadcn/ui. use when chatgpt must turn a structured product brief or input json into a direct-buy landing page code bundle, policy pages, analytics wiring, offer-selection logic, and performance-safe static assets. optimize for profit-first bundle presentation, truthful claims only, buy now checkout handoff, preserved ad query params, and baymard-informed product-page ux without carts, fake urgency, stock shadcn styling, or generic template aesthetics.
---

# Ecommerce Direct Buy Lander

## Mission
Build a complete, static-export Next.js + Tailwind + shadcn project for a single-product, direct-buy landing page and matching policy pages from structured input.

## Operating Principles
- Build the page, not a strategy memo.
- Match the incoming paid-ad promise in the first viewport.
- Optimize for direct purchase, not browsing.
- Use only validated claims from the input.
- Keep the design distinctive but purposeful.
- Honor the input font strategy. If it says system fonts only, do not override it.
- Use `Buy Now` as the dominant purchase CTA everywhere.
- Keep the project static-export compatible and performance-safe.
- Customize shadcn primitives. Never ship stock defaults.

## Workflow
1. Read the input JSON. Use `references/example_input.json` only as a shape reference when the live input is missing examples.
2. Identify the page's message-match anchor from `customerSegment.angleAndTrigger`, `segmentLandingPageAdaptation`, and `mobileFirstLandingPagePlan`.
3. Derive the offer map. Use `scripts/derive_offer_map.py <input-json>` when available so every offer gets a stable alphabet-only short name and checkout URL.
4. Pick one clear art direction using `references/design-direction.md` and `references/design-system.md`.
5. Build the project tree from `references/output-contract.md`.
6. Wire analytics, query-param persistence, and performance rules from `references/tracking-and-performance.md`.
7. Generate the policy pages with the shared header and footer, but keep their bodies simple.
8. Must run `scripts/validate_bundle.py <project-dir> <input-json>` and fix any failures before finalizing.
9. Return the complete code bundle plus a short build/export note.

## Input Handling
Treat the input JSON as the source of truth for:
- brand name, colors, logo, font strategy
- product definition, validated claims, limits, and use cases
- offer names, prices, descriptions, and recommended primary offer
- customer segment, ad angle, hero continuity, and objections
- shipping, refund, and support constraints
- analytics IDs, event names, and query params to preserve
- checkout URL template
- legal policy content, testimonials, and asset paths

Do not invent claims, certifications, timelines, savings, or use cases that are not supported by the input.

You may infer layout, hierarchy, and concise conversion copy from validated facts.

If a provided asset path is unavailable, preserve the slot and render a graceful fallback instead of pretending the asset exists.

If the input conflicts with itself, prioritize in this order:
1. explicit user hard constraints
2. truthfulness and legal clarity
3. checkout-routing and analytics requirements
4. direct-response conversion quality
5. visual-system defaults

## Non-Negotiable Build Rules
- Output a static site only:
  - Next.js App Router
  - TypeScript
  - Tailwind CSS
  - customized shadcn/ui primitives
  - `output: 'export'`
- No cart, mini-cart, cart drawer, wishlist, newsletter capture, countdown timer, fake scarcity, or checkout page.
- Use a single dominant purchase CTA: `Buy Now`.
- Permit offer selection, but route every purchase CTA directly to the external or custom checkout URL.
- Preserve only the whitelisted query params from the input when building checkout links and internal policy-page links.
- Replace `:offer-short-name` with an alphabet-only short name of the selected offer.
- Preselect the recommended primary offer unless the input explicitly instructs otherwise.
- Keep policy pages simple and styled like the same site.
- Use SVG or CSS icons when possible. Avoid heavy icon dependencies if they hurt bundle size.
- Keep all major content on the main page. Do not hide key proof in subpages or secondary routes.
- Prefer vertically collapsed sections or accordions over tabs or mobile subpages when content is long.
- Use thumbnails for image galleries, not dot-only controls.

## Design Direction Rules
- Start from ad-message continuity. The landing page should feel like the next frame after the Meta ad click.
- Favor one clear art direction. Use scale contrast, layered surfaces, asymmetry, or texture intentionally. Do not default to a bland centered SaaS layout.
- Import from the reviewed `landing-page-guide-v2` skill only these ideas:
  - choose a deliberate aesthetic direction before coding
  - use anti-generic heuristics
  - strengthen composition, color, and typography guidance
  - customize shadcn components rather than shipping stock styling
- Explicitly reject from that reviewed skill:
  - mandatory 11-element coverage
  - mandatory custom fonts
  - countdowns
  - newsletters or waitlists
  - dual-CTA frameworks that split attention
- If the input says `system fonts only`, use system fonts and create distinction with spacing, hierarchy, weight, composition, icon treatment, and section rhythm.

## UX Rules
- Above the fold must answer:
  - what this is
  - why it matters now
  - what the main offer is
  - what delivery reality looks like
  - why the page is trustworthy
  - what happens when the user taps `Buy Now`
- Surface shipping or delivery timing near the first CTA and again in the FAQ.
- Prefer an estimated delivery date only when the business can state it truthfully. Otherwise use a transparent delivery window.
- Keep copy blocks short for mobile paid traffic.
- Repeat the main CTA roughly every 1 to 1.5 mobile screens without creating competing actions.
- Keep proof close to CTA blocks: demo/result, testimonial or review, support or guarantee, and transparent shipping or returns.
- Keep major details on-page and easy to scan.
- Avoid heavy carousels, sliders, or tab systems that bury content or hurt performance.

## Analytics Rules
- Wire GA4, Meta Pixel, and Microsoft Clarity from the input IDs.
- Fire the input-specified events at appropriate moments. At minimum:
  - `ViewContent` on page view
  - `CTA_Click` when any `Buy Now` control is pressed
  - `Bundle_Select` when the selected offer changes
  - `Video_Play` when a demo video begins playback
  - `InitiateCheckout` immediately before navigation to checkout
- Do not invent cart or purchase-completion events on the landing page.
- Keep tracking code lightweight and isolated in layout and utility files.
- Preserve the whitelisted query params across internal policy-page links and outbound checkout links.

## Performance Rules
- Build toward the Core Web Vitals targets and project budgets in `references/tracking-and-performance.md`.
- Treat the hero media as the likely LCP asset:
  - compress it aggressively
  - size it explicitly
  - preload only the single LCP image or poster when needed
  - never lazy-load the first-viewport hero image
- Lazy-load below-the-fold images, iframes, and non-critical video.
- Prefer Server Components by default and mark Client Components only where interaction is required.
- Keep third-party scripts non-blocking.
- Do not add custom fonts when the input specifies system fonts.
- Avoid heavy animation libraries unless the motion is essential and still stays within performance budget.
- Do not rely on Next.js default image optimization in a static export. Use standard `img` elements with explicit dimensions, or a static-export-compatible image strategy.

## Output Requirements
Follow `references/output-contract.md`.

Unless the user explicitly asks for a different framework, return a complete Next.js App Router project with:
- `app/` routes for the landing page and policy pages
- reusable `components/`
- `lib/` helpers for offers, tracking, query params, and content mapping
- `public/` for user-provided assets
- configuration files needed to install, build, and export
- a short `README.md` with install, build, and export commands plus key assumptions

When code execution is possible, create the files. When it is not, emit the full file tree and file contents.

## Failure Mode
- If crucial content is missing, still output a compilable project with narrowly-scoped `TODO` comments only where the input truly lacks required data.
- If an asset path is provided but the file itself is unavailable, preserve the path contract and show a graceful fallback component.
- If an analytics ID is missing, guard the integration and keep the build working.
- If a requested design choice conflicts with conversion clarity, trust, or truthfulness, choose the safer direct-response implementation and explain the tradeoff briefly.

## References
- `references/input-contract.md` - how to interpret the input JSON and derive page decisions
- `references/output-contract.md` - required project tree and file responsibilities
- `references/design-system.md` - visual and UX rules synthesized from the provided design system and Baymard-style product-page guidance
- `references/design-direction.md` - anti-generic heuristics and visual-direction process adapted from the reviewed `landing-page-guide-v2` skill
- `references/tracking-and-performance.md` - analytics wiring, query-param persistence, static export constraints, and performance budgets
- `references/example_input.json` - example input schema

## Scripts
- `scripts/derive_offer_map.py <input-json>` - derive stable alphabet-only offer short names and checkout URLs
- `scripts/validate_bundle.py <project-dir> <input-json>` - run a fast structural check on the generated project
