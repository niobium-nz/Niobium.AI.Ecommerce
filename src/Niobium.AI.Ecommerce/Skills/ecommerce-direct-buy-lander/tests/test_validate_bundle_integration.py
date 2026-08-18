from __future__ import annotations

import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VALIDATOR = ROOT / "scripts" / "validate_bundle.py"
EXAMPLE_INPUT = ROOT / "references" / "example_input.json"


def write(root: Path, relative: str, content: str = "") -> None:
    path = root / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")


def run_validator(project: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        [sys.executable, str(VALIDATOR), str(project), str(EXAMPLE_INPUT)],
        text=True,
        capture_output=True,
        check=False,
    )


def build_valid_fixture(root: Path) -> None:
    routes = [
        "app/page.tsx",
        "app/checkout/page.tsx",
        "app/contact/page.tsx",
        "app/track-order/page.tsx",
        "app/order-status/page.tsx",
        "app/privacy-policy/page.tsx",
        "app/terms/page.tsx",
        "app/returns-policy/page.tsx",
        "app/shipping-policy/page.tsx",
    ]
    home_source = r'''
import { Testimonials } from '../components/sections/testimonials';
import testimonials from '../config/testimonials.json';
export const copy = `Buy Now /checkout?offer= offer=1 offer=2 offer=3
/contact /track-order /privacy-policy /terms /returns-policy /shipping-policy
content/policies/privacy-policy.md content/policies/terms.md content/policies/returns-policy.md content/policies/shipping-policy.md
StartCheckoutForm OfferSelect InitiatePurchase PurchaseSuccess PurchaseFailed redirect_status
stripe.confirmPayment logo-primary.png transparent carrier_delivery_estimate tracked`;
export const pricingKeys = { default_price: { amount_cents: 2495, currency: 'AUD' } };
export default function Home(){ return <main><h1 className="text-balance" style={{fontSize:'clamp(2rem,7vw,3rem)', textWrap:'balance'}}>Remove pet hair in minutes</h1><Testimonials testimonials={testimonials} /><a data-primary-action="true" href="/checkout?offer=1">Buy Now</a></main>; }
'''
    checkout_source = r'''
import { Elements, PaymentElement, useElements, useStripe } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import { HomeLink } from '../../components/layout/home-link';
const stripePromise = loadStripe('pk_test');
export async function submit(){ const stripe = useStripe(); const elements = useElements(); await elements.submit(); return stripe.confirmPayment({ elements, clientSecret: '', confirmParams: {} }); }
export default function Page(){ return <main><h1 className="text-balance" style={{fontSize:'clamp(2rem,7vw,3rem)', textWrap:'balance'}}>Complete your order</h1><section data-checkout-order-summary="true"><p>Order information</p><div data-checkout-coupon="true"><button data-coupon-toggle="true">Add or change coupon</button><p data-coupon-applied="true">Coupon applied to this order</p></div></section><form data-checkout-shipping-form="true">Shipping</form><section data-checkout-payment="true"><Elements stripe={stripePromise} options={{ mode: 'payment', amount: 2495, currency: 'aud' }}><PaymentElement /></Elements></section><HomeLink /></main>; }
'''
    other_source = r'''
import { HomeLink } from '../../components/layout/home-link';
export default function Page(){ return <main><h1 className="text-balance" style={{fontSize:'clamp(2rem,7vw,3rem)', textWrap:'balance'}}>Customer information</h1><HomeLink /></main>; }
'''

    policy_route_fields = {
        "app/privacy-policy/page.tsx": "privacy_policy",
        "app/terms/page.tsx": "terms",
        "app/returns-policy/page.tsx": "returns_policy",
        "app/shipping-policy/page.tsx": "shipping_policy",
    }

    for index, route in enumerate(routes):
        if index == 0:
            source = home_source
        elif route == "app/checkout/page.tsx":
            source = checkout_source
        elif route in policy_route_fields:
            field = policy_route_fields[route]
            source = f'''
import {{ readPolicySource }} from '../../lib/legal-content';
import {{ HomeLink }} from '../../components/layout/home-link';
export default function Page(){{ const source = readPolicySource('{field}'); return <main><article data-policy-source="{field}">{{source}}</article><HomeLink /></main>; }}
'''
        else:
            source = other_source
        write(root, route, source)

    write(root, "components/layout/home-link.tsx", '''
import Link from 'next/link';
export function HomeLink(){ return <Link data-home-link="true" href="/">Back to home</Link>; }
''')
    write(root, "components/brand/site-logo.tsx", "export const logo = '/assets/logo-primary.png';\n")
    write(root, "components/sections/testimonials.tsx", (ROOT / "templates/components/sections/testimonials.tsx").read_text(encoding="utf-8"))
    write(root, "components/integrations/third-party-scripts.tsx", (ROOT / "templates/components/integrations/third-party-scripts.tsx").read_text(encoding="utf-8"))
    write(root, "lib/legal-content.ts", (ROOT / "templates/lib/legal-content.ts").read_text(encoding="utf-8"))

    write(root, "lib/env.ts", r'''
const SHIPPING_OPTION_ID_PATTERN = /^[1-9]\d*$/;
export function parseShippingOptionId(raw: string): number {
  if (!SHIPPING_OPTION_ID_PATTERN.test(raw)) throw new Error('SHIPPING_OPTION_ID');
  const value = Number(raw);
  if (!Number.isSafeInteger(value) || value <= 0) throw new Error('SHIPPING_OPTION_ID');
  return value;
}
''')
    write(root, "lib/vendor-response.ts", '''
export class VendorResponseError extends Error {}
export type Kind = 'invalid_json' | 'invalid_body';
export async function parseVendorJsonResponse<T>(response: Response): Promise<T> {
  if (!response.ok || response.status < 200 || response.status >= 300) throw new VendorResponseError('http');
  const body: unknown = await response.json();
  if (body === null) throw new VendorResponseError('invalid_body');
  return body as T;
}
export async function callVendorJson<T>(request: () => Promise<Response>): Promise<T> {
  try { return await parseVendorJsonResponse<T>(await request()); }
  catch (error) { if (error instanceof VendorResponseError) throw error; throw new VendorResponseError('invalid_json'); }
}
''')
    write(root, "lib/quote.ts", '''
import { callVendorJson } from './vendor-response';
export const shipping_option_id: number = 101;
export const store_integration_endpoint = STORE_INTEGRATION_ENDPOINT;
export function quote(){ return callVendorJson(() => niobium.store.getQuote('', '', shipping_option_id, '', [], null, store_integration_endpoint)); }
''')
    write(root, "lib/order.ts", '''
import { shipping_option_id, store_integration_endpoint } from './quote';
import { callVendorJson } from './vendor-response';
export function order(){ return callVendorJson(() => niobium.store.makeOrder('', '', { shippingId: shipping_option_id }, store_integration_endpoint)); }
''')
    write(root, "lib/vendor-support.ts", '''
import { callVendorJson } from './vendor-response';
export const store_integration_endpoint = STORE_INTEGRATION_ENDPOINT;
export const notification_integration_endpoint = NOTIFICATION_INTEGRATION_ENDPOINT;
export function track(){ return callVendorJson(() => niobium.store.trackOrder('', { email: 'john@example.com', order: 123 }, store_integration_endpoint)); }
export function subscribe(){ return callVendorJson(() => niobium.notification.subscribe('', '', '', 'john@example.com', '', '', '', notification_integration_endpoint)); }
export function contact(){ return callVendorJson(() => niobium.notification.contactUs('', '', 'John', 'john@example.com', 'Help', notification_integration_endpoint)); }
''')
    write(root, "lib/utils.ts", '''
export function isSafeCentAmount(value: unknown): value is number { return typeof value === 'number' && Number.isSafeInteger(value); }
export function formatMoneyFromCents(amountCents: number | null | undefined, currency: string | null | undefined){
  if (!isSafeCentAmount(amountCents) || amountCents < 0) return 'Price unavailable';
  const normalizedCurrency = currency?.trim().toUpperCase();
  if (!normalizedCurrency || !/^[A-Z]{3}$/.test(normalizedCurrency)) return 'Price unavailable';
  try { return new Intl.NumberFormat('en', { style: 'currency', currency: normalizedCurrency }).format(amountCents / 100); }
  catch { return 'Price unavailable'; }
}
''')
    write(root, "lib/offer-pricing.ts", '''
export const keys = 'default_price.amount_cents quote.total';
export function createImmediateOfferPrice(){ return { amountCents: 2495, currency: 'AUD', source: "default" }; }
export function applyLiveQuotePrice(){ return { amountCents: 2395, currency: 'AUD', source: "quote" }; }
export async function refreshOfferPriceInBackground(){ return applyLiveQuotePrice(); }
''')

    offer_options = [
        {"source_offer_key": "single_unit_offer", "offer_option_key": "1", "default_price": {"amount_cents": 2495, "currency": "AUD"}},
        {"source_offer_key": "best_seller_bundle", "offer_option_key": "2", "default_price": {"amount_cents": 3995, "currency": "AUD"}},
        {"source_offer_key": "higher_aov_bundle", "offer_option_key": "3", "default_price": {"amount_cents": 5495, "currency": "AUD"}},
    ]
    write(root, "config/offer-options.json", json.dumps(offer_options))
    write(root, "config/site-input-summary.json", json.dumps({"brand_system": {"logo_file": "source-assets/logo.svg", "primary_color": "#372010", "secondary_color": "#faf3e0"}}))
    input_data = json.loads(EXAMPLE_INPUT.read_text(encoding="utf-8"))
    write(root, "config/testimonials.json", json.dumps(input_data["trust_signal"]["testimonials"], ensure_ascii=False))
    import hashlib
    policy_map = {
        "privacy_policy": "content/policies/privacy-policy.md",
        "terms": "content/policies/terms.md",
        "returns_policy": "content/policies/returns-policy.md",
        "shipping_policy": "content/policies/shipping-policy.md",
    }
    manifest = {}
    for field, project_path in policy_map.items():
        source_path = EXAMPLE_INPUT.parent / input_data["trust_signal"][field]
        payload = source_path.read_bytes()
        target = root / project_path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(payload)
        manifest[field] = {"project_path": project_path, "sha256": hashlib.sha256(payload).hexdigest()}
    write(root, "config/legal-content-manifest.json", json.dumps(manifest))
    target_logo = root / "source-assets/logo.svg"
    target_logo.parent.mkdir(parents=True, exist_ok=True)
    target_logo.write_bytes((ROOT / "references/assets/logo.svg").read_bytes())

    write(root, "next.config.mjs", '''
import { networkInterfaces } from 'node:os';
const configured = process.env.DEV_ALLOWED_ORIGINS ?? '';
export default { output: 'export', allowedDevOrigins: ['localhost', configured, String(networkInterfaces())], logging: { browserToTerminal: 'warn' } };
''')
    write(root, ".vscode/launch.json", json.dumps({
        "version": "0.2.0",
        "configurations": [{
            "name": "Next.js: debug full stack",
            "type": "node-terminal",
            "request": "launch",
            "command": "npm run dev",
            "serverReadyAction": {
                "pattern": "- Local:.+(https?://.+)",
                "uriFormat": "%s",
                "action": "debugWithChrome",
            },
        }],
    }))


    versions = {
        "next": "16.2.11", "react": "19.2.0", "react-dom": "19.2.0", "typescript": "7.0.2",
        "@types/node": "24.0.0", "@types/react": "19.0.0", "@types/react-dom": "19.0.0",
        "tailwindcss": "4.1.0", "@tailwindcss/postcss": "4.1.0", "postcss": "8.5.0", "eslint": "9.0.0",
        "vitest": "4.0.0", "@vitest/coverage-v8": "4.0.0", "@testing-library/react": "16.0.0",
        "@testing-library/jest-dom": "6.0.0", "@testing-library/user-event": "14.0.0", "jsdom": "27.0.0",
        "@playwright/test": "1.50.0", "serve": "14.2.0", "@stripe/stripe-js": "7.0.0",
        "@stripe/react-stripe-js": "4.0.0", "sharp": "0.34.0", "wrangler": "4.113.0",
    }
    dependencies = {name: f"^{version}" for name, version in versions.items() if name in {"next", "react", "react-dom", "@stripe/stripe-js", "@stripe/react-stripe-js"}}
    dev_dependencies = {name: f"^{version}" for name, version in versions.items() if name not in dependencies}
    package = {
        "scripts": {
            "prepare:app": "node scripts/export-offer-env.mjs && node scripts/prepare-logo-assets.mjs && node scripts/generate-public-env.mjs",
            "dev": "npm run prepare:app && next dev --hostname 0.0.0.0",
            "deps:check": "node scripts/check-dependency-freshness.mjs",
            "deps:health": "node scripts/check-dependency-health.mjs",
            "deps:scripts": "npm approve-scripts --allow-scripts-pending --json",
            "project:boundaries": "node scripts/check-project-boundaries.mjs",
            "test:content": "node scripts/check-customer-facing-copy.mjs",
            "lint": "eslint --max-warnings=0 .", "typecheck": "tsc --noEmit", "test": "vitest run",
            "test:coverage": "vitest run --coverage", "serve:static": "serve out --listen 4173 --no-clipboard",
            "test:e2e": "playwright test", "test:runtime": "node scripts/check-dev-runtime.mjs",
            "quality": "npm run deps:check && npm run deps:health && npm run deps:scripts && npm run project:boundaries && npm run lint && npm run typecheck && npm run test:coverage && npm run build && npm run test:content && npm run test:e2e && npm run test:runtime",
            "build": "npm run prepare:app && next build", "deploy": "node scripts/deploy-cloudflare-pages.mjs",
        },
        "engines": {"node": ">=24.11.0"}, "dependencies": dependencies, "devDependencies": dev_dependencies,
        "allowScripts": {"workerd@1.20260722.1": True},
    }
    write(root, "package.json", json.dumps(package))
    lock_packages = {"": {"dependencies": dependencies, "devDependencies": dev_dependencies}}
    for name, version in versions.items():
        lock_packages[f"node_modules/{name}"] = {"name": name, "version": version}
    lock_packages["node_modules/workerd"] = {"name": "workerd", "version": "1.20260722.1", "hasInstallScript": True}
    write(root, "package-lock.json", json.dumps({"name": "fixture", "lockfileVersion": 3, "requires": True, "packages": lock_packages}))
    write(root, ".npmrc", "strict-allow-scripts=true\n")
    write(root, ".nvmrc", "24\n")
    write(root, ".env.example", "SHIPPING_OPTION_ID=101\n")
    write(root, ".gitignore", "node_modules\n.next\nout\ncoverage\n.vscode/.debug-profile\n")
    write(root, "AGENTS.md", "# Agents\n")
    write(root, "eslint.config.mjs", "export default []\n")
    write(root, "playwright.config.ts", "export default { webServer: { command: 'npm run serve:static' }, use: { baseURL: 'http://127.0.0.1:4173' } };\n")
    write(root, "tsconfig.json", "{}")
    write(root, "vitest.config.mts", "export default { test: { coverage: { provider: 'v8', include: ['app/**', 'components/**', 'lib/**', 'scripts/**'], thresholds: { perFile: true, statements: 100, branches: 100, functions: 100, lines: 100 } } } };\n")

    write(root, "scripts/deploy-cloudflare-pages.mjs", "// deploy\n")
    write(root, "scripts/generate-public-env.mjs", "// public env\n")
    write(root, "scripts/export-offer-env.mjs", "// OFFER_OPTION__1 OFFER_OPTION__2 OFFER_OPTION__3\n")
    write(root, "scripts/prepare-logo-assets.mjs", "// sharp #000 #fff transparent alpha luminance coverage logo-primary.png logo-inverse.png source-assets/logo.svg\n")
    write(root, "scripts/check-dependency-freshness.mjs", "// npm dependencies devDependencies dist-tags.latest latest CARET_STABLE_SEMVER latest stable compatible queryLatestStableWithinRange isCaretCompatible\n")
    write(root, "scripts/check-dependency-health.mjs", "// npm ci --dry-run --strict-allow-scripts approve-scripts --allow-scripts-pending --json allowScripts workerd engines warn npm ls\n")
    write(root, "scripts/check-project-boundaries.mjs", "// findAbsoluteLocalReferences findEscapingRelativeReferences isSymbolicLink file: link: Project boundary check failed\n")
    write(root, "scripts/check-customer-facing-copy.mjs", "// data-testimonials data-testimonials-total data-testimonials-visible data-load-more-testimonials data-testimonial data-home-link Coupon applied to this order active coupon em dash config/testimonials.json legal-content-manifest\n")
    write(root, "scripts/check-dev-runtime.mjs", "// 0.0.0.0 networkInterfaces console pageerror requestfailed warning outdated cross-origin source-map new Response data-home-link waitForURL React DevTools HMR classifyBrowserConsoleMessage external-diagnostic --disable-extensions data-testimonials data-testimonials-total data-load-more-testimonials MOBILE_VIEWPORTS 320 360 390 430 interactiveDefects EXPECTED_INFORMATIONAL_DEV_MESSAGES ObjectMultiplex MaxListenersExceededWarning private-token\n")

    write(root, "tests/e2e/all.spec.ts", '''
// page.on console pageerror requestfailed
// / /checkout /contact /track-order /order-status /privacy-policy /terms /returns-policy /shipping-policy
// formatMoneyFromCents SHIPPING_OPTION_ID shippingId STORE_INTEGRATION_ENDPOINT NOTIFICATION_INTEGRATION_ENDPOINT
// OfferSelect logo-primary.png transparent response.ok response.status response.json VendorResponseError
// 400 429 500 amount_cents 2495 24.95 refreshOfferPriceInBackground data-home-link pathname
// data-testimonials data-testimonial data-load-more-testimonials testimonialCount while toHaveCount testimonials.json testimonial.name testimonial.testimonial Coupon applied to this order 320 360 390 430 horizontal overflow project boundary
// data-checkout-order-summary data-checkout-coupon data-checkout-shipping-form data-checkout-payment boundingBox toBeLessThan
// readPolicySource data-policy-source
// scripts/deploy-cloudflare-pages.mjs scripts/generate-public-env.mjs scripts/export-offer-env.mjs scripts/prepare-logo-assets.mjs scripts/check-dependency-freshness.mjs scripts/check-dependency-health.mjs scripts/check-dev-runtime.mjs scripts/check-project-boundaries.mjs scripts/check-customer-facing-copy.mjs
''')
    policy_test_markers = '\n'.join(f"{entry['project_path']} {entry['sha256']}" for entry in manifest.values())
    with (root / "tests/e2e/all.spec.ts").open("a", encoding="utf-8") as handle:
        handle.write("\n// " + policy_test_markers.replace("\n", "\n// ") + "\n")

    readme_markers = '''
APP_NAME CLOUDFLARE_ACCOUNT_ID CLOUDFLARE_API_TOKEN TENANT_ID GOOGLE_RECAPTCHA_SITE_KEY
STORE_INTEGRATION_ENDPOINT NOTIFICATION_INTEGRATION_ENDPOINT STRIPE_PUBLIC_KEY SHIPPING_OPTION_ID
TARGET_COUNTRY META_PIXEL_ID GOOGLE_TAG CLARITY_ID FACEBOOK_URL INSTAGRAM_URL CONTACT_EMAIL
OFFER_OPTION__1 OFFER_OPTION__2 OFFER_OPTION__3 fbclid utm_source utm_medium utm_campaign utm_content utm_term
carrier_delivery_estimate tracked
'''
    write(root, "README.md", readme_markers)

    test_workflow = '''name: Test\non:\n  push:\n    branches-ignore:\n      - main\n  pull_request:\n    branches-ignore:\n      - main\n  workflow_dispatch:\njobs:\n  test:\n    environment: test\n    steps:\n      - run: npm ci --strict-allow-scripts\n      - run: npx playwright install --with-deps chromium\n      - run: npm run quality\n      - run: npm run deploy\n'''
    prod_workflow = '''name: Prod\non:\n  pull_request:\n    branches: [main]\n  push:\n    branches: [main]\n  workflow_dispatch:\njobs:\n  prod:\n    environment: prod\n    steps:\n      - run: npm ci --strict-allow-scripts\n      - run: npx playwright install --with-deps chromium\n      - run: npm run quality\n      - run: npm run deploy\n'''
    write(root, ".github/workflows/test.yml", test_workflow)
    write(root, ".github/workflows/prod.yml", prod_workflow)

