# Domain boundaries without runtime code

Use the current real browser's reliable registrable-domain information when
available. Otherwise consult only the relevant final-label entry in the bundled
`public-suffix-rules.json`; do not load the entire lookup unnecessarily. Retain
ICANN and private rules so two unrelated hosted storefronts are not merged.

The bundled parsing reference is a frozen snapshot with unknown upstream revision,
reused unchanged from the earlier product-transformer package. Its metadata records
that limitation and its original provenance. It is not claimed to be current or
product evidence. Do not refresh it from another website during a profiling run.
If uncertainty about a current boundary prevents safe scope enforcement, stop the
affected navigation with blocker/error instead of guessing.

For the browser-normalized lowercase hostname, match whole labels. Prefer a
matching exception rule beginning `!`; remove the exception's leftmost label to
obtain its public suffix. Otherwise select the matching exact/wildcard rule with
the most labels. A wildcard matches one label. The registrable domain adds the
label immediately to the left of the public suffix. Do not universally take the
last two labels. If no meaningful boundary can be established, do not assert one.

Examples for parser review, not sites to visit:
- `www.shop.example.com` -> `example.com`.
- `shop.example.co.uk` -> `example.co.uk`.
- `shop.example.co.nz` -> `example.co.nz`.
- `alice.myshopify.com` and `bob.myshopify.com` are separate tenant scopes.
- `www.ck` uses the exception; `a.b.ck` uses the wildcard rule.

Use the browser's normalized ASCII/Unicode equivalence when available; do not
invent a Punycode conversion. If conflicting hostname representations cannot be
resolved reliably, refuse the affected navigation. IP addresses and single-label
hosts are not registrable ecommerce domains. Never treat a public suffix itself
as a seller's scope, or use substring matching to accept a malicious lookalike.

## Third-party data notice

The lookup states MPL-2.0 licensing. Its upstream data source is
`https://publicsuffix.org/list/public_suffix_list.dat` and the format reference is
`https://github.com/publicsuffix/list/wiki/Format`. The license text is available
at `https://mozilla.org/MPL/2.0/` and in the bundled `MPL-2.0.txt`. Preserve the lookup metadata when redistributing.
The original implementation code is not bundled, and no dependency on that
implementation is required. A network attempt during this build could not obtain
a fresh downloadable list; the existing snapshot was retained rather than relabeled
as current. Scope parsing remains a documented limitation of no-script execution.
