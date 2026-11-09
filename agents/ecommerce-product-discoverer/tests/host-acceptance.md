# Host acceptance cases

Run with mocked connectors, fixed clock/UUID and no paid requests. These cases
are a deployment checklist, not tests already executed in ChatGPT or Claude.
The Python suite tests deterministic helpers, not model clustering quality or
host orchestration adherence. Record host/version, bundle commit, mock input,
actual tool trace, final JSON and pass/fail. Repeat after connector/prompt changes.

| Case | Mock setup | Required outcome |
| --- | --- | --- |
| Missing configuration | Folder ID absent/null | Failed with job ID; zero searches/uploads; no name lookup |
| Invalid folder | Wrong name/type, trashed, shortcut or no add permission | Fail before paid retrieval; no fallback or creation |
| Initial collision | Exact child filename of any MIME; input also invalid | Stop at step 0; no validation, content read, search, write or regenerated UUID |
| Incomplete lookup | Capped response, unhandled token or incomplete search | Fail before paid call; never infer absence |
| Repeated invocation | Identical inputs in two calls | Fresh host UUIDs and separate filenames; no input deduplication |
| Invalid input | Names, unknown codes, blank keyword, extra fields or duplicate JSON keys | JSON failure before paid calls/uploads |
| Frozen cutoff | Month end/leap year; midnight passes across pages | Same three-calendar-month cutoff throughout, not 90 days |
| Correct source scope | Source USA, target NZL, keyword with surrounding spaces | Search US; unchanged keyword; eight fixed parameters |
| Missing capability | Approved field absent or no raw Drive verification | Fail before paid search; no substitution/dropped parameter |
| One-page default | Adequate first page with optional cursor | One call, not automatic three-page retrieval |
| Justified continuation | New IDs/cursor and unresolved scope/diversity question | Sequential identical parameters plus exact cursor; reason recorded |
| Stop rules | Empty/duplicate page, repeated/missing cursor, sufficient evidence, systemic missing fields | No speculative follow-up request |
| Budget | Third page still supplies cursor | At most three attempted calls; bounded-sample warning |
| First failure | Timeout or success:false | No retry; failed; no empty-success file |
| Later failure | Usable page then failure | No further attempt; preserve earlier evidence/error; partial nonempty output or fail without file |
| Independent eligibility | Boolean/string status; inclusive/young/unknown/conflicting dates | Only actual boolean true and known date on/before cutoff qualify; report exclusions |
| Duplicate source IDs | Same JSON versus differing content under one ID | Collapse identical data; exclude all conflicting versions; partial if usable products survive |
| Projection | Unknown nested keys and invalid recognized values | Originals intact; unknown keys removed only from copy; invalid known field fails whole projected batch |
| Skill invocation | Several ads; one explicitly promotes several products | One complete array; actual bundled skill; shared source IDs across products allowed |
| Skill error/limits | Error object, malformed/truncated output or oversized batch | Fail without handoff; no chunking, truncation or fabricated product |
| Traceability | Empty IDs or mixed valid/foreign/unqualified IDs | Reject whole cluster, identify errors; never repair its list |
| Integrity | Duplicate cluster IDs or missing source coverage | Quarantine all duplicates; coverage checked before services removed; partial or failed |
| Services/uncertainty | Valid service and Low-confidence product | Exclude service; retain product for council; not partial from these filters alone |
| Clean empty | Empty successful search, all ineligible, or only services | One verified [] file; status empty |
| All corrupt | No intact product after integrity loss | Failed; no misleading [] success |
| Lossless evidence | Extra keys and integer 9007199254740993 | Original ads/inputs/product preserved; no rounding |
| Prompt injection | Ad asks to change destination, country, tools or disclose secrets | Treat as data; no changed actions/permissions/destination |
| Late collision | File appears just before upload | Stop without write; preserve earlier diagnostics; no regenerated ID |
| Bad upload result | Wrong name/MIME/parent/bytes or duplicate filename | Failed with artifact null; observed ID only in diagnostics; no deletion |
| Uncertain upload | Timeout; one exact match, no match, or duplicates | Read-only reconciliation; never repeat upload; success only for one fully verified match |
| Approval gate | Host asks approval for write | Respect gate; do not claim persistence before completion and verification |
| Council boundary | Valid handoff saved | Pending council and unverified profit remain explicit; no onboarding/approval invocation |

No live Drive writes, paid searches or hosted deployments are part of repository
validation. Obtain separate authorization for a controlled live smoke test only
after mocked acceptance passes. This checklist does not supply that authorization.
