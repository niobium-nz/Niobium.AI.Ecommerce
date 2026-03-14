# Playbook (compact) for create-meta-video-ad

This playbook is a concise sequence of Playwright MCP browser actions. It is written for automation runners and assumes earlier checks (auth, English UI) are true.

Checkpoint rule

- After each successful numbered step below, immediately record a compact checkpoint and drop the large UI snapshot from active reasoning.
- Checkpoint format:
  - `step`: current numbered step
  - `status`: `ok`
  - `facts`: only durable facts needed later
  - `next`: next numbered step
- Never carry forward raw HTML, full accessibility dumps, or long lists of visible elements once the checkpoint is written.
- If a later step fails, keep only the latest checkpoint, the latest failure evidence, and the exact failed step.

High-level steps

1. Navigate to Ads Manager root: https://www.facebook.com/adsmanager
   - Checkpoint only that Ads Manager loaded and note the next step.
2. Snapshot only the account selector area and verify active account ID visible in account selector equals input.ad_account_id.
   - If mismatch: open account selector and choose exact ID. If exact ID option not found, stop (switch_ad_account).
   - Checkpoint matched account id only.
3. Locate campaign list. Read only enough of the campaign table to find a row with exact visible campaign_name. If none or multiple exact rows, stop (match_campaign).
   - Checkpoint matched campaign name only.
4. Click campaign row to expand/manage; within campaign, read only enough to find ad set with exact visible ad_set_name. If none or multiple, stop (match_ad_set).
   - Checkpoint matched ad set name only.
5. Start ad creation flow in that ad set (New Ad > Create Ad). Ensure flow is ad-level only.
   - Checkpoint that ad-level creation flow is open.
6. Inspect only the creative area for any Meta pre-populated media.
   - If any image, video, carousel card, or other media is already attached, clear all ad creative first.
   - Re-snapshot and confirm the creative area is empty/cleared before add the requested video to this new ad. If the pre-populated creative cannot be removed safely, stop (clear_prepopulated_creative).
   - Checkpoint whether creative area is confirmed empty.
7. In creative section, choose Video > Use URL (or equivalent). Fill `video_url` into the URL input.
   - Before confirming import, fill the video Title/Name field with a deterministic asset name derived from `ad_name` when provided; otherwise derive it deterministically from the final path segment of `video_url`. Do not leave the imported video unnamed.
   - Checkpoint the deterministic asset name and that the import was submitted.
8. Re-snapshot only the selected-video state and wait for video preview. Confirm the resolved imported asset appears with that deterministic title/name and that the title/name can be used as the evidence for selecting the correct asset. If the asset is unnamed, duplicated ambiguously, or the deterministic name is not visible, stop (verify_video_asset).
   - Checkpoint verified asset name only.
9. Fill ad-level text fields exactly: primary_text, headline, description, destination_url, display_link, call_to_action, pixel_id (where requested), etc. If a required field is missing from inputs, stop (fill_required_fields).
   - Checkpoint which fields were successfully filled; do not retain the whole form snapshot.
10. If an ad On/Off toggle exists, set to Off/stopped before publish if allowed.
   - Checkpoint the intended pre-publish status.
11. Publish the ad.
   - Checkpoint publish confirmation evidence and created ad name.
12. After publishing, re-snapshot only campaign/ad set's ad list region. Find the newly created ad by name. Verify status is not Active. If Active, toggle only that ad to Stopped and re-verify. 
   - Treat as success in case if post-publish delivery state is shown as "Processing".
   - Checkpoint the final verified status only.
13. Return structured JSON result based on checkpoints and final evidence.

Retries and evidence

- For transient waits (loading spinners), wait up to reasonable timeouts with 2 bounded retries. Re-snapshot before each retry.
- On retry, inspect only the blocked region instead of refreshing a full-page mental model unless navigation reset is required.
- On any failure, take a screenshot and include the failure step in the output. Do not attempt unsafe recovery.
