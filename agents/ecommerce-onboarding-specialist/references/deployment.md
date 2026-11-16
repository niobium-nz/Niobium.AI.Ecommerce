# Portable deployment contract

`AGENT.md` owns this agent's behavior. `agent.yaml` describes it; neither is a
vendor-native import manifest. Host-specific setup belongs in `adapters/`.
The agent does not require ChatGPT, Python, Linux, an upload UI or a fixed local
layout. Required business/storage/browser/coordination semantics still apply.

## Resources and selection

The selected packages supply substantive behavior and their schemas. A host may
select them through explicit skill handles, task configuration, plugin resources,
directories or resource APIs. No particular host's attachment UI is required.
Register every logical URI in `skill-bindings.json` against the corresponding
selected schema, preserving relative references within that same package. Do not
substitute an arbitrary same-named global copy. A deliberate global binding is
valid when the host uses that mechanism and its selection is explicit.

The repository has one canonical source for each skill under root `skills/`.
A generated kit contains one ZIP per skill; each ZIP contains only that skill's
complete directory. Install/select each independently. The enclosing kit is a
transport container, never a multi-skill installable ZIP. Directory/API hosts may
bind the canonical skill directories without using ZIPs at all.

- `ecommerce-develop-strategy`: bind its complete selected package; deployment ZIP is `ecommerce-develop-strategy.zip`.
- `ecommerce-draft-prompts`: bind its complete selected package; deployment ZIP is `ecommerce-draft-prompts.zip`.
- `ecommerce-profile-offers`: bind its complete selected package; deployment ZIP is `ecommerce-profile-offers.zip`.

Resolve current resources on each execution. Persist logical bindings, not host
mount paths. The host selects the configured skill/child version; no additional
runtime skill hash pin is imposed by this package. Validate actual selected schemas
after an update. Download integrity manifests detect stale/mixed agent resources;
they do not authenticate a host attachment or impose a skill version.

## Setup evidence

Make instructions and required references/schemas readable, provide one active
configuration and check actual host capabilities. Default config IDs are null
placeholders, never valid live settings. Run true Draft 2020-12 validation with
format checks and local reference resolution; JSON parsing does not count.

The generated kit includes an optional offline Python adapter and all locked
validation dependencies. It supports standard CPython 3.11-3.14 on Linux glibc
>= 2.17, x86_64/aarch64. This adapter's implementation limits do not constrain the
portable agent: another host may use an equivalent validator and resource APIs.
No online dependency installation is required inside the agent host.

Resolve current resource handles and use the helper's check command with
`--package`, `--config`, one `--skill NAME=PATH` per declared skill, and
`--contracts` when generated contract resources are present; add `--source-tests`
for deterministic unit tests. These are per-execution arguments, not saved paths.
Use `validate --schema <local-schema-or-logical-URI> --instance <json-file>` with
the same resource bindings for a runtime JSON Schema check.

Record package checks, schema/unit tests, host mock traces, authenticated read-only
checks and live execution separately. If the host cannot inject mocks, record
`host_orchestration_acceptance: not_executed`; that alone does not block installing
the resources. Actual required capabilities, including browser/registry locks or
child/coordinator guarantees where specified, must exist before live work. Do not
claim those guarantees from a config flag or passing source tests.

Setup does not perform paid research, substantive browsing or cloud writes.
A live job/test needs the user's authorization and the host's existing approvals;
do not add repeated approval requests to steps already authorized.

## Updates

Build a fresh coherent kit. Replace the agent, validation support and generated
contract resources together; preserve exactly one active deployment config.
Do not merge stale source files or keep obsolete instructions as a second authority.
Update each selected skill independently when intended, then recheck compatibility.
A child-agent update also requires validating its actual invocation/coordinator
binding; contract snapshots alone do not prove live compatibility. Retain the prior
working configuration for rollback. GitHub does not update hosted agents automatically.

See the repository's
[build guide](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/tools/agent-packaging/README.md)
and [portability policy](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/docs/agent-portability.md).
