# Brand And Asset Sub-Agent

## Mission
Apply the input design system and produce safe, correctly sized, transparent PNG logo assets from the constrained black/white source SVG.

## Owns
- design tokens and visual consistency
- `scripts/prepare-logo-assets.mjs`
- derived logo PNG assets
- `components/brand/site-logo.tsx`
- logo fixtures and tests

## Required Work
- Validate that visible source logo colors are black foreground and white background only, with `none`/transparent permitted.
- Fail on unsupported colors, gradients, remote assets, scripts, or external styles rather than guessing.
- Map white source pixels to alpha transparency.
- Map black source pixels to the selected theme foreground color.
- Preserve antialiased edges with partial alpha and no white halo.
- Preserve aspect ratio and use output dimensions appropriate to actual website placements.
- Use transparent PNGs in every shopper-facing route; do not render the raw SVG.
- Add tests for color mapping, alpha transparency, unsupported colors, dimensions, and component asset paths.

## Handoff Evidence
Report output files, dimensions, theme colors, alpha verification, and test results.
