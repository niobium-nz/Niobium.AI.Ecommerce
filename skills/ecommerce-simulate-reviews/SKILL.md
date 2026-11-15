---
name: ecommerce-simulate-reviews
description: Create a fixed batch of fictional, internal-use customer-voice drafts and paired video-generation prompts from one target country, approved product details, and one selected customer segment. Use for review simulation, internal creative evaluation, and synthetic customer-voice fixtures in an ecommerce planning workflow. Return strict JSON with is_fictional always true, percentage-based length and scripted-rating distributions, and grounded nonempty video prompts. Do not create genuine customer feedback, public testimonials, final videos, or proof of demand. Do not browse, scrape reviews, call advertising tools, or require script execution.
---

# Ecommerce Simulate Reviews

## Contract and boundaries

Read [input.schema.json](schemas/input.schema.json), [output.schema.json](schemas/output.schema.json), and [input-contract.md](references/input-contract.md) before drafting.

Accept one JSON object containing `target_country`, `product_details`, and one `customer_segment`. Return either the complete 30-item JSON array or exactly `{"error":"..."}`. Do not wrap the success array, emit markdown, add commentary, return a partial set, or use an empty array for failure.

Require exactly four fields in every successful item: `is_fictional`, `review_text`, `rating`, and `video_prompt`. Set `is_fictional` to the JSON boolean `true` without exception. Do not expose a real-review mode or a flag override. Keep `review_text` free of the disclosure prefix: carry the disclosure structurally instead.

Keep the work fictional and internal-use-only. Require downstream displays to show a separate fictional/internal-use notice whenever they display `review_text`. Require every video prompt to request the visible notice `FICTIONAL SIMULATION - INTERNAL USE ONLY` throughout a generated clip. Do not treat the boolean as a substitute for visible disclosure in a display that otherwise hides it. See [grounding-and-disclosure.md](references/grounding-and-disclosure.md).

## Workflow

### 1. Validate and identify the selected context

Parse strict JSON; reject duplicate keys, wrong types, unexpected properties, or missing root fields. Check the country against the bundled [country-codes.json](references/country-codes.json), accepting alpha-2 or alpha-3 case-insensitively and resolving internally to alpha-2. Never infer the country from location. Do not add country fields to output items.

Read the entire selected product and segment, including the approved upstream product-naming additions. Resolve a usable product identity, supported use case, audience, and buyer need from the supplied fields. Follow the semantic checks in the input contract; a structurally valid empty object is not sufficient evidence. Do not import other segments, infer unseen product appearance, or fetch reference photos.

If critical facts are absent, material contradictions remain, or the use requested is to pass simulations off as authentic customer feedback, return a specific error without any drafts.

### 2. Separate fact, scenario, and proposal

Internally list approved product capabilities and limitations separately from segment motivations, fictional scene choices, forecasts, and strategy suggestions. Treat proposed promises, proof requirements, commercial hypotheses, and fulfillment/refund assumptions as unverified unless explicitly approved as facts in the supplied context.

Use lower-risk supported content when an uncertain optional claim can be omitted. Do not silently choose a product version when a contradiction changes safe use, product identity, or the core claim. Do not invent purchase records, reviewer identities, delivery events, refunds, long-term results, endorsements, certifications, or performance measurements.

### 3. Allocate the batch by percentages

Use the fixed [distribution-policy.json](references/distribution-policy.json) and [distribution-rules.md](references/distribution-rules.md). Keep the total at 30: the current input has no count override.

Allocate length independently of rating:

| Length | Share of batch | Review-body words |
|---|---:|---:|
| Short | 20% | 10 to 25 |
| Medium | 40% | 26 to 60 |
| Long | 40% | 61 to 100 |

Allocate scripted ratings using exact shares: rating 10 = `66 2/3%` (`2/3`), rating 9 = `23 1/3%` (`7/30`), rating 8 = `10%` (`1/10`). Multiply the total by each exact share; do not multiply rounded decimal percentages. Check that each distribution separately totals 100% and the quotas total 30.

Plan varied motivations, objections, use moments, and perspectives within the one selected segment. Mix ratings across length categories. Do not mechanically equate short with a lower rating, long with a higher rating, or ratings with factual product quality.

### 4. Draft the fictional buyer voices

Write in English only, with country-appropriate English spelling when unambiguous; otherwise use neutral English. Avoid stereotypes, forced accents, guessed regional customs, or unprovided demographic attributes.

Write everyday, first-person fictional viewpoints, not expert endorsements or brand copy. Ground every objective capability and limitation in supplied facts. Invent only harmless scene framing and subjective reactions compatible with those facts. Keep these invented reactions inside explicitly fictional items; never treat them as observed customer experiences.

Meet the chosen word band and the independent minimum of 30 characters in the body. Count whitespace-separated tokens in `review_text` only; contractions and hyphenated words count once. Avoid standalone punctuation tokens, metadata, and padding. Do not include a bracketed fictional prefix in the review body.

Use light, readable variations in punctuation, contractions, occasional lowercase, or a minor typo only when appropriate for internal voice testing. Never damage product names, specifications, limitations, the boolean, or prompt instructions. Do not introduce defects or unsupported negative outcomes to justify a rating of 8 or 9. Distinguish voices through emphasis and subjective fit rather than false facts.

### 5. Pair a video-generation prompt with every draft

Follow [video-prompts.md](references/video-prompts.md). Write one nonempty prompt per review, not a screenplay, URL, or actual video. Include setting, generic fictional subject and viewpoint, supported use and object behavior, camera style, emotional tone, lighting/environment, and pacing/action beats.

Tie the visual idea closely to the paired text. State that it is a fictional internal creative concept and keep the visible fictional/internal-use notice throughout the clip. Do not add customer badges, testimonial-platform UI, fabricated rating widgets, named reviewers, or verification cues.

Require the downstream video model to receive the actual product reference and preserve its appearance and supported function. Do not claim to have inspected that reference. Do not invent color, geometry, ports, packaging, or accessories to compensate for its absence here. Tell the downstream stage to stop for missing or conflicting references rather than guess.

### 6. Validate before returning

Complete [quality-checklist.md](references/quality-checklist.md). Check exact total, both percentage distributions, word bands, 30-character minimum, unique review wording, all four fields, `is_fictional: true`, and a grounded paired prompt for every item.

Repair errors before emitting. If the entire valid batch cannot be completed without unsupported padding, return only an error. Do not return an error alongside a partial array.

Use no code, web research, paid connectors, accounts, or media-generation tools at runtime. Perform the checks directly when no validator is available. Do not claim that manual checking guarantees machine-valid output; downstream software should independently validate the schemas and disclosures.

## Resources

Use [integration-notes.md](references/integration-notes.md) for consumer responsibilities and upstream field compatibility. Consult [examples/README.md](examples/README.md) before using the fictional [input](examples/input.json) and complete [output](examples/output.json) as test fixtures. Use [behavioral-cases.json](examples/behavioral-cases.json) as a review checklist, not proof that an agent has executed those cases.
