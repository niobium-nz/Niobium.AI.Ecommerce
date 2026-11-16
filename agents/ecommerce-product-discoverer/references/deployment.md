# Portable kit deployment

The shared agent-kit builder supports this agent as well as the other repository
agents. Its default output is platform-neutral and contains one separate clustering
skill ZIP. Select a host adapter only for that host; directory/resource API bindings
are equally valid. See `skill-binding.md`, `tool-bindings.md` and `validation.md` for
behavior, capabilities and evidence limits.

The shared optional validator uses `--skill ecommerce-cluster-ads=<current-resources>`
instead of the legacy discoverer helper's `--skill-resources` argument. Both bind
only the selected package for the current execution. Do not persist the path.
The shared kit includes `validation-cases.json` and a coherent manifest; use its
own support folder rather than mixing helpers from different kit formats.

[Shared build guide](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/tools/agent-packaging/README.md)
