---
title: Migrating from MudBlazor
description: Move an existing MudBlazor app to DaisyBlazor incrementally with a MudBlazor-compatible API — shared enums, icons, dialogs, snackbars, and a MudX → X component map.
---

DaisyBlazor ships a **MudBlazor-compatible API surface** so an existing MudBlazor app migrates
incrementally — mostly by **renaming `MudX` to `X`**. The enums, icon constants, dialog service and
snackbar service keep the same shapes you already use, so most markup and code-behind compiles after a
rename with minimal edits.

> This is a compatibility layer, not a drop-in clone. DaisyBlazor renders **daisyUI** markup, so the
> visual result follows your daisyUI theme rather than Material Design, and a few parameters differ.
> Migrate a page at a time and adjust as needed.

## The shared enums

These live in the `DaisyBlazor` namespace with the same names and members MudBlazor uses, mapped onto
daisyUI tokens:

| Enum | Members | Maps to |
|------|---------|---------|
| `Color` | `Default, Inherit, Primary, Secondary, Tertiary, Info, Success, Warning, Error, Dark, Surface` | daisyUI color tokens (`Tertiary` → `accent`, `Dark`/`Surface` → `neutral`). |
| `Variant` | `Text, Filled, Outlined` | `btn-ghost` / solid / `btn-outline` (and equivalents on chips, alerts, fields). |
| `Size` | `Small, Medium, Large` | daisyUI `-sm` / default / `-lg` size modifiers. |
| `Typo` | `h1…h6, subtitle1/2, body1/2, button, caption, overline, inherit` | Tailwind text-size + weight scale (used by `Typography`). |
| `Severity` | `Normal, Info, Success, Warning, Error` | daisyUI status colors (alerts + snackbars). |

Other familiar enums are present too: `Justify`, `AlignItems`, `MaxWidth`, `Adornment`, `Origin`,
`Anchor`, `Breakpoint`, `DrawerVariant`, `ButtonType`, `SortDirection`, `InputType`, `Margin`,
`Underline`, `Wrap`, and more — so `Color.Primary`, `Variant.Outlined`, `Size.Small`, `Typo.h6`,
`Severity.Success` all keep working after the tag rename.

## Icons

`Icons.Material.Filled.*` and `Icons.Material.Outlined.*` constants exist just like in MudBlazor:

```razor
<Button StartIcon="@Icons.Material.Filled.Check">OK</Button>
<Icon Name="@Icons.Material.Filled.Delete" />
```

The values are **Material Symbols ligatures** (e.g. `Icons.Material.Filled.Check` == `"check"`),
rendered by the `Icon` component via the Material Symbols webfont — so make sure that font is loaded in
your host page (see [CSS preset](/daisyblazor/css-preset/)). The constant set covers the commonly-used icons;
because the values are just ligature strings, you can also pass any Material Symbols name directly
(`<Icon Name="rocket_launch" />`).

## Dialogs — `IDialogService`

Inject `IDialogService` exactly as before. It's registered by `AddDaisyBlazor()`, and a single
`<DialogProvider />` mounted in your layout renders the dialogs.

```razor
@inject IDialogService DialogService

@code {
    private async Task Open()
    {
        DialogParameters parameters = new() { { "Message", "Are you sure?" } };
        IDialogReference dialog = await DialogService.ShowAsync<MyDialog>("Confirm", parameters);
    }

    private async Task Confirm()
    {
        bool? ok = await DialogService.ShowMessageBoxAsync(
            "Delete item", "This cannot be undone.", yesText: "Delete", cancelText: "Cancel");
    }
}
```

Compatible types: `IDialogService`, `DialogParameters` (dictionary **and** collection-initializer
syntax) and `DialogParameters<T>` (the `{ x => x.Prop, value }` selector syntax), `DialogOptions`,
`DialogResult`, `IDialogReference`. The built-in confirm dialog is `MessageBoxDialog`, surfaced through
`ShowMessageBoxAsync(...)` (returns `true` = yes, `false` = no, `null` = canceled/dismissed).

Dialogs open as true modals (`showModal()`: focus moves in, the page behind is inert, focus returns to
the opener on close) and are labelled by their title. Escape cancels unless
`DialogOptions.CloseOnEscapeKey = false`; `DialogOptions.AlertDialog` forces `role="alertdialog"`
(the default for `MessageBoxDialog` and `ConfirmDialog`). Localise the close button with
`<DialogProvider CloseLabel="Fermer" />`.

## Snackbar — `ISnackbar`

Inject `ISnackbar` and call `Add` with a `Severity`, just like MudBlazor. It's registered by
`AddDaisyBlazor()`; mount one `<SnackbarProvider />` in your layout.

```razor
@inject ISnackbar Snackbar

@code {
    private void Notify() => Snackbar.Add("Saved!", Severity.Success);
}
```

`ISnackbar` exposes `Add(message, severity, configure)`, `Remove`, `Clear`, the `Shown` list and an
`OnChange` event.

Toasts are announced once through a single live region (assertive for errors/warnings, polite
otherwise), pause their auto-hide while hovered or focused, and the close button label is
localisable via `<SnackbarProvider CloseLabel="Fermer" />`.

## Setup deltas vs MudBlazor

