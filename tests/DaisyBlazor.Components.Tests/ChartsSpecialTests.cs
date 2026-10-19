using AngleSharp.Dom;
using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The specialised charts (Smith chart, 3D columns, choropleth map): each shows a placeholder, sends
/// its inputs to its D3 layout, draws the returned geometry in theme colours, and draws nothing for
/// an empty layout.
/// </summary>
public class ChartsSpecialTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private (BunitJSModuleInterop Module, JSRuntimeInvocationHandler<TLayout> Call) Setup<TLayout>()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        return (module, module.Setup<TLayout>("layout", _ => true));
    }

    private static (string Kind, string Spec) Sent(BunitJSModuleInterop module)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        return ((string)invocation.Arguments[0]!, JsonSerializer.Serialize(invocation.Arguments[1], _json));
    }

    private static SmithLayout EmptySmith => new(320, 320, 290, [], [], [], []);

    [Fact]
    public void Smith_sends_normalized_impedances_and_draws_markers_with_gamma()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<SmithLayout> call) = Setup<SmithLayout>();
        SmithSeries[] series = [new("Antenna", [new(1, 0, "2.40 GHz"), new(1, 1, "2.45 GHz")])];

        IRenderedComponent<SmithChart> cut = Render<SmithChart>(ps => ps.Add(p => p.Series, series).Add(p => p.Mode, SmithChartMode.Admittance));
        cut.Find("[aria-busy=true]").ShouldNotBeNull();

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("smith");
        spec.ShouldContain("\"points\":[{\"r\":1,\"x\":0},{\"r\":1,\"x\":1}]");
        spec.ShouldContain("\"admittance\":true");
        spec.ShouldContain("\"height\":640");

        call.SetResult(new SmithLayout(320, 320, 290,
            [new(1, 465, 320, 145)],
            [new(1, "M610,320A290,290,0,0,1,320,30")],
            [new("1", 322, 316, "start"), new("+j1", 320, 24, "middle")],
            [new([new(0, 320, 320, 0, 0), new(1, 378, 204, 0.4472, 63.43)], "M320,320L378,204")]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[r='4.5']").Count.ShouldBe(2));
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Smith chart (admittance)");
        cut.Markup.ShouldContain("2.45 GHz: z = 1 + j1, |Γ| = 0.447 ∠ 63.4°");
        cut.Markup.ShouldContain("+j1");
        cut.Markup.ShouldContain("var(--color-primary)");
        cut.FindAll("svg path[stroke='var(--color-primary)']").Count.ShouldBe(1);
    }

    [Fact]
    public void Smith_without_series_draws_the_grid_only()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<SmithLayout> call) = Setup<SmithLayout>();
        IRenderedComponent<SmithChart> cut = Render<SmithChart>();

        Sent(module).Spec.ShouldContain("\"series\":[]");
        call.SetResult(EmptySmith);

        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg circle").Count.ShouldBe(1);
        cut.FindAll("svg title").Count.ShouldBe(1);
    }

    [Fact]
    public void Column3d_sends_series_and_shades_each_face_from_the_series_colour()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<Column3DLayout> call) = Setup<Column3DLayout>();
        ChartSeries[] series = [new("Free", [120, 140]), new("Pro", [40, 55], "var(--color-accent)")];

        IRenderedComponent<Column3DChart> cut = Render<Column3DChart>(ps => ps
            .Add(p => p.Series, series).Add(p => p.Labels, ["Q1", "Q2"]).Add(p => p.Stacked, true).Add(p => p.Depth, 0.8));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("column3d");
        spec.ShouldContain("\"categories\":[\"Q1\",\"Q2\"]");
        spec.ShouldContain("\"series\":[[120,140],[40,55]]");
        spec.ShouldContain("\"stacked\":true");
        spec.ShouldContain("\"depth\":0.8");

        call.SetResult(new Column3DLayout(new(40, 30, 600, 330),
            [new(150, "Q1", null, "Q1"), new(450, "Q2", null, "Q2")], [new(330, "0", 0), new(30, "200", 200)],
            "40,330 600,330 620,318 60,318", "60,18 620,18 620,318 60,318",
            [new(0, "M40,330L60,318L620,318"), new(200, "M40,30L60,18L620,18")],
            [
                new(0, 0, 120, 100, 150, 100, 180, "a", "b", "c"),
                new(0, 1, 40, 100, 90, 100, 60, "d", "e", "f"),
            ]));

        cut.WaitForAssertion(() => cut.FindAll("svg g > polygon").Count.ShouldBe(6));
        cut.Markup.ShouldContain("color-mix(in oklab, var(--color-primary) 68%, black)");
        cut.Markup.ShouldContain("color-mix(in oklab, var(--color-accent) 72%, white)");
        cut.Markup.ShouldContain("Pro · Q1: 40");
        cut.Markup.ShouldContain("Q2");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("3D column chart");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2);
    }

    [Fact]
    public void Column3d_with_no_data_draws_no_column()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<Column3DLayout> call) = Setup<Column3DLayout>();
        IRenderedComponent<Column3DChart> cut = Render<Column3DChart>(ps => ps.Add(p => p.Horizontal, true));

        Sent(module).Spec.ShouldContain("\"horizontal\":true");
        call.SetResult(new Column3DLayout(new(8, 8, 630, 340), [], [], "", "", [], []));

        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg polygon").Count.ShouldBe(0);
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("3D bar chart");
    }

    private const string Geo = """
        {"type":"FeatureCollection","features":[
          {"type":"Feature","id":"250","properties":{"name":"France","iso":"FRA","pop_est":67000000},"geometry":{"type":"Polygon","coordinates":[[[0,0],[0,1],[1,1],[0,0]]]}},
          {"type":"Feature","id":76,"properties":{"name":"Brazil","iso":"BRA"},"geometry":{"type":"Polygon","coordinates":[[[2,0],[2,1],[3,1],[2,0]]]}},
          {"type":"Feature","properties":{"name":"Nowhere"},"geometry":null}
        ]}
        """;

    [Fact]
    public void Choropleth_sends_only_keys_names_and_geometry()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChoroplethLayout> call) = Setup<ChoroplethLayout>();
        using JsonDocument doc = JsonDocument.Parse(Geo);

        IRenderedComponent<ChoroplethMap> cut = Render<ChoroplethMap>(ps => ps
            .Add(p => p.Features, doc.RootElement).Add(p => p.Values, new Dictionary<string, double> { ["FRA"] = 1200 })
            .Add(p => p.IdProperty, "iso").Add(p => p.Projection, MapProjection.EqualEarth).Add(p => p.ValueFormat, "{0:N0} customers"));
        cut.Find("[aria-busy=true]").ShouldNotBeNull();

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("choropleth");
        spec.ShouldContain("\"features\":[{\"id\":\"FRA\",\"name\":\"France\",\"geometry\":{\"type\":\"Polygon\"");
        spec.ShouldContain("{\"id\":\"BRA\",\"name\":\"Brazil\"");
        spec.ShouldNotContain("pop_est");
        spec.ShouldNotContain("Nowhere");
        spec.ShouldContain("\"values\":{\"FRA\":1200}");
        spec.ShouldContain("\"projection\":\"equal-earth\"");
        spec.ShouldContain("\"color\":\"var(--color-primary)\"");
        spec.ShouldContain("\"noDataColor\":\"url(#nodata-");

        call.SetResult(new ChoroplethLayout("", 1200, 1200, "low", "high",
            [new("FRA", "France", 1200, "M0,0L1,1Z", "high"), new("BRA", "Brazil", null, "M2,0L3,1Z", "var(--color-base-300)")]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.Markup.ShouldContain("France: 1,200 customers");
        cut.Markup.ShouldContain("Brazil: no data");
        cut.Markup.ShouldContain("linear-gradient(to right, low, high)");
        cut.Markup.ShouldContain("No data");
    }

    [Fact]
    public void Choropleth_keys_by_the_feature_id_by_default_and_reextracts_only_for_new_geojson()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChoroplethLayout> call) = Setup<ChoroplethLayout>();
        using JsonDocument doc = JsonDocument.Parse(Geo);

        IRenderedComponent<ChoroplethMap> cut = Render<ChoroplethMap>(ps => ps.Add(p => p.Features, doc.RootElement));
        string spec = Sent(module).Spec;
        spec.ShouldContain("{\"id\":\"250\",\"name\":\"France\"");
        spec.ShouldContain("{\"id\":\"76\",\"name\":\"Brazil\"");
        call.SetResult(new ChoroplethLayout("", null, null, "low", "high", []));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        // Same document, new values: one more layout call with the new values.
        cut.Render(ps => ps.Add(p => p.Values, new Dictionary<string, double> { ["250"] = 3 }));
        cut.WaitForAssertion(() => module.Invocations["layout"].Count.ShouldBe(2));
        JsonSerializer.Serialize(module.Invocations["layout"][1].Arguments[1], _json).ShouldContain("\"values\":{\"250\":3}");
    }

    // Counts the specs built: the map's spec holds all the geometry, so it is only rebuilt for new inputs.
    private sealed class CountingMap : ChoroplethMap
    {
        public int Specs { get; private set; }

        protected override object BuildSpec()
        {
            Specs++;
            return base.BuildSpec();
        }
    }

    [Fact]
    public void Choropleth_builds_no_spec_when_its_inputs_are_the_same_references()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChoroplethLayout> call) = Setup<ChoroplethLayout>();
        using JsonDocument doc = JsonDocument.Parse(Geo);
        Dictionary<string, double> values = new() { ["250"] = 3 };

        IRenderedComponent<CountingMap> cut = Render<CountingMap>(ps => ps.Add(p => p.Features, doc.RootElement).Add(p => p.Values, values));
        call.SetResult(new ChoroplethLayout("", null, null, "low", "high", []));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.Instance.Specs.ShouldBe(1);

        // The parent renders again with the same Features and Values: nothing to build or serialize.
        cut.Render(ps => ps.Add(p => p.Features, doc.RootElement).Add(p => p.Values, values).Add(p => p.ValueFormat, "{0:N0} customers"));
        cut.Instance.Specs.ShouldBe(1);
        module.Invocations["layout"].Count.ShouldBe(1);

        // New Values (or any input of the spec): built and laid out again.
        cut.Render(ps => ps.Add(p => p.Values, new Dictionary<string, double> { ["250"] = 4 }));
        cut.Instance.Specs.ShouldBe(2);
        cut.WaitForAssertion(() => module.Invocations["layout"].Count.ShouldBe(2));
        cut.Render(ps => ps.Add(p => p.Projection, MapProjection.Mercator));
        cut.Instance.Specs.ShouldBe(3);
    }

    [Fact]
    public void Choropleth_without_features_draws_the_globe_and_no_legend()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChoroplethLayout> call) = Setup<ChoroplethLayout>();
        IRenderedComponent<ChoroplethMap> cut = Render<ChoroplethMap>();

        Sent(module).Spec.ShouldContain("\"features\":[]");
        call.SetResult(new ChoroplethLayout("M0,0L10,10Z", null, null, "low", "high", []));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(1));
        cut.Markup.ShouldNotContain("linear-gradient");
    }

    [Fact]
    public void Polar_sends_angles_radii_and_formatted_spokes_and_draws_markers_with_tooltips()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarLayout> call) = Setup<PolarLayout>();
        PolarSeries[] series =
        [
            new("p95 latency", [new(0, 120), new(12, 180, "Peak")]),
            new("p50 latency", [new(0, 40), new(12, 60)], "var(--color-info)"),
        ];

        IRenderedComponent<PolarChart> cut = Render<PolarChart>(ps => ps
            .Add(p => p.Series, series)
            .Add(p => p.AngleMax, 24)
            .Add(p => p.AngleStep, 6)
            .Add(p => p.AngleFormat, "{0:00}:00")
            .Add(p => p.ValueFormat, "{0} ms")
            .Add(p => p.Mode, PolarChartMode.Area)
            .Add(p => p.Smooth, true));
        cut.Find("[aria-busy=true]").ShouldNotBeNull();

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("polar");
        spec.ShouldContain("\"points\":[{\"a\":0,\"r\":120},{\"a\":12,\"r\":180}]");
        spec.ShouldContain("\"angleMax\":24");
        spec.ShouldContain("\"angleTicks\":[{\"value\":0,\"label\":\"00:00\"},{\"value\":6,\"label\":\"06:00\"},{\"value\":12,\"label\":\"12:00\"},{\"value\":18,\"label\":\"18:00\"}]");
        spec.ShouldContain("\"mode\":\"area\",\"smooth\":true,\"closed\":false");

        call.SetResult(new PolarLayout(320, 320, 290,
            [new(100, "100", 145, 323, 172), new(200, "200", 290, 323, 27)],
            [new(0, "00:00", 320, 30, 320, 22, "middle"), new(12, "12:00", 320, 610, 320, 627, "middle")],
            [new("M320,146Z", [new(0, 320, 146), new(1, 320, 581)]), new("M320,262Z", [new(0, 320, 262), new(1, 320, 407)])]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[r='3']").Count.ShouldBe(4));
        cut.FindAll("svg path[fill='var(--color-info)']").Count.ShouldBe(1);
        cut.Markup.ShouldContain("p95 latency · Peak, 12:00: 180 ms");
        cut.Markup.ShouldContain("12:00");
        cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 640");
        cut.FindAll("span.inline-flex").Count.ShouldBe(2);
    }

    [Fact]
    public void Polar_marker_mode_draws_no_path()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarLayout> call) = Setup<PolarLayout>();
        IRenderedComponent<PolarChart> cut = Render<PolarChart>(ps => ps
            .Add(p => p.Series, [new PolarSeries("Regions", [new(45, 3), new(90, double.NaN)])])
            .Add(p => p.Mode, PolarChartMode.Marker));

        string spec = Sent(module).Spec;
        spec.ShouldContain("{\"a\":90,\"r\":null}");
        spec.ShouldContain("\"angleTicks\":[{\"value\":0,\"label\":\"0\"},{\"value\":30,");
        call.SetResult(new PolarLayout(320, 320, 290, [], [], [new(string.Empty, [new(0, 400, 240)])]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[r='4.5']").Count.ShouldBe(1));
        cut.FindAll("svg path").Count.ShouldBe(0);
        cut.FindAll("span.inline-flex").Count.ShouldBe(0);
    }

    [Fact]
    public void Polar_stacked_sends_the_flags_and_adds_the_total_and_share_to_tooltips()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarLayout> call) = Setup<PolarLayout>();
        PolarSeries[] series =
        [
            new("Free", [new(0, 30), new(90, 10)]),
            new("Pro", [new(0, 10), new(90, 30)]),
        ];

        IRenderedComponent<PolarChart> cut = Render<PolarChart>(ps => ps
            .Add(p => p.Series, series)
            .Add(p => p.Percent, true)
            .Add(p => p.Mode, PolarChartMode.Area));

        Sent(module).Spec.ShouldEndWith("\"levels\":4,\"stacked\":true,\"percent\":true}");
        call.SetResult(new PolarLayout(320, 320, 290, [], [],
            [new("M0,0Z", [new(0, 320, 100), new(1, 400, 320)]), new("M0,0ZM1,1Z", [new(0, 320, 30), new(1, 610, 320)])]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle[r='3']").Count.ShouldBe(4));
        cut.Markup.ShouldContain("Free, 0: 30 (total 40, 75%)");
        cut.Markup.ShouldContain("Pro, 90: 30 (total 40, 75%)");
    }

    [Fact]
    public void Polar_unstacked_sends_no_stack_flags()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarLayout> _) = Setup<PolarLayout>();
        Render<PolarChart>(ps => ps.Add(p => p.Series, [new PolarSeries("Free", [new(0, 30)])]));

        string spec = Sent(module).Spec;
        spec.ShouldEndWith("\"levels\":4}");
        spec.ShouldNotContain("stacked");
    }

    [Fact]
    public void MultiPie_draws_one_titled_pie_per_group_with_shared_colours_and_one_legend()
    {
        JSInterop.SetupCharts();
        PieGroup[] groups =
        [
            new("Europe", [new("Free", 50), new("Pro", 30), new("Team", 20)]),
            new("Americas", [new("Pro", 40), new("Free", 45), new("Enterprise", 15, "var(--color-accent)")]),
            new("Asia Pacific", [new("Team", 25), new("Free", 75)]),
        ];

        IRenderedComponent<MultiPieChart> cut = Render<MultiPieChart>(ps => ps
            .Add(p => p.Groups, groups)
            .Add(p => p.Title, "Plan mix by region")
            .Add(p => p.Columns, 3)
            .Add(p => p.InnerRadiusRatio, 0.5)
            .Add(p => p.ShowPercent, false));

        IReadOnlyList<IRenderedComponent<PieChart>> pies = cut.FindComponents<PieChart>();
        pies.Count.ShouldBe(3);
        cut.FindAll("svg").Select(s => s.GetAttribute("aria-label")).ShouldBe(["Europe", "Americas", "Asia Pacific"]);
        cut.FindAll("svg desc").Select(d => d.Id).Distinct().Count().ShouldBe(3);
        cut.Find("[role=group]").GetAttribute("aria-label").ShouldBe("Plan mix by region");
        cut.Find(".grid").GetAttribute("style").ShouldBe("grid-template-columns:repeat(3,minmax(0,1fr));");

        // "Free" is the first label everywhere it appears, whatever its place in the group.
        string Color(int pie, string label) => pies[pie].Instance.Data.Single(d => d.Label == label).Color!;
        Color(0, "Free").ShouldBe(Color(1, "Free"));
        Color(0, "Free").ShouldBe(Color(2, "Free"));
        Color(0, "Pro").ShouldBe(Color(1, "Pro"));
        Color(0, "Team").ShouldBe(Color(2, "Team"));
        Color(1, "Enterprise").ShouldBe("var(--color-accent)");
        pies.ShouldAllBe(p => p.Instance.InnerRadiusRatio == 0.5 && !p.Instance.ShowPercent && !p.Instance.ShowLegend);

        cut.FindAll("ul").Count.ShouldBe(0);
        cut.FindAll("span.inline-flex").Select(s => s.TextContent.Trim()).ShouldBe(["Free", "Pro", "Team", "Enterprise"]);
        cut.FindAll("path title")[0].TextContent.ShouldBe("Free: 50");
    }

    [Fact]
    public void MultiPie_fills_the_width_when_no_columns_are_given()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<MultiPieChart> cut = Render<MultiPieChart>(ps => ps
            .Add(p => p.Groups, [new PieGroup("Europe", [new("Free", 1)])])
            .Add(p => p.ShowLegend, false));

        cut.Find(".grid").GetAttribute("style").ShouldBe("grid-template-columns:repeat(auto-fill,minmax(min(12rem,100%),1fr));");
        cut.FindAll("span.inline-flex").Count.ShouldBe(0);
        cut.Find("[role=group]").GetAttribute("aria-label").ShouldBe("Pie charts");
    }

    [Fact]
    public void Shape_sends_rings_and_colours_shapes_by_group()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ShapeLayout> call) = Setup<ShapeLayout>();
        ShapePoint[] rack = [new(0, 0), new(2, 0), new(2, 1), new(0, 1)];
        ChartShape[] shapes =
        [
            new("Rack A1", [rack], Group: "Full"),
            new("Rack A2", [rack], Group: "Free"),
            new("Rack A3", [rack], Group: "Full"),
            new("Fibre run", [[new(0, 2), new(6, 2)]], Closed: false, Color: "var(--color-accent)"),
        ];

        IRenderedComponent<ShapeChart> cut = Render<ShapeChart>(ps => ps
            .Add(p => p.Shapes, shapes)
            .Add(p => p.KeepAspect, true)
            .Add(p => p.XTitle, "Metres"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("shape");
        spec.ShouldContain("\"rings\":[[{\"x\":0,\"y\":0},{\"x\":2,\"y\":0},{\"x\":2,\"y\":1},{\"x\":0,\"y\":1}]],\"closed\":true");
        spec.ShouldContain("\"closed\":false");
        spec.ShouldContain("\"keepAspect\":true");

        call.SetResult(new ShapeLayout(new PlotArea(30, 10, 634, 330),
            [new(30, "0", 0), new(634, "6", 6)], [new(330, "0", 0), new(10, "2", 2)],
            ["M30,330L230,330Z", "M30,300L230,300Z", "M30,270L230,270Z", "M30,10L634,10"]));

        cut.WaitForAssertion(() => cut.FindAll("path.dbc-shape").Count.ShouldBe(3));
        IReadOnlyList<AngleSharp.Dom.IElement> polygons = cut.FindAll("path.dbc-shape");
        polygons[0].GetAttribute("fill").ShouldBe("var(--color-primary)");
        polygons[1].GetAttribute("fill").ShouldBe("var(--color-secondary)");
        polygons[2].GetAttribute("fill").ShouldBe("var(--color-primary)");
        polygons[0].GetAttribute("fill-rule").ShouldBe("evenodd");
        AngleSharp.Dom.IElement cable = cut.Find("path.dbc-line");
        cable.GetAttribute("fill").ShouldBe("none");
        cable.GetAttribute("stroke").ShouldBe("var(--color-accent)");
        cut.Markup.ShouldContain("Rack A2 (Free)");
        cut.Markup.ShouldContain("Metres");
        cut.FindAll("span.inline-flex").Count.ShouldBe(3);

        // The group colours are worked out per parameters set: regrouping (same geometry, same layout) recolours.
        ChartShape[] regrouped = [shapes[0] with { Group = "Free" }, shapes[1] with { Group = "Full" }, shapes[2], shapes[3]];
        cut.Render(ps => ps.Add(p => p.Shapes, regrouped));
        polygons = cut.FindAll("path.dbc-shape");
        polygons[0].GetAttribute("fill").ShouldBe("var(--color-primary)");
        polygons[1].GetAttribute("fill").ShouldBe("var(--color-secondary)");
        polygons[2].GetAttribute("fill").ShouldBe("var(--color-secondary)");
        cut.Find("span.inline-flex").TextContent.ShouldContain("Free");
    }

    [Fact]
    public void Shape_without_shapes_draws_axes_only()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ShapeLayout> call) = Setup<ShapeLayout>();
        IRenderedComponent<ShapeChart> cut = Render<ShapeChart>();

        Sent(module).Spec.ShouldContain("\"shapes\":[]");
        call.SetResult(new ShapeLayout(new PlotArea(30, 10, 634, 330), [], [], []));

        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg path").Count.ShouldBe(0);
        cut.Find("desc").TextContent.ShouldBe("No shapes.");
    }
}
