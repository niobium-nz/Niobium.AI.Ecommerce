# ChatGPT Workspace Agent binding

Create a workspace-agent definition named ecommerce-discovery-looper with AGENT.md
as instructions and the referenced contracts/resources. Keep all canonical paths
resolvable or provide explicit resource mappings. agent.yaml is descriptive source,
not a native import schema. Do not deploy or publish while authoring this PR.

Bind read-only Google Drive actions and trusted researcher/coordinator handles.
Provision the existing researcher separately with its complete package and skills.
Do not replace its workflow with a prompt paraphrase or attach the full discovery
array to every child request. The looper does not need the researcher's write tools.

Official guidance describes workspace-agent instructions, files, apps and custom
MCP configuration. That alone does not prove a particular workspace offers an
agent-to-agent invocation with terminal result retrieval, durable journaling and
global locking. Verify those capabilities in the actual host or an authorized
coordinator integration. A channel trigger receipt without the final result is
insufficient. Do not invent an API, use a browser workaround or expose access tokens.

Map the coordinator to the researcher's actual registry guard/recovery domain;
exercise mocked failure/timeout and crash cases before enabling paid child runs.
Approval gates still apply to the child's actions. Missing capabilities must fail
preflight rather than falling back to concurrent products or manual skill calls.
No live workspace deployment or end-to-end integration has been tested here.

Source checked 2026-11-11:
https://help.openai.com/en/articles/20001143-chatgpt-workspace-agents-for-enterprise-and-business
