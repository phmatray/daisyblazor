# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

.NET developers building Blazor apps (Server or WebAssembly) who want Tailwind CSS v4 + daisyUI v5 styling with typed components. Two arrivals: newcomers evaluating the library ("is this better than MudBlazor for me?") and teams migrating an existing MudBlazor app one `MudX` → `X` rename at a time. Once adopted, they return to the docs to look up a component's parameters and copy an example.

## Product Purpose

DaisyBlazor is a NuGet component library (`DaisyBlazor.Components`, `DaisyBlazor.Charts`) plus a `dotnet new daisyblazor` template. The documentation site exists to let a developer see every component working, copy its Razor source, read its API, and wire up the CSS preset and theming without leaving the page. Success: a visitor goes from landing to a running component in their app, and returns for reference.

## Positioning

daisyUI-native components for Blazor with a MudBlazor-compatible API surface: the only library that lets a MudBlazor app migrate to Tailwind/daisyUI incrementally. Ships the Tailwind build inside NuGet (no Node/npm), 35 daisyUI themes plus two DaisyBlazor themes through a cascading `ThemeProvider`, and 54 SVG chart types (D3 layouts bundled in the package, optional server rendering) that follow the active theme.

## Operating Context

Visitors read on desktop next to their IDE, often with a Blazor project open; secondary reading on mobile. Hosted on GitHub Pages at `https://phmatray.github.io/daisyblazor/`. The site is a Blazor WebAssembly app built with DaisyBlazor itself (dogfooding): its chrome and demos use the library's own components and themes.

## Capabilities and Constraints

- 60+ components grouped by daisyUI taxonomy: Actions, Data display, Navigation, Feedback, Data input, Layout, Mockup; plus 54 chart types (core, hierarchy and flow, distribution, over time, compare, composition and progress).
- Services: `IDialogService`, `ISnackbar`, `ThemeProvider`; `Icons.Material.*` (Material Symbols).
- .NET 10, Tailwind v4, daisyUI v5. Static hosting only (no server).
- Undecided: custom domain.

## Brand Commitments

Name "DaisyBlazor", the 🌼 daisy mark, README banner at `.github/banner.png`. MIT licence. Author: Philippe Matray.

## Evidence on Hand

Real: NuGet packages and download badge, CI, release history (CHANGELOG.md), screenshots in `assets/screenshots/`, guide content (getting started, theming, CSS preset, charts, MudBlazor migration) in `website/src/content/docs/` and `docs/`. No testimonials, customer logos, or benchmarks exist; do not fabricate them.

## Product Principles

1. Show, then tell: every component appears live before any prose about it.
2. The docs are the proof: built with the library, so every pixel is a demo.
3. Copy-paste fidelity: shown code is the exact code that renders the demo.
4. Migration is a first-class path, not an appendix.

## Accessibility & Inclusion

WCAG 2.2 AA for the site chrome; keyboard-reachable navigation, search and playground controls; respect `prefers-reduced-motion` and `prefers-color-scheme`.
