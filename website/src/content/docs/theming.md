---
title: Theming
description: Drive daisyUI themes with the parameter-driven ThemeProvider — dark mode, persistence, 35 built-in themes, custom brand themes, and a theme picker.
---

DaisyBlazor uses [daisyUI themes](https://daisyui.com/docs/themes/). A theme is selected by setting
`data-theme="..."` on the `<html>` element; daisyUI swaps every semantic color token
(`--color-primary`, `--color-base-100`, …) so components re-skin automatically.

## ThemeProvider

`ThemeProvider` manages the active theme: it applies `data-theme`, follows the OS light/dark setting
(including live changes) when the preference is `"system"`, and persists the choice to `localStorage` +
a cookie. It cascades itself, so descendants can read the state and switch themes. Its JavaScript is a
static ES module (no `eval`), so it works under a strict Content-Security-Policy.

```razor
<ThemeProvider>
    @Body
</ThemeProvider>
```

### No flash on first paint

Add the pre-paint script to `<head>` in `App.razor`, after `<base href>`. It is a plain static file
(no inline code), runs before first paint, and works on static SSR pages that have no circuit:

```html
<script src="_content/DaisyBlazor.Components/theme-init.js"></script>
```

It reads the same `localStorage` key as `ThemeProvider` and falls back to `prefers-color-scheme`.
If you changed the provider's defaults, mirror them on the tag:
`data-storage-key="..."`, `data-light="mybrand"`, `data-dark="mybrand-dark"`.

It is **brand-neutral and parameter-driven** — every theme name is configurable:

| Parameter | Default | Purpose |
|---|---|---|
| `SystemLightTheme` | `"light"` | Theme used for `"system"` in OS light mode. |
| `SystemDarkTheme` | `"dark"` | Theme used for `"system"` in OS dark mode. |
| `Themes` | all 35 built-in daisyUI themes | The selectable set (e.g. for a picker). |
| `DarkThemes` | daisyUI's dark themes | Which themes count as "dark" for `IsDarkMode`. |
| `StorageKey` / `CookieKey` / `DarkModeCookieKey` | `daisy-theme` / `daisy-theme` / `daisy-darkmode` | Persistence keys. |
| `InitialPreference` | `null` | Seed from the server cookie during SSR to avoid a flash. |
| `OnThemeChanged` | — | Raised with the resolved theme after a switch. |
| `ThemeChanged` (event) | — | Raised on every change; subscribe to refresh a component (see below). |

### Reading & switching from a component

The cascade is fixed (`IsFixed="true"`), so a theme change never re-renders the wrapped tree.
Components that *display* theme state subscribe to `ThemeChanged`:

```razor
@implements IDisposable

@code {
    [CascadingParameter] private ThemeProvider? Theme { get; set; }

    protected override void OnInitialized()
    {
        if (Theme is not null) Theme.ThemeChanged += Refresh;
    }

    private void Refresh() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        if (Theme is not null) Theme.ThemeChanged -= Refresh;
    }

    private async Task UseDark() => await Theme!.SetThemeAsync("dark");
    private bool IsDark => Theme?.IsDarkMode ?? false;
}
```

`Theme.Preference` is the stored choice (a theme name or `"system"`); `Theme.Theme` is the concrete
applied theme; `Theme.IsDarkMode` resolves against `DarkThemes`.

## Built-in themes

All 35 daisyUI v5 themes are enabled by `preset.css` (`light`, `dark`, `cupcake`, `synthwave`,
`dracula`, `nord`, `business`, `night`, `dim`, `sunset`, `abyss`, `silk`, …) plus the neutral
DaisyBlazor defaults: **`daisyblazor`** (light, default) and **`daisyblazor-dark`** (dark,
`prefers-color-scheme: dark`). To ship only the two DaisyBlazor themes, import `preset-core.css`
instead of `preset.css` (see [CSS preset](/daisyblazor/css-preset/)).

## A custom brand theme

Add a daisyUI theme block to your `main.css` after the preset import, then point `ThemeProvider` at it:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/preset.css";

@plugin "../obj/daisyblazor/daisyui-theme.mjs" {
    name: "mybrand";
    default: true;
    color-scheme: light;
    --color-primary: oklch(55% 0.2 264);
    --color-secondary: oklch(60% 0.18 200);
    /* …base/accent/neutral/info/success/warning/error… */
    --radius-box: 0.75rem;
}
```

```razor
<ThemeProvider SystemLightTheme="mybrand"
               SystemDarkTheme="daisyblazor-dark"
               Themes="@(new[] { "mybrand", "daisyblazor-dark", "light", "dark", "synthwave" })">
    @Body
</ThemeProvider>
```

## A theme picker

Build a picker from `Themes` and `SetThemeAsync`:

```razor
<select class="select" @onchange="@(e => Theme!.SetThemeAsync(e.Value!.ToString()!))">
    <option value="system">System</option>
    @foreach (string t in Theme!.Themes)
    {
        <option value="@t" selected="@(t == Theme.Preference)">@t</option>
    }
</select>
```

daisyUI's pure-CSS `theme-controller` input is also wrapped as the `ThemeController` component for
no-JS toggles.
