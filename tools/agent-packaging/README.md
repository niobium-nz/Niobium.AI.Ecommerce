# Portable agent installation kits

Build one agent or all four from the repository root:

```sh
python3 -B tools/agent-packaging/build.py --agent all
python3 -B tools/agent-packaging/test_packages.py --bundle dist/portable-agents
```

The default kit is platform-neutral. INSTRUCTIONS.md is exactly the agent's AGENT.md.
Use `--agent ecommerce-competitive-researcher` (or another name) for one kit. Use
`--adapter chatgpt` or `--adapter claude` only to include that host's additional setup
guide; it does not change the core instructions or contracts. Host guides already
remain available under the agent's adapters/ directory.

| Agent | Independently deployable skill ZIPs | Other dependency |
| --- | --- | --- |
| Product discoverer | ecommerce-cluster-ads | Configured search and Drive actions |
| Competitive researcher | ecommerce-normalize-products; ecommerce-analyze-competition; ecommerce-score-products | OneDrive registry, shared guard/recovery and Drive publication |
| Onboarding specialist | ecommerce-profile-offers; ecommerce-develop-strategy; ecommerce-draft-prompts | Headed browser, board reading, Drive publication |
| Discovery looper | None | Separate researcher, coordinator and generated contract schemas |

Each ZIP contains one complete skill, never several skills as one installable ZIP.
The outer downloaded kit is only a transport container. Directory/resource API hosts
may use canonical directories instead. `agent-files/` is a distribution convention,
not a required host path or vendor file tab. Looper contract-resources contain only
the canonical schema closure, not a duplicate child agent or skill installation.

The builder reads the shared checksum lock, downloads exact wheel URLs and verifies
hashes before caching. Rebuild without network using `--offline`. The current cache
default retains the previous discoverer cache location, `.cache/discoverer-wheels`;
`--wheel-cache` can choose another. Use a fresh `--output` to avoid overwriting edited
settings. Fixed ZIP order/timestamps make equal inputs produce equal output bytes.
Generated outputs and caches remain outside Git. Default configurations are unfilled.

## Optional validation adapter

The generated offline support includes jsonschema, referencing, format-checking
libraries, pycountry and their locked dependencies/licenses. It supports standard
CPython 3.11-3.14 on Linux glibc >= 2.17, x86_64/aarch64. This is not a requirement
on agent hosts: equivalent schema validators/resource APIs are supported. No host
needs network access or pip merely to run the supplied checks.

Resolve current resources through the host and run, substituting current handles:

```sh
python3 -I -S -B <support>/validate.py check \
  --package <agent-resources> --config <active-config> \
  --skill <skill-name>=<selected-skill-resources> --source-tests
```

Repeat --skill for each declared skill. The looper has no --skill arguments; supply
`--contracts <generated-contract-resources>` instead. Missing bindings have no global
fallback. Use `validate --schema <local-schema-or-logical-URI> --instance <json-file>`
with the same package/skill/contract arguments for individual schema checks.
Do not save these execution paths in deployment configuration. Passing a directory
to a checker does not prove host identity or pin its skill version.

The tests execute real package/source checks, relocated roots, independently selected
skill changes, missing/corrupt dependencies, external-reference rejection, generated
contract integrity and offline reproducibility. They also verify that the optional
host guide leaves core instructions unchanged. Unit/reference mocks are not actual
hosted model traces. No connector calls, paid research, browsing jobs or cloud writes
are performed by these tests.

## Maintenance and compatibility

Change source once. The common build primitives, locked libraries and resource
adapter are shared with the discoverer's earlier ChatGPT exporter. That exporter
remains at tools/ecommerce-product-discoverer/build.py for existing workflows;
new multi-platform deployments should use this default-neutral builder. Do not mix
support folders/manifests from the two kit formats. Their CLI arguments differ.

Review pinned library versions, exact wheel hashes/targets, complete dependencies and
licenses when updating the lock. The checker fails if a declared format is unavailable.
Skill source updates do not require changing a runtime hash pin. Update the selected
skill deliberately and rerun compatibility checks against its actual schema resources.

The Agent packages workflow checks all kits on Python 3.11 and 3.14 and publishes one
artifact per agent. Artifacts expire after 90 days and can be regenerated. A separate
discoverer compatibility workflow protects its earlier exporter. Both use read-only
repository permissions and perform no live agent deployment. See
[the portability policy](../../docs/agent-portability.md) for boundaries and lessons.
