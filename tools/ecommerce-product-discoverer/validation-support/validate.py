"""Offline Draft 2020-12 validation support; no connector or network actions.

check runs package checks and optionally the supplied deterministic Python tests.
validate checks one JSON file against an agent or current host-selected skill schema.
Neither command is a production orchestration runner or a host mock harness.
"""
from __future__ import annotations

import argparse
from contextlib import contextmanager
import copy
from datetime import datetime, timezone
import hashlib
import importlib.metadata
import io
import json
import math
import os
from pathlib import Path, PurePosixPath
import platform
import sys
import sysconfig
import tempfile
import unittest
import zipfile
from zoneinfo import ZoneInfo

HERE = Path(__file__).resolve().parent
sys.dont_write_bytecode = True


def no_network(event, args):
    if event.startswith("socket.") or event in {"subprocess.Popen", "os.system"}:
        raise RuntimeError("Network and subprocess actions are disabled in this validation process")


sys.addaudithook(no_network)


def strict_json(text):
    def pairs(items):
        result = {}
        for key, value in items:
            if key in result:
                raise ValueError("Duplicate JSON object key: " + key)
            result[key] = value
        return result

    def invalid(value):
        raise ValueError("Invalid JSON number: " + value)

    def finite(value):
        number = float(value)
        if not math.isfinite(number):
            invalid(value)
        return number

    return json.loads(text, object_pairs_hook=pairs, parse_constant=invalid, parse_float=finite)


def load(path):
    return strict_json(Path(path).read_text(encoding="utf-8"))


def environment():
    return {"python": platform.python_version(), "implementation": platform.python_implementation(),
            "system": platform.system(), "machine": platform.machine(),
            "libc": list(platform.libc_ver()),
            "gil_disabled": bool(sysconfig.get_config_var("Py_GIL_DISABLED"))}


def target_for_host():
    env = environment()
    if env["implementation"] != "CPython" or env["system"] != "Linux" or env["gil_disabled"]:
        raise RuntimeError("This dependency bundle needs a standard CPython build on Linux; see environment details")
    if sys.version_info[:2] not in {(3, 11), (3, 12), (3, 13), (3, 14)}:
        raise RuntimeError("This dependency bundle supports Python 3.11 through 3.14")
    arch = {"x86_64": "x86_64", "aarch64": "aarch64", "arm64": "aarch64"}.get(env["machine"])
    libc, version = env["libc"]
    if arch is None or libc != "glibc" or tuple(map(int, version.split(".")[:2])) < (2, 17):
        raise RuntimeError("This dependency bundle needs Linux glibc >= 2.17 on x86_64 or aarch64")
    return f"linux-{arch}-cp{sys.version_info.major}{sys.version_info.minor}"


def extract_wheel(raw, destination):
    with zipfile.ZipFile(io.BytesIO(raw)) as wheel:
        for member in wheel.infolist():
            path = PurePosixPath(member.filename)
            if path.is_absolute() or ".." in path.parts or "\\" in member.filename:
                raise ValueError("Unsafe path in dependency archive")
            if member.is_dir():
                continue
            out = destination.joinpath(*path.parts)
            out.parent.mkdir(parents=True, exist_ok=True)
            out.write_bytes(wheel.read(member))


