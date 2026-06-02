# Input Schema for create-meta-video-ad

This file documents the structured input schema and interpretation rules used by the skill.

Required fields

- ad_account_id (string) — Exact Meta ad account ID to operate in
- campaign_name (string) — Exact visible campaign name
- ad_set_name (string) — Exact visible ad set name
- video_url (string) — Direct URL or Meta-accepted URL for the video asset

Optional fields (ad-level only)

- ad_name (string)
- primary_text (string)
- headline (string)
- description (string)
- call_to_action (string)
- page_id (string)
- page_name (string)
- instagram_account (string)
- destination_url (string)
- display_link (string)
- pixel_id (string)
- url_parameters (string)
- website_event (string)
- additional ad-level fields as explicitly provided

Interpretation rules

- campaign_name and ad_set_name are treated as exact visible matches.
- video_url is the only supported video input source; local file uploads are disallowed.
- If a required field is not present or the UI requires a field not provided, the skill must stop and report the missing field.
- The skill assumes an authenticated browser session and English Ads Manager UI.
