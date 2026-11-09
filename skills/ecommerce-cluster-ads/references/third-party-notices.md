# Third-party notices

## Offline public-suffix lookup

`public-suffix-rules.json` is a language-neutral derivative of Public Suffix List
data under the Mozilla Public License 2.0. Preserve its metadata and this notice.
The full license is included in `MPL-2.0.txt`. This license notice applies to the
derived suffix data, not a newly asserted license for the user's workflow.

The build used the pre-existing system snapshot bundled in
`IO::Socket::SSL::PublicSuffix`. No library source code is distributed and that
library is not a runtime dependency. The upstream revision and snapshot date were
not available. This is not claimed to be the latest suffix list. The data's SHA-256
and exact transformation are recorded in the JSON metadata.

The derivative removes comments, groups rules by final hostname label, retains
ICANN and private rules, and adds ASCII aliases for Unicode rules where applicable.
Use it only for offline parsing, not to infer any business or product facts.

Reference locations, for maintenance only (do not browse them during normalization):

```text
https://publicsuffix.org/list/public_suffix_list.dat
https://github.com/publicsuffix/list/wiki/Format
https://www.mozilla.org/MPL/2.0/
```

## Package and schema formats

Use JSON Schema Draft 2020-12 for the declarative contracts. Use the root manifest
and `skills/` layout for the optional plugin wrapper. The corresponding technical
references were checked during packaging, not used to enrich any input ad:

```text
https://json-schema.org/draft/2020-12
https://developers.openai.com/plugins/build/plugins
```
