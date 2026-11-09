# JSON contract

## Input

Accept a top-level array. Accept zero items. Require a nonblank string
`ad_archive_id` on every item; all other input properties are optional.
Use `schemas/input.schema.json` as the authoritative structural contract.
Reject unknown keys, including alternate casing. Do not coerce values.

| Object | Nullable string fields | Other fields |
| --- | --- | --- |
| Ad | `collation_id`, `page_id`, `page_name`, `url`, `start_date_string`, `end_date_string` | Required string `ad_archive_id`; nullable `snapshot`; boolean `is_active`, `has_user_reported`, `page_is_deleted`, `contains_digital_created_media`, `contains_sensitive_content`; integer `start_date`, `end_date`; string arrays `categories`, `publisher_platform` |
| Snapshot | `page_id`, `page_profile_uri`, `page_name`, `page_profile_picture_url`, `display_format`, `cta_type`, `cta_text`, `caption`, `link_description`, `link_url`, `title`, `original_image_url`, `resized_image_url` | Nullable `body`; integer `page_like_count`; boolean `is_reshared`; arrays `page_categories` (strings), `images`, `videos`, `cards` |
| Snapshot body | `text` | None |
| Card | `body`, `cta_type`, `cta_text`, `caption`, `link_description`, `link_url`, `title`, `original_image_url`, `resized_image_url`, `video_hd_url`, `video_preview_image_url`, `video_sd_url` | None |
| Image | `original_image_url`, `resized_image_url` | None |
| Video | `video_hd_url`, `video_preview_image_url`, `video_sd_url` | None |

Treat omitted arrays as empty working collections. Keep explicit null valid only
on nullable properties. Lists may be empty, but list members must have the declared
type; null list members are invalid. Do not inject defaults into the source JSON.
Leave absent metadata absent; do not infer dates, timestamp units, or flags.
Preserve large integer values exactly; their representation must not affect clustering.

Treat URL and date fields as supplied strings, not externally verified facts.
An unusable landing-URL string is missing usable evidence, not a structural schema
error. A number in that field is a schema error. Reject malformed JSON and duplicate
object keys before schema checks; a schema cannot detect keys already lost by a parser.

## Success

Return one array, with all ten keys in every cluster. Emit them in this order for
consistent readability (object-key order is not a JSON validity requirement):

| Key | Type and meaning |
| --- | --- |
| `cluster_id` | Nonblank string; readable cluster key, unique in this response |
| `cluster_label` | Nonblank string; supported name or neutral unknown-offering label |
| `landing_page_domain` | Registrable landing domain string, or JSON null |
| `likely_product_name` | Explicitly supported offering name, or JSON null |
| `category_guess` | Explicitly supported product-specific category/noun, or JSON null |
| `known_features` | Array of nonblank, deduplicated, explicitly stated feature strings |
| `cluster_confidence` | Exactly `High`, `Medium`, or `Low` |
| `ad_archive_ids` | Nonempty array of distinct, exact input ID strings |
| `is_product` | Boolean |
| `is_service` | Boolean; exactly the opposite of `is_product` |

Use actual nulls, never placeholder strings such as `"null"`, `"unknown"`, or
`"N/A"` for unavailable nullable values. Do not infer product names from advertiser
names alone. A conservative default classification is not new evidence that the
item is a tangible good.

Allow the same source ID in several output objects when several products/offers
are explicitly identifiable in one ad. Require every distinct valid input ID to
appear somewhere in a successful response. Include no ID not present in the input.
Require unique cluster IDs and no duplicate source IDs within a cluster.

## Failure

Return an object with exactly one field:

```json
{"error":"Invalid input at [2].ad_archive_id: expected a nonblank string. No products were returned."}
```

Fail the entire batch for structural invalidity or absent/blank IDs. Do not place
errors in an output array, add a success envelope, or return partial clusters.
Use the same shape for processing/resource/context failures that prevent a complete
valid response. Keep error text useful, but do not dump the full source payload.
Handle semantic ambiguity with conservative Low-confidence clusters, not an error.

## Beyond JSON Schema

Check source-ID coverage, foreign IDs, ID uniqueness, feature support, category/name
support, correct domains, multi-product completeness, and confidence semantics in
addition to schema validity. JSON Schema alone cannot establish these facts.
An instruction-only agent self-check is not machine-enforced validation.
