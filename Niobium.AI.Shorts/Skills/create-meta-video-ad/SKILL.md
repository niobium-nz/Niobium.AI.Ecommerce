---
name: create-meta-video-ad
description: Exact-match single-video ad creation inside an existing Meta Ads Manager campaign+ad set using Playwright MCP browser automation. Trigger when the task requires: operating in a specific ad_account_id; exact visible campaign_name and ad_set_name matching; single-video ads provided by a video_url; publishing immediately but ensuring the ad is stopped. Use only ad-level edits; never modify campaigns, ad sets, budgets, audiences, placements, or account configuration.
---

# Create Meta Video Ad

Purpose

- Create exactly one single-video ad inside an existing Meta Ads Manager campaign and ad set using Playwright MCP browser automation.
- Enforce exact matching on ad account, campaign name, and ad set name; publish immediately and ensure the created ad ends in stopped status.

When to use this skill

- Input supplies ad_account_id, campaign_name, ad_set_name, and video_url and expects deterministic, safety-first automation to create a single-video ad.
- Use when the browser session is already authenticated and Ads Manager UI is in English.

Inputs

- Required: ad_account_id, campaign_name, ad_set_name, video_url
- Optional (ad-level only): ad_name, primary_text, headline, description, call_to_action, page_id, page_name, instagram_account, destination_url, display_link, pixel_id, url_parameters, website_event, and other explicitly-provided ad-level fields.

Key operating principles (short)

- Exact-match or stop: match ad_account_id, campaign_name, ad_set_name by exact visible text only. If no exact single match, stop.
- Ad-level only: do not create/rename/edit/duplicate/ archive/modify campaigns or ad sets or any account/billing settings.
- Single-video only: accept only video_url as source; do not upload local files or create multi-asset creatives.
- Deterministic naming: use provided ad_name or derive deterministically from video_url final path segment; never generate random strings.
- Fail loudly with evidence: on ambiguity, missing required inputs, rejected video_url, or unexpected UI, capture screenshot, stop, and return a failure result.

Behavioral checklist (condensed)

1. Navigate to Ads Manager and verify the active ad account equals ad_account_id. If not, switch by exact ID; if switching fails, stop (exact_failure_step: "switch_ad_account").
2. Locate campaign by exact visible campaign_name; if none or multiple, stop ("match_campaign").
3. Within campaign, locate ad set by exact visible ad_set_name; if none or multiple, stop ("match_ad_set").
4. Only create new ad; do not duplicate any existing ad.
5. Begin ad-level creation flow only; do not alter campaign/ad set settings.
6. Provide video_url in the UI flow that accepts URLs. Always fill the video import Title/Name field with a deterministic asset name before confirming import. Verify the URL is accepted and the resolved video asset appears with that deterministic name attached/selected. If rejected, unnamed, ambiguous, or not attached, stop ("verify_video_asset").
7. Fill only provided ad-level fields exactly as supplied. If UI requires a missing field, stop ("fill_required_fields").
8. Prefer setting the ad toggle to stopped during creation if available. Take a screenshot of the review state before publishing.
9. Publish the ad. After publishing, verify the new ad exists and is in stopped status. If active, switch only the new ad to stopped and verify. If stopped cannot be confirmed, return status: "partial" with exact_failure_step: "verify_stopped_status".
10. Use bounded retries (max 2) for transient UI issues; re-snapshot before each retry. On repeated failure, capture evidence and stop.

Reasoning and evidence

- Use stepwise verify-act-wait-confirm cycles for each major action (account switch, campaign match, ad set match, video attach, fill fields, publish, verify stopped).
- On any failure or ambiguity, capture screenshot and UI snapshot and return a failure result referencing the exact failed step.

Tooling and constraints

- Use Playwright MCP browser tools conservatively: navigate, snapshot, wait-for, click, fill/type, select-option, take-screenshot, handle dialogs, manage tabs.
- Do not use browser file uploads, console/network changes, or injected JS to bypass UI flows except as last-resort non-destructive diagnostics.
- Prefer accessible labels and visible text over brittle selectors; avoid coordinates.

Output

- Return exactly one JSON object matching the required schema:
  {"status":"success|partial|failed","ad_name":"string|null","campaign_matched":"string|null","ad_set_matched":"string|null","warnings":["..."],"screenshot_filepath":["..."],"exact_failure_step":"string|null"}

References

- See references/INPUT_SCHEMA.md for the exact input schema and interpretation rules.
- See references/PLAYBOOK.md for a compact step-by-step Playwright action sequence (intended to be loaded by the automation runner only when needed).
