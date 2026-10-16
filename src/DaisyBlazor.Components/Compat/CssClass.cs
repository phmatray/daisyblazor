using System.Text;

namespace DaisyBlazor;

/// <summary>
/// Maps the MudBlazor-compatible enums to daisyUI / Tailwind utility classes.
/// Returned strings are complete literal class names so the Tailwind build picks them up
/// (see the <c>@source inline(...)</c> safelist in Styles/main.css for dynamic combinations).
/// </summary>
public static class CssClass
{
    /// <summary>Joins non-empty class fragments with a single space.</summary>
    public static string Merge(params string?[] parts)
    {
        StringBuilder sb = new();
        foreach (string? p in parts)
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(p.Trim());
        }

        return sb.ToString();
    }

    /// <summary>daisyUI color suffix (e.g. "primary") for a semantic <see cref="Color"/>.</summary>
    public static string? ColorToken(Color color) => color switch
    {
        Color.Primary => "primary",
        Color.Secondary => "secondary",
        Color.Tertiary => "accent",
        Color.Info => "info",
        Color.Success => "success",
        Color.Warning => "warning",
        Color.Error => "error",
        Color.Dark => "neutral",
        // Surface is MudBlazor's light card surface: no colour modifier, so components fall back
        // to their base-100/base-200 look instead of duplicating Dark.
        _ => null
    };

    /// <summary>Button color class, e.g. "btn-primary".</summary>
    public static string ButtonColor(Color color) =>
        ColorToken(color) is { } t ? $"btn-{t}" : string.Empty;

    /// <summary>Button variant modifier ("", "btn-outline", "btn-ghost").</summary>
    public static string ButtonVariant(Variant variant) => variant switch
    {
        Variant.Outlined => "btn-outline",
        Variant.Text => "btn-ghost",
        _ => string.Empty
    };

    /// <summary>Button size class.</summary>
    public static string ButtonSize(Size size) => size switch
    {
        Size.Small => "btn-sm",
        Size.Large => "btn-lg",
        _ => string.Empty
    };

    /// <summary>Badge color class, e.g. "badge-success".</summary>
    /// <remarks>Default maps to badge-ghost (filled base-200) so it stays visible on base-100 surfaces.</remarks>
    public static string BadgeColor(Color color) =>
        ColorToken(color) is { } t ? $"badge-{t}" : color == Color.Default ? "badge-ghost" : string.Empty;

    /// <summary>Badge size class.</summary>
    public static string BadgeSize(Size size) => size switch
    {
        Size.Small => "badge-sm",
        Size.Large => "badge-lg",
        _ => string.Empty
    };

    /// <summary>Alert color class for a <see cref="Severity"/>, e.g. "alert-error".</summary>
    public static string AlertSeverity(Severity severity) => severity switch
    {
        Severity.Info => "alert-info",
        Severity.Success => "alert-success",
        Severity.Warning => "alert-warning",
        Severity.Error => "alert-error",
        _ => string.Empty
    };

    /// <summary>Text color utility for a semantic color, e.g. "text-primary".</summary>
    public static string? TextColor(Color color) => color switch
    {
        Color.Primary => "text-primary",
        Color.Secondary => "text-secondary",
        Color.Tertiary => "text-accent",
        Color.Info => "text-info",
        Color.Success => "text-success",
        Color.Warning => "text-warning",
        Color.Error => "text-error",
        // Not text-neutral: on dark themes neutral is close to base-100 and the glyph vanishes.
        Color.Dark => "text-base-content",
        Color.Surface => "text-base-content/70",
        _ => null
    };

    /// <summary>Tailwind text classes for a <see cref="Typo"/> scale step.</summary>
    public static string Typography(Typo typo) => typo switch
    {
        Typo.h1 => "text-4xl font-bold",
        Typo.h2 => "text-3xl font-bold",
        Typo.h3 => "text-2xl font-bold",
        Typo.h4 => "text-xl font-semibold",
        Typo.h5 => "text-lg font-semibold",
        Typo.h6 => "text-base font-semibold",
        Typo.subtitle1 => "text-base font-medium",
        Typo.subtitle2 => "text-sm font-medium",
        Typo.body1 => "text-base",
        Typo.body2 => "text-sm",
        Typo.button => "text-sm font-medium uppercase tracking-wide",
        Typo.caption => "text-xs opacity-70",
        Typo.overline => "text-xs uppercase tracking-widest opacity-70",
        _ => string.Empty
    };

    /// <summary>HTML element for a <see cref="Typo"/> value.</summary>
    public static string TypographyElement(Typo typo) => typo switch
    {
        Typo.h1 => "h1",
        Typo.h2 => "h2",
        Typo.h3 => "h3",
        Typo.h4 => "h4",
        Typo.h5 => "h5",
        Typo.h6 => "h6",
        _ => "p"
    };

    /// <summary>Spinner/progress size class.</summary>
    public static string LoadingSize(Size size) => size switch
    {
        Size.Small => "loading-sm",
        Size.Large => "loading-lg",
        _ => "loading-md"
    };

    /// <summary>justify-content utility for <see cref="Justify"/>.</summary>
    public static string JustifyContent(Justify justify) => justify switch
    {
        Justify.Center => "justify-center",
        Justify.FlexEnd => "justify-end",
        Justify.SpaceBetween => "justify-between",
        Justify.SpaceAround => "justify-around",
        Justify.SpaceEvenly => "justify-evenly",
        _ => "justify-start"
    };

    /// <summary>align-items utility for <see cref="AlignItems"/>.</summary>
    public static string AlignItemsClass(AlignItems align) => align switch
    {
        AlignItems.Center => "items-center",
        AlignItems.End => "items-end",
        AlignItems.Stretch => "items-stretch",
        AlignItems.Baseline => "items-baseline",
        _ => "items-start"
    };

    /// <summary>Container max-width utility.</summary>
    public static string MaxWidthClass(MaxWidth maxWidth) => maxWidth switch
    {
        MaxWidth.Small => "max-w-screen-sm",
        MaxWidth.Medium => "max-w-screen-md",
        MaxWidth.Large => "max-w-screen-lg",
        MaxWidth.ExtraLarge => "max-w-screen-xl",
        MaxWidth.ExtraExtraLarge => "max-w-screen-2xl",
        _ => string.Empty
    };
}
