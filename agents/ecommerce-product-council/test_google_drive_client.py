"""Drive v3 request/response tests; no credentials, network or SDK required."""
import json
import sys
from types import ModuleType
import unittest
from unittest.mock import MagicMock, patch

from google_drive_client import GoogleDriveClient, DriveReadError


class GoogleDriveClientTests(unittest.TestCase):
    def setUp(self):
        self.service = MagicMock()
        self.files = self.service.files.return_value
        self.client = GoogleDriveClient(self.service)

    def test_pagination_and_shared_drive_options(self):
        self.files.list.return_value.execute.side_effect = [
            {"files": [], "nextPageToken": "page-2"},
            {"files": [{"id": "f", "properties": {}}], "incompleteSearch": False}]
        client = GoogleDriveClient(self.service, shared_drive_id="shared")
        self.assertEqual(client.list_files("folder", {"store": "niobium-council"})[0]["id"], "f")
        first, second = [call.kwargs for call in self.files.list.call_args_list]
        self.assertEqual(first["corpora"], "drive")
        self.assertEqual(first["driveId"], "shared")
        self.assertTrue(first["supportsAllDrives"])
        self.assertTrue(first["includeItemsFromAllDrives"])
        self.assertIn("incompleteSearch", first["fields"])
        self.assertNotIn("pageToken", first)
        self.assertEqual(second["pageToken"], "page-2")

    def test_query_escaping_and_my_drive(self):
        self.files.list.return_value.execute.return_value = {"files": []}
        self.client.list_files("folder'\\id", {"key'": "value\\'"})
        options = self.files.list.call_args.kwargs
        self.assertIn("'folder\\'\\\\id' in parents", options["q"])
        self.assertIn("key='key\\''", options["q"])
        self.assertIn("value='value\\\\\\''", options["q"])
        self.assertIn("trashed = false", options["q"])
        self.assertNotIn("mimeType", options["q"])
        self.assertEqual(options["corpora"], "user")
        self.assertNotIn("driveId", options)

    def test_incomplete_or_malformed_search_fails_closed(self):
        for result in ({"files": [], "incompleteSearch": True}, {}, {"files": None},
                       {"files": [{"name": "no-id"}]}, None):
            with self.subTest(result=result):
                self.files.list.return_value.execute.return_value = result
                with self.assertRaises(DriveReadError):
                    self.client.list_files("folder", {})

    def test_repeated_page_token_fails_closed(self):
        self.files.list.return_value.execute.return_value = {"files": [], "nextPageToken": "same"}
        with self.assertRaises(DriveReadError):
            self.client.list_files("folder", {})
        self.assertEqual(self.files.list.call_count, 2)

    def test_api_error_is_not_an_empty_listing(self):
        self.files.list.return_value.execute.side_effect = PermissionError("denied")
        with self.assertRaises(PermissionError):
            self.client.list_files("folder", {})

    def test_read_json_uses_media_and_shared_drive_support(self):
        self.files.get_media.return_value.execute.return_value = b'{"actor":"human"}'
        self.assertEqual(self.client.read_json("file"), {"actor": "human"})
        self.files.get_media.assert_called_once_with(fileId="file", supportsAllDrives=True)

    def test_malformed_duplicate_key_and_nonfinite_json_rejected(self):
        for raw in (b"not-json", b"[]", b'{"a":1,"a":2}', b'{"a":NaN}', b'{"a":Infinity}'):
            with self.subTest(raw=raw):
                self.files.get_media.return_value.execute.return_value = raw
                with self.assertRaises(ValueError):
                    self.client.read_json("file")

    def test_create_upload_is_json_and_create_only(self):
        http = ModuleType("googleapiclient.http")
        http.MediaIoBaseUpload = MagicMock()
        self.files.create.return_value.execute.return_value = {"id": "new-file"}
        with patch.dict(sys.modules, {"googleapiclient": ModuleType("googleapiclient"),
                                      "googleapiclient.http": http}):
            self.assertEqual(self.client.create_json("folder", "review.json", {"store": "test"},
                                                     {"value": "test"}), "new-file")
        options = self.files.create.call_args.kwargs
        self.assertEqual(options["body"]["parents"], ["folder"])
        self.assertEqual(options["body"]["properties"], {"store": "test"})
        self.assertTrue(options["supportsAllDrives"])
        media_call = http.MediaIoBaseUpload.call_args
        self.assertEqual(json.loads(media_call.args[0].getvalue()), {"value": "test"})
        self.assertEqual(media_call.kwargs, {"mimetype": "application/json", "resumable": False})
        self.files.update.assert_not_called()
        self.files.delete.assert_not_called()

    def test_create_without_id_reports_uncertain_outcome(self):
        http = ModuleType("googleapiclient.http")
        http.MediaIoBaseUpload = MagicMock()
        self.files.create.return_value.execute.return_value = {}
        with patch.dict(sys.modules, {"googleapiclient": ModuleType("googleapiclient"),
                                      "googleapiclient.http": http}):
            with self.assertRaises(RuntimeError):
                self.client.create_json("folder", "name", {}, {})


if __name__ == "__main__":
    unittest.main()
