using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static ComponentDoc WithPlayground(ComponentDoc doc) =>
        doc.Playground is null && ChartPlaygrounds.FromExample(doc) is { } spec ? doc with { Playground = spec } : doc;
}

/// <summary>
/// Every chart's playground starts from its first example: the chart's tag in the example's source gives the
/// starting values (fields such as Series="_signups" are read off a live instance of the example, literals
/// such as Smooth="true" are parsed), and the example's @code block travels with the generated Razor, so the
/// copied snippet compiles as it is.
/// </summary>
internal static partial class ChartPlaygrounds
{
    // The options worth a control on the charts people reach for most; the others expose every simple parameter.
    private static readonly Dictionary<string, string[]> Controls = new()
    {
        ["BarChart"] = ["Horizontal", "Stacked", "Percent", "ShowGrid", "ShowLegend", "Trendline", "Height"],
        ["LineChart"] = ["Smooth", "Step", "Area", "Stacked", "ShowPoints", "ShowGrid", "ShowLegend", "Trendline", "Height"],
        ["AreaChart"] = ["Smooth", "Step", "Stacked", "Percent", "ShowPoints", "ShowGrid", "ShowLegend", "Height"],
        ["PieChart"] = ["ShowLegend", "ShowPercent", "ExplodeOffset", "OthersThreshold", "Height"],
        ["DonutChart"] = ["InnerRadiusRatio", "CenterLabel", "ShowLegend", "ShowPercent", "ExplodeOffset", "Height"],
    };

    // A chart's own simple parameters, without the ones every chart shares and nobody tunes in a playground.
    private static readonly HashSet<string> Skipped = ["Class", "Style", "Width", "Description"];

    public static PlaygroundSpec? FromExample(ComponentDoc doc)
    {
        if (doc.Examples.Length == 0) return null;
        Type chart = doc.Types[0];
        Type example = doc.Examples[0].Component;
        string source = ExampleSource.Get(example);

        Dictionary<string, string>? attributes = TagAttributes(source, doc.Name);
        if (attributes is null) return null;

        object? instance;
        try { instance = Activator.CreateInstance(example); } catch { return null; }

        var initial = new Dictionary<string, object?>();
        var fields = new Dictionary<string, string>();
        foreach ((string name, string raw) in attributes)
        {
            PropertyInfo? property = chart.GetProperty(name);
            if (property is null) continue;
            string value = raw.TrimStart('@');
            if (FieldName().IsMatch(value))
            {
                object? data = example.GetField(value, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(instance);
                if (data is null || !property.PropertyType.IsInstanceOfType(data)) continue;
                initial[name] = data;
                fields[name] = value;
            }
            else if (Parse(value, property.PropertyType) is { } literal)
            {
                initial[name] = literal;
            }
        }
        if (fields.Count == 0) return null; // nothing to draw without the example's data

        int code = source.IndexOf("@code", StringComparison.Ordinal);
        string[] controls = Controls.TryGetValue(doc.Name, out string[]? names)
            ? names
            : [.. chart.GetProperties().Where(p => !Skipped.Contains(p.Name)).Select(p => p.Name)];
        return new PlaygroundSpec(chart, controls)
        {
            Initial = initial,
            Fields = fields,
            Code = code < 0 ? null : source[code..],
        };
    }

    /// <summary>The attributes of the first &lt;{tag} …&gt; in the source, or null when it has none.</summary>
    private static Dictionary<string, string>? TagAttributes(string source, string tag)
    {
        Match open = Regex.Match(source, $@"<{tag}\b");
        if (!open.Success) return null;
        var attributes = new Dictionary<string, string>();
        foreach (Match a in Attribute().Matches(source[(open.Index + open.Length)..]))
        {
            if (a.Groups["end"].Success) break;
            // A bare attribute (Smooth) is a true boolean.
            attributes[a.Groups["name"].Value] = a.Groups["value"].Success ? a.Groups["value"].Value : "true";
        }
        return attributes;
    }

    private static object? Parse(string text, Type type)
    {
        Type t = Nullable.GetUnderlyingType(type) ?? type;
        try
        {
            if (t == typeof(string)) return text;
            if (t == typeof(bool)) return bool.Parse(text);
            if (t.IsEnum) return Enum.Parse(t, text[(text.LastIndexOf('.') + 1)..]);
            if (t == typeof(double) || t == typeof(int)) return Convert.ChangeType(text, t, CultureInfo.InvariantCulture);
        }
        catch (FormatException) { }
        catch (ArgumentException) { }
        return null;
    }

    [GeneratedRegex(@"^_\w+$")]
    private static partial Regex FieldName();

    // name="value" pairs up to the end of the tag (">" or "/>" outside an attribute).
    [GeneratedRegex(@"\G\s*(?:(?<name>[A-Za-z][\w-]*)=""(?<value>[^""]*)""|(?<name>[A-Za-z][\w-]*)(?=[\s/>])|(?<end>/?>))")]
    private static partial Regex Attribute();
}
