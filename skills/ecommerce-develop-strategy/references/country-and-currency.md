# Country and currency

Accept assigned alpha-2 or alpha-3 country letter codes case-insensitively and
resolve to uppercase alpha-2. Use `iso3166-1.json`, copied unchanged from the
project's Product Scorer package. Its metadata identifies pycountry 24.6.1,
extraction on 2026-11-01 and 249 entries. This is a pinned lookup, not a live registry
or a Python runtime dependency. Reject names, numeric codes, regional groups,
unsupported aliases and surrounding whitespace rather than silently repairing them.
Update lookup and schema together when an assignment changes.

Require currency independently. Use a recognizable three-letter monetary currency
code and normalize letters to uppercase. The schema validates letter syntax; the
agent must resolve its meaning. When an unfamiliar code cannot be identified safely,
verify it with an available authoritative currency reference or return an error
asking for a valid explicit currency. Do not invent a code or a country-currency map.
Do not treat no-currency/test labels as a usable settlement currency.

Use the currency for COGS, fixed fees, offer prices, AOV, buffers, CPC and CPA. Do
not assume currency from country, from a dollar sign, from location, or from the
sample files. Do not fetch an exchange rate or convert money. Target-country
research may use listings in another currency as context only when clearly labeled;
do not mix those numbers into this monetary model without a user-supplied basis.
No such conversion field exists in the current contract.

Country labels do not establish tax rates, shipping, language, purchasing power,
seasonality, advertising-policy compliance or demand. Tax/fee strings are explicit
modeling instructions supplied by the caller, not legal conclusions. Keep all
unverified local observations as hypotheses or validate them with selective research.
