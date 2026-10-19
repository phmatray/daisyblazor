using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>PriceActionChart: sends its prices and settings to the price-action layout and draws bricks, Kagi strokes and X/O columns.</summary>
public class ChartsFinancialTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly PlotArea _plot = new(40, 10, 616, 336);

    private static readonly Candle[] _prices =
    [
        new(new DateTime(2026, 10, 19, 9, 0, 0), 10, 11, 9, 10),
        new(new DateTime(2026, 10, 19, 10, 0, 0), 10, 12, 10, 11),
    ];

    private static string Spec(BunitJSModuleInterop module)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe("price-action");
        return JsonSerializer.Serialize(invocation.Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    [Fact]
    public void Renko_draws_a_brick_per_item_with_the_dates_of_its_move()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        PriceActionLayout layout = new("renko", 1, 0,
            [
                new(1, true, 10, 11, "Oct 19, 2026 09:00", "Oct 19, 2026 10:00", 100, 40, 200, 150),
                new(2, false, 11, 10, "Oct 19, 2026 10:00", "Oct 19, 2026 10:00", 150, 40, 150, 200),
            ],
            [new(100, "1"), new(150, "2")], [new(200, "10", 10)], _plot);
        module.Setup<PriceActionLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<PriceActionChart> cut = Render<PriceActionChart>(ps => ps
            .Add(p => p.Prices, _prices)
            .Add(p => p.BoxSize, 1)
            .Add(p => p.ValueFormat, "{0:0.#} %"));

        cut.WaitForAssertion(() => cut.FindAll("svg g rect").Count.ShouldBe(2));
        cut.FindAll("svg g rect")[0].GetAttribute("fill").ShouldBe("var(--color-success)");
        cut.FindAll("svg g rect")[1].GetAttribute("fill").ShouldBe("var(--color-error)");
        cut.Markup.ShouldContain("Brick 1, rising: 10 % → 11 %; Oct 19, 2026 09:00 – Oct 19, 2026 10:00");
        cut.Markup.ShouldContain("Brick 2, falling: 11 % → 10 %; Oct 19, 2026 10:00<");
        cut.Markup.ShouldContain("2 bricks, 1 rising and 1 falling");
        cut.Markup.ShouldContain(">10<"); // y ticks as the layout labels them
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Renko chart");
        Spec(module).ShouldBe("{\"prices\":[{\"time\":\"2026-10-19T09:00:00\",\"open\":10,\"high\":11,\"low\":9,\"close\":10},{\"time\":\"2026-10-19T10:00:00\",\"open\":10,\"high\":12,\"low\":10,\"close\":11}],"
            + "\"kind\":\"renko\",\"boxSize\":1,\"reversal\":null,\"reversalPercent\":false,\"lines\":3,\"atrPeriod\":14,\"width\":640,\"height\":360}");
    }

    [Fact]
    public void Kagi_draws_thick_yang_and_thin_yin_strokes()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        PriceActionLayout layout = new("kagi", 0, 15,
            [new(1, true, 10, 13, "a", "b", 100, 30, 200, 120)],
            [new(100, "1")], [], _plot)
        {
            Strokes = [new(100, 200, 100, 160, false), new(100, 160, 100, 120, true)],
        };
        module.Setup<PriceActionLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<PriceActionChart> cut = Render<PriceActionChart>(ps => ps
            .Add(p => p.Prices, _prices)
            .Add(p => p.ChartKind, PriceActionKind.Kagi)
            .Add(p => p.Reversal, 15)
            .Add(p => p.ReversalPercent, true));

        cut.WaitForAssertion(() => cut.FindAll("line[stroke-width='3']").Count.ShouldBe(1));
        cut.Find("line[stroke-width='3']").GetAttribute("stroke").ShouldBe("var(--color-success)");
        cut.Find("line[stroke-width='1.25']").GetAttribute("stroke").ShouldBe("var(--color-error)");
        cut.Markup.ShouldContain("Line 1, rising: 10 → 13; a – b");
        cut.Markup.ShouldContain("reversal 15 %");
        string legend = cut.Find("svg + div").TextContent;
        legend.ShouldContain("Yang (thick)");
        legend.ShouldContain("Yin (thin)");
        Spec(module).ShouldContain("\"kind\":\"kagi\",\"boxSize\":null,\"reversal\":15,\"reversalPercent\":true");
    }

    [Fact]
    public void Point_and_figure_draws_x_and_o_marks()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        PriceActionLayout layout = new("pointandfigure", 1, 3,
            [
                new(1, true, 10, 12, "a", "b", 100, 20, 200, 160) { Marks = [200, 180, 160] },
                new(2, false, 11, 9, "b", "c", 130, 20, 180, 220) { Marks = [220, 200, 180] },
            ],
            [new(100, "1"), new(130, "2")], [], _plot);
        module.Setup<PriceActionLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<PriceActionChart> cut = Render<PriceActionChart>(ps => ps
            .Add(p => p.Prices, _prices)
            .Add(p => p.ChartKind, PriceActionKind.PointAndFigure)
            .Add(p => p.BoxSize, 1));

        cut.WaitForAssertion(() => cut.FindAll("svg g circle").Count.ShouldBe(3));
        cut.FindAll("svg g path").Count.ShouldBe(3); // one X per box
        cut.Markup.ShouldContain("Column 1, rising: 10 → 12 (3 X); a – b");
        cut.Markup.ShouldContain("Column 2, falling: 11 → 9 (3 O); b – c");
        cut.Markup.ShouldContain("box 1, 3-box reversal");
        cut.Find("svg + div").TextContent.ShouldContain("Rising (X)");
    }

    [Fact]
    public void Line_break_with_no_prices_still_asks_for_a_layout()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<PriceActionLayout>("layout", _ => true).SetResult(new PriceActionLayout("linebreak", 0, 2, [], [], [], _plot));

        IRenderedComponent<PriceActionChart> cut = Render<PriceActionChart>(ps => ps
            .Add(p => p.ChartKind, PriceActionKind.LineBreak)
            .Add(p => p.Lines, 2));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("No data."));
        Spec(module).ShouldContain("\"kind\":\"linebreak\"");
        Spec(module).ShouldContain("\"lines\":2");
    }
}
