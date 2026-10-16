---
title: CSS preset
description: What preset.css ships — the daisyUI plugin, registered themes, the generated class safelist, and how to install it from npm.
---

DaisyBlazor ships a Tailwind CSS v4 + daisyUI v5 **preset** — `preset.css` — that does three jobs so
your app doesn't have to:

1. **Enables the daisyUI plugin** with `logs: false`.
2. **Registers themes**: the two neutral DaisyBlazor defaults — `daisyblazor` (light, the default) and
   `daisyblazor-dark` (dark, `prefers-color-scheme: dark`) — plus all 35 built-in daisyUI v5 themes
   (`light`, `dark`, `cupcake`, `synthwave`, `dracula`, `nord`, `business`, `night`, `dim`, `sunset`,
   `abyss`, `silk`, …).
3. **Safelists every class the components emit** so they survive Tailwind's purge (see below).

### Only the DaisyBlazor themes: `preset-core.css`

The 35 built-in themes add weight. If you only need `daisyblazor` / `daisyblazor-dark` (optionally
plus your own theme blocks), import `preset-core.css` instead. It is identical apart from the theme list:

```css
@import "tailwindcss";
@import "@daisyblazor/tailwind/preset-core.css";
```

If you declare **all** your themes yourself, import just `@daisyblazor/tailwind/safelist.css` and keep
your own `@plugin "daisyui"` block.

## Wiring it into your app

Your app's source stylesheet (`Styles/main.css`) imports Tailwind, then the preset, then declares the
`@source` globs Tailwind uses to discover the classes **your own** markup uses (the preset already
covers the components' classes):

```css
@import "tailwindcss";
@import "@daisyblazor/tailwind/preset.css";

@source "./Components/**/*.razor";
@source "./Components/**/*.cs";
```

Compile it with the Tailwind CLI (`npm run build:css` / `watch:css`) and link the output in your host
page. The DaisyBlazor template wires all of this for you.

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

## The `@daisyblazor/tailwind` npm package

So the preset is resolvable by name (rather than by a fragile relative path into a NuGet cache),
DaisyBlazor publishes the preset as the npm package **`@daisyblazor/tailwind`**. Add it to your
`devDependencies` and import it as `@daisyblazor/tailwind/preset.css`:

```json
{
  "devDependencies": {
    "@daisyblazor/tailwind": "^0.3.0",
    "@tailwindcss/cli": "^4.1.0",
    "tailwindcss": "^4.1.0"
  }
}
```

```css
@import "@daisyblazor/tailwind/preset.css";
```

The package depends on `daisyui`, so you don't need to install it separately.

> Alternatively, copy the `.css` files (`preset.css`, `preset-core.css`, `kit.css`, `safelist.css`)
> out of the DaisyBlazor.Components NuGet package's `staticwebassets/styles` folder and import
> `preset.css` by relative path.

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

## Monorepo / workspace node_modules note

The Tailwind CLI resolves the daisyUI plugin (referenced by the preset) through Node's normal module
resolution. In a single app that's just the app's own `node_modules`. In a **monorepo** where the CSS is
built from a nested project, Node walks up the directory tree — so daisyUI must be resolvable from
either the project's `node_modules` **or** a workspace/root `node_modules`. This repo keeps a
root-level `package.json` with `daisyui`/`tailwindcss` for exactly that reason: it lets the shipped
preset's `@plugin "daisyui"` resolve during local CSS builds of the samples. If a build fails with
"cannot find module 'daisyui'", add daisyUI to the nearest `node_modules` the CLI can see.

## See also

- [Getting started](/daisyblazor/getting-started/) — full install + wiring walkthrough.
- [Theming](/daisyblazor/theming/) — the themes the preset registers and how to switch / extend them.
