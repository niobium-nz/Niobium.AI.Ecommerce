# Scope and research rules

## Evidence precedence

Use supplied features as a functionality ceiling when the list is nonempty.
Keep unknown functions unknown even when a similarly named web product has them.
For example, a glove stated to lift pet hair from upholstery is not automatically
a pet-grooming glove; do not introduce deshedding, massage or direct animal use.
A title that conflicts with stated features is a conflict, not permission to add
both sets of functions. A base category is only a container, not feature evidence.

Without supplied features, infer only reasonably indicated form/job/category and
label the inference. The absence of features does not authorize fabricated
materials, mechanisms, efficacy, usage contexts or brand facts. A pure SKU can
remain unidentified. Record concise support, not a hidden reasoning transcript.

For material, mechanism and qualifiers, require explicit input support or a source
actually consulted in research. Add a researched qualifier only when it does not
expand or contradict the authoritative supplied features. Distinguish seller
claims from established facts. Remove unjustified superlatives and market claims.

## Search decision and logging

Use sufficient supplied evidence without browsing. Use a host search or page
reading tool when needed to decode identity or standard same-archetype terminology.
Do not choose a specific provider as a dependency. Do not call ads, shopping-cart,
email or account-management tools to execute a keyword plan.

Prefer one to three focused queries, scoped to the supplied country. Use native
region controls where supported; otherwise add the resolved country and prefer
relevant local sources. Do not claim a search was region-filtered when only query
wording supplied the location. Keep query strings as actually submitted.

Open relevant pages before reporting their contents. Put factual observations in
`observed_from_web`; record their domain in `source_domains` and concise query-linked
phrasing takeaways in `key_takeaways`. An individual query with no useful evidence
can have a takeaway explicitly saying so. Never count a failed load as an opened
page. Do not invent multiple sources or require the keyword expander's three-source
minimum in this skill.

If only a direct page was read, used=true and queries=[] is truthful: describe the
direct-page action and its domain in key_takeaways/notes, rather than inventing a
search. If the host requires citations, put them in existing evidence/notes strings
while preserving valid JSON. Keep keywords, tokens and source-domain strings free
of citation markup; domains contain no scheme, paths or query strings.

If research is necessary and no usable research action can be completed, return a
JSON error. If valid research succeeds but identity remains unresolved, use
needs_more_info or ambiguous. Do not confuse a legitimate search finding no match
with a tool/access failure. With no research attempt, all research arrays must be
empty and observed_from_web must also be empty.

When using an interactive browser, use only authorized pages and observe the
host/user's session-cleanup requirements. Never use email accounts for research.
Close task-created tabs when done without disturbing unrelated user sessions;
if an account was used, follow the user's sign-out rules. Prefer stateless search
and page-reading tools when they suffice. Do not require an interactive browser.

## Competitive-set definition

State the tight archetype as form + job + supported target. Add mechanism/material,
surface and use context only when justified. Use `archetype_explanation` for short
evidence-based scope reasons. Name nearby but wrong classes as exclusions, not as
claims about named competitors or demand. Do not add fields to represent evidence.

For ambiguity, keep two or three separately coherent possibilities. Put only true
shared constraints in the common CSD, using null for non-shared scalar values.
Map terms to interpretations explicitly in notes, for example:
`Interpretation 1: Tier A terms [...]; Tier B terms [...].`
Do not pool a glove's form with a vacuum's mechanism into a fictional hybrid.

## Keyword construction

Require form and job in every Tier A term; this is stricter than choosing any two
of form/job/target and satisfies the explicit form-plus-job rule. Include target
when known. Tier B retains a form-factor equivalent and the same functional scope.
Do not accept a category-only phrase merely because a form term appears elsewhere.

Use two to five `must_include_tokens` as semantic concepts for resolved scope.
Each Tier A/B query must respect those scope concepts through literal or equivalent
wording; do not require every literal token in every string. A glove/mitt synonym
is allowed only where equivalent for the specified product, not universally.
Do not require contradictory common concepts across ambiguous alternatives.

Aim for 6 to 12 A, 4 to 10 B and 2 to 8 C terms, allowing fewer when evidence or
natural wording is limited. C may be empty or have a single supported term. D has
0 to 6 broad anchors. Never pad tiers with punctuation, plural/spacing variations,
meaningless word-order permutations, unsupported brands or extra functions.
Keep every tier at or below its stated maximum.

Use known country-appropriate phrasing when supported; do not invent a translation,
regional popularity claim or output-language field. The country primarily scopes
research and terminology; no additional language parameter is part of this contract.
Retain direct brand/model strings faithfully rather than lowercasing them blindly.

For a novel-looking product, describe form + observed mechanism/feature + job rather
than inventing a category. Add two to four problem-phrasing variants only when the
problem is explicitly stated in the input/features, within A/B and their existing
limits. Do not invent an extra tier or assert novelty as a market fact.

Deduplicate derived keyword strings using normalized whitespace and case for
comparison, and remove clear near-duplicate variants without dropping meaningful
model distinctions. Put a term in its earliest appropriate tier rather than
repeating it in another. Build recommended queries from A, B, C in that order;
append D only when both A and B are empty. A few A/B terms are sufficient to exclude
D; do not use D just because the preferred tier counts were not reached.

## Confidence and unresolved content

Use confidence0_to10 for overall evidence quality and High/Medium/Low per
interpretation. No numeric threshold mapping or probability calibration is
prescribed. Lower confidence for unresolved identities, contradictory input and
conflicting sources. Do not overstate a plausible name-based inference.

Place clarification questions in `notes_for_downstream_agent`. Preserve useful
supported output while leaving genuinely unknown fields null/empty. Do not invent
a complete competitive set merely to obtain status=ok or fill tier counts.
