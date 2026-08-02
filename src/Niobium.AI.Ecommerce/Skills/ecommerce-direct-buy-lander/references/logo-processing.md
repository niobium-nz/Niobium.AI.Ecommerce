# SVG Logo To Transparent PNG Contract

## Purpose
When `brand_system.logo_file` is SVG, treat the SVG as a source asset only. Prepare brand-colored, correctly sized PNG assets with a transparent background, then use those PNG files in the generated website.

The final website must not embed, inline, mask, or directly display the raw logo SVG.

## Source SVG Contract
The supplied logo SVG is assumed to use:
- black foreground marks: `#000`, `#000000`, or `black`
- white background: `#fff`, `#ffffff`, or `white`
- `none` or `transparent` only where transparency is already intended

The SVG may omit `fill` on foreground paths because SVG's default fill is black.

Do not guess when the SVG contains another visible color, a gradient with other colors, an external image, an external stylesheet, or an external resource. The logo preparation script must fail with a clear error that identifies the unsupported source feature.

The SVG source must be a locally available file. Treat a missing path or remote-only URL as a blocking error; do not ship a text fallback or raw SVG as a substitute for the required PNG conversion.

## Required Transformation
For each required website logo variant:
1. Load and validate the SVG without resolving external entities, scripts, remote stylesheets, or remote images.
2. Preserve the source `viewBox` and aspect ratio.
3. Rasterize at an appropriate high-density size, normally 2x the largest intended CSS dimensions.
4. Convert source white to alpha transparency.
5. Convert source black to the selected theme color for that variant.
6. Convert anti-aliased edge greys into partial alpha using luminance, rather than leaving grey or white halos.
7. Export an RGBA PNG with transparency preserved. Never flatten the output onto white or another opaque background.
8. Optimize the PNG without removing the alpha channel.

A robust pixel transform is:

```txt
sourceCoverage = sourceAlpha * (1 - sourceLuminance)
outputRGB = selectedThemeColor
outputAlpha = sourceCoverage
```

With normalized values, pure white produces alpha `0`, pure black produces the source alpha, and edge antialiasing becomes partial transparency.

## Theme Variants
Generate only the variants the site needs, but normally provide:
- `public/assets/logo-primary.png`: black foreground mapped to `brand_system.primary_color`, for light surfaces
- `public/assets/logo-inverse.png`: black foreground mapped to the selected high-contrast theme color for dark surfaces, usually `brand_system.secondary_color` or an explicitly derived light neutral

Both files must have transparent backgrounds.

Do not map the white source background to the inverse logo color. White source pixels always become transparent.

## Sizing
Use the actual SVG aspect ratio when deriving output dimensions.

Recommended CSS display sizes:
- mobile header: `28-34px` high
- desktop header: `32-40px` high
- footer: up to about `200px` wide when the composition supports it

Recommended raster size:
- export at least 2x the largest CSS display size
- avoid enormous output dimensions merely because the SVG contains a large coordinate system
- cap generated dimensions to what the website actually displays
- provide explicit rendered width and height in the logo component to prevent layout shift

## Implementation Guidance
Use the retained `templates/scripts/prepare-logo-assets.mjs` as the starting implementation for generated `scripts/prepare-logo-assets.mjs`, with the latest stable `sharp` package. Adapt paths/config only; do not weaken source validation, alpha conversion, output verification, or atomic writes.

The script should:
- expose pure functions for source validation, dimension calculation, pixel transformation, and output verification so they can be unit-tested
- avoid `flatten()` or any operation that adds an opaque background
- write PNG files atomically
- include the source logo hash and selected color in its cache key, or regenerate on every build
- remove stale derived variants before writing new ones
- never copy the original logo SVG into a shopper-facing path unless it is retained only as a non-rendered source artifact

## Required Verification
After export, verify each PNG:
- format is PNG
- alpha channel is present
- at least one transparent pixel exists when the SVG had a white background
- no opaque white background rectangle remains
- foreground pixels match the requested theme color within rasterization tolerance
- width and height are positive and preserve the SVG aspect ratio within rounding tolerance
- file dimensions are appropriate for the website rather than the SVG's arbitrary vector coordinate size

The generated project's tests must include fixtures that prove:
- black foreground becomes the requested theme color
- white background becomes transparent
- anti-aliased edges do not retain a white halo
- unsupported source colors fail clearly
- output dimensions preserve aspect ratio
- `site-logo.tsx` references generated PNG assets and not the raw SVG
