"""Offline package checks through an optional filesystem/Python host adapter."""
from __future__ import annotations
import argparse
import copy
import hashlib
import io
import json
import os
from pathlib import Path
import sys
import unittest
from zoneinfo import ZoneInfo

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
from runtime import dependencies, environment, load, strict_json, target_for_host
from resource_bindings import SchemaBindings, read_contract_resources


def verify_files(root, expected):
    if root is None:
        if expected: raise ValueError("Required resource folder is not bound")
        return
    names = set()
    for item in expected:
        path = (root / item["path"]).resolve(strict=True)
        if not path.is_relative_to(root): raise ValueError("Manifest resource leaves its bound root")
        raw = path.read_bytes()
        if hashlib.sha256(raw).hexdigest() != item["sha256"]:
            raise ValueError("Resource differs from package manifest: " + item["path"])
        names.add(item["path"])
    actual = {p.relative_to(root).as_posix() for p in root.rglob("*") if p.is_file()
              and "__pycache__" not in p.parts and p.suffix != ".pyc"}
    if actual != names:
        raise ValueError("Stale or missing package resources; replace the complete folder: " + str(sorted(actual ^ names)))


def selected_skills(items):
    result = {}
    for item in items:
        name, separator, path = item.partition("=")
        if not separator or name in result: raise ValueError("Use each --skill NAME=current-resource-path once")
        result[name] = Path(path).resolve(strict=True)
    return result


def contract_resources(root):
    return read_contract_resources(root, load)


def pointer(value, path):
    if not path: return value
    if not path.startswith("/"): raise ValueError("Invalid fixture pointer")
    for token in path[1:].split("/"):
        token = token.replace("~1", "/").replace("~0", "~")
        value = value[int(token)] if isinstance(value, list) else value[token]
    return value


def fixtures(root, bindings, config):
    from jsonschema import ValidationError
    specification = load(root / "validation-cases.json")
    active = load(config)
    bindings.validator(specification["config_schema"]).validate(active)
    if "timezone" in active: ZoneInfo(active["timezone"])
    for case in specification["fixtures"]:
        bindings.validator(case["schema"]).validate(pointer(load(root / case["instance"]), case.get("pointer", "")))
    sample = specification["fixtures"][specification["invalid_input_fixture"]]
    original = pointer(load(root / sample["instance"]), sample.get("pointer", ""))
    validator = bindings.validator(sample["schema"])
    rejected = []
    for name, invalid in [("extra input field", dict(copy.deepcopy(original), unexpected=True)), ("wrong input type", [])]:
        try: validator.validate(invalid)
        except ValidationError: rejected.append(name)
        else: raise AssertionError("Invalid fixture accepted: " + name)
    try: bindings.validator(specification["config_schema"]).validate(load(root / "config.example.json"))
    except ValidationError: rejected.append("unconfigured template")
    else: raise AssertionError("Unconfigured template was accepted")
    for name, text in [("duplicate key", '{"a":1,"a":2}'), ("non-JSON number", "NaN"), ("number overflow", "1e999")]:
        try: strict_json(text)
        except ValueError: rejected.append(name)
        else: raise AssertionError("Invalid JSON accepted: " + name)
    if strict_json('9007199254740993') != 9007199254740993: raise AssertionError("Large integer rounded")
    for fmt, invalid in [("date", "2026-02-30"), ("date-time", "2026-02-30T25:00:00Z"), ("uri", "https://invalid space.example/")]:
        check = bindings.Validator({"type":"string", "format":fmt}, format_checker=bindings.Formats())
        if check.is_valid(invalid): raise AssertionError("Invalid " + fmt + " accepted")
        rejected.append("invalid " + fmt)
    return {"active_config_validated": True, "valid_fixtures": len(specification["fixtures"]) + 1,
            "invalid_cases_rejected": rejected, "large_integer_preserved": True}


def source_tests(root, bindings):
    os.environ["AGENT_SKILL_RESOURCES"] = json.dumps({name: str(path) for name, path in bindings.skills.items()})
    os.environ["AGENT_SCHEMA_RESOURCES"] = json.dumps({uri: str(path) for uri, path in bindings.aliases.items()})
    # Backward-compatible adapter for the discoverer's existing source suite.
    for name, declaration in bindings.bindings.items():
        if "resource_environment_variable" in declaration:
            os.environ[declaration["resource_environment_variable"]] = str(bindings.skills[name])
    sys.path.insert(0, str(root / "tests"))
    suite = unittest.defaultTestLoader.discover(str(root / "tests"), pattern="test_*.py")
    log = io.StringIO()
    result = unittest.TextTestRunner(stream=log, verbosity=2).run(suite)
    success = result.wasSuccessful() and result.testsRun > 0 and not result.skipped
    report = {"status": "passed" if success else "failed", "tests_run": result.testsRun,
              "failures": len(result.failures), "errors": len(result.errors), "skipped": len(result.skipped)}
    if not success: report["log"] = log.getvalue()
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["check", "validate", "environment"])
    parser.add_argument("--package", type=Path)
    parser.add_argument("--skill", action="append", default=[], metavar="NAME=CURRENT_RESOURCE_PATH")
    parser.add_argument("--contracts", type=Path)
    parser.add_argument("--config", type=Path)
    parser.add_argument("--source-tests", action="store_true")
    parser.add_argument("--schema")
    parser.add_argument("--instance", type=Path)
    args = parser.parse_args()
    report = {"environment": environment(), "network_actions": "disabled",
              "host_orchestration_acceptance": "not_executed", "connector_checks": "not_executed",
              "resource_selection": "explicit caller bindings; this helper cannot authenticate host identity"}
    try:
        if args.command == "environment":
            report.update(status="compatible", target=target_for_host())
        else:
            if args.package is None: raise ValueError("--package is required")
            root = args.package.resolve(strict=True)
            contracts = args.contracts.resolve(strict=True) if args.contracts else None
            manifest = load(HERE / "deployment-manifest.json")
            verify_files(root, manifest["agent_files"])
            verify_files(contracts, manifest["contract_files"])
            skills = selected_skills(args.skill)
            with dependencies() as api:
                bindings = SchemaBindings(root, skills, contract_resources(contracts))
                report.update(bindings.check_definitions())
                report.update({"package_version":manifest["package_version"], "agent_files_verified":len(manifest["agent_files"]),
                               "selected_skills":list(bindings.skills), "runtime_skill_version_pin":None,
                               "dependency_versions":api[-1], "validator":"Draft202012Validator with FormatChecker and local-only Registry"})
                if args.command == "check":
                    if args.config is None: raise ValueError("--config is required")
                    report.update(fixtures(root, bindings, args.config))
                    report["source_unit_tests"] = source_tests(root, bindings) if args.source_tests else {"status":"not_run"}
                    report["status"] = "failed" if report["source_unit_tests"]["status"] == "failed" else "passed"
                else:
                    if args.schema is None or args.instance is None: raise ValueError("--schema and --instance are required")
                    errors = list(bindings.validator(args.schema).iter_errors(load(args.instance)))
                    report.update(status="invalid" if errors else "valid", error_count=len(errors),
                                  errors=[{"path":list(e.absolute_path),"message":e.message} for e in errors[:20]])
    except Exception as error:
        report.update(status="failed", error_type=type(error).__name__, message=str(error))
    print(json.dumps(report, indent=2))
    return 0 if report["status"] in {"passed", "valid", "compatible"} else 1


if __name__ == "__main__":
    raise SystemExit(main())
