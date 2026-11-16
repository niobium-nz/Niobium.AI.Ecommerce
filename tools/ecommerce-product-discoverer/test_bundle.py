"""Test the shipped upload layout in isolated processes, with no real connectors."""
from __future__ import annotations

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
import zipfile

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
BUNDLE = None


def inventory(root):
    return {p.relative_to(root).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in root.rglob("*") if p.is_file()}


class BundleTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory(prefix="discoverer artifact tests ")
        cls.base = Path(cls.temp.name)
        # This is an explicit test fixture simulating a selected host attachment.
        # Production validation never unpacks or discovers a skill itself.
        cls.skill = cls.base / "selected skill resources" / "ecommerce-cluster-ads"
        with zipfile.ZipFile(BUNDLE / "skills/ecommerce-cluster-ads.zip") as archive:
            archive.extractall(cls.skill.parent)
        cls.config = cls.base / "active config.json"
        cls.config.write_text(json.dumps({"discovery_folder_id": "offline_fixture_folder", "timezone": "Pacific/Auckland"}))
        cls.original_skill = inventory(cls.skill)

    @classmethod
    def tearDownClass(cls):
        cls.temp.cleanup()

    def setUp(self):
        self.case = Path(tempfile.mkdtemp(prefix="case ", dir=self.base))
        self.agent = BUNDLE / "agent-files/ecommerce-product-discoverer"
        self.support = BUNDLE / "agent-files/offline-validation-support"

    def run_helper(self, command="check", extra=(), skill=None, config=None, expected=0):
        args = [sys.executable, "-I", "-S", "-B", str(self.support / "validate.py"), command,
                "--package", str(self.agent), "--skill-resources", str(skill or self.skill)]
        if command == "check":
            args += ["--config", str(config or self.config)]
        completed = subprocess.run(args + list(extra), capture_output=True, text=True, timeout=90)
        self.assertEqual(completed.returncode, expected, completed.stdout + completed.stderr)
        return json.loads(completed.stdout)

    def copied_agent(self):
        self.agent = Path(shutil.copytree(self.agent, self.case / "relocated agent resources"))
        return self.agent

    def copied_skill(self):
        return Path(shutil.copytree(self.skill, self.case / "another selected skill"))

    def test_distribution_integrity_and_single_skill(self):
        manifest = json.loads((BUNDLE / "kit-manifest.json").read_text())
        expected = {item["path"]: item["sha256"] for item in manifest["files"]}
        actual = inventory(BUNDLE)
        actual.pop("kit-manifest.json")
        self.assertEqual(actual, expected)
        self.assertFalse((self.agent / "skills").exists())
        self.assertEqual(len(list(BUNDLE.rglob("SKILL.md"))), 0)
        self.assertEqual(list((BUNDLE / "skills").iterdir()), [BUNDLE / "skills/ecommerce-cluster-ads.zip"])
        self.assertEqual(inventory(REPO / "skills/ecommerce-cluster-ads"), self.original_skill)
        self.assertEqual(inventory(REPO / "agents/ecommerce-product-discoverer"), inventory(self.agent))
        self.assertTrue((BUNDLE / "instructions.chatgpt.md").read_text().endswith((self.agent / "AGENT.md").read_text()))

    def test_offline_schema_and_source_suite(self):
        report = self.run_helper(extra=["--source-tests"])
        self.assertEqual(report["status"], "passed")
        self.assertGreaterEqual(report["source_unit_tests"]["tests_run"], 76)
        self.assertEqual(report["source_unit_tests"]["skipped"], 0)
        self.assertEqual(report["schemas_validated"], 6)
        self.assertEqual(len(report["invalid_cases_rejected"]), 13)
        self.assertTrue(report["large_integer_preserved"])
        self.assertEqual(report["host_orchestration_acceptance"], "not_executed")
        self.assertEqual(report["connector_checks"], "not_executed")
        self.assertIsNone(report["source"]["skill_version_pin"])
        self.assertNotIn("offline_fixture_folder", json.dumps(report))
        self.assertEqual(inventory(self.skill), self.original_skill)
        print("\nContract suite:", json.dumps(report["source_unit_tests"]))

    def test_relocated_agent_support_and_skill_with_spaces(self):
        self.copied_agent()
        self.support = Path(shutil.copytree(self.support, self.case / "validation support elsewhere"))
        report = self.run_helper(skill=self.copied_skill())
        self.assertEqual(report["status"], "passed")

    def test_nested_skill_rejected(self):
        (self.copied_agent() / "skills").mkdir()
        self.assertIn("Nested skills", self.run_helper(expected=1)["message"])

    def test_same_root_cannot_be_used_as_skill(self):
        self.assertIn("own root", self.run_helper(skill=self.agent, expected=1)["message"])

    def test_missing_skill_schema_has_actionable_error(self):
        skill = self.copied_skill()
        (skill / "schemas/output.schema.json").unlink()
        report = self.run_helper(skill=skill, expected=1)
        self.assertIn("schemas/output.schema.json", report["message"])

    def test_missing_agent_resource_fails_before_validation(self):
        (self.copied_agent() / "schemas/result.schema.json").unlink()
        report = self.run_helper(expected=1)
        self.assertEqual(report["error_type"], "FileNotFoundError")

    def test_stale_extra_files_rejected(self):
        (self.copied_agent() / "old-instructions.md").write_text("stale source")
        self.assertIn("inventory differs", self.run_helper(expected=1)["message"])

    def test_modified_agent_fails_manifest(self):
        path = self.copied_agent() / "AGENT.md"
        path.write_text(path.read_text() + "\nStale instructions\n")
        self.assertIn("differs from", self.run_helper(expected=1)["message"])

    def test_skill_instruction_revision_not_hash_pinned(self):
        skill = self.copied_skill()
        path = skill / "SKILL.md"
        path.write_text(path.read_text() + "\nSynthetic compatible revision.\n")
        self.assertEqual(self.run_helper(skill=skill)["status"], "passed")

    def test_selected_schema_is_used_not_cached_old_schema(self):
        skill = self.copied_skill()
        path = skill / "schemas/output.schema.json"
        schema = json.loads(path.read_text())
        schema["not"] = {"type": "array", "minItems": 1}
        path.write_text(json.dumps(schema))
        self.assertEqual(self.run_helper(skill=skill, expected=1)["error_type"], "ValidationError")

    def test_external_reference_cannot_fetch_network(self):
        skill = self.copied_skill()
        path = skill / "schemas/output.schema.json"
        schema = json.loads(path.read_text())
        schema["$ref"] = "https://invalid.example/schema.json"
        path.write_text(json.dumps(schema))
        report = self.run_helper(skill=skill, expected=1)
        self.assertIn(report["error_type"], ("Unresolvable", "Unretrievable", "NoSuchResource"))

    def test_logical_schema_validation_and_invalid_instance(self):
        instance = self.case / "input.json"
        instance.write_text("[]")
        extra = ["--schema", "urn:niobium:ecommerce-cluster-ads:schema:input", "--instance", str(instance)]
        self.assertEqual(self.run_helper("validate", extra)["status"], "valid")
        instance.write_text('{"not":"an array"}')
        self.assertEqual(self.run_helper("validate", extra, expected=1)["status"], "invalid")
        extra[1] = "urn:missing:schema"
        self.assertIn("Schema must be", self.run_helper("validate", extra, expected=1)["message"])

    def test_unconfigured_template_rejected(self):
        self.assertEqual(self.run_helper(config=BUNDLE / "agent-files/deployment-config.json", expected=1)["error_type"], "ValidationError")

    def test_corrupt_dependency_archive_rejected(self):
        self.support = Path(shutil.copytree(self.support, self.case / "corrupt support"))
        with (self.support / "python-dependencies.zip").open("ab") as handle:
            handle.write(b"corruption")
        self.assertIn("checksum mismatch", self.run_helper(expected=1)["message"])

    def test_dependency_cache_tamper_fails_build(self):
        spec = importlib.util.spec_from_file_location("discoverer_build", HERE / "build.py")
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        lock = json.loads((HERE / "validation-support/dependency-lock.json").read_text())
        cache = self.case / "bad cache"
        cache.mkdir()
        (cache / lock["wheels"][0]["filename"]).write_bytes(b"corrupt")
        with self.assertRaisesRegex(ValueError, "checksum/length mismatch"):
            module.wheel_bytes(lock["wheels"][0], cache, True)

    def test_reproducible_offline_build(self):
        cache = self.case / "verified wheels"
        cache.mkdir()
        with zipfile.ZipFile(self.support / "python-dependencies.zip") as archive:
            archive.extractall(cache)
        output = self.case / "rebuilt kit"
        command = [sys.executable, "-B", str(HERE / "build.py"), "--output", str(output),
                   "--wheel-cache", str(cache), "--offline"]
        completed = subprocess.run(command, capture_output=True, text=True, timeout=90)
        self.assertEqual(completed.returncode, 0, completed.stdout + completed.stderr)
        self.assertEqual(inventory(output), inventory(BUNDLE))
        # Existing settings are never silently overwritten by a rebuild.
        completed = subprocess.run(command, capture_output=True, text=True, timeout=90)
        self.assertNotEqual(completed.returncode, 0)
        self.assertIn("Choose a fresh", completed.stderr)


def main():
    global BUNDLE
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--bundle", type=Path, required=True)
    args = parser.parse_args()
    BUNDLE = args.bundle.resolve(strict=True)
    result = unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(BundleTests))
    return 0 if result.wasSuccessful() else 1


if __name__ == "__main__":
    raise SystemExit(main())
