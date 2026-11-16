# Agent packaging and portability policy

The repository contains platform-neutral agent behavior plus optional host adapters.
This policy applies the product-discoverer setup lessons to all agents without
turning one host's UI or execution environment into a universal requirement.

## Boundaries

| Layer | Owns | Does not require |
| --- | --- | --- |
| Agent core | Inputs, outputs, business gates, evidence and operational semantics | A particular chat product, UI, language or mount path |
| Skill dependency | Its substantive instructions, schemas and referenced resources | A duplicate copy inside each agent |
| Host adapter | Skill selection, resource mapping, tools, identity and approvals | Identical tool names or import formats across platforms |
| Build tooling | Reproducible distribution, individual skill ZIPs, coherent manifests | Runtime network installation or a host version API |
| Validation adapter | Real schemas/formats, fixtures and deterministic tests | That every host runs Python or exposes local files |

The core may require real capabilities such as a headed browser, full-content
persistence, global serialization, settled child results or durable recovery.
Those requirements protect the workflow. A host lacking them cannot perform the
corresponding live work; platform neutrality is not permission to silently replace
them with a scraper, model promise, local chat sequence or trigger receipt.

## One source, one package per skill

Skill source lives once under repository-root `skills/`. Agent dependencies are
logical selections declared in `skill-bindings.json`; schemas use logical URIs.
At deployment, the selected package supplies both behavior and schema resources.
Selection may be an agent-specific attachment, a task/plugin binding, an explicitly
selected global skill, a directory or a resource API, according to the host.
Never substitute a namesake or persist a temporary execution path as configuration.

A kit with several skills contains several independently installable ZIPs. Each ZIP
has one complete skill root including references, examples and licenses. The kit's
outer download archive is transport only; it is not a combined installable skill.
A host supporting directories or resource APIs need not use ZIP files at all.

No extra runtime skill-version hash pin is imposed. The host chooses each configured
package. Compatibility checks use the actual selected schemas; they cannot prove
host attachment identity or detect every semantic change in skill instructions.
Distribution checksums protect the download, not a supposed host version guarantee.

## Inter-agent dependencies

The looper's researcher remains a separate complete agent with its own three skills,
settings and provider authority. The looper has zero directly invoked skills.
Its build exports only the needed canonical schema closure as contract resources,
including relative and logical references. Those files enable offline validation;
they neither install the child nor prove its live version, invocation or coordinator.
Generated contract snapshots are rebuilt from source, never manually maintained
copies of full skills. Update and validate them with their consumer package.

## Validation and operational metadata

Test the actual distribution with moved/renamed resource roots, individually selected
skills and site packages disabled. Validate schema definitions, local references,
formats, valid examples, controlled invalid cases and the source regression suite.
Ship the optional checker's full locked dependency set, including URI/date formats
and onboarding's country/currency lookup. Do not equate JSON parsing with validation.

The supplied Python/Linux checker is an optional implementation. Hosts may use an
equivalent validator in another language or bind schema bytes through resource APIs.
Separate resource/schema checks, deterministic reference mocks, actual host mock
traces, authenticated reads and live execution. An unavailable host mock-injection
harness is recorded as untested; actual operational capabilities still need to work.
Installation alone does not authorize paid searches, browser jobs or probe writes.

Missing/null optional trash/add-permission metadata is unknown and recorded in host
run notes. Explicit denial or trash state fails. Required identity, parent/type,
complete unique-name lookup, full byte verification and stable-version reads remain
mandatory. Do not infer add permission from sharing/edit flags, or weaken locks,
registry preservation and recovery because a connector omits optional fields.
Keep each existing result format; do not turn warnings into fabricated research
failures or add unrequested business fields.

## Updates and evidence

Build from canonical source, validate the kit, and replace agent/support/generated
contracts coherently. Keep one active deployment configuration. Replace each selected
skill independently when intended and recheck compatibility. Preserve a prior working
configuration for rollback. Updating GitHub does not update a hosted agent.

The November 2026 discoverer setup showed how duplicate skill trees, fixed execution
paths, stale manifests, missing validator dependencies and overstated test evidence
made installation painful. The extension found onboarding also needed pycountry and
URI format validation, and the looper's schemas depended on neighboring source
folders. These are now build/contract concerns, rather than manual setup repairs.
The [original review](discoverer-setup-lessons.md) also records assistant-guidance
mistakes; the lesson is to inspect actual host capabilities before proposing detours.
