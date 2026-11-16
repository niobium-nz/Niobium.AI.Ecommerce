"""Pure Drive policy examples for offline tests; no connector or production runner.

Adapters map their actual response fields to these Drive-v3 names. Missing optional
flags remain unknown; mandatory identity, complete listings and bytes do not.
"""
from __future__ import annotations


def trash_unknown(metadata):
    value = metadata.get("trashed")
    if value is True:
        raise ValueError("Explicitly trashed")
    return ["trashed"] if type(value) is not bool else []


def check_folder(metadata, folder_id):
    if (not folder_id or folder_id == "root" or metadata.get("id") != folder_id
            or metadata.get("name") != "discovery"
            or metadata.get("mimeType") != "application/vnd.google-apps.folder"):
        raise ValueError("Folder identity mismatch")
    unknown = trash_unknown(metadata)
    capabilities = metadata.get("capabilities")
    can_add = capabilities.get("canAddChildren") if isinstance(capabilities, dict) else None
    if can_add is False:
        raise ValueError("Explicit add-child denial")
    if type(can_add) is not bool:
        unknown.append("capabilities.canAddChildren")
    return unknown


def check_saved_file(metadata, file_id, folder_id, filename, expected_bytes,
                     actual_bytes, exact_matches, listing_complete):
    if (not file_id or metadata.get("id") != file_id or metadata.get("name") != filename
            or metadata.get("mimeType") != "application/json"
            or metadata.get("parents") != [folder_id]):
        raise ValueError("Artifact identity mismatch")
    unknown = trash_unknown(metadata)
    if (not isinstance(expected_bytes, bytes) or not isinstance(actual_bytes, bytes)
            or actual_bytes != expected_bytes):
        raise ValueError("Unverified complete bytes")
    if listing_complete is not True or exact_matches != [file_id]:
        raise ValueError("Unverified unique name")
    return unknown
