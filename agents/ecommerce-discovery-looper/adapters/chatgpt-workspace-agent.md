# ChatGPT Workspace Agent adapter

These steps apply only to ChatGPT. The core agent and default kit do not require
this UI, an attachment mechanism, Linux or Python. Check the actual workspace's
available controls before concluding an action is unsupported.

1. Extract the kit locally. Open Agents, then Create on the Agents page and Start blank.
2. Use INSTRUCTIONS.md as the core instructions and bind the selected resources below.
3. With Add files -> Upload folder, select the outer `agent-files/ecommerce-discovery-looper` folder
   itself, preserving relative resources. Upload `offline-validation-support` if using
   its optional checker, and `contract-resources` if present. Reading/extracting a ZIP
   in a temporary run does not itself install persistent Agent Files.
4. Fill `deployment-config.json` with actual settings and upload exactly one active
   config. Null placeholders are intentionally invalid. Never embed credentials.

This kit contains no skill ZIP. Bind the generated `contract-resources` folder
as files and provision the researcher separately. Its three skills belong to that
researcher, not to the looper.

Resolve the looper's current agent and contract resources in its execution context.
Do not persist temporary mount paths. The researcher manages its own skill selection;
this looper's checker validates the supplied contract schemas.

Provision the full researcher separately, with its own selected skills and
configuration. Bind one-product invocation with correlated terminal/quiescent results,
read-only Drive, and the actual globally shared coordinator, durable dispatch journal
and recovery holds. A trigger receipt or subagent feature alone proves none of those
guarantees. Missing operational capabilities block dispatch; a contract resource file
does not replace a child agent. The looper needs no skill ZIP, paid search or write tools.

The Workspace Agents trigger API can queue an execution and report run status.
Use the [checkpoint runner](../references/checkpoint-runner.md) with a trusted
terminal-result and registry-coordination adapter. The API does not supply the
structured result required by the looper.
[OpenAI trigger and status documentation](https://developers.openai.com/workspace-agents/trigger-runs)

Run the setup checks in references/deployment.md in Preview, which may have a
different authenticated context from the builder. Use the current package's helper
with --contracts and no --skill arguments, resolving current resource paths. Do not install
libraries online, spend on research or write probe files during setup. If no host
mock harness exists, report it untested; actual required capabilities still apply.
Missing optional Drive trash/add metadata warns; explicit denial or unverified
persistence still fails. Preserve host approvals. Save privately unless broader
sharing/scheduling is requested.

For updates, preserve settings, replace the complete agent/support/contract folders
and Instructions together, retire obsolete source ZIPs, and keep one active config.
Update the researcher's skills on that agent when intended, then recheck the child binding.
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
