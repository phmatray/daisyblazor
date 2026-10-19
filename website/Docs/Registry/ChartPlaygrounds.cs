using DaisyBlazor.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static ComponentDoc WithPlayground(ComponentDoc doc) =>
        ChartPlaygrounds.Specs.TryGetValue(doc.Name, out PlaygroundSpec? spec) ? doc with { Playground = spec } : doc;
}

// A class of its own so its fields are ready whenever DocRegistry's own initializers ask for them.
file static class ChartPlaygrounds
{
    private static readonly string[] Quarters = ["Q1", "Q2", "Q3", "Q4"];

    private static readonly ChartSeries[] SeatsByPlan =
    [
        new("Pro", [10.2, 11.8, 12.4, 18.9]),
        new("Team", [6.1, 5.4, 4.9, 7.2]),
        new("Enterprise", [9.7, 10.3, 11.1, 12.6]),
    ];

    private static readonly ChartDataPoint[] Channels =
    [
        new("Organic search", 1_150), new("Referral", 610), new("Paid ads", 380), new("Social", 260),
    ];

    /// <summary>Playgrounds for the charts people reach for most: their data is fixed, their options are the controls.</summary>
    public static readonly Dictionary<string, PlaygroundSpec> Specs = new()
    {
        ["BarChart"] = new(typeof(BarChart), "Horizontal", "Stacked", "Percent", "ShowGrid", "ShowLegend", "Trendline", "Height")
        {
            Initial = new Dictionary<string, object?> { ["Labels"] = Quarters, ["Series"] = SeatsByPlan },
        },
        ["LineChart"] = new(typeof(LineChart), "Smooth", "Step", "Area", "Stacked", "ShowPoints", "ShowGrid", "ShowLegend", "Trendline", "Height")
        {
            Initial = new Dictionary<string, object?> { ["Labels"] = Quarters, ["Series"] = SeatsByPlan },
        },
        ["AreaChart"] = new(typeof(AreaChart), "Smooth", "Step", "Stacked", "Percent", "ShowPoints", "ShowGrid", "ShowLegend", "Height")
        {
            Initial = new Dictionary<string, object?> { ["Labels"] = Quarters, ["Series"] = SeatsByPlan },
        },
        ["PieChart"] = new(typeof(PieChart), "ShowLegend", "ShowPercent", "ExplodeOffset", "OthersThreshold", "Height")
        {
            Initial = new Dictionary<string, object?> { ["Data"] = Channels },
        },
        ["DonutChart"] = new(typeof(DonutChart), "InnerRadiusRatio", "ShowLegend", "ShowPercent", "ExplodeOffset", "Height")
        {
            Initial = new Dictionary<string, object?> { ["Data"] = Channels },
        },
        ["FunnelChart"] = new(typeof(FunnelChart), "Shape", "Gap", "Height")
        {
            Initial = new Dictionary<string, object?>
            {
                ["Data"] = new ChartDataPoint[] { new("Pricing page", 8_400), new("Started trial", 2_150), new("Activated", 1_320), new("Paid", 410) },
            },
        },
        ["RadarChart"] = new(typeof(RadarChart), "Grid", "Levels", "Filled", "Stacked", "ShowPoints", "ShowLegend")
        {
            Initial = new Dictionary<string, object?>
            {
                ["Labels"] = new[] { "Seats", "Storage", "API calls", "Support", "Integrations" },
                ["Series"] = new ChartSeries[] { new("Starter", [20, 25, 30, 20, 35]), new("Team", [55, 60, 65, 50, 70]), new("Scale", [90, 85, 95, 90, 80]) },
            },
        },
        ["GaugeChart"] = new(typeof(GaugeChart), "Value", "Min", "Max", "StartAngle", "EndAngle", "Thickness")
        {
            Initial = new Dictionary<string, object?> { ["Value"] = 72d },
        },
        ["SankeyChart"] = new(typeof(SankeyChart), "Align", "NodeWidth", "NodePadding", "ShowValues")
        {
            Initial = new Dictionary<string, object?>
            {
                ["Links"] = new SankeyLink[]
                {
                    new("Organic", "Starter", 320), new("Organic", "Pro", 180), new("Referral", "Pro", 140),
                    new("Partner", "Enterprise", 70), new("Starter", "Active", 260), new("Starter", "Churned", 60),
                    new("Pro", "Active", 290), new("Pro", "Churned", 30), new("Enterprise", "Active", 70),
                },
            },
        },
    };
}
