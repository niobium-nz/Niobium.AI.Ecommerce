"""Build one portable install kit. Network is used here, never by hosted validation."""
from __future__ import annotations

import argparse
import hashlib
import io
import json
from pathlib import Path
import re
import shutil
import tempfile
import urllib.request
import zipfile

REPO = Path(__file__).resolve().parents[2]
AGENT = REPO / "agents/ecommerce-product-discoverer"
SKILL = REPO / "skills/ecommerce-cluster-ads"
SUPPORT = Path(__file__).resolve().parent / "validation-support"
FIXED_DATE = (2020, 1, 1, 0, 0, 0)


import sys
sys.path.insert(0, str(REPO / "tools/agent-packaging"))
from build_common import files, digest, write_json, zip_bytes, wheel_bytes
COMMON_SUPPORT = REPO / "tools/agent-packaging/validation-support"


BINDING_INSTRUCTIONS = '''Use the ecommerce-product-discoverer folder uploaded as Agent Files as the
agent resource root. Use the single ecommerce-cluster-ads ZIP attached to THIS
workspace agent through Add skill. It is separate from Agent Files and from any
global Plugins/Skills installation. Resolve its resources through the current
attachment context each run; never store a temporary host path or add a version pin.

Use the one active deployment configuration uploaded with this agent. Its normal
name is deployment-config.json; an existing 04-deployment-config.json may be kept
instead. If both exist, ask which is active and remove the ambiguity before a job.
Never use config.example.json or infer a folder ID from a name.

The offline-validation-support folder contains a real schema validator and its
dependencies. Follow references/validation.md with resource paths resolved for the
current execution only. Do not install dependencies online or treat parsing as
schema validation. Register logical skill schema URIs from skill-bindings.json
against the currently attached skill. Do not copy that skill into Agent Files.

During setup, run only requested offline checks and read-only capability/folder
checks. An unavailable host mock harness is reported as untested, not fabricated
or treated as an installation blocker. Paid searches and Drive writes require a
real discovery request or an explicitly authorized test. Missing optional Drive
trash/add-permission metadata is a warning; explicit denial and failed upload
verification remain errors. Read AGENT.md and its references for full behavior.

'''

SETUP_CHECK = '''Check this installation in Preview without paid searches, Drive writes,
publishing or scheduling. Resolve the agent files, active deployment configuration
and the resources of this agent's attached ecommerce-cluster-ads skill through
this execution's actual bindings. Do not choose a global same-name skill or save
temporary mount paths. Confirm referenced skill resources are readable.

Using the resolved current paths, run:
python3 -I -S -B <support>/validate.py check --package <agent-resources> --skill-resources <selected-skill-resources> --config <active-config> --source-tests

Report the exact exit code, validator/dependency versions, file/schema/reference
counts, accepted and rejected cases and test results. Do not call JSON parsing
schema validation. Do not install dependencies online. If the runtime is unsupported,
report its exact environment and the missing equivalent capability.

Inspect the connected action schemas and, if authentication is available, read the
configured folder's identity/name/type. Do not create a probe file. Missing optional
trashed/canAddChildren fields are unknown warnings, not installation blockers;
explicit denial or wrong destination still fails. Report offline validation,
read-only connection checks, host orchestration and live execution separately.
If no host mock harness exists, mark that section not executed and continue setup.
'''

START_HERE = '''# Install ecommerce-product-discoverer

This kit is generated from one source revision. Extract it on your computer.
For an already installed agent, use UPDATE-EXISTING.txt instead of the new-install
steps below. Keep its existing configuration and skill attachment.
For ChatGPT, follow these steps in order:

1. Open Agents, then Create on the Agents page, then Start blank.
2. Paste instructions.chatgpt.md into Instructions.
3. Add files -> Upload folder: select the outer agent-files/ecommerce-product-discoverer
   folder itself. Repeat for the outer agent-files/offline-validation-support folder.
4. Set the existing discovery folder ID and IANA timezone in
   agent-files/deployment-config.json and upload that one file. Null is a placeholder.
5. Add skill: attach skills/ecommerce-cluster-ads.zip once. Do not upload an extracted
   second copy into Agent Files.
6. Connect ScrapeCreators and Google Drive with the required actions. Run SETUP-CHECK.txt
   in Preview. Setup does not spend search credits or write to Drive.
7. Save/create privately. A passing setup check does not claim a successful live run.

For other hosts, use AGENT.md and the adapter notes under
agent-files/ecommerce-product-discoverer/adapters. The core is platform-neutral;
the included Python wheels support the Linux targets listed in validation.md.

For updates, preserve existing settings, replace the complete agent/support folders
and Instructions together, remove obsolete source ZIPs, and leave one active config.
An older 04-deployment-config.json may be retained as the sole active config.
Keep the selected skill unless deliberately updating it, and rerun compatibility
checks against that selection. Never merge an old nested skill into the new folder.

kit-manifest.json records what was built, including skill ZIP integrity for download
checking. It is not a runtime skill-version pin. Agent hashes protect against stale
or mixed agent files. No account ID, credentials or configured Drive destination is
included in the default kit. Hosted agents do not update automatically from GitHub.
'''

