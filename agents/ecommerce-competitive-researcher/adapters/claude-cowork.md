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

Bind the selected competition skill's approved ad search, normalization/scoring
research, trusted clock/UUID/JSON, raw Google Drive publication and OneDrive full
read/append verification. Require the real shared-registry guard and durable recovery
holds across all writers. Allow query workers only where supported and authorized;
otherwise execute sequentially. Workers do not receive storage-write authority.

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
