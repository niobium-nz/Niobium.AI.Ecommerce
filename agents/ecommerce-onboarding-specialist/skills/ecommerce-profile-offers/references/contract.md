# JSON contract

Treat the schemas as language-agnostic JSON Schema Draft 2020-12. Do not require a
source-language model, serializer, executable validator or runtime library.

## Input

Use exactly one object: `{"landing_page_url":"https://shop.example/product"}`.
Reject extra keys and invalid types. Do not accept a bare URL as the JSON contract,
or introduce a target-country, product selector, crawler budget or consent flag.
Accept HTTP and HTTPS only, with a real hostname and no embedded credentials.
Reject local/private addresses, local files and executable URL schemes. Do not
make DNS or network probes through another tool to repair an invalid URL. Apply
the navigation denylist before the entry load as well as before following links;
a forbidden entry destination produces an error-only result.

## Profile shape

Emit all six profile properties whenever returning a profile:

| Field | Type and meaning |
| --- | --- |
| `url` | String or null. Preserve the input URL exactly when available. |
| `final_url` | String or null. Observed entry destination after redirects, not a later visited URL. |
| `retrieval_date_time_utc` | UTC RFC 3339 timestamp ending in `Z`, or null if the real run time is unavailable. |
| `product` | Product object or null. Do not construct a product when the offer cannot be identified. |
| `vendor` | Vendor object or null. Retain supported vendor evidence even when product identification fails. |
| `blockers` | Deduplicated list of concrete gaps, conflicts, access failures and stop conditions. |

Add `error` only for an operational failure or unidentifiable primary offer.
An error-only object has exactly one key, `error`, with a nonblank human-readable
string. Never use empty/null error values on success or add a status wrapper.

When a nested object is present, emit all its listed fields. Missing scalar values
are null and missing lists are []. Unknown parent objects may be null; do not emit
empty `{}` objects that omit contract fields.

## Product

Use `name` (nullable string), `price` (nullable nonnegative number), `currency`
(nullable string), and string lists `variants`, `bundle_offers`, `key_claims`,
`ingredients_or_materials`, `images`, `videos`. Use `how_it_wins` as a nullable
object containing these twelve string lists:

`positioning`, `differentiators`, `proof_points`, `target_customer`,
`objection_handling`, `comparison_claims`, `calls_to_action`,
`offers_and_guarantees`, `risk_reducers`, `social_proof`, `urgency_scarcity`,
`compliance_language`.

Keep `price` numeric, not an amount/currency object. Do not put comparison prices,
instalment amounts or unsupported currency assumptions into the primary price.
The 5-to-12 key-claim target and 3-to-10 selling-point guidance are editorial
recommendations, not schema minima or permission to invent entries.

## Vendor

Use nullable strings `brand_name` and `domain`, a nonblank string
`platform_detected`, a nullable `policy_urls` object, and a `trust_signals` array. Use lowercase registrable scope
domain for `domain` only when established. Use `Unknown` for platform when no
platform evidence is available in a present vendor object.

`policy_urls` contains exactly nullable `shipping`, `returns`, `privacy` URLs.
Do not add a `terms` key. A visible policy link supports recording its URL, not
claiming that the policy text was inspected. Its content may remain unverified.

Each trust signal has nullable string fields `type`, `value`, `source_url`. Use
short descriptive type labels, not an invented closed enum. Never add an all-null
placeholder signal. `source_url` identifies the approved page showing the signal;
a social URL is the value, not a claim that its external destination was visited.

## Error distinctions

| Condition | Output |
| --- | --- |
| Malformed input or no real-browser control | Error only; no fabricated profile. |
| Entry unreachable after two allowed loads | Profile metadata, null product/vendor, blockers and error. |
| Unresolved overlay, blocked policy page, partial navigation failure | Preserve inspected in-scope data; blockers and error. |
| Cross-domain entry unverifiable or rejected | Never ingest destination fields; known metadata and prior in-scope data, blockers and error. |
| No enforceable post-entry redirect guard | Keep entry evidence; stop further navigation; blockers and error. |
| No primary product among unrelated offers | Vendor evidence if available, product null, blockers and error requesting a specific URL. |
| Absent facts, hidden disallowed controls, normal crawl limit | Best-effort profile with blockers, no error solely for these conditions. |
| Cleanup cannot be verified | Preserve result; blockers and error explaining the remaining session state. |

Do not conflate an absent fact with an access failure. Report material conflicts
in blockers. The contract has no per-field evidence or page-history object; keep
those in the execution ledger rather than adding unapproved JSON properties.
