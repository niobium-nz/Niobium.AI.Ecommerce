# Model names and data contract ownership

The owner's rule of 14 November 2026 applies to all Niobium projects and their
examples. Read the [framework guide](https://github.com/niobium-nz/Niobium/blob/main/docs/model-contracts.md)
for detailed mapping examples, the responsibility table and the legacy inventory.

This repository contains ecommerce agent and skill guidance. No Table model was
found in the inspected source at `9e9503f`.

When introducing Table models, name properties for the actual business values,
then apply `[EntityKey(EntityKeyKind.PartitionKey)]` and
`[EntityKey(EntityKeyKind.RowKey)]`. The framework repository handles physical
column names. Composite values need a name that describes the whole value.
Infrastructure types and protocol fields are distinct from business Table models.

The owning data contract privately defines IDs, canonical fingerprints, key/path
formats and serialized shape. Expose small meaningful factory/lookup/comparison
operations, rather than public implementation helpers that callers must repeat.
Domains own business rules, authorization and transitions; flows coordinate;
adapters perform backend mapping and I/O.

Agent instructions and generated examples must use the actual vendor request and
response schemas. Do not reinterpret a legacy API field or rewrite external IDs
while applying a naming convention to a new server-side model.

The owner selected Notification's refactor now and a separate rollout for legacy
public contract renames. Preserve existing keys, ordering, wire values and
suppression identities until that rollout is explicitly scoped. A private helper
localizes future edits but does not make incompatible format changes safe.
