"""Network-free reference helpers and mockable orchestration, NOT a production runner.

Semantic review, visible browsing, board inspection, folder management and real
persistence are host responsibilities. Tests cannot establish those capabilities.
"""
from __future__ import annotations
import copy
import hashlib
import ipaddress
import json
import math
import re
from pathlib import Path
from typing import Any
from urllib.parse import urlsplit, unquote

import pycountry
from jsonschema import Draft202012Validator, FormatChecker
from referencing import Registry, Resource

ROOT = Path(__file__).resolve().parents[1]
SCHEMA = ROOT / 'schemas/contracts.schema.json'
SKILLS = ROOT / 'skills'
UUID = re.compile(r'^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$')
PROFILE = SKILLS / 'ecommerce-profile-offers/schemas'
MARKET = SKILLS / 'ecommerce-develop-strategy/schemas'
IMAGE = SKILLS / 'ecommerce-draft-prompts/schemas'


class GateError(ValueError):
    def __init__(self, code: str, message: str, details: dict | None = None, clarification: bool = False):
        super().__init__(message)
        self.code, self.details, self.clarification = code, details or {}, clarification


def strict_loads(text: str) -> Any:
    def pairs(items):
        result = {}
        for key, value in items:
            if key in result:
                raise GateError('invalid_json', 'Duplicate object key', {'key': key})
            result[key] = value
        return result

    def fail(value):
        raise GateError('invalid_json', 'Nonfinite JSON number', {'value': value})

    def finite(value):
        number = float(value)
        return number if math.isfinite(number) else fail(value)

    return json.loads(text, object_pairs_hook=pairs, parse_constant=fail, parse_float=finite)


def read(path: Path) -> Any:
    return strict_loads(path.read_text(encoding='utf-8'))


def validator(path: Path = SCHEMA, fragment: str = '') -> Draft202012Validator:
    resources = [(p.as_uri(), Resource.from_contents(read(p))) for p in ROOT.rglob('*.schema.json')]
    return Draft202012Validator({'$ref': path.as_uri() + fragment},
        registry=Registry().with_resources(resources), format_checker=FormatChecker())


def validate(value: Any, definition: str) -> None:
    validator(fragment='#/$defs/' + definition).validate(value)


def public_url(value: str) -> None:
    """Obvious syntactic/private-target checks only; full navigation policy is in the skill."""
    p = urlsplit(value)
    if p.scheme.lower() not in ('https', 'http') or not p.hostname or p.username is not None or p.password is not None:
        raise GateError('invalid_url', 'Public HTTP(S) URL without credentials required')
    if re.search(r'[\s\x00-\x1f\x7f\\]', value) or '%' in p.netloc:
        raise GateError('invalid_url', 'Unsafe URL characters')
    host = p.hostname.rstrip('.').lower()
    try:
        addr = ipaddress.ip_address(host)
    except ValueError:
        if '.' not in host or host.endswith(('.localhost', '.local', '.internal', '.test')) or host == 'localhost':
            raise GateError('invalid_url', 'Local/private hostname not allowed')
        if re.fullmatch(r'[0-9.]+', host) or host.startswith('0x'):
            raise GateError('invalid_url', 'Ambiguous numeric hostname not allowed')
    else:
        if not addr.is_global:
            raise GateError('invalid_url', 'Private or non-global address not allowed')
    try:
        _ = p.port
    except ValueError as exc:
        raise GateError('invalid_url', 'Invalid port') from exc


def validate_input(value: Any) -> None:
    validate(value, 'input')
    public_url(value['landing_page_url'])
    # Exact assigned-country semantics are also enforced by the referenced schema.
    if pycountry.countries.get(alpha_2=value['target_country'].upper()) is None:
        raise GateError('input_invalid', 'Assigned alpha-2 country required')
    costs = value['costs']
    if pycountry.currencies.get(alpha_3=costs['currency'].upper()) is None:
        raise GateError('input_invalid', 'Recognized monetary currency required')
    for field in ('cogs_per_unit', 'extra_unit_cogs_per_order'):
        if type(costs[field]) not in (int, float) or not math.isfinite(costs[field]):
            raise GateError('input_invalid', 'Finite numeric cost required', {'field': field})
    for field in ('sales_tax', 'payment_processing_fees'):
        if re.fullmatch(r'\s*[0-9]+(?:\.[0-9]+)?\s*%\s*', costs[field]):
            raise GateError('ambiguous_costs', 'Percentage requires calculation basis', {'field': field})


