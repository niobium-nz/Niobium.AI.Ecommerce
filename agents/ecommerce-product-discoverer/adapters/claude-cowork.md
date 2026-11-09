# Claude Cowork setup

Keep AGENT.md and the bundled skill as canonical behavior. This source bundle is
not a prebuilt Claude plugin, and agent.yaml is not a Claude manifest. Official
guidance checked on 2026-11-09 describes plugins/skills and connectors; verify
current packaging and availability during deployment.

1. Provide the complete agent directory as trusted task/project context, or use
   the supported plugin packaging workflow. Set instructions to AGENT.md without
   changing its contracts or dropping required resources.
2. Install the complete ecommerce-cluster-ads skill through the supported skill
   mechanism. Preserve all relative schemas, suffix lookup, license and examples.
3. Bind authorized ScrapeCreators keyword search and Drive exact child lookup,
   metadata, explicit-parent raw JSON upload and complete verification. Do not
   assume a default Drive connector supports writes or equivalent schemas.
4. Supply signal_folder_id and timezone as trusted deployment settings. Verify
   clock, UUID, timezone, lossless JSON and actual file-reference capabilities.
   Use tool-bindings.md rather than copying ChatGPT-specific namespaces.
5. Limit permissions and retain write approvals. Do not grant unrelated apps,
   use a browser fallback or put API keys in the bundle.
6. Run the offline fixtures and mocked host acceptance cases, then obtain separate
   authorization for any paid search/live-write test.

No Cowork deployment or live integration test was performed. Check the target
host's current plugin format instead of assuming a shared vendor-neutral ZIP
format. Use supported instruction/resource binding, or resolve capability gaps
before running. Do not claim successful deployment merely from these notes.

Sources:
https://support.claude.com/en/articles/13837440-use-plugins-in-claude
https://support.claude.com/en/articles/12512180-use-skills-in-claude
