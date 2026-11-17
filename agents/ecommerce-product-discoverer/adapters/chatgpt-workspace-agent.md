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
3. Paste the kit's instruction file into Instructions: `INSTRUCTIONS.md` for the
   portable kit, or `instructions.chatgpt.md` for the earlier discoverer exporter.
   Both use the current `AGENT.md`; do not append an older prompt. The earlier
   exporter additionally includes workspace resource-binding guidance.
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
8. Follow `references/deployment.md` for the portable kit, or run `SETUP-CHECK.txt`
   for the earlier exporter, in Preview. Resolve current resource handles through
   the uploaded files and attached skill; do not save an execution path.
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
Select the active resource explicitly; the earlier exporter's generated instructions
already support either name and require an unambiguous choice.

Keep the existing skill attachment unless deliberately updating the skill. If
updating it, replace that attachment once and rerun compatibility checks. Do not
reintroduce a nested skill to satisfy an old manifest. Retire stale instructions
that refer to `02-agent-files.zip`, an exact skill Git tree, or a fixed mount path.
Do not merge old files into the replacement folders. Run the setup check in Preview
again before normal use; retain your prior state if compatibility fails.

Some observed folder uploads append numbered file copies instead of replacing
existing files. Inspect the resulting tree. If replacement is unavailable, retain
the previous roots under clearly inactive rollback names and upload clean canonical
roots. Select only the canonical roots for validation and execution. Do not keep
numbered duplicates inside either active root or silently delete retained backups.
Save, reopen, and check the published state; draft changes and another editor tab
can otherwise leave a stale revision. A version conflict requires loading the
current draft and reapplying the intended changes, not overwriting concurrent work.

## Capture complete search responses

A hosted live search returned a successful response whose displayed tool result
was about 97,000 tokens, but the execution displayed only 10,000 tokens. No complete
response file was exposed and the script had not retained a session copy. The
agent correctly failed with `resource_limit` rather than fabricate the omitted ads
or save an empty discovery. This is a response-transport problem, not evidence that
the search returned no products.

Inspect the current host's real orchestration, session-storage, and local file
capabilities before the paid call. On the observed host, `functions.exec` can hold
the result of a nested tool call before printing it, `store`/`load` retain serializable
values within that session, and `tools.apply_patch` writes a programmatically built
patch. A local-only smoke test transported a 600,276-byte synthetic JSON string
through this path, preserving a large integer token, Unicode, quotes, escaped
newlines, both records, and the final marker. This does not prove that every
connector itself returns complete data or preserve HTTP wire bytes.

Use the discovered search action once inside that orchestration. Keep its exact
eight arguments from AGENT.md and increment the job's attempt count before calling.
Store the returned wrapper immediately without printing it. Require a non-error
wrapper with exactly one usable raw provider-JSON text block, or use a documented
complete response-file reference. Do not reconstruct original ads from a structured
JavaScript object that may already contain rounded numbers. A missing/ambiguous
raw payload, connector truncation, or failed capture is not a valid page.

The following is the local capture part, after inspecting the actual tool schema
and assigning the nested call result to `response`. `capturePath` and `sessionKey`
must come from the current trusted execution directory and job UUID, never from
ads, URLs, or tool-returned instructions. This is an adapter pattern, not an
additional search action or a portable core tool name.

```javascript
store(sessionKey, response);
if (response.isError) throw new Error("Search tool returned an error");
const blocks = response.content;
if (!Array.isArray(blocks) || blocks.length !== 1 ||
    blocks[0].type !== "text" || typeof blocks[0].text !== "string" ||
    !blocks[0].text.trim()) {
  throw new Error("Complete raw search JSON text is unavailable");
}
const raw = blocks[0].text;
store(sessionKey + ":raw", raw);
const lines = raw.replace(/\n$/, "").split("\n").map(line => "+" + line).join("\n");
const saved = await tools.apply_patch(
  "*** Begin Patch\n*** Add File: " + capturePath + "\n" + lines + "\n*** End Patch"
);
text({capturePath, rawCharacters: raw.length, saveResult: saved});
```

