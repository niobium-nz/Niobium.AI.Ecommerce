"""Shared offline validation libraries; no connector or network actions.

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
    if ({item["name"] for item in selected} != set(lock["required_distributions"])
            or len(selected) != len(lock["required_distributions"])):
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
            import rfc3339_validator, six, tzdata, typing_extensions, pycountry, rfc3986_validator
            for module in (attrs, jsonschema, jsonschema_specifications, referencing, rpds,
                           rfc3339_validator, six, tzdata, typing_extensions, pycountry, rfc3986_validator):
                if not Path(module.__file__).resolve().is_relative_to(path):
                    raise ValueError("Dependency was loaded from outside the verified bundle")
            yield Draft202012Validator, FormatChecker, ValidationError, Registry, Resource, versions
        finally:
            sys.path.remove(str(path))
