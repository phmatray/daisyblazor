# CSS preset

DaisyBlazor ships a Tailwind CSS v4 + daisyUI v5 **preset** inside the `DaisyBlazor.Components` NuGet
package. Nothing comes from npm: the preset bundles daisyUI itself, and the package can run the
Tailwind standalone CLI for you. The preset does three jobs:

1. **Enables daisyUI** (a vendored copy of daisyUI's standalone bundle, so no `npm install daisyui`).
2. **Registers themes**: the two neutral DaisyBlazor defaults — `daisyblazor` (light, the default) and
   `daisyblazor-dark` (dark, `prefers-color-scheme: dark`) — plus all 35 built-in daisyUI v5 themes
   (`light`, `dark`, `cupcake`, `synthwave`, `dracula`, `nord`, `business`, `night`, `dim`, `sunset`,
   `abyss`, `silk`, …).
3. **Safelists every class the components emit** so they survive Tailwind's purge (see below).

## Wiring it into your app

On every build, the package copies the preset into your project's `obj/daisyblazor/` folder. Turn on
the CSS build in your `.csproj`:

```xml
<PropertyGroup>
  <DaisyBlazorBuildCss>true</DaisyBlazorBuildCss>
</PropertyGroup>
```

and write `Styles/main.css`:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/preset.css";

/* Your own markup. The packaged components need no glob: the preset safelists their classes. */
@source "./Components/**/*.{razor,cs}";
```

`dotnet build` then compiles `Styles/main.css` to `wwwroot/css/app.css`. The first build downloads the
pinned [Tailwind CSS standalone CLI](https://github.com/tailwindlabs/tailwindcss/releases) into a
per-user cache (`~/.local/share/daisyblazor/tailwind/<version>/` on Linux,
`%LOCALAPPDATA%\daisyblazor\tailwind\<version>\` on Windows); later builds reuse it. Link the output in
your host page. The `dotnet new daisyblazor` template wires all of this for you.

| MSBuild property | Default | Purpose |
| --- | --- | --- |
| `DaisyBlazorBuildCss` | `false` | Compile the CSS on build with the Tailwind standalone CLI. |
| `DaisyBlazorCssInput` | `Styles/main.css` | Tailwind entry stylesheet. |
| `DaisyBlazorCssOutput` | `wwwroot/css/app.css` | Compiled stylesheet. |
| `DaisyBlazorTailwindVersion` | the version the package was tested with | Tailwind CLI release to download. |
| `DaisyBlazorTailwindPath` | the cached download | Use your own Tailwind binary (offline builds, musl Linux). |
| `DaisyBlazorStylesDir` | `obj/daisyblazor/` | Where the preset is copied. |

Already building CSS with the npm `tailwindcss` CLI or Vite? Leave `DaisyBlazorBuildCss` off and just
import `obj/daisyblazor/preset.css` from your stylesheet — the copy happens either way.

### Only the DaisyBlazor themes: `preset-core.css`

The 35 built-in themes add weight. If you only need `daisyblazor` / `daisyblazor-dark` (optionally
plus your own theme blocks), import `preset-core.css` instead. It is identical apart from the theme list:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/preset-core.css";
```

### Only your own themes: `safelist.css`

If you declare **all** your themes yourself, import just the safelist and enable daisyUI with your own
theme list, using the bundles the package copied next to it:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/safelist.css";

@plugin "../obj/daisyblazor/daisyui.mjs" {
    themes: mybrand --default, mybrand-dark --prefersdark;
}
@plugin "../obj/daisyblazor/daisyui-theme.mjs" {
    name: "mybrand";
    /* --color-primary: …; etc. */
}
```

## `@source` globs

Both forms work on Tailwind v4.1+ (verified on 4.3): a brace group such as
`@source "./Components/**/*.{razor,cs}";`, or one line per extension. The template uses one line per
extension, which also works on older 4.0.x builds.

## The comment-scanning gotcha

Tailwind v4 scans for `@source` and `@import` **tokens even inside CSS comments**. If you put a literal
`@source "..."` or `@import "..."` at-rule inside a `/* … */` block as documentation, Tailwind will try
to resolve it and can fail the build (or pull in unexpected files).

Never place those at-rule literals inside comments. When you want to *document* an optional source glob
in a comment, write it as plain prose (e.g. `source "<path>"` without the leading `@`) and tell the
reader to turn it into a real at-rule.

## The class safelist

The NuGet package ships compiled components, not their `.razor`/`.cs` sources, so Tailwind in your app
cannot scan them. `safelist.css` therefore force-generates, with `@source inline(...)`:

- the classes composed **dynamically in C#** (color/size variants such as `btn-primary`, `alert-soft`,
  `loading-spinner`, the 12-column grid/span/gap utilities, `mask-*`, `tooltip-*`, …), and
- **every literal class** used in the component markup (`menu`, `modal`, `navbar`, `drawer`, …).
  This block is generated from the component sources by `scripts/gen-styles.mjs`, and CI fails if it
  goes stale.

So you do **not** need any `@source` glob pointing at the DaisyBlazor packages.

## The required Material Symbols webfont

The `Icon` component and the `Icons.Material.*` constants render
[Material Symbols](https://fonts.google.com/icons) **ligatures** — the font turns a name like
`check_circle` into the glyph. The preset can't bundle a webfont, so load it once in your host page
(`App.razor` / `index.html`):

```html
<link rel="stylesheet"
      href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" />
```

Without it, icons render as their literal ligature text.

## See also

- [Getting started](/daisyblazor/getting-started/) — full install + wiring walkthrough.
- [Theming](/daisyblazor/theming/) — the themes the preset registers and how to switch / extend them.
