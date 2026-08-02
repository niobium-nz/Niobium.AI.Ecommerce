# Vendor Integrations Contract

All vendor integrations run browser-side in client components. Do not introduce API routes, server actions, middleware, or server proxies.

Load scripts non-blockingly and only on routes that need them.

## Shared Global And Return Type
Vendor scripts attach to the `niobium` global. Every vendor method in this contract returns the raw result of a browser `fetch()` call:

```ts
type VendorMethodResult = Promise<Response>;
```

The return value is never the business payload directly. Generated code must not read fields such as `response.total`, `response.instruction`, or `response.status` as application data.

Declare minimal TypeScript global types so these methods return `Promise<Response>`, and design wrappers for dependency injection so tests can replace vendor globals with deterministic doubles.

## Integration Endpoint Routing
Two browser-safe endpoint values are required:

```txt
STORE_INTEGRATION_ENDPOINT
NOTIFICATION_INTEGRATION_ENDPOINT
```

Pass `STORE_INTEGRATION_ENDPOINT` as the final argument to:
- `niobium.store.getQuote`
- `niobium.store.makeOrder`
- `niobium.store.trackOrder`

Pass `NOTIFICATION_INTEGRATION_ENDPOINT` as the final argument to:
- `niobium.notification.subscribe`
- `niobium.notification.contactUs`

Do not omit, reorder, merge, or silently substitute these endpoint arguments.

## Mandatory HTTP Response Handling
Every vendor call must follow this sequence:

1. Await the vendor method and receive a `Response`.
2. Catch network/fetch rejection separately.
3. Verify that the result is Response-like.
4. Read the response body as JSON exactly once.
5. Check `response.ok` and the numeric status code before treating the JSON as success data.
6. Reject non-2xx responses even when their body is valid JSON.
7. Reject a successful status with an empty or malformed JSON body.
8. Convert failures into operation-specific, user-friendly messages.
9. Never display raw backend bodies, stack traces, HTML, endpoint URLs, or internal diagnostic messages to shoppers.

Copy/adapt `templates/lib/vendor-response.ts` into the generated project as `lib/vendor-response.ts`. Use its `callVendorJson` or an equivalent fully tested implementation:

```ts
const quote = await callVendorJson<QuoteResponse>("quote", () =>
  niobium.store.getQuote(
    GOOGLE_RECAPTCHA_SITE_KEY,
    TENANT_ID,
    shipping_option_id,
    TARGET_COUNTRY,
    cartItems,
    couponOrNull,
    STORE_INTEGRATION_ENDPOINT,
  ),
);
```

The helper must use both `response.ok` and `response.status`, call `response.json()`, and expose a safe customer message. It may retain parsed error JSON as internal error metadata for tests/diagnostics, but shopper UI must use the safe message only.

Recommended status behavior:
- `400`, `409`, `422`: ask the customer to review the submitted information and retry.
- tracking `400`, `404`, `422`: say no matching order could be found and ask the customer to check the details.
- `401`, `403`: say the service is temporarily unavailable and offer retry/contact support; do not expose authorization language.
- `429`: ask the customer to wait briefly and retry.
- `5xx`: say the service is temporarily unavailable and suggest retrying shortly.
- network failure: ask the customer to check their connection and retry.
- malformed/empty JSON on a successful 2xx response: say the service returned an unreadable response and suggest retrying.
- malformed/non-JSON body on a non-2xx response: keep the failure classified by HTTP status and show the safe status/operation message; never expose the raw body.

Do not use `console.warn` or `console.error` as the customer-facing error path.

## Monetary Unit Contract
Every monetary amount returned by store vendor responses is an integer number of cents, not dollars or other major currency units.

This includes all quote-level and quote-line monetary fields such as:

```txt
shippingCost
discount
tax
subtotal
total
shipping
was
now
lineTotal
lineTax
```

It also applies to any monetary amount field that may appear in an order response. Do not divide cents before passing an amount to Stripe when Stripe expects the smallest currency unit. Divide by `100` only at the display/analytics boundary that explicitly requires major units.

