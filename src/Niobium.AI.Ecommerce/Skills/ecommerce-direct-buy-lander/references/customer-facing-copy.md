# Customer-Facing Copy And Mobile Readability Contract

## Audience Perspective
Every visible word in the generated website is written for a potential customer.

Do not expose internal website-building, conversion, merchandising, design, or operator language in headings, labels, helper text, empty states, errors, or navigation.

Customer-facing copy should answer a customer question, explain a product benefit, clarify an action, or reduce uncertainty.

Examples of acceptable checkout headings:
- `Complete your order`
- `Enter your delivery details`
- `Review your order and pay securely`

Examples that must not be rendered:
- `A focused, guest checkout.`
- `Conversion-focused checkout`
- `Low-friction purchase flow`
- `Message-matched landing page`
- `Offer stack`

Internal terminology may remain only in non-shipping developer documentation. Do not place it in application source, comments, configuration, fixtures, or content that could be bundled, rendered, indexed, exposed to assistive technology, or surfaced by an error state.

## Punctuation
Do not use the Unicode em dash character in any rendered website wording.

Use a normal spaced hyphen (` - `) when a separator is genuinely needed, or rewrite the sentence with a comma, colon, full stop, or parentheses.

Generated source/config that can reach the rendered UI and generated static HTML must contain no em dash character.

## Checkout Clarity
Checkout wording must be literal and unambiguous because it relates to payment.

When a coupon is already applied, use this label:

```txt
Coupon applied to this order
```

Do not use:
- `Active coupon`
- `Current coupon`
- wording that suggests another undisclosed coupon is available

Use clear action labels such as:
- `Apply coupon`
- `Update coupon`
- `Remove coupon`
- `Pay securely`
- `Try again`

Do not expose technical state labels such as `quote state`, `selected option key`, `payment intent state`, or `vendor response`.

## Testimonials
Render the complete `trust_signal.testimonials` array on the home page without altering any entry. Persist the exact input array in `config/testimonials.json`, import that file directly in the home route, and pass the imported array unchanged to `<Testimonials>`; names, text, location fields, media fields, ratings, spelling, punctuation, and order are immutable. Never shorten, paraphrase, merge, filter, replace, rank away, or invent feedback.

Use these markers:
```html
<section data-testimonials="true" data-testimonials-total="..." data-testimonials-visible="...">
<article data-testimonial="true" data-testimonial-index="...">
<button data-load-more-testimonials="true">Load more testimonials</button>
```

Initial-load heuristic:
- 1-6 entries: render all.
- 7-9 entries: render 4.
- 10 or more entries: render 6.

When entries remain, show an accessible load-more button on the same home-page section. Load another batch on each activation, preserve order, update the visible-count marker, and remove the button only after every supplied entry is rendered. Do not hide all feedback in a carousel, modal, tab, or separate route. Missing media must fall back to a well-designed text card rather than removing the testimonial.

Component and E2E tests must load the exact input data, assert the initial count, activate load-more until it disappears, assert final count equals the input length, and verify every supplied name and testimonial text verbatim.

## Legal Policy Copy Fidelity
Treat all four policy files as legally binding. Copy input bytes exactly into `content/policies/`, bind SHA-256 values in `config/legal-content-manifest.json`, and use `lib/legal-content.ts` plus `readPolicySource` as the only route content source. Render those files without rewriting, summarizing, correcting, translating, inserting, deleting, or reordering wording. Design and formatting may change, but text content must not. Validation and tests must fail on a one-character difference or a route bound to the wrong policy.

## Mobile Typography And Layout
The generated site must remain readable from narrow phones through large phones, not only at one flagship viewport.

Mandatory validation widths:
- `320px`
- `360px`
- `390px`
- `430px`

All headings must use semantic `h1`-`h6`, `role="heading"`, or `data-headline="true"` markup so the full mobile audit can inspect every headline.

At all mandatory widths:
- no horizontal page overflow
- no clipped headings, controls, prices, or form labels
- body copy is normally at least `16px`
- interactive targets are normally at least `44px` high/wide where applicable
- headings use balanced wrapping and are not placed inside unnecessarily narrow containers

Recommended mobile type limits:
- hero `h1`: normally `30-40px`; cap at `36px` through 360px and `40px` from 361px through 430px
- section `h2`: normally `24-34px`; cap at `32px` through 360px and `36px` from 361px through 430px
- card/form `h3`: normally `20-28px`; cap at `28px` through 360px and `32px` from 361px through 430px
- display line height: normally `1.05-1.18`

Use a fluid scale such as `clamp()` or equivalent responsive Tailwind classes. Do not apply unprefixed mobile classes equivalent to `text-5xl` or larger.

A visible heading with no more than six words and no more than 42 characters must fit within two rendered lines at each mandatory mobile width. If it does not:
1. reduce the mobile font size,
2. remove an unnecessary narrow max-width,
3. reduce tracking,
4. shorten or rewrite the customer-facing copy without changing meaning.

Do not solve wrapping by shrinking important text below accessible sizes.

## Automated Checks
Generated projects must test the built static site and the local dev site for:
- all mandatory mobile widths
- heading computed sizes and short-heading line counts
- horizontal overflow
- em dash absence
- forbidden operator-facing phrases
- required testimonial section/count
- exact applied-coupon wording
- clear return-home navigation
- minimum 44px actionable dimensions for buttons, form controls, and primary actions

Any failure is unfinished work.
