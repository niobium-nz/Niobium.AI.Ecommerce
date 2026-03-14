# TASK:
Create an 8-second vertical (9:16) single-scene short-form video concept for organic social (Facebook Reels + Instagram Reels) for a small local business. You will NOT generate the video yourself. You will do lightweight audience reasoning + creative planning, then output:
1) ONE single-line Sora2 generation prompt (copy/paste-ready) that includes an explicit NEGATIVE PROMPT clause
2) A structured JSON “subtitle plan” (for adding subtitles later in editing software; NOT rendered in-video by Sora)
3) Optional social post caption + hashtags (no comment hook required)

# REQUIRED INPUTS
You will receive:
- BUSINESS INFO
- PREVIOUS_VIDEO_IDEAS: array of strings

Definition of PREVIOUS_VIDEO_IDEAS:
- An array of all prior video idea summaries ever used for this business/account.
- Each string may be short, messy, or partially specified.
- Treat this array as a hard anti-repetition constraint.
- If the array is empty, proceed normally.

# PRIMARY GOAL
Earn follows because the video is instantly funny/odd/unusual and self-contained. It must feel like content, not an ad.

# Reasoning Framework:
Use **moderate depth** internal reasoning:
- Briefly infer audience preferences.
- Parse PREVIOUS_VIDEO_IDEAS into internal “idea fingerprints” using:
  - business anchor
  - hero object
  - action verb
  - surreal device
  - punchline image
  - location/staging
- Generate 6 candidate gags internally with *mandatory novelty diversity*.
- Reject any candidate that is too similar to a prior idea.
- Score remaining candidates against readability/reliability rules.
- Select the single best concept and produce the final outputs.
Do **not** reveal your internal candidate list, fingerprints, or scoring notes beyond the required fields.

# Input Handling:
Interpret the BUSINESS INFO section as grounding constraints:
- Treat business name and spend range as realism hints only (never shown on-screen).
- Use business type + products sold as the primary “business anchor” source.
- Use location/area vibe only for set dressing (no full address, no readable signage/text).

Interpret PREVIOUS_VIDEO_IDEAS as a novelty blacklist:
- Use semantic matching, not exact wording, when checking similarity.
- Assume synonyms, paraphrases, related objects, and cosmetic rewrites may still be “similar.”
- Do NOT repeat the same gag engine with only superficial swaps (different product flavor, animal type, prop color, camera angle, or nearby setting).
- If a previous idea is vague, infer its most likely core gag and avoid nearby variants.

# Output Requirement:
- Output concept idea as part of the required structured outputs.
- Concept idea should only include subject, scene, action and reason of funny/odd/unusual.
- Concept idea should not include video shot details or plan.
- Do NOT output the internal candidate list, fingerprint analysis, rejected ideas, or scoring notes.
- Do NOT output the PREVIOUS_VIDEO_IDEAS array.

# CORE CREATIVE RULE (NON-NEGOTIABLE)
This must be a “thumbnail joke + one clean action”:
- Self-explanatory as a SINGLE IMAGE + a SINGLE PHYSICAL ACTION.
- The viewer should get the premise from the FIRST FRAME (0.0s) with no reading required.
- No “setup → twist → punchline” scaffolding. No multi-beat story. Just one odd premise + one action.

# BUSINESS-ADJACENT SURREALISM RULE (NON-NEGOTIABLE)
Your ideas must be related to the business, but **must not be trapped in literal business scenes**.
- Minimum relevance requirement (“Business Anchor”): the FIRST FRAME must clearly show at least ONE of:
  1) a product sold, OR
  2) a core material/ingredient used, OR
  3) a tool/equipment associated with the business, OR
  4) the service outcome (visually obvious result).
- Maximum novelty requirement (“Surreal Twist”): the FIRST FRAME must also include at least ONE:
  A) anthropomorphic or unexpected performer (e.g., object/animal/creature/miniature human-like figure), OR
  B) impossible physics (gravity reversal, self-moving object), OR
  C) extreme scale mismatch (tiny hero object or comically oversized), OR
  D) magical transformation (object morphs into the outcome).
- The twist must be instantly readable and must not require text to understand.
- The location can be business-adjacent (workbench, prep table, back room, supply shelf, studio corner) rather than the stereotypical customer-facing scene.

# HARD CONSTRAINTS
1) Final video length: exactly 8.0 seconds.
2) Aspect ratio: 9:16 vertical.
3) Single scene / single continuous shot (no cuts). One location only.
4) Max 2 characters on screen. Prefer 1 character if possible.
5) “Not an ad” rule:
   - No direct selling language. No CTAs to buy/visit/order.
   - No prices, discounts, deals, “best in town,” etc.
   - No brand logos; keep it generic to the business type.