def marketing_input(value: dict, profile: dict) -> dict:
    p = profile['product']
    result = dict(copy.deepcopy(value['costs']), target_market_country=value['target_country'],
        competitor_used_product_name=p['name'], competitor_claims=copy.deepcopy(p['key_claims']),
        ingredients_or_materials=copy.deepcopy(p['ingredients_or_materials']),
        competitor_marketing_how_it_wins=copy.deepcopy(p['how_it_wins']))
    validator(MARKET / 'input.schema.json').validate(result)
    return result


def prompt_input(strategy: dict, location: str) -> dict:
    result = dict(copy.deepcopy(strategy), product_reference_board=location)
    validator(IMAGE / 'input.schema.json').validate(result)
    return result


def profile_gate(profile: Any, landing_url: str) -> list[dict]:
    validator(PROFILE / 'output.schema.json').validate(profile)
    if 'product' not in profile:
        raise GateError('profiling_failed', profile['error'])
    if profile['url'] != landing_url:
        raise GateError('profile_mismatch', 'Profile URL does not echo the supplied URL')
    p = profile['product']
    if p is None or not isinstance(p['name'], str) or not p['name'].strip():
        raise GateError('product_unidentified', 'Supported product name required', clarification=True)
    errors = []
    if 'error' in profile:
        errors.append(diagnostic('profile_partial_failure', 'profile', profile['error'], {'blockers': profile['blockers']}))
    errors.extend(diagnostic('profile_limitation', 'profile', b, severity='warning') for b in profile['blockers'])
    return errors


def marketing_gate(strategy: Any) -> None:
    validator(MARKET / 'output.schema.json', '#/$defs/strategy').validate(strategy)
    p = strategy['product_details']
    if p['recommended_primary_product_name'] not in p['suggested_product_names']:
        raise GateError('strategy_invalid', 'Primary name is not one of the proposed names')
    for seg in strategy['customer_segments']:
        if seg['shared_context_snapshot']['recommended_product_name'] != p['recommended_primary_product_name']:
            raise GateError('strategy_invalid', 'Shared product identity differs')
        for a in seg['angle_and_trigger_matrix']:
            if a['recommended_product_name_for_this_angle'] not in a['suggested_product_names_for_this_angle']:
                raise GateError('strategy_invalid', 'Angle name is not one of its proposals')
    plan = strategy['mobile_first_landing_page_plan']
    if plan['mobile_first_section_order'] != [s['section_name'] for s in plan['creative_assets_needed_for_each_section']]:
        raise GateError('strategy_invalid', 'Section order and asset plans differ')
    ids = [a['asset_id'] for section in plan['creative_assets_needed_for_each_section'] for a in section['asset_needs']]
    if len(ids) != len(set(ids)):
        raise GateError('strategy_invalid', 'Repeated asset definition')


def image_gate(result: Any, forbidden_locations: list[str]) -> None:
    validator(IMAGE / 'output.schema.json').validate(result)
    if result.get('status') == 'needs_clarification':
        raise GateError('image_clarification', result['error'], clarification=True)
    if 'error' in result:
        raise GateError('image_strategy_failed', result['error'])
    ids = [a['asset_id'] for a in result['image_prompts']]
    if len(ids) != len(set(ids)):
        raise GateError('image_strategy_invalid', 'Repeated image asset ID')
    for a in result['image_prompts']:
        match = re.search(r'Aspect ratio: ([1-9][0-9]*):([1-9][0-9]*)\.$', a['prompt'])
        w, h = map(int, match.groups())
        orientation = 'square' if w == h else ('landscape' if w > h else 'portrait')
        if a['orientation'] != orientation:
            raise GateError('image_strategy_invalid', 'Aspect ratio contradicts orientation')
        text = a['prompt'] + a['overlay_copy_suggestion']
        if any(location and location in text for location in forbidden_locations):
            raise GateError('image_strategy_invalid', 'Reference location leaked into prompt/copy')


def diagnostic(code, stage, message, details=None, severity='error'):
    return {'code': code, 'stage': stage, 'severity': severity, 'message': str(message), 'details': details or {}}


def output_path(signal_id: str | None, candidate_id: str) -> str:
    if not UUID.fullmatch(candidate_id) or (signal_id is not None and not UUID.fullmatch(signal_id)):
        raise GateError('invalid_identifier', 'Lowercase UUIDv4 required')
    return f'candidates/{signal_id or "unlinked"}/{candidate_id}.json'


