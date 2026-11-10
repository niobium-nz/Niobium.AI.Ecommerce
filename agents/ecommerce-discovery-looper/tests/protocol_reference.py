"""Network-free contract reference for tests; NOT a production agent/connector runner."""
from __future__ import annotations

import copy
import hashlib
import json
import re
from collections import Counter
from dataclasses import dataclass
from decimal import Decimal
from functools import lru_cache
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator, FormatChecker
from referencing import Registry, Resource

ROOT = Path(__file__).resolve().parents[1]
AGENTS = ROOT.parent
UUID = r"[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}"
CHILD_SCHEMA = AGENTS / 'ecommerce-competitive-researcher/schemas/contracts.schema.json'
SOURCE_SCHEMA = AGENTS / 'ecommerce-product-discoverer/schemas/handoff.schema.json'
STATUS_KEYS = ('succeeded', 'partial', 'failed', 'duplicate', 'halted', 'not_run')


def strict_loads(text: str | bytes) -> Any:
    """Preserve all integer and decimal digits and reject duplicate object keys."""
    if isinstance(text, bytes):
        text = text.decode('utf-8', errors='strict')

    def pairs(entries):
        obj = {}
        for key, value in entries:
            if key in obj:
                raise ValueError('Duplicate JSON key')
            obj[key] = value
        return obj

    def bad_number(_):
        raise ValueError('Non-JSON number')

    return json.loads(text, object_pairs_hook=pairs, parse_float=Decimal, parse_constant=bad_number)


def load(path: Path) -> Any:
    return strict_loads(path.read_bytes())


@lru_cache(maxsize=1)
def registry() -> Registry:
    paths = [SOURCE_SCHEMA, SOURCE_SCHEMA.with_name('input.schema.json'),
             AGENTS / 'ecommerce-product-discoverer/skills/ecommerce-cluster-ads/schemas/output.schema.json',
             CHILD_SCHEMA, ROOT / 'schemas/contracts.schema.json']
    return Registry().with_resources([(p.as_uri(), Resource.from_contents(load(p))) for p in paths])


def validate(path: Path, value: Any, fragment: str = '') -> None:
    Draft202012Validator({'$ref': path.as_uri() + fragment}, registry=registry(),
                         format_checker=FormatChecker()).validate(value)


def exact_equal(left: Any, right: Any) -> bool:
    if type(left) is not type(right):
        return False
    if isinstance(left, dict):
        return left.keys() == right.keys() and all(exact_equal(v, right[k]) for k, v in left.items())
    if isinstance(left, list):
        return len(left) == len(right) and all(exact_equal(a, b) for a, b in zip(left, right))
    return left == right


def validate_request(request: Any, config: Any) -> None:
    validate(ROOT / 'schemas/contracts.schema.json', request, '#/$defs/input')
    validate(ROOT / 'schemas/contracts.schema.json', config, '#/$defs/config')
    if not re.fullmatch(UUID, request['job_id']):
        raise ValueError('Invalid upstream UUID')
    if not re.fullmatch(r'[A-Za-z0-9_-]+', config['discovery_folder_id']):
        raise ValueError('Invalid folder ID')


def select_source(folder: dict, children: list, complete: bool, folder_id: str, job_id: str) -> str:
    if (folder.get('id') != folder_id or folder.get('name') != 'discovery'
            or folder.get('mimeType') != 'application/vnd.google-apps.folder'
            or folder.get('trashed') is not False or folder_id == 'root'):
        raise ValueError('Invalid discovery folder')
    if complete is not True:
        raise ValueError('Incomplete lookup')
    matches = [r for r in children if r.get('name') == job_id + '.json'
               and folder_id in r.get('parents', []) and r.get('trashed') is False]
    if len(matches) != 1:
        raise ValueError('Missing or ambiguous exact-name source')
    row = matches[0]
    if row.get('mimeType') != 'application/json' or not row.get('id'):
        raise ValueError('Not a raw discovery JSON file')
    return row['id']


