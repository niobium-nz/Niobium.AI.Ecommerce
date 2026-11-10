# Claude Cowork setup

This is portable source, not a native import manifest or a completed deployment.
Configure ecommerce-competitive-researcher with AGENT.md as the instructions and
all referenced schemas/resources available at their relative paths. Use the host's
current supported instruction/file/skill mechanism; do not claim a shared ZIP is
one-click importable across vendors.

Install/bind all THREE complete bundled skills, not SKILL.md alone and not older
installed variants. Preserve their schema/reference/example/metadata directories.
Bind the competition skill's approved ScrapeCreators search capability, ordinary
web research for normalization/scoring, trusted clock/UUID/JSON, writable Google
Drive and writable OneDrive full-file access. Validate actual tool schemas and
permissions. A read-only connector is insufficient; resolve missing write or
serialization capabilities before running rather than substituting providers.

Fill the real resource IDs, registry_format and timezone in deployment settings.
Keep public job input at exactly job/product/ads. The discovery root must exist;
job folders are created only for publication. Restrict Drive writes to that root
and OneDrive writes to the configured registry. Preserve approval gates and avoid
credentials in prompts or repository files.

Configure a single serialized product-job queue/shared guard for this registry
and durable recovery holds for interrupted commits. This applies across hosts.
Allow bounded query workers only when the host supports them with the same skill
resources and tool scope; otherwise execute queries sequentially. Workers receive
no Drive/OneDrive write authority. Do not invent a universal subagent API.

Run offline tests and mocked host acceptance before any authorized live test.
A published_registry_pending result requires reconciliation, not automatic rerun.
Candidate publication is not council approval or permission to invoke onboarding.
No hosting, publishing, scheduling, paid searches or cloud writes were performed
while authoring this package.

Official setup reference, checked 2026-11-11: https://support.claude.com/en/articles/13837440-use-plugins-in-claude
