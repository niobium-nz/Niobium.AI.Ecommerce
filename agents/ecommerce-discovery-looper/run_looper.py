"""Run or advance one checkpoint of competitive research from a durable journal."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sqlite3
import subprocess
import sys
import time
from contextlib import contextmanager
from decimal import Decimal
from pathlib import Path
from urllib.parse import quote, urlencode
from urllib.request import Request, urlopen
from uuid import uuid4


def contracts():
    import looper_contracts
    return looper_contracts


def dump(value):
    """Encode Decimal as a JSON number without rounding provider evidence."""
    if isinstance(value, Decimal):
        if not value.is_finite():
            raise ValueError('Non-finite JSON number')
        return str(value)
    if isinstance(value, dict):
        return '{' + ','.join(json.dumps(k) + ':' + dump(v) for k, v in value.items()) + '}'
    if isinstance(value, list):
        return '[' + ','.join(map(dump, value)) + ']'
    return json.dumps(value, allow_nan=False, separators=(',', ':'))


class DriveReader:
    fields = 'id,name,parents,mimeType,trashed,version,size,md5Checksum,driveId'

    def __init__(self, token, timeout):
        if not token:
            raise ValueError('Provide GOOGLE_DRIVE_ACCESS_TOKEN for the read-only Drive connection')
        self.token, self.timeout = token, timeout

    def get(self, path='', **params):
        url = 'https://www.googleapis.com/drive/v3/files' + path + '?' + urlencode(params)
        request = Request(url)
        request.add_unredirected_header('Authorization', 'Bearer ' + self.token)
        with urlopen(request, timeout=self.timeout) as response:
            return response.read()

    def metadata(self, file_id):
        return contracts().strict_loads(self.get('/' + quote(file_id, safe=''),
                                     supportsAllDrives='true', fields=self.fields))

    def snapshot(self, folder_id, job_id):
        folder = self.metadata(folder_id)
        matches, tokens = [], set()
        params = {'q': f"'{folder_id}' in parents and name = '{job_id}.json' and trashed = false",
                  'fields': 'nextPageToken,incompleteSearch,files(' + self.fields + ')',
                  'supportsAllDrives': 'true', 'pageSize': 100}
        if folder.get('driveId'):
            params.update(corpora='drive', driveId=folder['driveId'], includeItemsFromAllDrives='true')
        while True:
            page = contracts().strict_loads(self.get(**params))
            if page.get('incompleteSearch') is True or not isinstance(page.get('files'), list):
                raise ValueError('Incomplete or malformed Drive lookup')
            if any(row.get('name') != job_id + '.json' or folder_id not in row.get('parents', [])
                   for row in page['files']):
                raise ValueError('Drive lookup returned an unexpected child')
            matches.extend(page['files'])
            if 'nextPageToken' not in page:
                break
            token = page['nextPageToken']
            if not isinstance(token, str) or not token or token in tokens:
                raise ValueError('Invalid Drive continuation token')
            tokens.add(token)
            params['pageToken'] = token
        file_id = contracts().select_source(folder, matches, True, folder_id, job_id)
        before = self.metadata(file_id)
        raw = self.get('/' + quote(file_id, safe=''), alt='media', supportsAllDrives='true')
        entries, source = contracts().freeze_source(before, raw, self.metadata(file_id), file_id, folder_id, job_id)
        return entries, source, raw


class ResearcherBinding:
    """Call an explicitly installed trusted host adapter using a bounded request."""
    def __init__(self, executable, timeout):
        self.executable, self.timeout = executable.resolve(strict=True), timeout

    def call(self, action, **fields):
        environment = dict(os.environ)
        environment.pop('GOOGLE_DRIVE_ACCESS_TOKEN', None)
        process = subprocess.run([str(self.executable)], input=dump({'action': action, **fields}),
                                 stdout=subprocess.PIPE, stderr=subprocess.DEVNULL,
                                 text=True, encoding='utf-8', timeout=self.timeout, check=True, env=environment)
        reply = contracts().strict_loads(process.stdout)
        if not isinstance(reply, dict):
            raise ValueError('Invalid researcher binding reply')
        return reply

    def scope(self, config):
        reply = self.call('preflight', config=config)
        scope = reply.get('registry_file_id')
        if (reply.get('config') != config or reply.get('serialized_writers') is not True
                or reply.get('terminal_results') is not True or reply.get('recovery_clear') is not True
                or not isinstance(scope, str) or not re.fullmatch(r'[A-Za-z0-9_-]+', scope)
                or scope in ('root', config['discovery_folder_id'])):
            raise ValueError('Researcher binding cannot prove the required production capabilities')
        return scope


class Journal:
    def __init__(self, path):
        try:
            fd = os.open(path, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
        except FileExistsError:
            pass
        else:
            os.close(fd)
        self.db = sqlite3.connect(path, timeout=10, isolation_level=None)
        self.db.row_factory = sqlite3.Row
        self.db.execute('PRAGMA synchronous = FULL')
        self.db.executescript("""
            CREATE TABLE IF NOT EXISTS batches (
                loop_id TEXT PRIMARY KEY, registry TEXT NOT NULL, phase TEXT NOT NULL,
                checkpoint TEXT NOT NULL, source_bytes BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS one_batch_per_registry
            ON batches(registry) WHERE phase NOT IN ('complete', 'stopped');
        """)

    @contextmanager
    def transaction(self):
        self.db.execute('BEGIN IMMEDIATE')
        try:
            yield
            self.db.execute('COMMIT')
        except BaseException:
            self.db.execute('ROLLBACK')
            raise

    def add(self, state, raw):
        try:
            self.db.execute('INSERT INTO batches VALUES (?, ?, ?, ?, ?)',
                            (state['loop_id'], state['registry'], state['phase'], dump(state), raw))
        except sqlite3.IntegrityError as exc:
            raise ValueError('A batch is active or held for this registry in the shared journal') from exc

    def read(self, loop_id, *, with_source=False):
        columns = 'checkpoint, source_bytes' if with_source else 'checkpoint'
        row = self.db.execute(f'SELECT {columns} FROM batches WHERE loop_id = ?', (loop_id,)).fetchone()
        if row is None:
            raise ValueError('Unknown loop ID')
        state = contracts().strict_loads(row['checkpoint'])
        if with_source:
            return state, row['source_bytes']
        return state

    def save(self, state):
        self.db.execute('UPDATE batches SET phase = ?, checkpoint = ? WHERE loop_id = ?',
                        (state['phase'], dump(state), state['loop_id']))


def start(journal, drive, binding, config, job_id):
    contracts().validate_request({'job_id': job_id}, config)
    entries, source, raw = drive.snapshot(config['discovery_folder_id'], job_id)
    registry = binding.scope(config) if entries else ''
    state = {'phase': 'ready' if entries else 'complete', 'loop_id': str(uuid4()),
             'job_id': job_id, 'registry': registry, 'config': config,
             'source': source, 'index': 0, 'intent_id': None, 'dispatched': 0,
             'items': [{'source_index': i, 'product_cluster_id': entry['product']['cluster_id'],
                        'likely_product_name': entry['product']['likely_product_name'], 'status': 'not_run',
                        'invocation_id': None, 'child_result': None, 'errors': []}
                       for i, entry in enumerate(entries)]}
    journal.add(state, raw)
    return state


def halt(state, code, recovery):
    item = state['items'][state['index']]
    item['status'] = 'halted'
    item['errors'].append(contracts().diagnostic(code, item['source_index'], item['product_cluster_id']))
    state['phase'] = 'hold' if recovery else 'stopped'
    return state


def advance(journal, binding, loop_id):
    from jsonschema import ValidationError

    # Take a snapshot of the checkpoint; reserve submission before leaving SQLite.
    with journal.transaction():
        state, raw = journal.read(loop_id, with_source=True)
        phase = state['phase']
        if phase not in ('ready', 'waiting'):
            return state
        if hashlib.sha256(raw).hexdigest() != state['source']['sha256']:
            halt(state, 'pinned_source_digest_mismatch', phase == 'waiting')
            journal.save(state)
            return state
        if phase == 'ready':
            state.update(phase='submitting', intent_id=str(uuid4()))
            journal.save(state)
    index, intent = state['index'], state['intent_id']
    entry = contracts().strict_loads(raw)[index]
    item = state['items'][index]
    correlation = {'loop_id': loop_id, 'intent_id': intent, 'source_index': index,
                   'registry_file_id': state['registry'], 'config': state['config']}

    if phase == 'ready':
        try:
            if binding.scope(state['config']) != state['registry']:
                raise ValueError('Registry binding changed')
        except (OSError, ValueError, subprocess.SubprocessError):
            halt(state, 'researcher_preflight_failed', False)
        else:
            # Persist the attempt before entering the transport, including crash paths.
            state['dispatched'] += 1
            journal.save(state)
            try:
                reply = binding.call('submit', **correlation, input=contracts().child_payload(entry))
                invocation = reply.get('invocation_id')
                if (reply.get('intent_id') != intent or not isinstance(invocation, str) or not invocation.strip()
                        or any(prev['invocation_id'] == invocation for prev in state['items'])):
                    raise ValueError('Missing, reused or miscorrelated invocation ID')
                item['invocation_id'] = invocation
                state['phase'] = 'waiting'
            except (OSError, ValueError, subprocess.SubprocessError):
                halt(state, 'dispatch_outcome_unknown', True)
        journal.save(state)
        return state

    invocation = item['invocation_id']
    try:
        receipt = binding.call('poll', **correlation, invocation_id=invocation)
    except (OSError, subprocess.SubprocessError):
        # Polling the known run changes no child state. A later step can poll it again.
        return state
    except ValueError:
        receipt = {}

    with journal.transaction():
        current = journal.read(loop_id)
        if (current['phase'], current['index'], current['intent_id']) != ('waiting', index, intent):
            return current  # Another process already recorded the outcome.
        state = current
        item = state['items'][index]
        try:
            if receipt.get('intent_id') != intent or receipt.get('invocation_id') != invocation:
                raise ValueError('Mismatched receipt')
            if receipt.get('state') == 'pending':
                return state
            if receipt.get('state') != 'settled' or receipt.get('quiescent') is not True:
                raise ValueError('Unconfirmed settlement')
            result = receipt['result']
            saved = [previous['child_result'] for previous in state['items'][:index]]
            published = [prior for prior in saved if prior and prior['published']]
            contracts().check_child(result, entry, {p['signal_id'] for p in published}, {p['output_file_id'] for p in published})
        except (ValueError, KeyError, TypeError, ValidationError):
            halt(state, 'child_result_invalid_or_unsettled', True)
        else:
            item['child_result'] = result
            item['status'], recovery = contracts().disposition(result)
            if receipt.get('recovery_clear') is not True:
                halt(state, 'recovery_hold_active_or_unverified', True)
            elif item['status'] == 'halted':
                halt(state, 'child_requires_reconciliation' if recovery else 'child_infrastructure_failed', recovery)
            else:
                state.update(index=index + 1, intent_id=None,
                             phase='complete' if index + 1 == len(state['items']) else 'ready')
        journal.save(state)
        return state


def progress(state):
    phase = state['phase']
    response = {'loop_id': state['loop_id'], 'job_id': state['job_id'], 'phase': phase,
                'settled_products': state['index'], 'total': len(state['items']),
                'recovery_required': phase in ('hold', 'submitting')}
    if phase in ('submitting', 'hold'):
        response['intent_id'] = state['intent_id']
    if phase in ('waiting', 'hold'):
        response['invocation_id'] = state['items'][state['index']]['invocation_id']
    return response


def result(state):
    if state['phase'] not in ('complete', 'hold', 'stopped'):
        raise ValueError('The loop has no terminal batch result')
    items = state['items']
    status = 'stopped'
    if state['phase'] == 'complete':
        status = ('empty' if not items else 'completed_with_errors'
                  if any(i['status'] in ('partial', 'failed') for i in items) else 'completed')
    return contracts().finish({'loop_id': state['loop_id'], 'job_id': state['job_id'], 'status': status,
                   'source': state['source'], 'counts': {'dispatched': state['dispatched']},
                   'items': items, 'recovery_required': state['phase'] == 'hold', 'errors': []})


def run_window(journal, binding, loop_id, time_budget, poll_interval):
    """Continue until the local budget expires without cancelling a remote child."""
    deadline = time.monotonic() + time_budget
    state = journal.read(loop_id)
    while state['phase'] in ('ready', 'waiting'):
        needed = binding.timeout * (2 if state['phase'] == 'ready' else 1)
        if deadline - time.monotonic() < needed:
            break
        was_waiting = state['phase'] == 'waiting'
        state = advance(journal, binding, loop_id)
        if was_waiting and state['phase'] == 'waiting':
            time.sleep(min(poll_interval, max(0, deadline - time.monotonic())))
    return state


def bind_contracts(support, resources):
    sys.path.insert(0, str(support.resolve(strict=True)))
    from validate import contract_resources
    os.environ['AGENT_SCHEMA_RESOURCES'] = json.dumps(
        {key: str(path) for key, path in contract_resources(resources.resolve(strict=True)).items()})


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--journal', type=Path, required=True)
    kit = Path(__file__).resolve().parent.parent
    parser.add_argument('--support', type=Path, default=kit / 'offline-validation-support')
    parser.add_argument('--contracts', type=Path, default=kit / 'contract-resources')
    parser.add_argument('--binding', type=Path)
    parser.add_argument('--timeout', type=int, default=30)
    modes = parser.add_subparsers(dest='action', required=True)
    begin = modes.add_parser('start')
    begin.add_argument('--config', type=Path, required=True)
    begin.add_argument('--job-id', required=True)
    window = modes.add_parser('run')
    window.add_argument('--loop-id', required=True)
    window.add_argument('--time-budget', type=int, default=300)
    window.add_argument('--poll-interval', type=int, default=15)
    for mode in ('advance', 'status', 'result'):
        modes.add_parser(mode).add_argument('--loop-id', required=True)
    args = parser.parse_args()
    if args.timeout < 1 or (args.action in ('start', 'advance', 'run') and args.binding is None):
        parser.error('Start, advance and run require a trusted --binding and a positive timeout')
    if args.action == 'run' and (args.time_budget < 1 or args.poll_interval < 1):
        parser.error('Time budget and poll interval must be positive')
    try:
        bind_contracts(args.support, args.contracts)
        journal = Journal(args.journal)
        if args.action == 'start':
            state = start(journal, DriveReader(os.environ.get('GOOGLE_DRIVE_ACCESS_TOKEN'), args.timeout),
                          ResearcherBinding(args.binding, args.timeout),
                          contracts().strict_loads(args.config.read_bytes()), args.job_id)
        elif args.action == 'advance':
            state = advance(journal, ResearcherBinding(args.binding, args.timeout), args.loop_id)
        elif args.action == 'run':
            state = run_window(journal, ResearcherBinding(args.binding, args.timeout), args.loop_id,
                               args.time_budget, args.poll_interval)
        else:
            state = journal.read(args.loop_id)
        print(dump(result(state) if args.action == 'result' else progress(state)))
        return 0
    except Exception as exc:
        # Avoid printing provider error bodies, credentials or child payloads.
        print(f'Looper could not {args.action} ({type(exc).__name__}).', file=sys.stderr)
        return 1


if __name__ == '__main__':
    raise SystemExit(main())
