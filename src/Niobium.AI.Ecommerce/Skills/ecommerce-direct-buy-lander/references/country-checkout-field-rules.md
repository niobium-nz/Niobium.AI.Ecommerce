# Country Checkout Field Rules

## Scope
Generated ecommerce landing page apps support these `TARGET_COUNTRY` values only:

```txt
US
UK
CA
AU
SG
NZ
IE
```

Each generated site targets exactly one country. The checkout page must never ask the customer to select a country.

## Universal Checkout Field Rules
Apply these rules for every supported country:

- Customer email is mandatory.
- Customer first name is mandatory.
- Customer last name is optional.
- Phone is always visible and always optional.
- Street address / delivery address line 1 is mandatory.
- Address line 2 / apartment / unit is optional where applicable.
- Country is not shown.
- Validation must be lightweight. Do not block customers unnecessarily.
- Prefer validating presence and broad shape over strict address normalization.
- Use clear optional/required labels.
- Keep billing fields collapsed by default behind a same-as-shipping choice.
- Billing fields should mirror shipping fields except billing email and billing phone are not collected.
- `billingCountry` must always equal `TARGET_COUNTRY`.

## Supported Country Rules

| TARGET_COUNTRY | Required shipping fields | Optional shipping fields | Labels and conventions | Lightweight validation |
|---|---|---|---|---|
| `US` | firstName, email, shippingAddressLine1, shippingCity, shippingState, shippingPostcode | lastName, phone, shippingAddressLine2, notes | Use `ZIP code`; use `State`; line 2 label can be `Apartment, suite, unit, etc.` | ZIP should accept 5 digits or ZIP+4. State should be present; a dropdown or two-letter state code field is acceptable. |
| `UK` | firstName, email, shippingAddressLine1, shippingCity, shippingPostcode | lastName, phone, shippingAddressLine2, locality, notes | Use `Postcode`; use `Town or city`; `County` is not required when town and postcode are present. | Postcode should be non-empty and broadly UK-shaped; normalize to uppercase for display. Do not require county. |
| `CA` | firstName, email, shippingAddressLine1, shippingCity, shippingState, shippingPostcode | lastName, phone, shippingAddressLine2, notes | Use `Postal code`; use `Province or territory`; use province abbreviations where practical. | Postal code should accept ANA NAN format with a space, and normalize uppercase. Province/territory is required. |
| `AU` | firstName, email, shippingAddressLine1, shippingCity, shippingState, shippingPostcode | lastName, phone, shippingAddressLine2, notes | Use `Suburb`; use `State/Territory`; use `Postcode`. Map suburb to `shippingCity` unless a separate city field is explicitly needed by the UI. | Postcode should be 4 digits. State/territory is required. |
| `SG` | firstName, email, shippingAddressLine1, shippingPostcode | lastName, phone, shippingAddressLine2, buildingName, notes | Use `Postal code`; Singapore postal code is 6 digits. Unit/floor belongs in address line 2, e.g. `#13-37`. City/locality can be hidden or fixed as `Singapore` and sent as `shippingCity`. | Postal code should be 6 digits. Do not require state/province/region. |
| `NZ` | firstName, email, shippingAddressLine1, shippingCity, shippingPostcode | lastName, phone, shippingAddressLine2, shippingSuburb, notes | Use `Postcode`; use `Town/City`; collect `Suburb` where useful. If suburb is collected, send it as `shippingSuburb`, not duplicated into `shippingAddressLine2`. | Postcode should be 4 digits. Do not require state/region. |
| `IE` | firstName, email, shippingAddressLine1, shippingCity | lastName, phone, shippingAddressLine2, shippingState, shippingPostcode, notes | Use `Eircode`; use `Town/City`; use `County` as optional region field. Eircode helps identify exact address but An Post says it is not always necessary. | If Eircode is entered, validate broad A65 F4E2-like shape and normalize uppercase with a space. Do not require Eircode. |

## Field Mapping Notes

### Name And Consignee
Create `consignee` as:

```txt
firstName
```

when only first name is provided.

Use:

```txt
<firstName> <lastName>
```

when both are provided.

### Address Line 2
Use address line 2 for apartment, suite, unit, floor, building, or delivery instruction details that are part of the address.

Do not force it to be required. Baymard checkout research also supports hiding/collapsing Address Line 2 where practical; for this skill it may remain visible if country conventions make units common, but it must be optional.

### New Zealand Suburb
For New Zealand, when suburb is collected:

```txt
shippingSuburb
```

Do not duplicate suburb into `shippingAddressLine2`.

### Singapore Unit/Floor
For Singapore, floor and apartment may be included in address line 2 in the common `#13-37` style. Do not require region/state.

### Ireland Eircode
Eircode should be optional to avoid blocking customers who do not have it available. If supplied, normalize to uppercase and allow the standard 3-character routing key plus 4-character unique identifier with a space.

## Source Notes

The following sources informed these rules:

- USPS Publication 28, Postal Addressing Standards, describes standardized delivery address line and last line requirements and references ZIP+4/city-state validation: https://pe.usps.com/text/pub28/welcome.htm and https://pe.usps.com/text/pub28/28c2_001.htm
- UK Post Office guidance lists UK address lines as addressee, house number and street, locality if needed, town, full postcode, and says county is not needed when town and postcode are included: https://www.postoffice.co.uk/mail/how-to-address-mail
- Canada Post states civic addresses should contain addressee, civic address, and municipality/province-or-territory/postal-code line; its business guidance says to put city, province, and postal code on the same line and to separate postal code halves with a space: https://www.canadapost-postescanada.ca/cpc/en/support/articles/addressing-guidelines/canadian-addresses.page and https://www.canadapost-postescanada.ca/cpc/en/support/kb/business/address-accuracy/addressing-mail-accurately.page
- Canada Post postal-code guidance defines the six-character `ANA NAN` format: https://www.canadapost-postescanada.ca/cpc/en/support/articles/addressing-guidelines/postal-codes.page
- Australia Post guidance says the final line should contain locality/suburb, state, and postcode, and that postcode is included on the same line as locality/suburb and state: https://auspost.com.au/sending/guidelines/addressing-guidelines
- NZ Post says street addresses consist of unit/building number, street number, street name, suburb, and town; its examples show street number/name, suburb, and town/city plus postcode: https://www.nzpost.co.nz/business/shipping-in-nz/addressing-standards
- UPU's Singapore addressing profile says Singapore postcodes are 6 digits to the right of the locality and includes floor/apartment format `#13-37`: https://www.upu.int/UPU/media/upu/PostalEntitiesFiles/addressingUnit/sgpEn.pdf
- SingPost provides postal-code lookup by building/block/house number and street name: https://www.singpost.com/find-postal-code
- An Post says Irish addresses should include person/organization, house number/name and street/road, rural locality/townland where relevant, Dublin postal district where relevant, and Eircode as the last domestic line; An Post FAQ encourages Eircode but says a full physical address remains necessary: https://www.anpost.com/Post-Parcels/Sending/Correct-Address and https://www.anpost.com/Help-Support/General-FAQ
- Eircode explains the seven-character code and typical format such as `A65 F4E2`: https://www.eircode.ie/what-is-eircode
