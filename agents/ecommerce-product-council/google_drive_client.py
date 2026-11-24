"""Optional Drive v3 adapter. The host owns credentials, scopes and SDK setup."""
from __future__ import annotations

import io
import json
from typing import Any


class DriveReadError(RuntimeError):
    """A complete authoritative Drive listing could not be obtained."""


def _literal(value: str) -> str:
    return "'" + value.replace("\\", "\\\\").replace("'", "\\'") + "'"


def _object(pairs: list[tuple[str, Any]]) -> dict[str, Any]:
    result: dict[str, Any] = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate JSON key: {key}")
        result[key] = value
    return result


def _invalid_constant(value: str) -> None:
    raise ValueError(f"invalid JSON constant: {value}")


class GoogleDriveClient:
    """Wrap an already-authenticated googleapiclient Drive v3 service.

    No credentials, scopes, user authentication, updates or deletes are handled
    here. Public custom properties let different authorized OAuth clients share
    the index. 'Public' here does not change the file's sharing permissions.
    """

    def __init__(self, service: Any, *, shared_drive_id: str | None = None):
        if shared_drive_id is not None and (not isinstance(shared_drive_id, str) or not shared_drive_id.strip()):
            raise ValueError("shared_drive_id must be nonempty")
        self.service = service
        self.shared_drive_id = shared_drive_id

    def list_files(self, folder_id: str, properties: dict[str, str]) -> list[dict[str, Any]]:
        query = [_literal(folder_id) + " in parents", "trashed = false"]
        # Do not filter by MIME type/schema/kind: a changed file must be checked,
        # not silently disappear from the authorization history.
        query.extend("properties has { key=" + _literal(key) + " and value=" + _literal(value) + " }"
                     for key, value in sorted(properties.items()))
        options = {"q": " and ".join(query), "spaces": "drive", "pageSize": 1000,
                   "fields": "nextPageToken,incompleteSearch,files(id,properties)",
                   "supportsAllDrives": True, "includeItemsFromAllDrives": True,
                   "corpora": "drive" if self.shared_drive_id else "user"}
        if self.shared_drive_id:
            options["driveId"] = self.shared_drive_id
        files = []
        seen_tokens: set[str] = set()
        while True:
            response = self.service.files().list(**options).execute()
            if not isinstance(response, dict) or response.get("incompleteSearch"):
                raise DriveReadError("incomplete Drive search; authorization denied")
            page = response.get("files")
            if not isinstance(page, list) or any(not isinstance(f, dict) or not f.get("id") for f in page):
                raise DriveReadError("malformed Drive listing")
            files.extend(page)
            token = response.get("nextPageToken")
            if not token:
                return files
            if not isinstance(token, str) or token in seen_tokens:
                raise DriveReadError("invalid or repeated Drive page token")
            seen_tokens.add(token)
            options["pageToken"] = token

    def read_json(self, file_id: str) -> dict[str, Any]:
        raw = self.service.files().get_media(fileId=file_id, supportsAllDrives=True).execute()
        if not isinstance(raw, (str, bytes, bytearray)):
            raise DriveReadError("Drive file did not contain JSON bytes")
        body = json.loads(raw, object_pairs_hook=_object, parse_constant=_invalid_constant)
        if not isinstance(body, dict):
            raise ValueError("Drive JSON root must be an object")
        return body

    def create_json(self, folder_id: str, name: str, properties: dict[str, str],
                    body: dict[str, Any]) -> str:
        # Lazy import keeps the domain and offline suite free of Google SDKs.
        from googleapiclient.http import MediaIoBaseUpload

        payload = json.dumps(body, sort_keys=True, separators=(",", ":"),
                             ensure_ascii=False, allow_nan=False).encode("utf-8")
        media = MediaIoBaseUpload(io.BytesIO(payload), mimetype="application/json", resumable=False)
        result = self.service.files().create(
            body={"name": name, "parents": [folder_id], "mimeType": "application/json", "properties": properties},
            media_body=media, fields="id", supportsAllDrives=True).execute()
        if not isinstance(result, dict) or not isinstance(result.get("id"), str) or not result["id"]:
            raise RuntimeError("Drive create returned no file ID; outcome is uncertain")
        return result["id"]
