using System.Text.Json;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Regressions from the visual QA pass of the docs site: hover tooltips on lines without markers,
/// contours kept inside the plot, label text sent to the layouts that reserve its room, chart colours
/// that stay visible in dark themes, a distinct no-data fill and the axis minus sign on value labels.
/// </summary>
public class ChartsQaFixTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private (BunitJSModuleInterop Module, JSRuntimeInvocationHandler<TLayout> Call) Setup<TLayout>()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        return (module, module.Setup<TLayout>("layout", _ => true));
    }

    private static string Spec(BunitJSModuleInterop module) =>
        JsonSerializer.Serialize(module.Invocations["layout"].Last().Arguments[1], _json);

    [Fact]
    public void Line_without_markers_gives_every_point_a_hover_target_with_its_tooltip()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<AreaChart> cut = Render<AreaChart>(ps => ps
            .Add(p => p.Series, [new ChartSeries("Visits", [100, 50])])
            .Add(p => p.Labels, ["Jan", "Feb"])
            .Add(p => p.ShowPoints, false));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[fill='transparent']").Count.ShouldBe(2));
        cut.FindAll("svg circle[fill='transparent'] title").Select(t => t.TextContent).ShouldBe(["Jan: 100", "Feb: 50"]);
    }

    [Fact]
    public void Polar_without_dots_still_titles_every_point()
    {
        (_, JSRuntimeInvocationHandler<PolarLayout> call) = Setup<PolarLayout>();
        IRenderedComponent<PolarChart> cut = Render<PolarChart>(ps => ps
            .Add(p => p.Series, [new PolarSeries("p95", [new(0, 120), new(12, 180)])])
            .Add(p => p.AngleMax, 24).Add(p => p.ShowPoints, false).Add(p => p.Mode, PolarChartMode.Area));
        call.SetResult(new PolarLayout(320, 320, 290,
            [new(100, "100", 145, 323, 172), new(200, "", 290, 323, 27)], [],
            [new("M320,146Z", [new(0, 320, 146), new(1, 320, 581)])]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[fill='transparent'] title").Count.ShouldBe(2));
        cut.Markup.ShouldContain("p95, 12: 180");
        cut.FindAll("svg text").Select(t => t.TextContent).ShouldBe(["100"], "the rim ring has no label");
    }

    [Fact]
    public void Contour_bands_are_clipped_to_the_plot()
    {
        (_, JSRuntimeInvocationHandler<ContourLayout> call) = Setup<ContourLayout>();
        IRenderedComponent<ContourChart> cut = Render<ContourChart>(ps => ps.Add(p => p.Points, [new ScatterPoint(1, 2), new ScatterPoint(2, 3)]));
        call.SetResult(new ContourLayout(new PlotArea(40, 26, 624, 318), [], [], [new(0.001, "M-12,0L5,300Z")], []));

        cut.WaitForAssertion(() => cut.FindAll("svg clipPath rect").Count.ShouldBe(1));
        string id = cut.Find("svg clipPath").Id!;
        cut.Find("svg clipPath rect").GetAttribute("width").ShouldBe("584");
        cut.Find($"svg g[clip-path='url(#{id})'] path").ShouldNotBeNull();
    }

    [Fact]
    public void Gauge_sends_its_formatted_tick_labels_so_the_layout_can_make_room()
    {
        (BunitJSModuleInterop module, _) = Setup<GaugeLayout>();
        Render<GaugeChart>(ps => ps.Add(p => p.Value, 99.95).Add(p => p.Min, 99).Add(p => p.Max, 100).Add(p => p.ValueFormat, "{0:0.0#}%"));

        string spec = Spec(module);
        spec.ShouldContain("\"tickText\":[\"99.0%\",\"99.05%\"");
        spec.ShouldContain("\"100.0%\"]");
    }

    [Fact]
    public void Polar_area_sends_each_label_as_printed()
    {
        (BunitJSModuleInterop module, _) = Setup<PolarAreaLayout>();
        Render<PolarAreaChart>(ps => ps.Add(p => p.Data, [new ChartDataPoint("Notifications", 27)]));
        Spec(module).ShouldContain("{\"label\":\"Notifications\",\"value\":27,\"text\":\"Notifications 27\"}");

        (BunitJSModuleInterop rose, _) = Setup<PolarAreaLayout>();
        Render<PolarAreaChart>(ps => ps.Add(p => p.Labels, ["N", "E"])
            .Add(p => p.Series, [new ChartSeries("Calm", [1, 2]), new ChartSeries("Gale", [3, double.NaN])]).Add(p => p.ShowValues, true));
        Spec(rose).ShouldContain("\"texts\":[\"N 4\",\"E 2\"]");
    }

    [Theory]
    [InlineData(Color.Dark)]
    [InlineData(Color.Surface)]
    public void Dark_and_surface_chart_colours_mix_the_base_content_so_they_show_in_every_theme(Color color)
    {
        string css = ChartPalette.FromColor(color)!;
        css.ShouldNotContain("--color-neutral");
        css.ShouldStartWith("color-mix(in oklab, var(--color-base-content) ");
        css.ShouldEndWith(", var(--color-base-100))");
        ChartPalette.Neutral.ShouldBe(ChartPalette.FromColor(Color.Dark));
    }

    private const string Geo = """
        {"type":"FeatureCollection","features":[
          {"type":"Feature","id":"FRA","properties":{"name":"France"},"geometry":{"type":"Polygon","coordinates":[[[0,0],[0,1],[1,1],[0,0]]]}}
        ]}
        """;

    [Fact]
    public void Choropleth_hatches_features_without_data_and_its_legend_matches()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChoroplethLayout> call) = Setup<ChoroplethLayout>();
        using JsonDocument doc = JsonDocument.Parse(Geo);
        IRenderedComponent<ChoroplethMap> cut = Render<ChoroplethMap>(ps => ps.Add(p => p.Features, doc.RootElement));

        string spec = Spec(module);
        spec.ShouldMatch("\"noDataColor\":\"url\\(#nodata-[0-9a-f]{8}\\)\"");
        string fill = JsonDocument.Parse(spec).RootElement.GetProperty("noDataColor").GetString()!;
        call.SetResult(new ChoroplethLayout("", 5, 5, "low", "high", [new("FRA", "France", null, "M0,0L1,1Z", fill)]));

        cut.WaitForAssertion(() => cut.FindAll("svg pattern").Count.ShouldBe(1));
        fill.ShouldBe($"url(#{cut.Find("svg pattern").Id})");
        cut.Markup.ShouldContain("repeating-linear-gradient(");
        cut.Markup.ShouldContain("No data");
    }

    [Fact]
    public void Lollipop_value_labels_use_the_axis_minus_sign()
    {
        (BunitJSModuleInterop module, _) = Setup<LollipopLayout>();
        Render<LollipopChart>(ps => ps.Add(p => p.Data, [new ChartDataPoint("Churn", -8), new ChartDataPoint("Growth", 12)]));
        string spec = Spec(module);
        spec.ShouldContain("\"text\":\"\\u22128\"");
        spec.ShouldContain("\"text\":\"12\"");
    }
}
