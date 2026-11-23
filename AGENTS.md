# Working in Niobium.AI.Ecommerce

Read [model names and data contract ownership](docs/model-contracts.md) before changing models, examples or serialized formats. Use business names with EntityKey attributes on Table models. Models own private representations; domains own rules and transitions; flows coordinate; adapters map and persist. Preserve existing wire/storage values unless separately migrated.

The approved lifecycle is documented in [Product Council, Sourcing and Human Approval](docs/product-council-sourcing-approval.md). Council selection is never onboarding approval; no specialist vetoes; sourcing precedes an explicit evidence-bound human `approved_for_onboarding` decision. Never silently bypass the human gate.

Default delivery uses feature branches, genuine independent review and checks; do not self-approve or fabricate status. Direct commits to main are permitted only on explicit owner instruction and where branch protections allow. Do not mutate production data.
