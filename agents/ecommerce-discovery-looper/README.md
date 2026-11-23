# Ecommerce Discovery Looper

A platform-neutral coordinator between the existing discoverer and competitive
researcher. It reads one discovery array and invokes the researcher separately
for each product, without reproducing normalization, competition or scoring logic.

```text
discovery -> competitive research -> candidate -> product council approval -> listing -> product onboarding
```

## Input and routing

```json
{"job_id":"81c8fdf2-5630-4d97-8f5f-ce5582872655"}
```

The looper looks up `discovery/{job_id}.json` by exact name under the configured
folder ID. It passes only `{job, product, ads}` for one entry to the researcher.
The source file stays unchanged. The researcher still owns candidate publication
at `discovery/{job_id}/{signal_id}.json` and the subsequent verification of per-ad records.
The looper does not rename signal_id, add candidate fields or run product council.

Products run sequentially in source order. Per-query parallelism remains inside
the researcher. This preserves its global ad-ID duplicate policy, including the
case where two different products share an ad: both entries are considered in
order and the researcher's duplicate claim decides whether the later one is a duplicate.
No preemptive cross-product deduplication or alternative market-scoped policy is added.

A definitive item failure can be reported while later entries continue. A pending
registry update, ambiguous execution or broken host coordination stops the batch.
Unprocessed entries are reported as not_run. Full child results are retained so
partial scores and published-but-unregistered artifacts are not hidden.

## Package and dependencies

AGENT.md is the compact entrypoint. agent.yaml is descriptive metadata, not a
native vendor manifest. references/ defines the transport, correlation and failure
rules; schemas/ defines public contracts; tests/ contains network-free reference
checks and mocks, not a production connector or LLM runner.

The looper has NO directly invoked skills and its kit contains no skill ZIPs.
The build supplies a generated closure of its canonical input/child schemas in
`contract-resources`, resolved by `contract-bindings.json`. These resources do not
install the child agent. Provision the complete researcher separately with its
three independently selected skills and real claim-store and coordinator bindings. See
[portable deployment](references/deployment.md); a sibling repository checkout is
no longer required merely to validate a deployed looper's schemas.

## Deployment

Configure discovery_folder_id, researcher_binding and coordinator_binding using
config.example.json. Null placeholders intentionally prevent an unconfigured run.
The bindings are trusted host handles, not API keys, URLs or public input fields.
The coordinator must supply durable dispatch records, terminal-result retrieval and
recovery for open dispatches. The child owns its per-ID claims.
Sequential calls within one chat alone cannot protect other concurrent writers.

The looper needs read-only Google Drive access. Storage writes belong only to the
researcher. Host task/journal storage is separate and must be durable. Neither a
universal one-click import format nor hosted execution is claimed. Separate
ChatGPT and Claude Cowork notes describe the binding requirements.

## Validation

Use the shared build/check commands below. Offline tests exercise mapping,
immutable source handling, one-product dispatch and recovery with mocks. They do
not run a real child agent or establish a global lock. If host mock injection is
unavailable, record that gap without blocking resource installation; the actual
invocation, serialization, journal and recovery guarantees remain prerequisites
for live child dispatch.

## Build the installation kit

From the repository root:

```sh
python3 -B tools/agent-packaging/build.py --agent ecommerce-discovery-looper
python3 -B tools/agent-packaging/test_packages.py --bundle dist/portable-agents/ecommerce-discovery-looper
```

Open the kit's START-HERE.md. The default kit and instructions are platform-neutral;
use the relevant host adapter only for that deployment. The optional Python checker
has documented runtime targets, and equivalent validators are supported elsewhere.
For another build use a fresh --output directory so edited settings are preserved.
Package version 1.1.1 distinguishes unreported lookup completeness from known
truncation. Existing wire formats and required identity/content checks are unchanged.
