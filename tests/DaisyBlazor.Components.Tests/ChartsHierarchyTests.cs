using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The "Hierarchy &amp; flow" D3 charts: each sends its spec to its own layout, shows a placeholder
/// until the layout arrives, then draws what comes back in theme colours, and copes with an empty layout.
/// </summary>
public class ChartsHierarchyTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _web = new(JsonSerializerDefaults.Web);

    private static readonly HierarchyNode _root = new("Storage", Children:
    [
        new("Platform", Children: [new("Logs", 40), new("Backups", 20)]),
        new("Data", 30),
    ]);

    private static readonly GraphLink[] _links = [new("Gateway", "Auth", 4), new("Gateway", "Projects", 2)];

    // Names: the node's place below the root, as the layout gives it; the tree's own root has none.
    private static HierarchyLayoutNode _rect(string name, string path, int depth, int group, bool leaf, double value, double x0, double y0, double x1, double y1, string? d = null, string[]? names = null) =>
        new(name, path, depth, group, leaf, value, x0, y0, x1, y1, d, x0 + 5, y0 + 14, 0, x1 - x0 - 10, y1 - y0)
        { Names = names ?? (group < 0 ? [] : path.Split(" / ")) };

    private static readonly HierarchyLayout _hierarchy = new(
        [
            _rect("Storage", "Storage", 0, -1, false, 90, 0, 0, 640, 360, "M0,-60A60,60,0,1,1,0,60A60,60,0,1,1,0,-60Z"),
            _rect("Platform", "Platform", 1, 0, false, 60, 0, 0, 400, 360, "M0,-120A120,120,0,1,1,0,120L0,60A60,60,0,1,0,0,-60Z"),
            _rect("Logs", "Platform / Logs", 2, 0, true, 40, 0, 18, 400, 240, "M0,-180L0,-120Z"),
            _rect("Backups", "Platform / Backups", 2, 0, true, 20, 0, 240, 400, 360, "M0,-180L0,-120Z"),
            _rect("Data", "Data", 1, 1, true, 30, 400, 0, 640, 360, "M0,120L0,60Z"),
        ],
        ["Platform", "Data"], 320, 180);

    private static readonly HierarchyLayout _emptyHierarchy = new([], [], 0, 0);

    private (BunitJSModuleInterop Module, JSRuntimeInvocationHandler<T> Call) _setup<T>()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        return (module, module.Setup<T>("layout", _ => true));
    }

    private static string _spec(BunitJSModuleInterop module, string kind)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe(kind);
        return JsonSerializer.Serialize(invocation.Arguments[1], _web);
    }

    private void _hierarchyChart<TChart>(string kind, Func<IRenderedComponent<TChart>> render, string specFragment, Action<IRenderedComponent<TChart>> drawn)
        where TChart : IComponent
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<HierarchyLayout> call) = _setup<HierarchyLayout>();

        IRenderedComponent<TChart> cut = render();

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        string spec = _spec(module, kind);
        spec.ShouldContain("\"name\":\"Logs\",\"value\":40");
        spec.ShouldContain("\"width\":640");
        spec.ShouldContain(specFragment);

        call.SetResult(_hierarchy);
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Storage");
        cut.Find("svg desc").TextContent.ShouldBe("Platform: 60; Data: 30");
        cut.Markup.ShouldContain("Platform / Logs: 40");
        cut.Markup.ShouldContain("var(--color-primary)");
        cut.Markup.ShouldContain("var(--color-secondary)");
        cut.Markup.ShouldNotContain("#");
        drawn(cut);
    }

    [Fact]
    public void Treemap_draws_frames_for_parents_and_tiles_for_leaves() =>
        _hierarchyChart("treemap", () => Render<TreemapChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Tile, TreemapTile.Binary)), "\"tile\":\"binary\"", cut =>
        {
            cut.FindAll("svg rect").Count.ShouldBe(5);
            cut.Markup.ShouldContain("Platform 60");
            cut.Markup.ShouldContain("var(--color-primary-content)");
        });

    [Fact]
    public void Icicle_draws_every_node_including_the_root() =>
        _hierarchyChart("icicle", () => Render<IcicleChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Horizontal, false)), "\"orientation\":\"vertical\"", cut =>
        {
            cut.FindAll("svg rect").Count.ShouldBe(5);
            cut.Markup.ShouldContain(ChartPalette.Neutral);
        });

    [Fact]
    public void Circle_packing_draws_circles_and_a_legend() =>
        _hierarchyChart("circle-packing", () => Render<CirclePackingChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Padding, 6)), "\"padding\":6", cut =>
        {
            cut.FindAll("svg circle").Count.ShouldBe(5);
            cut.FindAll(".rounded-sm").Count.ShouldBe(2, "one legend key per top-level branch");
        });

    [Fact]
    public void Sunburst_draws_arcs_around_its_centre() =>
        _hierarchyChart("sunburst", () => Render<SunburstChart>(ps => ps.Add(p => p.Root, _root)), "\"height\":360", cut =>
        {
            cut.FindAll("svg path").Count.ShouldBe(5);
            cut.Find("svg g").GetAttribute("transform").ShouldBe("translate(320,180)");
            cut.Markup.ShouldContain("rotate(");
        });

    [Fact]
    public void Hierarchy_charts_with_no_data_draw_an_empty_chart()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<HierarchyLayout> call) = _setup<HierarchyLayout>();
        call.SetResult(_emptyHierarchy);

        IRenderedComponent<TreemapChart> treemap = Render<TreemapChart>();
        IRenderedComponent<SunburstChart> sunburst = Render<SunburstChart>(ps => ps.Add(p => p.Root, new HierarchyNode("Nothing", -1)));

        treemap.WaitForAssertion(() => treemap.FindAll("svg").Count.ShouldBe(1));
        treemap.FindAll("svg rect").Count.ShouldBe(0);
        treemap.Find("svg").GetAttribute("aria-label").ShouldBe("Treemap");
        sunburst.WaitForAssertion(() => sunburst.FindAll("svg").Count.ShouldBe(1));
        sunburst.FindAll("svg path").Count.ShouldBe(0);
        sunburst.FindAll(".rounded-sm").Count.ShouldBe(0);
        module.Invocations["layout"].Count.ShouldBe(2);
    }

    [Fact]
    public void Chord_draws_arcs_ribbons_and_labels()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ChordLayout> call) = _setup<ChordLayout>();
        ChordLayout layout = new(
            [new("Gateway", 6, "M0,-100Z", 110, 0, "start"), new("Auth", 4, "M0,100Z", -110, 0, "end"), new("Projects", 2, "M100,0Z", 0, 110, "middle")],
            [new(0, 1, 4, 4, "M1,1Z"), new(0, 2, 2, 2, "M2,2Z")],
            320, 180);

        IRenderedComponent<ChordChart> cut = Render<ChordChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.Directed, true).Add(p => p.ShowValues, true));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        string spec = _spec(module, "chord");
        spec.ShouldContain("\"directed\":true");
        spec.ShouldContain("\"source\":\"Gateway\"");
        spec.ShouldContain("\"labelWidth\":");

        call.SetResult(layout);
        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(5));
        cut.Markup.ShouldContain("Gateway → Auth: 4");
        cut.Markup.ShouldContain("Gateway 6");
        cut.Find("svg desc").TextContent.ShouldBe("Gateway to Auth: 4; Gateway to Projects: 2");
    }

    [Fact]
    public void Network_draws_links_nodes_labels_and_group_legend()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<GraphLayout> call) = _setup<GraphLayout>();
        GraphLayout layout = new(
            [new("Gateway", "Edge", 0, 2, 100, 100, 8), new("Auth", "Core", 1, 1, 300, 200, 7), new("Projects", null, -1, 1, 500, 100, 7)],
            [new(0, 1, 4, 5, "M100,100L300,200"), new(0, 2, 2, 3, "M100,100L500,100")],
            ["Edge", "Core"], 70);

        IRenderedComponent<NetworkChart> cut = Render<NetworkChart>(ps => ps
            .Add(p => p.Links, _links)
            .Add(p => p.Nodes, [new GraphNode("Gateway", "Edge"), new GraphNode("Auth", "Core")])
            .Add(p => p.Iterations, 50));

        string spec = _spec(module, "network");
        spec.ShouldContain("\"iterations\":50");
        spec.ShouldContain("\"group\":\"Edge\"");
        spec.ShouldContain("\"labels\":true");

        call.SetResult(layout);
        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(3));
        cut.FindAll("svg path").Count.ShouldBe(2);
        cut.FindAll("svg text").Count.ShouldBe(3);
        cut.Markup.ShouldContain("Gateway (Edge): 2 links");
        cut.Markup.ShouldContain("Gateway – Auth: 4");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2);
    }

    [Fact]
    public void Arc_diagram_draws_arcs_and_rotated_names()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<GraphLayout> call) = _setup<GraphLayout>();
        GraphLayout layout = new(
            [new("Gateway", null, -1, 2, 100, 280, 5), new("Auth", null, -1, 1, 300, 280, 4), new("Projects", null, -1, 1, 500, 280, 4)],
            [new(0, 1, 4, 6, "M100,280A100,100 0 0,1 300,280"), new(0, 2, 2, 3, "M100,280A200,200 0 0,1 500,280")],
            [], 80);

        IRenderedComponent<ArcDiagramChart> cut = Render<ArcDiagramChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.Sort, ArcDiagramSort.Degree));

        _spec(module, "arc-diagram").ShouldContain("\"sort\":\"degree\"");

        call.SetResult(layout);
        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.FindAll("svg circle").Count.ShouldBe(3);
        cut.FindAll("svg g[transform^=rotate]").Count.ShouldBe(3);
        cut.Markup.ShouldContain("var(--color-primary)");
        cut.FindAll(".rounded-sm").Count.ShouldBe(0, "no groups, no legend");
    }

    [Fact]
    public void Graph_charts_with_no_links_draw_an_empty_chart()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<GraphLayout> call) = _setup<GraphLayout>();
        call.SetResult(new GraphLayout([], [], [], 0));
        module.Setup<ChordLayout>("layout", inv => (string?)inv.Arguments[0] == "chord").SetResult(new ChordLayout([], [], 0, 0));

        IRenderedComponent<NetworkChart> network = Render<NetworkChart>();
        IRenderedComponent<ChordChart> chord = Render<ChordChart>(ps => ps.Add(p => p.Links, [new GraphLink("A", "A", 3)]));

        network.WaitForAssertion(() => network.FindAll("svg").Count.ShouldBe(1));
        network.FindAll("svg circle").Count.ShouldBe(0);
        chord.WaitForAssertion(() => chord.FindAll("svg").Count.ShouldBe(1));
        chord.FindAll("svg path").Count.ShouldBe(0);
        chord.FindAll("svg desc").Count.ShouldBe(1);
        chord.Find("svg desc").TextContent.ShouldBeEmpty();
    }

    // Drill-down: the fake layout answers the whole tree, or the Platform branch when the spec asks for it.
    private static readonly HierarchyLayout _platform = new(
        [
            _rect("Platform", "Platform", 0, 0, false, 60, 0, 0, 640, 360, "M0,-60A60,60,0,1,1,0,60A60,60,0,1,1,0,-60Z"),
            _rect("Logs", "Platform / Logs", 1, 0, true, 40, 0, 18, 640, 240, "M0,-180L0,-120Z"),
            _rect("Backups", "Platform / Backups", 1, 0, true, 20, 0, 240, 640, 360, "M0,-180L0,-120Z"),
        ],
        ["Platform", "Data"], 320, 180)
    { Focus = ["Platform"] };

    private BunitJSModuleInterop _drillSetup()
    {
        SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<HierarchyLayout>("layout", i => !_lastSpec(i).Contains("\"focus\"")).SetResult(_hierarchy);
        module.Setup<HierarchyLayout>("layout", i => _lastSpec(i).Contains("\"focus\":[\"Platform\"]")).SetResult(_platform);
        return module;
    }

    private static string _lastSpec(JSRuntimeInvocation invocation) => JsonSerializer.Serialize(invocation.Arguments[1], _web);

    [Fact]
    public void Drilldown_click_on_a_parent_re_roots_and_raises_FocusChanged()
    {
        BunitJSModuleInterop module = _drillSetup();
        List<string?> changes = [];

        IRenderedComponent<TreemapChart> cut = Render<TreemapChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true)
            .Add(p => p.FocusChanged, (string? f) => changes.Add(f)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.Find("nav[aria-label] button").GetAttribute("aria-current").ShouldBe("location");

        AngleSharp.Dom.IElement platform = cut.Find("rect[role=button]");
        platform.GetAttribute("aria-label").ShouldBe("Platform: 60");
        platform.GetAttribute("tabindex").ShouldBe("0");
        platform.Click();

        changes.ShouldBe(["Platform"]);
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"Platform\"]");
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(2));
        cut.FindAll("nav button").Select(b => b.TextContent.Trim()).ShouldBe(["Storage", "Platform"]);
        cut.FindAll("nav button")[1].GetAttribute("aria-current").ShouldBe("location");
        cut.FindAll("nav button")[0].HasAttribute("aria-current").ShouldBeFalse();
    }

    [Fact]
    public void Drilldown_breadcrumb_goes_back_to_the_root()
    {
        BunitJSModuleInterop module = _drillSetup();
        List<string?> changes = [];

        IRenderedComponent<IcicleChart> cut = Render<IcicleChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true)
            .Add(p => p.Focus, "Platform").Add(p => p.FocusChanged, (string? f) => changes.Add(f)));
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(2));

        // The drawn root (Platform) is a button too: it goes one level up.
        cut.Find("rect[role=button]").GetAttribute("aria-label").ShouldBe("Platform: 60. Back to Storage");
        cut.Find("nav button").Click();

        changes.ShouldBe([null]);
        _lastSpec(module.Invocations["layout"].Last()).ShouldNotContain("focus");
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(1));
    }

    [Fact]
    public void Drilldown_Enter_on_a_focused_node_drills_in()
    {
        _drillSetup();
        List<string?> changes = [];

        IRenderedComponent<SunburstChart> cut = Render<SunburstChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true)
            .Add(p => p.FocusChanged, (string? f) => changes.Add(f)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        cut.Find("path[role=button]").KeyDown(new KeyboardEventArgs { Key = "Tab" });
        changes.ShouldBeEmpty();
        cut.Find("path[role=button]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        changes.ShouldBe(["Platform"]);
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(2));
    }

    [Fact]
    public void Drilldown_a_treemap_leaf_opens_its_parent()
    {
        BunitJSModuleInterop module = _drillSetup();
        List<string?> changes = [];

        IRenderedComponent<TreemapChart> cut = Render<TreemapChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true)
            .Add(p => p.FocusChanged, (string? f) => changes.Add(f)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "Platform / Logs: 40").Click();

        changes.ShouldBe(["Platform"]);
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"Platform\"]");
    }

    [Fact]
    public void Drilldown_first_level_leaves_and_the_top_root_are_not_clickable_and_deeper_leaves_open_their_parent()
    {
        _drillSetup();

        IRenderedComponent<IcicleChart> cut = Render<IcicleChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        // Storage (the root, nowhere to go up to) and Data (a leaf right under it) do nothing; Logs and
        // Backups are leaves one level deeper, so they open Platform.
        cut.FindAll("[role=button]").Select(e => e.GetAttribute("aria-label")).ShouldBe(["Platform: 60", "Platform / Logs: 40", "Platform / Backups: 20"]);
        cut.Find("svg").ClassList.ShouldContain("[&_text]:pointer-events-none");
    }

    [Fact]
    public void OnNodeClick_reports_any_node_leaves_included()
    {
        _drillSetup();
        List<HierarchyNode> clicked = [];

        IRenderedComponent<TreemapChart> cut = Render<TreemapChart>(ps => ps.Add(p => p.Root, _root)
            .Add(p => p.OnNodeClick, (HierarchyNode n) => clicked.Add(n)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        cut.FindAll("rect[role=button]").Count.ShouldBe(5, "every node, leaves included");
        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "Platform / Logs: 40").Click();

        clicked.ShouldBe([_root.Children![0].Children![0]]);
        cut.FindAll("nav").Count.ShouldBe(0);
    }

    [Fact]
    public void Without_Drilldown_nothing_is_a_button()
    {
        _drillSetup();

        IRenderedComponent<SunburstChart> cut = Render<SunburstChart>(ps => ps.Add(p => p.Root, _root));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        cut.FindAll("[role=button], [tabindex], nav, button").Count.ShouldBe(0);
        cut.Find("svg").HasAttribute("class").ShouldBeFalse();
    }

    // Names holding '/' and ' / ': Ops > CI/CD > A / B > x, y; CI/CD > z.
    private static readonly HierarchyNode _slashes = new("Ops", Children:
    [
        new("CI/CD", Children: [new("A / B", Children: [new("x", 5), new("y", 3)]), new("z", 2)]),
    ]);

    private BunitJSModuleInterop _slashSetup()
    {
        SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));
        BunitJSModuleInterop module = this.SetupChartModule();
        HierarchyLayout whole = new(
            [
                _rect("CI/CD", "CI/CD", 1, 0, false, 10, 0, 0, 640, 360, names: ["CI/CD"]),
                _rect("A / B", "CI/CD / A / B", 2, 0, false, 8, 0, 18, 500, 360, names: ["CI/CD", "A / B"]),
                _rect("x", "CI/CD / A / B / x", 3, 0, true, 5, 0, 36, 300, 360, names: ["CI/CD", "A / B", "x"]),
            ],
            ["CI/CD"], 320, 180);
        HierarchyLayout cicd = new(
            [
                _rect("CI/CD", "CI/CD", 0, 0, false, 10, 0, 0, 640, 360, names: ["CI/CD"]),
                _rect("A / B", "CI/CD / A / B", 1, 0, false, 8, 0, 18, 500, 360, names: ["CI/CD", "A / B"]),
            ],
            ["CI/CD"], 320, 180)
        { Focus = ["CI/CD"] };
        HierarchyLayout ab = new(
            [
                _rect("A / B", "CI/CD / A / B", 0, 0, false, 8, 0, 0, 640, 360, names: ["CI/CD", "A / B"]),
                _rect("x", "CI/CD / A / B / x", 1, 0, true, 5, 0, 18, 400, 360, names: ["CI/CD", "A / B", "x"]),
            ],
            ["CI/CD"], 320, 180)
        { Focus = ["CI/CD", "A / B"] };
        module.Setup<HierarchyLayout>("layout", i => !_lastSpec(i).Contains("\"focus\"")).SetResult(whole);
        module.Setup<HierarchyLayout>("layout", i => _lastSpec(i).Contains("\"focus\":[\"CI/CD\"]")).SetResult(cicd);
        module.Setup<HierarchyLayout>("layout", i => _lastSpec(i).Contains("\"focus\":[\"CI/CD\",\"A / B\"]")).SetResult(ab);
        return module;
    }

    [Fact]
    public void Drilldown_names_with_slashes_drill_and_climb_back_with_an_escaped_Focus()
    {
        BunitJSModuleInterop module = _slashSetup();
        List<string?> changes = [];
        IRenderedComponent<TreemapChart> cut = Render<TreemapChart>(ps => ps.Add(p => p.Root, _slashes).Add(p => p.Drilldown, true)
            .Add(p => p.FocusChanged, (string? f) => changes.Add(f)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "CI/CD: 10").Click();
        changes.ShouldBe(["CI\\/CD"]);
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"CI/CD\"]");
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(2));

        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "CI/CD / A / B: 8").Click();
        changes.ShouldBe(["CI\\/CD", "CI\\/CD/A \\/ B"]);
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"CI/CD\",\"A / B\"]");
        cut.WaitForAssertion(() => cut.FindAll("nav button").Count.ShouldBe(3));
        cut.FindAll("nav button").Select(b => b.TextContent.Trim()).ShouldBe(["Ops", "CI/CD", "A / B"]);

        cut.FindAll("nav button")[1].Click();
        changes[^1].ShouldBe("CI\\/CD");
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"CI/CD\"]");
    }

    [Fact]
    public void Focus_with_escaped_slashes_names_the_node_and_OnNodeClick_finds_it()
    {
        BunitJSModuleInterop module = _slashSetup();
        List<HierarchyNode> clicked = [];
        IRenderedComponent<IcicleChart> cut = Render<IcicleChart>(ps => ps.Add(p => p.Root, _slashes)
            .Add(p => p.Focus, "CI\\/CD/A \\/ B")
            .Add(p => p.OnNodeClick, (HierarchyNode n) => clicked.Add(n)));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        _lastSpec(module.Invocations["layout"].Last()).ShouldContain("\"focus\":[\"CI/CD\",\"A / B\"]");

        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "CI/CD / A / B: 8").Click();
        cut.FindAll("rect[role=button]").Single(r => r.GetAttribute("aria-label") == "CI/CD / A / B / x: 5").Click();
        HierarchyNode ab = _slashes.Children![0].Children![0];
        clicked.ShouldBe([ab, ab.Children![0]]);
    }

    // Exposes the breadcrumb a drill-down chart builds, to check it is built once per layout.
    private sealed class TrailProbe : TreemapChart
    {
        public IReadOnlyList<string>? Probe => Trail;
    }

    // Counts the number formats asked of it (string.Format asks for an ICustomFormatter once per call).
    private sealed class CountingCulture() : System.Globalization.CultureInfo("en-US")
    {
        public int Formats { get; set; }

        public override object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                Formats++;
            }

            return base.GetFormat(formatType);
        }
    }

    [Fact]
    public void Drilldown_builds_the_trail_once_per_layout_and_each_tooltip_once_per_node()
    {
        _drillSetup();
        System.Globalization.CultureInfo before = System.Globalization.CultureInfo.CurrentCulture;
        CountingCulture culture = new();
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
            IRenderedComponent<TrailProbe> cut = Render<TrailProbe>(ps => ps.Add(p => p.Root, _root).Add(p => p.Drilldown, true)
                .Add(p => p.ShowValues, false));
            cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

            IReadOnlyList<string>? trail = cut.Instance.Probe;
            trail.ShouldBe(["Storage"]);
            cut.Instance.Probe.ShouldBeSameAs(trail);

            // A re-render with the same layout: one tooltip per node (5) and the description's two
            // branch totals; the trail is not rebuilt.
            culture.Formats = 0;
            cut.Render();
            culture.Formats.ShouldBe(5 + 2);
            cut.Instance.Probe.ShouldBeSameAs(trail);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = before;
        }
    }

    [Fact]
    public void Treemap_strip_tiling_is_sent_to_the_layout() =>
        _hierarchyChart("treemap", () => Render<TreemapChart>(ps => ps.Add(p => p.Root, _root).Add(p => p.Tile, TreemapTile.Strip)), "\"tile\":\"strip\"", cut =>
            cut.FindAll("svg rect").Count.ShouldBe(5));

    [Fact]
    public void Sunburst_hides_a_label_its_arc_would_cut_to_three_letters()
    {
        // At tile size the docs cover showed "Bil…" and "Inv…": a stub that names nothing.
        (_, JSRuntimeInvocationHandler<HierarchyLayout> call) = _setup<HierarchyLayout>();
        HierarchyLayoutNode Arc(string name, double room) => new(name, name, 1, 0, true, 10, 0, 40, 1, 80, "M0,0Z", 0, -60, 0, room, 20) { Names = [name] };
        call.SetResult(new HierarchyLayout([Arc("Billing", 30), Arc("Invoices", 60), Arc("Deployments", 45)], ["Billing"], 100, 100));

        IRenderedComponent<SunburstChart> cut = Render<SunburstChart>(ps => ps.Add(p => p.Root, _root));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));

        string[] labels = cut.FindAll("svg text").Select(t => t.TextContent).ToArray();
        labels.ShouldNotContain("Bil…");
        labels.ShouldContain("Invoices");
        labels.ShouldContain("Deplo…", "four letters or more still make a cut label");
    }
}
