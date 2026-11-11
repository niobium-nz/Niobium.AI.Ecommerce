"""Offline contracts and mocked orchestration. No network, browser or model calls."""
from __future__ import annotations
import copy
import json
import unittest
from pathlib import Path

from jsonschema import Draft202012Validator, ValidationError
from protocol_reference import (ROOT, SKILLS, PROFILE, MARKET, IMAGE, GateError, artifact,
    git_blob, image_gate, marketing_gate, marketing_input, output_path, profile_gate,
    prompt_input, public_url, read, run, strict_loads, validate, validate_input, validator)

CANDIDATE = '19219de3-9422-45b1-b8eb-4947ac1fe34a'


def structural_value(schema, root, key='fixture'):
    """Build explicitly synthetic shape mocks, never substantive market evidence."""
    if '$ref' in schema:
        part = root
        for step in schema['$ref'][2:].split('/'):
            part = part[step]
        return structural_value(part, root, key)
    if 'const' in schema:
        return schema['const']
    if 'enum' in schema:
        return schema['enum'][0]
    if 'anyOf' in schema:
        return structural_value(schema['anyOf'][0], root, key)
    kind = schema.get('type')
    if isinstance(kind, list):
        kind = kind[0]
    if kind == 'object':
        return {k: structural_value(schema['properties'][k], root, k) for k in schema.get('required', [])}
    if kind == 'array':
        n = schema.get('minItems', 0)
        return [structural_value(schema['items'], root, key + str(i)) for i in range(n)]
    if kind in ('integer', 'number'):
        return schema.get('minimum', 0)
    if kind == 'null':
        return None
    if kind == 'boolean':
        return False
    if key == 'asset_id':
        return 'lp-hero-01'
    return 'Synthetic ' + key


def fixtures():
    value = read(ROOT / 'examples/input.json')
    profile = {'url': value['landing_page_url'], 'final_url': value['landing_page_url'],
        'retrieval_date_time_utc': '2026-11-12T00:00:00Z',
        'product': {'name': 'Synthetic Desk Organizer', 'price': None, 'currency': None,
                    'variants': [], 'bundle_offers': [], 'key_claims': ['Separate compartments'],
                    'ingredients_or_materials': [], 'images': [], 'videos': [], 'how_it_wins': None},
        'vendor': None, 'blockers': []}
    schema = read(MARKET / 'output.schema.json')
    strategy = structural_value(schema['$defs']['strategy'], schema)
    p = strategy['product_details']
    p['recommended_primary_product_name'] = p['suggested_product_names'][0]
    plan = strategy['mobile_first_landing_page_plan']
    plan['creative_assets_needed_for_each_section'][0]['section_name'] = plan['mobile_first_section_order'][0]
    strategy['customer_segments'][0]['shared_context_snapshot']['recommended_product_name'] = p['recommended_primary_product_name']
    a = strategy['customer_segments'][0]['angle_and_trigger_matrix'][0]
    a['recommended_product_name_for_this_angle'] = a['suggested_product_names_for_this_angle'][0]
    image = {'status': 'ok', 'image_prompts': [{'asset_id': 'lp-hero-01', 'orientation': 'square',
        'overlay_copy_suggestion': '', 'prompt': 'Show the organizer clearly for a buyer seeking a tidy desk. Use the supplied clean multi-view product reference board for one exact SKU. Do not invent features or claims. Aspect ratio: 1:1.'}]}
    return value, profile, strategy, image


