import { readFile, readdir } from "node:fs/promises";
import { extname, relative, resolve } from "node:path";
import { pathToFileURL } from "node:url";

const FORBIDDEN_RENDERED_PATTERNS = [
  ["em dash", /\u2014/],
  ["owner-facing checkout phrase", /\ba focused,?\s+guest checkout\b/i],
  ["guest-checkout meta copy", /\bguest checkout\b/i],
  ["conversion meta copy", /\bconversion[- ]focused\b|\bconversion rate\b/i],
  ["friction meta copy", /\blow[- ]friction\b|\breduce friction\b/i],
  ["operator offer terminology", /\boffer stack\b|\bmessage match(?:ed)?\b|\bpurchase flow\b/i],
  ["owner/operator terminology", /\bwebsite owner\b|\bsite owner\b|\bbusiness operator\b/i],
  ["ambiguous coupon label", /\bactive coupon\b/i],
];

async function collectHtml(root) {
  const result = [];
  async function walk(current) {
    for (const entry of await readdir(current, { withFileTypes: true })) {
      const fullPath = resolve(current, entry.name);
      if (entry.isDirectory()) await walk(fullPath);
      else if (extname(entry.name).toLowerCase() === ".html") result.push(fullPath);
    }
  }
  await walk(root);
  return result;
}

export function auditRenderedHtml(text, fileName = "HTML") {
  const defects = [];
  for (const [label, pattern] of FORBIDDEN_RENDERED_PATTERNS) {
    if (pattern.test(text)) defects.push(`${fileName} contains ${label}`);
  }
  return defects;
}

export function routeForHtml(outDir, filePath) {
  const rel = relative(outDir, filePath).replaceAll("\\", "/");
  if (rel === "index.html") return "/";
  return `/${rel.replace(/\/index\.html$/i, "").replace(/\.html$/i, "")}`;
}

export async function checkCustomerFacingCopy({ outDir = resolve(process.cwd(), "out") } = {}) {
  const htmlFiles = await collectHtml(outDir);
  if (htmlFiles.length === 0) throw new Error(`No static HTML files found under ${outDir}; run npm run build first`);

  const defects = [];
  for (const filePath of htmlFiles) {
    const text = await readFile(filePath, "utf8");
    const route = routeForHtml(outDir, filePath);
    defects.push(...auditRenderedHtml(text, filePath));
    if (route !== "/") {
      const hasHomeMarker = /data-home-link=["']true["']/i.test(text);
      const hasHomeHref = /<a\b[^>]*href=["']\/["'][^>]*data-home-link=["']true["']|<a\b[^>]*data-home-link=["']true["'][^>]*href=["']\/["']/i.test(text);
      if (!hasHomeMarker || !hasHomeHref) defects.push(`${route} is missing a visible project-local home link`);
    }
    if (/data-coupon-applied=["']true["']/i.test(text) && !/Coupon applied to this order/i.test(text)) {
      defects.push(`${route} has an applied-coupon marker without the exact clear label`);
    }
  }

  const homePath = resolve(outDir, "index.html");
  const home = await readFile(homePath, "utf8");
  if (!/data-testimonials=["']true["']/i.test(home)) defects.push("home page is missing data-testimonials=\"true\"");
  const testimonialCount = [...home.matchAll(/data-testimonial=["']true["']/gi)].length;
  if (testimonialCount < 3) defects.push(`home page renders only ${testimonialCount} testimonial(s); at least 3 are required`);

  if (defects.length > 0) throw new Error(`Customer-facing copy check failed:\n- ${defects.join("\n- ")}`);
  return { htmlFiles: htmlFiles.length, testimonialCount };
}

async function main() {
  const result = await checkCustomerFacingCopy();
  process.stdout.write(`Customer-facing copy check passed across ${result.htmlFiles} HTML files with ${result.testimonialCount} testimonials.\n`);
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
  main().catch((error) => {
    process.stderr.write(`${error instanceof Error ? error.stack : String(error)}\n`);
    process.exitCode = 1;
  });
}
