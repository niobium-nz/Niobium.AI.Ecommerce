---
name: ecommerce-draft-prompts
description: >-
  Turn a finalized ecommerce market strategy plus a product-reference-board HTTP(S) URL or
  local file path into still-image concepts and self-contained image-generation prompts. Use
  for Ecommerce Draft Prompts (formerly Ecommerce Image Strategist) or
  MarketStrategyOutput-to-ImageStrategyOutput tasks with product_details,
  pricing_economics_and_offers, mobile_first_landing_page_plan, customer_segments and
  product_reference_board. Inspect the supplied board, preserve one-SKU fidelity, audience
  psychology and source asset identifiers, and return lower snake case JSON. Do not generate
  images, execute campaigns, perform market research, produce video prompts or invoke paid
  discovery. No scripts or original source-language models are required.
---

# Ecommerce Draft Prompts

Read the complete strategy, inspect its single-SKU reference board, select honest
still-image opportunities, then write prioritized production prompts. Return JSON
only during execution; do not generate images, layouts, code or final ad files.

## Read the contracts

Read [input schema](schemas/input.schema.json), [output schema](schemas/output.schema.json)
and [contract guide](references/contract.md). Use the finalized upstream strategy
contract, including product-level name recommendations and nullable bundle slots.
Add only the required root string `product_reference_board`; do not wrap the
strategy inside a new `market_strategy` object.

Use [reference-board handling](references/reference-board.md) before inspecting
any location, and [creative workflow](references/creative-workflow.md) before
selecting concepts. Finish with [quality checks](references/quality-checks.md).
Consult [examples](examples/README.md) for output shapes and synthetic cases.

## 1. Validate and resolve the reference

1. Accept one JSON object with the four upstream sections and
   `product_reference_board`. Reject malformed JSON, missing fields, invalid
   field types, unknown fields, upstream error-only objects and duplicate JSON
   keys. Do not silently repair a strategy or infer its missing business inputs.
2. Read all strategy sections, not only the asset list. Distinguish supplied
   product facts from competitor references, modeled economics, unapproved
   offers, creative hypotheses and proof assets that have yet to be made.
3. Open the supplied HTTP(S) image URL or authorized local image file through
   available image-reading tools. Use its pixels, not a filename, URL slug,
   surrounding webpage, cached recollection or search snippet, as visual evidence.
4. Confirm that the board shows multiple views of one and the same complete
   product/SKU, sufficiently clearly to preserve its appearance. Do not treat the
   views as several units, a bundle, accessories, components or a product lineup.
5. If the board is unreachable, unreadable, insufficiently clear, incompatible
   with the stated product, or ambiguous between several SKUs, return
   `needs_clarification` with no prompts and an actionable `error`. Never pretend
   to have inspected a board. No image input is needed to author this skill, but
   a usable board is required when the skill executes.

## 2. Establish product truth and commercial context

Apply this priority: product facts and limitations; audience and trigger; funnel
role and proof requirements; offer logic; brand cues; placement constraints.
Treat the board as authority for appearance, not proof of performance, material
composition, safety, ownership, endorsements or real-world results. Reconcile
strategy and board evidence conservatively; stop for material contradictions.

Identify the channel, funnel role, audience, current customer state, emotional
driver, trigger moment, main objection, desired feeling, belief shift and proof
requirement for each candidate. State concise usable context in the final
`prompt`, not hidden chain-of-thought or separate uncontracted fields.

Do not turn modeled pricing, internal refund assumptions, forecast CPA, suggested
guarantees or requested proof assets into established customer-facing facts.
Do not put operational short codes, asset IDs or reference locations in rendered
copy. Do not invent physical branding to match a suggested product name.

## 3. Select and prioritize still-image concepts

- Extract strategically sound explicit stills and clearly supported implicit
  visual opportunities from landing-page sections, segment adaptations and
  angle-level handoffs. Do not impose an arbitrary concept quota or manufacture
  weak concepts to fill an array.
- Ignore purely video-only deliverables. Reuse their non-motion context only to
  improve an independently valid still; do not turn every video into a still.
- Prefer one primary persuasion job per image: hook, problem recognition,
  mechanism explanation, honest comparison, objection handling, reassurance,
  offer rationale, aspiration or practical lifestyle integration.
