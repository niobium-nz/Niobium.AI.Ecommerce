# Tracking, Query Params, Static Export, And Performance

## Tracking Integration
The landing page must support:
- GA4
- Meta Pixel
- Microsoft Clarity

### Script Placement
Use a shared layout-level integration so the scripts load on the landing page and policy pages.

For Next.js, prefer `next/script` for third-party tracking. Analytics scripts are good candidates for the `afterInteractive` strategy.

### Event Wiring
Centralize event helpers in `lib/tracking.ts`.

At minimum support these landing-page events when they are present in the input:
- `ViewContent`
- `CTA_Click`
- `Bundle_Select`
- `Video_Play`
- `InitiateCheckout`

The landing page should not emit `AddToCart` or `Purchase` unless the user explicitly asks for a different flow.

### Guarding
If a platform ID is missing, skip that integration cleanly. Never break the build over missing analytics.

## Query Param Preservation
Preserve only the query params listed in the input, for example:
- `utm_source`
- `utm_campaign`
- `utm_content`
- `fbclid`

Preserve them across:
- CTA links to checkout
- footer links to policy pages
- internal in-page stateful links if present

Do not preserve arbitrary or unknown query params.

## Static Export Constraints
The project must use Next.js static export.

Required:
- `output: 'export'`
- routes that can be fully rendered at build time
- no server actions
- no API routes that depend on the incoming request
- no rewrites or redirects that require a server
- no cookie-dependent rendering

The deployable build output is `out/`.

### Images In Static Export
Do not rely on the default Next.js image optimizer in a static export.

Prefer:
- standard `img` elements with explicit width and height
- or another static-export-compatible image approach

## Core Web Vitals Targets
Use these as the non-negotiable quality targets for mobile performance:
- LCP: `<= 2.5s`
- INP: `<= 200ms`
- CLS: `<= 0.1`

Treat these targets as 75th-percentile goals, not just lab-only ideals.

## Project-Level Performance Budgets
These are the skill's internal guardrails for PageSpeed-oriented output:
- initial route JavaScript should stay lean and avoid unnecessary client hydration
- do not lazy-load the main hero image or hero poster
- only one above-the-fold media asset should be eagerly prioritized
- all media must reserve space with explicit dimensions or aspect-ratio boxes
- below-the-fold media must lazy-load
- no autoplay hero video with sound
- no heavy carousel library for a simple media gallery
- avoid adding custom fonts when the input says system fonts only
- keep third-party scripts limited to the required analytics stack

## Tactics That Support PageSpeed Success
- keep most components server-rendered
- move interactive state only into small client islands
- compress images aggressively and use modern formats when available
- use poster images for videos
- reserve media dimensions to avoid layout shift
- lazy-load offscreen images and iframes
- keep sticky bars transform-based so they do not cause layout jumps
- avoid render-blocking analytics and non-essential scripts
- use SVG or CSS icons instead of large icon packs when possible

## Mobile Performance Checklist
Before finalizing, confirm:
- the first viewport renders without waiting on analytics
- the hero media is dimensioned and not lazy-loaded
- the sticky buy bar does not shift layout when shown
- accordions animate cheaply and do not relayout the whole page unnecessarily
- policy pages reuse the same shell without loading landing-page-only interactive code
