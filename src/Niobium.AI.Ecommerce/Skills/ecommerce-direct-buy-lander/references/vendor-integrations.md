# Vendor Integrations Contract

All vendor integrations must run browser-side in client components. Do not introduce API routes, server actions, or middleware to proxy these calls.

All scripts should be loaded non-blockingly and only where needed.

## Shared Globals
Vendor scripts attach to the `niobium` global.

Implementation guidance:
- declare minimal TypeScript global types in a local `global.d.ts` or vendor type module
- load scripts through `next/script` or a small idempotent script loader
- guard duplicate script injection
- show user-facing loading and error states for every async call

## Quote Library
Both the landing page and checkout page must call quote.

Script:

```html
<script src="https://assets.store.niobium.co.nz/quote.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call:

```ts
const response = await niobium.store.getQuote(
  GOOGLE_RECAPTCHA_SITE_KEY,
  TENANT_ID,
  SHIPPING_OPTION_ID,
  TARGET_COUNTRY,
  cartItems,
  couponOrNull,
  STORE_INTEGRATION_ENDPOINT
);
```

### Landing Page Quote Behavior
On landing page load:
- call quote for all visible offers listed in `offer_options_mapping`
- parallel quote calls are allowed
- display quote-derived pricing as each offer returns
- keep pending/loading states for offers still waiting
- unlock purchase UI as soon as the selected offer has valid quote data

### Checkout Page Quote Behavior
On checkout page load:
- read selected `offer` from query string
- resolve it to the matching `OFFER_OPTION__n` cart
- call quote again for that selected cart and selected coupon

Use quote response for:
- amount
- currency
- price breakdown
- product names where available
- shipping cost
- tax when greater than zero
- discount when relevant

Do not hardcode displayed prices.

### Quote Types

```ts
type QuoteResponse = {
  cart: QuoteCartItem[];
  quote: QuoteLine[];
  shippingCost: number;
  shippingDescription?: string;
  discount: number;
  discountDescription?: Record<string, string>;
  taxInfo?: TaxInfo;
  currency: string;
  tax: number;
  subtotal: number;
  total: number;
  id: string;
  coupon?: string | null;
  shipping: number;
  shippingCountry: string;
};

type QuoteCartItem = {
  listing: number;
  option?: string | null;
  quantity: number;
  name?: string | null;
};