UPDATE_EXISTING = '''UPDATE AN EXISTING ECOMMERCE-PRODUCT-DISCOVERER

This update allows missing optional Drive metadata during real discovery jobs.
It records drive_metadata_unavailable and continues when trashed and/or
capabilities.canAddChildren are omitted or null. An actual denial or deleted
folder still stops the job; saved-file identity and complete bytes are verified.

1. Extract this kit and edit the existing workspace agent.
2. Replace its Instructions completely with instructions.chatgpt.md.
3. In Files, replace these two complete folders using Upload folder:
   - agent-files/ecommerce-product-discoverer
   - agent-files/offline-validation-support
   Select each outer folder itself. Do not merge old files or retain duplicates.
4. KEEP the existing active 04-deployment-config.json (or deployment-config.json)
   and the existing ecommerce-cluster-ads skill attachment. Do not upload this
   kit's unconfigured deployment-config.json or attach its skill ZIP a second time.
5. Save the agent. Start a fresh Preview conversation and use SETUP-CHECK.txt.
   Setup performs no paid searches or Drive writes. Then run the intended real
   discovery input in a fresh conversation; it receives a new job ID.

Missing optional fields must produce a warning, not folder_invalid or a request
to prove those unavailable fields. Real access errors, wrong folder identity,
incomplete filename checks and unverified file contents remain failures.
The Instructions and uploaded AGENT.md must agree; updating GitHub alone does
not update this installed workspace agent. Keep untested live behavior separate
from offline setup results.
'''


def build(output, cache, offline=False):
    if output.exists():
        raise FileExistsError("Choose a fresh --output directory; existing kits may contain your edited settings: " + str(output))
    if (AGENT / "skills").exists():
        raise ValueError("Nested skill duplication in agent source")
    output.parent.mkdir(parents=True, exist_ok=True)
    cache.mkdir(parents=True, exist_ok=True)
    version = re.search(r"^version: ([0-9.]+)$", (AGENT / "agent.yaml").read_text(), re.M).group(1)
    with tempfile.TemporaryDirectory(prefix="discoverer-build-", dir=output.parent) as temporary:
        stage = Path(temporary) / "kit"
        resources = stage / "agent-files"
        target_agent = resources / AGENT.name
        target_support = resources / "offline-validation-support"
        target_support.mkdir(parents=True)
        source_files = []
        for path in files(AGENT):
            relative, raw = path.relative_to(AGENT), path.read_bytes()
            destination = target_agent / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_bytes(raw)
            source_files.append({"path": relative.as_posix(), "bytes": len(raw), "sha256": digest(raw)})
        source_digest = digest(json.dumps(source_files, sort_keys=True, separators=(",", ":")).encode())
        manifest = {"package_version": version, "source_digest": source_digest,
                    "agent_contract_version": "2.0.0",
                    "agent": {"name": AGENT.name, "files": source_files},
                    "skill_selection": "current_agent_or_task_binding", "skill_version_pin": None}
        write_json(target_support / "deployment-manifest.json", manifest)
        lock = json.loads((COMMON_SUPPORT / "dependency-lock.json").read_text())
        archive = zip_bytes([(item["filename"], wheel_bytes(item, cache, offline)) for item in lock["wheels"]])
        (target_support / lock["archive"]).write_bytes(archive)
        lock["archive_sha256"] = digest(archive)
        write_json(target_support / "dependency-lock.json", lock)
        for name in ("runtime.py", "resource_bindings.py", "metadata_policy.py"):
            shutil.copyfile(COMMON_SUPPORT / name, target_support / name)
        for name in ("validate.py", "README.md"):
            shutil.copyfile(SUPPORT / name, target_support / name)
        shutil.copyfile(AGENT / "config.example.json", resources / "deployment-config.json")
        (stage / "skills").mkdir()
        skill_zip = zip_bytes([((Path(SKILL.name) / path.relative_to(SKILL)).as_posix(), path.read_bytes()) for path in files(SKILL)])
        (stage / "skills/ecommerce-cluster-ads.zip").write_bytes(skill_zip)
        (stage / "instructions.chatgpt.md").write_text(BINDING_INSTRUCTIONS + (AGENT / "AGENT.md").read_text(), encoding="utf-8")
        (stage / "SETUP-CHECK.txt").write_text(SETUP_CHECK, encoding="utf-8")
        (stage / "START-HERE.md").write_text(START_HERE, encoding="utf-8")
        (stage / "UPDATE-EXISTING.txt").write_text(UPDATE_EXISTING, encoding="utf-8")
        inventory = [{"path": path.relative_to(stage).as_posix(), "sha256": digest(path.read_bytes())} for path in files(stage)]
        write_json(stage / "kit-manifest.json", {"package_version": version, "agent_source_digest": source_digest,
                   "files": inventory, "note": "Distribution integrity only; selected runtime skill is not version-pinned."})
        stage.rename(output)
    return {"output": str(output), "package_version": version, "agent_files": len(source_files),
            "skill_packages": 1, "dependency_wheels": len(lock["wheels"]), "source_digest": source_digest}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=REPO / "dist/ecommerce-product-discoverer")
    parser.add_argument("--wheel-cache", type=Path, default=REPO / ".cache/discoverer-wheels")
    parser.add_argument("--offline", action="store_true", help="Require an already populated, hash-verified wheel cache")
    args = parser.parse_args()
    print(json.dumps(build(args.output.resolve(), args.wheel_cache.resolve(), args.offline), indent=2))


if __name__ == "__main__":
    main()
