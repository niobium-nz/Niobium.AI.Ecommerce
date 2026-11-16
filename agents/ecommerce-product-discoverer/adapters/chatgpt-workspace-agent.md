# ChatGPT Workspace Agent setup and updates

This adapter applies to a skill ZIP attached to this workspace agent. It does not
install a global Plugins/Skills entry. UI labels can vary by workspace; the folder
upload flow below was observed during the November 2026 setup. `agent.yaml` and the
kit ZIP are not native one-click import manifests.

## First installation

1. Download a tested installation kit and extract it on your computer. Do not
   attach the whole kit or ask the builder to promote an extracted temporary
   directory into Agent Files.
2. Open **Agents** in the sidebar, then **Create** on the Agents page and **Start
   blank**. Create need not be visible directly in the sidebar.
3. Paste `instructions.chatgpt.md` into Instructions. It is generated from the
   adapter binding text and the current `AGENT.md`; do not append an older prompt.
4. In **Add files -> Upload folder**, select the outer
   `agent-files/ecommerce-product-discoverer` folder itself. Repeat for
   `agent-files/offline-validation-support`. Preserve both folder names and their
   contents. Upload the two folders separately; do not add a nested skills folder.
5. Edit `agent-files/deployment-config.json` with the existing `discovery` folder
   ID and your IANA timezone, then upload that one file. The null placeholder is
   intentionally invalid. Never embed account credentials in it.
6. Use **Add skill** to attach `skills/ecommerce-cluster-ads.zip` once. Keep its
   internal resources together. Do not also upload the extracted skill to Files.
7. Connect the approved ScrapeCreators and Google Drive actions described in
   `references/tool-bindings.md`. Check actions and authentication in **Preview**,
   since the builder's connection context may differ from execution.
8. Run `SETUP-CHECK.txt` from the kit in Preview. Resolve current resource handles
   through the uploaded files and attached skill; do not save an execution path.
   Expect real schema validation and source tests, no paid searches or Drive writes.
9. Save/create the agent privately. Publishing, sharing or scheduling is a separate
   user choice. If no host mock harness exists, mark those cases untested; do not
   block installation solely for that reason. A live discovery run remains untested
   until the user requests one and its actual tool trace/result is verified.

The intended layout in Agent Files is:

```text
deployment-config.json
ecommerce-product-discoverer/
  AGENT.md
  skill-bindings.json
  schemas/ references/ examples/ tests/ adapters/
offline-validation-support/
  validate.py
  deployment-manifest.json
  dependency-lock.json
  python-dependencies.zip
  README.md
```

One `ecommerce-cluster-ads` attachment appears in the agent's skill area, outside
this file tree. The validator's dependency ZIP stays compressed intentionally;
the helper reads it. Agent source files must be readable as individual resources.

## Updating an existing agent

Keep the current instructions, settings and skill attachment available for rollback.
Replace the complete agent folder, support folder and Instructions from one kit.
Remove obsolete source ZIPs and old copies through the editor's supported file
controls. Preserve exactly one active deployment config. If the older install uses
`04-deployment-config.json`, either keep that name and explicitly select it as the
active config, or replace it with `deployment-config.json`; never leave both active.
The generated instructions support either name and require an unambiguous choice.

Keep the existing skill attachment unless deliberately updating the skill. If
updating it, replace that attachment once and rerun compatibility checks. Do not
reintroduce a nested skill to satisfy an old manifest. Retire stale instructions
that refer to `02-agent-files.zip`, an exact skill Git tree, or a fixed mount path.
Do not merge old files into the replacement folders. Run the setup check in Preview
again before normal use; retain your prior state if compatibility fails.

## When setup cannot be completed automatically

State the exact unavailable operation (for example, no connected agent-management
API or UI cannot preserve resource structure), and give the corresponding manual
step above. Do not describe reading a ZIP or writing local files as installing an
agent. Do not add a wrapper skill as an automatic workaround. First use the host's
actual file/folder and skill controls. Ask only for the specific missing setting
or capability, not repeated approvals for steps already authorized.

Official [workspace agent guidance](https://help.openai.com/en/articles/20001143-chatgpt-workspace-agents-for-enterprise-and-business)
describes the builder, Preview, files, skills and app connections. Product behavior
and the availability of management APIs must be checked in the target workspace.
