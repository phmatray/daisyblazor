---
name: DaisyBlazor Docs
description: daisyUI components for Blazor. The docs wear the library; every page is printed in the active theme.
colors:
  ground: "oklch(55% 0.18 256)"
  ground-ink: "oklch(98% 0.01 256)"
  reading-surface: "oklch(100% 0 0)"
  surface-shade: "oklch(97% 0.002 264)"
  surface-rule: "oklch(93% 0.004 264)"
  reading-ink: "oklch(22% 0.02 264)"
  code-surface: "oklch(32% 0.02 264)"
  code-ink: "oklch(96% 0.01 264)"
  secondary: "oklch(50% 0.22 312)"
  accent: "oklch(72% 0.17 65)"
  info: "oklch(52% 0.14 233)"
  success: "oklch(52% 0.15 150)"
  warning: "oklch(78% 0.16 75)"
typography:
  display:
    fontFamily: "Fredoka, Manrope, ui-rounded, sans-serif"
    fontSize: "clamp(3rem, 7.5vw, 6rem)"
    fontWeight: 700
    lineHeight: 0.92
    letterSpacing: "-0.03em"
  display-page:
    fontFamily: "Fredoka, Manrope, ui-rounded, sans-serif"
    fontSize: "clamp(2.75rem, 6.5vw, 5.5rem)"
    fontWeight: 700
    lineHeight: 0.92
    letterSpacing: "-0.03em"
  headline:
    fontFamily: "Fredoka, Manrope, ui-rounded, sans-serif"
    fontSize: "clamp(2.4rem, 5vw, 4rem)"
    fontWeight: 700
    lineHeight: 0.95
    letterSpacing: "-0.02em"
  title:
    fontFamily: "Fredoka, Manrope, ui-rounded, sans-serif"
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.1
    letterSpacing: "-0.02em"
  title-sm:
    fontFamily: "Fredoka, Manrope, ui-rounded, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 600
    lineHeight: 1.2
    letterSpacing: "-0.02em"
  body:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1.0625rem"
    fontWeight: 400
    lineHeight: 1.7
    fontFeature: "\"ss01\", \"cv11\""
  lede:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 400
    lineHeight: 1.625
  label:
    fontFamily: "Manrope, ui-sans-serif, system-ui, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 700
    lineHeight: 1.4
  code:
    fontFamily: "JetBrains Mono, ui-monospace, SFMono-Regular, monospace"
    fontSize: "0.85rem"
    fontWeight: 400
    lineHeight: 1.7
rounded:
  cover: "6px"
  tile: "12px"
  card: "16px"
  palette: "24px"
  pill: "9999px"
spacing:
  gutter-mobile: "16px"
  gutter-tablet: "32px"
  gutter-desktop: "56px"
  section: "80px"
  section-lg: "112px"
  card-pad: "24px"
  card-pad-lg: "32px"
