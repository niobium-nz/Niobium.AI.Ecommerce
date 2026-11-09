# Final checks

Perform these checks without requiring scripts. A host may independently validate
JSON, but structural validation is not a substitute for visual or semantic review.

## Contract

- Use the finalized market-strategy success contract plus the one root board string.
- Preserve all four upstream sections and approved naming/bundle behavior.
- On success emit `status` and `image_prompts` only; use the compact four-field item.
- On material ambiguity emit `needs_clarification`, an empty prompt list and `error`.
- On invalid input or missing image-reading capability emit `error` only.
- Do not include markdown fences, extra summaries, files, generated-image URIs or
  a `clarifications_needed` field. Do not combine success with an error.

## Reference and honesty

- Confirm that actual pixels were inspected through an authorized tool.
- Confirm one SKU across multiple views, not a bundle or lineup.
- Distinguish appearance from verified product performance and material facts.
- Stop on material contradictions, unreadable boards or unknown critical use cases.
- Keep input locators and internal analytics out of prompts and overlays.
- Ensure the producer is instructed to receive the same board separately.
- Do not treat a generated scene as proof that a customer, expert or test existed.

## Concept and prompt

- Give every still a specific audience, trigger and one primary persuasion job.
- Put the objective and feeling first; state objection, proof need and belief shift.
- Provide scene, interaction, composition, realism, lighting and copy-safe space.
- State concise exact product fidelity and explicit honest constraints.
- Preserve existing source IDs and allocate collision-free readable derived IDs.
- Keep array priority consistent with the strategy; do not blend conflicting segments.
- Ignore purely video deliverables and avoid unsupported filler concepts.
- Reconcile text: no overlay, suggested post-production overlay, or exact approved
  in-image wording. Do not accidentally render a suggestion or field label.
- Match orientation to width:height; end exactly `Aspect ratio: W:H.`.
- Allow non-default explicit ratios; default only when no explicit ratio exists.
- Distinguish a genuine empty-opportunity result from uncertainty that blocks work.

## Runtime limits

Do not generate images, final ad files, layouts, code or campaigns. Do not call paid
Ad Library tools, browse for product substitutes, or use unrelated saved images.
No code runtime is required. An authorized image reader and an accessible supplied
board are required. Never promise that local paths are accessible across hosts.
