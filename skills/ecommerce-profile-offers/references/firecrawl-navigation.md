# Source-market retrieval with Firecrawl

This workflow runs when the invoking agent selects Firecrawl. Use the authorized
Firecrawl plugin, hosted MCP, SDK or API on the current platform. It needs no
desktop browser. Accept an assigned uppercase alpha-2 source country. Keep that
country fixed for the entire investigation.

## Read one public page at a time

Use Firecrawl Scrape on the supplied landing URL. Start with these options. Keep
the location, formats, cache and access settings on every follow-up and allowed
retry. MCP adds its required
`objective` and conversation context as specified by the exposed tool.

```json
{
  "url": "<current public URL>",
  "location": {"country": "<source_country>"},
  "formats": ["markdown", "links"],
  "onlyMainContent": false,
  "maxAge": 0,
  "proxy": "basic"
}
```

Use equivalent field names in a Firecrawl SDK. Request the full landing and
product pages to retain footer policy links. On a linked returns, guarantee or
shipping policy, set `onlyMainContent: true` on its first request. This keeps
policy text and links while cutting repeated menus and footers. Keep both
formats, including on these policy pages. Use Markdown labels together
with the links array. A merchant link is a lead. Read its page before attributing
a policy or claim to it. Keep large pages and link lists inside the investigation.
When the host exposes a structured result, read its effective URL and status
before selecting the evidence snippets and relevant links. If Firecrawl retains
the result server-side, use its supplied read action. Record the limitation if
the host truncates the result and the missing metadata or evidence cannot be read.

The source country requests a market view. Record the country, currency, stock
and delivery conditions the merchant actually shows when relevant. An unsupported
location, regional denial, CAPTCHA, login wall or bot check is a blocker. Keep the
country fixed and stop the affected request. Reserve an entry retry for a transient
retrieval failure. A tool or country parameter unavailable before the first request
yields an error-only result. Send only the public URL and retrieval parameters.

## Assemble and validate the profile before following links

After the first accepted page, immediately extract its evidence into a complete
profile object and check it against `schemas/output.schema.json`. Create all
required keys in each present object. Use null for unknown scalars and [] for
unknown lists, with the literal `Unknown` for a present vendor whose platform
cannot be established. Keep this schema-valid profile before making
another request. A complete object can contain unknown facts. Record conflicting
claims in `blockers` and leave the disputed facts out of the claims used by later
stages. A conflict in a specification does not make an otherwise identified
product unknown.

Once the accepted evidence identifies the primary product and captures the
material offer terms visible on the entry, finish the profile unless a specific
material gap can be answered by an eligible link. For example, a product link
can establish the offer behind an advertorial, and a policy link can clarify a
material condition that the entry only names. When the entry already states the
relevant terms, do not open another page just because it has a policy link or the
profile has optional unknowns. The page limits are ceilings. Stop with adequate
evidence and leave time for the invoking agent to validate and use the profile.

## Choose follow-up pages

1. Validate the entry and each candidate against the URL exclusions before sending
   a request. Establish the merchant's registrable domain with the bundled suffix
   rules (including private storefront tenants). Read the provider's effective URL
   (for example `metadata.url`) and status before accepting a response. The input
   or `sourceURL` alone does not establish the destination. Discard content and links
   when the destination is unknown, outside the entry merchant, or a denied path.
   Firecrawl can follow redirects internally. This controls requests the agent
   selects and evidence it accepts. It does not claim interception of vendor
   redirects. Reject out-of-scope entry destinations in this mode.
2. From each accepted page, resolve relative links against its effective URL. Use
   the canonical keys, alias handling and eligibility rules in the **URL keys and
   queue** and **Eligibility and priorities** sections of browser-navigation.md.
   Deduplicate tracking variants, fragments and already attempted destinations.
   Preserve selectors that change the offer. Keep only linked pages relevant to
   the same item or the merchant's policies. Skip external sites, other products,
   account, cart, checkout, site search and pagination.
3. Visit the highest-priority eligible link that could fill an evidence gap.
   Start with the primary offer/PDP, then guarantees/returns, shipping, proof
   and materials. Break ties by canonical URL. Before each follow-up, identify
   the material gap it could resolve. Extract the supporting text and source URL,
   update and validate the profile, then reassess whether another page is needed.
   Keep the last validated result until its replacement passes the schema. Keep
   a short internal ledger of attempted URL, effective URL, depth, source country,
   findings and blockers.
4. Apply the shared limits in policy.json: serial requests, ten unique attempted
   pages including the entry, two link hops, one attempt per follow-up and at most
   eleven deliberate loads including one transient entry retry. Check for empty
   queue, sufficient evidence, actual cycles and three consecutive page slots with
   no progress. Additional policy wording can resolve an outstanding investigation
   question even when the policy URL was already present. Keep partial evidence
   and explain material gaps when a limit or error stops work.

## Dynamic content and completion

Use the rendered scrape when sufficient. If an ordinary consent overlay or
JavaScript shell obscures needed content and the host exposes Firecrawl Interact
and its stop action, reuse the scrape ID from this localized page. If the host
requires a fresh URL session, send the same country under
`scrapeOptions.location.country` and use the same basic access mode. Count a new
page load within the same limits. Restrict actions to observing the page, scrolling
and the least permissive ordinary consent needed for access. Confirm the effective
URL before consuming evidence. End task-created sessions with
`firecrawl_interact_stop` or the equivalent SDK/API call on every exit. A simple
scrape creates no session for the agent to close.

Apply extraction.md and the shared output schema. Preserve the original entry
URL, observed final entry URL and known retrieval time. Keep unsupported facts
unknown. Return the latest validated profile as soon as retrieval is done. If an
operational failure or host limit interrupts enrichment, use the last validated
profile and add the observed blocker and any applicable error, then validate the
final object again. Report a specific schema path if a complete object cannot be
validated. Keep per-page evidence internal except where the profile contract calls
for a source or diagnostic.
