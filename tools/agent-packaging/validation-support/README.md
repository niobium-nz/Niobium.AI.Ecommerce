# Optional offline validation adapter

These scripts are development/setup utilities, not agent runners or a universal
host requirement. They use real JSON Schema Draft 2020-12 validation, format checks
and explicitly bound local references. A host may provide equivalent validation
through another language, in-memory schemas or resource APIs.

The bundled wheels support standard CPython 3.11-3.14, Linux glibc >= 2.17,
x86_64/aarch64. They do not imply that an agent requires Linux, Python, ZIP upload,
a particular filesystem layout, or ChatGPT. Other environments need an equivalent
validator or a compatible optional test environment. Never substitute JSON parsing.

Run validate.py with -I -S -B and explicit per-execution resource arguments. Each
--skill NAME=PATH selects one already configured skill's current resources. Do not
store temporary paths as deployment settings or discover arbitrary global copies.
The helper checks agent/contract manifests and compatibility, not skill hashes or
host attachment identity. The current host/task configuration selects skill versions.

Dependencies come from checksum-locked original wheels with their license metadata
intact. No runtime package installation/network access is used. Audit hooks prevent
accidental socket/subprocess actions; they are not a security boundary for untrusted
Python. Execute only trusted source tests. All connector/host execution checks remain
explicitly untested by this helper. A missing host mock harness is an evidence gap;
actual missing operational capabilities remain a blocker for live work.
