"""Build atomic Google Sheets requests for per-ID duplicate claims.

This module performs no I/O. The caller validates the workflow input, executes
the requests through authorized tools, and verifies the state before proceeding.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path
from uuid import UUID


STATES = ("held", "publishing", "processed", "released")
OPERATIONS = ("claim", "prepare", "finish", "release", "release_publishing", "seed", "claim_job_folder")


def _namespace_key(namespace, external_id):
    if not re.fullmatch(r"[a-z][a-z0-9_]{0,31}", namespace):
        raise ValueError("Invalid duplicate namespace")
    if not isinstance(external_id, str) or not external_id.strip():
        raise ValueError("Duplicate IDs must be nonblank strings")
    return hashlib.sha256((namespace + "\0" + external_id).encode("utf-8")).hexdigest()


def _uuid(value):
    if not isinstance(value, str):
        raise ValueError("An exact lowercase UUIDv4 is required")
    parsed = UUID(value)
    if parsed.version != 4 or str(parsed) != value:
        raise ValueError("An exact lowercase UUIDv4 is required")
    return parsed.hex


def _digest(parts):
    return hashlib.sha256("\0".join(parts).encode("ascii")).hexdigest()


def _range(sheet_id, state):
    index = STATES.index(state)
    return {"sheetId": sheet_id, "startRowIndex": index, "endRowIndex": index + 1,
            "startColumnIndex": 0, "endColumnIndex": 1}


def _add(sheet_id, identity, name, state):
    return {"addNamedRange": {"namedRange": {"namedRangeId": identity, "name": name,
            "range": _range(sheet_id, state)}}}


def _set(sheet_id, identity, state):
    return {"updateNamedRange": {"namedRange": {"namedRangeId": identity,
            "range": _range(sheet_id, state)}, "fields": "range"}}


def _delete(identity):
    return {"deleteNamedRange": {"namedRangeId": identity}}


def record_filename(namespace, external_id):
    key = _namespace_key(namespace, external_id)
    safe = re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]{0,119}", external_id)
    return f"{external_id}.json" if safe else f"_sha256_{key}.json"


def read_legacy_ids(path, format_name):
    """Read previously verified bytes for a one-time migration."""
    text = Path(path).read_bytes().decode("utf-8")
    if format_name == "json_string_array":
        values = json.loads(text)
        if not isinstance(values, list):
            raise ValueError("Expected a JSON string array")
    elif format_name == "newline_ids":
        values = [line.removesuffix("\r") for line in text.split("\n") if line.strip()]
        if any("\r" in line for line in values):
            raise ValueError("Invalid newline registry record")
    else:
        raise ValueError("Unsupported legacy ID format")
    if any(not isinstance(value, str) or not value.strip() for value in values):
        raise ValueError("Legacy IDs must be nonblank strings")
    return list(dict.fromkeys(values))


def make_plan(operation, namespace, ids, sheet_id, *, attempt_id=None, job_id=None,
              signal_id=None, artifact_sha256=None):
    """Return one indivisible batch and the exact named ranges to read back."""
    if type(sheet_id) is not int or not 0 <= sheet_id <= 2**31 - 1:
        raise ValueError("Invalid claim sheet ID")
    if operation not in OPERATIONS or not isinstance(ids, list):
        raise ValueError("Invalid duplicate claim operation")
    if operation == "claim_job_folder":
        job, owner = _uuid(job_id), _uuid(attempt_id)
        identity = f"dup_job_folder_{job}"
        receipt = f"{identity}_{owner}"
        return {"requests": [_add(sheet_id, identity, receipt, "held")],
                "receipt": receipt, "expected_state": "held"}
    keys = [_namespace_key(namespace, value) for value in ids]
    if len(set(keys)) != len(keys) or (not keys and operation != "seed"):
        raise ValueError("Expected unique duplicate IDs")
    ranges = [f"dup_id_{key}" for key in keys]
    plan = {"records": [
        {"id": value, "range": identity, "filename": record_filename(namespace, value)}
        for value, identity in zip(ids, ranges)
    ]}
    if operation == "seed":
        return {**plan, "requests": [_add(sheet_id, identity, identity, "processed") for identity in ranges],
                "receipt": None, "publication_receipt": None, "expected_state": "processed"}

    owner, job = _uuid(attempt_id), _uuid(job_id)
    batch = _digest(sorted(keys))
    context = _digest((namespace, owner, job, batch))
    receipt = "dup_run_" + _digest((namespace, owner))
    receipt_name = f"dup_run_{owner}_{job}_{batch}"
    held = "dup_held_" + context
    publishing = None
    publication_receipt = None
    if operation in ("prepare", "finish", "release_publishing"):
        signal = _uuid(signal_id)
        if not isinstance(artifact_sha256, str) or not re.fullmatch(r"[a-f0-9]{64}", artifact_sha256):
            raise ValueError("Expected the exact validated artifact SHA-256")
        publishing = "dup_pub_" + _digest((context, signal, artifact_sha256))
        publication_receipt = f"dup_pub_{owner}_{signal}_{artifact_sha256}"

    if operation == "claim":
        state = "held"
        requests = [_add(sheet_id, receipt, receipt_name, state), _add(sheet_id, held, held, state)]
        requests.extend(_add(sheet_id, identity, identity, state) for identity in ranges)
        return {**plan, "requests": requests, "receipt": receipt_name,
                "publication_receipt": None, "expected_state": state}

    # Updating a missing named range fails the whole batch. Deletion alone is a
    # no-op for a missing range, so every transition checks its own prior fence.
    expected = held if operation in ("prepare", "release") else publishing
    state = {"prepare": "publishing", "finish": "processed",
             "release": "released", "release_publishing": "released"}[operation]
    requests = [_set(sheet_id, expected, state), _delete(expected)]
    if operation == "prepare":
        requests.append(_add(sheet_id, publishing, publication_receipt, state))
    requests.append(_set(sheet_id, receipt, state))
    if state == "released":
        requests.extend(_delete(identity) for identity in ranges)
    else:
        requests.extend(_set(sheet_id, identity, state) for identity in ranges)
    return {**plan, "requests": requests, "receipt": receipt_name,
            "publication_receipt": publication_receipt, "expected_state": state}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("operation", choices=OPERATIONS)
    parser.add_argument("--namespace", required=True)
    parser.add_argument("--sheet-id", type=int, required=True)
    inputs = parser.add_mutually_exclusive_group(required=True)
    inputs.add_argument("--input", type=Path, help="Validated researcher input containing job and ads")
    inputs.add_argument("--ids", type=Path, help="UTF-8 file with exact IDs for a generic workflow or migration")
    parser.add_argument("--legacy-format", choices=("json_string_array", "newline_ids"), default="json_string_array")
    parser.add_argument("--job-id")
    parser.add_argument("--claim-id")
    parser.add_argument("--signal-id")
    parser.add_argument("--artifact-sha256")
    args = parser.parse_args()
    job_id = args.job_id
    if args.input:
        value = json.loads(args.input.read_text(encoding="utf-8"))
        job_id = value["job"]["job_id"]
        ids = [ad["ad_archive_id"] for ad in value["ads"]]
    elif args.operation == "seed":
        ids = read_legacy_ids(args.ids, args.legacy_format)
    else:
        ids = json.loads(args.ids.read_text(encoding="utf-8"))
    print(json.dumps(make_plan(args.operation, args.namespace, ids, args.sheet_id,
          job_id=job_id, attempt_id=args.claim_id, signal_id=args.signal_id,
          artifact_sha256=args.artifact_sha256)))


if __name__ == "__main__":
    main()
