# Target-country research workflow

## Contents

- Establish scope
- Fixed sequence
- Required buckets and source priority
- Evidence ledger and final attribution
- Failure decisions

## Establish scope

Resolve `target_country` to one market. Determine the relevant country name,
search terminology and authoritative local sources during research. Use localized
product terms when necessary for useful coverage; keep the output in the user's
conversation language unless requested otherwise. Never assume that every country
uses English, one currency, one climate, or nationwide uniform regulation.

Use the current runtime date for freshness checks; do not freeze a date in the
skill. Source names in the country-code snapshot are labels, not market facts.
Research current retailer access, currencies, shipping context, regulatory authority
and local restrictions rather than deriving them from a country-code suffix.

## Fixed sequence

Run these eight logical query families in this order. Individual tool calls may
return several results; this is a research sequence, not a billable-API contract.
Select exact queries from the normalized type, buyer pain, category and supported
features. Preserve the generic form factor and main job. Use country filters when
the host supports them; otherwise include the country and check actual source scope.

| Step | Query family | Goal |
|---|---|---|
| Q1 | `[normalized type] [target country]` | Establish generic local product presence and candidate sources |
| Q2 | `[normalized type] buy [target country]` | Confirm retail context and local prices |
| Q3 | `[normalized type] site:[verified local marketplace/retailer A] [country when needed]` | Examine one relevant shopping source |
| Q4 | `[normalized type] site:[verified independent local marketplace/retailer B] [country when needed]` | Cross-check a second independent source |
| Q5 | `[buyer pain OR supported feature phrase] [target country]` | Check problem-solution and buyer language |
| Q6 | `[normalized type] ads [target country]` | Examine public ad/competition evidence without paid calls |
| Q7 | `[verified relevant authority/topic] [normalized type] [target country]` | Screen compliance/import/claims and platform issues |
| Q8 | `[normalized type] recall [target country]` | Check product safety, warnings and trust risks |

Choose A/B from actually verified target-market operators, preferably discovered
in Q1/Q2. Do not hard-code Australian marketplaces or assume an international
operator has a local site. If two credible distinct operators cannot be found,
use a country-scoped marketplace/retailer discovery query in the missing slot,
report limited coverage and do not invent or duplicate an independent source.
A website's local suffix is not by itself proof of local availability.

After the generic sequence, run one branded validation query when an identifiable
brand/model or competitor brand appears in supplied product/ad data. Do not guess a
brand from an opaque ID. Do not let brand-only evidence replace generic local evidence.

Open relevant results and inspect the product match, price context and source scope.
Use targeted follow-up browsing or searches only to resolve material gaps, especially
official-source safety/claim questions. Do not drop one of the four required buckets
because a different bucket produced many hits. If capability is completely unavailable,
stop rather than making pointless repeated calls. Track incomplete/failed steps.

## Required buckets and source priority

Cover retail/marketplace reality, in-scope competition, compliance/import risk,
and risk/trust checks. Prefer, for each applicable claim:
1. Current official regulator, border, consumer/product-safety, recall and platform-policy sources.
2. Current target-market retailer and marketplace listings.
3. Supplied source ads and accessible ordinary public ad-transparency pages.
4. Broader search evidence where stronger material is unavailable.

Inspect actual applicable official sources; do not rely on a reseller's legal claim.
National, supranational or subnational rules may matter. Where destination details
or certification documents are missing, describe the unresolved issue and score
conservatively; do not assume the most permissive jurisdiction. Risk screening is
not legal advice or proof of product-specific compliance.

Keep currencies and tax/shipping qualifiers attached to price observations. Do not
compare different currencies as though identical or invent exchange rates. Prefer
current local prices. Do not infer supplier cost, ad acquisition cost, margin, order
volume, return rates or continuous ad delivery from retail/ad presence alone.

## Evidence ledger and final attribution

Keep an operational ledger of queries attempted/completed, source/page identities,
access dates, source countries, observed facts, product relevance, source limitations
and tool failures. This ledger is an audit record, not private chain-of-thought.
Do not add it as a new response field. Summarize material facts with source names
in `top_reasons`, `key_risks`, or `caps_applied`; list only actually examined source
names/domains in `sources_checked`. Label supplied Scout reports/ads as supplied
sources, not newly visited sites. Never cite a source that was only invented for an
example. Keep ordinary web-tool citations inside existing strings when required
by the host; do not attach prose or extra properties to JSON.

## Failure decisions

- No web tool, denied access, or every operational attempt fails without usable
  research: return error-only. Do not score from memory alone.
- Some usable current research and partial tool failures: return a full score plus
  `error`; reduce confidence, apply 60 if failure is material, and any stricter caps.
- Working searches produce little/no matching evidence: this is thin evidence,
  not a tool error. Return a conservative score, normally with the 60 ceiling and
  the 35 ceiling where compliance risk remains unresolved. List no fabricated hits.
- Sources conflict: disclose the disagreement, prefer lower supported bands and
  relevant caps, without treating disagreement itself as operational failure.
- A supplied Scout report contains an error: record that prior as unavailable or
  partial evidence. Do not label the current scoring run a tool failure solely
  because an upstream report failed.

Do not initiate paid ScrapeCreators/Ad Library calls or paid enrichment. Do not
invoke another skill to bypass that restriction. Ordinary web research, not fresh
paid competition discovery, is the Scorer's responsibility.
