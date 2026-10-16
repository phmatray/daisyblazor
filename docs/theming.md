# Theming

DaisyBlazor uses [daisyUI themes](https://daisyui.com/docs/themes/). A theme is selected by setting
`data-theme="..."` on the `<html>` element; daisyUI swaps every semantic color token
(`--color-primary`, `--color-base-100`, …) so components re-skin automatically.

## ThemeProvider

`ThemeProvider` manages the active theme: it applies `data-theme`, follows the OS light/dark setting
when the preference is `"system"`, and persists the choice to `localStorage` + a cookie (so SSR paints
the right theme with no flash). It cascades itself, so descendants can read the state and switch
themes.

```razor
<ThemeProvider>
    @Body
</ThemeProvider>
```

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

### Reading & switching from a component

```razor
@code {
    [CascadingParameter] private ThemeProvider? Theme { get; set; }

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
`prefers-color-scheme: dark`).

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