@contextmanager
def dependencies():
    target = target_for_host()
    lock = load(HERE / "dependency-lock.json")
    archive = HERE / lock["archive"]
    if hashlib.sha256(archive.read_bytes()).hexdigest() != lock["archive_sha256"]:
        raise ValueError("Dependency archive checksum mismatch")
    selected = [item for item in lock["wheels"] if item["target"] in {"any", target}]
    if len(selected) != 9 or sum(item["name"] == "rpds-py" for item in selected) != 1:
        raise ValueError("No complete dependency set for this runtime")
    with tempfile.TemporaryDirectory(prefix="discoverer-validator-") as temp:
        path = Path(temp)
        with zipfile.ZipFile(archive) as bundle:
            for item in selected:
                raw = bundle.read(item["filename"])
                if hashlib.sha256(raw).hexdigest() != item["sha256"]:
                    raise ValueError("Wheel checksum mismatch: " + item["filename"])
                extract_wheel(raw, path)
        sys.path.insert(0, str(path))
        try:
            versions = {}
            for item in selected:
                version = importlib.metadata.version(item["name"])
                if version != item["version"]:
                    raise ValueError("Unexpected dependency version for " + item["name"])
                versions[item["name"]] = version
            # Ensure installed system modules cannot silently replace the bundle.
            from jsonschema import Draft202012Validator, FormatChecker, ValidationError
            from referencing import Registry, Resource
            import attrs, jsonschema, jsonschema_specifications, referencing, rpds
            import rfc3339_validator, six, tzdata, typing_extensions
            for module in (attrs, jsonschema, jsonschema_specifications, referencing, rpds,
                           rfc3339_validator, six, tzdata, typing_extensions):
                if not Path(module.__file__).resolve().is_relative_to(path):
                    raise ValueError("Dependency was loaded from outside the verified bundle")
            yield Draft202012Validator, FormatChecker, ValidationError, Registry, Resource, versions
        finally:
            sys.path.remove(str(path))


def verify_source(root, skill_root):
    manifest = load(HERE / "deployment-manifest.json")
    if skill_root.is_relative_to(root) or root.is_relative_to(skill_root):
        raise ValueError("Selected skill resources must have their own root outside the agent package")
    if (root / "skills").exists() or (root / "skills").is_symlink():
        raise ValueError("Nested skills directory found; this deployment uses one host-selected skill")
    for item in manifest["agent"]["files"]:
        path = (root / item["path"]).resolve()
        if not path.is_relative_to(root):
            raise ValueError("Agent resource path is outside its bound root")
        raw = path.read_bytes()
        if len(raw) != item["bytes"] or hashlib.sha256(raw).hexdigest() != item["sha256"]:
            raise ValueError("Agent resource differs from the deployment manifest: " + item["path"])
    expected = {item["path"] for item in manifest["agent"]["files"]}
    actual = {path.relative_to(root).as_posix() for path in root.rglob("*")
              if path.is_file() and "__pycache__" not in path.parts and path.suffix != ".pyc"}
    if actual != expected:
        raise ValueError("Agent resource inventory differs from manifest; replace the complete folder. "
                         + str({"extra": sorted(actual - expected), "missing": sorted(expected - actual)}))
    binding = load(root / "skill-bindings.json")["ecommerce-cluster-ads"]
    resources = [binding["entrypoint"], *[item["path"] for item in binding["schemas"].values()]]
    for relative in resources:
        path = (skill_root / relative).resolve()
        if not path.is_relative_to(skill_root):
            raise ValueError("Skill resource is outside its selected root")
        if not path.is_file():
            raise FileNotFoundError("Selected skill resource is missing: " + relative)
        path.read_text(encoding="utf-8")
    return {"package_version": manifest["package_version"],
            "source_digest": manifest["source_digest"],
            "agent_files_verified": len(manifest["agent"]["files"]),
            "skill_selection": binding["selection"],
            "skill_version_pin": None, "skill_resources_read": resources,
            "resource_selection": "provided by caller; this offline helper cannot authenticate host attachment identity",
            "nested_skill_copy": False}