| MudBlazor | DaisyBlazor |
|-----------|-------------|
| `AddMudServices()` | `AddDaisyBlazor()` |
| `<MudThemeProvider />` + `<MudPopoverProvider />` | `<ThemeProvider>…</ThemeProvider>` (wraps your layout) |
| `<MudDialogProvider />` | `<DialogProvider />` |
| `<MudSnackbarProvider />` | `<SnackbarProvider />` |
| `@using MudBlazor` | `@using DaisyBlazor` (+ `@using DaisyBlazor.Theming`) |
| MudBlazor bundled CSS/JS | Tailwind v4 + daisyUI build (see [Getting started](/daisyblazor/getting-started/)) |

## Component name map (`MudX` → `X`)

Drop the `Mud` prefix. A representative mapping:

| MudBlazor | DaisyBlazor | Notes |
|-----------|-------------|-------|
| `MudButton` | `Button` | `Color` / `Variant` / `Size` / `StartIcon` / `EndIcon`. |
| `MudIconButton` | `IconButton` | |
| `MudFab` | `Fab` | |
| `MudButtonGroup` | `ButtonGroup` | |
| `MudIcon` | `Icon` | `Name="@Icons.Material.Filled.X"`. |
| `MudCard` | `Card` | with `CardHeader` / `CardContent` / `CardActions`. |
| `MudPaper` | `Paper` | |
| `MudTable` | `Table` | data table (paging/sort); use `SimpleTable` for static markup. |
| `MudSimpleTable` | `SimpleTable` | |
| `MudDataGrid` | `DataGrid` | column-based grid (`PropertyColumn` / `TemplateColumn`). |
| `MudAlert` | `Alert` | `Severity=…`. |
| `MudBadge` | `Badge` | |
| `MudChip` | `Chip` | |
| `MudAvatar` | `Avatar` | |
| `MudProgressLinear` | `ProgressLinear` | |
| `MudProgressCircular` | `ProgressCircular` | (also `RadialProgress`). |
| `MudSkeleton` | `Skeleton` | `SkeletonType`. |
| `MudTooltip` | `Tooltip` | |
| `MudTabs` / `MudTabPanel` | `Tabs` / `Tab` | |
| `MudExpansionPanels` / `MudExpansionPanel` | `ExpansionPanels` / `ExpansionPanel` | |
| `MudMenu` / `MudMenuItem` | `Menu` / `MenuItem` | |
| `MudList` / `MudListItem` | `List` / `ListItem` | |
| `MudBreadcrumbs` | `Breadcrumbs` | |
| `MudPagination` | `Pagination` | |
| `MudNavMenu` / `MudNavLink` | `Menu` / `NavMenuLink` | |
| `MudDrawer` | `Drawer` | |
| `MudAppBar` | `Navbar` | |
| `MudTextField` | `TextField` | `InputType`, `Adornment`. |
| `MudNumericField` | `NumericField` | |
| `MudSelect` / `MudSelectItem` | `Select` / `SelectItem` | |
| `MudAutocomplete` | `Autocomplete` | |
| `MudCheckBox` | `Checkbox` | |
| `MudSwitch` | `Switch` | |
| `MudRadio` / `MudRadioGroup` | `Radio` / `RadioGroup` | |
| `MudSlider` | `Range` | daisyUI `range`. |
| `MudRating` | `Rating` | |
| `MudDatePicker` | `DatePicker` | |
| `MudColorPicker` | `ColorPicker` | |
| `MudFileUpload` | `FileInput` | |
| `MudForm` | `Form` | with `Validator` / `ValidatorHint`. |
| `MudField` | `Field` | |
| `MudGrid` / `MudItem` | `Grid` / `Item` | 12-column responsive spans. |
| `MudStack` | `Stack` | |
| `MudContainer` | `Container` | `MaxWidth`. |
| `MudDivider` | `Divider` | |
| `MudSpacer` | `Spacer` | |
| `MudText` | `Typography` | `Typo=…`. |
| `MudLink` | `TextLink` | `Underline`. |
| `MudThemeProvider` | `ThemeProvider` | parameter-driven; see [Theming](/daisyblazor/theming/). |

DaisyBlazor also adds daisyUI-native components with no MudBlazor equivalent (`Hero`, `Stats`/`Stat`,
`Steps`/`Step`, `Timeline`, `ChatBubble`, `Countdown`, `Diff`, `Kbd`, `Mask`, `Swap`, `Dock`,
`Indicator`, `Status`, `Carousel`, the `Mockup*` family). See the
[component reference](/daisyblazor/components/) for the full list.

## Migration recipe

1. Replace `AddMudServices()` with `AddDaisyBlazor()` and add the Tailwind/daisyUI build (or scaffold a
   fresh app with `dotnet new daisyblazor` and copy your pages in).
2. Swap `@using MudBlazor` for `@using DaisyBlazor` (+ `@using DaisyBlazor.Theming`).
3. Replace the Mud providers with `ThemeProvider` / `DialogProvider` / `SnackbarProvider`.
4. Rename `MudX` tags to `X` page by page; the enums, `Icons.*`, `IDialogService` and `ISnackbar` keep
   working. Fix the occasional parameter that differs, and re-check styling against your daisyUI theme.
