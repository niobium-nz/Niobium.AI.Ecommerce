# Real browser and bounded navigation

## Contents

- Browser capability gate
- Overlay protocol
- Entry redirect and scope
- URL keys and queue
- Eligibility and priorities
- Counters and stop conditions
- Policy PDFs and cleanup

## Browser capability gate

Require a real, headed, user-facing browser under the authorized computer-use
controller. Inspect actual live rendering and interact through normal visible UI
controls. A headless browser with JavaScript, an HTML fetcher, browserless scraping,
a screenshot service or search results is not an equivalent fallback. A tool label
alone does not prove a real browser session. Use the host's documented capability;
if unavailable or uncertain, return an error before profiling.

Do not run Python, shell, arbitrary JavaScript, Playwright/Selenium scripts or
scraping code to replace browser control. Host-provided read-only observations of
the current real page may provide DOM links, location and redirect evidence, but
must not become a separate non-browser fetch or a way to extract hidden content.
The package does not supply or authenticate the browser.

Check which redirect controls actually exist. Do not claim an auto-following
browser can prevent out-of-domain redirects. Missing redirect capabilities restrict
the affected navigation, not a reason to fabricate evidence or use a scraper.

## Overlay protocol

On each approved page, inspect for a visible blocking ad popup, cookie banner,
consent modal or dialog that keeps the content unrendered/inaccessible. Handling
is mandatory when such an overlay exists, not merely optional cleanup afterward.
Use an observed Close, Dismiss, X, Reject non-essential, Continue, Agree or equivalent
control whose actual visible purpose is accessing the page. Prefer the least
permissive effective choice. Necessary ordinary cookie/consent agreement is
allowed when closing/rejecting does not unlock the content. Do not agree to a
purchase, subscription, account contract or unrelated sensitive use of data.

Treat ordinary cookie/consent buttons as this specific access-overlay exception,
not permission for account, commercial or other form submissions. After each
distinct permitted action, let the same page settle and inspect it again. Do not read an empty pre-consent shell as the final product content. Do not
repeat the same ineffective action or reload just to retry it. Handle a genuinely
new overlay state only when its next safe action is visible; stop recurring or
unresolvable gates with a blocker and operational error. Do not invent a numeric
interaction budget not present in the approved workflow; enforce finite distinct
states and no repeated ineffective actions.

Do not complete CAPTCHA/challenge-bypass procedures, fake a human identity, alter
fingerprints, rotate proxies or exploit private endpoints. If access stays denied,
record that. Ordinary overlay consent is not permission to overcome access controls.

Outside overlay handling, allow only scrolling, eligible page navigation and
cleanup. Do not open accordions/tabs, choose a variant, play media, submit a quiz,
use a site search, click purchase CTAs or enter forms. If the page needs these
other interactions to reveal a claim, keep that field empty with the exact
blocker `Claims/offer details not accessible without interaction`.

## Entry redirect and scope

Determine origin eTLD+1 using the domain guide. The entry may load through redirects.
Capture final URL, chain length where exposed, and any original/redirect evidence
actually visible through the same real-browser controller. Do not obtain missing
history through an HTTP client or a second browser mode.

For a same-domain final URL, use the origin anchor. For a different domain, accept
only the one initial automatic entry exception, verified at most three hops, and
an observed overlap in title/brand, product/hero offer or header/footer brand/logo
text. Do not infer continuity merely from similar domain strings, a familiar logo
at the destination or an encoded destination parameter. If the original provides
no comparable evidence, the similarity condition is unverified, not waived.

If hop count or continuity cannot be established, do not accept the new scope.
Record the known entry final URL, discard unapproved destination extraction, add
an error and a precise blocker, and stop the affected exploration. If the chain
exceeds three hops, reject the exception. Use `Entry redirect to different domain
not accepted by similarity heuristics` when observed comparison fails, and a
specific unverified-chain/continuity blocker when evidence is missing.

For an accepted HTTP destination, add `Entry redirect landed on non-HTTPS URL`.
Do not downgrade HTTPS or try guessed scheme variants to manufacture success.
Subdomains are in scope only under the same verified registrable boundary. Private
hosting suffixes separate unrelated tenants. Redirect acceptance is not proof of
business ownership or endorsement.

Lock the anchor. Before any later main-document navigation, require prevention of
off-scope HTTP and script-triggered redirects. If absent, leave the page as it is,
stop subsequent navigation, preserve entry evidence and return a partial error.
Do not click to test whether the guard works. If a guard blocks a cross-domain
redirect, count the attempted page slot, add `Post-entry redirect off scope_domain
detected`, and do not extract or enqueue the destination. Other already-discovered
eligible links may continue if the guard still works. If an unguarded off-scope
arrival happened unexpectedly, disclose it, stop navigation and do not claim it
was blocked. Overlay-triggered or click-triggered changes do not reopen the entry
exception. Normal third-party subresources rendered by the page are not permission
to navigate, scrape or inspect external sites.

## URL keys and queue

