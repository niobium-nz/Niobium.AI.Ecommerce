# Drive lookup evidence

A live discoverer run stopped before search because an exact-name Drive lookup
returned no matches without proving its result set complete. A read-only check
reproduced the connector response `{"results": []}`. Its all-type legacy search
omits pagination/completeness metadata; the paginated typed search does not cover
all file types. Requiring a positive completion flag made this binding unusable.

The revised platform-neutral contract distinguishes three states:

| Evidence | Action |
| --- | --- |
| Successful exact-scope search with documented completion | Use the observed matches |
| Successful exact-scope search with completeness unreported | Warn `drive_lookup_completeness_unavailable`; use observed matches without claiming exhaustive absence or global uniqueness |
| Failed/malformed/wrong-scope search, known truncation or unhandled token | Stop the affected operation |

Follow every exposed continuation token, including after empty pages. Never invent
a token, narrow the file types to gain pagination, use viewed-only results, hide an
error as an empty array or convert missing evidence to a true/false proof.

Fresh UUID outputs use create-only operations, stop for an observed collision and
verify the returned ID, exact destination/type/name and complete bytes. An empty
post-create search warns `drive_created_file_not_listed` after direct verification;
an observed different ID still conflicts. This verifies a particular file, not
global filename uniqueness. An uncertain write is never repeated. Without a returned
ID, reconciliation needs exactly one observed match and complete verification.

Existing-source selection and folder reuse still need one observed valid match.
Known competing matches fail. Looper source version/byte checks, researcher registry
completeness and serialization, onboarding Firecrawl/board checks and actual access
denials are unaffected. These are different business requirements from optional
search-completeness metadata. The schemas and saved business payloads are unchanged.

Google's [files.list documentation](https://developers.google.com/workspace/drive/api/reference/rest/v3/files/list)
defines provider pagination and incomplete-search signals. A wrapper need not expose
all those fields; observing an omitted field is not a provider guarantee. The
best-effort policy above is this application's explicit tradeoff. The optional
Python reference adapter normalizes established/unreported/incomplete evidence to
True/None/False only after successful response parsing and pagination; these are
internal test values, not fields a connector must return.

Regression checks cover the observed empty response, positive collisions, explicit
incompleteness, uncertain completeness with verified bytes, competing file IDs and
missing/ambiguous source selection. They do not establish live model orchestration
or perform uploads. Install complete updated resources and Instructions together;
changing GitHub does not change a hosted deployment.
