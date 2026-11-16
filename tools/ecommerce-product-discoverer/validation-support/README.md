# Offline validation support

Use `validate.py` with the agent package, its current host-selected skill resources
and active deployment config. Resolve all three through the current host bindings;
do not save a host execution path. See the agent's `references/validation.md`.

This helper checks contracts, fixtures and deterministic source tests. It is not
an agent runner, connector client or host mock-injection harness. It cannot prove
which attachment the host selected. No extra skill-version pin is enforced.

The dependency archive is generated at build time from the committed SHA-256 lock.
Original wheels retain their license and distribution metadata. Runtime validation
verifies the archive and each selected wheel, then loads them into a temporary
local library directory without installation or network access. The skill remains
in its own resource location and is never copied. Supported targets: standard
CPython 3.11-3.14 on Linux glibc >= 2.17, x86_64 or aarch64. Other environments need
an equivalent validator or a compatible runtime; do not claim parsing is validation.

Run with `python3 -I -S -B` to exclude site packages. Network/subprocess audit checks
are a guard against accidental external actions, not a security boundary for
untrusted Python. Execute only trusted package source tests. No paid searches,
Drive operations, publication or scheduling occur in these checks.

To update validation libraries, maintainers review the pinned versions, source URLs,
wheel target tags, complete transitive dependency set, licenses and SHA-256 values
in `dependency-lock.json`, then rebuild and run the package checks. Do not resolve
latest versions at host startup. These validation-library pins are separate from
skill selection, which is intentionally managed by the host configuration.
