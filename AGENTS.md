# Working in Niobium.AI.Ecommerce

Read [model names and data contract ownership](docs/model-contracts.md) before
adding or changing models, examples, identity derivation or serialized formats.
Use business names with EntityKey attributes on Table models. Models own their
private representations; domains own business rules and transitions; flows
coordinate and adapters map/persist. Preserve existing wire and storage values.
Legacy public contract renames require the documented separate rollout.

Use feature branches and the owner's standing delivery process. Inspect workflow
triggers before submission, verify available checks, native review and a separate
genuine approval, and monitor resulting publication/deployment. Do not push main,
self-approve, fabricate status, bypass protections or mutate production data.
