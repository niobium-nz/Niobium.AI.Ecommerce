# Current Stable Dependency Policy

## Goal
Generated projects should receive current, compatible packages without freezing every direct dependency to one exact package.json version and without silently crossing a major-version boundary during routine freshness updates.

## package.json Version Rule
Every direct dependency and devDependency must use a stable caret range:

```json
{
  "devDependencies": {
    "wrangler": "^4.113.0"
  }
}
```

Rules:
- Use `^x.y.z` with a stable three-part semantic version floor. Apply full caret semantics: `^1.2.3` stays within major 1, `^0.2.3` stays within minor 0.2, and `^0.0.3` stays on patch 0.0.3.
- Do not use exact direct-dependency declarations, `~`, `*`, `latest`, prerelease tags, Git URLs, local `file:` dependencies, or arbitrary ranges.
- `package-lock.json` must be committed and contains the exact resolved versions.
- A routine install/freshness pass may update to the newest stable version allowed by the declared caret range.
- A newer release outside the declared caret range may be reported, but do not cross the range automatically. A major upgrade requires an explicit compatibility review and a deliberate package.json change.

For a newly generated project, query npm immediately before generation and choose the current stable major unless a known compatibility constraint requires another major. Do not rely on remembered package versions.

## Resolution Procedure
1. Query npm `dist-tags.latest` for every direct dependency.
2. Select a compatible stable release and write a caret range anchored at that version.
3. Generate/update `package-lock.json` with npm so the exact resolution is reproducible.
4. Query the latest stable version accepted by each declared caret range.
5. Fail the freshness gate when the locked direct dependency is not the newest stable release accepted by that caret range.
6. Report, but do not automatically adopt, a newer release outside the declared caret range from `dist-tags.latest`.
7. Resolve peer, engine, deprecation, and install-script warnings without `--force` or `--legacy-peer-deps`.

## Install-Script Approval
Generated projects must use npm's explicit install-script controls.

For the first lockfile generation, do not run unreviewed dependency scripts. Use a lockfile-only install with scripts disabled, inspect every `hasInstallScript` entry, record the reviewed `allowScripts` decisions, and only then run a strict real install. An equivalent safe sequence is:

```bash
npm install --package-lock-only --ignore-scripts
npm approve-scripts
npm ci --strict-allow-scripts
```

The interactive approval step may be replaced by deterministic generation of the same version-qualified decisions, but it may not be skipped.

Required project state:
- top-level `package.json.allowScripts` object
- `.npmrc` containing `strict-allow-scripts=true`
- an explicit true/false decision for every resolved lockfile entry with `hasInstallScript: true`
- version-qualified keys such as `workerd@1.20260722.1`

When Wrangler resolves a `workerd` package that needs its install script, set the exact resolved `workerd@<version>` entry to `true` after review. Do not hardcode a remembered workerd version; derive it from `package-lock.json`.

Use `npm approve-scripts` to review pending scripts or generate an equivalent reviewed allowlist. The automated read-only gate must run `npm approve-scripts --allow-scripts-pending --json` and fail when the returned pending set is non-empty. CI must install with:

```bash
npm ci --strict-allow-scripts
```

If a newly resolved install-script package is missing from `allowScripts`, installation/quality must fail until the decision is reviewed and committed.

## Freshness Script Contract
`scripts/check-dependency-freshness.mjs` must:
- require `^x.y.z` for all direct dependencies/devDependencies
- reject prerelease or non-semver range floors
- read exact direct resolutions from `package-lock.json`
- verify the locked version satisfies the complete caret semantics, including the narrower rules for `0.x`, and is not older than its range floor
- query the newest stable release accepted by the complete declared caret range
- fail if the lockfile is behind that compatible release
- query overall `dist-tags.latest` and report a newer release outside the caret range without failing solely because the range is intentional

## Resolution Health Script Contract
`scripts/check-dependency-health.mjs` must:
- validate `engines.node`
- validate every `hasInstallScript` lock entry against `package.json.allowScripts`
- require an explicit approved workerd entry when workerd has an install script
- run an npm CI dry run with strict install-script enforcement
- run `npm ls --all`
- fail on npm warnings, deprecations, peer overrides, engine errors, invalid/extraneous/missing dependencies, or pending install-script approval
- never use `--force` or `--legacy-peer-deps`

## Node And TypeScript
Choose a current Node release that satisfies all direct dependencies and CI actions. Record it in `.nvmrc` and `package.json.engines.node`.

Resolve TypeScript from npm at generation time under the same caret-range policy. The known baseline at this skill revision is TypeScript 7.0.2, but the live npm registry is authoritative.

## Evidence
Before completion, report:
- every direct caret range
- every exact locked direct version
- any newer release outside the caret range that was intentionally not crossed
- the reviewed `allowScripts` entries
- `npm ci --strict-allow-scripts` result
- freshness and dependency-health command results