- Prioritize the strategy's highest-priority segments. Use explicit segment
  priority ratings where provided; use source order to break equal priorities.
  Keep different segment/angle logic distinct. Do not combine conflicting buyers
  into a generic audience. Array order expresses priority; there is no `priority`
  output field.
- Refine a supported execution, not the product truth. Omit incidental unsupported
  decoration; request clarification when the central scene or use case depends
  on missing substantiation. Do not pass unsafe or deceptive concepts downstream
  with a disclaimer appended to them.
- If there are no valid still-image opportunities and no material unresolved
  ambiguity, return `{"status":"ok","image_prompts":[]}`. A required board must
  still be present and usable. Ambiguity is not an empty success.

## 4. Allocate IDs and aspect ratios

Preserve a source `asset_id` exactly for its primary matching still-image concept.
For additional distinct concepts from that asset, use `-alt-01`, `-alt-02`, etc.
For concepts with no source ID, use readable IDs such as
`img-seg-1-angle-2-01`; use `img-shared-01` for shared concepts. Reserve all source
IDs first, then choose the first unused suffix. Reject materially conflicting
source definitions that share an ID; reuse one identical shared definition
without duplicating it. Keep a private source-to-concept map. Do not hash or
require UUID generation. IDs are unique within a response, not globally unique.

Honor explicit positive numerical aspect ratios or dimensions first. Otherwise
use square = 1:1, portrait = 4:5, landscape = 16:9. Use square when neither the
strategy nor placement determines orientation. Do not infer portrait merely
from the fact that a page is mobile-first. Derive orientation from the ratio;
request clarification for contradictory same-asset placement constraints instead
of silently distorting the frame. Keep alternate ratios in separate concepts only
when the strategy actually requests such variants.

## 5. Write each prompt

Create exactly four fields per item: `asset_id`, `orientation`,
`overlay_copy_suggestion`, `prompt`. Keep the strategic context inside `prompt`.
Start the prompt with the commercial objective and the buyer feeling to evoke,
then cover audience, current state, trigger, objection and desired belief shift
before describing cameras or lighting. Include the proof needed and its limits.

Describe the decisive scene, first visual focal point, subject interaction,
product placement, visual logic, realism, composition, lighting, mood and copy-safe
space. Give the downstream model enough context to understand why the image
should persuade. Do not replace commercial intent with generic aesthetics.

Explicitly reference "the supplied clean multi-view product reference board" as
showing one single product/SKU only. Require exact shape, proportions, color,
materials as evidenced, visible features and supported branding. Do not redesign,
simplify, recolor, add or remove features, invent accessories or fake packaging.
For supported multi-unit offers, show the specified number of identical complete
instances of that one SKU, not one instance per reference-board view.

Use generic, non-identifiable people unless explicit permitted identity
requirements exist. Default to text-light scenes. Use `""` when no overlay is
suggested. If suggesting later overlay copy, keep it honest and reserve blank
space; explicitly say not to render it in the base image. If the strategy requires
text inside the image, quote only the exact authorized words in `prompt` and
match `overlay_copy_suggestion`. Never render field labels or internal IDs.

Include a concise honesty clause prohibiting unsupported claims, invented
features, wrong materials, fake packaging, invalid use cases, fake reviews or
authority cues, deceptive scarcity and dishonest transformations. Do not portray
an AI-made scene as recorded customer or laboratory evidence.

End every prompt exactly with `Aspect ratio: W:H.` using positive integers and
nothing after the period. Prompts must be self-contained in all scene and
strategy details; the board remains a separately required visual attachment for
the downstream image-producing step. Never embed a local file path, private URL
or access token into the prompt or overlay.

## 6. Return and verify

- On success, return exactly `status: "ok"` and `image_prompts`.
- For material ambiguity or an unusable supplied board, return
  `status: "needs_clarification"`, `image_prompts: []`, and a nonempty `error`
  describing the blocker and specific correction needed. Use the established
  error channel; do not add `clarifications_needed` or fake prompt items.
- For malformed or structurally invalid input or unavailable image-reading
  capability, return only `{"error":"..."}`. Do not invent a successful reference
  inspection. Do not output partially guessed prompts alongside an error.

Check the entire response against the schemas and semantic quality checklist.
No fences, prose, creative-strategy summary, `image_concepts`, image URLs, generated
files or hidden analysis belong in the runtime response. Keep `ImageProducer`
results and all actual generation outside this skill. Never call image generation
as a way to read or validate the board. Do not browse for substitute products or
use unrelated saved reference images.
