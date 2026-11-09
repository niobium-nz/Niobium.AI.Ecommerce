#!/usr/bin/env python3
"""Optional maintainer tests; all keyword fixtures are synthetic, not researched."""
from __future__ import annotations

import copy
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

import validate_json as v

ROOT = Path(__file__).resolve().parents[1]


def input_fixture(**changes):
    value = {'category_focus': 'travel backpacks', 'country': 'NZ'}
    value.update(changes)
    return value


def output_fixture(count=25):
    return {'category_focus': 'test category', 'optimized_keywords': [f'synthetic phrase {i}' for i in range(count)]}


class InputTests(unittest.TestCase):
    def test_required_fields_only(self):
        self.assertEqual([], v.validate_input(input_fixture()))

    def test_all_country_pairs_and_case_variants(self):
        records = json.loads((ROOT/'references/iso3166-1.json').read_text())['countries']
        for record in records:
            for prop in ['alpha_2', 'alpha_3']:
                code = record[prop]
                for form in [code, code.lower(), code[0] + code[1:].lower()]:
                    with self.subTest(code=form):
                        self.assertEqual([], v.validate_input(input_fixture(country=form)))
                        self.assertEqual(record, v.resolve_country(form))

    def test_numeric_codes_rejected(self):
        for code in ['554', '55', 554, 55, 1.0, True]:
            with self.subTest(code=code):
                self.assertTrue(v.validate_input(input_fixture(country=code)))

    def test_names_aliases_subdivisions_unassigned_rejected(self):
        for code in ['New Zealand', 'UK', 'ZZ', 'ZZZ', 'XK', 'en-NZ', 'NZ-AUK', 'SU', 'USSR']:
            with self.subTest(code=code):
                self.assertTrue(v.validate_input(input_fixture(country=code)))

    def test_unicode_and_whitespace_country_rejected(self):
        for code in [' NZ', 'NZ ', 'NZ\n', '\u0131s', '\u017fE', '\uff2e\uff3a']:
            with self.subTest(code=code):
                self.assertTrue(v.validate_input(input_fixture(country=code)))

    def test_missing_category(self):
        self.assertTrue(v.validate_input({'country': 'NZ'}))

    def test_missing_country(self):
        self.assertTrue(v.validate_input({'category_focus': 'backpacks'}))

    def test_blank_required_strings(self):
        for key in ['category_focus', 'country']:
            for value in ['', ' ', '\t\n', None, False, 12, [], {}]:
                with self.subTest(key=key, value=value):
                    self.assertTrue(v.validate_input(input_fixture(**{key: value})))

    def test_wrong_top_level(self):
        for value in [None, [], 'text', True, 2]:
            with self.subTest(value=value):
                self.assertTrue(v.validate_input(value))

    def test_pascal_case_rejected(self):
        self.assertTrue(v.validate_input({'CategoryFocus':'backpacks','Country':'NZ'}))

    def test_unknown_override_fields_rejected(self):
        for key in ['keyword_count', 'include_brands', 'include_adjacent_categories', 'error']:
            with self.subTest(key=key):
                self.assertTrue(v.validate_input(input_fixture(**{key: True})))

    def test_empty_lists_allowed(self):
        self.assertEqual([], v.validate_input(input_fixture(seed_keywords=[], optional_constraints=[])))

    def test_null_lists_rejected(self):
        for key in ['seed_keywords', 'optional_constraints']:
            with self.subTest(key=key):
                self.assertTrue(v.validate_input(input_fixture(**{key:None})))

    def test_wrong_list_types_rejected(self):
        for key in ['seed_keywords', 'optional_constraints']:
            for value in ['backpack', {}, 1, True]:
                with self.subTest(key=key, value=value):
                    self.assertTrue(v.validate_input(input_fixture(**{key:value})))

    def test_seed_values_must_be_strings(self):
        for value in [None, 1, True, {}, []]:
            with self.subTest(value=value):
                self.assertTrue(v.validate_input(input_fixture(seed_keywords=[value])))

    def test_blank_and_duplicate_seeds_are_hints(self):
        self.assertEqual([], v.validate_input(input_fixture(seed_keywords=['', '  ', 'Backpack', 'backpack'])))

    def test_constraint_nullable_fields(self):
        self.assertEqual([], v.validate_input(input_fixture(optional_constraints=[{key:None for key in v.CONSTRAINT_KEYS}])))

    def test_constraint_free_text(self):
        self.assertEqual([], v.validate_input(input_fixture(optional_constraints=[{key:'free text' for key in v.CONSTRAINT_KEYS}])))

    def test_empty_constraint_object(self):
        self.assertEqual([], v.validate_input(input_fixture(optional_constraints=[{}])))

    def test_constraint_items_must_be_objects(self):
        for value in [None, [], 'adults', 1, True]:
            with self.subTest(value=value):
                self.assertTrue(v.validate_input(input_fixture(optional_constraints=[value])))

    def test_constraint_unknown_keys(self):
        self.assertTrue(v.validate_input(input_fixture(optional_constraints=[{'age_range':'18-24'}])))

    def test_constraint_wrong_value_type(self):
        for value in [1, False, [], {}]:
            with self.subTest(value=value):
                self.assertTrue(v.validate_input(input_fixture(optional_constraints=[{'price_tier':value}])))

    def test_alternative_segments_not_intersected_by_validator(self):
        value = input_fixture(optional_constraints=[{'price_tier':'budget'}, {'price_tier':'premium'}])
        self.assertEqual([], v.validate_input(value))

    def test_semantic_ambiguity_not_misclassified_as_schema_failure(self):
        self.assertEqual([], v.validate_input(input_fixture(category_focus='accessories')))

    def test_validation_does_not_mutate_input(self):
        value = input_fixture(country='nZl', seed_keywords=[' BACKPACK ', 'backpack'])
        saved = copy.deepcopy(value)
        v.validate_input(value)
        self.assertEqual(saved, value)

    def test_packaged_input_examples(self):
        for name in ['input.minimal.json', 'input.segments.json']:
            self.assertEqual([], v.validate_input(json.loads((ROOT/'examples'/name).read_text())))


