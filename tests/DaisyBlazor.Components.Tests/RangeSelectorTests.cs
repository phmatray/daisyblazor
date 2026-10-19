using AngleSharp.Dom;
using DaisyBlazor.Charts;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The range selector: its window follows From and To, the handles are sliders the keyboard moves,
/// and dragging the window or a handle raises FromChanged and ToChanged, clamped and snapped.
/// </summary>
public class RangeSelectorTests : BunitContext
{
    // Eleven days, Oct 1 to Oct 11, across a 600-pixel plot: 60 pixels a day.
    private static readonly RangeSelectorPoint[] _days = Enumerable.Range(0, 11)
        .Select(i => new RangeSelectorPoint(new DateTime(2026, 10, 1).AddDays(i), 10 + i))
        .ToArray();

    private static readonly RangeSelectorLayout _layout = new(600, 80, new PlotArea(0, 4, 600, 58), [new(0, "Oct")], "M0,58L600,4L600,58Z", "M0,58L600,4");

    private DateTime? _from, _to;

    private IRenderedComponent<RangeSelector> RenderSelector(DateTime from = default, DateTime to = default, TimeSpan? step = null)
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ChartFakes.ModulePath);
        module.Setup<RangeSelectorLayout>("observe", _ => true).SetResult(_layout);
        module.SetupVoid("unobserve");
        IRenderedComponent<RangeSelector> cut = Render<RangeSelector>(ps => ps
            .Add(p => p.Points, _days)
            .Add(p => p.From, from)
            .Add(p => p.To, to)
            .Add(p => p.Step, step)
            .Add(p => p.FromChanged, (DateTime d) => _from = d)
            .Add(p => p.ToChanged, (DateTime d) => _to = d));
        cut.WaitForAssertion(() => cut.FindAll("[role=slider]").Count.ShouldBe(2));
        return cut;
    }

    private static IElement Handle(IRenderedComponent<RangeSelector> cut, int i) => cut.FindAll("[role=slider]")[i];

    [Fact]
    public void Draws_the_overview_and_selects_everything_by_default()
    {
        IRenderedComponent<RangeSelector> cut = RenderSelector();

        cut.Find("svg[role=img] path").GetAttribute("d").ShouldBe("M0,58L600,4L600,58Z");
        Handle(cut, 0).GetAttribute("aria-valuetext").ShouldBe("Oct 1, 2026");
        Handle(cut, 1).GetAttribute("aria-valuetext").ShouldBe("Oct 11, 2026");
        Handle(cut, 0).GetAttribute("aria-label").ShouldBe("Start of the selected range");
        Handle(cut, 0).GetAttribute("tabindex").ShouldBe("0");
        Handle(cut, 0).GetAttribute("aria-valuemin").ShouldBe(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds().ToString());
        // The start handle can go up to a step before the end.
        Handle(cut, 0).GetAttribute("aria-valuemax").ShouldBe(new DateTimeOffset(2026, 10, 10, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds().ToString());
        cut.Find(".range-selector-window").GetAttribute("style")!.ShouldContain("left:0%;width:100%");
    }

    [Fact]
    public void Arrow_keys_move_a_handle_by_one_step_shift_by_ten_home_and_end_to_the_limits()
    {
        IRenderedComponent<RangeSelector> cut = RenderSelector(new DateTime(2026, 10, 3), new DateTime(2026, 10, 6));
        cut.Find(".range-selector-window").GetAttribute("style")!.ShouldContain("left:20%;width:30%");

        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        _from.ShouldBe(new DateTime(2026, 10, 4));
        _to.ShouldBeNull();
        Handle(cut, 0).GetAttribute("aria-valuetext").ShouldBe("Oct 4, 2026");

        Handle(cut, 1).KeyDown(new KeyboardEventArgs { Key = "ArrowRight", ShiftKey = true });
        _to.ShouldBe(new DateTime(2026, 10, 11)); // ten days on, clamped to the last point

        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "End" });
        _from.ShouldBe(new DateTime(2026, 10, 10)); // a step before the end

        Handle(cut, 1).KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });
        _to.ShouldBe(new DateTime(2026, 10, 11)); // cannot pass the start: unchanged, nothing raised again
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "Home" });
        _from.ShouldBe(new DateTime(2026, 10, 1));

        _from = null;
        Handle(cut, 0).KeyDown(new KeyboardEventArgs { Key = "a" });
        _from.ShouldBeNull();
    }

    [Fact]
    public void From_and_To_outside_the_data_are_clamped()
    {
        IRenderedComponent<RangeSelector> cut = RenderSelector(new DateTime(2026, 9, 1), new DateTime(2027, 1, 1));

        Handle(cut, 0).GetAttribute("aria-valuetext").ShouldBe("Oct 1, 2026");
        Handle(cut, 1).GetAttribute("aria-valuetext").ShouldBe("Oct 11, 2026");
    }

    [Fact]
    public void Dragging_the_window_moves_both_ends_and_stays_inside_the_data()
    {
        IRenderedComponent<RangeSelector> cut = RenderSelector(new DateTime(2026, 10, 3), new DateTime(2026, 10, 5));

        cut.Find(".range-selector-window").PointerDown(new PointerEventArgs { ClientX = 100, PointerId = 1 });
        cut.Find(".range-selector-capture").PointerMove(new PointerEventArgs { ClientX = 160, PointerId = 1 });
        _from.ShouldBe(new DateTime(2026, 10, 4));
        _to.ShouldBe(new DateTime(2026, 10, 6));

        cut.Find(".range-selector-capture").PointerMove(new PointerEventArgs { ClientX = 2000, PointerId = 1 });
        _from.ShouldBe(new DateTime(2026, 10, 9));
        _to.ShouldBe(new DateTime(2026, 10, 11));

        cut.Find(".range-selector-capture").PointerUp(new PointerEventArgs { ClientX = 2000, PointerId = 1 });
        cut.FindAll(".range-selector-capture").ShouldBeEmpty();
    }

    [Fact]
    public void Dragging_a_handle_resizes_the_window_snapped_to_the_step()
    {
        IRenderedComponent<RangeSelector> cut = RenderSelector(step: TimeSpan.FromDays(2));

        // 70 pixels is 1⅙ days: the start snaps to one whole step of two days.
        Handle(cut, 0).PointerDown(new PointerEventArgs { ClientX = 0 });
        cut.Find(".range-selector-capture").PointerMove(new PointerEventArgs { ClientX = 70 });
        _from.ShouldBe(new DateTime(2026, 10, 3));
        _to.ShouldBeNull();
        cut.Find(".range-selector-capture").PointerUp(new PointerEventArgs());

        // The end cannot come closer to the start than one step.
        Handle(cut, 1).PointerDown(new PointerEventArgs { ClientX = 600 });
        cut.Find(".range-selector-capture").PointerMove(new PointerEventArgs { ClientX = -500 });
        _to.ShouldBe(new DateTime(2026, 10, 5));

        // Keys step by the step too.
        cut.Find(".range-selector-capture").PointerUp(new PointerEventArgs());
        Handle(cut, 1).KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        _to.ShouldBe(new DateTime(2026, 10, 7));
    }

    [Fact]
    public void No_points_draws_an_empty_overview_without_handles()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ChartFakes.ModulePath);
        module.Setup<RangeSelectorLayout>("observe", _ => true).SetResult(_layout with { Area = "", Line = "" });
        IRenderedComponent<RangeSelector> cut = Render<RangeSelector>();

        cut.WaitForAssertion(() => cut.Find("svg[role=img]").ShouldNotBeNull());
        cut.FindAll("[role=slider]").ShouldBeEmpty();
        cut.Find("desc").TextContent.ShouldBe("No data.");
    }
}