Required rules:
- validate vendor monetary fields with `Number.isSafeInteger`
- require non-negative cents for quote prices, tax, discount, shipping, subtotal, and total
- use names such as `amountCents`, `totalCents`, or comments/types that make the unit explicit in application code
- format UI values through `formatMoneyFromCents` from `templates/lib/utils.ts`
- never pass a cent value directly to `Intl.NumberFormat` as though it were dollars
- retain quote totals in cents for Stripe Elements initialization

## Shipping Option Type
`vendor_integration.shipping_option_id` is a positive integer. `SHIPPING_OPTION_ID` is only its decimal environment transport form. Parse and validate it once, then use a numeric value throughout application code.

```ts
function parsePositiveIntegerEnv(name: string, raw: string | undefined): number {
  if (!raw || !/^[1-9]\d*$/.test(raw)) {
    throw new Error(`${name} must be a positive integer`);
  }
  const value = Number(raw);
  if (!Number.isSafeInteger(value) || value <= 0) {
    throw new Error(`${name} must be a safe positive integer`);
  }
  return value;
}
```

Do not use loose `parseInt`, unary `+`, or implicit coercion without full-string validation. Never pass the raw environment string to a vendor function.

## Quote Library
Both the landing page and checkout page call quote, but they have different blocking behavior.

Script:

```html
<script src="https://assets.store.niobium.co.nz/quote.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call and parse:

```ts
const quote = await callVendorJson<QuoteResponse>("quote", () =>
  niobium.store.getQuote(
    GOOGLE_RECAPTCHA_SITE_KEY,
    TENANT_ID,
    shipping_option_id,
    TARGET_COUNTRY,
    cartItems,
    couponOrNull,
    STORE_INTEGRATION_ENDPOINT,
  ),
);
```

`shipping_option_id` is a JavaScript `number`. `STORE_INTEGRATION_ENDPOINT` is the final argument.

### Landing Page Price Behavior
Every visible offer has a required input `default_price`:

```ts
type DefaultOfferPrice = {
  amount_cents: number;
  currency: string;
};
```

Landing behavior:
- render each offer's validated default price immediately during the first render
- keep offer selection and `Buy Now` available immediately; do not block the landing page on quote
- after hydration, start quote calls for all visible offers in the background; parallel calls are allowed
- as each successful quote arrives, validate its cents and currency, then replace the displayed default amount/currency only when different
- do not flash a blank price or skeleton over the default price
- if a landing quote fails, retain the default price and optionally show a quiet non-blocking note such as `Live price will be confirmed at checkout`
- never let one failed offer quote block the other offer cards
- default prices are display fallback only and must never be passed to Stripe or order creation

### Checkout Quote Behavior
Checkout must use a successful live quote. On checkout load:
- read selected `offer` from query string
- resolve it to the matching `OFFER_OPTION__n` cart
- call quote for that cart and selected coupon
- check HTTP status and parse JSON through the shared response helper
- validate the response shape, currency, and cent amounts
- disable payment until a valid quote exists
- show a user-friendly retry path on quote failure

Use the live quote for:
- Stripe amount in cents
- currency
- price breakdown
- product names where available
- shipping cost in cents
- tax in cents when greater than zero
- discount in cents when relevant

### Quote Types

```ts
type QuoteResponse = {
  cart: QuoteCartItem[];
  quote: QuoteLine[];
  shippingCost: number; // integer cents
  shippingDescription?: string;
  discount: number; // integer cents
  discountDescription?: Record<string, string>;
  taxInfo?: TaxInfo;
  currency: string;
  tax: number; // integer cents
  subtotal: number; // integer cents
  total: number; // integer cents
  id: string;
  coupon?: string | null;
  shipping: number; // integer cents
  shippingCountry: string;
};

type QuoteCartItem = {
  listing: number;
  option?: string | null;
  quantity: number;
  name?: string | null;
};

