# Approved behavior

Apply these confirmed choices; do not reintroduce superseded requirements.

| Topic | Confirmed behavior |
| --- | --- |
| Name | Ecommerce Cluster Ads / `ecommerce-cluster-ads` |
| Packaging | Language-neutral schemas; no retained original specification or source-model files |
| Runtime | Operate without script execution; use supplied fields only; no browsing or country input |
| Input | A list of supplied ad records using lower snake case fields |
| Success | A top-level list of normalized competing-product/service objects |
| Error | An object containing a nonblank `error` string, not a list item or a partial success |
| Cluster IDs | Readable normalized context/product identifiers with source-ad discrimination where needed; no hashes |
| Unknown names | Actual JSON null, never the string `"null"` |
| Confidence | `High`, `Medium`, `Low` |
| Multi-product ads | Each explicitly identifiable promoted product receives its own cluster; one ad ID may be referenced by several clusters |
| Invalid batch | Fail the whole batch for missing/blank ad IDs or structural invalidity |
| Unclear content | Preserve it as conservative smaller Low-confidence clusters rather than guessing or dropping it |

Keep the supplied-field clustering workflow, not the unrelated single-product
keyword-planning workflow that was superseded. Do not inherit the keyword
expander's mandatory web research, country codes, keyword counts, or clarification
exception. Do not require any source-language class or serialization library.

Treat schema closure, evidence precedence, ordering, delimiter escaping, and the
frozen suffix lookup as documented implementation mechanics of the confirmed
workflow. Do not present these mechanics as additional product evidence.
