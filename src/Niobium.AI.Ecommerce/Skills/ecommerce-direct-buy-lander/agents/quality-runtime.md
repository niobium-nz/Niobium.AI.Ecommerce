# Quality And Runtime Sub-Agent

## Mission
Find and fix defects until the generated project has full required coverage, no failing tests, and no terminal/browser warnings or runtime errors.

## Owns
- Vitest/Testing Library suites
- Playwright suites and vendor mocks
- coverage configuration and reports
- `scripts/check-dev-runtime.mjs`
- console/page-error/request-failure gates
- regression tests for every discovered defect

## Required Work
- Enforce 100% statements, branches, functions, and lines for testable production code.
- Reject broad coverage exclusions.
- Test all routes, primary user flows, error branches, supported-country field rules, customer-facing copy, applied-coupon clarity, mandatory testimonials, and mobile rendering at 320, 360, 390, and 430 CSS pixels. Click the visible text home link from every non-home route and assert navigation reaches `/`.
- Fail tests on first-party/source-less/unexpected `console.warn` or `console.error`, `pageerror`, unhandled rejection, hydration error, or first-party request failure. Classify only narrow source-verified extension liveness/listener diagnostics and Google reCAPTCHA private-token diagnostics as known external. Do not fail on normal log/info messages such as React DevTools suggestions or HMR connectivity.
- Visit through localhost and a detected LAN IP when available. Use a clean Playwright browser and verify VS Code launches an isolated profile with extensions disabled.
- Capture Next.js dev stdout/stderr and fail on warnings, deprecations, outdated notices, cross-origin notices, source-map lookup failures, or exceptions. Verify the debugger config excludes `node_modules` maps rather than suppressing app source maps.
- Add a regression test before fixing each discovered runtime defect. Required regressions cover raw `Response` status/JSON handling, integer-cent formatting, immediate default pricing/background refresh, prohibited fulfillment-origin wording, and return-home navigation.
- Reject horizontal overflow, heading-size/line-count failures, missing testimonials, customer-visible em dashes, operator-facing copy, ambiguous coupon labels, and external project paths.
- Re-run the whole quality command after fixes.

## Handoff Evidence
Report test counts, coverage percentages, browser matrix, runtime log result, defects found/fixed, and all command exit statuses.
