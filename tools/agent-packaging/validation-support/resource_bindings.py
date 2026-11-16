"""Optional filesystem adapter for explicit, execution-scoped resource bindings.

The agent contracts do not require this adapter, Python, environment variables or
any particular host layout. Callers select resources; this module never discovers
skills globally or authenticates which host attachment was selected.
"""
from __future__ import annotations
import copy
import json
import os
import re
from pathlib import Path
from urllib.parse import urljoin


def schema_uri(owner, filename):
    return f"urn:niobium:{owner}:schema:{filename.removesuffix('.schema.json')}"


class SchemaBindings:
    def __init__(self, root, skill_roots=None, schema_resources=None):
        from jsonschema import Draft202012Validator, FormatChecker
        from referencing import Registry, Resource
        from runtime import load
        self.root = Path(root).resolve(strict=True)
        self.Validator, self.Formats = Draft202012Validator, FormatChecker
        self.agent_name = re.search(r"^name: ([a-z0-9_-]+)$", (self.root / "agent.yaml").read_text(), re.M).group(1)
        self.bindings = load(self.root / "skill-bindings.json")
        skill_roots = skill_roots if skill_roots is not None else json.loads(os.environ.get("AGENT_SKILL_RESOURCES", "{}"))
        schema_resources = schema_resources if schema_resources is not None else json.loads(os.environ.get("AGENT_SCHEMA_RESOURCES", "{}"))
        if set(skill_roots) != set(self.bindings):
            raise ValueError("Supply exactly the declared skills as current resource bindings: " + ", ".join(self.bindings))
        self.skills = {name: Path(path).resolve(strict=True) for name, path in skill_roots.items()}
        self.aliases = {uri: Path(path).resolve(strict=True) for uri, path in schema_resources.items()}
        self.schema_paths = set(self.root.glob("schemas/*.schema.json"))
        for path in self.schema_paths:
            self.aliases[schema_uri(self.agent_name, path.name)] = path
        for name, binding in self.bindings.items():
            self.skill(name, binding["entrypoint"]).read_text(encoding="utf-8")
            self.schema_paths.update(self.skills[name].glob("schemas/*.schema.json"))
            for schema in binding["schemas"].values():
                self.aliases[schema["uri"]] = self.skill(name, schema["path"])
        required = self.root / "contract-bindings.json"
        if required.exists():
            missing = set(load(required).values()) - self.aliases.keys()
            if missing:
                raise ValueError("Missing explicit contract resource bindings: " + ", ".join(sorted(missing)))
        self.schema_paths.update(self.aliases.values())
        self.schema_paths = sorted({p.resolve(strict=True) for p in self.schema_paths})
        self.schemas = {path: load(path) for path in self.schema_paths}
        resources = [(p.as_uri(), Resource.from_contents(s)) for p, s in self.schemas.items()]
        # Logical URI aliases retain the schema's local relative-file semantics.
        # Source bytes are never modified; fragments continue to address the schema.
        for uri, path in self.aliases.items():
            schema = copy.deepcopy(self.schemas[path])
            def absolute_refs(node):
                if isinstance(node, dict):
                    ref = node.get("$ref")
                    if isinstance(ref, str) and not ref.startswith("#"):
                        node["$ref"] = urljoin(path.as_uri(), ref)
                    for value in node.values(): absolute_refs(value)
                elif isinstance(node, list):
                    for value in node: absolute_refs(value)
            absolute_refs(schema)
            resources.append((uri, Resource.from_contents(schema)))
        self.registry = Registry().with_resources(resources)

    def skill(self, name, relative=""):
        path = (self.skills[name] / relative).resolve(strict=True)
        if not path.is_relative_to(self.skills[name]):
            raise ValueError("Resource leaves selected skill root")
        return path

    def path(self, uri):
        return self.aliases[uri.split("#", 1)[0]]

    def validator(self, schema="schemas/contracts.schema.json", fragment=""):
        value = str(schema)
        base, separator, pointer = value.partition("#")
        if separator:
            if fragment: raise ValueError("Specify one schema fragment")
            fragment = "#" + pointer
        if base in self.aliases:
            uri = base
        else:
            path = (self.root / base).resolve()
            if path not in self.schemas:
                raise ValueError("Schema is not an explicitly bound local resource: " + base)
            uri = path.as_uri()
        return self.Validator({"$ref": uri + fragment}, registry=self.registry, format_checker=self.Formats())

    def check_definitions(self):
        count = 0
        formats = self.Formats()
        for path, schema in self.schemas.items():
            self.Validator.check_schema(schema)
            def walk(node):
                nonlocal count
                if isinstance(node, dict):
                    if "$ref" in node:
                        self.registry.resolver(base_uri=path.as_uri()).lookup(node["$ref"])
                        count += 1
                    if "format" in node and node["format"] not in formats.checkers:
                        raise ValueError("Missing format checker: " + node["format"])
                    for value in node.values(): walk(value)
                elif isinstance(node, list):
                    for value in node: walk(value)
            walk(schema)
        return {"schemas_validated": len(self.schemas), "local_references_resolved": count}
