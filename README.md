# Niobium.AI.Ecommerce

## Workflow

Product Discovery -> Competitive Research -> **Product Council** (selects exactly one candidate, no veto or approval) -> **Supplier Sourcing** -> **Human Review** (`approved_for_onboarding`) -> Product Onboarding.

Read the [approved architecture and gate contract](docs/product-council-sourcing-approval.md) and [implementation roadmap](agents/ROADMAP.md). The council reference implementation is under [agents/ecommerce-product-council](agents/ecommerce-product-council/README.md). Live supplier integration and hosted human review are still pending.

## Model conventions

Read [model names and data contract ownership](docs/model-contracts.md).

## Product discovery agent

See [portable agent and installation instructions](agents/ecommerce-product-discoverer/README.md), [installation kit tooling](tools/ecommerce-product-discoverer/README.md), and [setup review](docs/discoverer-setup-lessons.md).

## All agent installation kits

The [shared builder](tools/agent-packaging/README.md) creates platform-neutral kits. Read the [portability policy](docs/agent-portability.md).
