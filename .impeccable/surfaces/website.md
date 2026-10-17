---
version: 1
slug: "website"
primary_target: "website"
related_targets: []
---

# Docs site (website/)

Scope: the DaisyBlazor documentation site, a Blazor WebAssembly app built with DaisyBlazor itself, replacing Starlight and absorbing the Server gallery. Mode: Read (landing page is Persuade inside the same world).

Audience/job: .NET devs evaluating or adopting DaisyBlazor; see every component live, tweak it in a playground, copy exact Razor, read parameters, wire preset and theming, migrate from MudBlazor.

Must ship: live examples with exact source, API tables (reflection + XML docs), per-component playground, 37 live themes with animated switch, ⌘K search. Static GitHub Pages hosting at /daisyblazor/.

## Direction contract

Revised 2026-10-17 at the owner's request: the record-sleeve metaphor (releases, tracks, sides, discs, crate, music demo data) confused the value proposition and is removed. The flat theme-ground masthead it produced is kept.

THESIS: The docs prove the library by wearing it: every page's masthead is a flat field in the active theme's primary, with the component itself running live as the cover. Refuses the grey app-bar-plus-drawer of MudBlazor docs.

OWN-WORLD: One flat ground = `--color-primary` of the active theme, primary-content as ink; base-100 is the reading relief; neutral for code and one contrast band. Fredoka display, Manrope body, JetBrains Mono code. Pills, 2px ink rules, 12-16px radii. Demo content uses one neutral domain (Acme Cloud SaaS admin: customers, orders, invoices, projects, team, deployments).

STORY: A .NET developer lands, sees that every component works in every theme with code they can paste, that MudBlazor apps migrate one rename at a time, and that the CSS build needs no npm.

FIRST VIEWPORT: Masthead in primary ground: logotype + nav + Ctrl K search + theme picker; H1 states the product; lede states the value proposition; CTAs; theme swatches; "On this page" jump pills; right: square base-100 cover with live components; the next theme peeks from the edge as a colour wheel.

FORM: posters-covers-sleeves-yeye-pop-sleeve form, de-themed (flat ground and cover kept, music vocabulary dropped); seed:3a4c26895bac7f77

SIGNATURE: theme switch = view-transition circular reveal from the pointer; /themes shows all 37 themes as tiles rendered in their own theme.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
