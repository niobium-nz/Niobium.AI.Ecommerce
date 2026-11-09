# Country-code lookup and provenance

Use `iso3166-1.json` to resolve valid alpha-2 and alpha-3 codes. Do not use a two-or-three-letter regex alone as evidence that a code is assigned.

The bundled snapshot contains 249 records extracted from the locally installed `pycountry` 24.6.1 database on 2026-11-01. The extraction date is not a claim that the package was updated on that date. This is a pinned validation snapshot, not a freshly downloaded ISO register. Only the code pairs and country lookup labels are included; no pycountry runtime dependency is required.

Use code matching without case sensitivity. Do not accept alpha-4 historic codes, numeric-3 codes, subdivision codes, reserved aliases, or user-assigned codes as assigned country identifiers. Keep country names as lookup labels; do not infer language, market demand, political status, or spelling rules from the label alone.

For registry verification and maintenance, consult the ISO country-code service. The United Nations M49 overview publishes ISO alpha-2/alpha-3 columns as a useful independent official lookup, but its coverage and statistical purpose are not identical to all ISO assignments. Do not replace the bundled code list by blindly taking a partial statistical table.

Reference locations:

```text
https://www.iso.org/iso-3166-country-codes.html
https://www.iso.org/obp/ui/
https://unstats.un.org/unsd/methodology/m49/overview/
```

If a plausible code is absent, verify whether it is unassigned, reserved, deleted, or newly assigned. Do not guess a substitute. For a verified assignment missing from the snapshot, explain that the bundled validator needs updating rather than reporting the country as nonexistent. Refresh the JSON data, the input schema's case-insensitive membership pattern, and the validator tests together. Keep runtime responses within the existing `error` contract until the update is approved and applied.