class ParsingTests(unittest.TestCase):
    def test_strict_json_object(self):
        self.assertEqual(input_fixture(), v.parse_json(json.dumps(input_fixture())))

    def test_duplicate_top_level_key(self):
        with self.assertRaises(v.JsonContractError):
            v.parse_json('{"country":"NZ","country":"US"}')

    def test_duplicate_nested_key(self):
        with self.assertRaises(v.JsonContractError):
            v.parse_json('{"optional_constraints":[{"gender":"women","gender":"men"}]}')

    def test_nonfinite_values(self):
        for token in ['NaN', 'Infinity', '-Infinity']:
            with self.subTest(token=token):
                with self.assertRaises(v.JsonContractError):
                    v.parse_json('{"value":' + token + '}')

    def test_invalid_and_multiple_json(self):
        for raw in ['', '{} {}', '{"country": "NZ",}', '```json\n{}\n```', '{', 'null text']:
            with self.subTest(raw=raw):
                with self.assertRaises(v.JsonContractError):
                    v.parse_json(raw)


class OutputTests(unittest.TestCase):
    def test_minimum_and_maximum(self):
        for size in [25, 60]:
            with self.subTest(size=size):
                self.assertEqual([], v.validate_output(output_fixture(size)))

    def test_out_of_range_counts(self):
        for size in [0, 24, 61]:
            with self.subTest(size=size):
                self.assertTrue(v.validate_output(output_fixture(size)))

    def test_missing_output_keys(self):
        for key in ['category_focus','optimized_keywords']:
            value = output_fixture()
            del value[key]
            self.assertTrue(v.validate_output(value))

    def test_unknown_output_keys(self):
        value = output_fixture()
        value['country'] = 'NZ'
        self.assertTrue(v.validate_output(value))

    def test_output_top_level_type(self):
        for value in [None, [], 'text', True, 1]:
            self.assertTrue(v.validate_output(value))

    def test_keyword_types_and_blank_strings(self):
        for keyword in [None, 1, True, {}, [], '', '  ']:
            with self.subTest(keyword=keyword):
                value = output_fixture()
                value['optimized_keywords'][0] = keyword
                self.assertTrue(v.validate_output(value))

    def test_exact_duplicate(self):
        value = output_fixture()
        value['optimized_keywords'][1] = value['optimized_keywords'][0]
        self.assertTrue(v.validate_output(value))

    def test_case_duplicate(self):
        value = output_fixture()
        value['optimized_keywords'][1] = value['optimized_keywords'][0].upper()
        self.assertTrue(v.validate_output(value))

    def test_hyphen_spacing_duplicate(self):
        value = output_fixture()
        value['optimized_keywords'][:2] = ['carry-on backpack', 'carry on backpack']
        self.assertTrue(v.validate_output(value))

    def test_unicode_dash_variant_duplicate(self):
        value = output_fixture()
        value['optimized_keywords'][:2] = ['carry\u2011on backpack', 'carry on backpack']
        self.assertTrue(v.validate_output(value))

    def test_dirty_whitespace(self):
        for keyword in [' backpack', 'backpack ', 'travel  backpack', 'travel\nbackpack', 'travel\tbackpack']:
            value = output_fixture()
            value['optimized_keywords'][0] = keyword
            self.assertTrue(v.validate_output(value))

    def test_urls_hashtags_citations_rejected(self):
        for keyword in ['https://example.com', 'www.example.com', '#backpack', 'backpack [1]', 'backpack \ue200cite\ue202source\ue201', '[backpack](source)']:
            value = output_fixture()
            value['optimized_keywords'][0] = keyword
            self.assertTrue(v.validate_output(value))

    def test_hash_in_legitimate_word_not_a_hashtag(self):
        value = output_fixture()
        value['optimized_keywords'][0] = 'A# tuning fork'
        self.assertEqual([], v.validate_output(value))

    def test_six_word_phrase_is_not_automatically_invalid(self):
        value = output_fixture()
        value['optimized_keywords'][0] = 'travel backpack with padded laptop compartment'
        self.assertEqual([], v.validate_output(value))

    def test_distinct_meanings_not_blindly_stemmed(self):
        value = output_fixture()
        value['optimized_keywords'][:2] = ['reading glasses', 'drinking glass']
        self.assertEqual([], v.validate_output(value))

    def test_wrong_keyword_collection_type(self):
        for keywords in [None, 'backpack', {}, 1]:
            value = output_fixture()
            value['optimized_keywords'] = keywords
            self.assertTrue(v.validate_output(value))

    def test_success_error_must_be_omitted(self):
        for error in ['', None, 'Failure', False]:
            value = output_fixture()
            value['error'] = error
            self.assertTrue(v.validate_output(value))

    def test_valid_error(self):
        self.assertEqual([], v.validate_output(v.error_response('Research is unavailable.')))

    def test_error_message_required_nonblank_string(self):
        for error in ['', ' ', None, False, 1, {}, []]:
            self.assertTrue(v.validate_output({'category_focus':'','optimized_keywords':[],'error':error}))

    def test_error_no_partial_keywords(self):
        value = v.error_response('Research is unavailable.')
        value['optimized_keywords'] = ['backpack']
        self.assertTrue(v.validate_output(value))

    def test_error_no_partial_category(self):
        value = v.error_response('Research is unavailable.')
        value['category_focus'] = 'backpacks'
        self.assertTrue(v.validate_output(value))

    def test_error_no_null_result_fields(self):
        for key in ['category_focus','optimized_keywords']:
            value = v.error_response('Research is unavailable.')
            value[key] = None
            self.assertTrue(v.validate_output(value))

    def test_error_no_extra_status(self):
        value = v.error_response('Research is unavailable.')
        value['status'] = 'error'
        self.assertTrue(v.validate_output(value))

    def test_error_no_url(self):
        with self.assertRaises(ValueError):
            v.error_response('Visit https://example.com.')

    def test_packaged_error_example(self):
        self.assertEqual([], v.validate_output(json.loads((ROOT/'examples/output.error.json').read_text())))