def freeze_source(before: dict, raw: bytes, after: dict, file_id: str,
                  folder_id: str, job_id: str) -> tuple[Any, dict]:
    fields = ('id', 'name', 'parents', 'mimeType', 'trashed', 'version', 'size', 'md5Checksum')
    if any(not exact_equal(before.get(k), after.get(k)) for k in fields):
        raise ValueError('Source changed during read')
    if (before.get('id') != file_id or before.get('name') != job_id + '.json'
            or before.get('mimeType') != 'application/json' or before.get('trashed') is not False
            or folder_id not in before.get('parents', [])
            or not isinstance(before.get('version'), str) or not before['version'].strip()):
        raise ValueError('Source identity or version mismatch')
    size = before.get('size')
    if not isinstance(size, str) or not re.fullmatch(r'0|[1-9][0-9]*', size) or int(size) != len(raw):
        raise ValueError('Incomplete source bytes')
    checksum = before.get('md5Checksum')
    if checksum is not None and hashlib.md5(raw).hexdigest() != checksum:
        raise ValueError('Source checksum mismatch')
    entries = strict_loads(raw)
    validate_batch(entries, job_id)
    return entries, {'file_id': file_id, 'filename': before['name'], 'version': before['version'],
                     'sha256': hashlib.sha256(raw).hexdigest()}


def validate_batch(entries: Any, job_id: str) -> None:
    validate(SOURCE_SCHEMA, entries)
    clusters, original_ads = set(), {}
    for entry in entries:
        if entry['job_id'] != job_id:
            raise ValueError('Mixed upstream jobs')
        if not exact_equal(entry['inputs'], entries[0]['inputs']) or not exact_equal(entry['provenance'], entries[0]['provenance']):
            raise ValueError('Mixed source inputs or provenance')
        cluster = entry['product']['cluster_id']
        if cluster in clusters:
            raise ValueError('Duplicate source cluster')
        clusters.add(cluster)
        ids = [ad['ad_archive_id'] for ad in entry['ads']]
        if ids != entry['product']['ad_archive_ids'] or len(ids) != len(set(ids)):
            raise ValueError('Source product/ad traceability mismatch')
        if entry['provenance']['successful_pages'] > entry['provenance']['search_attempts']:
            raise ValueError('Impossible source page counts')
        for ad in entry['ads']:
            key = ad['ad_archive_id']
            if key in original_ads and not exact_equal(ad, original_ads[key]):
                raise ValueError('Conflicting original ad across products')
            original_ads[key] = ad


def child_payload(entry: dict) -> dict:
    return copy.deepcopy({'job': {'job_id': entry['job_id'], **entry['inputs']},
                          'product': entry['product'], 'ads': entry['ads']})


def check_child(result: Any, entry: dict, seen_signals: set, seen_files: set) -> None:
    validate(CHILD_SCHEMA, result, '#/$defs/result')
    if result['likely_product_name'] != entry['product']['likely_product_name']:
        raise ValueError('Child product name mismatch')
    if any(e['product_cluster_id'] != entry['product']['cluster_id'] for e in result['errors']):
        raise ValueError('Child cluster correlation mismatch')
    if result['published']:
        sid = result['signal_id']
        filename = sid + '.json'
        path = f"discovery/{entry['job_id']}/{filename}"
        if (result['output_filename'] != filename or result['output_path'] != path
                or sid == entry['job_id'] or sid in seen_signals or result['output_file_id'] in seen_files):
            raise ValueError('Child publication correlation mismatch')
        if result['status'] == 'partial' and any(e['stage'] not in ('competition', 'scoring') for e in result['errors']):
            raise ValueError('Partial result hides an infrastructure failure')


