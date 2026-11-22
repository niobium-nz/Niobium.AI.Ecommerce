"""Lossless input handling and result contracts shared by the looper and its tests."""
from __future__ import annotations

import copy
import hashlib
import json
import re
from collections import Counter
from decimal import Decimal
from pathlib import Path
from typing import Any

from metadata_policy import lookup_completeness, optional_metadata
from resource_bindings import SchemaBindings

ROOT = Path(__file__).resolve().parent
BINDINGS = SchemaBindings(ROOT)
UUID = r"[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}"
CHILD_SCHEMA = BINDINGS.path('urn:niobium:ecommerce-competitive-researcher:schema:contracts')
SOURCE_SCHEMA = BINDINGS.path('urn:niobium:ecommerce-product-discoverer:schema:handoff')
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


def validate(path: Path, value: Any, fragment: str = '') -> None:
    BINDINGS.validator(path, fragment).validate(value)


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


def select_source(folder: dict, children: list, complete: bool | None, folder_id: str, job_id: str) -> str:
    if (folder.get('id') != folder_id or folder.get('name') != 'discovery'
            or folder.get('mimeType') != 'application/vnd.google-apps.folder'
            or folder_id == 'root'):
        raise ValueError('Invalid discovery folder')
    optional_metadata(folder)
    lookup_completeness(complete)
    matches = [r for r in children if r.get('name') == job_id + '.json'
               and folder_id in r.get('parents', [])]
    if len(matches) != 1:
        raise ValueError('Missing or ambiguous exact-name source')
    row = matches[0]
    optional_metadata(row)
    if row.get('mimeType') != 'application/json' or not row.get('id'):
        raise ValueError('Not a raw discovery JSON file')
    return row['id']


def freeze_source(before: dict, raw: bytes, after: dict, file_id: str,
                  folder_id: str, job_id: str) -> tuple[Any, dict]:
    fields = ('id', 'name', 'parents', 'mimeType', 'version', 'size', 'md5Checksum')
    if any(not exact_equal(before.get(k), after.get(k)) for k in fields):
        raise ValueError('Source changed during read')
    if (before.get('id') != file_id or before.get('name') != job_id + '.json'
            or before.get('mimeType') != 'application/json'
            or folder_id not in before.get('parents', [])
            or not isinstance(before.get('version'), str) or not before['version'].strip()):
        raise ValueError('Source identity or version mismatch')
    optional_metadata(before)
    optional_metadata(after)
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
    job = {'job_id': entry['job_id']}
    job.update({key: entry['inputs'][key] for key in ('source_country', 'target_country', 'keyword')})
    return copy.deepcopy({'job': job, 'product': entry['product'], 'ads': entry['ads']})


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


def diagnostic(code: str, index=None, cluster=None) -> dict:
    return {'code': code, 'message': code.replace('_', ' '), 'source_index': index,
            'product_cluster_id': cluster, 'details': {}}


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
