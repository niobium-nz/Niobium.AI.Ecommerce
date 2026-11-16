"""Build platform-neutral agent kits; emit a separate deployment ZIP per skill."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import shutil
import tempfile
from urllib.parse import unquote, urlparse
from build_common import files, digest, write_json, zip_bytes, wheel_bytes

REPO = Path(__file__).resolve().parents[2]
SUPPORT = Path(__file__).resolve().parent / "validation-support"


def source_schemas():
    result = {}
    for category in ("agents", "skills"):
        for owner in (REPO / category).iterdir():
            if not owner.is_dir(): continue
            for path in (owner / "schemas").glob("*.schema.json"):
                result[f"urn:niobium:{owner.name}:schema:{path.name.removesuffix('.schema.json')}"] = path.resolve()
    return result


def references(value):
    if isinstance(value, dict):
        if "$ref" in value: yield value["$ref"]
        for child in value.values(): yield from references(child)
    elif isinstance(value, list):
        for child in value: yield from references(child)


def contract_closure(agent):
    catalog = source_schemas()
    binding_file = agent / "contract-bindings.json"
    pending = [catalog[uri] for uri in json.loads(binding_file.read_text()).values()] if binding_file.exists() else []
    found = set()
    while pending:
        path = pending.pop().resolve(strict=True)
        if path in found: continue
        if not path.is_relative_to(REPO): raise ValueError("Contract source leaves repository")
        found.add(path)
        for ref in references(json.loads(path.read_text())):
            target = ref.partition("#")[0]
            if not target: continue
            if target in catalog: pending.append(catalog[target])
            elif urlparse(target).scheme: raise ValueError("Unbound external schema: " + target)
            else: pending.append(path.parent / unquote(target))
    return {uri: path for uri, path in catalog.items() if path in found}, found


def build(name, output, cache, offline=False, adapter=None):
    agent = REPO / "agents" / name
    if name not in available_agents(): raise ValueError("Unknown agent: " + name)
    if output.exists(): raise FileExistsError("Choose a fresh --output directory; existing kits may contain edited settings")
    if (agent / "skills").exists(): raise ValueError("Agent source contains a duplicate skill tree")
    bindings = json.loads((agent / "skill-bindings.json").read_text())
    version = re.search(r"^version: ([0-9.]+)$", (agent / "agent.yaml").read_text(), re.M).group(1)
    output.parent.mkdir(parents=True, exist_ok=True)
    cache.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="agent-kit-", dir=output.parent) as temporary:
        stage = Path(temporary) / "kit"
        resources = stage / "agent-files"
        target = resources / name
        support = resources / "offline-validation-support"
        support.mkdir(parents=True)
        inventory = []
        for path in files(agent):
            relative, raw = path.relative_to(agent), path.read_bytes()
            dest = target / relative
            dest.parent.mkdir(parents=True, exist_ok=True); dest.write_bytes(raw)
            inventory.append({"path": relative.as_posix(), "sha256": digest(raw), "bytes": len(raw)})
        lock = json.loads((SUPPORT / "dependency-lock.json").read_text())
        archive = zip_bytes([(item["filename"], wheel_bytes(item, cache, offline)) for item in lock["wheels"]])
        (support / lock["archive"]).write_bytes(archive)
        lock["archive_sha256"] = digest(archive)
        write_json(support / "dependency-lock.json", lock)
        for file in ("validate.py", "runtime.py", "resource_bindings.py", "metadata_policy.py", "README.md"):
            shutil.copyfile(SUPPORT / file, support / file)
        aliases, contracts = contract_closure(agent)
        contract_inventory = []
        if contracts:
            contract_root = resources / "contract-resources"
            for path in sorted(contracts):
                relative, raw = path.relative_to(REPO), path.read_bytes()
                dest = contract_root / relative
                dest.parent.mkdir(parents=True, exist_ok=True); dest.write_bytes(raw)
                contract_inventory.append({"path": relative.as_posix(), "sha256": digest(raw)})
            write_json(contract_root / "schema-map.json", {uri: path.relative_to(REPO).as_posix() for uri, path in aliases.items()})
            contract_inventory.append({"path": "schema-map.json", "sha256": digest((contract_root / "schema-map.json").read_bytes())})
        write_json(support / "deployment-manifest.json", {"agent": name, "package_version": version,
                   "agent_files": inventory, "contract_files": contract_inventory,
                   "skills": list(bindings), "runtime_skill_version_pin": None})
        shutil.copyfile(agent / "config.example.json", resources / "deployment-config.json")
        if bindings:
            (stage / "skills").mkdir()
            for skill in bindings:
                root = REPO / "skills" / skill
                raw = zip_bytes([((Path(skill) / p.relative_to(root)).as_posix(), p.read_bytes()) for p in files(root)])
                (stage / "skills" / (skill + ".zip")).write_bytes(raw)
        shutil.copyfile(agent / "AGENT.md", stage / "INSTRUCTIONS.md")
        (stage / "START-HERE.md").write_text(start_here(name, list(bindings), bool(contracts)), encoding="utf-8")
        if adapter:
            if adapter not in {"chatgpt", "claude"}: raise ValueError("Unknown adapter")
            filename = "chatgpt-workspace-agent.md" if adapter == "chatgpt" else "claude-cowork.md"
            shutil.copyfile(agent / "adapters" / filename, stage / ("SETUP-" + adapter + ".md"))
        all_files = [{"path": p.relative_to(stage).as_posix(), "sha256": digest(p.read_bytes())} for p in files(stage)]
        write_json(stage / "kit-manifest.json", {"agent": name, "package_version": version,
                   "adapter": adapter, "skill_archives": ["skills/" + skill + ".zip" for skill in bindings],
                   "files": all_files, "note": "Distribution integrity only, not a runtime skill pin or hosted deployment."})
        stage.rename(output)
    return {"agent": name, "output": str(output), "package_version": version, "skill_archives": len(bindings), "contract_resources": len(contracts)}


def start_here(name, skills, contracts):
    skill_text = "\n".join("- `skills/" + skill + ".zip`" for skill in skills) if skills else "This agent has no directly invoked skills, so its kit contains no skill ZIPs."
    return f'''# Install {name}

This is a platform-neutral resource kit, not a vendor-native import manifest.
The folder name `agent-files` is a distribution convention, not a required host UI
or filesystem path. Map these resources through the host's supported APIs, folders,
project context, plugin or agent configuration. Never persist temporary mount paths.

1. Use INSTRUCTIONS.md (the canonical AGENT.md content) as the agent instructions.
2. Make agent-files/{name} readable with relative resource paths preserved or mapped.
3. Configure the actual resource IDs/host bindings in agent-files/deployment-config.json.
   The null placeholders are intentionally invalid. Keep one active configuration.
4. Select each required skill independently through the host's supported mechanism.
   Install each ZIP below separately if the host uses skill ZIPs. Do not submit the
   enclosing kit or a combined multi-skill archive as one skill. Folder/resource API
   hosts may bind each canonical skill directory instead. No nested duplicate is needed.

{skill_text}

5. {'Bind agent-files/contract-resources using its schema-map.json. These are generated schema resources only; they do not install or invoke a child agent. Provision the complete researcher separately.' if contracts else 'Register skill-bindings.json logical schema URIs against the resources of the explicitly selected skills.'}
6. Map the agent's required capabilities and run the offline setup checks described
   in its references/deployment.md. Python validation support is supplied as an
   optional adapter under agent-files/offline-validation-support. Its Linux wheels
   are not a requirement on other platforms; an equivalent real validator may be used.
7. Distinguish offline validation from live host execution. An unavailable host mock
   harness is reported as untested. Actual missing operational capabilities still
   block live work. Setup does not run paid research, browsing jobs or cloud writes.

Read the relevant adapter under agent-files/{name}/adapters only when deploying to
that host. No ChatGPT-specific controls or permissions are required by the core.

For updates, build a fresh kit. Replace the agent/support and generated contract
resources together, preserve one active deployment configuration, and update each
skill independently when intended. Revalidate selected schemas after skill changes.
Do not merge obsolete files, reintroduce nested skills or assume GitHub updates an
existing hosted agent. The kit manifest checks download integrity, not host identity.
'''


def available_agents():
    return sorted(p.name for p in (REPO / "agents").iterdir() if (p / "agent.yaml").is_file())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--agent", choices=["all"] + available_agents(), default="all")
    parser.add_argument("--output", type=Path, help="Fresh output folder; with all, each agent gets a child folder")
    parser.add_argument("--wheel-cache", type=Path, default=REPO / ".cache/discoverer-wheels")
    parser.add_argument("--offline", action="store_true")
    parser.add_argument("--adapter", choices=["chatgpt", "claude"], help="Optionally add a host-specific setup guide")
    args = parser.parse_args()
    names = available_agents() if args.agent == "all" else [args.agent]
    base = args.output or REPO / "dist/portable-agents"
    for name in names:
        output = base / name if args.agent == "all" or args.output is None else base
        print(json.dumps(build(name, output.resolve(), args.wheel_cache.resolve(), args.offline, args.adapter)))


if __name__ == "__main__":
    main()
