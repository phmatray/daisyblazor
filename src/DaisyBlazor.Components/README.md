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
```

```xml
<!-- MyApp.csproj: compile Styles/main.css with the Tailwind standalone CLI on every build (no Node/npm) -->
<PropertyGroup>
  <DaisyBlazorBuildCss>true</DaisyBlazorBuildCss>
</PropertyGroup>
```

## Wire up

`Program.cs`:

```csharp
builder.Services.AddDaisyBlazor();
```

The built-in labels ("Close", "Loading", "Pagination"…) are set once for the whole app, and a component's own
parameter still wins:

```csharp
builder.Services.AddDaisyBlazor(o => o.Strings = new DaisyBlazorStrings { Close = "Fermer", Loading = "Chargement" });
```

An app that switches culture at runtime subclasses it instead; the properties are read on every render, so a
`.resx` designer class (which follows `CultureInfo.CurrentUICulture`) gives each request its own language:

```csharp
sealed class AppStrings : DaisyBlazorStrings
{
    public override string Close => Resources.Shared.Close;
}

builder.Services.AddDaisyBlazor(o => o.Strings = new AppStrings());
```

`_Imports.razor`:

```razor
@using DaisyBlazor
@using DaisyBlazor.Theming
```

Your `main.css`:

```css
@import "tailwindcss";
@import "../obj/daisyblazor/preset.css";      /* daisyUI + themes + class safelist, copied by the package */
@source "./Components/**/*.{razor,cs}";       /* your own markup only */
```

See the repository docs for theming, the MudBlazor migration guide, and the full component reference.
