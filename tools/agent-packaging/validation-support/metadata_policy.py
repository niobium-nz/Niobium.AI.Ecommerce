"""Offline reference for optional provider metadata, not a connector client."""
import warnings


def optional_metadata(metadata, require_add=False):
    unknown = []
    trash = metadata.get('trashed')
    if trash is True:
        raise ValueError('Explicitly trashed resource')
    if type(trash) is not bool:
        unknown.append('trashed')
    if require_add:
        capabilities = metadata.get('capabilities')
        can_add = capabilities.get('canAddChildren') if isinstance(capabilities, dict) else None
        if can_add is False:
            raise ValueError('Explicit add-child permission denial')
        if type(can_add) is not bool:
            unknown.append('capabilities.canAddChildren')
    if unknown:
        warnings.warn('drive_metadata_unavailable: ' + ', '.join(unknown), RuntimeWarning, stacklevel=2)
    return unknown


def lookup_completeness(complete):
    """Normalized evidence: True = established, None = unreported, False = incomplete.

    Call only after a successful, correctly scoped exact-name search and after
    following every returned continuation token. Errors, malformed results and
    known truncation must never be normalized to None.
    """
    if complete is True:
        return []
    if complete is not None:
        raise ValueError('Known incomplete lookup or invalid completeness evidence')
    code = 'drive_lookup_completeness_unavailable'
    warnings.warn(code + ': using observed exact-name matches; global uniqueness is unverified',
                  RuntimeWarning, stacklevel=2)
    return [code]
