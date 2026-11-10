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
at `discovery/{job_id}/{signal_id}.json` and the subsequent OneDrive ID append.
The looper does not rename signal_id, add candidate fields or run product council.

Products run sequentially in source order. Per-query parallelism remains inside
the researcher. This preserves its global ad-ID duplicate policy, including the
case where two different products share an ad: both entries are considered in
order and the researcher's registry decides whether the later one is a duplicate.
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

Keep this folder alongside ecommerce-product-discoverer and
ecommerce-competitive-researcher in the repository's agents
folder. See dependencies.json for exact canonical contracts. When exporting,
bind those same resources and the complete researcher package explicitly; the
looper folder alone cannot execute research. No skill copy or relocation is
needed: the researcher remains the owner of its three existing skills.

## Deployment

Configure discovery_folder_id, researcher_binding and coordinator_binding using
config.example.json. Null placeholders intentionally prevent an unconfigured run.
The bindings are trusted host handles, not API keys, URLs or public input fields.
The coordinator must supply actual globally shared serialization, durable dispatch
records, terminal-result retrieval and recovery holds for the researcher's registry.
Sequential calls within one chat alone cannot protect other concurrent writers.

The looper needs read-only Google Drive access. Storage writes belong only to the
researcher. Host task/journal storage is separate and must be durable. Neither a
universal one-click import format nor hosted execution is claimed. Separate
ChatGPT and Claude Cowork notes describe the binding requirements.

## Validation

Run from this folder in a full repository checkout with Python 3.11+ and
jsonschema available:

```sh
PYTHONDONTWRITEBYTECODE=1 python -m unittest discover -s tests -v
```

No Python runtime is required by the agent instructions. Tests exercise contract
mapping, immutable source handling, one-product dispatch, safe continuation and
recovery stops with mocks. They do not run real skills, connectors, hosted agents,
locks or paid searches. Complete tests/host-acceptance.md before live deployment.
