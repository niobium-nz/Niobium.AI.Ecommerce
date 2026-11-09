#!/usr/bin/env python3
"""Optionally validate Ecommerce Keyword Expander JSON using the standard library.

This checks the wire contract and deterministic text hygiene. It does not verify
that research occurred or assess relevance, semantic conflicts, safety, or demand.
"""
from __future__ import annotations

import argparse
import functools
import json
from pathlib import Path
import re
import sys
from typing import Any
import unicodedata

ROOT = Path(__file__).resolve().parents[1]
INPUT_KEYS = frozenset({'category_focus', 'country', 'seed_keywords', 'optional_constraints'})
CONSTRAINT_KEYS = frozenset({'target_audience', 'price_tier', 'use_case', 'gender', 'seasonality', 'platform'})
SUCCESS_KEYS = frozenset({'category_focus', 'optimized_keywords'})
ERROR_KEYS = SUCCESS_KEYS | {'error'}
URL_PATTERN = re.compile(r'(?:https?://|www\.)\S+', re.IGNORECASE)
HASHTAG_PATTERN = re.compile(r'(?:^|\s)#[\w]+')
CITATION_PATTERN = re.compile(r'[\ue200\ue201\ue202]|\[\d+\]|\[[^\]]+\]\(')
DASH_PATTERN = re.compile(r'[-\u2010-\u2015\u2212]')


class JsonContractError(ValueError):
    """A JSON parsing error that prevents safe contract validation."""


def _unique_object(pairs: list[tuple[str, Any]]) -> dict[str, Any]:
    result: dict[str, Any] = {}
    for key, value in pairs:
        if key in result:
            raise JsonContractError(f'Duplicate JSON object key: {key!r}.')
        result[key] = value
    return result


def _reject_constant(value: str) -> Any:
    raise JsonContractError(f'{value} is not a valid JSON number.')


def parse_json(text: str) -> Any:
    """Parse one strict JSON value, rejecting duplicate keys and NaN/Infinity."""
    try:
        return json.loads(text, object_pairs_hook=_unique_object, parse_constant=_reject_constant)
    except json.JSONDecodeError as exc:
        raise JsonContractError(
            f'Invalid JSON at line {exc.lineno}, column {exc.colno}: {exc.msg}.'
        ) from exc


@functools.lru_cache(maxsize=1)
def country_index() -> dict[str, dict[str, str]]:
    """Load the bundled, versioned lookup without making network requests."""
    snapshot = json.loads((ROOT / 'references' / 'iso3166-1.json').read_text(encoding='utf-8'))
    countries = snapshot['countries']
    result: dict[str, dict[str, str]] = {}
    for record in countries:
        for field, length in [('alpha_2', 2), ('alpha_3', 3)]:
            code = record[field]
            if not isinstance(code, str) or not re.fullmatch(r'[A-Z]{' + str(length) + '}', code):
                raise ValueError('The bundled country-code data is malformed.')
            if code in result:
                raise ValueError('The bundled country-code data contains duplicate codes.')
            result[code] = record
    if not result:
        raise ValueError('The bundled country-code data is empty.')
    return result


def resolve_country(value: Any) -> dict[str, str] | None:
    if not isinstance(value, str) or not re.fullmatch(r'[A-Za-z]{2,3}', value):
        return None
    return country_index().get(value.upper())


