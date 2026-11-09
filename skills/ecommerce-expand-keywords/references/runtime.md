# Runtime and deployment notes

Keep keyword generation independent of any particular tool name or programming language. Require a working country-relevant research capability and access to the packaged text/JSON resources. Do not require Python, a shell, or local file access. The JSON Schemas define the structural contract regardless of which host runs the skill.

## No-code execution path

Use the same workflow when script execution is unavailable:

1. Read `SKILL.md`, the contract/research references, and the JSON Schemas through the host's resource-reading facility.
2. Check exact keys, JSON types, required values, and country membership. Read `references/iso3166-1.json` directly to resolve alpha-2 and alpha-3 pairs. Apply omission defaults and do not silently coerce invalid input.
3. Resolve category/segment ambiguity with the approved clarification behavior. Use a JSON error for actual validation failures.
4. Perform the required country-scoped research using available native search/fetch/browse tools.
5. Generate, combine, rank, and deduplicate candidates following the same semantic rules.
6. Count and inspect the final phrases, check the success/error shape directly, and emit the required response. Do not claim a programmatic validation pass.

Do not request code execution merely to invoke the skill. Do not return a missing-Python error or skip research because the optional helper cannot run. Resource-loading failure is separate from missing script execution; report it accurately rather than guessing hidden resource contents.

## Optional developer helpers

- `scripts/validate_json.py`: optional deterministic input/output checks using Python's standard library. It does not generate keywords or conduct research.
- `scripts/test_validate_json.py`: maintainer regression tests for that helper. Do not run them as part of an ordinary keyword request.

Keep both optional. The complete instructions, contract, schemas, and country lookup remain usable if a host ignores the scripts directory. When machine validation is required downstream, choose any compatible JSON Schema implementation; Python is not prescribed.

## ChatGPT web and iOS

OpenAI documents web search on ChatGPT web and mobile, subject to usage limits and workspace controls. This does not guarantee a search tool is exposed in a particular conversation. Check actual availability and return a JSON error when research cannot be performed.

OpenAI distinguishes standalone local skills from plugin-bundled skills for web/mobile distribution. A local Codex installation does not, by itself, install the skill in ChatGPT web or iOS. Make the skill available through the account/workspace's supported skill or plugin installation route. Do not claim this archive has been uploaded, installed, approved, or synchronized across devices.

The companion `ecommerce-expand-keywords-plugin.zip` wrapper bundles exactly the same single skill in an Agent Plugins directory layout. It is an unsubmitted local package, not an approved directory listing. Complete any publisher, support, privacy, or listing metadata that the upload service requests using the user's real information; do not invent it.

## Codex

Extract the `ecommerce-expand-keywords` directory into a supported local skill location, for example:

```text
~/.agents/skills/ecommerce-expand-keywords/
```

For a repository-specific installation, use:

```text
<repository>/.agents/skills/ecommerce-expand-keywords/
```

Invoke `$ecommerce-expand-keywords` with the input JSON. Do not assume uploading a ZIP in a chat installs a local Codex skill.

Codex's documented search configuration distinguishes cached, indexed, live, and disabled modes. Where live research is desired and permitted, the user can configure:

```toml
web_search = "live"
```

The skill must not edit the user's configuration or weaken sandbox settings. It should use the exposed tool and evaluate evidence freshness, or return an error when adequate research is not possible.

## Response and validation limits

A skill is an instruction bundle, not a schema-enforcing application server. Runtime behavior can vary by model, tools, host policies, and permissions. Validate downstream JSON before consuming it. Do not count a local unit test as an end-to-end test in ChatGPT web, iOS, or Codex.

If the host has no Python execution, read the schemas and apply the contract checks directly. If the validator is run, keep its JSON diagnostic report separate from the keyword output. Do not introduce a mandatory MCP connector just to run this workflow.

## Documentation checked during preparation

These deployment references were checked on 2026-11-01. Recheck before relying on future UI or configuration behavior.

```text
https://help.openai.com/en/articles/9237897-searching-the-web-with-chatgpt
https://learn.chatgpt.com/docs/build-skills
https://learn.chatgpt.com/docs/config-file/config-basic
https://developers.openai.com/plugins/build/skills
https://developers.openai.com/plugins/concepts/skills
https://developers.openai.com/plugins/build/plugins
https://developers.openai.com/plugins/deploy/submission
```
