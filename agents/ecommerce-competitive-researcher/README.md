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

In normal mode the agent atomically claims all input ad IDs before research.
An exact processed ID rejects the whole product, across all jobs and markets.
Concurrent runs can proceed when their ad IDs are disjoint. An uncertain write
retains only the affected claims for reconciliation.

The configured `ecommerce/processed-ad-ids` folder stores one JSON file per ad
and a native Sheets claim index. The index enforces conditional creation and
atomic multi-ID batches. Individual processed files are written only after the
candidate is verified. The existing result field `registry_updated` reports
verified per-ID records and finalized claims. See
[duplicate records and migration](references/duplicate-records.md).

Explicit `override_duplicate_check: true` bypasses the duplicate store. It still
writes the verified candidate to the same production path and returns
`published_registry_skipped`. Omission and false use atomic claims.

## Deployment and tests

See `adapters/chatgpt-workspace-agent.md` and `adapters/claude-cowork.md`. Bind actual
capabilities, permissions and approval gates in the target host; there is no
claim of a shared one-click import ZIP or a completed hosted deployment.

Build and test the actual install artifact using the shared tooling below. The
source tests cover mappings, gates, duplicate handling and mocked ordering/recovery,
not LLM quality or real provider behavior. Host mock acceptance is recommended
where supported; lack of injection is an evidence gap, while actual unavailable
claim/verification capability remains a live-execution prerequisite.

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
Version 2.3.0 uses per-ID records and an atomic claim index. Refresh the private
deployment bindings after a verified migration. Existing input, artifact and
result contracts are preserved.
