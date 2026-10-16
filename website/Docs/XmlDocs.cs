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

    private string? Get(string key) => _members.Value.TryGetValue(key, out string? v) ? v : null;

    private static string XmlName(Type type)
    {
        if (type.IsGenericType) type = type.GetGenericTypeDefinition();
        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static Dictionary<string, string> Load()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
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

    /// <summary>Turns a summary element into plain text, keeping cref/c content readable.</summary>
    private static string Flatten(XElement el)
    {
        var sb = new StringBuilder();
        foreach (XNode node in el.Nodes())
        {
            switch (node)
            {
                case XText t: sb.Append(t.Value); break;
                case XElement e when e.Name == "see" || e.Name == "seealso":
                    string? cref = e.Attribute("cref")?.Value ?? e.Attribute("langword")?.Value;
                    sb.Append(e.Value.Length > 0 ? e.Value : cref is null ? "" : cref[(cref.LastIndexOf('.') + 1)..]);
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
