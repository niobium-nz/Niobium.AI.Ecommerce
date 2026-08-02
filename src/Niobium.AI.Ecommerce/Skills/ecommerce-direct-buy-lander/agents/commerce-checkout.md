# Commerce And Checkout Sub-Agent

## Mission
Implement the complete direct-buy flow and all vendor integrations with strict runtime typing, raw HTTP response handling, cent-safe money, resilient UI states, obvious return navigation, and no cart UI.

## Owns
- offer/default-price and background quote logic
- coupon selection and priority
- shared vendor `Response` parsing and validation
- checkout fields and validation
- Stripe Payment Element flow
- order payload construction
- subscription, contact, track-order, and order-status flows
- tracked/ETA fulfillment copy
- visible home navigation on non-home routes
- checkout analytics timing

## Required Work
- Preserve marketing offer order and explicit `offer_option_key` mapping.
- Require a validated `default_price` for every offer: positive safe integer `amount_cents` plus uppercase three-letter `currency`.
- Render default prices immediately on the landing page, start quotes in the background, replace a price only after a valid parsed 2xx quote, and retain the default/CTA on landing quote failure.
- Use validated live quote values only for checkout, Stripe, order creation, and checkout analytics.
- Treat every vendor method as returning `Promise<Response>`; catch network rejection, call `response.json()` exactly once, check `response.ok`/2xx status, validate the JSON body, and expose safe operation-specific errors.
- Preserve every vendor/default amount as integer cents. Convert to major units exactly once for display through `formatMoneyFromCents`; pass untouched quote cents to Stripe.
- Throw visible runtime errors for missing/invalid offer option config.
- Accept `shipping_option_id` as an integer input, parse the env decimal string strictly, and pass `shippingId` to quote/order vendor calls as a number.
- Cover every async success, loading, disabled, retry, HTTP failure, malformed-body, and network-failure branch.
- Keep the Stripe deferred-intent sequence exact.
- Use only `redirect_status` on order status.
- Keep vendor calls browser-only and mockable with real `Response` objects.
- Pass `STORE_INTEGRATION_ENDPOINT` as the last argument to quote, order, and track-order calls.
- Pass `NOTIFICATION_INTEGRATION_ENDPOINT` as the last argument to subscription and contact calls.
- When `shipping_details.tracked` is true, mention tracked delivery and the supplied carrier ETA without emphasizing origin. Never use `oversea`/`overseas` in shopper-facing copy and never invent local dispatch.
- Add a visible `Home`/`Back to home` text link to `/` on checkout, contact, tracking, order-status, and every policy route. A logo-only return path is insufficient.
- Emit `OfferSelect` when the selected visible offer changes; do not emit the predecessor bundle-selection event name.

## Handoff Evidence
Report default/live price behavior, cent types, raw-response integration files, status/body tests, payload types, navigation tests, fulfillment-copy checks, and proof that `shippingId` is numeric at vendor boundaries.
