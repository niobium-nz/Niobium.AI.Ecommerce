# Paired video prompts

Write one self-contained prose video-generation prompt per draft. Do not generate a video, provide a video URI, write a screenplay, create a shot-list document, or run a media tool.

Include these elements naturally in each prompt:

1. State the internal fictional-customer-voice purpose and require `FICTIONAL SIMULATION - INTERNAL USE ONLY` to remain clearly visible throughout the clip.
2. Describe a generic fictional subject appropriate to the supplied segment and an everyday scene that does not add product claims. Do not invent real customer identities or impose unprovided sensitive characteristics.
3. Express the paired review's viewpoint and intended emotional emphasis. Include only supported product use and behavior. Do not introduce a promise absent from the review or input.
4. Explain camera style, lighting/environment, emotional tone, and a simple progression of action. Use a natural beginning, supported use moment, and end state rather than numbered screenplay scenes. Do not force a long draft into an implausibly short voiceover.
5. Require the actual product reference supplied to the downstream video model. Preserve the reference product's proportions, materials, colors, controls, markings, and configuration. Do not claim this skill has seen that image. Do not turn several board views into several products.
6. Forbid invented accessories, packaging, transformations, logos, testimonial badges, expert endorsement, extra functionality, and ungrounded performance claims. If the actual reference is missing or conflicts with the described use, require the downstream stage to stop for correction.

The disclosure is a display instruction inside `video_prompt`, not a prefix in `review_text`. Avoid typographic variation in the notice. A prompt requests visible disclosure; it cannot guarantee that a later video tool renders it correctly. The producing stage must verify that separately.

No reference URL/path field is added to this skill's input. The actual image remains a downstream requirement. Do not fetch images, inspect them, or invent their appearance during simulation.
