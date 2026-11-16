"""Regression checks against actual portable installation kits; no live services."""
from __future__ import annotations
import argparse
import hashlib
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
KITS = []
REPORTS = []


def inventory(root):
    return {p.relative_to(root).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in root.rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.suffix != '.pyc'}


class PackageTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory(prefix='portable agent package ')
        cls.base = Path(cls.temp.name)

    @classmethod
    def tearDownClass(cls):
        cls.temp.cleanup()

    def setup_kit(self, kit):
        case = Path(tempfile.mkdtemp(prefix='selected resources ', dir=self.base))
        self.name = json.loads((kit / 'kit-manifest.json').read_text())['agent']
        self.agent = kit / 'agent-files' / self.name
        self.support = kit / 'agent-files/offline-validation-support'
        self.contracts = kit / 'agent-files/contract-resources'
        self.contracts = self.contracts if self.contracts.exists() else None
        self.skills = {}
        for archive in (kit / 'skills').glob('*.zip'):
            # Simulates explicit host selection in tests; runtime never unpacks skills.
            with zipfile.ZipFile(archive) as contents:
                names = contents.namelist()
                self.assertTrue(names and all(name.startswith(archive.stem + '/') for name in names))
                contents.extractall(case)
            self.skills[archive.stem] = case / archive.stem
        self.case = case
        self.config = case / 'active configuration.json'
        self.config.write_text(json.dumps(json.loads((self.agent / 'validation-cases.json').read_text())['fixture_config']))

    def helper(self, expected=0, tests=False, config=None, command='check', extra=()):
        args = [sys.executable, '-I', '-S', '-B', str(self.support / 'validate.py'), command, '--package', str(self.agent)]
        for name, path in self.skills.items(): args += ['--skill', name + '=' + str(path)]
        if self.contracts: args += ['--contracts', str(self.contracts)]
        if command == 'check': args += ['--config', str(config or self.config)]
        if tests: args += ['--source-tests']
        completed = subprocess.run(args + list(extra), capture_output=True, text=True, timeout=180)
        try: report = json.loads(completed.stdout)
        except Exception: self.fail(completed.stdout + completed.stderr)
        if completed.returncode != expected:
            tests = report.get('source_unit_tests', {})
            log = tests.get('log', '')
            self.fail(json.dumps({k:v for k,v in report.items() if k != 'source_unit_tests'}) + '\n' + log[-12000:] + completed.stderr)
        return report

    def test_roadmap_contains_requested_pending_agents(self):
        text = (REPO / 'agents/ROADMAP.md').read_text()
        self.assertIn('## Product council', text)
        self.assertIn('## Manufacturer sourcing', text)
        self.assertIn('photographs', text)
        self.assertIn('reference board', text)


    def test_each_kit_inventory_and_individual_skill_archives(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                manifest = json.loads((kit / 'kit-manifest.json').read_text())
                expected = {item['path']: item['sha256'] for item in manifest['files']}
                actual = inventory(kit); actual.pop('kit-manifest.json')
                self.assertEqual(actual, expected)
                self.assertIsNone(manifest['adapter'])
                self.assertEqual((kit / 'INSTRUCTIONS.md').read_bytes(), (self.agent / 'AGENT.md').read_bytes())
                self.assertFalse((self.agent / 'skills').exists())
                self.assertEqual(set(self.skills), set(json.loads((self.agent / 'skill-bindings.json').read_text())))
                for name, root in self.skills.items(): self.assertEqual(inventory(root), inventory(REPO / 'skills' / name))
                self.assertEqual(inventory(self.agent), inventory(REPO / 'agents' / self.name))
                self.assertFalse(list(kit.rglob('SKILL.md')))
                self.assertEqual(len(list((kit / 'skills').glob('*.zip'))), len(self.skills))
                if self.name == 'ecommerce-discovery-looper':
                    self.assertEqual(self.skills, {})
                    self.assertTrue(self.contracts)
                    self.assertFalse(list(self.contracts.rglob('AGENT.md')))
                    self.assertTrue(all(p.suffix == '.json' for p in self.contracts.rglob('*') if p.is_file()))

    def test_full_offline_contract_and_source_suites(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                before = {name: inventory(path) for name, path in self.skills.items()}
                report = self.helper(tests=True)
                self.assertEqual(report['status'], 'passed')
                self.assertGreater(report['source_unit_tests']['tests_run'], 0)
                self.assertTrue(report['large_integer_preserved'])
                self.assertIn('invalid uri', report['invalid_cases_rejected'])
                self.assertEqual(report['host_orchestration_acceptance'], 'not_executed')
                self.assertEqual(report['connector_checks'], 'not_executed')
                self.assertIsNone(report['runtime_skill_version_pin'])
                self.assertEqual(before, {name: inventory(path) for name, path in self.skills.items()})
                REPORTS.append({'agent':kit.name, 'tests':report['source_unit_tests'],
                                'schemas':report['schemas_validated'], 'references':report['local_references_resolved']})

    def test_renamed_relocated_resource_roots_and_separate_skills(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                self.agent = Path(shutil.copytree(self.agent, self.case / 'arbitrary agent location'))
                self.support = Path(shutil.copytree(self.support, self.case / 'checker elsewhere'))
                if self.contracts: self.contracts = Path(shutil.copytree(self.contracts, self.case / 'contracts elsewhere'))
                self.skills = {name: Path(shutil.move(path, self.case / ('selected package ' + str(index))))
                               for index, (name, path) in enumerate(self.skills.items())}
                self.assertEqual(self.helper()['status'], 'passed')

    def test_missing_selected_skill_has_no_global_fallback(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.skills: continue
            with self.subTest(agent=kit.name):
                self.skills.pop(next(iter(self.skills)))
                self.assertIn('exactly the declared skills', self.helper(expected=1)['message'])

    def test_missing_schema_is_reported(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.skills: continue
            with self.subTest(agent=kit.name):
                path = next(iter(self.skills.values())) / 'schemas/output.schema.json'
                path.unlink()
                self.assertEqual(self.helper(expected=1)['error_type'], 'FileNotFoundError')

    def test_selected_skill_updates_are_not_hash_pinned(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.skills: continue
            with self.subTest(agent=kit.name):
                for path in self.skills.values():
                    entrypoint = path / 'SKILL.md'
                    entrypoint.write_text(entrypoint.read_text() + '\nSynthetic compatible instruction revision.\n')
                self.assertEqual(self.helper()['status'], 'passed')

    def test_incompatible_selected_schema_fails(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.skills: continue
            with self.subTest(agent=kit.name):
                if self.name == 'ecommerce-product-discoverer':
                    path = self.skills['ecommerce-cluster-ads'] / 'schemas/output.schema.json'
                    value = json.loads(path.read_text()); value['$defs']['cluster']['not'] = {}
                elif self.name == 'ecommerce-competitive-researcher':
                    path = self.skills['ecommerce-normalize-products'] / 'schemas/input.schema.json'
                    value = json.loads(path.read_text()); value['properties']['country']['not'] = {}
                else:
                    path = self.skills['ecommerce-develop-strategy'] / 'schemas/input.schema.json'
                    value = json.loads(path.read_text()); value['properties']['target_market_country']['not'] = {}
                path.write_text(json.dumps(value))
                self.assertEqual(self.helper(expected=1)['error_type'], 'ValidationError')

    def test_unbound_external_references_fail_without_network(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.skills: continue
            with self.subTest(agent=kit.name):
                path = next(iter(self.skills.values())) / 'schemas/output.schema.json'
                value = json.loads(path.read_text()); value['$ref'] = 'https://invalid.example/schema'
                path.write_text(json.dumps(value))
                self.assertIn(self.helper(expected=1)['error_type'], ('Unresolvable','Unretrievable','NoSuchResource'))

    def test_stale_and_modified_package_files_fail(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                self.agent = Path(shutil.copytree(self.agent, self.case / 'mixed agent'))
                stale = self.agent / 'obsolete-instructions.md'; stale.write_text('stale')
                self.assertIn('Stale or missing', self.helper(expected=1)['message'])
                stale.unlink()
                path = self.agent / 'AGENT.md'; path.write_text(path.read_text() + '\nmodified\n')
                self.assertIn('differs from package', self.helper(expected=1)['message'])

    def test_contract_snapshot_integrity_and_required_binding(self):
        for kit in KITS:
            self.setup_kit(kit)
            if not self.contracts: continue
            original = self.contracts; self.contracts = None
            self.assertIn('Required resource folder', self.helper(expected=1)['message'])
            self.contracts = Path(shutil.copytree(original, self.case / 'corrupt contracts'))
            path = self.contracts / 'schema-map.json'; path.write_text('{}')
            self.assertIn('differs from package', self.helper(expected=1)['message'])

    def test_unconfigured_settings_and_corrupt_libraries_fail(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                self.assertEqual(self.helper(config=self.agent / 'config.example.json', expected=1)['error_type'], 'ValidationError')
                self.support = Path(shutil.copytree(self.support, self.case / 'corrupt libraries'))
                with (self.support / 'python-dependencies.zip').open('ab') as stream: stream.write(b'corrupt')
                self.assertIn('checksum mismatch', self.helper(expected=1)['message'])

    def test_explicit_unknown_and_denied_metadata(self):
        import importlib.util
        spec = importlib.util.spec_from_file_location('metadata_policy', HERE / 'validation-support/metadata_policy.py')
        module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
        with self.assertWarns(RuntimeWarning):
            self.assertEqual(module.optional_metadata({'capabilities': {'canShare':True}}, True), ['trashed','capabilities.canAddChildren'])
        for metadata in ({'trashed':True}, {'capabilities':{'canAddChildren':False}}):
            with self.assertRaises(ValueError): module.optional_metadata(metadata, True)
        self.assertEqual(module.optional_metadata({'trashed':False}), [])

    def test_reproducible_build_and_optional_host_adapter(self):
        for kit in KITS:
            with self.subTest(agent=kit.name):
                self.setup_kit(kit)
                cache = self.case / 'locked wheel cache'; cache.mkdir()
                with zipfile.ZipFile(self.support / 'python-dependencies.zip') as archive: archive.extractall(cache)
                output = self.case / 'rebuilt kit'
                args = [sys.executable,'-B',str(HERE / 'build.py'),'--agent',self.name,'--output',str(output),'--wheel-cache',str(cache),'--offline']
                completed = subprocess.run(args,capture_output=True,text=True,timeout=60)
                self.assertEqual(completed.returncode,0,completed.stdout+completed.stderr)
                self.assertEqual(inventory(output),inventory(kit))
                completed = subprocess.run(args,capture_output=True,text=True,timeout=60)
                self.assertNotEqual(completed.returncode,0)
                self.assertIn('fresh --output',completed.stderr)
                # Optional host guide never changes the portable entrypoint/resources.
                args[args.index('--output')+1] = str(self.case / 'host-specific export')
                completed = subprocess.run(args+['--adapter','chatgpt'],capture_output=True,text=True,timeout=60)
                self.assertEqual(completed.returncode,0,completed.stdout+completed.stderr)
                exported = self.case / 'host-specific export'
                self.assertEqual((exported / 'INSTRUCTIONS.md').read_bytes(),(kit / 'INSTRUCTIONS.md').read_bytes())
                self.assertTrue((exported / 'SETUP-chatgpt.md').exists())


def main():
    global KITS
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--bundle',type=Path,required=True,help='One kit or a directory containing all agent kits')
    parser.add_argument('--report',type=Path)
    args = parser.parse_args(); root=args.bundle.resolve(strict=True)
    KITS=[root] if (root/'kit-manifest.json').exists() else sorted(p for p in root.iterdir() if (p/'kit-manifest.json').exists())
    if not KITS: parser.error('No generated kit found')
    # A kit directory may be renamed after download; its identity comes from its manifest.
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(PackageTests))
    report={'status':'passed' if result.wasSuccessful() else 'failed','package_regressions':result.testsRun,
            'agents':REPORTS,'host_orchestration':'not_executed','connector_checks':'not_executed'}
    print(json.dumps(report,indent=2))
    if args.report: args.report.write_text(json.dumps(report,indent=2)+'\n')
    return 0 if result.wasSuccessful() else 1


if __name__=='__main__': raise SystemExit(main())