type QuoteLine = {
  was: number; // integer cents
  now: number; // integer cents
  taxInfo?: TaxInfo;
  currency: string;
  tax: number; // integer cents
  lineTotal: number; // integer cents
  lineTax: number; // integer cents
  discount: number; // integer cents
  listing: number;
  option?: string | null;
  quantity: number;
  name?: string | null;
};

type TaxInfo = {
  rate: number;
  kind: number;
};
```

Product name comes from `QuoteCartItem.name` where available.

## Coupon Flow
The landing page supports optional `?coupon=COUPON_CODE`.

Only pass the landing coupon to checkout when present.

Checkout coupon priority:
1. Manual coupon entered on checkout.
2. Coupon from landing-page query string.
3. `FALLBACK_COUPON`, only when non-empty.

When coupon changes:
- block payment interaction temporarily
- show loading state
- call quote again immediately
- parse/check the returned `Response`
- refresh all cent-based price breakdown values
- when a coupon is present, show `Coupon applied to this order`
- retain the last valid summary only as visually stale/disabled state; never allow payment against a stale quote

## Order Library
Script:

```html
<script src="https://assets.store.niobium.co.nz/order.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call and parse:

```ts
const orderResponse = await callVendorJson<OrderResponse>("order", () =>
  niobium.store.makeOrder(
    GOOGLE_RECAPTCHA_SITE_KEY,
    TENANT_ID,
    {
      shippingId: shipping_option_id,
      shippingCountry: TARGET_COUNTRY,
      consignee,
      email,
      shippingAddressLine1,
      shippingCity,
      shippingPostcode,
      billingName,
      billingAddressLine1,
      billingCity,
      billingCountry: TARGET_COUNTRY,
      billingPostcode,
      cart,
      coupon,
      notes,
      phone,
      shippingAddressLine2,
      shippingSuburb,
      shippingState,
      billingBusiness,
      billingAddressLine2,
      billingSuburb,
      billingState,
      marketingSubscription,
      track,
      culture: navigator.language || "en-US",
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC",
    },
    STORE_INTEGRATION_ENDPOINT,
  ),
);
```

Rules:
- `shippingId` is the validated numeric value derived from `SHIPPING_OPTION_ID`
- `shippingCountry` and `billingCountry` equal `TARGET_COUNTRY`
- `cart` comes from the selected `OFFER_OPTION__n`
- `coupon` follows coupon priority
- `billingName` is always sent
- billing fields are sent even when derived from shipping
- `marketingSubscription` defaults to `true` but remains customer-controllable
- do not call the separate subscription library from checkout
- check the HTTP response and parse JSON before reading `instruction`

```ts
type OrderResponse = {
  instruction: string;
  [key: string]: unknown;
};

if (!orderResponse.instruction?.trim()) {
  throw new Error("The order service did not return payment instructions.");
}
const clientSecret = orderResponse.instruction;
```

Any additional monetary field in `OrderResponse` is cents. Do not display or use an undocumented field merely because it exists.

## Stripe Payment Element
Use Stripe Payment Element in deferred-intent style.

Flow:
1. Obtain and validate a live quote.
2. Initialize Stripe Elements using quote `total` directly as cents, quote currency, and `STRIPE_PUBLIC_KEY`.
3. Mount Payment Element.
4. Wait for customer submit.
5. Call `elements.submit()`.
6. If local and Stripe validation pass, call `makeOrder`.
7. Check the HTTP status and parse order JSON.
8. Treat validated `orderResponse.instruction` as `clientSecret`.
9. Call `stripe.confirmPayment({ elements, clientSecret, confirmParams })`.

Return URL:

```ts
const returnUrl = new URL("/order-status", window.location.origin).toString();
```

If `stripe.confirmPayment` fails before redirect, show a safe user-facing failure, allow retry where appropriate, do not call a cancel/failure endpoint, and retain the created order.

## Marketing Email Subscription
The landing page includes an email-only subscription form near the footer.

Script:

