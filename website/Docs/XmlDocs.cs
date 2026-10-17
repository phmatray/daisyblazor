using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace DaisyBlazor.Docs.Docs;

/// <summary>Reads the libraries' XML documentation (embedded at build) for summaries in the API tables.</summary>
public sealed class XmlDocs
{
    private readonly Lazy<Dictionary<string, string>> _members = new(Load);

    /// <summary>Summary of a type, or null.</summary>
    public string? Summary(Type type) => Get("T:" + XmlName(type));

    /// <summary>Summary of a property, looked up on its declaring type (generic definitions included).</summary>
    public string? Summary(PropertyInfo prop) =>
        prop.DeclaringType is null ? null : Get($"P:{XmlName(prop.DeclaringType)}.{prop.Name}");

    /// <summary>Summary of an event or method (by name; the first overload wins), or null.</summary>
    public string? Summary(MemberInfo member)
    {
        if (member is PropertyInfo p) return Summary(p);
        if (member.DeclaringType is null) return null;
        string prefix = $"{(member is EventInfo ? "E" : "M")}:{XmlName(member.DeclaringType)}.{member.Name}";
        if (Get(prefix) is { } exact) return exact;
        // ponytail: prefix match instead of building the full XML-doc signature; overloads share the first summary.
        return _members.Value.FirstOrDefault(kv => kv.Key.StartsWith(prefix + "(", StringComparison.Ordinal)
            || kv.Key.StartsWith(prefix + "`", StringComparison.Ordinal)).Value;
    }

    private string? Get(string key) => _members.Value.TryGetValue(key, out string? v) ? v : null;

    private static string XmlName(Type type)
    {
        if (type.IsGenericType) type = type.GetGenericTypeDefinition();
        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static Dictionary<string, string> Load()
    {
        Dictionary<string, string> map = new(StringComparer.Ordinal);
        Assembly asm = typeof(XmlDocs).Assembly;
        foreach (string res in asm.GetManifestResourceNames().Where(n => n.StartsWith("xmldoc:", StringComparison.Ordinal)))
        {
            using Stream? s = asm.GetManifestResourceStream(res);
            if (s is null) continue;
            XDocument doc = XDocument.Load(s);
            foreach (XElement m in doc.Descendants("member"))
            {
                string? name = m.Attribute("name")?.Value;
                XElement? summary = m.Element("summary");
                if (name is null || summary is null) continue;
                map[name] = Flatten(summary);
            }
        }
        return map;
    }

    /// <summary>
    /// Readable name of a cref: "T:N.DataGrid`1" → "DataGrid&lt;T&gt;", "M:N.StatusMap.For``1(System.String)" → "For&lt;T&gt;()".
    /// </summary>
    internal static string CrefName(string cref)
    {
        bool method = cref.StartsWith("M:", StringComparison.Ordinal);
        int paren = cref.IndexOf('(');
        if (paren >= 0) cref = cref[..paren];
        string name = cref[(cref.LastIndexOf('.') + 1)..];
        if (name.Contains(':')) name = name[(name.IndexOf(':') + 1)..];
        int tick = name.IndexOf('`');
        if (tick >= 0)
        {
            int arity = int.TryParse(name.AsSpan(tick).TrimStart('`'), out int n) ? n : 1;
            name = name[..tick] + "<" + (arity == 1 ? "T" : string.Join(", ", Enumerable.Range(1, arity).Select(i => $"T{i}"))) + ">";
        }
        return method ? name + "()" : name;
    }

    /// <summary>Turns a summary element into plain text, keeping cref/c content readable.</summary>
    private static string Flatten(XElement el)
    {
        StringBuilder sb = new();
        foreach (XNode node in el.Nodes())
        {
            switch (node)
            {
                case XText t: sb.Append(t.Value); break;
                case XElement e when e.Name == "see" || e.Name == "seealso":
                    string? cref = e.Attribute("cref")?.Value;
                    sb.Append(e.Value.Length > 0 ? e.Value
                        : cref is not null ? CrefName(cref)
                        : e.Attribute("langword")?.Value ?? "");
                    break;
                case XElement e when e.Name == "paramref" || e.Name == "typeparamref":
                    sb.Append(e.Attribute("name")?.Value);
                    break;
                case XElement e when e.Name == "para":
                    sb.Append(' ').Append(Flatten(e)).Append(' ');
                    break;
                case XElement e:
                    sb.Append(Flatten(e));
                    break;
            }
        }
        return string.Join(' ', sb.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
