# Checkout Principles

## Scope
Apply these principles to the generated `/checkout` page where they fit this skill's constraints:

- static-export Next.js
- browser-side vendor integrations only
- no cart UI
- single selected marketing offer
- live-quote checkout pricing in integer cents
- Stripe Payment Element deferred-intent flow
- one target country per generated site

Nothing in this document overrides explicit user constraints, truthful-claims rules, vendor API contracts, or the requirement to avoid server runtime.

## Principles To Apply

### 1. Keep Checkout Guest-First
Do not add account creation, login gates, or password creation.

The generated checkout is inherently guest checkout. Keep it that way and avoid competing account prompts.

### 2. Reduce Visible Form Effort
Keep the form focused on fields required to fulfill and process the order.

Use the minimum fields defined by `country-checkout-field-rules.md`:
- email
- first name
- optional last name
- optional phone
- localized shipping address
- optional order notes
- optional collapsed billing section
- payment element

Do not add title, date of birth, company, marketing preferences beyond the required checkbox, or extra demographic fields unless explicitly required by the user.

### 3. Keep Optional Fields Clearly Optional
Mark required and optional fields in the checkout information flow.

At minimum:
- visually mark optional phone
- visually mark optional last name
- visually mark optional address line 2
- visually mark optional order notes
- visually mark optional billing fields when expanded

### 4. Collapse Billing By Default
Default billing to same as shipping.

Hide the full billing form unless the customer explicitly chooses to provide separate billing information.

Even when hidden, send billing fields to the order API using shipping-derived values.

### 5. Treat Address Line 2 As Optional
Address line 2 should not create friction.

For most countries, it should be optional and visually secondary. Where unit/floor is common, such as Singapore, keep it easy to find but still optional unless the customer chooses to enter it.

### 6. Make Coupon Handling Clear But Not Dominant
Coupon entry is allowed and required by this skill, but it should not dominate the payment path.

Checkout must:
- when a coupon is present, label it exactly `Coupon applied to this order`
- allow the customer to change it
- refresh quote immediately after coupon change
- show loading while quote refreshes
- temporarily block payment interaction during quote refresh

Do not hardcode discount claims.

### 7. Show Live-Quote Order Summary
Use a successfully parsed, validated 2xx quote response for every checkout price, shipping, tax, discount, currency, and total claim. Vendor amounts are integer cents; format them through the cent-safe display helper and pass the untouched cent total to Stripe. Never use the landing-page default price in checkout.

Recommended display:
- product/offer summary
- line items when useful
- subtotal
- discount when greater than zero or coupon explains it
- shipping cost/description
- tax when greater than zero
- total and currency

If quote is still loading, use skeleton/loading states rather than fake prices.

### 8. Keep Payment Trust High
The Stripe Payment Element should feel integrated with the site.

Use:
- brand-consistent radius, spacing, and typography where Stripe appearance settings allow it
- concise secure-payment reassurance near the payment section
- clear error messages above or near the failing area
- no fake security badges or unsupported payment claims

### 9. Use Adaptive, Helpful Errors
Vendor methods return raw `Promise<Response>` values. Handle rejected promises, non-2xx status, malformed/empty JSON, and unexpected body shapes before consuming any fields. Error messages should tell the customer what failed and what to try next without showing raw backend response text.

Examples:
- missing offer: “We could not identify the selected offer. Please return to the product page and choose an offer again.”
- quote failure: “We could not refresh the price right now. Please retry before payment.”
- payment validation failure: show Stripe's safe error message if provided, otherwise show a generic retry message
- order creation failure: explain the order could not be created and suggest retrying or contacting support

Do not rely only on console errors.

### 10. Prevent Double Submission
During quote refresh, order creation, Stripe Element submission, and Stripe confirmation:
- disable relevant buttons
- show loading state
- prevent duplicate order calls
- prevent interacting with payment while amount/currency is stale

### 11. Keep The Checkout Layout Distraction-Reduced
Use a simplified shell for `/checkout`:
- minimal header
- no large navigation
- a visible text `Back to home` link to `/` near the top; do not rely on a logo-only path
- no social feeds or unrelated promotional blocks
- focused order summary + form + payment layout

The page should still feel like the same brand and a continuation of the landing page purchase flow.

### 12. Handle Order Status Carefully
The order status page must not overstate success.

If Stripe `redirect_status` indicates success, say the order is being processed and email updates will follow. Do not claim payment, fulfillment, shipping, or delivery is fully complete beyond what the redirect status supports.

## Source Notes

The following Baymard Institute resources informed these principles:

- Baymard's checkout form-field research recommends reducing form effort and notes common opportunities such as hiding Address Line 2, hiding billing address fields, hiding coupon code where appropriate, and delaying account creation: https://baymard.com/blog/checkout-flow-average-form-fields
- Baymard's checkout UX best-practices article emphasizes prominent guest checkout, marking required/optional fields, using adaptive error messages, choosing the right interface for optional inputs, and explaining phone-number requests: https://baymard.com/blog/current-state-of-checkout-ux
- Baymard's payment UX guide stresses that unclear, untrustworthy, or hard-to-recover payment forms can lose users at the point they are ready to complete purchase: https://baymard.com/learn/payment-ux
- Baymard's checkout usability research hub states that it has repeatedly tested checkout flows of leading ecommerce sites and uses large-scale qualitative studies and audits: https://baymard.com/research/checkout-usability


### 13. Keep Delivery Reassurance Useful
When the input confirms tracking, say the package is tracked and show the supplied carrier delivery estimate. Keep the wording focused on ETA, tracking, and support. Do not emphasize fulfillment origin, do not use `oversea` or `overseas` in customer-facing copy, and do not invent local-dispatch claims.
