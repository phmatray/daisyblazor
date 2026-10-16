# DaisyBlazor styles

`preset.css` is a Tailwind v4 + daisyUI v5 preset shipped with the package. It enables daisyUI,
registers two neutral default themes (`daisyblazor` / `daisyblazor-dark`) plus all 35 built-in daisyUI
themes, and safelists every class the DaisyBlazor components emit (so they are never purged).
`preset-core.css` is the same minus the 35 built-in themes.

| File | Contents |
| --- | --- |
| `preset.css` | daisyUI plugin with all themes + `kit.css` |
| `preset-core.css` | daisyUI plugin with only the two DaisyBlazor themes + `kit.css` |
| `kit.css` | the two DaisyBlazor theme blocks, `safelist.css`, and a few component styles |
| `safelist.css` | `@source inline(...)` rules only — no plugin, no themes |

This folder is the single source: `node scripts/gen-styles.mjs` regenerates the literal-class block of
`safelist.css` from the component sources and copies these files into the npm package
(`src/DaisyBlazor.Tailwind`) and the `dotnet new` template. CI runs it with `--check`.

## Consumer wiring

Install the npm package (`npm install -D tailwindcss @tailwindcss/cli @daisyblazor/tailwind`), then in
your app's source stylesheet (e.g. `Styles/main.css`):

```css
@import "tailwindcss";
@import "@daisyblazor/tailwind/preset.css";   /* or preset-core.css */
@source "./Components/**/*.{razor,cs}";        /* your own markup only */
```

No `@source` glob on the DaisyBlazor package is needed. Without npm, copy the four `.css` files out
of the NuGet package's `staticwebassets/styles` folder and import `preset.css` by relative path.

## Custom themes

If you declare your own daisyUI themes, import `safelist.css` instead of the full `preset.css`
(which would also register the daisyUI plugin and the neutral `daisyblazor` themes), then add your
own `@plugin "daisyui"` block:

```css
@import "tailwindcss";
@import "@daisyblazor/tailwind/safelist.css";   /* kit class safelist only — no plugin, no themes */

@plugin "daisyui" {
    themes: mybrand --default, mybrand-dark --prefersdark;
}
@plugin "daisyui/theme" {
    name: "mybrand";
    default: true;
    /* --color-primary: ...; etc. */
}
```

Then reference your themes in `ThemeProvider`:

```razor
<ThemeProvider SystemLightTheme="mybrand" SystemDarkTheme="daisyblazor-dark"
               Themes="@(new[] { "mybrand", "daisyblazor-dark", "dark", "light" })">
    ...
</ThemeProvider>
```

## Required webfont

The `Icon` component and the `Icons.Material.*` constants render
[Material Symbols](https://fonts.google.com/icons) ligatures. Load the font once in your host page:

```html
<link rel="stylesheet"
      href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
```

### Using a different icon font or icon set

The Material Symbols font is the default, not a hard requirement — there are two opt-outs.

**1. Swap the icon font** by cascading an `IconOptions` near the app root. Every component that
renders icons (`Pagination`, `Alert`, `Menu`, …) inherits the new `FontClass`:

```razor
<CascadingValue Value="_icons" IsFixed="true">
    <Router ... />
</CascadingValue>

@code { readonly IconOptions _icons = new() { FontClass = "material-symbols-rounded" }; }
```

Because Material Symbols renders through **ligatures** (the span text `chevron_left` becomes a
glyph), this swap is transparent only for a *ligature-compatible* font that shares Material's
ligature names — e.g. another Material Symbols build or variant (`-outlined` / `-rounded` /
`-sharp`), or a self-hosted Material Symbols. The built-in composites (`Pagination`, `Alert`, …)
pass `Icons.Material.*` ligature names, so an **arbitrary** icon set (Lucide, Heroicons, …) is
*not* yet wired through them — overriding those built-in icons is a separate, not-yet-implemented
step. For your own icons, use the SVG escape below.

**2. Render inline SVG** by passing child content to `Icon` instead of a ligature `Name`
(the icon-font class is then omitted entirely). This covers any `<Icon>` you author directly:

```razor
<Icon><svg viewBox="0 0 24 24" class="w-6 h-6">...</svg></Icon>
```

> Notes: `Size` is ignored when child content is supplied — size the SVG yourself (e.g. `w-6 h-6`).
> The hero-badge and nav-card icon-sizing CSS still targets `.material-symbols-outlined`, so a
> fully custom icon set may need to restyle those two spots.