```html
<script src="https://assets.notification.niobium.co.nz/subscribe.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call and parse:

```ts
const subscriptionResult = await callVendorJson<unknown>("subscribe", () =>
  niobium.notification.subscribe(
    GOOGLE_RECAPTCHA_SITE_KEY,
    TENANT_ID,
    APP_NAME,
    subscriberEmail,
    "",
    "",
    "",
    NOTIFICATION_INTEGRATION_ENDPOINT,
  ),
);
```

Pass empty strings for first name, last name, and tracking ID. The endpoint remains final. Treat success only after the HTTP status is 2xx and JSON parses successfully.

UI: validate email, show loading, disable submit, show success after parsed 2xx JSON, and show the safe retry error otherwise.

## Contact Page
Route: `/contact`.

Required fields: name, email, message.

Script:

```html
<script src="https://assets.notification.niobium.co.nz/contact-us.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call and parse:

```ts
const contactResult = await callVendorJson<unknown>("contact", () =>
  niobium.notification.contactUs(
    GOOGLE_RECAPTCHA_SITE_KEY,
    TENANT_ID,
    visitorName,
    visitorEmail,
    visitorMessage,
    NOTIFICATION_INTEGRATION_ENDPOINT,
  ),
);
```

The endpoint remains final. Treat success only after parsed 2xx JSON. Show validation, loading, disabled, success, and safe retry states.

## Track Order Page
Route: `/track-order`.

Display both methods in one form:
- preferred: email + numeric order number
- alternative: email + first name

First-name matching is case-insensitive. Order numbers are numeric with no leading zero.

Script:

```html
<script src="https://assets.notification.niobium.co.nz/track.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Email + order number:

```ts
const tracking = await callVendorJson<TrackResponse>("track_order", () =>
  niobium.store.trackOrder(
    GOOGLE_RECAPTCHA_SITE_KEY,
    { email: "john@example.com", order: 1234567890 },
    STORE_INTEGRATION_ENDPOINT,
  ),
);
```

Email + first name:

```ts
const tracking = await callVendorJson<TrackResponse>("track_order", () =>
  niobium.store.trackOrder(
    GOOGLE_RECAPTCHA_SITE_KEY,
    { email: "john@example.com", firstName: "john" },
    STORE_INTEGRATION_ENDPOINT,
  ),
);
```

The namespace/script pairing is intentional. The store endpoint remains final.

```ts
type TrackResponse = {
  created: string;
  status: OrderStatus;
  cart: TrackCartItem[];
  shippingStatus: ShippingStatus;
  shippingCity: string;
  shippingState?: string | null;
  shippingCountry: string;
};

type TrackCartItem = {
  listing: number;
  option: string;
  quantity: number;
  name?: string | null;
};

enum OrderStatus {
  Created = 0,
  PartiallyPaid = 10,
  Paid = 20,
  Shipped = 30,
  Delivered = 40,
  Completed = 50,
  Cancelled = 60,
  Refunded = 70,
}

enum ShippingStatus {
  NotApplicable = 0,
  Pending = 1,
  Processed = 2,
  Shipped = 3,
  Customs = 4,
  Delivering = 5,
  DeliverAttemptFailed = 6,
  Delivered = 7,
  Returned = 8,
  Cancelled = 9,
}
```

On success, display tracking clearly. It is appropriate to say the package is tracked or show carrier progress when tracking data supports that claim. Do not use the words `oversea` or `overseas`, do not emphasize fulfillment origin, and do not falsely imply local dispatch. A clear carrier delivery ETA is allowed when supported by the input or tracking response.

## Order Status Page
Route: `/order-status`.

Do not call Stripe.js or vendor APIs to retrieve or verify status. Use only Stripe's `redirect_status` URL parameter.

Success:
- say the order is being processed
- say confirmation and shipping updates will arrive by email
- if the input says delivery is tracked, it is appropriate to mention that tracking details will be sent after dispatch
- do not overstate fulfillment completion or mention fulfillment origin

Failure:
- say payment/order could not be completed
- show a meaningful safe URL status reason when available
- otherwise use a generic failure
- link to contact

Missing/unknown/uncertain:
- do not provide a definitive order/payment update
- say status cannot be confirmed from this page
- link to contact/support