Keep the original navigation URL separate from its deduplication key. Normalize
scheme/hostname case, default ports, fragments and trailing slash. Remove tracking
keys `utm_*`, `gclid`, `fbclid`, `msclkid`, `ttclid`, `mc_cid`, `mc_eid`, `igshid`,
`vero_*`, `ref`, `wickedsource`, `wickedid`, `affiliate`, `aff`, `cid`, except a key
explicitly evidenced as essential to product selection. Keep only query parameters
evidenced to materially change product content; order retained key/value pairs
consistently. Do not lowercase URL paths or values. Do not remove meaningful
variant selectors merely because the same URL also contains tracking parameters.

A rel=canonical may supply a preferred key only when observed on the real page,
valid, allowed and still within the accepted scope. It is a deduplication hint,
not a navigation instruction or permission to change scope. When it would merge
explicitly different product variants, retain the evidenced selector distinction
and record the canonical conflict. Keep request, final and accepted canonical keys
as aliases to the same page slot. A rel=canonical pointing to a prior page is a
duplicate hint, not by itself a navigation cycle.

Maintain `visited_keys`, `queued_keys`, aliases, page slots, entry attempts,
per-page attempts, depths, the last three actually visited keys and the progress
streak. On enqueue reject visited/queued aliases. On dequeue first remove its
queued marker; then recheck visited aliases and eligibility. Mark attempts before
navigation, including failures. Discover eligible links once per inspected page,
after overlay resolution and ordinary scrolling, not once per scroll position.

Never revisit solely to improve a score or fill gaps. One immediate retry of a
genuinely failed entry load is the only visited-key exception. Do not retry
non-entry pages. Stop terminal safety/access-denial issues rather than repeatedly
trying to defeat them. A blocking overlay on a successfully loaded entry is not
itself permission for a reload.

## Eligibility and priorities

Allow only relevant, same-scope PDPs, shipping/delivery/returns/refund/privacy/terms,
contact/about/FAQ/help, review/testimonial/UGC/press, ingredients/materials/sizing/
care and relevant subscription-plan information pages. A quiz/collection entry
may lead to an unambiguous primary PDP; do not answer a quiz or pick a random item.

Apply the denylist before priority: no external destinations, cart/checkout/
checkouts/account/login/register/order paths, mailto/tel navigation, search/filter/
infinite pagination, or non-policy file downloads. Deny query keys `page`, `p`,
`sort`, `filter`, `q`; deny `/page/` paths except visibly finite FAQ content. Apply
path screening to the actual resolved path, not a misleading link label. Capture
external social, email and telephone links only as page-observed trust data.

Rank eligible links by expected ability to complete missing fields:
1. Price/primary offer, including the relevant PDP.
2. Guarantees and returns.
3. Shipping time and cost.
4. Reviews and proof.
5. Materials/ingredients.
6. Other allowed schema-completion pages.

Use canonical-key lexicographic order for equal ranks. Do not search the web,
guess policy paths, fetch unrelated links or use sitemap/crawl tools. Within scope
does not make every link eligible.

## Counters and stop conditions

Count at most ten unique deliberate page-attempt slots including failures. Each
slot has one page attempt except entry, which may have two. This is at most eleven
deliberate page loads, not a promise that browser network requests are eleven or
fewer. Redirect hops and subresources do not replenish or duplicate page slots.
Aliases share a slot; an attempted source that redirects to a prior page still
consumes its allocated slot before cycle termination. Depth follows the shortest
observed eligible link path from entry and is at most two clicks. Redirects and
overlay dismissal do not increase click depth or reset it.

After finishing a page's permitted load attempt(s) and extraction, update progress.
Count only null-to-known name/price/policy fields and empty-to-nonempty trust or
how-it-wins lists. A new image, a new key claim or another item in an already
populated list does not reset this specifically defined streak. Stop after three
consecutive completed page slots without tracked progress, including failed pages.

Stop after finishing the tenth slot, on no eligible queue entries, on exhaustion of
allowed depth, or on an actual redirect/navigation returning to any of the last
three visited keys. Skip ordinary duplicate links without treating them as cycles.
If an earlier visited key is reached, stop expansion of that duplicate regardless
of whether it lies in the last-three cycle window. Never fetch again to test it.
Record the actual stop condition as a blocker. A normal stop is not an operational
error unless a separate operational failure occurred.

## Policy PDFs and cleanup

Only open a clearly relevant, same-scope policy PDF if the same real browser can
render it without violating navigation restrictions. It occupies one page slot.
Inspect just the necessary policy content; use the browser's visible PDF rendering
or supported screenshot inspection. Do not follow PDF links or crawl the document.
Do not fall back to a non-browser PDF downloader. Otherwise capture only an observed
policy URL and record that its content could not be inspected.

Prefer signed-out public browsing. Do not initiate account sessions or access
webmail. Track and close task-created tabs/windows/sessions after success, failure
or cancellation unless the user explicitly requests that they remain open. Verify
closure; leave unrelated tabs/accounts alone. Follow standing sign-out rules for
an account session actually used and do not claim cookie deletion or logout from
mere tab closure. Include any cleanup failure in blockers and error.