class CliTests(unittest.TestCase):
    def run_cli(self, kind, payload):
        return subprocess.run([sys.executable, str(ROOT/'scripts/validate_json.py'), kind, '-'], input=payload, text=True, capture_output=True, check=False)

    def test_stdin_success(self):
        result = self.run_cli('input', json.dumps(input_fixture(country='NZL')))
        self.assertEqual(0, result.returncode)
        report = json.loads(result.stdout)
        self.assertTrue(report['valid'])
        self.assertEqual('NZ', report['resolved_country']['alpha_2'])

    def test_stdin_invalid(self):
        result = self.run_cli('input', '{"country":554}')
        self.assertEqual(1, result.returncode)
        self.assertFalse(json.loads(result.stdout)['valid'])

    def test_stdin_malformed(self):
        result = self.run_cli('input', '{')
        self.assertEqual(1, result.returncode)
        self.assertFalse(json.loads(result.stdout)['valid'])

    def test_file_utf8_bom(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder)/'input.json'
            path.write_text(json.dumps(input_fixture()), encoding='utf-8-sig')
            result = subprocess.run([sys.executable, str(ROOT/'scripts/validate_json.py'), 'input', str(path)], text=True, capture_output=True, check=False)
            self.assertEqual(0, result.returncode)

    def test_missing_file(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder)/'missing.json'
            result = subprocess.run([sys.executable, str(ROOT/'scripts/validate_json.py'), 'input', str(path)], text=True, capture_output=True, check=False)
            self.assertEqual(2, result.returncode)
            self.assertFalse(json.loads(result.stdout)['valid'])

    def test_command_independent_of_working_directory(self):
        with tempfile.TemporaryDirectory() as folder:
            result = subprocess.run([sys.executable, str(ROOT/'scripts/validate_json.py'), 'input', str(ROOT/'examples/input.minimal.json')], cwd=folder, text=True, capture_output=True, check=False)
            self.assertEqual(0, result.returncode)


if __name__ == '__main__':
    unittest.main(verbosity=2)
