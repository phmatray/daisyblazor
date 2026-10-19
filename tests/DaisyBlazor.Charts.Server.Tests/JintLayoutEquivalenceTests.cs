using System.Text.Json;
using DaisyBlazor.Charts.Server;

namespace DaisyBlazor.Charts.Server.Tests;

/// <summary>
/// Every layout call the node chart tests make (recorded by scripts/record-layouts.mjs) gives the same
/// result in Jint on the server as in the browser's engine, so a server-rendered chart matches the
/// chart the browser draws.
/// </summary>
public sealed class JintLayoutEquivalenceTests : IClassFixture<JintLayoutEquivalenceTests.EngineFixture>
{
    private readonly JintChartLayoutEngine _engine;

    public JintLayoutEquivalenceTests(EngineFixture fixture) => _engine = fixture.Engine;

    public static TheoryData<int, string> Calls()
    {
        TheoryData<int, string> data = [];
        JsonElement[] calls = Fixture();
        for (int i = 0; i < calls.Length; i++)
        {
            data.Add(i, calls[i].GetProperty("kind").GetString()!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Calls))]
    public void Jint_matches_the_browser_layout(int index, string kind)
    {
        JsonElement call = Fixture()[index];

        string json = _engine.Layout(kind, call.GetProperty("spec").GetRawText());

        using JsonDocument actual = JsonDocument.Parse(json);
        Same(actual.RootElement, call.GetProperty("result"), "$");
    }

    [Fact]
    public void Every_layout_kind_is_covered()
    {
        string layouts = Path.Combine(AppContext.BaseDirectory, "../../../../../src/DaisyBlazor.Charts/wwwroot/layouts");
        string[] kinds = Directory.GetFiles(layouts, "*.mjs").Select(Path.GetFileNameWithoutExtension).Where(k => !k!.StartsWith('_')).ToArray()!;
        HashSet<string> recorded = Fixture().Select(c => c.GetProperty("kind").GetString()!).ToHashSet();

        kinds.Where(k => !recorded.Contains(k)).ShouldBeEmpty("re-run scripts/record-layouts.mjs after adding a layout");
    }

    [Theory]
    [InlineData("../d3")]
    [InlineData("_compare")]
    [InlineData("Sankey")]
    public void Paths_and_helpers_are_refused(string kind) =>
        Should.Throw<ArgumentException>(() => _engine.Layout(kind, "{}"));

    [Fact]
    public void Concurrent_calls_each_get_their_own_engine()
    {
        string spec = """{"links":[{"source":"A","target":"B","value":3}],"width":200,"height":100}""";
        string expected = _engine.Layout("sankey", spec);

        string[] results = new string[16];
        Parallel.For(0, results.Length, i => results[i] = _engine.Layout("sankey", spec));

        results.ShouldAllBe(r => r == expected);
    }

    private static JsonElement[] Fixture()
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "layouts.fixture.json")));
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
    }

    // Numbers may differ in the last bits between engines' Math functions; everything else must match.
    private static void Same(JsonElement actual, JsonElement expected, string path)
    {
        actual.ValueKind.ShouldBe(expected.ValueKind, path);
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                actual.EnumerateObject().Select(p => p.Name).Order().ShouldBe(expected.EnumerateObject().Select(p => p.Name).Order(), path);
                foreach (JsonProperty p in expected.EnumerateObject())
                {
                    Same(actual.GetProperty(p.Name), p.Value, $"{path}.{p.Name}");
                }

                break;
            case JsonValueKind.Array:
                actual.GetArrayLength().ShouldBe(expected.GetArrayLength(), path);
                for (int i = 0; i < expected.GetArrayLength(); i++)
                {
                    Same(actual[i], expected[i], $"{path}[{i}]");
                }

                break;
            case JsonValueKind.Number:
                double a = actual.GetDouble(), e = expected.GetDouble();
                Math.Abs(a - e).ShouldBeLessThanOrEqualTo(1e-9 * Math.Max(1, Math.Abs(e)), path);
                break;
            case JsonValueKind.String:
                SameText(actual.GetString()!, expected.GetString()!, path);
                break;
            default:
                actual.GetRawText().ShouldBe(expected.GetRawText(), path);
                break;
        }
    }

    // SVG path strings carry numbers too: compare them number by number with the same tolerance.
    private static void SameText(string actual, string expected, string path)
    {
        if (actual == expected)
        {
            return;
        }

        string[] a = System.Text.RegularExpressions.Regex.Split(actual, @"(-?\d+(?:\.\d+)?(?:e[-+]?\d+)?)");
        string[] e = System.Text.RegularExpressions.Regex.Split(expected, @"(-?\d+(?:\.\d+)?(?:e[-+]?\d+)?)");
        a.Length.ShouldBe(e.Length, $"{path}: \"{actual}\" vs \"{expected}\"");
        for (int i = 0; i < e.Length; i++)
        {
            if (i % 2 == 1 && double.TryParse(a[i], System.Globalization.CultureInfo.InvariantCulture, out double x)
                && double.TryParse(e[i], System.Globalization.CultureInfo.InvariantCulture, out double y))
            {
                Math.Abs(x - y).ShouldBeLessThanOrEqualTo(1e-6 * Math.Max(1, Math.Abs(y)), path);
            }
            else
            {
                a[i].ShouldBe(e[i], path);
            }
        }
    }

    public sealed class EngineFixture : IDisposable
    {
        public JintChartLayoutEngine Engine { get; } = new();

        public void Dispose() => Engine.Dispose();
    }
}
