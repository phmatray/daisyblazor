using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace DaisyBlazor.Charts;

/// <summary>
/// Renders an SVG <c>&lt;text&gt;</c> element from code. Razor reserves the literal
/// <c>&lt;text&gt;</c> markup tag as its template-transition directive, so chart components emit
/// axis/legend labels through this helper to sidestep the parser while keeping pure inline SVG.
/// </summary>
public sealed class SvgText : ComponentBase
{
    /// <summary>X coordinate in SVG user units.</summary>
    [Parameter] public double X { get; set; }

    /// <summary>Y coordinate in SVG user units.</summary>
    [Parameter] public double Y { get; set; }

    /// <summary>Text anchor (start | middle | end).</summary>
    [Parameter] public string Anchor { get; set; } = "start";

    /// <summary>Font size in SVG user units.</summary>
    [Parameter] public double FontSize { get; set; } = 11;

    /// <summary>Optional font weight (e.g. "600").</summary>
    [Parameter] public string? FontWeight { get; set; }

    /// <summary>Fill color (CSS color or theme token).</summary>
    [Parameter] public string Fill { get; set; } = "var(--color-base-content)";

    /// <summary>Fill opacity (0..1).</summary>
    [Parameter] public double FillOpacity { get; set; } = 1;

    /// <summary>Optional vertical shift (e.g. "0.35em" to centre the text on <see cref="Y"/>).</summary>
    [Parameter] public string? Dy { get; set; }

    /// <summary>The text content to render.</summary>
    [Parameter] public string? Text { get; set; }

    private static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "text");
        builder.AddAttribute(1, "x", X.ToString(Ci));
        builder.AddAttribute(2, "y", Y.ToString(Ci));
        builder.AddAttribute(3, "text-anchor", Anchor);
        builder.AddAttribute(4, "font-size", FontSize.ToString(Ci));
        if (!string.IsNullOrEmpty(FontWeight))
        {
            builder.AddAttribute(5, "font-weight", FontWeight);
        }

        builder.AddAttribute(6, "fill", Fill);
        if (FillOpacity < 1)
        {
            builder.AddAttribute(7, "fill-opacity", FillOpacity.ToString(Ci));
        }

        if (!string.IsNullOrEmpty(Dy))
        {
            builder.AddAttribute(8, "dy", Dy);
        }

        builder.AddContent(9, Text);
        builder.CloseElement();
    }
}
