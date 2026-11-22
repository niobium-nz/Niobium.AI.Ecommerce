"""Checkpoint regressions using a real SQLite journal and synthetic host I/O."""
from __future__ import annotations

import hashlib
import subprocess
import tempfile
import unittest
from decimal import Decimal
from pathlib import Path

from test_looper import CONFIG, JOB, SOURCE, child, entries
from protocol_reference import child_payload, strict_loads
from run_looper import Journal, advance, dump, progress, result, start


class DriveFixture:
    def __init__(self, rows):
        self.rows = rows

    def snapshot(self, folder_id, job_id):
        raw = dump(self.rows).encode()
        return self.rows, dict(SOURCE, sha256=hashlib.sha256(raw).hexdigest()), raw


class BindingFixture:
    def __init__(self, rows):
        self.rows = rows
        self.payloads = []
        self.ready = False
        self.submit_error = False
        self.poll_error = False
        self.invalid_result = False

    def scope(self, config):
        return 'shared-registry'

    def call(self, action, **request):
        index = request['source_index']
        response = {'intent_id': request['intent_id'], 'invocation_id': f'run-{index}'}
        if action == 'submit':
            self.payloads.append(request['input'])
            if self.submit_error:
                raise subprocess.TimeoutExpired('binding', 30)
        elif self.poll_error:
            raise subprocess.TimeoutExpired('binding', 30)
        elif not self.ready:
            response['state'] = 'pending'
        else:
            response.update(state='settled', quiescent=True, recovery_clear=True,
                            result={} if self.invalid_result else child(self.rows[index]))
        return response


class CheckpointRunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name) / 'loop.sqlite'
        self.journal = self.open_journal()
        self.rows = entries()
        self.rows[1]['ads'][0]['provider_field'] = Decimal('0.12345678901234567890123456789')
        self.binding = BindingFixture(self.rows)
        self.drive = DriveFixture(self.rows)
        self.loop_id = start(self.journal, self.drive, self.binding, CONFIG, JOB)['loop_id']

    def open_journal(self):
        journal = Journal(self.path)
        self.addCleanup(journal.db.close)
        return journal

    def test_restart_polls_one_saved_invocation_before_dispatching_next(self):
        state = advance(self.journal, self.binding, self.loop_id)
        self.assertEqual(progress(state)['invocation_id'], 'run-0')
        reopened = self.open_journal()
        state = advance(reopened, self.binding, self.loop_id)
        self.assertEqual(progress(state)['invocation_id'], 'run-0')
        self.assertEqual(len(self.binding.payloads), 1)
        self.binding.ready = True
        state = advance(reopened, self.binding, self.loop_id)
        self.assertEqual(progress(state)['settled_products'], 1)
        advance(reopened, self.binding, self.loop_id)
        completed = result(advance(reopened, self.binding, self.loop_id))
        self.assertEqual(completed['status'], 'completed')
        self.assertEqual(completed['counts']['succeeded'], 2)
        self.assertEqual(self.binding.payloads, [child_payload(entry) for entry in self.rows])
        self.assertIsInstance(strict_loads(dump(self.binding.payloads[1]))['ads'][0]['provider_field'], Decimal)

    def test_shared_journal_rejects_another_active_batch_and_unknown_submit(self):
        with self.assertRaisesRegex(ValueError, 'active or held'):
            start(self.open_journal(), self.drive, self.binding, CONFIG, JOB)
        self.binding.submit_error = True
        state = advance(self.journal, self.binding, self.loop_id)
        self.assertTrue(progress(state)['recovery_required'])
        reopened = self.open_journal()
        with self.assertRaisesRegex(ValueError, 'active or held'):
            start(reopened, self.drive, self.binding, CONFIG, JOB)
        stopped = result(advance(reopened, self.binding, self.loop_id))
        self.assertEqual(stopped['counts']['dispatched'], 1)
        self.assertEqual(stopped['counts']['not_run'], 1)
        self.assertEqual(len(self.binding.payloads), 1)

    def test_timeout_polling_known_run_and_bad_result_keep_next_product_safe(self):
        advance(self.journal, self.binding, self.loop_id)
        self.binding.poll_error = True
        state = advance(self.journal, self.binding, self.loop_id)
        self.assertEqual(progress(state)['invocation_id'], 'run-0')
        self.binding.poll_error = False
        self.binding.ready = self.binding.invalid_result = True
        stopped = result(advance(self.journal, self.binding, self.loop_id))
        self.assertEqual(stopped['counts']['not_run'], 1)
        self.assertTrue(stopped['recovery_required'])
        self.assertEqual(len(self.binding.payloads), 1)
