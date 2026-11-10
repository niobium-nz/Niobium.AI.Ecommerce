# Approved decisions and runtime

## Approved behavior

| Topic | Rule |
| --- | --- |
| Name | Ecommerce Normalize Products / `ecommerce-normalize-products` |
| Separate predecessor | Rename the earlier ad-clustering skill to Ecommerce Cluster Ads / `ecommerce-cluster-ads`; do not change its behavior |
| Contracts | Language-neutral JSON Schema Draft 2020-12; lower snake case keys |
| Packaging | No original uploaded specifications/models, executable scripts or source-language dependencies |
| Input | One product object; product_name and country required |
| Country | Assigned ISO alpha-2 or alpha-3 letter codes only |
| Features | Omitted or [] means no features supplied; nonempty list prohibits inferred additional functions |
| Research | Optional; use available host tools instead of requiring Playwright/Google; scope actual research to country |
| Necessary research unavailable | Return only a JSON error object |
| Normal statuses | ok, needs_more_info, ambiguous |
| Clarifications | Put requests in notes_for_downstream_agent; no plain-text exception |
| Errors | {"error":"..."} for invalid input/operational failures |
| Confidence | Overall integer 0 through 10; per-interpretation High/Medium/Low |
| Tier counts | Targets, not padding quotas; unsupported brand/model tier may be [] |
| Scope tokens | Semantic scope constraints, allowing equivalent wording |
| Recommendations | Deduplicated A, then B, then supported C; D only when A and B are empty |

Treat exact key names, closed object shapes, required normal-result sections,
empty/null handling, case-insensitive country matching, echo preservation and
source-log conventions as documented implementation mechanics. They do not supply
product evidence or replace the agreed workflow. Do not retain dependency on the
uploaded models or a language-specific serializer.

## Runtime

Require readable skill instructions/resources and user input. Do not require script
execution, Python, a shell, external packages, credentials, or an ads connector.
A missing browser/search tool matters only when research is necessary, including
when required by higher-priority host instructions. A browser-free sufficient-input
run is valid. An optional local JSON Schema validator may improve structural checks
but is not a prerequisite and cannot validate semantic research truth.

Do not install software, enable capabilities, change settings, or send input to an
external service merely to validate JSON. Use authorized research tools only for
necessary product/terminology investigation. Follow host privacy and tool policies.

## Examples and test scope

`examples/cases.json` contains synthetic contract fixtures and expected responses.
Cases tagged with research stubs use fictional .example domains and explicit mock
observations solely for authoring-time validation. Never cite those domains as
real research or reuse fixture observations for a user's product. Error wording
is illustrative; the single-field error shape is normative.

The separate delivery report records build-time checks actually performed.
Schema/example validation is not a live-agent behavior test. No claim of end-to-end
execution in ChatGPT web, iOS, Codex or another host follows from packaging. Model
self-checking is not machine-enforced schema compliance. Confidence judgments and
semantic keyword choices can vary across runs even with identical inputs.

## Distribution and migration

The standalone archive contains one `ecommerce-normalize-products` folder. The
optional wrapper contains the identical folder under `skills/` plus a manifest.
Use the host's supported installation flow; packaging alone does not install,
publish, grant access, or promise availability in a particular workspace.

The previous ad-clustering artifact used this identifier in error. Replace that
installed copy with the renamed `ecommerce-cluster-ads` before adding this
new single-product normalizer. Do not leave the old ad-clustering normalizer and
this single-product normalizer registered under the same identifier. Install only
one distribution form of each skill in a given host.
