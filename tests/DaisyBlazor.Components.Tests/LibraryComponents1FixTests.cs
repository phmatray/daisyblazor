using System.Globalization;
using AngleSharp.Dom;
using DaisyBlazor;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-1 bug bucket (Countdown, Paper, Range, Drawer, Mask).</summary>
public class LibraryComponents1FixTests : BunitContext
{
    public LibraryComponents1FixTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Countdown_MinDigits_emits_digits_and_clamps_to_999()
    {
        IElement inner = Render<Countdown>(ps => ps.Add(p => p.Value, 1500).Add(p => p.MinDigits, 2))
            .Find(".countdown > span");

        inner.GetAttribute("style").ShouldBe("--value:999;--digits:2;");
        Render<Countdown>(ps => ps.Add(p => p.Value, 9)).Find(".countdown > span")
            .GetAttribute("style").ShouldBe("--value:9;");
    }

    [Fact]
    public void Countdown_exposes_its_value_to_assistive_tech()
    {
        IElement inner = Render<Countdown>(ps => ps.Add(p => p.Value, 42)).Find(".countdown > span");

        inner.GetAttribute("aria-live").ShouldBe("polite");
        inner.GetAttribute("aria-label").ShouldBe("42");
    }

    [Fact]
    public void Countdown_label_is_stacked_under_the_number()
    {
        IRenderedComponent<Countdown> cut = Render<Countdown>(ps => ps.Add(p => p.Value, 42).Add(p => p.Label, "sec"));

        IElement wrapper = cut.Find("span.flex-col");
        wrapper.Children.Length.ShouldBe(2);
        wrapper.Children[0].ClassList.ShouldContain("countdown");
        wrapper.Children[1].TextContent.ShouldBe("sec");
    }

    [Fact]
    public void Countdown_Digits_keeps_hours_above_99()
    {
        Countdown.Digits(150 * 3600 + 61).ShouldBe([150, 1, 1]);
    }

    [Theory]
    [InlineData(1, "shadow-xs")]
    [InlineData(3, "shadow-sm")]
    [InlineData(6, "shadow-md")]
    [InlineData(12, "shadow-lg")]
    [InlineData(18, "shadow-xl")]
    [InlineData(24, "shadow-2xl")]
    public void Paper_elevation_scale_is_strictly_increasing(int elevation, string shadow)
    {
        Render<Paper>(ps => ps.Add(p => p.Elevation, elevation)).Find("div").ClassList.ShouldContain(shadow);
    }

    [Fact]
    public void Paper_with_OnClick_is_a_keyboard_button()
    {
        int clicks = 0;
        IRenderedComponent<Paper> cut = Render<Paper>(ps => ps.Add(p => p.OnClick, () => clicks++));
        IElement div = cut.Find("div");

        div.GetAttribute("role").ShouldBe("button");
        div.GetAttribute("tabindex").ShouldBe("0");
        div.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        div.KeyUp(new KeyboardEventArgs { Key = " " });
        div.KeyDown(new KeyboardEventArgs { Key = "a" });
        clicks.ShouldBe(2);

        IElement plain = Render<Paper>().Find("div");
        plain.HasAttribute("role").ShouldBeFalse();
        plain.HasAttribute("tabindex").ShouldBeFalse();
    }

    [Fact]
    public void Paper_applies_width_height_and_max_width()
    {
        IElement div = Render<Paper>(ps => ps
            .Add(p => p.Style, "color:red;")
            .Add(p => p.Width, "320px")
            .Add(p => p.Height, "10rem")
            .Add(p => p.MaxWidth, "100%")).Find("div");

        div.GetAttribute("style").ShouldBe("color:red;width:320px;height:10rem;max-width:100%;");
    }

    [Fact]
    public void Range_formats_numbers_with_invariant_culture_and_takes_a_label()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            IElement input = Render<Range>(ps => ps
                .Add(p => p.Step, 0.5)
                .Add(p => p.Value, 1.5)
                .Add(p => p.Label, "CPU")).Find("input");

            input.GetAttribute("step").ShouldBe("0.5");
            input.GetAttribute("value").ShouldBe("1.5");
            input.GetAttribute("aria-label").ShouldBe("CPU");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Drawer_Temporary_variant_overlays_instead_of_docking()
    {
        IRenderedComponent<Drawer> overlay = Render<Drawer>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.Variant, DrawerVariant.Temporary));
        overlay.Find(".drawer").ClassList.ShouldNotContain("drawer-open");
        overlay.Find("input.drawer-toggle").HasAttribute("checked").ShouldBeTrue();

        Render<Drawer>(ps => ps.Add(p => p.Open, true)).Find(".drawer").ClassList.ShouldContain("drawer-open");
    }

    [Theory]
    [InlineData(MaskShape.Hexagon2, "mask-hexagon-2")]
    [InlineData(MaskShape.Decagon, "mask-decagon")]
    [InlineData(MaskShape.Triangle2, "mask-triangle-2")]
    [InlineData(MaskShape.Triangle3, "mask-triangle-3")]
    [InlineData(MaskShape.Triangle4, "mask-triangle-4")]
    public void Mask_new_shapes_map_to_daisyui_classes(MaskShape shape, string cls)
    {
        Render<Mask>(ps => ps.Add(p => p.Shape, shape)).Find("div").ClassList.ShouldContain(cls);
    }
}
