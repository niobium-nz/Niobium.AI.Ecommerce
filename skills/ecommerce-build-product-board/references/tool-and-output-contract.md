# Tool and output contract

## Runtime capabilities

Require image viewing, actual-reference image editing/compositing, a transparent
PNG output surface, and a way to inspect the candidate. No scripts, Python, .NET,
external account analytics, paid discovery calls, or web image searches are
required. The package does not itself provide an image editor or guarantee that
these capabilities exist in any particular host.

Use the host's authorized native image tools. Preserve their exact reference-input
semantics and restrictions. Do not invent image IDs, reuse unrelated saved
references, paste URL text where a binary image is required, or pretend every
view was supplied when the tool silently drops excess references. If an input
limit prevents retaining necessary visual evidence, use a supported composition
workflow that preserves actual source pixels or ask to split source preparation;
do not summarize hidden views into prose and treat that as an equivalent source.

Prefer an editor's source-preserving cutout, mask and compositing workflow. Use
reference-aware generative editing only for local permitted cleanup/debranding
when reliable preservation is possible. Do not run full text-only regeneration.
Where the host mandates a particular image tool or image-only final response,
follow those requirements. A known inability to review a result is a capability
limitation, not permission to claim successful quality assurance.

Inspect images only from the user-selected sources. HTTP(S) retrieval is source
loading, not market research. It does not authorize exploring a website, changing
accounts, spending paid discovery credits, or searching for substitute products.
Keep any browser use limited to authorized source access and respect the host's
session cleanup rules. Never sign in or grant new access merely to bypass a
blocked image URL; request an accessible upload instead.

## Production brief for an editor

Use the actual mapped source images, not this template alone. Adapt the brief to
the verified views; never include unsupported view placeholders:

> Create one transparent PNG reference board from the attached real photographs
> of the same single SKU. Preserve the observed geometry, proportions, material
> appearance, actual colors, functional markings, handedness and configuration.
> Use only these evidenced views: [list mapped views and sources]. Remove all
> logos and brand identity, including marks physically on the product, with local
> edits that preserve the supporting part and functional details. Preserve
> original photo regions wherever possible. Remove photographic overlays,
> backgrounds, people, hands, props and retail packaging without inventing hidden
> surfaces. Do not generate missing sides, mirror a side, create a hybrid variant,
> add accessories, redesign the body, or recolor it. Arrange complete views at
> coherent scale with clear transparent spacing. No added labels, arrows,
> measurements, frames, background, floor, or marketing design. Keep actual
> product transparency and small construction details. Return one PNG image.

The permitted local fill is only the cosmetic exception defined in
[evidence and debranding](evidence-and-debranding.md). Resolve structural gaps
before production. A natural-language brief is not a standalone output artifact.

## Success

Deliver exactly one image file, media type `image/png`, with genuine alpha
transparency and multiple distinct source-supported product views. Return it
through the native image/file surface. Do not emit a JSON success wrapper,
text-only prompt, ZIP of separate view images, or invented file URL. Choose
layout and image dimensions from source coverage and detail needs; there is no
fixed view count, canvas ratio, or fictitious resolution requirement.

Keep QA observations and source mappings out of the pixels. Do not add visual
branding, metadata IDs, source paths, personal data or explanatory text. If the
user explicitly permits an incomplete board, retain that limitation in the task
context and never describe the board as complete. Do not add a sidecar file unless
requested; success still consists of one image.

## Clarification

Ask a focused plain-text question before image generation when missing evidence,
SKU/configuration conflict, uncertain component identity, or inseparable brand
geometry prevents a faithful board. Name the particular view or issue. Examples:

- "Please provide a clear rear view showing the hinge and underside; those areas
  are hidden in the supplied photographs."
- "The second photo shows an extra side port. Is it the same SKU as the others?"
- "The logo forms the raised housing outline. Removing it would change product
  geometry. Please provide an unbranded reference or clarify the intended edit."

Do not ask already-answered questions or repeat the branding preference: physical
logos and brand identity must be removed in every successful result.

## Errors

For invalid input, inaccessible sources, missing editing/viewing capability or a
failed production process, return only an object conforming to
[the error schema](../schemas/error.schema.json), for example:

```json
{"error":"The rear image could not be opened. Upload an accessible copy or explicitly exclude it; no board was produced."}
```

Do not make image-generation calls when there are no usable image targets. Do not
report a filename as created unless the host actually returned or created it.