class Host:
    """Every method here is a mock, not a real browser/connector/approval assertion."""
    def __init__(self):
        self.value, self.p, self.m, self.i = fixtures()
        self.calls, self.inputs, self.saved = [], {}, None
        self.fail_at = None
        self.good_identity = True
        self.board = {'readable_location': self.value['product_reference_board'],
                      'handoff_url': self.value['product_reference_board'],
                      'same_content_verified': True, 'durable_verified': True}
        self.receipt_patch = {}
        self.candidate = CANDIDATE

    def record(self, name):
        self.calls.append(name)
        if self.fail_at == name:
            raise GateError(name + '_failed', 'Synthetic host failure')

    def preflight(self, value, config):
        self.record('preflight')
    def resolve_board(self, location, binding):
        self.record('resolve_board')
        if not location.lower().startswith(('http://', 'https://')) and not binding:
            raise GateError('board_handoff_unavailable', 'Durable mapping required', clarification=True)
        return copy.deepcopy(self.board)
    def profile(self, value):
        self.record('profile'); self.inputs['profile'] = copy.deepcopy(value)
        return copy.deepcopy(self.p)
    def identity_supported(self, p):
        self.record('identity_supported'); return self.good_identity
    def strategy(self, value):
        self.record('strategy'); self.inputs['strategy'] = copy.deepcopy(value)
        return copy.deepcopy(self.m)
    def review_marketing(self, *args):
        self.record('review_marketing')
    def prompts(self, value):
        self.record('prompts'); self.inputs['prompts'] = copy.deepcopy(value)
        return copy.deepcopy(self.i)
    def review_images(self, *args):
        self.record('review_images')
    def reverify_board(self, *args):
        self.record('reverify_board')
    def uuid4(self):
        self.record('uuid4'); return self.candidate
    def publish(self, path, raw):
        self.record('publish'); self.saved = (path, raw)
        result = {'id': 'synthetic-file', 'name': path.rsplit('/', 1)[1], 'mime_type': 'application/json',
                  'trashed': False, 'parent_verified': True, 'unique_name_count': 1, 'raw_bytes': raw}
        result.update(self.receipt_patch)
        return result


