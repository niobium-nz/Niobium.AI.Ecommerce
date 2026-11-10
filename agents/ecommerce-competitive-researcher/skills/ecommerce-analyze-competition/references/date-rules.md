# Calendar cutoff and ad-age verification

Resolve the current date once at the start of a run from a trusted host clock/date,
using the user's configured timezone where available. If using a host date instead,
state its basis. Do not use an old example, upload timestamp or retrieved ad text
as today's date. If the date is unavailable, return an error before paid retrieval.

Subtract THREE CALENDAR MONTHS, not 90 days. Preserve day-of-month where possible;
otherwise use the last valid day of the destination month. Adjust the year when
crossing January. For February, a Gregorian leap year is divisible by 4 except
century years not divisible by 400. Format the cutoff as `yyyy-MM-dd`.

Examples below are frozen arithmetic examples, never runtime constants:

| Run date | Cutoff |
| --- | --- |
| 2026-11-04 | 2026-08-04 |
| 2026-05-31 | 2026-02-28 |
| 2024-05-31 | 2024-02-29 |
| 2026-01-31 | 2025-10-31 |
| 2026-03-31 | 2025-12-31 |
| 2026-12-31 | 2026-09-30 |

Freeze the cutoff and run-date basis across pages even if midnight passes.

## Two distinct dates

Send the cutoff as request `start_date`, as approved. Do not claim that its
impressions-date semantics impose a minimum ad age. Separately interpret each
record's explicit actual ad start date. Use an unambiguous supplied date string
or a numeric timestamp with an established encoding. The provider's documented
examples expose Unix-second `start_date` values. Do not silently reinterpret a
13-digit value as seconds, a local ambiguous date as US format, an end date as a
start date, or zero/default/missing timestamps as a real launch date.

Use ISO date-only values literally. For an explicitly UTC/offset datetime or known
Unix-seconds timestamp, compare the calendar date after conversion to the run's
recorded timezone. If exact conversion is unavailable, do not guess a borderline
classification: mark the record unverified. Read-only date/arithmetic utilities
may be used when exposed, but no script execution is required. No extra ad lookup
may be made to recover a date. Disagreeing date representations remain unresolved
unless the tool explicitly explains which one is authoritative.

Admit equality: an ad with a known start date ON the cutoff date qualifies for the
age condition. Reject definitely later starts from competitor evidence. Require
`is_active: true` from the record; the requested ACTIVE status alone is not enough
when the response omits or contradicts it. Unknown status/date is unverified,
not a confirmed young/inactive ad. Keep those counts/reasons visible in limitations.

Say "currently active with a start date on/before the cutoff" rather than
"ran continuously for three months". Current status and an old start establish
neither continuous delivery, spend, impressions, conversions nor profitability.
This is also a selected sample: recent competitors are deliberately excluded.
