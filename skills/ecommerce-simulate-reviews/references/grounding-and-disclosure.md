# Grounding and disclosure

## Keep the simulation structural

Set `is_fictional` to the boolean `true` on every output object. Never use `false`, a string such as `"true"`, a numeric substitute, null, or an omitted flag. Do not prefix `review_text` with a disclosure string.

Preserve this field through storage, copying, and handoff. A human-facing internal interface must use the flag to display a separate visible notice such as `FICTIONAL SIMULATION - INTERNAL USE ONLY` beside the draft and its scripted rating. Text exported alone needs an accompanying disclosure outside the review body. Do not silently strip the flag or mix these objects with genuine customer feedback.

A machine-readable flag is not visible proof of disclosure in a rendered page or video. Therefore each video prompt must require the same notice to remain visible throughout the resulting clip. Include an internal fictional-purpose instruction in the prompt as well. Do not render reviewer names, purchase identifiers, verified badges, dates of purchase, retailer review widgets, or other authenticity cues.

## What may be fictional

Use generic everyday buyer viewpoints, neutral scene staging, subjective preferences and emotions, and imagined supported product use for internal creative evaluation. Keep those viewpoints recognizably fictional through the record and display contracts. Do not impersonate a real customer, influencer, public figure, professional, or existing reviewer.

A fictional person can appreciate an input-supported feature. A simulated scene can show ordinary use the input explicitly supports. Neither is proof that any real person bought, tested, endorsed, or benefited from the product.

## What must be grounded

Require support for product function, materials, construction, dimensions, compatibility, limitations, promised outcomes, pricing, approved offers, and customer-facing policies. Do not turn forecasts or commercial proposals into facts. Do not add measurements, time-to-result claims, long-term durability, medical/safety claims, delivery events, refund events, warranty outcomes, quantified savings, scarcity, or social proof without appropriate supplied support. When there is no factual basis, omit the assertion rather than soften it into a purported experience.

For ratings 8 and 9, use compatible subjective fit or an already supplied limitation if helpful. Do not manufacture a defect, missing component, delay, failure, or adverse reaction to make the score look realistic.

## Voice variation

Use everyday English, varied openings and syntax, and genuinely different emphases. Minor casing, slang or punctuation variation is for internal voice testing only, not to evade AI detection or disguise public testimonials. Avoid stereotyped accents and demographic caricatures. Keep all changes readable; never corrupt product identifiers, facts, specifications, the disclosure flag, or video instructions.

## Failed safe completion

Return `{"error":"..."}` when asked to pass generated content off as actual customer reviews, remove fictional labeling from its presentation, or imply genuine verification. Do not publish, post, upload into review services, generate testimonial videos, or create ratings summaries. This skill produces an internal JSON planning artifact only.
