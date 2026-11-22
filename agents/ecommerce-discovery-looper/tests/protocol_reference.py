"""Network-free mock dispatch reference for the looper's behavioral contract."""
from __future__ import annotations

import copy
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Any
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from looper_contracts import (BINDINGS, CHILD_SCHEMA, ROOT, SOURCE_SCHEMA, STATUS_KEYS, UUID,
                              child_payload, check_child, diagnostic, disposition, exact_equal,
                              finish, freeze_source, load, select_source, strict_loads, validate,
                              validate_batch, validate_request, validate_summary)

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
