# ChatGPT Workspace Agent adapter

These steps apply only to ChatGPT. The core agent and default kit do not require
this UI, an attachment mechanism, Linux or Python. Check the actual workspace's
available controls before concluding an action is unsupported.

1. Extract the kit locally. Open Agents, then Create on the Agents page and Start blank.
2. Use INSTRUCTIONS.md as the core instructions and bind the selected resources below.
3. With Add files -> Upload folder, select the outer `agent-files/ecommerce-competitive-researcher` folder
   itself, preserving relative resources. Upload `offline-validation-support` if using
   its optional checker, and `contract-resources` if present. Reading/extracting a ZIP
   in a temporary run does not itself install persistent Agent Files.
4. Fill `deployment-config.json` with actual settings and upload exactly one active
   config. Null placeholders are intentionally invalid. Never embed credentials.

Attach each of these ZIPs independently through **Add skill**, once each:

- `skills/ecommerce-analyze-competition.zip`
- `skills/ecommerce-normalize-products.zip`
- `skills/ecommerce-score-products.zip`

Do not attach a combined skills archive or copy the extracted skills into Agent
Files. These are attachments to this agent, not global Plugins/Skills entries.

Resolve each attached skill's current resources from this agent's execution context.
Do not persist a temporary mount path, invent a host version or substitute a global
same-name skill. The host selects the attachment; the checker validates compatibility.

Bind the selected competition skill's approved ad search, normalization/scoring
research, trusted clock/UUID/JSON, raw Google Drive publication, and full read,
in-place update and readback of the Drive registry. Require the shared-registry
guard and durable recovery holds across all writers. Allow query workers only where supported and authorized;
otherwise execute sequentially. Workers do not receive storage-write authority.

Run the setup checks in references/deployment.md in Preview, which may have a
different authenticated context from the builder. Use the current package's helper
arguments with one --skill binding per skill, not a hardcoded path. Do not install
libraries online, spend on research or write probe files during setup. If no host
mock harness exists, report it untested; actual required capabilities still apply.
Missing optional Drive trash/add metadata warns; explicit denial or unverified
persistence still fails. Preserve host approvals. Save privately unless broader
sharing/scheduling is requested.

For updates, preserve settings, replace the complete agent/support/contract folders
and Instructions together, retire obsolete source ZIPs, and keep one active config.
Update skill attachments individually when intended, then rerun compatibility checks.
Do not merge stale nested skills or claim that a local build changed the hosted agent.
If automated agent management is unavailable, state that specific missing operation
and use these native file/skill controls; do not invent a wrapper-skill workaround.

[Official workspace agent guidance](https://help.openai.com/en/articles/20001143-chatgpt-workspace-agents-for-enterprise-and-business)
describes configurable files, skills, apps and Preview. It is not evidence that this
particular deployment has the agent invocation, browser or coordination guarantees.

A successful all-type exact-name search may omit completeness metadata. Record
`drive_lookup_completeness_unavailable` and continue under the core storage policy;
do not require a fabricated completion flag. Follow exposed tokens and fail on
actual errors, known truncation or observed conflicts. This applies to live jobs,
not only setup. Updating only the Instructions while keeping old agent resources
can restore the old blocker; replace a coherent kit.
