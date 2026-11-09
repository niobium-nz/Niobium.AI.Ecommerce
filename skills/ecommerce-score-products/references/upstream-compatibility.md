# Upstream compatibility

## Ecommerce Cluster Ads

Consume one `CompetingProduct`-shaped object as `product`, not the Transformer's
entire output array. Accept all its fields. Require the physical-product flags for
this scoring workflow. Keep the product's cluster/source IDs as references, not
signals of market strength. Do not call the Transformer or alter its clusters.

Permit empty `ad_archive_ids` and sparse ad metadata in standalone scoring inputs.
When IDs exist, isolate the selected product and its matching ad evidence. The same
ad may promote several products; evidence for a different carousel item must not
raise this product's score. Missing source records reduce evidence confidence but
do not automatically invalidate an otherwise identifiable physical product.

## Ecommerce Analyze Competition

Use the finalized language-agnostic response schema bundled alongside the Scorer's
input schema. It uses `raw_ads_discovered.tool_call_made`, not the old MCP fields,
and allows a top-level `error` for a partial report or an error-only response.
Do not reconstruct missing counts from an error-only object; do not reinterpret -1
as zero. Do not silently accept legacy property aliases.

Compare countries after resolving alpha-2/alpha-3 codes. Use only matching-country,
product-relevant evidence for local competition. A report from a different country
is separately labeled transferability context and cannot establish target-market
legality, price positioning, demand or seller density. A matching country alone
does not establish matching product scope; inspect query and stated limitations.

Prefer filtered in-scope evidence, preserve all coverage/age/window limitations,
and avoid summing overlapping Scout queries as independent advertisers or ads.
Do not copy or invert the Scout's 0-10 rating into a 0-15 scoring component. A high
competition rating and strong competition advantage are not the same concept.

The Scout may focus on older active ads. Treat that as a sample restriction, not
proof of all-market saturation or absence. The Scorer must not exclude supplied
newer creatives solely because they lack three months of age. Ad duration alone
does not prove continuous delivery, profitability or a successful creative test.

Treat ads with no country evidence as unlocated/source evidence, not local ads.
Use public current target-country research to assess transferability. Do not infer
source countries from this project's name, the user's location or target_country.
