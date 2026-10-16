# DaisyBlazor.App

A Blazor Web App (Interactive Server) pre-wired with
[DaisyBlazor](https://github.com/phmatray/daisyblazor) — Tailwind CSS v4 + daisyUI v5 components,
theme switching, and dependency-free SVG charts.

## Run it

```bash
dotnet run      # builds the CSS, then starts the app on http://localhost:5080
```

No Node or npm needed: `dotnet build` / `dotnet run` rebuild `wwwroot/css/app.css` through the
`DaisyBlazorBuildCss` target shipped in the DaisyBlazor.Components package. It downloads the
Tailwind CSS standalone CLI once into a per-user cache, then compiles `Styles/main.css`.

## Where things live

| Path | What it is |
|------|------------|
| `Styles/main.css` | Tailwind entry point: imports the DaisyBlazor preset, declares `@source` globs, and the app brand layer. **Edit themes / source globs here.** |
| `obj/daisyblazor/` | The DaisyBlazor preset (daisyUI + themes + class safelist), copied from the NuGet package on every build. |
| `wwwroot/css/app.css` | Generated stylesheet (do not hand-edit). |
| `Program.cs` | `AddRazorComponents().AddInteractiveServerComponents()` + `AddDaisyBlazor()`. |
| `Components/App.razor` | Host page — links `app.css`, Material Symbols + Plus Jakarta Sans fonts. |
| `Components/Layout/MainLayout.razor` | `ThemeProvider`, navbar, theme picker, `DialogProvider` + `SnackbarProvider`. |
| `Components/Pages/Home.razor` | The sample page (hero, card, buttons, a chart). |

## Themes

The `daisyblazor` (light) and `daisyblazor-dark` themes ship in the preset, alongside all built-in
daisyUI themes. Switch at runtime via the picker in the navbar, or wire your own brand theme by
adding a `@plugin "../obj/daisyblazor/daisyui-theme.mjs" { name: "mybrand"; ... }` block to
`Styles/main.css` and pointing `ThemeProvider` at it.

See the DaisyBlazor docs: getting-started, theming, css-preset, components, and charts.