components:
  masthead:
    backgroundColor: "{colors.ground}"
    textColor: "{colors.ground-ink}"
    typography: "{typography.display-page}"
  nav-pill:
    textColor: "{colors.ground-ink}"
    typography: "{typography.label}"
    rounded: "{rounded.pill}"
    padding: "6px 14px"
  nav-pill-active:
    backgroundColor: "{colors.ground-ink}"
    textColor: "{colors.ground}"
    rounded: "{rounded.pill}"
    padding: "6px 14px"
  button-ground-primary:
    backgroundColor: "{colors.ground-ink}"
    textColor: "{colors.ground}"
    rounded: "{rounded.pill}"
    padding: "0 28px"
    height: "48px"
  button-ground-outline:
    textColor: "{colors.ground-ink}"
    rounded: "{rounded.pill}"
    padding: "0 28px"
    height: "48px"
  button-surface-outline:
    textColor: "{colors.reading-ink}"
    rounded: "{rounded.pill}"
    padding: "0 24px"
  search-pill:
    backgroundColor: "{colors.reading-surface}"
    textColor: "{colors.reading-ink}"
    rounded: "{rounded.pill}"
    height: "44px"
  live-cover:
    backgroundColor: "{colors.reading-surface}"
    textColor: "{colors.reading-ink}"
    rounded: "{rounded.cover}"
    padding: "24px"
    width: "22rem"
  theme-swatch:
    backgroundColor: "{colors.ground}"
    rounded: "{rounded.pill}"
    size: "28px"
  jump-pill:
    textColor: "{colors.ground-ink}"
    typography: "{typography.label}"
    rounded: "{rounded.pill}"
    padding: "3px 13px"
  jump-pill-hover:
    backgroundColor: "{colors.ground-ink}"
    textColor: "{colors.ground}"
    rounded: "{rounded.pill}"
    padding: "3px 13px"
  theme-tile:
    backgroundColor: "{colors.ground}"
    textColor: "{colors.ground-ink}"
    rounded: "{rounded.cover}"
    padding: "16px"
  example-card:
    backgroundColor: "{colors.reading-surface}"
    textColor: "{colors.reading-ink}"
    rounded: "{rounded.card}"
    padding: "{spacing.card-pad-lg}"
  code-block:
    backgroundColor: "{colors.code-surface}"
    textColor: "{colors.code-ink}"
    typography: "{typography.code}"
    rounded: "{rounded.card}"
    padding: "16px 20px"
  view-toggle-active:
    backgroundColor: "{colors.reading-ink}"
    textColor: "{colors.reading-surface}"
    rounded: "{rounded.pill}"
    padding: "4px 12px"
  playground-controls:
    backgroundColor: "{colors.surface-shade}"
    textColor: "{colors.reading-ink}"
    padding: "20px"
  search-palette:
    backgroundColor: "{colors.reading-surface}"
    textColor: "{colors.reading-ink}"
    rounded: "{rounded.palette}"
    width: "42rem"
---

# Design System: DaisyBlazor Docs

## Overview

**Creative North Star: "The Flat Theme Poster"**

The docs prove the library by wearing it. Every page opens on a flat field in the active theme's primary, with that theme's primary-content as the ink printed on it; the top bar, the masthead, the home page's closing band and the footer share that field, so the page is framed front and back in the theme. Below the masthead the page turns to base-100 for reading. Switching theme re-colours the whole site with a circular reveal from the point you clicked, which demonstrates in one gesture that every component works in every theme.

The visual world borrows its form from flat 1960s pop print: rounded, confident Fredoka set tight and large, a single saturated ground, simple circles and squares. It borrows form only. Every label, heading and piece of demo data speaks the product's own words (components, themes, parameters, examples), and demo content comes from one neutral SaaS domain. Each page's component runs live on a base-100 square inside the ground, the cover. The next theme peeks off the masthead's right edge as a colour wheel; clicking it switches to that theme. Structure on the reading surface comes from 2px ink rules and pill controls, not from grey panels or shadows.

The world is built entirely from daisyUI semantic colour roles. Every value in the frontmatter is the default "daisyblazor" theme's binding of a role; under any of the 37 themes the same roles re-bind, so the page re-colours itself with no extra rules.

**Key Characteristics:**
- One flat ground per theme: `--color-primary` as surface, `--color-primary-content` as ink, edge to edge.
- base-100 is the only relief for reading; the reading column never takes the ground colour.
- Rounded display type (Fredoka 700) set at 0.92 line-height and -0.03em tracking.
- The live component is the cover of every page; the next theme waits at the edge as a colour wheel.
- 2px ink rules (base-content) frame cards, tables and section breaks.
- Pills for every small control; near-square 6px corners for covers and theme tiles; 12-16px for cards.
- Code takes the theme's neutral with syntax colours mixed from the theme's own roles.
- Labels, headings and demo data use the product's words; demo data is the Acme Cloud SaaS admin.

## Colors