def _nonblank(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def validate_input(value: Any) -> list[str]:
    """Return deterministic input issues; do not infer semantic intent."""
    if not isinstance(value, dict):
        return ['Input must be a single JSON object.']
    issues: list[str] = []
    for key in sorted(value.keys() - INPUT_KEYS):
        issues.append(f'Unsupported input property: {key}. Use the exact lower_snake_case contract.')
    for key in ['category_focus', 'country']:
        if key not in value:
            issues.append(f'Missing required property: {key}.')
        elif not _nonblank(value[key]):
            issues.append(f'{key} must be a nonblank string.')
    if _nonblank(value.get('country')) and resolve_country(value['country']) is None:
        issues.append(
            'country must be an assigned ISO 3166-1 alpha-2 or alpha-3 letter code in the bundled snapshot; '
            'numeric codes, country names, aliases, and subdivision codes are not accepted. '
            'Verify a suspected new ISO assignment before updating the snapshot.'
        )
    if 'seed_keywords' in value:
        seeds = value['seed_keywords']
        if not isinstance(seeds, list):
            issues.append('seed_keywords must be an array of strings; null is not omission.')
        else:
            for i, seed in enumerate(seeds):
                if not isinstance(seed, str):
                    issues.append(f'seed_keywords[{i}] must be a string.')
    if 'optional_constraints' in value:
        constraints = value['optional_constraints']
        if not isinstance(constraints, list):
            issues.append('optional_constraints must be an array of objects; null is not omission.')
        else:
            for i, segment in enumerate(constraints):
                path = f'optional_constraints[{i}]'
                if not isinstance(segment, dict):
                    issues.append(f'{path} must be an object.')
                    continue
                for key in sorted(segment.keys() - CONSTRAINT_KEYS):
                    issues.append(f'{path}.{key} is not a supported constraint property.')
                for key in sorted(segment.keys() & CONSTRAINT_KEYS):
                    if segment[key] is not None and not isinstance(segment[key], str):
                        issues.append(f'{path}.{key} must be a string or null.')
    return issues


def phrase_key(value: str) -> str:
    """Normalize easy case, dash, and whitespace variants; do not stem words."""
    normalized = unicodedata.normalize('NFKC', value).casefold()
    return ' '.join(DASH_PATTERN.sub(' ', normalized).split())


def _clean_phrase(value: Any, path: str) -> list[str]:
    if not _nonblank(value):
        return [f'{path} must be a nonblank string.']
    issues: list[str] = []
    if value != ' '.join(value.split()):
        issues.append(f'{path} must be trimmed and use single spaces, without line breaks or tabs.')
    if URL_PATTERN.search(value):
        issues.append(f'{path} must not contain a URL.')
    if HASHTAG_PATTERN.search(value):
        issues.append(f'{path} must not contain hashtags.')
    if CITATION_PATTERN.search(value):
        issues.append(f'{path} must not contain citation tokens or Markdown links.')
    return issues


def validate_output(value: Any) -> list[str]:
    """Validate success/error shape and basic hygiene, not research quality."""
    if not isinstance(value, dict):
        return ['Output must be a single JSON object.']
    issues: list[str] = []
    is_error = 'error' in value
    expected = ERROR_KEYS if is_error else SUCCESS_KEYS
    for key in sorted(expected - value.keys()):
        issues.append(f'Missing output property: {key}.')
    for key in sorted(value.keys() - expected):
        issues.append(f'Unsupported output property: {key}.')
    if is_error:
        if not _nonblank(value['error']):
            issues.append('error must be a nonblank string; omit it entirely on success.')
        elif URL_PATTERN.search(value['error']):
            issues.append('error must not contain a URL.')
        elif CITATION_PATTERN.search(value['error']):
            issues.append('error must not contain citation tokens or Markdown links.')
        if value.get('category_focus') != '':
            issues.append('On error, category_focus must be an empty string.')
        if not isinstance(value.get('optimized_keywords'), list) or value['optimized_keywords']:
            issues.append('On error, optimized_keywords must be an empty array.')
        return issues
    issues.extend(_clean_phrase(value.get('category_focus'), 'category_focus'))
    keywords = value.get('optimized_keywords')
    if not isinstance(keywords, list):
        issues.append('optimized_keywords must be an array of strings.')
        return issues
    if not 25 <= len(keywords) <= 60:
        issues.append('optimized_keywords must contain 25 to 60 entries in total.')
    seen: dict[str, int] = {}
    for i, keyword in enumerate(keywords):
        path = f'optimized_keywords[{i}]'
        issues.extend(_clean_phrase(keyword, path))
        if not isinstance(keyword, str):
            continue
        canonical = phrase_key(keyword)
        if not canonical:
            issues.append(f'{path} must not be an empty punctuation variant.')
        elif canonical in seen:
            issues.append(f'{path} duplicates optimized_keywords[{seen[canonical]}] after basic normalization.')
        else:
            seen[canonical] = i
    return issues


def error_response(message: str) -> dict[str, Any]:
    """Construct a contract-compliant error without partial results."""
    response = {'category_focus': '', 'optimized_keywords': [], 'error': message}
    issues = validate_output(response)
    if issues:
        raise ValueError('; '.join(issues))
    return response


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('kind', choices=['input', 'output'])
    parser.add_argument('path', help='UTF-8 JSON file, or - to read from standard input')
    args = parser.parse_args(argv)
    try:
        raw = sys.stdin.read() if args.path == '-' else Path(args.path).read_text(encoding='utf-8-sig')
        value = parse_json(raw)
        issues = validate_input(value) if args.kind == 'input' else validate_output(value)
        report: dict[str, Any] = {'valid': not issues, 'kind': args.kind, 'issues': issues}
        if not issues and args.kind == 'input':
            report['resolved_country'] = resolve_country(value['country'])
        print(json.dumps(report, ensure_ascii=True, allow_nan=False))
        return 1 if issues else 0
    except JsonContractError as exc:
        print(json.dumps({'valid': False, 'kind': args.kind, 'issues': [str(exc)]}, ensure_ascii=True))
        return 1
    except (OSError, UnicodeError, KeyError, TypeError, ValueError) as exc:
        print(json.dumps({
            'valid': False, 'kind': args.kind,
            'issues': [f'Validator resource or file error: {exc}']
        }, ensure_ascii=True))
        return 2


if __name__ == '__main__':
    raise SystemExit(main())
