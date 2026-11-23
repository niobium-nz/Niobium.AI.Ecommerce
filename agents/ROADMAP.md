# Ecommerce agent roadmap

The approved architecture is in [Product Council, Sourcing and Human Approval](../docs/product-council-sourcing-approval.md).

Discovery -> Competitive Research -> Product Council (always selects one product, never approves) -> Supplier Sourcing (validate selected SKU and suppliers) -> Human Review (`approved_for_onboarding`) -> Product Onboarding.

The Product Council reference synthesis and human decision contract are implemented in `agents/ecommerce-product-council`. Host-backed specialist research, real supplier sourcing, a durable human-review queue, and onboarding authorization integration are pending. No specialist has veto power. A nonempty valid research batch always yields exactly one relative winner, even when all products are risky. Onboarding requires a human approval after supplier findings are available.

Manufacturer sourcing must verify exact SKU, imagery, materials, dimensions, certifications, quotes and landed cost. A supplier's alternative SKU must not inherit competitor claims or be silently substituted. Reference-board creation remains a separate upstream step. The website implementation agent remains downstream of onboarding and is not authorized by a council selection.
