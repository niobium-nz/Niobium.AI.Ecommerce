# Claude Cowork and other task-host adapter

Use the host's supported instruction/project/plugin/resource workflow to bind
AGENT.md and all agent resources. agent.yaml is descriptive, not a native plugin
manifest. Preserve relative references or register explicit equivalent mappings;
no ChatGPT attachment UI, fixed filesystem layout or Python runtime is required.

Select each declared skill independently. The kit supplies one ZIP per skill;
use each ZIP separately when the host accepts skill ZIPs, or bind each complete
canonical directory through the host's plugin/resource API. Do not combine the
skills into one installable archive or add a second copy inside the agent.
Read skill-bindings.json and register schemas from the actual selected packages.
An explicit global skill binding is valid if that is how this host selects skills;
do not discover an arbitrary same-name copy. No additional version pin is required.

Connect Firecrawl through the host's MCP connector, authorized plugin or equivalent Firecrawl API binding. Verify that its scrape action accepts the source location and returns content and links.
Bind board pixel reading, trusted UUID generation and Google Drive lookup, raw
upload and verification. Keep provider credentials in the host connection.

Configure trusted deployment settings, inspect actual authorized tool semantics
and retain existing host approvals. Never copy another host's tool namespaces or
infer write access from search support. Follow references/deployment.md for evidence
levels, optional offline validation and updates. An equivalent Draft 2020-12 validator
with local references and format checks is valid; the supplied Linux Python checker
is only one implementation. No online dependency install is required in the host.

Where mock injection is unsupported, mark host acceptance untested while completing
resource/schema setup. Operational prerequisites remain required before live work.
Do not start paid research, substantive browsing, cloud writes, publication or a
schedule merely to install. Missing optional Drive flags warn; actual failures stop.
Update agent/support/contracts coherently and selected skills independently; preserve
one active config. No hosted execution or provider verification is claimed here.

Check current [skills](https://support.claude.com/en/articles/12512180-use-skills-in-claude)
and [plugin](https://support.claude.com/en/articles/13837440-use-plugins-in-claude)
support in the target host; this source is not a universal native import format.

A successful all-type exact-name search may omit completeness metadata. Record
`drive_lookup_completeness_unavailable` and continue under the core storage policy;
do not require a fabricated completion flag. Follow exposed tokens and fail on
actual errors, known truncation or observed conflicts. This applies to live jobs,
not only setup. Updating only the Instructions while keeping old agent resources
can restore the old blocker; replace a coherent kit.