def disposition(result: dict) -> tuple[str, bool]:
    if result['status'] in ('succeeded', 'partial'):
        return result['status'], False
    if result['status'] == 'published_registry_pending' or result['signal_id'] is not None:
        return 'halted', True
    errors = result['errors']
    if all(e['stage'] == 'duplicate_check' and e['code'] == 'duplicate_found' for e in errors):
        return 'duplicate', False
    if all(e['stage'] in ('input', 'normalization', 'competition', 'scoring') for e in errors):
        return 'failed', False
    if all(e['stage'] in ('preflight', 'duplicate_check') for e in errors):
        return 'halted', False
    return 'halted', True


@dataclass
class Receipt:
    invocation_id: str
    result: Any
    terminal: bool = True
    quiescent: bool = True
    correlated: bool = True


class HostStop(Exception):
    def __init__(self, code: str, recovery: bool = False):
        self.code, self.recovery = code, recovery
        super().__init__(code)


def diagnostic(code: str, index=None, cluster=None) -> dict:
    return {'code': code, 'message': code.replace('_', ' '), 'source_index': index,
            'product_cluster_id': cluster, 'details': {}}


def run_reference(entries: Any, source: dict, request: Any, config: Any, host) -> dict:
    """Pure dispatch model using an injected MOCK host; contains no real I/O bindings."""
    result = {'loop_id': None, 'job_id': None, 'status': 'failed', 'source': None,
              'counts': {'total': None, 'dispatched': 0, **dict.fromkeys(STATUS_KEYS, 0)},
              'items': [], 'recovery_required': False, 'errors': []}
    try:
        validate(ROOT / 'schemas/contracts.schema.json', request, '#/$defs/input')
        if not re.fullmatch(UUID, request['job_id']):
            raise ValueError('Invalid upstream UUID')
        result['job_id'] = request['job_id']
    except Exception:
        result['errors'] = [diagnostic('input_invalid')]
        return result
    try:
        loop_id = host.uuid()
        if not isinstance(loop_id, str) or not re.fullmatch(UUID, loop_id):
            raise ValueError('UUID unavailable')
        result['loop_id'] = loop_id
    except Exception:
        result['errors'] = [diagnostic('uuid_unavailable')]
        return result
    try:
        validate_request(request, config)
        validate(ROOT / 'schemas/contracts.schema.json', source, '#/$defs/result/properties/source')
        if source['filename'] != request['job_id'] + '.json':
            raise ValueError('Source name mismatch')
        validate_batch(entries, request['job_id'])
    except Exception:
        result['errors'] = [diagnostic('input_or_source_invalid')]
        return result
    result['source'] = copy.deepcopy(source)
    result['items'] = [{'source_index': i, 'product_cluster_id': e['product']['cluster_id'],
                        'likely_product_name': e['product']['likely_product_name'], 'status': 'not_run',
                        'invocation_id': None, 'child_result': None, 'errors': []}
                       for i, e in enumerate(entries)]
    if not entries:
        result['status'] = 'empty'
        return finish(result)
    try:
        host.preflight(config)
    except HostStop as exc:
        result['errors'] = [diagnostic(exc.code)]
        result['recovery_required'] = exc.recovery
        return finish(result)
    except Exception:
        result['errors'] = [diagnostic('capability_unavailable')]
        return finish(result)
    seen_signals, seen_files, seen_invocations = set(), set(), set()
    result['status'] = 'completed'
    for index, entry in enumerate(entries):
        item = result['items'][index]
        try:
            host.before_dispatch(index, child_payload(entry))  # Includes durable intent, before send.
            result['counts']['dispatched'] += 1
            receipt = host.invoke(index, child_payload(entry))
            if (not isinstance(receipt, Receipt) or not isinstance(receipt.invocation_id, str)
                    or not receipt.invocation_id.strip() or receipt.invocation_id in seen_invocations):
                raise HostStop('child_receipt_invalid', True)
            item['invocation_id'] = receipt.invocation_id
            seen_invocations.add(receipt.invocation_id)
            if not receipt.terminal or not receipt.quiescent or not receipt.correlated:
                raise HostStop('child_outcome_unknown', True)
            try:
                check_child(receipt.result, entry, seen_signals, seen_files)
            except Exception as exc:
                raise HostStop('child_result_invalid', True) from exc
            item['child_result'] = copy.deepcopy(receipt.result)
            item['status'], recovery = disposition(receipt.result)
            host.record_terminal(index, receipt)  # Must durably succeed before next entry.
            if host.recovery_hold():
                raise HostStop('recovery_hold_active', True)
            if item['status'] == 'halted':
                raise HostStop('child_requires_reconciliation' if recovery else 'child_infrastructure_failed', recovery)
            if receipt.result['published']:
                seen_signals.add(receipt.result['signal_id'])
                seen_files.add(receipt.result['output_file_id'])
        except Exception as exc:
            problem = exc if isinstance(exc, HostStop) else HostStop('dispatch_or_journal_unknown', True)
            item['status'] = 'halted'
            item['errors'].append(diagnostic(problem.code, index, item['product_cluster_id']))
            result['status'] = 'stopped'
            result['recovery_required'] = problem.recovery
            if problem.recovery:
                try:
                    host.hold(problem.code)
                except Exception:
                    result['errors'].append(diagnostic('recovery_hold_unconfirmed', index, item['product_cluster_id']))
            break
    if result['status'] == 'completed' and any(i['status'] in ('partial', 'failed') for i in result['items']):
        result['status'] = 'completed_with_errors'
    return finish(result)