class SchemaChecks:
    def __init__(self, root, skill_root, api):
        self.root = root
        self.skill_root = skill_root
        self.binding = load(root / "skill-bindings.json")["ecommerce-cluster-ads"]
        self.Validator, self.FormatChecker, self.ValidationError, Registry, Resource, self.versions = api
        self.aliases = {item["uri"]: (skill_root / item["path"]).resolve()
                        for item in self.binding["schemas"].values()}
        paths = list(root.glob("schemas/*.schema.json")) + list(self.aliases.values())
        self.schemas = {path.resolve(): load(path) for path in paths}
        resources = [(path.as_uri(), Resource.from_contents(schema)) for path, schema in self.schemas.items()]
        resources.extend((uri, Resource.from_contents(self.schemas[path])) for uri, path in self.aliases.items())
        self.registry = Registry().with_resources(resources)
        self.formats = self.FormatChecker()
        self.references = 0
        for path, schema in self.schemas.items():
            self.Validator.check_schema(schema)
            self.check_references_and_formats(path, schema)

    def check_references_and_formats(self, path, value):
        if isinstance(value, dict):
            if "$ref" in value:
                self.registry.resolver(base_uri=path.as_uri()).lookup(value["$ref"])
                self.references += 1
            if "format" in value and value["format"] not in self.formats.checkers:
                raise ValueError("No validator for required format: " + value["format"])
            for child in value.values():
                self.check_references_and_formats(path, child)
        elif isinstance(value, list):
            for child in value:
                self.check_references_and_formats(path, child)

    def validator(self, relative_path, fragment=""):
        path = self.aliases.get(relative_path)
        if path is None:
            path = (self.root / relative_path).resolve()
        if path not in self.schemas:
            raise ValueError("Schema must be a verified agent schema or a bound selected-skill schema identifier")
        if fragment and not fragment.startswith("#"):
            raise ValueError("Schema fragment must begin with #")
        return self.Validator({"$ref": path.as_uri() + fragment}, registry=self.registry,
                              format_checker=self.formats)

    def expect_invalid(self, schema, value, name):
        try:
            self.validator(schema).validate(value)
        except self.ValidationError:
            return name
        raise AssertionError("Invalid fixture was accepted: " + name)


