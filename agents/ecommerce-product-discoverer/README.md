# Ecommerce Product Discoverer

A platform-neutral agent that discovers physical products from long-lived,
currently active Meta ads and preserves the evidence for competitive research.
It does not verify profit, approve products or invoke downstream agents.

## Install

Use the portable discoverer kit from a successful **Agent packages** run in
[GitHub Actions](https://github.com/niobium-nz/Niobium.AI.Ecommerce/actions/workflows/agent-packages.yml),
or build it from the repository root:

```sh
python3 -B tools/agent-packaging/build.py --agent ecommerce-product-discoverer
python3 -B tools/agent-packaging/test_packages.py --bundle dist/portable-agents/ecommerce-product-discoverer
```

The build downloads checksum-locked validation dependencies. The resulting kit
works offline on its supported Python targets; no package installation is needed
inside the agent host. Generated files go under `dist/`, outside source control.
Open the kit's `START-HERE.md`, then use the adapter for your host:

- [ChatGPT Workspace Agent](adapters/chatgpt-workspace-agent.md)
- [Claude Cowork or another task host](adapters/claude-cowork.md)

The [earlier discoverer exporter](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/tools/ecommerce-product-discoverer/README.md)
remains supported for existing installations. Its checker arguments and manifest
format differ; keep each kit's support files together. New deployments can use the
default-neutral kit above. See [deployment](references/deployment.md).

`AGENT.md` is the authoritative behavior. `agent.yaml` is descriptive metadata,
not a native vendor import format. A ZIP attached as a file is not necessarily
installed as persistent agent resources. Use the host's folder upload or explicit
resource mapping and verify the result in its actual execution/Preview surface.

The skill has one canonical source at repository-root
`skills/ecommerce-cluster-ads`. The kit contains one skill ZIP, separate from agent
files. Select it once through the host's skill mechanism. There is no nested skill
copy, hardcoded host mount path, or extra skill-version pin. The host configuration
selects the package; [logical schema bindings](references/skill-binding.md) resolve
against that selected package during each execution.

## Use

Configure the existing Drive folder ID and an IANA timezone in the deployment
configuration. The default example deliberately has a null folder ID and cannot
be used as active configuration. Then invoke with exactly three strings:

```json
{"source_country":"USA","target_country":"NZL","keyword":"portable blender"}
```

Each invocation receives a fresh UUID, makes at most three sequential search
attempts, independently checks activity and ad age, clusters the whole qualifying
batch and saves a raw JSON array as `discovery/{job_id}.json`. It returns the
structured result only after checking the saved file. Identical inputs may be
repeated. See [tool bindings](references/tool-bindings.md) for required actions.

Drive connectors sometimes omit trash/add-permission metadata. Unknown fields
produce a warning; explicit denial still stops the run. Destination identity,
complete collision lookup and full upload verification remain required. A real
write can still fail after paid retrieval when advance permission is unknown;
that failure must be visible and must never be reported as a saved artifact.

## Validate and update

Follow [validation stages](references/validation.md). Schema checks and source
unit tests do not prove hosted orchestration or live connector behavior. A missing
host mock harness is recorded as untested, not an installation failure. Setup
never starts paid searches or Drive writes by itself.

Build one fresh kit when updating. Replace the complete agent folder, validation
support folder and instructions together. Keep exactly one active deployment
configuration with your existing settings. Update the skill attachment only when
intentionally changing it, then run compatibility checks against the selected
attachment. Do not merge old and new folders or leave an older ZIP as a competing
instruction source. Source updates do not automatically update hosted agents.
See the host adapter for migration from the earlier numbered ZIP bundle.

## Versions and boundaries

Package version is **2.1.1**; the saved handoff remains **2.0.0**. This patch makes
the optional-metadata rule explicit for live jobs and documents updates from older
installed instructions that still rejected missing fields. Input/configuration,
search policy, original evidence, cluster shape and lifecycle values are unchanged:

`discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding`

The agent stops after saving discoveries. The schema's external cluster reference
now uses a logical URI instead of a nested file path. Schema consumers must bind
that URI; no saved JSON migration is needed. [Contracts](references/contracts.md)
describe failures and data ownership.

The country table remains the dated 249-country snapshot with Git blob
`e30d148d75d75b82646f48fd1243aa84d5bfdb49`; it is not a live ISO registry.
[Setup lessons and acceptance criteria](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/docs/discoverer-setup-lessons.md)
record why packaging and validation changed.
