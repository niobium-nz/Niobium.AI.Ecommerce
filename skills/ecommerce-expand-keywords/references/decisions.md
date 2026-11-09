# Approved workflow requirements

Apply this self-contained specification and its JSON Schemas. Do not require earlier attachments, source-language models, or an external application to interpret the contract.

| Topic | Approved rule |
| --- | --- |
| Skill identity | Use display name `Ecommerce Expand Keywords` and identifier `ecommerce-expand-keywords`. |
| Project naming | Begin display names with `Ecommerce ` and identifiers with `ecommerce-` for skills authored in this project. This records a naming rule, not a claim that project-wide settings or memory were changed. |
| Target | Support ChatGPT web, ChatGPT iOS, and Codex when the host can load the skill resources and perform country-scoped research. |
| Contract format | Use JSON Schema Draft 2020-12 and lower_snake_case JSON names. Require no programming-language contract or serializer. |
| Package contents | Keep skill instructions, current reference guides, JSON Schemas/data/examples, metadata, and optional validation helpers. Do not retain uploaded source documents or application source files. |
| Country | Accept ISO 3166-1 alpha-2 and alpha-3 letter codes only; do not accept numeric-3. |
| Constraints | Treat objects as alternative segments; combine keywords into one result and ask when genuine conflicts require clarification. |
| Overrides | Do not support count, brand, or adjacent-category exceptions because the input contract has no fields for them. Retain 25 to 60 keywords and category/brand restrictions. |
| Research | Stop with an error when research cannot be performed; do not generate from memory alone. |
| Clarification | Ask up to three targeted plain-text questions for semantic ambiguity or conflicts, without provisional keywords. |
| Invalid input | Return actual errors through the JSON `error` field; put input-correction guidance in that string. |
| Success | Return exactly `category_focus` and `optimized_keywords`; omit `error`. |
| Failure | Return a nonblank `error` string, `category_focus` as `""`, and `optimized_keywords` as `[]`. |
| Script execution | Follow the complete instruction-based workflow without code. Use Python helpers only as optional validation and maintainer testing tools. |

## Keep interpretations explicit

Distinguish semantic clarification from an error. Vague category meaning, substantial seed/category disagreement, and unresolved segment conflicts trigger the authorized plain-text questions. Malformed JSON, missing fields, invalid values/types, unavailable research, insufficient evidence, and inability to produce a compliant list trigger JSON errors.

Accept letter codes without case sensitivity and resolve them internally to canonical uppercase alpha-2. This does not add country names, numeric codes, aliases, or a new input property.

Interpret multiple independent sources as at least two; seek at least three whenever possible. Do not make three an unconditional minimum, and do not count multiple pages from one operator as independent sources.

Do not invent enumerations for constraint strings, weights, fixed per-segment counts, price bands, locale preferences for uncertain markets, or error codes. Ask targeted clarification when a material interpretation cannot be established from the input and research.