def finish(result: dict) -> dict:
    c = Counter(i['status'] for i in result['items'])
    result['counts'].update({key: c[key] for key in STATUS_KEYS})
    result['counts']['total'] = len(result['items']) if result['source'] is not None else None
    validate_summary(result)
    return result


def validate_summary(result: dict) -> None:
    validate(ROOT / 'schemas/contracts.schema.json', result, '#/$defs/result')
    if result['source'] is None:
        if result['items'] or result['counts']['total'] is not None or result['counts']['dispatched']:
            raise ValueError('Unvalidated source cannot have dispatched entries')
        return
    items, counts = result['items'], result['counts']
    if result['source']['filename'] != result['job_id'] + '.json':
        raise ValueError('Summary source name mismatch')
    if [i['source_index'] for i in items] != list(range(len(items))):
        raise ValueError('Missing/reordered source indexes')
    observed = Counter(i['status'] for i in items)
    if counts['total'] != len(items) or any(counts[k] != observed[k] for k in STATUS_KEYS):
        raise ValueError('Counts do not reconcile')
    attempted_min = sum(i['status'] in ('succeeded', 'partial', 'failed', 'duplicate')
                        or i['invocation_id'] is not None for i in items)
    attempted_max = len(items) - counts['not_run']
    if not attempted_min <= counts['dispatched'] <= attempted_max:
        raise ValueError('Dispatch count does not reconcile')
    seen_signals, seen_files = set(), set()
    for item in items:
        child = item['child_result']
        if child is not None:
            if item['invocation_id'] is None:
                raise ValueError('Child result without invocation correlation')
            check_child(child, {'job_id': result['job_id'], 'product': {
                'cluster_id': item['product_cluster_id'], 'likely_product_name': item['likely_product_name']}},
                seen_signals, seen_files)
            if child['published']:
                seen_signals.add(child['signal_id'])
                seen_files.add(child['output_file_id'])
            if item['status'] != 'halted' and disposition(child)[0] != item['status']:
                raise ValueError('Child outcome misclassified')
    status = result['status']
    if status.startswith('completed'):
        if not items or counts['halted'] or counts['not_run'] or counts['dispatched'] != len(items):
            raise ValueError('Incomplete loop reported completed')
        if (counts['partial'] + counts['failed'] > 0) != (status == 'completed_with_errors'):
            raise ValueError('Item errors hidden by loop status')
    if status == 'stopped' and not (counts['halted'] or counts['not_run']):
        raise ValueError('Stop without a halted or unprocessed entry')