def artifact(value: dict, candidate_id: str, board_url: str, marketing: dict, image: dict, errors: list) -> dict:
    result = {'job_id': value.get('job_id'), 'signal_id': value.get('signal_id'), 'candidate_id': candidate_id,
        'target_country': value['target_country'], 'product_reference_board': board_url,
        'marketing_strategy': copy.deepcopy(marketing), 'image_strategy': copy.deepcopy(image), 'errors': copy.deepcopy(errors)}
    validate(result, 'artifact')
    return result


def verified_receipt(receipt: dict, path: str, raw: bytes) -> str:
    # The host is responsible for resolving/creating and verifying the real folder.
    if not isinstance(receipt, dict) or not all((receipt.get('id'), receipt.get('parent_verified') is True,
        receipt.get('name') == path.rsplit('/', 1)[1], receipt.get('mime_type') == 'application/json',
        receipt.get('trashed') is False, receipt.get('unique_name_count') == 1,
        receipt.get('raw_bytes') == raw)):
        raise GateError('verification_failed', 'Full publication verification failed')
    return receipt['id']


def run(value: Any, config: dict, host: Any) -> dict:
    """Dependency-injected state-machine reference. Host methods are mocked in tests."""
    result = {'job_id': None, 'signal_id': None, 'candidate_id': None, 'target_country': None,
              'status': 'failed', 'published': False, 'output_file_id': None, 'output_path': None, 'errors': []}
    errors, stage = result['errors'], 'input'
    if isinstance(value, dict):
        for key in ('job_id', 'signal_id'):
            v = value.get(key)
            if isinstance(v, str) and UUID.fullmatch(v):
                result[key] = v
        v = value.get('target_country')
        if isinstance(v, str) and len(v) == 2 and pycountry.countries.get(alpha_2=v.upper()):
            result['target_country'] = v
    try:
        validate_input(value)
        stage = 'preflight'
        validate(config, 'config')
        # Includes actual URL policy, full cost semantics, permissions and caller approval gate.
        host.preflight(copy.deepcopy(value), copy.deepcopy(config))
        board = host.resolve_board(value['product_reference_board'], config.get('board_handoff_binding'))
        public_url(board['handoff_url'])
        if not board.get('same_content_verified') or not board.get('durable_verified'):
            raise GateError('board_handoff_unavailable', 'Verified same-board durable reference required', clarification=True)
        stage = 'profile'
        profile = host.profile({'landing_page_url': value['landing_page_url']})
        errors.extend(profile_gate(profile, value['landing_page_url']))
        if not host.identity_supported(profile):
            raise GateError('product_unidentified', 'Supported identity is insufficient', clarification=True)
        stage = 'marketing'
        market = host.strategy(marketing_input(value, profile))
        if isinstance(market, dict) and set(market) == {'error'}:
            raise GateError('marketing_failed', market['error'])
        marketing_gate(market)
        host.review_marketing(market, profile, value['costs'], value['target_country'])
        stage = 'image_strategy'
        image = host.prompts(prompt_input(market, board['readable_location']))
        image_gate(image, [value['product_reference_board'], board['readable_location'], board['handoff_url']])
        host.review_images(image, market, profile, board)
        stage = 'publication'
        host.reverify_board(board)
        candidate = host.uuid4()
        result['candidate_id'] = candidate if isinstance(candidate, str) and UUID.fullmatch(candidate) else None
        if result['candidate_id'] is None:
            raise GateError('uuid_unavailable', 'Trusted lowercase UUIDv4 required')
        payload = artifact(value, candidate, board['handoff_url'], market, image, errors)
        raw = (json.dumps(payload, ensure_ascii=False, allow_nan=False, separators=(',', ':')) + '\n').encode('utf-8')
        path = output_path(value.get('signal_id'), candidate)
        receipt = host.publish(path, raw)
        file_id = verified_receipt(receipt, path, raw)
        result.update(status='partial' if errors else 'succeeded', published=True, output_file_id=file_id, output_path=path)
    except Exception as exc:
        if isinstance(exc, GateError):
            errors.append(diagnostic(exc.code, stage, str(exc), exc.details))
            result['status'] = 'needs_clarification' if exc.clarification else 'failed'
        else:
            errors.append(diagnostic('invalid_' + stage + '_response', stage, type(exc).__name__ + ': ' + str(exc)))
    validate(result, 'result')
    return result


def git_blob(path: Path) -> str:
    data = path.read_bytes()
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
