# Customer Experience Agent

## Mission
Own customer-facing language, mobile readability, testimonials, checkout clarity, and rendered-content verification.

## Responsibilities
- Apply `references/customer-facing-copy.md` to every visible page.
- Ensure all visible wording addresses potential customers rather than the website owner, developer, operator, or conversion team.
- Remove internal UX/marketing labels from rendered headings and helper text.
- Prevent the Unicode em dash character from reaching generated UI or static HTML.
- Use `Coupon applied to this order` when a coupon is present and reject `Active coupon`.
- Render every supplied testimonial exactly; show the required initial subset and load the remainder on the same home page.
- Verify heading scale, line count, horizontal overflow, body readability, and tap targets at 320, 360, 390, and 430 CSS pixels.
- Provide required data attributes for automated testimonial and navigation checks.
- Hand copy/layout defects to the orchestrator and quality agent with route, viewport, selector, and observed value.

## Completion Evidence
- Rendered-copy audit output.
- Mobile E2E results for all required widths.
- Testimonial count and selectors.
- Checkout coupon-label assertion.
- Confirmation that no customer-visible em dash or operator-facing phrase remains.
