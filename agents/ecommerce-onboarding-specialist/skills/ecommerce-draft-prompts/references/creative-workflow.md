# Still-image planning workflow

## Contents

- Read the whole strategy
- Candidate discovery
- Priority and traceability
- Ratios and text
- Prompt composition
- Safety and integrity

## Read the whole strategy

Retain the finalized product name, limitations, use cases, economic assumptions,
offer ladder, refund/shipping framing, landing-page sequence and every segment's
angle matrix. Separate commercially useful context from publishable proof.
"Proof asset required" means the proof has not yet been supplied. A synthetic
scene may illustrate a verified use, but cannot establish a real test result.

Use validated product facts first, audience psychology second, funnel/channel
and proof requirements third, offer logic fourth, brand styling fifth, and
placement constraints last. A lower-priority request cannot authorize an untrue
physical feature or deceptive claim. A proposed brand palette may guide scenery,
not recolor the actual product. A new sellable name does not authorize changing
visible product branding or inventing a logo.

## Candidate discovery

Review explicit landing-page `asset_needs`, static image directions, visual proof
moments, segment hero directions and asset needs specific to the segment. Then
look for supported implicit still opportunities that have a distinct commercial
job. Treat textual mentions of an existing asset ID as references, not new assets.

Still indicators include image, static, photo, close-up, macro, banner, card,
comparison, diagram, icon row, proof grid and product-only still. Interpret these
in context: a proof-grid instruction is not proof that reviews or tests exist.
Video-only signals include reel, animation, loop, motion and continuous footage.
Ignore a purely video deliverable; borrow only non-motion context that improves
an already valid still. Do not generate video storyboards or animation prompts.

Identify an explicit buyer and trigger for each candidate. Preserve current
customer state, emotional driver, main objection, proof needed, desired feeling
and belief shift. Choose one primary persuasion job. Generic lifestyle beauty
without a supported commercial role is not a sufficient concept.

## Priority and traceability

Use explicit segment priority ratings, then source order for ties. Within a
segment retain source angle/asset order unless the strategy states a priority.
Place shared landing-page concepts with their matching highest-priority audience;
do not invent a universal buyer when the segments conflict. List concise audience,
channel and funnel context in each prompt so it stands alone.

Reserve all upstream asset IDs before creating any derived IDs. Keep an existing
ID for its primary matching still. Allocate alternative IDs as `<source>-alt-01`,
then increment until unused. Use `img-seg-<segment>-angle-<angle>-01` for new
segment/angle concepts, or `img-shared-01` for genuinely shared concepts. Do not
change an existing ID to fit a preferred style. Keep exact repeated references
pointing to the same concept; do not overwrite conflicting definitions sharing
an ID. Material ambiguity in identity or content requires clarification.

Deduplicate only genuinely equivalent scenes with the same audience, persuasion
job, framing and aspect ratio. A meaningful alternate concept can keep its own
suffix. Do not merge two separately required upstream assets merely because their
wording is similar; preserve the source obligations or clarify their relationship.
Readable IDs provide plan-local traceability, not global or cross-run guarantees.

## Ratios and text

An explicit numerical ratio/dimension overrides the fallback mapping. A 1080 by
1920 asset is portrait 9:16, not default 4:5. A 1200 by 628 asset may use 300:157
or the equivalent unreduced positive integer pair. If only orientation is known,
use 1:1, 4:5 or 16:9 for square, portrait or landscape. If nothing identifies
orientation, use square. Mobile-first by itself does not specify orientation.
Conflicting explicit instructions for the same concept are not silently resolved.

Use `overlay_copy_suggestion: ""` for no proposed text. For a text-free base image
with optional later copy, suggest concise honest wording in that field, reserve
space, and say in the prompt that no overlay is to be rendered. Only explicit
in-image copy belongs in quotation marks in the rendering instructions. Never
include asset labels, prompt field names, hidden performance metrics or source
file paths as visible copy. Do not fabricate discount terms, expiry dates, review
counts, ratings, guarantees or immediate delivery promises.

## Prompt composition

Write natural, self-contained prose in this order:

1. Commercial objective and desired buyer feeling.
2. Audience, present state, trigger, objection, proof need and intended belief shift.
3. Decisive scene, first focal point and persuasive visual logic.
4. Product placement and supported subject interaction; explain any offer quantity.
5. Single-SKU multi-view reference authority and concise exact-fidelity instruction.
6. Composition, viewpoint, lighting, realism, mood and copy-safe space.
7. Honest constraints and limits on any text.
8. Final sentence: `Aspect ratio: W:H.`

Do not give chain-of-thought, scoring calculations or a long strategic essay.
Do give the producer enough concrete direction to understand the intended image.
Avoid references such as "same as above" or "see segment 2" that make a prompt
incomplete when separated from the rest of the output. A separate visual board
is still required; self-contained scene text cannot replace it.

## Safety and integrity

Do not invent physical features, materials, accessories, packaging, competitor
products, logos, copyrighted characters, endorsements, certifications or fake
social proof. Do not depict unsupported medical, financial, legal or safety
outcomes, impossible transformations, dishonest before/after contrasts or
manipulative shame/fear. Use generic people without implying genuine testimonials.
Only depict supported multi-unit bundles as identical complete instances of the
single referenced SKU. Do not mistake repeated board views for included units.

If the core concept would require unsafe or unvalidated use, material product
contradictions, missing crucial brand/reference assets, or unsupported text,
return `needs_clarification` with no prompts. Minor missing aesthetics can use the
approved defaults; missing product truth cannot. Do not add unsafe prompts and
rely on a negative clause to undo the visual deception.
