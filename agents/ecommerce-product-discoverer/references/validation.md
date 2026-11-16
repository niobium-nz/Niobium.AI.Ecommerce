# Validation evidence and setup checks

Keep these evidence levels separate. Report actual commands/results, the source
manifest and environment; never promote a checklist or JSON parse to a test pass.

| Level | Evidence | What it does not establish |
| --- | --- | --- |
| Package | Files and manifest match, one skill, references readable | Model behavior, connection or permissions |
| Offline contracts | Real Draft 2020-12 validation, format checks, invalid cases, deterministic source tests | Hosted tool sequencing or live writes |
| Host acceptance | Actual mocked tool traces, where injection is available | Real provider/authentication behavior |
| Connected read-only | Actual runtime reads the configured folder and action schemas | Ability to write or a saved result |
| Authorized live run | Search trace and complete upload verification | Future permissions, exhaustive research or profit |

The repository supplies package/contract tests and a host acceptance checklist.
It does not supply a universal hosted model mock-injection harness. Where the host
has no such mechanism, record `host_orchestration_acceptance: not_executed` and
continue setup if its resource and validation requirements pass. Do not claim the
agent is live-tested. Do not run paid searches or create files merely to complete
installation. A real discovery request authorizes its documented search/save flow,
subject to the host's existing approvals.

## Offline helper

Two compatible kit formats are available. The shared, platform-neutral kit uses
`--skill ecommerce-cluster-ads=<selected-skill-resources>` and accepts a fragment
directly in `--schema <schema-URI>#/$defs/cluster`. Its instructions are in
[deployment.md](deployment.md). The commands below use the earlier discoverer
exporter's `--skill-resources` and separate `--fragment` arguments. Use the command
format matching your kit; never mix their support folders or manifests.

The generated kit supplies `offline-validation-support/validate.py`, verified
wheels, their lock, licenses and the agent-file manifest. Use standard CPython
3.11-3.14, Linux glibc >= 2.17, x86_64 or aarch64. Free-threaded builds, musl,
Windows/macOS and other versions require an equivalent validator or a compatible
execution environment. They are not supported by these particular wheels.

Resolve the placeholders below from current host resource handles. They are not
literal paths, persistent configuration or a request to copy the skill:

```sh
python3 -I -S -B <support>/validate.py check \
  --package <agent-resources> --skill-resources <selected-skill-resources> \
  --config <active-deployment-config> --source-tests
```

This verifies the agent manifest, required skill resources, schema definitions,
local references, valid examples, controlled invalid cases, large integers and
source tests. It loads dependencies offline with `FormatChecker` and a local-only
registry. It does not install packages, use connectors, choose a skill version,
prove attachment identity or start a discovery job. The configuration is validated
but not printed in the report. A failed/unsupported check must be reported exactly.

For a runtime JSON Schema check, use the same bindings:

```sh
python3 -I -S -B <support>/validate.py validate \
  --package <agent-resources> --skill-resources <selected-skill-resources> \
  --schema schemas/handoff.schema.json --instance <complete-json-file>
```

`--schema` also accepts either logical skill URI. Cluster-level validation can add
`--fragment '#/$defs/cluster'` with the output URI. Semantic checks in `AGENT.md`
(original equality, eligibility, coverage and persistence) remain required.

## First-run and update checks

Confirm the actual execution context can read agent resources and the selected
skill, not merely that a ZIP is attached or the editor lists tools. Run the offline
check with the actual active configuration. Inspect authenticated app actions;
check folder ID/name/type read-only if available. Missing optional trash/add flags
are warnings, not permission failures. Actual denial is a failure.

After changes to instructions, resources, skill selection or connectors, repeat
the affected checks. Replace package/support together; mismatched or extra files
are a useful stale-installation error. Do not rerun already passed checks simply
because a provider omits the same optional fields. Record untested host/live
behavior separately and keep the agent private/unscheduled unless requested.
