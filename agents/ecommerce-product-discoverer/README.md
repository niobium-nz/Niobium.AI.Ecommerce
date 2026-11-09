# Ecommerce Product Discoverer

A platform-neutral agent definition for discovering product signals from
competitors' long-lived, currently active Meta ads. The package preserves evidence
for competitive research; it neither verifies profits nor produces candidates,
approves products or triggers onboarding.

## Business lifecycle

`signal -> competitive research -> candidate -> product council approval -> listing -> product onboarding`

This agent outputs signals only. A downstream competitive research agent turns
research results into candidates for product council review. Only approved
candidates become listings for product onboarding. Signals are not submitted
directly to council. This package persists the signal handoff and returns the
job result; it does not invoke any downstream agent.

## Package

`AGENT.md` is the authoritative entrypoint. `agent.yaml` is descriptive metadata,
not a vendor import schema. `schemas/` contains input/configuration/handoff/result
contracts; `references/` contains tool bindings and country data; `examples/`
contains synthetic fixtures; `tests/` contains offline checks and host acceptance
cases. `adapters/` provides separate ChatGPT Workspace Agent and Claude Cowork
setup notes.

The sole runtime skill is bundled at `skills/ecommerce-cluster-ads/`. Its entire
unchanged tree was moved from the repository-root `skills/ecommerce-cluster-ads/`,
including schemas, examples, suffix data, UI metadata and licenses. Other root
skills are not dependencies and remain untouched. Approved search/date policies
are implemented in this agent, not delegated to the competition-analysis skill.
The country lookup is bundled data, not another skill dependency.

## Invocation and configuration

```json
{"source_country":"USA","target_country":"NZL","keyword":"portable blender"}
```

Provision an existing Drive folder named exactly `signal`. Configure its actual
ID and the host timezone separately using `config.example.json`. The example
uses the owner's Pacific/Auckland timezone; null folder ID deliberately fails
validation until configured. Public invocation remains exactly three fields.

Each invocation generates a fresh UUID. Identical searches may be repeated.
The agent spends at most three sequential search attempts, independently verifies
actual active status and start dates, clusters the whole qualifying batch and
saves a raw JSON array as `signal/{job_id}.json`. Entire invalid clusters and
services are excluded; valid Low-confidence products remain as signals for
competitive research.

## Handoff contract version 2.0.0

Saved records require `lifecycle_stage: "signal"` and
`research_status: "pending_competitive_research"`; profitability stays unverified.
These replace the former `review_status: "pending_product_council"` contract.
The version changes because the next workflow step and required fields change.
Consumers must not treat a successful discovery job as a council-ready candidate.
The original inputs, nested product cluster and matching ads remain unchanged.
No existing Drive artifact is rewritten or migrated by this change.

## Portability

This is portable source, not a promise of one-click import into both vendors.
Bind instructions, files, the bundled skill and connectors using the relevant
adapter. Required semantics cannot change with tool names. No credentials,
account IDs, real folder ID, schedule or published agent are included. Read-only
Drive access is insufficient. The Python checks are optional development tooling,
not a runtime-language requirement or a production runner.

## Offline validation

From this directory with Python 3.11+ and jsonschema installed:

```sh
PYTHONDONTWRITEBYTECODE=1 python -m unittest discover -s tests -v
```

Tests cover schemas, fixtures, calendar boundaries, country normalization,
lossless originals, strict projection, eligibility, traceability, lifecycle
boundaries and exact skill preservation. They do NOT test live connectors, hosted
permissions, model clustering quality or transactional Drive writes. Run tests/host-acceptance.md
in each target host before production, with mocked tools and no paid requests.

## Provenance

Reviewed against repository commit
`40f6b50e9d212a8dc78d05275f4a1c6309c1d7e6` on 2026-11-09.
Moved skill Git tree: `bb1edb84055f7b2a83c5de3590301900b4963d21`.
Country lookup Git blob: `e30d148d75d75b82646f48fd1243aa84d5bfdb49`.
This lookup is the repository's dated pycountry snapshot, not a live ISO registry.
Update it deliberately if assigned codes change. Integration-source checks are
not evidence of live execution or deployment.
