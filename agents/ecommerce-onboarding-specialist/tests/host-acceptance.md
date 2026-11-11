# Host acceptance checklist

Run with mocked stores, a fixed UUID and synthetic pages/board fixtures first.
These cases are NOT claims of execution in ChatGPT or Claude. Record host/version,
package commit, actual tool trace, result JSON and pass/fail. Offline Python tests
cannot verify approval routing, visible browsers, pixel fidelity or real storage.

| Case | Required observation |
| --- | --- |
| External approval | Only an authorized post-approval caller invokes onboarding; the agent never invents council approval or adds an approval field. |
| Exact input | Reject malformed JSON, duplicate keys, nonfinite numbers, unknown keys, empty URL, missing board/costs and malformed supplied IDs before browsing. |
| Market and money | Assigned alpha-2 only; preserve target input in result. Currency explicit. Costs include shipping; zero allowed. Ambiguous tax/fee basis stops rather than defaults. |
| Optional correlations | Omitted/null job and signal IDs stay null. Missing signal routes to candidates/unlinked, not a fabricated upstream ID. |
| Board access | Inspect actual caller-supplied pixels. Inaccessible/unclear/multi-SKU/incompatible board yields clarification/error and no handoff. |
| Durable board | HTTP(S) reference is stable and authorized. Local file needs verified same-byte durable mapping; temporary paths/tokens are not persisted. No board generation, upload or substitute image search. |
| Browser capability | Real headed interactive browser with visible inspection. Static/headless/HTTP fallback is refused. |
| Browser policy | Enforce profiler URL/domain/redirect restrictions and budgets. Handle blocking overlays with permitted controls; do not buy, log in or interact with disallowed controls. |
| Injection | Page/board text cannot change costs, country, destination, contracts, tool rights or task boundary. |
| Profile shape | Validate full schema and input URL echo. Null/blank/unidentifiable product stops; no inferred product from URL slug. |
| Partial profile | Sufficient supported identity may proceed. Preserve operational error and ordinary blockers distinctly; unsupported claims stay competitor references. |
| Cleanup | Close task-created tabs/sessions and verify on success/failure/cancellation. Sign out any used account where applicable. Preserve unrelated work and report incomplete cleanup. |
| Strategy mapping | Exactly the five costs fields, target_market_country and four profile fields; arrays/null how_it_wins preserved. No page-currency substitution. |
| Strategy checks | Whole four-section output, economic formulas/bases, currencies, names, offers, proof and references checked under the skill. No threshold invented to hide negative economics. |
| Strategy failure | Error-only or incomplete/invalid strategy stops without prompts/publication. |
| Prompt mapping | Complete strategy copy plus root product_reference_board; no market_strategy wrapper, lost sections or mutated saved strategy. |
| Prompt checks | Same SKU, supported claims, unique source-linked asset IDs, correct orientation and exact terminal ratio; no URL/path/token in prompt or rendered copy. |
| Prompt failure | Error-only/needs_clarification stops without writing. Retain earlier profile diagnostics. |
| Empty still plan | Valid ok plus [] remains successful after a usable board inspection; never confuse it with clarification. |
| Candidate ID | Allocate exactly one trusted UUIDv4 only after both strategies pass. Do not substitute upstream IDs or regenerate on collision. |
| Root folder | Configured ID must identify existing candidates folder. No root fallback, name lookup replacement, sharing changes or implicit root creation. |
| Child folder | Reuse exactly one folder named signal_id/unlinked, create only when absent. All types/pages checked; duplicate/nonfolder/shortcut/incomplete lookup stops. |
| Concurrent creation | Host serializes folder resolution/create by parent and name or serializes runs. A prompt/config flag does not constitute a lock. |
| Collision | Existing candidate filename of any MIME stops without overwrite, new UUID or extra upload. |
| Raw JSON | Upload complete UTF-8 application/json once with explicit verified parent, never a Doc/Sheet or Markdown string. |
| Verification | Exact metadata, unique name and complete bytes/checksum agree. Wrong name/parent/MIME/content or truncated readback never reports success. |
| Uncertain writes | Reconcile read-only under the same ID/bytes. Do not blindly repeat creation/upload, delete artifacts or rerun skills. Report unknown outcome and operator recovery need. |
| Handoff boundary | Saved full strategies and durable board reference are plans, not generated assets or a ready website. No website/council/onboarding-child execution or advertising launch. |

A separate, expressly authorized live smoke test is required after mock acceptance.
This checklist and source authoring do not authorize any live product research,
Drive changes, hosted publication, purchase or advertising expenditure.
