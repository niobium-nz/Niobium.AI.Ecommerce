# Playbook (compact) for create-meta-video-ad

This playbook is a concise sequence of Playwright MCP browser actions. It is written for automation runners and assumes earlier checks (auth, English UI) are true.

High-level steps

1. Navigate to Ads Manager root: https://www.facebook.com/adsmanager
2. Snapshot UI and verify active account ID visible in account selector equals input.ad_account_id.
   - If mismatch: open account selector and choose exact ID. If exact ID option not found, stop (switch_ad_account).
3. Locate campaign list. Find a row with exact visible campaign_name. If none or multiple exact rows, stop (match_campaign).
4. Click campaign row to expand/manage; within campaign, find ad set with exact visible ad_set_name. If none or multiple, stop (match_ad_set).
5. Start ad creation flow in that ad set (New Ad > Create Ad). Ensure flow is ad-level only.
6. Inspect the creative area for any Meta pre-populated media.
   - If any image, video, carousel card, or other media is already attached, clear all ad creative first.
   - Re-snapshot and confirm the creative area is empty/cleared before add the requested video to this new ad. If the pre-populated creative cannot be removed safely, stop (clear_prepopulated_creative).
7. In creative section, choose Video > Use URL (or equivalent). Fill `video_url` into the URL input.
   - Before confirming import, fill the video Title/Name field with a deterministic asset name derived from `ad_name` when provided; otherwise derive it deterministically from the final path segment of `video_url`. Do not leave the imported video unnamed.
8. Re-snapshot and wait for video preview/selected-video state. Confirm the resolved imported asset appears with that deterministic title/name and that the title/name can be used as the evidence for selecting the correct asset. If the asset is unnamed, duplicated ambiguously, or the deterministic name is not visible, stop (verify_video_asset).
9. Fill ad-level text fields exactly: primary_text, headline, description, destination_url, display_link, call_to_action, pixel_id (where requested), etc. If a required field is missing from inputs, stop (fill_required_fields).
10. If an ad On/Off toggle exists, set to Off/stopped before publish if allowed.
11. Publish the ad.
12. After publishing, re-snapshot campaign/ad set's ad list. Find the newly created ad by name. Verify status is not Active. If Active, toggle only that ad to Stopped and re-verify.
13. Return structured JSON result.

Retries and evidence

- For transient waits (loading spinners), wait up to reasonable timeouts with 2 bounded retries. Re-snapshot before each retry.
- On any failure, take a screenshot and include the failure step in the output. Do not attempt unsafe recovery.
