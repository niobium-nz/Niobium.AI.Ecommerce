"""Unknown provider metadata must not hide real denials or failed persistence."""
import copy
import unittest
from drive_checks import check_collision, check_folder, check_saved_file
from contract_checks import ROOT, load, validate_result


class DrivePolicyTests(unittest.TestCase):
    def setUp(self):
        self.folder = {"id": "fixture_folder", "name": "discovery",
                       "mimeType": "application/vnd.google-apps.folder"}
        self.file = {"id": "fixture_file", "name": "job.json", "mimeType": "application/json",
                     "parents": ["fixture_folder"]}

    def test_unknown_flags_warn_without_claiming_permission(self):
        for flags in ({}, {"trashed": None, "capabilities": {"canAddChildren": None}},
                      {"trashed": "false", "capabilities": {"canAddChildren": "true"}},
                      {"capabilities": {"canShare": True, "canEdit": True}}):
            with self.subTest(flags=flags):
                self.assertEqual(check_folder(dict(self.folder, **flags), "fixture_folder"),
                                 ["trashed", "capabilities.canAddChildren"])

    def test_explicit_denial_overrides_other_unknown_fields(self):
        for flags in ({"trashed": True}, {"capabilities": {"canAddChildren": False}}):
            with self.subTest(flags=flags), self.assertRaises(ValueError):
                check_folder(dict(self.folder, **flags), "fixture_folder")

    def test_positive_flags_have_no_warning(self):
        self.assertEqual(check_folder(dict(self.folder, trashed=False,
                         capabilities={"canAddChildren": True}), "fixture_folder"), [])

    def test_identity_remains_required(self):
        for field, value in (("id", "other"), ("name", "other"), ("mimeType", "application/vnd.google-apps.shortcut")):
            with self.subTest(field=field), self.assertRaises(ValueError):
                check_folder(dict(self.folder, **{field: value}), "fixture_folder")
        with self.assertRaises(ValueError):
            check_folder({}, "fixture_folder")

    def test_observed_empty_connector_response_does_not_need_completion_flag(self):
        # Actual all-type connector response shape, without private folder/job IDs.
        reply = {"results": []}
        with self.assertWarnsRegex(RuntimeWarning, 'drive_lookup_completeness_unavailable'):
            action, warnings = check_collision(reply['results'], reply.get('complete'))
        self.assertEqual(action, 'continue')
        self.assertEqual(warnings, ['drive_lookup_completeness_unavailable'])
        self.assertEqual(check_collision([], True), ('continue', []))

    def test_observed_collisions_and_known_incomplete_lookups_still_stop(self):
        for state in (True, None, False):
            self.assertEqual(check_collision(['existing-file'], state), ('stopped', []))
        for matches, state in (([], False), (None, None), ([None], None), ([], 'true')):
            with self.assertRaises(ValueError): check_collision(matches, state)

    def verify(self, metadata=None, raw=b"[]", matches=None, complete=True):
        return check_saved_file(self.file if metadata is None else metadata,
                                "fixture_file", "fixture_folder", "job.json", b"[]", raw,
                                ["fixture_file"] if matches is None else matches, complete)

    def test_missing_trash_flag_does_not_mask_verified_bytes(self):
        self.assertEqual(self.verify(), ["trashed"])
        self.assertEqual(self.verify(metadata=dict(self.file, trashed=False)), [])
        with self.assertRaises(ValueError):
            self.verify(metadata=dict(self.file, trashed=True))

    def test_bytes_parent_and_known_lookup_failures_remain_mandatory(self):
        for change in ({"raw": None}, {"raw": b"["}, {"raw": "[]"},
                       {"matches": ["other"]}, {"matches": ["fixture_file", "other"]},
                       {"complete": False}, {"metadata": dict(self.file, parents=["other"])}):
            with self.subTest(change=change), self.assertRaises(ValueError):
                self.verify(**change)

    def test_verified_new_file_uses_id_and_bytes_when_search_evidence_is_limited(self):
        with self.assertWarnsRegex(RuntimeWarning, 'drive_lookup_completeness_unavailable'):
            self.assertEqual(self.verify(complete=None),
                             ['trashed', 'drive_lookup_completeness_unavailable'])
        with self.assertWarnsRegex(RuntimeWarning, 'drive_lookup_completeness_unavailable'):
            self.assertEqual(self.verify(matches=[], complete=None),
                             ['trashed', 'drive_lookup_completeness_unavailable', 'drive_created_file_not_listed'])
        with self.assertRaises(ValueError): self.verify(matches=[], complete=None, raw=b'wrong')

    def test_denied_upload_cannot_report_artifact(self):
        result = load(ROOT / "examples/result.json")
        result.update(status="failed", errors=[{"code": "upload_failed", "message": "Permission denied", "details": {}}])
        from jsonschema import ValidationError
        with self.assertRaises(ValidationError):
            validate_result(result)
        result["artifact"] = None
        result["warnings"] = [{"code": "drive_metadata_unavailable", "message": "Add permission unknown",
                               "details": {"fields": ["capabilities.canAddChildren"], "stage": "preflight"}}]
        validate_result(result)

    def test_unknown_metadata_warning_is_not_integrity_loss(self):
        result = copy.deepcopy(load(ROOT / "examples/result.json"))
        result["warnings"].append({"code": "drive_metadata_unavailable", "message": "Trash flag unknown",
                                   "details": {"fields": ["trashed"], "stage": "verification"}})
        validate_result(result)
        self.assertEqual(result["status"], "succeeded")