6) NO TEXT-IN-WORLD (MANDATORY):
   - Do NOT include readable text anywhere in the environment or on props (no menus, receipts, stamps, labels, signs, packaging text).
   - If unavoidable, explicitly instruct heavy blur/blank surfaces so text is not readable.
7) Subtitles:
   - Sora MUST NOT render subtitles or any on-screen overlay text.
   - You MUST output a separate structured “subtitle plan” JSON for later editing.
8) Instant comprehension:
   - The FIRST FRAME must already show the odd/funny premise clearly (hero object centered, unobstructed).
   - No backstory, no ambiguity, no symbolism, no slow reveals.
9) Style:
   - Phone-shot skit style: handheld smartphone look, slight natural shake, bright flat indoor lighting (fluorescent), ordinary casual realism.
   - Surreal elements must still look like they were captured on a phone (practical/VFX-in-camera vibe), not cinematic.
   - No cinematic look: no dramatic lighting, no slow-motion, no lens flare, no glossy “commercial” hero shots of products.

# RELIABILITY GUIDELINES (OPTIMIZE FOR SORA2)
- Use ONE “hero object” that is visually obvious at t=0 (comically oversized/tiny OR physically impossible-but-clear).
- The joke must still land if the actor’s expression is only “okay.” (Object/premise does the heavy lifting.)
- Single action must be one verb (e.g., SLAMS / SQUEEZES / POURS / BITES / OPENS / LIFTS).
- End with a held final pose for 1.5–2.0 seconds so viewers can process.
- Keep props minimal: only the hero object + essential surface/tools; no clutter.

# NOVELTY FILTER AGAINST PREVIOUS VIDEO IDEAS (NON-NEGOTIABLE)
Before selecting a final concept, compare each candidate against PREVIOUS_VIDEO_IDEAS using its internal idea fingerprint.

A candidate is TOO SIMILAR and must be rejected if ANY of the following are true:
1) Same or near-same hero object/product family + same action verb.
2) Same business anchor + same surreal device + same punchline image/result.
3) Same gag engine with only cosmetic substitutions:
   - different flavor/color/size of the same item
   - different animal/creature performing the same joke
   - same joke moved to a slightly different corner of the business
   - same action but with a near-equivalent tool/material/product
4) Viewer would describe both ideas with essentially the same one-sentence summary.
5) The new idea would feel like a sequel, remix, or reskin of a prior post rather than a fresh concept.

Novelty preference rules:
- Change at least TWO major axes from prior ideas whenever possible:
  - hero object
  - action verb
  - surreal device
  - business anchor
  - location/staging
  - punchline image
- Prioritize unused surreal devices before reusing one.
- Prioritize unused action verbs before reusing one.
- If previous ideas heavily used products, consider tools/materials/service outcomes instead.
- If previous ideas were mostly anthropomorphic, pivot to physics/scale/transformation, or vice versa.

# NOVELTY DIVERSITY REQUIREMENT (INTERNAL ONLY)
When generating 6 candidate gags internally (do NOT output them), enforce diversity:
- At least 2 candidates must use an anthropomorphic/unexpected performer.
- At least 2 candidates must use impossible physics or scale mismatch.
- At least 1 candidate must be “business-adjacent but not inside the obvious storefront scene.”
- At least 4 of the 6 candidates must be materially distinct from the dominant patterns found in PREVIOUS_VIDEO_IDEAS.

Then score and pick the best concept as usual.

# WORKFLOW (MUST DO IN ORDER)

## SECTION 1 — Audience Reasoning (brief)
- List 3 plausible audience segments for this business type.
- Randomly choose ONE segment.
- In 2–3 bullets, state what that segment tends to enjoy in short-form humor.
- Add 1 bullet: what level of weirdness this segment will still find “pleasant” (cute/chaotic/deadpan) while staying on-brand.

## SECTION 2 — Concept Selection (thumbnail test)
- Generate 6 candidate gag ideas (do NOT output them). Must follow the NOVELTY DIVERSITY REQUIREMENT.
- Reject candidates that violate the NOVELTY FILTER AGAINST PREVIOUS VIDEO IDEAS.
- Score remaining candidates internally using this checklist:
  - “Readable at 0.0s as an image?”
  - “One hero object?”
  - “One verb action?”
  - “No text required?”
  - “Non-ad vibe?”
  - “Has BOTH: Business Anchor + Surreal Twist in first frame?”
  - “Clearly distinct from prior ideas?”
- Pick the single best idea and proceed.
- Output:
  - HERO OBJECT (one short phrase)
  - SINGLE ACTION VERB (one word)
  - FIRST FRAME EXPLANATION (one sentence: what viewer instantly understands)
  - WHY IT’S FUNNY (one sentence)

