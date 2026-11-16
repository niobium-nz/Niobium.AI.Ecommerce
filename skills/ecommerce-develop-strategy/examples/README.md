# Synthetic examples

All example products, name concepts, prices, tax rates, processing fees, segments,
conversion assumptions and economics are fictional test data. No live ads, market
research, trademark search, country tax verification or performance validation was
performed to create them. The 10% tax example is not a tax recommendation for NZ
or any other country. Do not reuse examples as defaults for a real request.

`input.json` and `output.json` form a complete synthetic strategy example, including
three segments with two angles each. It illustrates every output field, coherent
asset references, independently understandable segment context and tax-inclusive
arithmetic. The worked calculation uses 50% single-unit orders, 35% two-unit orders
and 15% three-unit orders, with a fixed NZD 5.00 modeled profit buffer.

`output.error.json` illustrates the error-only shape. `scenarios.json` contains
structural and semantic acceptance cases. `economics-cases.json` contains offline
formula cases for tax inclusion, tax addition, fixed and percentage fees, bundle
costs, zero/negative margins and undefined funnel denominators. They are test
fixtures, not an executable implementation or proof that a model follows the rules.
