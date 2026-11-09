# ChatGPT Workspace Agent setup

These are setup notes, not a native import manifest. agent.yaml and the neutral
ZIP are not a guaranteed one-click hosted-agent import. Official guidance checked
on 2026-11-09 describes configurable instructions, apps/custom MCPs, files and
skills; verify availability and resource-binding behavior in the target workspace.

1. Create ecommerce-product-discoverer. Use AGENT.md as instructions. Attach its
   schemas/references/examples with relative paths preserved or explicitly mapped.
2. Add the complete bundled ecommerce-cluster-ads directory as one skill ZIP.
   Preserve its schemas, suffix lookup, license and metadata. Do not rely on an
   older installed version with the same name.
3. Bind ScrapeCreators keyword search and Drive exact search, metadata, raw JSON
   upload and full verification. Follow tool-bindings.md. Read-only Drive or
   native document creation alone is insufficient.
4. Set the real existing signal folder ID and IANA timezone as deployment config,
   not public job inputs. Bind an authorized account with folder read/add access.
5. Constrain uploads to that folder and retain approval gates. Exclude unrelated
   paid endpoints, ads-manager writes, sharing and deletion. Verify UUID, clock,
   timezone conversion, lossless JSON and resource access.
6. Run offline/mocked host-acceptance cases before any explicitly authorized live
   smoke test. Publishing/scheduling is a separate deployment decision.

Keep final JSON contracts unchanged. Missing capabilities are deployment errors,
not permission to change destination or substitute local output. No hosted
Workspace Agent deployment or live connector test was performed for this bundle.

Sources:
https://help.openai.com/en/articles/20001143-chatgpt-workspace-agents-for-enterprise-and-business
https://developers.openai.com/cookbook/articles/chatgpt-agents-sales-meeting-prep
