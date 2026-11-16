# Build and verify the discovery agent installation kit

From the repository root, use Python 3.11+:

```sh
python3 -B tools/ecommerce-product-discoverer/build.py
python3 -B tools/ecommerce-product-discoverer/test_bundle.py --bundle dist/ecommerce-product-discoverer
python3 -I -S -B tools/ecommerce-product-discoverer/test_consumers.py --bundle dist/ecommerce-product-discoverer
```

The builder downloads only exact URLs and hashes from the reviewed dependency lock.
For subsequent builds, use `--offline` with the populated `.cache/discoverer-wheels`
cache. Use `--output dist/a-fresh-kit-name` for another build. An existing output is
never overwritten because users may have edited its deployment configuration.
The host itself never downloads dependencies. Distribution ZIP timestamps/order
are fixed, so identical source and locked wheels produce identical kit bytes.

The output contains ready-to-upload agent/support folders, an unconfigured deployment
JSON, exactly one skill ZIP, generated ChatGPT instructions and a setup-check prompt.
Start with its `START-HERE.md`. It is not a native vendor import format. No credentials,
real folder ID, runtime mount path or extra skill-version pin is generated.

The default offline libraries support standard CPython 3.11-3.14 on Linux glibc
>= 2.17, x86_64/aarch64. CI runs the package and affected consumer suites on Linux
x86_64 Python 3.11 and 3.14. The presence of other wheel targets is compatibility
provisioning, not evidence that every target has been executed. The portable agent
can use another host's equivalent real schema validator.

`test_bundle.py` tests the built upload artifact and the 76 deterministic source
tests it contains. It verifies separate skill resources, relocatability, current
schema selection, valid/invalid cases, unchanged resources, integrity errors and
reproducible offline rebuilding. `test_consumers.py` checks the downstream discovery
looper against the relocated canonical skill schema. Both are offline checks; they
do not test an LLM's hosted orchestration or live connectors.

The **Discoverer package** workflow runs on relevant pushes, pull requests and manual
dispatch. A successful run exposes the tested install kit as an artifact. Artifact
retention is 90 days; regenerate it with the same commands if it expires. Review
results for both Python versions before distributing the kit. The workflow has read
access to repository contents and does not publish, schedule or invoke any agent.

For updates, keep generated outputs out of Git and change canonical source only.
Build a fresh kit, run both checks, and replace hosted agent/support/instructions
as one coherent set while retaining one active deployment config. Skill changes are
a deliberate separate attachment replacement followed by compatibility checks.
If the host cannot execute the mock acceptance checklist, record it untested and
continue setup when the resource/schema prerequisites pass. A real discovery run
still needs the user's request and complete result verification.
