# Host acceptance checklist

Use mocks and a fixed source snapshot before any authorized live run. Record host
version, package commit, actual connector/dispatcher schemas, payloads, receipts,
journal transitions and terminal JSON. These are required deployment cases, not
claims of hosted tests already performed by the offline Python suite.

| Scenario | Required observable behavior |
| --- | --- |
| Exact source | Lookup is complete, parent-scoped and all-types; zero/multiple matches fail without child calls. |
| Wrong artifact | Reject a native doc, shortcut, folder, excerpt or candidate subfolder file instead of upstream JSON. |
| Read race | Metadata/version/content mismatch or incomplete bytes fails before any dispatch. |
| Source integrity | Mixed job/inputs, duplicate clusters, bad ad traceability or malformed final entry prevents all calls. |
| Empty source | Return empty, zero calls, no researcher or storage write. |
| Nullable product details | A schema-valid discovery with unknown name/link reaches researcher validation unchanged. |
| Mapping | Exactly one job/product/ads payload; no full array, additional product, provenance or rewritten source data. |
| Lossless transport | Preserve large integers, arbitrary decimal precision, nulls, exact ID strings and unknown provider fields. |
| Injection | Ad text cannot change agent binding, country, folder, ordering, permissions or registry scope. |
| Product serialization | Across two loopers and direct researcher calls, the same registry has only one product transaction active. |
| Guard handoff | Coordinator admission works with the researcher's own guard; no nested-lock deadlock. |
| Context isolation | Fresh child context per product; only the researcher may fan out keyword workers. |
| Shared ad | Do not prefilter another product; the global registry decides whether its shared input ID is duplicate. |
| Partial research | Preserve the child's partial result/errors and continue only after confirmed registration and quiescence. |
| Known local failure | Input/normalization/all-query/scoring failure before signal allocation is recorded; later products continue. |
| Infrastructure failure | Preflight or unreadable registry stops; do not repeatedly submit the rest against a broken dependency. |
| Pending registration | Preserve published identifiers, stop, retain durable recovery hold; remaining entries not_run. |
| Timeout/unknown acceptance | Do not retry or start the next product; query the same invocation only when its receipt is known. |
| Invalid correlation | Wrong name, path, cluster, reused invocation/file/signal ID or inconsistent flags halts with recovery required. |
| Crash after intent | A restarted process or another host cannot silently replay an open dispatch intent. |
| Crash after publication | Coordinator retains hold; researcher recovery reconciles publication/OneDrive, not a new research run. |
| Journal/hold outage | Stop dispatch and explicitly report unconfirmed hold rather than claiming persistence. |
| Capacity/cancellation | Stop safely, list every unprocessed index, and do not claim a live child was cancelled without confirmation. |
| Result accounting | Counts and indexes reconcile; no truncated summary or completed status for unprocessed entries. |
| Authority | Looper never uploads candidates, appends history, clears holds, invokes council or starts onboarding. |

No live Drive or OneDrive operations, paid searches, hosted child invocations or
real distributed-lock/crash tests are performed by this repository's offline suite.
Do not enable production without a working coordinator and verified terminal-result
binding. Merely importing instructions cannot provide those guarantees.