def package_checks(root, config, checks, source_tests):
    skill_root = checks.skill_root
    skill_schemas = checks.binding["schemas"]
    fixtures = [("schemas/input.schema.json", root / "examples/input.json"),
                ("schemas/handoff.schema.json", root / "examples/handoff.json"),
                ("schemas/result.schema.json", root / "examples/result.json"),
                (skill_schemas["output"]["uri"], root / "examples/clusters.json")]
    for kind, names in {"input": ["minimal", "multi-product"],
                        "output": ["minimal", "multi-product", "error"]}.items():
        fixtures.extend((skill_schemas[kind]["uri"], skill_root / f"examples/{kind}.{name}.json")
                        for name in names)
    for schema, fixture in fixtures:
        checks.validator(schema).validate(load(fixture))
    active_config = load(config)
    checks.validator("schemas/config.schema.json").validate(active_config)
    ZoneInfo(active_config["timezone"])
    invalid = []
    inputs = load(root / "examples/input.json")
    for name, value in [("extra input field", dict(inputs, unexpected=True)),
                        ("wrong input type", dict(inputs, keyword=1)),
                        ("blank keyword", dict(inputs, keyword="  "))]:
        invalid.append(checks.expect_invalid("schemas/input.schema.json", value, name))
    handoff = load(root / "examples/handoff.json")
    for name, field, value in [("wrong lifecycle", "lifecycle_stage", "candidate"),
                               ("legacy version", "schema_version", "1.0.0"),
                               ("missing evidence", "ads", [])]:
        mutated = copy.deepcopy(handoff)
        mutated[0][field] = value
        invalid.append(checks.expect_invalid("schemas/handoff.schema.json", mutated, name))
    for name, field, value in [("invalid calendar date", "cutoff_date", "2026-02-30"),
                               ("invalid timestamp", "run_started_at", "not-a-timestamp"),
                               ("impossible timestamp", "run_started_at", "2026-02-30T12:00:00Z")]:
        mutated = copy.deepcopy(handoff)
        mutated[0]["provenance"][field] = value
        invalid.append(checks.expect_invalid("schemas/handoff.schema.json", mutated, name))
    invalid.append(checks.expect_invalid("schemas/config.schema.json", load(root / "config.example.json"),
                                         "unconfigured example is not active configuration"))
    for name, raw in [("duplicate JSON key", '{"a":1,"a":2}'), ("non-JSON number", "NaN"),
                      ("number overflow", "1e999")]:
        try:
            strict_json(raw)
        except ValueError:
            invalid.append(name)
        else:
            raise AssertionError("Invalid JSON accepted: " + name)
    if strict_json('{"n":9007199254740993}')["n"] != 9007199254740993:
        raise AssertionError("Large integer was rounded")
    report = {"status": "passed", "schemas_validated": len(checks.schemas),
              "local_references_resolved": checks.references, "valid_fixtures": len(fixtures) + 1,
              "invalid_cases_rejected": invalid, "large_integer_preserved": True,
              "active_config_validated": True,
              "source_unit_tests": {"status": "not_run"}}
    if source_tests:
        environment_key = checks.binding["resource_environment_variable"]
        previous_skill = os.environ.get(environment_key)
        os.environ[environment_key] = str(skill_root)
        sys.path.insert(0, str(root / "tests"))
        try:
            suite = unittest.defaultTestLoader.discover(str(root / "tests"), pattern="test_*.py")
            log = io.StringIO()
            result = unittest.TextTestRunner(stream=log, verbosity=2).run(suite)
        finally:
            sys.path.remove(str(root / "tests"))
            if previous_skill is None:
                os.environ.pop(environment_key, None)
            else:
                os.environ[environment_key] = previous_skill
        report["source_unit_tests"] = {"status": "passed" if result.wasSuccessful() else "failed",
                                       "tests_run": result.testsRun, "failures": len(result.failures),
                                       "errors": len(result.errors), "skipped": len(result.skipped)}
        if not result.wasSuccessful() or result.testsRun == 0 or result.skipped:
            report["status"] = "failed"
            report["source_unit_tests"]["log"] = log.getvalue()
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("environment")
    check = commands.add_parser("check")
    check.add_argument("--package", type=Path, required=True)
    check.add_argument("--skill-resources", dest="skill_root", type=Path, required=True,
                       help="Resources exposed for the host-selected skill in the current execution only")
    check.add_argument("--config", type=Path, required=True)
    check.add_argument("--source-tests", action="store_true")
    validate = commands.add_parser("validate")
    validate.add_argument("--package", type=Path, required=True)
    validate.add_argument("--skill-resources", dest="skill_root", type=Path, required=True,
                          help="Resources exposed for the host-selected skill in the current execution only")
    validate.add_argument("--schema", required=True)
    validate.add_argument("--instance", type=Path, required=True)
    validate.add_argument("--fragment", default="")
    args = parser.parse_args()
    report = {"environment": environment(), "network_actions": "disabled",
              "host_orchestration_acceptance": "not_executed", "connector_checks": "not_executed"}
    try:
        if args.command == "environment":
            report["status"] = "compatible" if target_for_host() else "unsupported"
        else:
            root = args.package.resolve(strict=True)
            skill_root = args.skill_root.resolve(strict=True)
            report["source"] = verify_source(root, skill_root)
            report["support_version"] = load(HERE / "dependency-lock.json")["support_version"]
            with dependencies() as api:
                checks = SchemaChecks(root, skill_root, api)
                report["validator"] = "jsonschema.Draft202012Validator with FormatChecker and local-only Registry"
                report["dependency_versions"] = checks.versions
                if args.command == "check":
                    report.update(package_checks(root, args.config, checks, args.source_tests))
                else:
                    instance = load(args.instance)
                    errors = list(checks.validator(args.schema, args.fragment).iter_errors(instance))
                    report.update({"status": "invalid" if errors else "valid", "schema": args.schema,
                                   "error_count": len(errors),
                                   "errors": [{"instance_path": list(e.absolute_path),
                                               "schema_path": list(e.absolute_schema_path),
                                               "message": e.message} for e in errors[:20]],
                                   "scope": "JSON Schema only; workflow and semantic checks still required"})
    except Exception as error:
        report.update({"status": "failed", "error_type": type(error).__name__, "message": str(error)})
    report["completed_at"] = datetime.now(timezone.utc).isoformat()
    print(json.dumps(report, indent=2, ensure_ascii=False))
    return 0 if report["status"] in {"compatible", "passed", "valid"} else 1


if __name__ == "__main__":
    raise SystemExit(main())
