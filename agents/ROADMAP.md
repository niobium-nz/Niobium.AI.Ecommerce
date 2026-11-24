# Ecommerce agent roadmap

The approved architecture is in [Product Council, Sourcing and Human Approval](../docs/product-council-sourcing-approval.md).

Discovery -> Competitive Research -> Product Council (always selects one product, never approves) -> Supplier Sourcing (validate selected SKU and suppliers) -> Human Review (`approved_for_onboarding`) -> Product Onboarding.

## Product council

The Product Council reference synthesis and human decision contract are implemented in `agents/ecommerce-product-council`. The durable review boundary now uses append-only Google Drive manifests and decision events, with evidence verification, semantic retry handling, optimistic head checks and explicit human conflict resolution. An optional Drive v3 adapter supports ordinary folders and Shared Drives. See [Drive persistence and rollout](../docs/product-council-drive-persistence.md).

Host-backed specialist research, real supplier sourcing, authenticated Drive configuration, a hosted human-review interface, deployment acceptance and onboarding authorization integration remain pending. Hosts requiring strict ordering must serialize decision writes and onboarding; Drive is not a transactional database. No specialist has veto power. A nonempty valid research batch always yields exactly one relative winner, even when all products are risky. Onboarding requires a human approval after supplier findings are available.

## Manufacturer sourcing

The live manufacturer-sourcing integration remains pending. It must verify exact SKU, supplier photographs, materials, dimensions, certifications, quotes and landed cost. A supplier's alternative SKU must not inherit competitor claims or be silently substituted. Creating a product reference board remains a separate upstream step; the board is not a substitute for supplier verification.

## Website implementation

The website implementation agent remains downstream of onboarding and is not authorized by a council selection. Its production integration must preserve the current evidence-bound human approval gate.