Semantic, theme-bound roles only: one saturated ground, one reading surface, one dark code band, and the theme's secondary, accent and status roles used solely as tints inside code, theme tiles and the colour wheel.

### Primary
- **Ground** (`--color-primary`): the flooded surface of the top bar, page masthead, home closing band, footer, boot screen, search palette header and every theme tile. Also the underline colour for links in prose, list markers, text selection, focus outline, the active guide pill in the side nav, the dot beside the active component link and the hover fill of home wall-tile links.
- **Ground Ink** (`--color-primary-content`): all text, icons, outlines and rules printed on the ground, at full strength for anything small. Inverts to become the fill of the primary button on the ground, the active top-nav pill, the hovered jump pill and the theme switcher pill (with the ground colour as their text).

### Secondary
- **Secondary** (`--color-secondary`): one quarter-arc of the next-theme colour wheel, the circle peeking from the corner of each theme tile and the small dot on theme-switcher swatches; mixed 50% into attribute tokens in code.

### Tertiary
- **Accent** (`--color-accent`): one arc of the colour wheel, string tokens in code (75% mix) and accent parts inside theme tiles.

### Neutral
- **Reading Surface** (`--color-base-100`): the reading page, live cover, example preview, colour-wheel centre, search pill, wall tiles and the theme-tile inset.
- **Surface Shade** (`--color-base-200`): playground controls tray, example-card footer (at 60%), hover fill of pills, and alternating home sections (playground, charts).
- **Surface Rule** (`--color-base-300`): 1px row dividers in tables, API rows and wall tiles; the 2px rail of side-nav sublists and the sticky "On this page" list; unselected enum chips in the playground.
- **Reading Ink** (`--color-base-content`): body text and every 2px structural rule; muted reading text is a base-content opacity (60-85%), never a separate grey.
- **Code Surface / Code Ink** (`--color-neutral`, `--color-neutral-content`): the code block surface, the colour wheel's last arc, and the home page's dark themes band.
- **Info / Success / Warning**: syntax tints only (keywords, numbers, Razor directives), each mixed 75% with neutral-content.

### Named Rules
**The One Ground Rule.** The ground is `--color-primary` and nothing else; its ink is `--color-primary-content`. No second accent ground, no hard-coded hex anywhere in the site layer, so all 37 themes work unchanged. The colour wheel's conic gradient is the one place several roles share a surface, because it previews a theme.

**The Full-Ink Rule.** Small text on the ground is printed at full `--color-primary-content`, including muted labels, breadcrumbs and footer copy. The softened ink (75% primary-content mixed into primary) is reserved for large type only: the "Blazor" half of the logotype.

**The Reading Relief Rule.** Reading happens on base-100. The ground floods mastheads, top bar, closing band and footer; it never floods the reading column.

## Typography

**Display Font:** Fredoka (with Manrope, ui-rounded)
**Body Font:** Manrope (with ui-sans-serif, system-ui)
**Label/Mono Font:** JetBrains Mono (with ui-monospace, SFMono-Regular)
**Icon Font:** Material Symbols Outlined (the `Icons.Material.*` glyphs; icons only, never text)

**Character:** Fredoka's soft, round strokes set tight and huge give the pages a friendly, poster-like voice; Manrope underneath is plain-spoken and sturdy, so the reference material stays calm. Mono carries parameter names, types, paths and code, with ligatures off.

### Hierarchy
- **Display** (700, clamp(3rem, 7.5vw, 6rem), 0.92): the home page H1 in the masthead.
- **Display page** (700, clamp(2.75rem, 6.5vw, 5.5rem), 0.92): every other page's H1, the component or guide name.
- **Headline** (700, clamp(2.4rem, 5vw, 4rem), 0.95): home section titles, stated plainly ("Every component, running live.").
- **Title** (700, 2rem, 1.1): prose H2s and the playground heading.
- **Title small** (600, 1.5rem / 1.35rem in prose, 1.2): example titles, prose H3s, pager targets.
- **Lede** (400, 1.125rem to 1.25rem, relaxed): masthead lede, max 40rem.
- **Body** (400, 1.0625rem, 1.7): the reading column, capped at 70ch; `ss01` and `cv11` features on.
- **Label** (700, 0.875rem): nav pills, jump pills, nav group labels ("Guides", "Components"), table heads. Sentence case, no added tracking.
- **Code** (400, 0.85rem, 1.7): code blocks; inline code at 0.86em on an 8% base-content tint.

