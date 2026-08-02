# Sub-Agent Orchestration Contract

## Purpose
Use focused sub-agents to reduce omissions as the skill grows. The coordinator remains responsible for consistency, merge decisions, and the final quality gate.

Sub-agent instructions live in `agents/`.

## Required Roles
- `platform-dependencies`: framework, caret-ranged current dependencies, exact lockfile resolutions, install-script approval, environment parsing, self-contained project scaffolding, and clean VS Code debugging
- `brand-assets`: design system application and transparent PNG logo pipeline
- `customer-experience`: customer-facing copy, narrow-mobile typography, testimonial rendering, coupon clarity, and rendered-content checks
- `commerce-checkout`: default offer prices, background landing quotes, raw vendor `Response` handling, cent-based money, coupon, checkout fields, neutral tracked/ETA copy, visible return-home navigation, Stripe, order/contact/subscription/tracking integrations, endpoint-last-argument routing, and `OfferSelect` analytics
- `quality-runtime`: unit/component/E2E tests, 100% thresholds, vendor HTTP/body regressions, cent/default-price/navigation coverage, warning-free dev/browser checks, runtime defect repair
- `deployment`: GitHub workflows, Cloudflare Pages provisioning, domains, DNS, secret isolation

The coordinator may create a separate conversion-design role when the page requires substantial content or visual work, but it must not duplicate the brand-assets ownership of logo processing.

## Execution Order
1. Coordinator validates the input contract and writes a shared decision record.
2. Platform/dependencies, brand-assets, and customer-experience may run in parallel after the decision record is fixed.
3. Commerce/checkout starts after the environment and offer contracts are fixed.
4. Deployment may start after script names and environment names are stable.
5. Quality/runtime reviews each merged workstream, writes tests, runs the full suite, and returns defects to the owning role.
6. Coordinator performs final integration and runs the complete acceptance gate again.

## Shared Handoff Record
Every role must report:
- files created or changed
- contract decisions made
- assumptions avoided
- unresolved blockers
- tests added
- commands run and results
- risks for the next role

No role may claim another role's checks passed without evidence.

## Ownership Boundaries
- Only the coordinator changes cross-cutting contracts after role work begins. The coordinator also owns recursive lower-snake-case validation for the input JSON.
- Brand-assets owns source SVG validation and derived PNG behavior.
- Commerce-checkout owns numeric `shippingId`, raw `Response` parsing, integer-cent contracts, `default_price` hydration, tracked/ETA wording, return-home UX, `STORE_INTEGRATION_ENDPOINT`/`NOTIFICATION_INTEGRATION_ENDPOINT` routing, and `OfferSelect` emission.
- Platform-dependencies owns package ranges/lockfile/install-script approval, Node version, self-contained project boundaries, `next.config`, and `.vscode/launch.json`.
- Customer-experience owns shopper-facing wording, mobile heading limits, testimonial presence, coupon clarity, and rendered-content selectors.
- Quality-runtime may require refactors in any workstream to make behavior testable.
- Deployment must not weaken quality commands to speed CI.

## Conflict Resolution
When two role outputs conflict, apply this order:
1. explicit user requirements
2. input/vendor contracts
3. security and secret isolation
4. static-export compatibility
5. correctness and testability
6. conversion/design preferences

The coordinator records the resolution and asks the user only when the conflict changes a missing business or vendor contract.

## Runtime Without Native Sub-Agents
If the execution environment cannot spawn sub-agents, the main agent must execute the same role files sequentially and preserve the same handoff/checkpoint discipline. Do not skip a role merely because native delegation is unavailable.
