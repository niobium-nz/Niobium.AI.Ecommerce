# Approved decisions and runtime boundaries

## Source workflow retained

Keep one query and country per run, evidence tethered to retrieved ads, literal
post-retrieval exclusions, operator notes reflected in interpretation, context-only
product interpretations, raw/in-scope distinctions, proxy limitations and no
unsupported market/sales/profit claims. Do not execute a recommended next query.

## Approved amendments

| Area | Adopted decision |
| --- | --- |
| Identity | Ecommerce Analyze Competition / ecommerce-analyze-competition |
| Contracts | Language-neutral JSON Schema Draft 2020-12, lower snake case, no source-language dependency |
| Packaging | No original source Markdown, source models or executable scripts |
| Integration | Connected ScrapeCreators Facebook Ad Library keyword search; not the old adslibrary server or unrelated Meta Ads account tools |
| Search | Fixed sort, match, ad, status and media values specified by the user |
| Date | Send three-calendar-month cutoff through start_date, yyyy-MM-dd |
| Country | Accept established alpha-2/alpha-3 input rule; always map tool country to uppercase alpha-2 |
| Paid calls | One page initially; up to three attempts, only same-query cursor continuation; stop on failure without retries |
| Eligibility | Require returned active status and actual start on/before cutoff, plus scope/exclusion checks |
| Scores | Numeric competition/confidence, -1 unknown; qualitative rubric documented in output |
| Demand | Weak proxy commentary only inside inference, not an added output field |
| Tool fields | tool_call_made instead of mcp_call_made; no nested mcp_error |
| Errors | error-only when no usable result; complete partial report plus top-level error after later failure |
| Zero results | Normal report, not a tool error |

## Portability

Run with instructions, schemas, readable references and the connected authorized
search action. Do not require Python, a shell, Playwright, a browser, a specific
search engine, or either earlier ecommerce product skill. A host without the search
action cannot execute this workflow and must return the error channel.

The optional plugin wrapper packages the skill only. It does not install, configure,
authenticate, copy or grant access to ScrapeCreators or the user's Meta Ads plugin.
Do not invent an MCP server URL or credentials. Inspect the action exposed by the
host and use the selected provider capability. No external connector was called
for paid ad retrieval during authoring.

Model self-checking is not machine-enforced JSON validation. A consuming application
should validate the supplied schemas independently. The package does not promise
deterministic subjective scores, unchanged future provider APIs, platform-wide
availability, or identical results across runs. The provided fixtures are synthetic
and the local build checks are not live-host/end-to-end evaluations.
