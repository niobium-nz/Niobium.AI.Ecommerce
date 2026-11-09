# Contract and integration

## Input

Accept the finalized Ecommerce Develop Strategy successful JSON object extended
with one required property at the same root level:

| Property | Required type |
| --- | --- |
| `product_details` | Finalized product-details object, including the three approved product-naming fields |
| `pricing_economics_and_offers` | Finalized economics object, including nullable bundle slots |
| `mobile_first_landing_page_plan` | Finalized landing-page plan |
| `customer_segments` | Finalized segment array |
| `product_reference_board` | String containing an HTTP(S) image URL or local file path |

Read `schemas/input.schema.json` for all nested fields. The four upstream
sections and their constraints are retained without alteration. The only root
extension is `product_reference_board`. Do not use a `market_strategy` wrapper,
accept a Market Strategist input instead of its output, or accept its error-only
result as a successful strategy. Preserve lower snake case throughout.

Illustrative locator values (not live resources):

```json
"https://assets.example.invalid/product/reference-board.png"
```

```json
"/mnt/data/product-reference-board.png"
```

```json
"C:\\product-assets\\reference-board.png"
```

A relative path is permitted only when the executing host has a definite,
authorized workspace to resolve it against. A path on the user's phone or
computer is not automatically available to a remote chat environment. Do not
expand a guessed user home directory or search unrelated folders. The schema
checks locator syntax, not existence, access, decodability or actual image content.
Reject unsupported URI schemes, inline base64 and opaque attachment IDs. Obtain
an accessible file path or HTTP(S) locator instead of fabricating one.

## Output

Treat the compact supplied output model as authoritative. On success return:

```json
{
  "status": "ok",
  "image_prompts": []
}
```

Each populated item has exactly `asset_id`, `orientation`,
`overlay_copy_suggestion` and `prompt`. Use no generated-image location or
`image_variants`. Preserve the source document's strategic analysis as concise
commercial context within each `prompt`, not separate output fields. Rename its
long-form `llm_image_prompt` concept to the supplied contract's `prompt`.

Preserve the source document's two status values. Use the project's established
`error` field to explain non-success outcomes because the compact model contains
no question/message list:

```json
{
  "status": "needs_clarification",
  "image_prompts": [],
  "error": "The board contains two different product variants. Provide multiple views of one SKU only."
}
```

```json
{
  "error": "Invalid input at product_reference_board: supply an HTTP(S) URL or local file path."
}
```

Do not add `creative_strategy_summary`, `image_concepts`, `clarifications_needed`,
per-item priority fields or an output copy of the board location. Array order
carries priority. The `error` field is absent on success; it is a diagnostic
extension, never an extra deliverable or a substitute prompt.

## Reference handoff

Keep the board location in the invocation/orchestrator context. The downstream
producer must receive the same actual image alongside each prompt. A prompt's
words "supplied product reference board" do not load the image automatically.
The producer must independently have authorized access to those pixels. Do not
copy signed URLs, local paths, analytics IDs or credential-bearing query strings
into prompt text or overlay copy. Use `asset_id` as metadata only, never image text.

## Syntax versus semantics

JSON Schema validates structure, field types, status branches, allowed orientation
labels and the final aspect-ratio phrase. It cannot certify product fidelity,
claim support, reader access, concept quality, unique IDs by field value, ratio
agreement or correct source mapping. Perform those checks separately. No runtime
validator script is required or bundled; do not claim machine validation occurred
unless a validator was actually run by the host.
