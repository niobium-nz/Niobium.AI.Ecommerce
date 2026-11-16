# One selected skill, portable resource binding

`ecommerce-cluster-ads` is an external skill dependency. Its canonical repository
source is `skills/ecommerce-cluster-ads` at the repository root. Release tooling
makes one ZIP from that source; the agent directory contains no skill copy.

## Host adapter contract

1. Select the complete skill through the current agent/task's host configuration.
   In ChatGPT this means the ZIP attached with **Add skill** in this workspace
   agent, not a global Plugins/Skills entry. Other hosts use their scoped skill
   selection mechanism.
2. Resolve `SKILL.md` and its referenced resources through that selection for the
   current execution. Read the suffix dataset and rules as well as both schemas.
   Do not search arbitrary local/global copies by name, scrape host storage,
   persist temporary paths or infer a host version from a local content hash.
3. Register both logical schema URIs in `skill-bindings.json` against the input
   and output schema bytes from the same selected package. Resolve internal and
   relative references locally. A URI is an identifier, not a network URL.
4. Invoke that same skill. Missing resources or incompatible schemas are a
   capability/compatibility error; do not fall back to a bundled duplicate.

There is intentionally no extra runtime skill version pin. Host selection is
sufficient for this deployment policy; no claim is made about a vendor's internal
immutability/version guarantees. Resource and fixture checks detect incompatibility,
not arbitrary semantic changes to the skill's instructions.

## File-based validation adapter

The optional Python helper takes `--skill-resources` as a per-execution argument.
The caller supplies the directory currently exposed for the selected skill. This
is a transient resource handle, not an installation path to put in instructions,
configuration or a manifest. The helper never discovers a skill, copies it into
agent files, changes its resources or authenticates host attachment identity.
For source tests it passes the same handle through the process-only
`ECOMMERCE_CLUSTER_ADS_RESOURCES` variable.

Hosts with resource APIs instead of readable directories may bind the same schema
bytes to an equivalent local registry and validator. Do not turn a limitation of
this Python adapter into a platform-wide requirement. If the selected skill cannot
expose required resources at all, report that specific missing capability.

## Schema consumers

The handoff's product reference is
`urn:niobium:ecommerce-cluster-ads:schema:output#/$defs/cluster`.
Register the output URI even when a consumer does not invoke the skill. Repository
consumer tests bind the canonical skill schema from source; a deployment binds
its configured dependency. This changes schema resolution, not stored product or
handoff fields. Existing discovery JSON remains version 2.0.0.
