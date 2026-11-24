# Product Council, Supplier Sourcing and human onboarding decision

Status: approved architecture; reference orchestration implemented under `agents/ecommerce-product-council`. Supplier network integrations and a hosted human-review UI remain host responsibilities.

## Lifecycle

Product Discovery -> Competitive Research -> Product Council -> Supplier Sourcing -> Human Review -> Product Onboarding.

The Council optimizes which product to investigate next. Sourcing establishes whether it can be sourced. A human decides whether it should enter the business.

## Council

Input is one immutable, nonempty batch of competitively researched candidates with stable product IDs and cited evidence. Empty batches, duplicate IDs and malformed inputs are workflow errors, not rejections. Five independent specialist perspectives assess market/demand, competitive position, commercial potential, risk/compliance/IP and operations/customer experience. Each provides a 0-100 score, pros, cons, risks, uncertainties, citations, supplier questions and mitigations. No specialist may veto, approve or remove a candidate.

A deterministic chair combines the five assessments with versioned configurable weights (default commercial 30%, market 25%, competitive 20%, risk 15%, operations 10%). Higher score wins; ties resolve by stable product ID. All candidates remain ranked. Exactly one top product is selected for sourcing even if every candidate has poor absolute quality or severe risks. The selected product is NOT approved, safe, compliant, profitable or eligible for onboarding merely by selection. Report absolute scores, limitations, dissent, evidence, full per-candidate assessments and the selection rationale.

Specialists may use AI research tools in a host implementation; the reference code accepts their structured evidence and performs deterministic synthesis. Never invent citations, supplier claims or compliance clearance.

## Sourcing

Supplier sourcing consumes the selected product ID and complete council dossier, not just the winning score. Validate supplier identity, exact SKU/equivalence, MOQ, pricing, lead time, quality, photographs, certifications, IP/compliance concerns, shipping and landed economics. Label each finding as verified, unverified or blocked with provenance. Preserve unresolved risks and the council's supplier-validation questions. Do not silently substitute another product. If no credible supplier is found, still create a sourcing report with blockers for human review; never claim successful sourcing.

## Human decision

`approved_for_onboarding` starts unset (null). After sourcing, a human reviews the research, full council assessments, dissent, pros/cons, risk findings, supplier evidence and open questions. Allowed outcomes: approve (sets true), reject (sets false), request_more_information (keeps null). Persist actor, timestamp, rationale and exact evidence-version fingerprint. A new evidence version invalidates the old approval. The onboarding boundary must require explicit true for the current evidence version; pending, rejected or stale decisions must not proceed. Council and supplier agents never write approval.

Rejection retains the audit trail. A new selection cycle may consider remaining candidates using updated evidence, rather than automatically promoting the runner-up or retrying rejected products indefinitely.

## Durable review boundary

The [Drive-backed review store](product-council-drive-persistence.md) persists an immutable sourcing manifest and append-only human decision events. JSON content is authoritative; compact Drive properties are only an index. Evidence, record checksums, decision identities and causal links are validated on reads. Identical retry files represent one logical event.

A reviewer submits the displayed `expected_head_event_id` along with the evidence version. Stale heads are rejected. Distinct competing decisions block `get()` and onboarding authorization; timestamp order never chooses a winner. An authorized human must inspect all competing decisions and append a resolution referencing the exact current heads. Conflicted reviews remain visible in the pending queue.

The host owns reviewer authentication, Drive credentials and permissions, retention, and the real onboarding side effect. Drive supplies neither a multi-object transaction nor protection against privileged modification/deletion. Hosts needing strict ordering must serialize writes and onboarding outside Drive. Always re-run `authorize(review_id, current_handoff)` immediately before onboarding; never authorize from `inspect()`, a cached result or a caller-supplied actor string.

## Acceptance criteria

- Nonempty valid batch yields exactly one winner, including an all-bad batch.
- No specialist veto; all risks and dissent remain visible.
- Scores and tie-breaks are reproducible and versioned.
- Sourcing operates only on selected ID and preserves evidence.
- Approval requires human actor, rationale, evidence version and explicit approve action.
- Pending/rejected/stale decisions block onboarding.
- Conflicted, corrupt, orphaned or incompletely read histories block onboarding.
- All transitions are auditable and idempotent for repeated identical human decisions.
- Conflict resolution retains every competing event and requires a fresh human action.
- The reference implementation is not a live supplier lookup, user interface or production orchestrator.