### Named Rules
**The Tight Display Rule.** All headings use Fredoka with negative tracking (-0.02em, -0.03em for H1) and balanced wrapping. Body and labels never take letter-spacing.

**The Sentence-Case Rule.** Labels, buttons, nav groups and section titles are sentence case. No uppercase tracked eyebrows, kickers or numbered lead-ins above headings; a heading stands on its own.

**The Style, Not Metaphor Rule.** The visual world lends form only. Labels, headings and demo data use the product's words (components, themes, parameters, examples, guides). Demo data uses the Acme Cloud SaaS domain: customers, orders, invoices, projects, team, deployments, plans, usage.

## Layout

Content sits in a 1500px max-width frame with gutters of 16px (mobile), 32px (sm) and 56px (lg). Home sections breathe at 80px vertical padding, 112px from lg, separated by 2px base-content rules and alternating base-100 / base-200, with one dark neutral band (themes) and a ground-coloured closing band.

The masthead is a two-column grid on lg: breadcrumb, title, lede, theme swatches and "On this page" jump pills on the left; a 22rem live cover on the right, with right padding reserved for the colour wheel. Below lg it stacks to one column and the cover becomes a full-width base-100 block (square from sm). The jump pills sit at the masthead's bottom-left, wrapping in a 42rem row.

Docs pages below the masthead use a three-column grid: 15rem side nav, the reading column (prose capped at 70ch), and a 12rem sticky "On this page" list from xl. Side nav appears from lg; on mobile it moves into the top bar's disclosure menu. The home live wall is a 1px-gap grid on a base-content backing (1, 2, then 4 columns) so the gaps read as ink rules. Theme tiles scroll horizontally with snap on the home page and form a 2/3/4-column grid on /themes.

## Elevation & Depth

Flat by default. Depth comes from the ground / reading-surface contrast and 2px ink rules, not shadows. Three soft shadows exist, each tied to one object or state.

### Shadow Vocabulary
- **Wheel lift** (`box-shadow: 0 18px 40px -18px rgb(0 0 0 / 0.45)`): under the next-theme colour wheel only.
- **Tile lift** (`box-shadow: 0 22px 40px -22px rgb(0 0 0 / 0.6)`): theme tile on hover, with a 4px upward lift.
- **Pill seat** (`box-shadow: 0 1px 0 rgb(0 0 0 / 0.08)`): hairline under the search pill on the ground.

### Named Rules
**The Flat Surface Rule.** Cards, covers and panels are flat at rest. A shadow appears only under the colour wheel and under a theme tile being hovered.

## Shapes

Three corner families. Covers and theme tiles are near-square (6px). Working surfaces are rounded cards (16px): example cards, the playground, code blocks, the live wall, chart cards; small tiles take 12px (theme-switcher swatches, search result icon tiles). Every small control is a full pill: nav links, CTA buttons, the install command, search, jump pills, the Preview/Code toggle, enum chips, theme swatches. The colour wheel and swatches are circles. Structural borders are 2px base-content; row dividers are 1px base-300. The search palette is the softest shape at 24px.

## Components

### Masthead
The page's cover. Full-bleed primary ground with primary-content ink; contains an optional breadcrumb trail, the H1, a lede, on the home page only a row of eight theme swatches with a "+29 more themes" link, the "On this page" jump pills, and the live cover. Ground and ink cross-fade over 600ms (cubic-bezier(.2, .8, .2, 1)) as a fallback where view transitions are unavailable. The top bar and footer share the same ground. On the home page the lede carries the two CTAs and a pill-shaped install command (`dotnet add package DaisyBlazor.Components`) outlined at 40% ink with an ink-filled copy button.

