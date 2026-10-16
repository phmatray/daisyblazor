# DaisyBlazor styles

The Tailwind CSS v4 + daisyUI v5 preset shipped in the `DaisyBlazor.Components` NuGet package. No npm
package is involved: daisyUI is vendored as its standalone bundles, and the package's
`buildTransitive` target copies this folder into the consumer's `obj/daisyblazor/` on every build (and,
with `<DaisyBlazorBuildCss>true</DaisyBlazorBuildCss>`, runs the Tailwind standalone CLI).

| File | Contents |
| --- | --- |
| `preset.css` | daisyUI with the two DaisyBlazor themes + all 35 built-in themes, then `kit.css` |
| `preset-core.css` | daisyUI with only the two DaisyBlazor themes, then `kit.css` |
| `kit.css` | the two DaisyBlazor theme blocks, `safelist.css`, and a few component styles |
| `safelist.css` | `@source inline(...)` rules only — every class the components emit |
| `daisyui.mjs`, `daisyui-theme.mjs` | daisyUI's standalone bundles (MIT), vendored |

Don't edit the generated parts by hand: `node scripts/gen-styles.mjs` re-vendors the daisyUI bundles at
the version in the root `package.json` and regenerates the literal-class block of `safelist.css` from
the component sources. CI runs it with `--check`.

## Consumer wiring

```css
@import "tailwindcss";
@import "../obj/daisyblazor/preset.css";   /* or preset-core.css */
@source "./Components/**/*.{razor,cs}";    /* your own markup only */
```

## Custom themes

Add a theme block after the preset import:

```css
@plugin "../obj/daisyblazor/daisyui-theme.mjs" {
    name: "mybrand";
    default: true;
    /* --color-primary: ...; etc. */
}
```

Or, to declare **all** themes yourself, import only the safelist and enable daisyUI with your list:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/safelist.css";
@plugin "../obj/daisyblazor/daisyui.mjs" {
    themes: mybrand --default, mybrand-dark --prefersdark;
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
