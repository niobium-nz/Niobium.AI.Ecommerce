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

The complete ecommerce-normalize-products, ecommerce-analyze-competition and
ecommerce-score-products trees are MOVED from root skills/ into this agent's
skills/ directory without content changes. No dependency on root skill paths or
an installed version with a matching name remains. The discoverer and the five
unrelated root skills are unchanged. `dependencies.json` pins the moved trees.

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

Configure the real discovery folder ID and OneDrive drive/item IDs before use.
The registry format is explicit deployment configuration: a JSON array of string
IDs or UTF-8 newline-separated IDs. No supplied existing file format is assumed
or auto-detected. The example selects json_string_array; use [] for an initially
empty JSON registry. The null resource IDs deliberately prevent an unconfigured
run. This source package does not create or modify live cloud resources.

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

With Python 3.11+ and jsonschema installed, run from this directory:

```sh
PYTHONDONTWRITEBYTECODE=1 python -m unittest discover -s tests -v
```

These network-free tests check contracts, mappings, gates and mocked ordering/failure
paths. They are not LLM-quality or live connector tests. The optional reference
model in tests/ is development-only, not a production runner or runtime language
requirement. Run `tests/host-acceptance.md` in each deployment with mocked tools
before an explicitly authorized live test. No paid searches are needed for tests.

Sources for host/storage setup are linked in `references/storage.md` and the
adapters. Baseline: 51cdf133d614d42f189bc6698c17c3593e029dc2, checked 2026-11-11.
