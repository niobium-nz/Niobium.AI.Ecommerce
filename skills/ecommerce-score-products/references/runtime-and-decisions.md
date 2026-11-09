# Approved decisions and runtime boundaries

## Approved adaptations

Use Ecommerce Score Products / `ecommerce-score-products`. Require `target_country`
as alpha-2/alpha-3 input and return normalized alpha-2. Replace the former
country-specific demand component with `target_market_demand_evidence`; use
`Ecommerce-PaidSocial-v1` as score version. Preserve all seven component weights,
integer bands, hard ceilings and priority thresholds from the provided workflow.

Replace fixed country names, marketplace domains, regulator references, price
context and ad suitability with verified target-market equivalents. Keep the
logical Q1-Q8 research order and conditional branded check. Do not create a new
market, language or country input through inference.

Use ordinary web research and supplied ads/Scout reports; prohibit fresh paid Ad
Library calls. Consume the finalized Scout contract, including partial and error-only
results. Treat foreign-country reports as transferability context, not local proof.
Limit this Scorer to physical ecommerce products. Use error-only on invalid input,
unidentifiable/nonphysical products or no usable research capability. Use full scores
with optional `error` on partial operational failure; keep evidence uncertainty
separate from execution errors.

## Runtime dependencies

Require an agent that can read this skill and its JSON/Markdown resources and use
ordinary web search/browsing. Do not require scripts, a compiler, a local shell,
a specific browser automation package, a paid ad connector or the original model
files. Do not install, authenticate or call another service merely to score a product.

The bundled schemas define language-agnostic wire contracts. A JSON Schema engine
is useful for external application validation but is not required for prompt-driven
execution. Default annotations document behavior; a generic schema validator does
not necessarily populate omitted arrays.

Distinguish repeatable rules from guarantees: point arithmetic and cap precedence
are specified; real evidence interpretation, localization and scoring still involve
judgment. Instruction-following is not deterministic software validation. The fixed
weights permit consistent methodology, not guaranteed cross-country statistical
calibration or identical results across model runs.

## Packaging and maintenance

Bundle only the authored instructions, JSON Schemas, data references, UI metadata and
synthetic examples. Exclude source-language models, original uploaded specifications,
executables and build/test scripts. Keep build-time validators outside the skill.
The optional plugin wrapper carries the same skill files without adding tools,
credentials, authentication, external services or verified host availability.

## Source-to-contract decisions

Preserve `product`, required `ads` (including []), optional `competition_signals`,
all nested ad/media fields, and all product fields. Make product/service flags
explicit for safe physical-product validation. Permit missing source ad IDs here;
the Transformer's stricter batch requirements are not a Scorer requirement.

Preserve output field names in lower snake case except the approved demand rename
and added target country. Keep the supplied priority enum values uppercase. Prefer
workflow integer bands to permissive source numeric types. Keep reason/risk limits
at three and confidence at integer 0-100. Keep sources as actual source names/domains.

The workflow does not define numeric seller thresholds or a confidence formula.
Do not silently introduce them. Cap-prefix labels are only an implementation convention
for concise reporting and checks, not new scoring conditions.
