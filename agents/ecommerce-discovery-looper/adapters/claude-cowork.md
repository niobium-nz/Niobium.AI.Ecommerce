# Claude Cowork binding

Provide this source package and its canonical dependencies through the host's
supported instruction/resource or plugin workflow. Preserve AGENT.md behavior;
agent.yaml is not a native Claude plugin manifest. Provision the complete
researcher separately, including its three existing skills and required connectors.

Bind an isolated researcher invocation for ONE product, with final-result retrieval,
read-only Drive access for the looper, and the actual globally shared coordinator.
Where the host supports subagents, use one product child at a time, not product
fan-out. The researcher's keyword fan-out remains its own responsibility.
A subagent feature by itself is not a global registry lock or durable task journal.

Verify the current host's supported plugin/connector packaging and runtime behavior
at deployment; these notes do not claim a universal ZIP format or automatic
agent import. Never treat chat-only skill invocation as equivalent to a settled
child execution with storage/recovery guarantees. Use the same host-protocol.md
requirements and reject missing capabilities rather than inventing tools.

Run tests/host-acceptance.md with mocked connectors before separately authorized
live research. Keep approval gates, shared recovery holds and credentials in the
host, not this source bundle. No Cowork deployment or live test was performed.

Sources checked 2026-11-11:
https://support.claude.com/en/articles/13837440-use-plugins-in-claude
https://support.claude.com/en/articles/11176164-use-connectors-to-extend-claude-s-capabilities