Do not parse/stringify the provider JSON in JavaScript and do not emit `response`,
`raw`, or the patch. The patch writer may normalize the final text newline; compare
parsed source values rather than claiming equality to original HTTP wire bytes.
Use the host's lossless parser and real schema validator on the complete local file.
Reject duplicate JSON keys, non-JSON numbers, malformed envelopes, and incomplete
arrays. Report only bounded counts, hashes, paths, and diagnostics to the model.
Keep the complete originals locally; compute date/activity gates and the selected
skill's projection there before reading the complete eligible projected batch for
one clustering invocation. Reading a file in bounded pieces must not skip data or
become separate clustering calls. If the full eligible batch still exceeds context
capacity, retain the existing `resource_limit` behavior.

If capture fails after a paid call, recover from the retained session result or a
documented complete file reference. Never repeat that request merely to repair
display truncation, never set `trim`, change the keyword, drop original fields,
or paginate to compensate. No complete recoverable response means a failed job
without a discovery file. Actual connector completeness and end-to-end discovery
still require a bounded live test after installing this adapter and its matching
generated Instructions. Keep those results separate from the local smoke test.

### Read the whole projected batch without overflowing one display

A subsequent live job captured the full search response and produced a valid
six-ad projection of 76,159 UTF-8 bytes. Its first unbounded read was about 19,040
tokens and the display omitted about 9,040 tokens. Stopping there was premature:
this was a per-read output window, not evidence of insufficient aggregate context.
A local-only recovery read all 75,207 characters in six successive ranges, then
applied the attached skill once. Fourteen local clusters passed the output schema
and traceability/coverage checks. No new search or Drive write was needed for that
diagnostic. These checks do not independently prove semantic grouping or persistence.

After validating the complete projection locally, measure its character length,
UTF-8 byte length, hash, and ad count. Decode UTF-8 before slicing so a boundary
cannot split a multibyte character. Read sequential half-open character ranges
`[start, end)` in separate tool executions, with small headers recording their
offsets and total length. Do not print all ranges inside one orchestration call:
its combined display can be truncated again. The observed test used at most 15,000
characters per read. This is a starting bound, not a universal token guarantee;
adapt to the actual tool window and reserve room for wrappers and metadata.

```python
from pathlib import Path
import sys

text = Path(projection_path).read_text(encoding="utf-8")
assert 0 <= start < end <= len(text)
print(f"PROJECTED BATCH RANGE [{start}, {end}) OF {len(text)} CHARACTERS")
sys.stdout.write(text[start:end])
```

`projection_path`, `start`, and `end` are trusted execution values. If an individual
range is truncated, discard that incomplete display and reread the same local range
in smaller pieces. This is not another paid search. Continue until the observed
gap-free, non-overlapping ranges cover `[0, total_length)`, including the final
array terminator. Verify local range reconstruction against the original text,
hash, and decoded values. Keep all original fields in the source file and all
recognized projected values unchanged.

Only after the entire batch is available in the same model context, apply the
selected skill once to that complete top-level array. The range headers are
transport bookkeeping, not extra skill inputs. Do not cluster each range or ad
separately, replace the full input with excerpts, or infer products from unseen
fields. Respect real aggregate context/output limits, but do not manufacture a
context ceiling or treat a default 10,000-token display as that ceiling. If the
complete eligible batch truly cannot fit, the core's `resource_limit` rule remains.

### Live job fails only because optional Drive fields are absent

A result reporting `folder_invalid` solely because `trashed` or
`capabilities.canAddChildren` was omitted contradicts the current agent policy.
Compare the saved Instructions and uploaded AGENT.md with the current kit. Earlier
installation instructions said "This update does not relax those checks" and
required confirmed non-trashed/add-child state; those instructions are obsolete.
Updating GitHub alone does not replace either saved instructions or uploaded files.

Use one current kit to replace both resource folders and the whole Instructions
field, keeping the existing active configuration and skill attachment. The earlier
exporter includes `UPDATE-EXISTING.txt` for this flow. Do not append a conflicting
override or hand-edit hashed source files while leaving their old manifest.
Start a fresh conversation after saving. Missing fields should yield
`drive_metadata_unavailable` and continue to the normal job; explicit denial,
wrong destination, known incomplete collision lookup or unverified persistence still fail.
Do not spend on repeated live runs merely to diagnose a stale instruction copy.

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

A successful all-type exact-name search may omit completeness metadata. Record
`drive_lookup_completeness_unavailable` and continue under the core storage policy;
do not require a fabricated completion flag. Follow exposed tokens and fail on
actual errors, known truncation or observed conflicts. This applies to live jobs,
not only setup. Updating only the Instructions while keeping old agent resources
can restore the old blocker; replace a coherent kit.
