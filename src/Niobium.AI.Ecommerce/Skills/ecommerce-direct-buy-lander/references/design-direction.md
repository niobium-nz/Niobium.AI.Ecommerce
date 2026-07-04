# Design Direction And Anti-Generic Heuristics

## Import These Ideas

### 1. Choose A Direction Before Coding
Do not start by arranging generic sections. First decide the page's aesthetic character.

For this page type, common high-fit directions are:
- warm editorial-natural DTC
- premium cozy utility
- tactile proof-led minimalism
- soft asymmetry with strong offer cards

Pick one that fits the most to the target audience and product selling, then commit.

### 2. Design A System, Not A Template
Before coding, decide:
- heading scale and rhythm
- body density
- surface palette
- CTA treatment
- card treatment
- proof treatment
- motion restraint

### 3. Customize Shadcn Components Aggressively
Use shadcn as a component primitive source, not as the finished look.

Customize at minimum:
- Button
- Card
- Accordion
- Badge
- Radio Group
- Separator
- any sticky buy bar shell

Customization should touch:
- radius
- spacing
- border tone
- shadow softness
- typography weight
- hover and active states
- selection states

## Explicit Rejections
Do not use these patterns:
- generic newsletter or waitlist capture; the required vendor-backed email subscription form near the footer is allowed and should be visually quiet
- dual-CTA hero structure

## Anti-Generic Rules

### Avoid These Patterns
- perfectly centered, symmetric layouts for every section
- default yellow stars with no brand integration
- repeated three-column feature grids with identical cards
- generic SaaS gradients unrelated to the brand
- untouched shadcn defaults
- placeholder stock photography tone
- giant walls of copy above the first CTA

### Prefer These Patterns
- asymmetry or scale contrast where useful
- section-to-section variation in composition
- authentic product proof and tactile detail
- offer cards that feel designed for this product, not copied from a pricing table
- custom star, badge, and chip styling that uses the brand palette
- distinctive hierarchy created with system-font discipline when custom fonts are unavailable

## Making System Fonts Feel Intentional
If the project uses system fonts only, create personality through:
- heavier headline weights
- tighter tracking on headings
- short uppercase or small-caps labels
- tighter line-height for display text
- strong contrast between display and body sizes
- distinctive section composition
- custom SVG icons and ratings treatment

## Composition Guidance
- let the hero feel slightly editorial, not marketplace-generic
- give the main proof asset generous visual weight
- place the offer selector early and make the recommended bundle obvious
- break up long pages with surface changes and card groupings
- vary section density so the page has pace

## Color Guidance
- keep one dominant brand tone for CTA and emphasis
- reserve the accent color for offer emphasis and select proof chips
- avoid introducing bright alarm colors unless tied to a truthful warning or error state
- keep backgrounds warm and calm so product proof stays legible

## Motion Guidance
- use only subtle motion that helps clarity
- prefer CSS transforms and opacity
- keep sticky bar reveals smooth and low-jank
- avoid motion that competes with the CTA

## Practical Test
Before finalizing, ask:
- does this still look like a real brand if the logo is hidden?
- does the hero feel like the next frame after the ad click?
- do the offer cards look custom to this product and economics?
- would this still feel designed if all animation were removed?
- does the page avoid the commodity-template feel?
