# Domain parsing and readable cluster IDs

## Contents

- Landing context
- Offline root-domain lookup
- Text normalization and escaping
- ID construction and collision handling
- Ordering and repeatability

## Landing context

Read only `snapshot.link_url` as the landing URL. Trim surrounding whitespace in a
working copy. For a well-formed absolute HTTP/HTTPS URL, or a scheme-relative URL
with an explicit host, take the authority's hostname, not its user-information,
port, path, query, or fragment. Lowercase the host and remove a terminal DNS dot.
Do not blindly strip `www` before suffix matching. Never access the URL.

Do not add a guessed scheme/domain to a relative, malformed, or bare ambiguous URL.
Treat an unusable URL as unavailable landing evidence. Do not recover a destination
from query parameters, a top-level ad `url`, card links, or a page profile link.
A wrapper/short-link host is not proof of the final seller; never follow redirects.
If supplied evidence makes the destination ambiguous, isolate and use Low rather
than assert a seller relationship.

Treat IP addresses and single-label hosts as hosts, not registrable domains. If
needed, keep their literal normalized host as an isolation key, use null for
`landing_page_domain`, and set Low. Do not invent a registrable domain.

## Offline root-domain lookup

Use `public-suffix-rules.json` as a frozen parsing reference. Read only the relevant
entry in `rules_by_final_label`; the full table need not enter context. Both public
and private suffix rules are included so hosted tenants do not collapse together.
The reference is a frozen local snapshot with unknown upstream revision; it is
not claimed to track today's suffix changes. Do not refresh it during a run.

Match rules against complete hostname labels, not substrings. Apply:
1. Find exact rules and wildcard rules that match the hostname's suffix. A `*`
   matches exactly one whole label at the left edge of the rule.
2. Prefer a matching exception (`!`) rule; remove its leftmost label to obtain
   the public suffix. Otherwise use the matching rule with the most labels.
3. If no rule matches, use the conventional implicit `*` rule (one suffix label).
4. The registrable root is that suffix plus the immediately preceding label.
   If no preceding label exists, there is no registrable root.

Do not use a last-two-label shortcut for all hosts. Illustrative parser results:

| Supplied hostname | Registrable root |
| --- | --- |
| `www.shop.example.com` | `example.com` |
| `shop.example.co.uk` | `example.co.uk` |
| `shop.example.co.nz` | `example.co.nz` |
| `alice.myshopify.com` | `alice.myshopify.com` |
| `bob.myshopify.com` | `bob.myshopify.com` |
| `www.ck` | `www.ck` (exception rule) |
| `a.b.ck` | `a.b.ck` (wildcard rule) |

Treat this as syntax parsing, not verification that a domain exists or belongs to
an advertiser. Do not infer business ownership from suffix data.

Use the supplied lowercase hostname representation when exact Unicode/ASCII
conversion is unavailable. The lookup includes Unicode rules and ASCII aliases;
do not invent a Punycode encoding. If equivalence between mixed representations
is uncertain, keep them separate and use Low. Do not access DNS or another service.

When the lookup/resource is unreadable or the root cannot be determined reliably,
do not guess. Use `host=<literal-host>` only as a conservative per-ad isolation key,
set `landing_page_domain` to null, and mark Low. When no usable host is present,
use the advertiser or ad-ID fallback below. This conservative branch keeps the
workflow usable without script execution or an online domain lookup.

## Text normalization and escaping

For a readable product or advertiser component, trim, lowercase, and replace runs
of whitespace with one ASCII hyphen. Preserve all other meaningful characters,
including model punctuation and non-Latin text. Do not translate or transliterate.
Use an input-supported canonical product phrase, not unordered invented keywords.
Use `unknown` only as an internal ID component for unclear identity.

Escape the following in every component, in this order, to avoid delimiter collisions:
replace `~` with `~0`, `|` with `~1`, and `=` with `~2`. Leave other characters intact.
For ad-ID components, apply escaping to the exact original ID without lowercasing,
trimming, or replacing whitespace. For domain components use the normalized host/root.
Do not truncate keys, hash them, or substitute random IDs or timestamps.

## ID construction and collision handling

Choose one context component:
- `domain=<root>` when the landing domain is available.
- `host=<literal-host>` only for uncertain-root per-ad isolation.
- `advertiser=<normalized-name>` when landing context is unusable and the supplied
  advertiser is unambiguous.
- `ad=<exact-escaped-ID>` when neither context nor an unambiguous advertiser is usable.

Build the base ID as:

```text
<context>|item=<normalized-canonical-name-or-unknown>|type=<product-or-service>
```

For example:

```text
domain=example.com|item=trail-bottle|type=product
advertiser=northstar-outdoors|item=trail-bottle|type=product
ad=A-100|item=unknown|type=product
```

Add `|ad=<smallest-exact-source-ID>` when ambiguous identity, incompatible evidence,
or a `host=` fallback requires isolation. For unknown identity, always isolate by
ad ID. Do not append a redundant `ad=` component when context is already that ad.
Use the lexicographically smallest exact ID for a cluster with several source IDs.

If distinct clusters still have the same base ID, append that ad discriminator to
all colliding base IDs that do not already carry that discriminator. If they still
collide (for example, two distinct offers
from one ad with the same vague name), append `|item_no=1`, `|item_no=2`, and so on.
Assign that ordinal by sorting the colliding candidates' distinguishing supplied
phrases, then features, then exact source ID and source field path. Keep a source
card index as the final within-ad tie-breaker when needed. If candidates have no
actual distinguishing evidence, do not invent distinct products just to create IDs.

Do not let a collision force two distinct products to merge. Keep explicit variants
in the item phrase whenever supported, before resorting to the discriminator.
All IDs must be unique within the output batch.

## Ordering and repeatability

Deduplicate exact ad-ID strings, preserving their exact values. Sort them by ordinary
case-sensitive Unicode lexicographic order, not numerically. Deduplicate features
by normalized case/whitespace comparison; keep a supported source wording, then
sort by normalized text and exact text as a tie-breaker. Sort output clusters by
`cluster_id` using the same case-sensitive lexicographic order.

Use the same canonical-source priorities, grouping tests, frozen suffix lookup,
escaping, and tie-breakers on every run. Target repeatable results for identical
input under identical skill data. Do not promise bit-for-bit model determinism:
semantic extraction/clustering remains model-driven without an executable engine.
IDs are batch-local stable identifiers, not global catalog keys; adding/changing
source evidence or the suffix dataset can change a cluster and its identifier.
