"""Shared deterministic packaging primitives; no host-specific assumptions."""
from pathlib import Path
import hashlib, io, json, urllib.request, zipfile
FIXED_DATE = (2020, 1, 1, 0, 0, 0)

def files(root):
    for path in sorted(root.rglob("*")):
        if "__pycache__" in path.parts or path.suffix == ".pyc":
            continue
        if path.is_symlink():
            raise ValueError(f"Source symlinks are not distributable: {path}")
        if path.is_file():
            yield path


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def zip_bytes(entries):
    output = io.BytesIO()
    with zipfile.ZipFile(output, "w", compression=zipfile.ZIP_STORED) as archive:
        for name, raw in sorted(entries):
            info = zipfile.ZipInfo(name, FIXED_DATE)
            info.create_system = 3
            info.external_attr = 0o100644 << 16
            archive.writestr(info, raw)
    return output.getvalue()


def wheel_bytes(item, cache, offline):
    path = cache / item["filename"]
    if path.exists():
        raw = path.read_bytes()
    elif offline:
        raise FileNotFoundError("Missing cached wheel: " + item["filename"])
    else:
        # Source URLs and hashes are reviewed in the committed lock, not selected
        # afresh from a mutable latest-version response at each build.
        if not item["source"].startswith("https://files.pythonhosted.org/"):
            raise ValueError("Unexpected dependency source")
        with urllib.request.urlopen(item["source"], timeout=60) as response:
            raw = response.read()
    if digest(raw) != item["sha256"] or len(raw) != item["bytes"]:
        raise ValueError("Wheel checksum/length mismatch: " + item["filename"])
    with zipfile.ZipFile(io.BytesIO(raw)) as wheel:
        if wheel.testzip():
            raise ValueError("Invalid wheel: " + item["filename"])
    if not path.exists():
        path.write_bytes(raw)
    return raw
