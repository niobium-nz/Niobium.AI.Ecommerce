# Platform And Dependencies Sub-Agent

## Mission
Create a warning-free, current, static-export Next.js foundation with compatible caret dependency ranges, exact lockfile resolutions, reviewed install scripts, strict environment parsing, self-contained project paths, and first-class local debugging.

## Owns
- `package.json`, `package-lock.json`, `.npmrc`, and `allowScripts`
- Node version files and CI Node version
- `next.config.mjs`
- TypeScript, ESLint, Tailwind, PostCSS, Vitest, and Playwright configs
- `.vscode/launch.json`
- environment parsing helpers
- dependency freshness/resolution-health scripts
- project-boundary scanner
- local dev origin handling

## Required Work
- Query npm immediately before generation for every direct dependency.
- Declare stable caret ranges and lock exact resolutions; keep the lock current within each declared caret range.
- Report newer majors but do not cross them automatically without compatibility review.
- Review every lockfile package with `hasInstallScript`, create version-qualified `allowScripts` decisions, explicitly approve resolved workerd when required, and enable `strict-allow-scripts=true`.
- Resolve peer, engine, install, and deprecation warnings without force flags.
- Configure `allowedDevOrigins` from localhost, LAN IPv4 addresses, and optional `DEV_ALLOWED_ORIGINS`.
- Keep browser warnings/errors visible during development.
- Parse `SHIPPING_OPTION_ID` as a strict positive safe integer and expose a number.
- Copy the retained launch config: clean isolated browser, extensions disabled, workspace source maps enabled, all node_modules source maps excluded.
- Copy every required external input asset into the generated project and fail on absolute/machine-specific/escaping paths or external symlinks.

## Handoff Evidence
Report caret ranges, exact locked versions, any unadopted newer majors, allowScripts decisions, install output, engine choice, boundary-scan result, configs, freshness/health results, lint, and typecheck.