# Ecommerce Onboarding Specialist

Platform-neutral orchestration for an already approved product: profile its source
landing page, develop the marketing strategy, draft still-image prompts, and save
one verified JSON handoff for website implementation. It prepares plans, not a
live website, approved advertising claims or generated images.

## Input and output

See examples/input.json for an illustrative invocation. Require landing_page_url,
source_country, target_country, costs and product_reference_board. Accept assigned
ISO alpha-2 or alpha-3 codes from the researcher. Source selects the Firecrawl
location on every page. Target selects the marketing and pricing market. Costs contain
cogs_per_unit, extra_unit_cogs_per_order, sales_tax, payment_processing_fees and
currency. Monetary amounts are explicit finite nonnegative numbers; tax/fees
are explicit unambiguous strings. Zero is valid; missing data is not zero.
job_id and signal_id may be omitted/null; otherwise use upstream lowercase UUIDv4s.

The board must be caller-supplied and visibly show the same SKU from multiple views.
Use a durable file URL on the connected Google Drive, which serves as shared
storage for the workflow. Other HTTP(S) URLs must be usable by the downstream
host. Local files require an authorized mapping to a durable, same-content
handoff URL. See references/contracts.md. No automatic board creation or upload.

Saved path: candidates/{signal_id}/{candidate_id}.json, or
candidates/unlinked/{candidate_id}.json when no upstream signal exists. Configure
the existing `candidates` folder directly under the deployment's ecommerce
folder by ID. Every invocation uses this destination. Each record contains the requested correlations,
target country, full marketing_strategy, full image_strategy and errors, plus the
durable product_reference_board reference needed by downstream image production.
No product profile payload or raw cost wrapper is added to the saved envelope.

A complete strategy and image-prompt success are required before any output write.
A usable partial profile may proceed with disclosed limitations. An empty successful
image_prompts list remains valid. Failure/clarification does not publish a partial
website handoff. A successful result does not certify economics or authorize launch.

## Ownership and portability

AGENT.md is the entrypoint. agent.yaml is descriptive metadata, NOT a vendor import
manifest. The unchanged complete skills live once under repository-root `skills/`:
ecommerce-profile-offers, ecommerce-develop-strategy and ecommerce-draft-prompts.
Each is selected independently through the host and shipped as its own deployment
ZIP. `skill-bindings.json` uses logical schemas instead of nested skill paths.
The saved handoff uses its existing two-letter target-country field. Read
[portable deployment](references/deployment.md) for setup and updates.

Configure candidates_folder_id and, only for local board inputs, an authorized
board_handoff_binding. Bind Firecrawl on each host for retrieval of public
merchant pages and links. The agent supplies the source country, follows relevant
same-merchant links and records any access limitations. Connect the authorized
board reader and Google Drive writer separately.

The adapters explain separate ChatGPT Workspace Agent and Claude Cowork setup.
They do not claim universal one-click import, production deployment or live tests.
Approval remains the authorized caller's responsibility until council is built.
The requested candidate_id/storage naming does not change the business lifecycle.

## Validation

The generated kit provides optional offline validation libraries, including
country/currency and URL-format checks. It needs no online install in the host.
Tests cover contracts, mappings, error gates and mocked publication; they do not
establish real browsing, pixel fidelity, pricing judgment or provider atomicity.
Host mock cases are reported untested where injection is unavailable. Required
Firecrawl/image/storage capabilities still need real bindings before live work.
No sample cost, product claim or URL in fixtures is real evidence.

## Pending work

See the [agent roadmap](https://github.com/niobium-nz/Niobium.AI.Ecommerce/blob/main/agents/ROADMAP.md). Product council remains pending. A manufacturer-sourcing agent
is also pending; obtaining real, SKU-specific product photographs from suppliers
will enable the reference-board workflow upstream of this onboarding agent.
The website implementation agent remains a separate downstream integration.

## Build the installation kit

From the repository root:

```sh
python3 -B tools/agent-packaging/build.py --agent ecommerce-onboarding-specialist
python3 -B tools/agent-packaging/test_packages.py --bundle dist/portable-agents/ecommerce-onboarding-specialist
```

Open the kit's START-HERE.md. The default kit and instructions are platform-neutral;
use the relevant host adapter only for that deployment. The optional Python checker
has documented runtime targets, and equivalent validators are supported elsewhere.
For another build use a fresh --output directory so edited settings are preserved.
Package version 1.1.1 distinguishes unreported lookup completeness from known
truncation. Existing wire formats and required identity/content checks are unchanged.