class OnboardingTests(unittest.TestCase):
    def setUp(self):
        self.host = Host()
        self.value = copy.deepcopy(self.host.value)
        self.config = {'candidates_folder_id': 'synthetic_candidates_folder', 'board_handoff_binding': None}

    def invoke(self):
        return run(self.value, self.config, self.host)

    def test_all_schema_definitions(self):
        for path in ROOT.rglob('*.schema.json'):
            Draft202012Validator.check_schema(read(path))

    def test_exact_dependency_blob_hashes(self):
        for name, sha in read(ROOT / 'dependencies.json')['validated_blob_shas'].items():
            with self.subTest(name=name):
                self.assertEqual(git_blob(SKILLS / name), sha)

    def test_fixtures_use_complete_real_contracts(self):
        validate_input(self.value)
        validator(PROFILE / 'output.schema.json').validate(self.host.p)
        marketing_gate(self.host.m)
        image_gate(self.host.i, [])

    def test_strict_json_duplicate_keys(self):
        for text in ('{"a":1,"a":2}', '{"costs":{"a":1,"a":2}}'):
            with self.assertRaises(ValueError): strict_loads(text)

    def test_strict_json_nonfinite_numbers(self):
        for text in ('NaN', 'Infinity', '-Infinity', '1e999'):
            with self.assertRaises(ValueError): strict_loads(text)

    def test_large_integer_preserved(self):
        self.assertEqual(strict_loads('9007199254740993'), 9007199254740993)

    def test_input_exact_shape(self):
        for bad in ([], {}, dict(self.value, approved=True)):
            with self.assertRaises(ValidationError): validate_input(bad)

    def test_country_only_assigned_alpha2(self):
        for code in ('NZL', 'New Zealand', 'UK', 'ZZ', ' NZ', 'N1', 'NZ\n'):
            with self.subTest(code=code), self.assertRaises((ValueError, ValidationError)):
                validate_input(dict(self.value, target_country=code))

    def test_country_case_preserved(self):
        self.value['target_country'] = 'nz'
        result = self.invoke()
        self.assertEqual(result['target_country'], 'nz')
        self.assertEqual(self.host.inputs['strategy']['target_market_country'], 'nz')

    def test_optional_ids_absent_remain_null(self):
        del self.value['job_id']; del self.value['signal_id']
        result = self.invoke()
        self.assertIsNone(result['job_id']); self.assertIsNone(result['signal_id'])
        self.assertEqual(result['output_path'], 'candidates/unlinked/' + CANDIDATE + '.json')

    def test_optional_ids_explicit_null(self):
        self.value.update(job_id=None, signal_id=None)
        self.assertEqual(self.invoke()['status'], 'succeeded')

    def test_each_optional_id_independent(self):
        for key in ('job_id', 'signal_id'):
            v = copy.deepcopy(self.value); v.pop(key)
            self.assertEqual(run(v, self.config, Host())['status'], 'succeeded')

    def test_invalid_supplied_ids_not_regenerated(self):
        for v in ('', 'not-a-uuid', CANDIDATE.upper(), '../elsewhere'):
            result = run(dict(self.value, signal_id=v), self.config, Host())
            self.assertEqual(result['status'], 'failed'); self.assertIsNone(result['candidate_id'])

    def test_landing_url_required_and_nonblank(self):
        for url in ('', ' ', None):
            with self.assertRaises((ValueError, ValidationError)): validate_input(dict(self.value, landing_page_url=url))

    def test_private_url_and_credentials_rejected(self):
        for url in ('http://localhost/a', 'http://127.0.0.1/a', 'http://10.1.1.1/a', 'http://[::1]/a',
                    'https://user:password@example.com/x', 'file:///tmp/a', 'http://2130706433/a'):
            with self.subTest(url=url), self.assertRaises(ValueError): public_url(url)

    def test_missing_each_cost_field(self):
        for key in self.value['costs']:
            value = copy.deepcopy(self.value); del value['costs'][key]
            with self.subTest(key=key), self.assertRaises(ValidationError): validate_input(value)

    def test_zero_cost_is_not_missing(self):
        self.value['costs'].update(cogs_per_unit=0, extra_unit_cogs_per_order=0)
        self.assertEqual(self.invoke()['status'], 'succeeded')
        self.assertEqual(self.host.inputs['strategy']['cogs_per_unit'], 0)

    def test_invalid_cost_numbers(self):
        for value in (-1, '12.5', True, None, float('inf'), float('nan')):
            v = copy.deepcopy(self.value); v['costs']['cogs_per_unit'] = value
            with self.subTest(value=value), self.assertRaises((ValueError, ValidationError)): validate_input(v)

    def test_unassigned_currency(self):
        self.value['costs']['currency'] = 'ZZZ'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('profile', self.host.calls)

    def test_tax_and_fee_bare_percentages_rejected(self):
        for field in ('sales_tax', 'payment_processing_fees'):
            value = copy.deepcopy(self.value); value['costs'][field] = '15%'
            with self.assertRaises(GateError): validate_input(value)

    def test_costs_not_inferred_from_profile(self):
        self.host.p['product'].update(price=999, currency='USD')
        self.invoke()
        self.assertEqual(self.host.inputs['strategy']['currency'], 'NZD')
        self.assertEqual(self.host.inputs['strategy']['cogs_per_unit'], 12.5)

    def test_unconfigured_folder_fails_before_browser(self):
        self.config = read(ROOT / 'config.example.json')
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('profile', self.host.calls)

    def test_root_folder_alias_is_rejected(self):
        self.config['candidates_folder_id'] = 'root'
        self.assertEqual(self.invoke()['status'], 'failed')

    def test_missing_browser_or_write_capability_fails_preflight(self):
        self.host.fail_at = 'preflight'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertEqual(self.host.calls, ['preflight'])

    def test_board_is_required(self):
        del self.value['product_reference_board']
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertEqual(self.host.calls, [])

    def test_local_board_requires_durable_mapping(self):
        self.value['product_reference_board'] = '/work/board.png'
        self.assertEqual(self.invoke()['status'], 'needs_clarification')
        self.assertNotIn('profile', self.host.calls)

    def test_local_board_mapping_keeps_readable_and_durable_separate(self):
        self.value['product_reference_board'] = '/work/board.png'
        self.config['board_handoff_binding'] = 'authorized-assets'
        self.host.board['readable_location'] = '/work/board.png'
        self.assertEqual(self.invoke()['status'], 'succeeded')
        self.assertEqual(self.host.inputs['prompts']['product_reference_board'], '/work/board.png')
        self.assertTrue(json.loads(self.host.saved[1])['product_reference_board'].startswith('https://'))

    def test_unverified_board_content_or_durability_stops(self):
        for flag in ('same_content_verified', 'durable_verified'):
            host = Host(); host.board[flag] = False
            self.assertEqual(run(self.value, self.config, host)['status'], 'needs_clarification')
            self.assertNotIn('profile', host.calls)

    def test_board_change_before_write_stops(self):
        self.host.fail_at = 'reverify_board'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('publish', self.host.calls)

    def test_profile_input_is_url_only(self):
        self.invoke()
        self.assertEqual(self.host.inputs['profile'], {'landing_page_url': self.value['landing_page_url']})

    def test_profile_error_only_stops_strategy(self):
        self.host.p = {'error': 'Synthetic unavailable browser'}
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('strategy', self.host.calls)

    def test_profile_null_or_blank_product_identity(self):
        for p in (None, dict(self.host.p['product'], name=None), dict(self.host.p['product'], name=' ')):
            host = Host(); host.p['product'] = p
            self.assertEqual(run(self.value, self.config, host)['status'], 'needs_clarification')
            self.assertNotIn('strategy', host.calls)

    def test_profile_url_mismatch_stops(self):
        self.host.p['url'] = 'https://other.example.com/product'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('strategy', self.host.calls)

    def test_insufficient_semantic_identity_stops(self):
        self.host.good_identity = False
        self.assertEqual(self.invoke()['status'], 'needs_clarification')

    def test_partial_profile_proceeds_with_preserved_diagnostics(self):
        self.host.p.update(error='Synthetic blocked policy page', blockers=['Synthetic access limitation'])
        result = self.invoke()
        self.assertEqual(result['status'], 'partial')
        saved = json.loads(self.host.saved[1])
        self.assertEqual(saved['errors'], result['errors'])
        self.assertIn('profile_partial_failure', [e['code'] for e in result['errors']])

    def test_ordinary_missing_fact_is_warning(self):
        self.host.p['blockers'] = ['Material composition not displayed']
        result = self.invoke()
        self.assertEqual(result['status'], 'partial')
        self.assertEqual(result['errors'][0]['severity'], 'warning')

    def test_complete_strategy_mapping(self):
        self.invoke()
        expected = dict(self.value['costs'], target_market_country='NZ', competitor_used_product_name='Synthetic Desk Organizer',
            competitor_claims=['Separate compartments'], ingredients_or_materials=[], competitor_marketing_how_it_wins=None)
        self.assertEqual(self.host.inputs['strategy'], expected)

    def test_null_how_it_wins_and_empty_materials_preserved(self):
        payload = marketing_input(self.value, self.host.p)
        self.assertIsNone(payload['competitor_marketing_how_it_wins'])
        self.assertEqual(payload['ingredients_or_materials'], [])

    def test_marketing_error_only_stops_images(self):
        self.host.m = {'error': 'Synthetic unresolved fee basis'}
        result = self.invoke()
        self.assertEqual(result['status'], 'failed')
        self.assertNotIn('prompts', self.host.calls)
        self.assertNotIn('publish', self.host.calls)

    def test_incomplete_strategy_rejected(self):
        del self.host.m['customer_segments']
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('prompts', self.host.calls)

    def test_strategy_name_inconsistency_rejected(self):
        self.host.m['product_details']['recommended_primary_product_name'] = 'Unlisted'
        self.assertEqual(self.invoke()['status'], 'failed')

    def test_strategy_semantic_review_failure_stops(self):
        self.host.fail_at = 'review_marketing'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('prompts', self.host.calls)

    def test_image_input_extends_full_strategy_without_mutation(self):
        original = copy.deepcopy(self.host.m)
        self.invoke()
        supplied = self.host.inputs['prompts']
        self.assertEqual(set(supplied), set(original) | {'product_reference_board'})
        self.assertEqual({k: supplied[k] for k in original}, original)
        self.assertEqual(self.host.m, original)

    def test_image_error_only_stops_publication(self):
        self.host.i = {'error': 'Synthetic unavailable image reader'}
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('uuid4', self.host.calls)
        self.assertNotIn('publish', self.host.calls)

    def test_image_clarification_stops_publication(self):
        self.host.i = {'status': 'needs_clarification', 'image_prompts': [], 'error': 'Another view is required'}
        self.assertEqual(self.invoke()['status'], 'needs_clarification')
        self.assertNotIn('publish', self.host.calls)

    def test_empty_successful_image_strategy_allowed(self):
        self.host.i = {'status': 'ok', 'image_prompts': []}
        self.assertEqual(self.invoke()['status'], 'succeeded')
        self.assertEqual(json.loads(self.host.saved[1])['image_strategy'], self.host.i)

    def test_duplicate_prompt_asset_ids_rejected(self):
        self.host.i['image_prompts'] *= 2
        self.assertEqual(self.invoke()['status'], 'failed')

    def test_aspect_ratio_orientation_mismatch_rejected(self):
        self.host.i['image_prompts'][0]['orientation'] = 'portrait'
        self.assertEqual(self.invoke()['status'], 'failed')

    def test_board_location_never_embedded_in_prompt(self):
        self.host.i['image_prompts'][0]['prompt'] = self.value['product_reference_board'] + ' Aspect ratio: 1:1.'
        self.assertEqual(self.invoke()['status'], 'failed')

    def test_image_semantic_review_failure_stops(self):
        self.host.fail_at = 'review_images'
        self.assertEqual(self.invoke()['status'], 'failed')
        self.assertNotIn('publish', self.host.calls)

    def test_clean_publication_shape_and_exact_outputs(self):
        result = self.invoke()
        self.assertEqual(result['status'], 'succeeded')
        saved = json.loads(self.host.saved[1])
        self.assertEqual(set(saved), {'job_id','signal_id','candidate_id','target_country','product_reference_board','marketing_strategy','image_strategy','errors'})
        self.assertEqual(saved['marketing_strategy'], self.host.m)
        self.assertEqual(saved['image_strategy'], self.host.i)
        self.assertEqual(result['output_path'], output_path(self.value['signal_id'], CANDIDATE))

    def test_uuid_allocated_only_after_strategies_pass(self):
        self.invoke()
        self.assertLess(self.host.calls.index('review_images'), self.host.calls.index('uuid4'))
        self.assertLess(self.host.calls.index('uuid4'), self.host.calls.index('publish'))
        self.assertEqual(self.host.calls.count('uuid4'), 1)
        self.assertEqual(self.host.calls.count('publish'), 1)

    def test_invalid_uuid_is_not_published(self):
        self.host.candidate = 'invented'
        result = self.invoke()
        self.assertIsNone(result['candidate_id']); self.assertEqual(result['status'], 'failed')
        self.assertNotIn('publish', self.host.calls)

    def test_persistence_failure_retains_candidate_and_prior_errors(self):
        self.host.p['blockers'] = ['Synthetic profile limitation']
        self.host.fail_at = 'publish'
        result = self.invoke()
        self.assertEqual(result['candidate_id'], CANDIDATE)
        self.assertFalse(result['published']); self.assertIsNone(result['output_path'])
        self.assertEqual(len(result['errors']), 2)
        self.assertEqual(self.host.calls.count('publish'), 1)

    def test_wrong_name_mime_parent_or_duplicate_receipt_rejected(self):
        for patch in ({'name':'wrong.json'}, {'mime_type':'text/plain'}, {'parent_verified':False},
                      {'unique_name_count':2}, {'trashed':True}, {'id':None}, {'raw_bytes':b'truncated'}):
            host = Host(); host.receipt_patch = patch
            result = run(self.value, self.config, host)
            with self.subTest(patch=patch):
                self.assertFalse(result['published']); self.assertIsNone(result['output_file_id'])
                self.assertEqual(host.calls.count('publish'), 1)

    def test_terminal_result_cannot_claim_success_without_publication(self):
        result = self.invoke(); result['published'] = False
        with self.assertRaises(ValidationError): validate(result, 'result')

    def test_artifact_rejects_incomplete_strategy_and_extra_profile(self):
        self.invoke(); saved = json.loads(self.host.saved[1])
        saved['product_profile'] = self.host.p
        with self.assertRaises(ValidationError): validate(saved, 'artifact')

    def test_all_stages_leave_input_unchanged(self):
        original = copy.deepcopy(self.value)
        self.invoke()
        self.assertEqual(self.value, original)

    def test_failure_preserves_prior_profile_warnings(self):
        self.host.p['blockers'] = ['No material data']
        self.host.i = {'status':'needs_clarification','image_prompts':[],'error':'Board ambiguous'}
        result = self.invoke()
        self.assertEqual(result['status'], 'needs_clarification')
        self.assertEqual([e['code'] for e in result['errors']], ['profile_limitation','image_clarification'])

    def test_roadmap_contains_requested_pending_agents(self):
        text = (ROOT.parent / 'ROADMAP.md').read_text()
        self.assertIn('## Product council', text)
        self.assertIn('## Manufacturer sourcing', text)
        self.assertIn('photographs', text)
        self.assertIn('reference board', text)

    def test_manifest_scope_and_local_links(self):
        text = (ROOT / 'agent.yaml').read_text()
        self.assertIn('root_folder_name: candidates', text)
        self.assertIn('candidates/unlinked/{candidate_id}.json', text)
        self.assertNotIn('ecommerce-build-product-board/SKILL.md', text)

    def test_authored_text_style(self):
        for p in ROOT.rglob('*'):
            if p.is_file() and 'skills' not in p.relative_to(ROOT).parts and p.suffix in ('.md','.py','.yaml'):
                self.assertNotIn(chr(0x2013), p.read_text())


if __name__ == '__main__':
    unittest.main()
