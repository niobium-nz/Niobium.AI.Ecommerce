# Product discoverer setup review

Reviewed 17 November 2026. Scope: the setup conversation, its pasted execution
reports, the prepared artifacts and the source package. These are observations
from that setup, not a claim that every ChatGPT/Claude environment behaves alike.
Some screenshots were unavailable to the assistant; UI conclusions use the user's
explicit descriptions. No paid searches, live Drive writes or hosted model
orchestration tests were performed for this repository change.

## What went wrong and what changes

| Observed problem | Cause and lesson | Change or next-time rule |
| --- | --- | --- |
| Confusion about where Create appears | Guidance assumed the sidebar layout and moved too quickly toward permissions | Check the Agents page and actual controls first; give the shortest supported UI steps |
| ZIP could be read/extracted but not made persistent Agent Files | Source archive readability was confused with installed resource binding | Generate upload-ready folders; upload each outer folder and verify readable resources in Preview |
| A wrapper-skill workaround was proposed before trying Upload folder | The assistant inferred a limitation before checking available UI actions | Try the native folder/resource controls before inventing packaging workarounds |
| Skill existed both as an attachment and inside the agent | Source layout and relative schema references forced a duplicate dependency | One canonical root skill, one ZIP attachment, logical schema URIs and no nested copy |
| Temporary execution path became proposed configuration | A readable local mount was mistaken for a stable host binding | Resolve the currently selected attachment/task skill each execution; persist no mount path |
| Uploaded skill was confused with global Plugins/Skills | Different host concepts were treated as interchangeable | Adapters name the exact selection surface and scope; never substitute a namesake |
| Host-issued skill version could not be verified | Extra content/hash pinning added maintenance without a usable host version contract | Host configuration selects the skill; no extra runtime pin; validate actual selected schemas |
| jsonschema missing and network installation failed | Hosted validation depended on undeclared environment state | Build in locked offline dependencies, formats and local schema resolution; declare supported targets |
| JSON parsing/static inspection was mistaken for validation progress | Evidence levels were not clearly separated | Report resource checks, true schema tests, unit tests, host traces and live connector results separately |
| Source manifest still expected the removed skill tree | Ad-hoc instructions/manifests/archives were updated independently | One repeatable build generates all install materials; test the built artifact, not only source |
| Instructions still referred to an obsolete source ZIP | Incremental uploads left competing instructions and stale files | Replace the complete package/support/instructions together; preserve one active config |
| Local tests were treated as proof of hosted orchestration | A host checklist existed but no mock injection harness was supplied | Mark unsupported host acceptance untested; do not invent a pass or block installation solely for it |
| Builder and Preview showed different connection availability | Exposed action schemas were confused with authenticated execution | Inspect the actual runtime; record schemas, authentication and write evidence separately |
| Drive wrapper omitted requested trash/add-permission fields | Optional provider metadata became a hard portability requirement | Unknown flags warn; explicit denial stops; mandatory identity, collisions and bytes still verify |
| Setup repeatedly expanded into new bundles and detours | Guidance and package defects were patched one at a time without a stable installation contract | Prepare and test one coherent kit; ask only for genuinely missing settings; stop repeating passed checks |

This was not solely an upstream package problem. The assistant's premature
workaround, temporary-path recommendation, ambiguous skill terminology and repeated
validation detours contributed to the work. Better assistance means inspecting the
available capabilities, owning these distinctions and giving concrete steps, rather
than making the user discover the correct packaging route through trial and error.

## Decisions retained

- Keep the core agent platform-neutral. Host adapters bind instructions, resources,
  skill selection and actual tool semantics; they do not redefine business behavior.
- Use the current skill selected for this agent/task. Download integrity metadata is
  not a runtime skill-version pin, and a filesystem hash is not host version identity.
- Never copy the selected skill into Agent Files to appease a stale manifest.
- Ship validation dependencies in generated kits, not as binary files in source
  control. Preserve licenses and verify hashes before loading libraries.
- Missing optional Drive metadata stays explicitly unknown. A real read/write denial,
  wrong destination, incomplete collision search or failed byte verification remains
  a failure. Do not infer add permission from sharing or editing permission.
- Installation does not authorize paid searches or probe writes. No new approval
  ritual is needed for steps the user has already authorized.
- Keep successful setup, verified live use and future permission guarantees separate.
  The current hosted agent is not automatically upgraded by changing GitHub source.

## Release and regression criteria

The [build tooling](../tools/ecommerce-product-discoverer/README.md) must produce
one coherent kit from canonical source, with consistent manifests and instructions.
The package tests execute it with site packages disabled, move its resource roots,
change the selected skill schema, reject missing/stale/modified agent files and
corrupt dependencies, and verify reproducible offline rebuilding. Tests must use
the selected schema and reject controlled invalid JSON/schema cases, not merely
parse example files. Explicit metadata denials and mandatory saved bytes remain
covered by deterministic policy tests. The downstream discovery looper must still
resolve and validate the handoff after the skill move.

The wire handoff remains 2.0.0. Package version 2.1.0 changes installation, resource
resolution and optional metadata handling. Original evidence, search limits,
clustering contract, lifecycle and save-verification requirements are preserved.
No folder/account IDs or host temporary paths from the real deployment belong in
this public repository. Synthetic fixtures and unconfigured templates are sufficient.

The earlier user-reported hosted check passed 68 tests with offline validation
support 1.1.0. That is evidence for that uploaded deployment only. The new source
and generated kit require their own checks; neither those tests nor the earlier
report establish live hosted model behavior or connector write success.

## Reference boundaries

[OpenAI's workspace agent guide](https://help.openai.com/en/articles/20001143-chatgpt-workspace-agents-for-enterprise-and-business)
describes builder/Preview and configurable files, skills and apps; exact account
capabilities still need observation. [Google's file resource](https://developers.google.com/workspace/drive/api/reference/rest/v3/files)
defines the optional metadata requested by adapters, but a wrapper may not expose
all API fields. [jsonschema validation documentation](https://python-jsonschema.readthedocs.io/en/stable/validate/)
distinguishes schema validation and format checking. The fallback policy and kit
layout here are project decisions informed by the observed setup, not claims that
these vendors guarantee a particular private mount path or import format.

## Applying the lessons to other agents

The [repository portability policy](agent-portability.md) extends these lessons to
the researcher, onboarding specialist and looper. It keeps host-specific controls
in adapters, supplies individual skill ZIPs, and shares offline validation tooling.
It preserves required browsing/locking/recovery semantics rather than weakening
them to fit a particular host. Generic kits are now available for the discoverer too.

## Follow-through after source changes

A subsequent live test failed with `folder_invalid` solely because optional
`trashed` and `capabilities.canAddChildren` fields were absent. The repository
already allowed this. The earlier installation bundle retained the stricter
wording, consistent with that result; inspect the saved host instructions before
concluding whether an old copy or noncompliant execution caused the failure.
Before guiding a live test after a
source change, establish whether the saved Instructions and uploaded resources
were updated. Do not describe a repository commit as a hosted-agent update.
Provide a coherent replacement kit and a short existing-installation guide that
preserves configuration and the selected skill. Distinguish a stale instruction
copy from an actual provider failure before spending on more test searches.
