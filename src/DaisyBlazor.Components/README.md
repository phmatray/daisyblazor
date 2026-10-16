# DaisyBlazor.Components

A [Tailwind CSS v4](https://tailwindcss.com) + [daisyUI v5](https://daisyui.com) component kit for
Blazor (Server or WebAssembly). It gives you:

- **60+ daisyUI-native components** mapped to daisyUI's own taxonomy (Actions, Data display,
  Navigation, Feedback, Data input, Layout, Mockup).
- A **MudBlazor-compatible API surface** (`Button`, `Card`, `Table`, dialog & snackbar services, an
  `Icons.Material.*` tree) so existing MudBlazor apps can migrate incrementally.
- A **parameter-driven `ThemeProvider`** for daisyUI's 35 built-in themes plus your own.
- A shippable **CSS preset** (`styles/preset.css`) so wiring Tailwind + daisyUI is a single `@import`.

## Install

```bash
dotnet add package DaisyBlazor.Components
npm install -D tailwindcss @tailwindcss/cli @daisyblazor/tailwind
```

## Wire up

`Program.cs`:

```csharp
builder.Services.AddDaisyBlazor();
```

`_Imports.razor`:

```razor
@using DaisyBlazor
@using DaisyBlazor.Theming
```

Your `main.css`:

```css
@import "tailwindcss";
@import "@daisyblazor/tailwind/preset.css";  /* daisyUI plugin + themes + class safelist */
@source "./Components/**/*.{razor,cs}";       /* your own markup only */
```

See the repository docs for theming, the MudBlazor migration guide, and the full component reference.
