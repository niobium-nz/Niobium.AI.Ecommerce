# Ecommerce Competitive Researcher

Platform-neutral source for researching one discovery into a candidate for product
council. The agent is an orchestrator; the existing three skills retain the
normalization, ad-search analysis and scoring work.

`discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding`

## Package and invocation

Use `AGENT.md` as the entrypoint. `agent.yaml` is descriptive metadata, not a native
vendor import format. The caller supplies one `{job, product, ads}` object; a future
looper reads the upstream discovery array and selects one entry. This agent does
not read/process the entire upstream file. See `examples/fixtures.json` and
`references/contracts.md` for the exact mapping and JSON pointers.

Each of the three skills has one canonical source in repository-root `skills/`:
ecommerce-normalize-products, ecommerce-analyze-competition and ecommerce-score-products.
They are selected independently through host configuration, with logical schemas
in `skill-bindings.json`. Deployment emits THREE separate skill ZIPs, never a
combined installable skill ZIP or a duplicate nested tree. The skill contents and
saved candidate contract are unchanged. See [portable deployment](references/deployment.md).

The approved contract corrections are confidence0_to10, top-level competition
error, and the scorer's required target_country. Confidence 6 passes; all failed
competition queries stop the run. Valid empty reports remain valid. A full score
with a partial research error can be published; an error-only score cannot.
Normalization remains internal and original input evidence stays unchanged.

## Persistence and duplicate history

Output is `discovery/{job_id}/{signal_id}.json`, a single object containing the
requested correlation fields, originals, valid reports, full score and errors.
The signal_id name is retained as requested; the business result is a candidate,
not council approval or an onboarded listing.

The OneDrive registry checks exact ad IDs globally, without product/job/market
scoping. Any match rejects the entire product, including a later product sharing
an ad or a later target market. After verified publication, append only the input
ad IDs. No IDs are added for rejected, failed or unpublished work.

The agent definition pins the OneDrive registry at
`Ecommerce/processed-ad-ids.json` in the designated drive. Provision it once as
a JSON string array starting with `[]`. Keep its real drive and item IDs in
private deployment configuration and reuse those same IDs across reinstalls and
updates. A different drive's file at the same path is a different registry.
Explicitly configured legacy registries may retain their existing JSON-array or
UTF-8 newline format. The null resource IDs prevent an unconfigured run.
See [registry provisioning and reuse](references/storage.md).

Product jobs sharing a registry must be serialized by the host from duplicate
check through publication and append; query subagents within one job may run in
parallel. This prevents duplicate processing races that versioned file writes
alone cannot prevent. Host validation must establish that guarantee, not merely
read a configuration flag. Cross-cloud writes are not atomic. If a file is saved
but the append is incomplete, the explicit published_registry_pending outcome
keeps its identifiers and blocks automatic reruns until reconciliation.

## Deployment and tests

See `adapters/chatgpt-workspace-agent.md` and `adapters/claude-cowork.md`. Bind actual
capabilities, permissions and approval gates in the target host; there is no
claim of a shared one-click import ZIP or a completed hosted deployment.

Build and test the actual install artifact using the shared tooling below. The
source tests cover mappings, gates, duplicate handling and mocked ordering/recovery,
not LLM quality or real provider behavior. Host mock acceptance is recommended
where supported; lack of injection is an evidence gap, while actual unavailable
registry serialization/recovery remains a live-execution blocker.

Sources for host/storage setup are linked in `references/storage.md` and the
adapters. Baseline: 51cdf133d614d42f189bc6698c17c3593e029dc2, checked 2026-11-11.

## Build the installation kit

From the repository root:

```sh
python3 -B tools/agent-packaging/build.py --agent ecommerce-competitive-researcher
python3 -B tools/agent-packaging/test_packages.py --bundle dist/portable-agents/ecommerce-competitive-researcher
```

Open the kit's START-HERE.md. The default kit and instructions are platform-neutral;
use the relevant host adapter only for that deployment. The optional Python checker
has documented runtime targets, and equivalent validators are supported elsewhere.
For another build use a fresh --output directory so edited settings are preserved.
Package version 1.1.2 pins the new registry's path and format for consistent
deployment. Existing wire formats and required identity and content checks are
unchanged.