class ValidateBundleIntegrationTests(unittest.TestCase):
    def test_synthetic_complete_project_passes(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            result = run_validator(project)
            self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
            self.assertIn("VALIDATION PASSED", result.stdout)

    def test_unrelated_decorative_svg_reference_is_allowed(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write("\nexport const decorativeIcon = './assets/decorative-icon.svg';\n")
            result = run_validator(project)
            self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_every_vendor_endpoint_must_be_the_final_argument(self) -> None:
        cases = {
            "getQuote": (
                "lib/quote.ts",
                "niobium.store.getQuote('', '', shipping_option_id, '', [], null, store_integration_endpoint)",
                "niobium.store.getQuote('', '', shipping_option_id, '', [], store_integration_endpoint, null)",
            ),
            "makeOrder": (
                "lib/order.ts",
                "niobium.store.makeOrder('', '', { shippingId: shipping_option_id }, store_integration_endpoint)",
                "niobium.store.makeOrder('', '', store_integration_endpoint, { shippingId: shipping_option_id })",
            ),
            "trackOrder": (
                "lib/vendor-support.ts",
                "niobium.store.trackOrder('', { email: 'john@example.com', order: 123 }, store_integration_endpoint)",
                "niobium.store.trackOrder('', store_integration_endpoint, { email: 'john@example.com', order: 123 })",
            ),
            "subscribe": (
                "lib/vendor-support.ts",
                "niobium.notification.subscribe('', '', '', 'john@example.com', '', '', '', notification_integration_endpoint)",
                "niobium.notification.subscribe('', '', '', 'john@example.com', '', '', notification_integration_endpoint, '')",
            ),
            "contactUs": (
                "lib/vendor-support.ts",
                "niobium.notification.contactUs('', '', 'John', 'john@example.com', 'Help', notification_integration_endpoint)",
                "niobium.notification.contactUs('', '', 'John', 'john@example.com', notification_integration_endpoint, 'Help')",
            ),
        }
        for method, (relative, valid_call, invalid_call) in cases.items():
            with self.subTest(method=method), tempfile.TemporaryDirectory() as temp:
                project = Path(temp) / "project"
                project.mkdir()
                build_valid_fixture(project)
                path = project / relative
                source = path.read_text(encoding="utf-8")
                self.assertIn(valid_call, source)
                path.write_text(source.replace(valid_call, invalid_call), encoding="utf-8")
                result = run_validator(project)
                self.assertNotEqual(result.returncode, 0)
                self.assertIn(method, result.stdout)
                self.assertIn("final argument", result.stdout)

    def test_direct_vendor_call_without_response_wrapper_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            quote_file = project / "lib/quote.ts"
            quote_file.write_text(
                quote_file.read_text(encoding="utf-8").replace(
                    "return callVendorJson(() => niobium.store.getQuote",
                    "return niobium.store.getQuote",
                ),
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("callVendorJson", result.stdout)

    def test_missing_http_status_check_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            response_file = project / "lib/vendor-response.ts"
            response_file.write_text(
                response_file.read_text(encoding="utf-8").replace("response.ok", "successFlag"),
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("response.ok", result.stdout)

    def test_major_unit_money_formatter_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write("\nexport const badAmount = formatMoney(2495, 'AUD');\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("formatMoneyFromCents", result.stdout)

    def test_incorrect_full_stack_debug_command_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            launch_path = project / ".vscode/launch.json"
            launch = json.loads(launch_path.read_text(encoding="utf-8"))
            launch["configurations"][0]["command"] = "next dev"
            launch_path.write_text(json.dumps(launch), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("full-stack node-terminal", result.stdout)

    def test_removed_bundle_event_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            removed_event = "Bundle" + "Select"
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write(f"\nexport const removedEvent = {removed_event!r};\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("OfferSelect", result.stdout)

    def test_raw_svg_reference_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write("\nexport const badLogo = './assets/logo.svg';\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("raw SVG", result.stdout)

    def test_exact_dependency_declaration_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            package_path = project / "package.json"
            package = json.loads(package_path.read_text(encoding="utf-8"))
            package["devDependencies"]["wrangler"] = "4.113.0"
            package_path.write_text(json.dumps(package), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("stable caret range", result.stdout)

    def test_test_workflow_feature_branch_condition_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            workflow_path = project / ".github/workflows/test.yml"
            workflow_path.write_text(workflow_path.read_text(encoding="utf-8") + "\n    if: startsWith(github.head_ref, 'feature/')\n", encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("feature-branch", result.stdout)

    def test_absolute_external_asset_path_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            with (project / "tests/e2e/all.spec.ts").open("a", encoding="utf-8") as handle:
                handle.write("\nconst logoPath = '/home/example/input/logo.svg';\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("absolute local filesystem path", result.stdout)

    def test_svg_project_copy_must_match_input_bytes(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            (project / "source-assets/logo.svg").write_text('<svg xmlns="http://www.w3.org/2000/svg"/>', encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("byte-for-byte", result.stdout)

    def test_missing_testimonials_are_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            component_path = project / "components/sections/testimonials.tsx"
            component_path.write_text(component_path.read_text(encoding="utf-8").replace('data-testimonials="true"', 'data-proof="true"'), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("data-testimonials", result.stdout)

    def test_ambiguous_coupon_copy_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            checkout = project / "app/checkout/page.tsx"
            checkout.write_text(checkout.read_text(encoding="utf-8").replace("Coupon applied to this order", "Active coupon"), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("ambiguous_coupon_label", result.stdout)

    def test_literal_em_dash_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write(f"\nexport const badPunctuation = {chr(0x2014)!r};\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("em_dash", result.stdout)

    def test_full_stack_server_ready_action_is_required(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            launch_path = project / ".vscode/launch.json"
            launch = json.loads(launch_path.read_text(encoding="utf-8"))
            launch["configurations"][0].pop("serverReadyAction")
            launch_path.write_text(json.dumps(launch), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("full-stack node-terminal", result.stdout)

    def test_strict_npmrc_and_workerd_approval_are_required(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            (project / ".npmrc").write_text("strict-allow-scripts=false\n", encoding="utf-8")
            package_path = project / "package.json"
            package = json.loads(package_path.read_text(encoding="utf-8"))
            package["allowScripts"].clear()
            package_path.write_text(json.dumps(package), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("strict-allow-scripts=true", result.stdout)
            self.assertIn("workerd", result.stdout)

    def test_prohibited_fulfillment_origin_wording_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            prohibited = "over" + "seas"
            with (project / "app/page.tsx").open("a", encoding="utf-8") as handle:
                handle.write(f"\nexport const badShippingCopy = {prohibited!r};\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("fulfillment_origin_wording", result.stdout)


    def test_reworded_or_missing_testimonial_data_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "config/testimonials.json"
            testimonials = json.loads(path.read_text(encoding="utf-8"))
            testimonials[0]["testimonial"] += " Changed."
            path.write_text(json.dumps(testimonials), encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("preserve every input testimonial", result.stdout)

    def test_home_page_must_pass_complete_imported_testimonial_array(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "app/page.tsx"
            path.write_text(
                path.read_text(encoding="utf-8").replace(
                    "<Testimonials testimonials={testimonials} />",
                    "<Testimonials testimonials={[]} />",
                ),
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("pass the complete imported testimonial array", result.stdout)

    def test_changed_legal_policy_character_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "content/policies/privacy-policy.md"
            path.write_bytes(path.read_bytes() + b" ")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("byte-for-byte", result.stdout)

    def test_policy_route_must_render_its_bound_source(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "app/privacy-policy/page.tsx"
            path.write_text(
                path.read_text(encoding="utf-8").replace(
                    "readPolicySource('privacy_policy')",
                    "'rewritten privacy copy'",
                ),
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("policy route must render exact source", result.stdout)

    def test_custom_script_loader_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "components/integrations/third-party-scripts.tsx"
            path.write_text(path.read_text(encoding="utf-8") + "\nfunction loadExternalScript() {}\n", encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("custom client-side script loaders", result.stdout)

    def test_custom_arrow_script_loader_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "components/integrations/third-party-scripts.tsx"
            path.write_text(path.read_text(encoding="utf-8") + "\nconst injectScript = () => undefined;\n", encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("custom client-side script loaders", result.stdout)

    def test_direct_integration_endpoint_fetch_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "lib/vendor-support.ts"
            path.write_text(
                path.read_text(encoding="utf-8") + "\nexport const badFetch = fetch(process.env.STORE_INTEGRATION_ENDPOINT);\n",
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("do not fetch integration endpoints directly", result.stdout)

    def test_checkout_summary_after_shipping_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "app/checkout/page.tsx"
            source = path.read_text(encoding="utf-8")
            source = source.replace(
                '<section data-checkout-order-summary="true"><p>Order information</p><div data-checkout-coupon="true"><button data-coupon-toggle="true">Add or change coupon</button><p data-coupon-applied="true">Coupon applied to this order</p></div></section><form data-checkout-shipping-form="true">Shipping</form>',
                '<form data-checkout-shipping-form="true">Shipping</form><section data-checkout-order-summary="true"><p>Order information</p><div data-checkout-coupon="true"><button data-coupon-toggle="true">Add or change coupon</button><p data-coupon-applied="true">Coupon applied to this order</p></div></section>',
            )
            path.write_text(source, encoding="utf-8")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("order summary must appear before shipping", result.stdout)

    def test_local_package_script_must_be_in_coverage(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            path = project / "vitest.config.mts"
            path.write_text(
                path.read_text(encoding="utf-8").replace(", 'scripts/**'", "") + "\n// scripts/** is not an include entry\n",
                encoding="utf-8",
            )
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("include must explicitly cover scripts/**", result.stdout)

    def test_new_local_package_script_requires_explicit_tests(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            project = Path(temp) / "project"
            project.mkdir()
            build_valid_fixture(project)
            package_path = project / "package.json"
            package = json.loads(package_path.read_text(encoding="utf-8"))
            package["scripts"]["extra:check"] = "node scripts/extra-check.mjs"
            package_path.write_text(json.dumps(package), encoding="utf-8")
            write(project, "scripts/extra-check.mjs", "export const ok = true;\n")
            result = run_validator(project)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("lacks explicit test coverage reference", result.stdout)



if __name__ == "__main__":
    unittest.main()