type QuoteLine = {
  was: number;
  now: number;
  taxInfo?: TaxInfo;
  currency: string;
  tax: number;
  lineTotal: number;
  lineTax: number;
  discount: number;
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

Product name comes from the `name` field of `QuoteCartItem` where available.

## Coupon Flow
The landing page supports optional `?coupon=COUPON_CODE`.

Only pass coupon from landing page to checkout if it is present.

Checkout coupon priority:
1. Manual coupon entered on checkout page.
2. Coupon from landing page query string.
3. `FALLBACK_COUPON`, only if it has a value.

When coupon changes on checkout:
- block payment interaction temporarily
- show loading state
- re-call quote immediately
- refresh price breakdown
- show the coupon currently being used if present

## Order Library
Script:

```html
<script src="https://assets.store.niobium.co.nz/order.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call:

```ts
const response = await niobium.store.makeOrder(
  GOOGLE_RECAPTCHA_SITE_KEY,
  TENANT_ID,
  {
    shippingId: SHIPPING_OPTION_ID,
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
    timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC"
  },
  STORE_INTEGRATION_ENDPOINT
);
```

Rules:
- `shippingId` uses `SHIPPING_OPTION_ID`.
- `shippingCountry` uses `TARGET_COUNTRY`.
- `billingCountry` always equals `TARGET_COUNTRY`.
- `cart` comes from the selected `OFFER_OPTION__n` environment value.
- `coupon` follows coupon priority rules.
- `billingName` is always sent.
- Billing fields are still sent when billing defaults to shipping.
- `marketingSubscription` comes from the checkout checkbox and defaults to `true`.
- Do not call the separate subscription library from checkout.

After successful order creation:

```ts
const orderResponse = await response.json();
const clientSecret = orderResponse.instruction;
```

Treat `orderResponse.instruction` as the Stripe client secret.

## Stripe Payment Element
Use Stripe Payment Element in deferred-intent style.

Flow:
1. Call quote.
2. Initialize Stripe Elements using quote total amount, quote currency, and `STRIPE_PUBLIC_KEY`.
3. Mount the Payment Element.
4. Wait for customer submit.
5. Call `elements.submit()` to trigger Stripe-side validation.
6. If local checkout validation and Stripe Element validation pass, call vendor `makeOrder`.
7. Receive payment instruction from order response.
8. Treat `orderResponse.instruction` as Stripe `clientSecret`.
9. Call `stripe.confirmPayment({ elements, clientSecret, confirmParams })`.

Return URL:

```ts
const returnUrl = new URL("/order-status", window.location.origin).toString();
```

Do not implement subfolder support.

If `stripe.confirmPayment` fails before redirect:
- show a user-facing failure message
- allow retry where appropriate
- do not call any cancel or failure endpoint
- keep the existing order as created

Customize Payment Element styling so it feels consistent with the generated website.

## Marketing Email Subscription
The landing page must include a marketing email subscription section near the bottom or in the footer.

Fields:
- email only

Script:

```html
<script src="https://assets.notification.niobium.co.nz/subscribe.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call:

```ts
await niobium.notification.subscribe(
  GOOGLE_RECAPTCHA_SITE_KEY,
  TENANT_ID,
  APP_NAME,
  subscriberEmail,
  "",
  "",
  "",
  NOTIFICATION_INTEGRATION_ENDPOINT
);
```

Pass empty strings for first name, last name, and tracking ID because they are not collected.

UI requirements:
- validate email before submit
- show spinner/loading state
- disable submit while request is in progress
- show success after call completes without error
- show user-facing retry message on failure

## Contact Page
Route:

```txt
/contact
```

Fields:
- name
- email
- message

Script:

```html
<script src="https://assets.notification.niobium.co.nz/contact-us.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

Call:

```ts
await niobium.notification.contactUs(
  GOOGLE_RECAPTCHA_SITE_KEY,
  TENANT_ID,
  visitorName,
  visitorEmail,
  visitorMessage,
  NOTIFICATION_INTEGRATION_ENDPOINT
);
```

UI requirements:
- validate required fields
- show spinner/loading state
- disable submit while request is in progress
- show success
- show user-facing retry error on failure

## Track Order Page
Route:

```txt
/track-order
```

Display both tracking methods in one form using a toggle or radio selection.

Preferred method:
- email + order number

Alternative method:
- email + first name

Order number is numeric and has no leading zero. First-name matching is case-insensitive.

Script:

```html
<script src="https://assets.notification.niobium.co.nz/track.js?siteKey=PUT-GOOGLE-RECAPTCHA-SITE-KEY-HERE"></script>
```

The namespace and script pairing are correct: script is hosted under `assets.notification.niobium.co.nz`, and the call uses `niobium.store.trackOrder`.

Email + order number:

```ts
const response = await niobium.store.trackOrder(
  GOOGLE_RECAPTCHA_SITE_KEY,
  {
    email: "john@example.com",
    order: 1234567890
  },
  STORE_INTEGRATION_ENDPOINT
);
```

Email + first name:

```ts
const response = await niobium.store.trackOrder(
  GOOGLE_RECAPTCHA_SITE_KEY,
  {
    email: "john@example.com",
    firstName: "john"
  },
  STORE_INTEGRATION_ENDPOINT
);
```

Response shape:

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
  Refunded = 70
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
  Cancelled = 9
}
```

The product name field is `TrackCartItem.name`.

UI requirements:
- validate required fields
- show spinner/loading state
- disable submit while request is in progress
- show user-facing retry error on failure
- on success, display tracking details clearly
- do not describe tracking as message posting

## Order Status Page
Route:

```txt
/order-status
```

Must not call Stripe.js or vendor APIs to retrieve or verify status.

Use only Stripe's `redirect_status` URL query parameter.

Success:
- tell customer the order is being processed
- say order confirmation email will be sent
- say another email will be sent when the order ships
- ask customer to check inbox for order and shipping updates
- do not overstate fulfillment completion

Failure:
- say payment/order could not be completed
- display meaningful URL status reason if present
- otherwise show generic failure message
- link to contact page

Missing/unknown/uncertain:
- do not provide a solid order/payment update
- say status cannot be confirmed from the current page
- ask customer to contact support if help is needed
- link to contact page
