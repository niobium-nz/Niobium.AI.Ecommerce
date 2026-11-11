# Ecommerce Onboarding Specialist

Platform-neutral orchestration for an already approved product: profile its source
landing page, develop the marketing strategy, draft still-image prompts, and save
one verified JSON handoff for website implementation. It prepares plans, not a
live website, approved advertising claims or generated images.

## Input and output

See examples/input.json for an illustrative invocation. Require landing_page_url,
target_country (assigned alpha-2), costs and product_reference_board. Costs contain
cogs_per_unit, extra_unit_cogs_per_order, sales_tax, payment_processing_fees and
currency. Monetary amounts are explicit finite nonnegative numbers; tax/fees
are explicit unambiguous strings. Zero is valid; missing data is not zero.
job_id and signal_id may be omitted/null; otherwise use upstream lowercase UUIDv4s.

The board must be caller-supplied and visibly show the same SKU from multiple views.
Prefer a durable HTTP(S) image URL usable by this and downstream hosts. Local files
are accepted only through an authorized host mapping to a durable, same-content
handoff URL; see references/contracts.md. No automatic board creation or upload.

Saved path: candidates/{signal_id}/{candidate_id}.json, or
candidates/unlinked/{candidate_id}.json when no upstream signal exists. The existing
candidates root is configured by ID. Each record contains the requested correlations,
target country, full marketing_strategy, full image_strategy and errors, plus the
durable product_reference_board reference needed by downstream image production.
No product profile payload or raw cost wrapper is added to the saved envelope.

A complete strategy and image-prompt success are required before any output write.
A usable partial profile may proceed with disclosed limitations. An empty successful
image_prompts list remains valid. Failure/clarification does not publish a partial
website handoff. A successful result does not certify economics or authorize launch.

## Ownership and portability

AGENT.md is the entrypoint. agent.yaml is descriptive metadata, NOT a vendor import
manifest. The three unchanged complete skill directories are moved from root skills/
into this agent's skills/: ecommerce-profile-offers, ecommerce-develop-strategy,
ecommerce-draft-prompts. Nothing is reimplemented in a new skill. The board-building
skill and all other agents remain unchanged. dependencies.json pins source trees
and schemas to the reviewed repository commit.

Configure candidates_folder_id and, only for local board inputs, an authorized
board_handoff_binding. Null folder ID is intentionally non-runnable. The browser
requirement is a real headed interactive session with visible inspection, NOT
JavaScript-capable scraping. Missing browser, board-reading or raw-file-write
capabilities are deployment blockers, not reasons to silently substitute tools.

The adapters explain separate ChatGPT Workspace Agent and Claude Cowork setup.
They do not claim universal one-click import, production deployment or live tests.
Approval remains the authorized caller's responsibility until council is built.
The requested candidate_id/storage naming does not change the business lifecycle.

## Validation

With Python 3.11+, jsonschema and pycountry installed, from this directory:

```sh
PYTHONDONTWRITEBYTECODE=1 python -m unittest discover -s tests -v
```

Tests exercise JSON contracts, exact skill mappings, error gates and mocked
publication, not real browsing, pixel fidelity, pricing judgment or connector
atomicity. Run tests/host-acceptance.md in each target host before a separately
authorized live test. Runtime does not require Python or those testing libraries.
No costs, product claims or sample URLs in tests/examples are real evidence.

## Pending work

See ../ROADMAP.md. Product council remains pending. A manufacturer-sourcing agent
is also pending; obtaining real, SKU-specific product photographs from suppliers
will enable the reference-board workflow upstream of this onboarding agent.
The website implementation agent remains a separate downstream integration.
