> [!IMPORTANT]
> **This repository has moved.** The code now lives in **[phmatray/blazor-tailwind-ui](https://github.com/phmatray/blazor-tailwind-ui)** under [`kits/daisyblazor`](https://github.com/phmatray/blazor-tailwind-ui/tree/main/kits/daisyblazor) — full git history preserved. This repository is archived (read-only).

<!-- portfolio-toc:start -->

## Table of Contents

- [Features ✨](#features-)
- [The problem it solves](#the-problem-it-solves)
- [Quick start](#quick-start)
- [Usage](#usage)
- [Packages](#packages)
- [Screenshots](#screenshots)
- [Repo layout](#repo-layout)
- [Documentation](#documentation)
- [Tech Stack](#tech-stack)
- [Roadmap 🗺️](#roadmap-)
- [Contributing & releasing](#contributing--releasing)
- [License](#license)

<!-- portfolio-toc:end -->

![daisyblazor banner](.github/banner.png)

<h1 align="center">🌼 DaisyBlazor</h1>

<p align="center">
  <strong>A Tailwind CSS v4 + daisyUI v5 component library for Blazor.</strong><br/>
  60+ daisyUI-native components, a MudBlazor-compatible API, theming, dialogs, snackbars,
  dependency-free charts, a shippable CSS preset, and a <code>dotnet new</code> template.
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/DaisyBlazor.Components"><img alt="NuGet" src="https://img.shields.io/nuget/v/DaisyBlazor.Components?logo=nuget&label=DaisyBlazor.Components"></a>
  <a href="https://www.nuget.org/packages/DaisyBlazor.Components"><img alt="Downloads" src="https://img.shields.io/nuget/dt/DaisyBlazor.Components?logo=nuget&label=downloads"></a>
  <a href="https://github.com/phmatray/daisyblazor/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/phmatray/daisyblazor/actions/workflows/ci.yml/badge.svg"></a>
  <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-blue.svg"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet">
</p>

<p align="center">
  <a href="https://phmatray.github.io/daisyblazor/"><strong>📖 Documentation</strong></a> ·
  <a href="https://phmatray.github.io/daisyblazor/getting-started/">Getting started</a> ·
  <a href="https://www.nuget.org/profiles/phmatray">NuGet</a>
</p>

<p align="center">
  <img src="assets/screenshots/home.png" alt="DaisyBlazor gallery — home" width="900">
</p>

---

## Features ✨

- **60+ daisyUI-native components** — organized under daisyUI's own taxonomy: Actions, Data display, Navigation, Feedback, Data input, Layout, and Mockup.
- **MudBlazor-compatible API** — `Button`, `Card`, `Table`, `IDialogService`, `ISnackbar`, `Icons.Material.*` let existing MudBlazor apps migrate incrementally, one `MudX` → `X` rename at a time.
- **`ThemeProvider` theming** — a cascading, MudThemeProvider-style component with 35 built-in daisyUI themes, dark-mode resolution via `prefers-color-scheme`, and SSR-safe persistence to localStorage/cookies.
- **Dependency-free SVG charts** — `LineChart`, `AreaChart`, `BarChart`, `PieChart`, `DonutChart` render as plain SVG and automatically follow the active daisyUI theme, with no JS charting runtime.
- **Shippable CSS preset** — one `@import "daisyblazor/preset.css";` wires up daisyUI, the theme set, and the dynamic-class safelist for Tailwind v4.
- **`dotnet new daisyblazor` template** — scaffolds a fully wired app (Tailwind, daisyUI, DI) in seconds.
- **Runnable component gallery** — `samples/DaisyBlazor.Gallery` showcases every component and chart live.

## The problem it solves

Blazor has no first-class Tailwind/daisyUI component story. Today you usually pick one of two paths,
and both have real costs:

- **Adopt a heavyweight component framework** (MudBlazor, Radzen, …) — you inherit its design language,
  its CSS runtime, and its lock-in. Restyling to match a Tailwind/daisyUI design system fights the framework.
- **Hand-roll daisyUI markup** — you write `<button class="btn btn-primary">` everywhere, then re-implement
  theming, dialogs, toasts, form binding, and a Tailwind build by hand in every project.

**DaisyBlazor closes that gap:**

| Pain | DaisyBlazor |
|------|-------------|
| No native daisyUI components for Blazor | **60+ components** mapped to daisyUI's own taxonomy (Actions, Data display, Navigation, Feedback, Data input, Layout, Mockup). |
| Migrating off MudBlazor is all-or-nothing | A **MudBlazor-compatible API** (`Button`, `Card`, `Table`, `IDialogService`, `ISnackbar`, `Icons.Material.*`) — migrate incrementally by renaming `MudX` → `X`. |
| Wiring Tailwind + daisyUI + a class safelist is fiddly | A **shippable CSS preset** — one `@import` enables daisyUI, the themes, and the dynamic-class safelist. |
| Theming/dark-mode is manual | A parameter-driven **`ThemeProvider`** with **35 built-in themes** + your own, dark mode, and SSR-safe persistence. |
| Charts mean a heavy JS dependency | **Dependency-free SVG charts** that follow the active daisyUI theme — zero charting runtime. |
| Starting a new app is boilerplate | `dotnet new daisyblazor` scaffolds a wired app in seconds. |

## Quick start

Scaffold a new app:

```bash
dotnet new install DaisyBlazor.Templates
dotnet new daisyblazor -o MyApp
cd MyApp && npm install && dotnet run
```

…or add it to an existing Blazor app:

```bash
dotnet add package DaisyBlazor.Components
npm install -D tailwindcss @tailwindcss/cli daisyui
```

```csharp
// Program.cs
builder.Services.AddDaisyBlazor();
```

```css
/* Styles/main.css — Tailwind v4 source globs are one line per extension (no { } brace groups). */
@import "tailwindcss";
@import "daisyblazor/preset.css";
@source "../**/DaisyBlazor.Components/**/*.razor";
@source "../**/DaisyBlazor.Components/**/*.cs";
```

```razor
<Card>
    <CardContent>
        <h2 class="card-title">Hello DaisyBlazor</h2>
        <Tabs>
            <Tab Title="One">First panel</Tab>
            <Tab Title="Two">Second panel</Tab>
        </Tabs>
        <Button Color="Color.Primary" StartIcon="@Icons.Material.Filled.Check">OK</Button>
    </CardContent>
</Card>
```

Full setup (CSS pipeline, fonts, DI, theming) is in **[Getting started](https://phmatray.github.io/daisyblazor/getting-started/)**.

## Usage

Drop a themed, dependency-free chart straight into a page — no JS bundle, no client-side charting library:

```razor
<LineChart Series="_lineSeries"
           Labels="_monthLabels"
           Smooth="true"
           ShowGrid="true"
           ShowPoints="true"
           ShowLegend="true"
           Title="Monthly Revenue vs. Expenses (k€)" />
```

Wrap your app in `ThemeProvider` once to get 35 daisyUI themes, dark-mode detection, and persisted preferences for free:

```razor
@* MainLayout.razor *@
<ThemeProvider>
    <Router AppAssembly="typeof(Program).Assembly">
        ...
    </Router>
</ThemeProvider>
```

Explore every component live with the gallery sample:

```bash
dotnet run --project samples/DaisyBlazor.Gallery
```

## Packages

| Package | Version | Description |
|---|---|---|
| **DaisyBlazor.Components** | [![nuget](https://img.shields.io/nuget/v/DaisyBlazor.Components?label=)](https://www.nuget.org/packages/DaisyBlazor.Components) | The component kit: daisyUI-native components + MudBlazor-compatible surface, `ThemeProvider`, dialog/snackbar services, and the CSS preset. |
| **DaisyBlazor.Charts** | [![nuget](https://img.shields.io/nuget/v/DaisyBlazor.Charts?label=)](https://www.nuget.org/packages/DaisyBlazor.Charts) | Dependency-free SVG charts (line, area, bar, pie/donut, sparkline) themed by daisyUI. |
| **DaisyBlazor.Templates** | [![nuget](https://img.shields.io/nuget/v/DaisyBlazor.Templates?label=)](https://www.nuget.org/packages/DaisyBlazor.Templates) | `dotnet new daisyblazor` starter template. |
| **@daisyblazor/tailwind** | — | npm package shipping the Tailwind/daisyUI preset for non-.NET build pipelines. |

## Screenshots

<table>
  <tr>
    <td width="50%"><img src="assets/screenshots/data-display.png" alt="Data display components"><br/><sub><b>Data display</b> — cards, table, stats, timeline, chat, carousel…</sub></td>
    <td width="50%"><img src="assets/screenshots/charts.png" alt="Charts"><br/><sub><b>Charts</b> — dependency-free SVG, theme-aware</sub></td>
  </tr>
  <tr>
    <td width="50%"><img src="assets/screenshots/data-input.png" alt="Data input components"><br/><sub><b>Data input</b> — inputs, select, range, rating, validation…</sub></td>
    <td width="50%"><img src="assets/screenshots/home.png" alt="Home"><br/><sub><b>Theming</b> — 35 daisyUI themes, light & dark</sub></td>
  </tr>
</table>

Run the live gallery yourself: `dotnet run --project samples/DaisyBlazor.Gallery`.

## Repo layout

```
src/        DaisyBlazor.Components, DaisyBlazor.Charts, DaisyBlazor.Tailwind
samples/    DaisyBlazor.Gallery — a runnable showcase of every component
templates/  DaisyBlazor.Templates — dotnet new template
website/    Astro Starlight documentation site (GitHub Pages)
tests/      bUnit component tests
docs/       markdown source for the docs site
scripts/    update-deps, build-css, pack
```

## Documentation

📖 **<https://phmatray.github.io/daisyblazor/>**

- [Getting started](https://phmatray.github.io/daisyblazor/getting-started/) — install, CSS wiring, DI, fonts.
- [Theming](https://phmatray.github.io/daisyblazor/theming/) — `ThemeProvider`, built-in & custom themes.
- [CSS preset](https://phmatray.github.io/daisyblazor/css-preset/) — what `preset.css` ships and how to wire `@source`.
- [Component reference](https://phmatray.github.io/daisyblazor/components/) — every component by daisyUI category.
- [Charts](https://phmatray.github.io/daisyblazor/charts/) — the dependency-free SVG charts.
- [Migrating from MudBlazor](https://phmatray.github.io/daisyblazor/migration/) — the `MudX` → `X` map.

<!-- portfolio-techstack:start -->

## Tech Stack

- **.NET 10**
- DaisyBlazor.Components
- DaisyBlazor.Charts
- bunit
- Shouldly
- xunit.v3
- xunit.runner.visualstudio

<!-- portfolio-techstack:end -->

## Roadmap 🗺️

- [ ] Grow chart coverage beyond line/area/bar/pie/donut/sparkline (e.g. radar, scatter)
- [ ] Add more MudBlazor-compatible surfaces to ease larger migrations
- [ ] Expand the `dotnet new daisyblazor` template with more starter layouts
- [ ] Broaden bUnit test coverage across the component set
- [ ] Publish additional recipes for the `@daisyblazor/tailwind` npm preset in non-.NET pipelines

Track progress and proposals in the [open issues](https://github.com/phmatray/daisyblazor/issues).

## Contributing & releasing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the branch strategy (`main` / `release` / feature branches)
and the automated release pipeline.

## License

[MIT](LICENSE)