### Live cover
A flat base-100 square (6px corners, 24px padding, 22rem on lg) holding the page's component running live; on the home page, several components composing an Acme Cloud dashboard card. No border, no shadow: the reading surface against the ground is the frame.

### Next-theme colour wheel
A circle rendered in the next theme (the button carries that theme's `data-theme`), filled with a conic gradient from -30deg of that theme's primary (40%), secondary (to 62%), accent (to 80%) and neutral. It bleeds off the masthead's top-right edge, growing from 9rem to 26rem across breakpoints. A base-100 centre (inset 31%) with a 0.35rem ring of primary darkened 20% toward black shows "Try theme" (0.65rem, semibold, 70%) over the theme name (bold Fredoka) from lg. Hover or focus rotates the wheel 40deg and nudges it 12px left over 900ms (cubic-bezier(.16, 1, .3, 1)). Its accessible name is "Try the next theme: {name}".

### Theme swatches
28px circles, each rendered in its own theme so it shows that theme's primary, with a 3px base-100 border. The active theme gets a base-content border plus a 2px base-100 ring; hover scales 110%. `aria-pressed` marks the current one.

### "On this page" jump pills
The page's sections as outlined pills on the ground: 2px primary-content border, bold 0.9rem label, inverting to ink fill with ground text on hover (200ms). On xl a sticky "On this page" list repeats them on the reading surface with a 2px base-300 rail, the section in view marked by a primary rail segment and bold ink.

### Theme tiles (/themes and the home themes band)
A square button rendered in its own `data-theme`: primary ground, primary-content ink, "Light" or "Dark" over the theme name in Fredoka, a secondary circle peeking from the top-right corner, and a base-100 inset at the foot with real daisyUI buttons (Save, Edit, More), a toggle and a progress bar. Hover lifts 4px with the tile shadow and slides the circle; the active tile gets a 4px base-content ring offset 4px and an "Active" pill.

### Doc tile
A component's live cover (inert, scaled to 78%, fading out at the foot) over a 2px-ruled base-200/60 strip with its Fredoka name and two-line summary; the strip floods with the ground on hover. Chart covers are capped at 15rem wide (12rem in the compact tile) because charts take their height from their width. The compact tile (name only) builds the home chart wall and the chart catalogue in the Charts guide; charts are always shown grouped by family (Essentials, Hierarchy and networks, Distribution, Over time, Compare, Composition and progress, Specialised).

### Example card
Fredoka title with a pill toggle (2px base-content border; the selected half inverts to base-content fill with base-100 text) between Preview and Code. The body is a 16px-radius box with a 2px base-content border: preview on base-100 at 24-32px padding, or the flush code block. A 2px-ruled footer on base-200/60 shows the source path in mono and a Source link.

### Code block
Neutral surface, neutral-content text, JetBrains Mono 0.85rem/1.7, 16px corners unless flush inside a card; an optional mono title strip ruled at 15% ink. Syntax colours are mixed from the theme: tags from primary (55%, semibold), attributes from secondary (50%), strings from accent (75%), Razor directives from warning (75%, semibold), keywords from info (75%, semibold), numbers from success (75%), comments at 65% neutral-content in italic. A ghost Copy button sits top-right, faded into the neutral by a leftward shadow, switching to "Copied" briefly.

### Playground
Same 2px-ruled, 16px card: the live component centred on base-100 (min 14rem tall), a base-200 controls grid under a 2px rule (1, 2, then 3 columns) built from DaisyBlazor's own inputs, each control headed by the mono parameter name and its mono type at 60%; enum values are pill radio chips (2px base-300 border, inverting to base-content fill when selected). The generated Razor sits in a flush code block below. A ghost pill Reset button activates once a value changes.

### API table
Desktop: full-width table, mono names and types, a 2px base-content head rule and 1px base-300 row rules, tabular numerals. Below md each parameter becomes a stacked row in a list bounded by 2px base-content top and bottom rules: name and type on one line, default ("= value") beneath, then the summary.

### Search palette (Ctrl K)
A native dialog, 42rem wide with 24px corners. Its header is a strip of ground: search icon, Fredoka 1.25rem semibold input in primary-content with its placeholder at 85% ink, Esc key. Results are 16px-radius rows with a 12px icon tile and a sentence-case kind label; the active row floods with primary and primary-content. A base-300-ruled footer lists the keyboard hints and the indexed component count. Opens on Ctrl+K, ⌘K or "/".

### Navigation
Top bar on the ground: rotating daisy mark, the "daisy" + softened "Blazor" logotype, bold nav pills (Guides, Components, Charts, Themes, GitHub) (hover at 10% ink, active inverted to ink fill), a base-100 search pill with Ctrl K keys and the theme switcher pill (ink fill, ground text, theme name from lg) opening a 2-3 column grid of swatch buttons each rendered in its own theme. Below md the nav collapses into a disclosure panel under a 20% ink rule. Side nav on the reading surface: sentence-case group labels at 60% ink ("Guides", "Components"), guide links as pills that turn primary when active, category headings with a component count, component links on a 2px base-300 rail with a primary dot for the active page.

### Buttons
- **Shape:** full pill (9999px).
- **Primary on the ground:** primary-content fill, primary text, 48px tall, 28px side padding, no border; hover at 90% fill.
- **Outline on the ground:** transparent with a 2px primary-content border; hover inverts to ink fill and ground text.
- **Outline on the reading surface:** transparent with a 2px base-content border; hover inverts to base-content fill and base-100 text.
- **Focus:** a 2.5px primary outline offset 3px on every focusable element.

### Motion
- **Switching theme:** a view transition where the new page is revealed by a circular clip growing from the pointer to 150vmax over 820ms (cubic-bezier(.16, 1, .3, 1)); the old snapshot does not animate.
- **Colour wheel:** rotate 40deg on hover/focus over 900ms; the boot screen spins the daisy mark every 2.4s.
- **Reduced motion:** the circular reveal is skipped, ground and wheel transitions are removed and the boot spinner stops; the theme still changes instantly.

## Do's and Don'ts

### Do:
- **Do** use only daisyUI semantic roles (`primary`, `primary-content`, `base-100/200/300`, `base-content`, `neutral`, `neutral-content`, and status roles as tints) so all 37 themes work.
- **Do** print small text on the ground at full `primary-content`; keep the 75% softened ink for the large logotype only.
- **Do** frame cards, tables and section breaks with 2px base-content rules, and divide rows with 1px base-300.
- **Do** use pills for small controls, 6px corners for covers and theme tiles, and 12-16px for cards and tiles.
- **Do** put the live component first: a live cover in the masthead, a preview before code in every example card.
- **Do** derive syntax colours from the theme's own roles mixed into neutral-content.
- **Do** name things in the product's words and draw demo data from the Acme Cloud SaaS domain (customers, orders, invoices, projects, team, deployments).
- **Do** honour `prefers-reduced-motion` with an instant theme change instead of the reveal.

### Don't:
- **Don't** hard-code hex or any colour outside the theme roles in the site layer.
- **Don't** flood the reading column with the ground colour; reading stays on base-100.
- **Don't** use uppercase tracked eyebrows, kickers or numbered lead-ins above headings; labels are sentence case.
- **Don't** soften small ink on the ground with opacity.
- **Don't** add shadows to cards or panels; depth belongs to the colour wheel and the hovered theme tile only.
- **Don't** let the visual style leak into vocabulary: no releases, tracks, sides, discs or music demo data in labels, headings or examples.
- **Don't** fall back to a grey app bar and drawer layout; the top bar is part of the ground.