## SECTION 3 — Director Notes (single-scene choreography)
- Provide a simple beat plan for the ONE continuous shot:
  - 0.0–1.0s: first-frame clarity (hero object + surreal twist already visible)
  - 1.0–6.5s: single action happens clearly
  - 6.5–8.0s: hold final pose/reaction
- Camera: framing + stability (static handheld; no pans; no reframing)
- Environment: single location that is business-adjacent; minimal clutter; no readable text anywhere
- Audio guidance (generic/original only): ambient sound matching the location + simple comedic SFX + optional light beat (no copyrighted music)

## SECTION 4 — Sora2 Generation Prompt (ONE LINE ONLY, no formatting)
- Output ONE single line prompt for Sora2 that includes:
  - Duration 8.0 seconds, 9:16 vertical
  - Single continuous shot, one location, max 2 characters
  - Phone-shot skit style, bright flat indoor lighting
  - Clear description of hero object visible at 0.0s AND the surreal twist visible at 0.0s
  - The one action verb (only one action)
  - End-pose hold (last 1.5–2.0s)
  - Explicit: “NO text-in-world” + “NO on-screen subtitles/overlays” + “no logos/prices/sales language”
  - Anti-cinematic constraints (no slow-mo, no dramatic lighting, no commercial hero shots)
  - Audio direction (generic)
  - The concept must be clearly different from all prior ideas in PREVIOUS_VIDEO_IDEAS
  - MUST INCLUDE an explicit NEGATIVE PROMPT clause within the same single line, using this pattern:
    “NEGATIVE PROMPT: …”
    The negative prompt must ban at minimum:
      - extra people/customers, crowds, children
      - any readable letters/words/numbers/symbols/pseudo-text anywhere (signage, menus, packaging)
      - camera cuts, zooms, pans, reframing, cinematic camera moves
      - slow motion, lens flares, dramatic lighting, glossy commercial product shots
      - additional props beyond the hero object and essential counter/work items
      - multiple actions / multi-beat story / backstory / ambiguity
      - brand logos, prices, discounts, explicit advertising language
      - copyrighted or recognizable characters/mascots

## SECTION 5 — Subtitle Plan JSON (for editing later; NOT rendered by Sora)
- Output a structured JSON object describing subtitles for this 8.0-second video:
  - Keep it minimal (1–3 short subtitle moments total).
  - Each moment includes:
    - start (float seconds)
    - end (float seconds)
    - text (array of 1–2 short lines, each 2–6 words)
    - emphasis (optional: one word to emphasize)
  - Include global style guidance:
    - font_size_pt (INTEGER, point units, e.g., 52)
    - color_rgb (HEX RGB string like “#FFFFFF”)
    - outline_rgb (HEX RGB string like “#000000”)
    - outline_width_pt (INTEGER points, e.g., 4)
    - safe_area (“bottom center, avoid covering faces and hero object”)
  - IMPORTANT FORMATTING REQUIREMENT:
    - Any color value anywhere in the final JSON output MUST be expressed as an RGB hex code string “#RRGGBB”.
    - Font sizes MUST be in point units (pt), not percentages.

## SECTION 6 — Social Post Copy + Hashtags (optional, no comment hook)
- 1 caption (1 short paragraph) that frames the gag; no CTA to buy/order.
- 12–18 hashtags, grouped:
  (a) broad (reach)
  (b) niche (humor + category moments)
  (c) local (city/suburb/area from BUSINESS INFO)
- Do NOT claim verification or audience sizes.

# Safety Constraints:
- Broad-audience safe: no hate/harassment, no adult content, no self-harm, no dangerous stunts, no gore, no illegal instruction.
- No copyrighted characters, recognizable branded mascots, or trademarked looks.
- No medical/legal/financial claims.
- No direct advertising language (no “buy”, “order”, “visit”, prices, discounts, “best in town”).

# Failure Mode:
If business info is missing or vague:
- Infer a generic anchor from “Business type” (e.g., tool/material/service outcome).
- Choose a universally readable hero object (oversized version of a product/tool/material).
- Keep the action extremely literal (pour/squeeze/open/lift) while the premise carries the weirdness (anthropomorphic/physics/scale).

If PREVIOUS_VIDEO_IDEAS is long, messy, or repetitive:
- Extract the most likely recurring gag patterns internally.
- Avoid both exact repeats and nearby variants of those patterns.
- Prefer a concept that changes the surreal device and action verb first.

If constraints conflict (e.g., product requires text/branding to be recognizable):
- Replace with a generic unbranded equivalent and explicitly instruct blank/blurred labels.

If novelty space is constrained by many prior ideas:
- Favor less obvious business anchors (tool/material/service outcome instead of flagship product).
- Favor a new surreal mechanism not already represented in prior ideas.
- Keep the concept simple and readable even if the anchor becomes more indirect.