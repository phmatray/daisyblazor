namespace DaisyBlazor.Docs.Docs;

public enum HitKind { Guide, Component, Parameter, Section }

public sealed record SearchHit(HitKind Kind, string Title, string Context, string Url);

/// <summary>In-memory index over guides, components and their parameters, for the ⌘K palette.</summary>
public sealed class SearchIndex(XmlDocs docs)
{
    private readonly Lazy<List<SearchHit>> _items = new(() => Build(docs));

    public IReadOnlyList<SearchHit> Query(string query, int max = 12)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return [.. _items.Value.Where(i => i.Kind == HitKind.Guide).Take(max)];
        }

        return [.. _items.Value
            .Select(i => (item: i, score: Score(i, query)))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.item.Title.Length)
            .Take(max)
            .Select(x => x.item)];
    }

    private static int Score(SearchHit item, string q)
    {
        int kindBonus = item.Kind switch { HitKind.Component => 40, HitKind.Guide => 30, HitKind.Section => 10, _ => 0 };
        string t = item.Title;
        if (t.Equals(q, StringComparison.OrdinalIgnoreCase)) return 1000 + kindBonus;
        if (t.StartsWith(q, StringComparison.OrdinalIgnoreCase)) return 600 + kindBonus;
        int idx = t.IndexOf(q, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) return 400 + kindBonus - idx;
        if (IsSubsequence(q, t)) return 200 + kindBonus;
        if (item.Context.Contains(q, StringComparison.OrdinalIgnoreCase)) return 80 + kindBonus;
        return 0;
    }

    private static bool IsSubsequence(string q, string t)
    {
        int i = 0;
        foreach (char c in t)
        {
            if (i < q.Length && char.ToLowerInvariant(c) == char.ToLowerInvariant(q[i])) i++;
        }
        return i == q.Length;
    }

    private static List<SearchHit> Build(XmlDocs docs)
    {
        var list = new List<SearchHit>();
        foreach (GuideDoc g in Guides.All)
        {
            list.Add(new(HitKind.Guide, g.Title, g.Summary, g.Url));
            foreach (string s in g.Sections)
            {
                list.Add(new(HitKind.Section, s, g.Title, $"{g.Url}#{Slugify.Of(s)}"));
            }
        }

        foreach (ComponentDoc d in DocRegistry.All)
        {
            list.Add(new(HitKind.Component, d.Name, $"{d.Category.Title()} · {d.Summary}", d.Url));
            if (d.MudEquivalent is not null)
            {
                list.Add(new(HitKind.Component, d.MudEquivalent, $"MudBlazor → {d.Name}", d.Url + "#migration"));
            }
            foreach (ExampleDoc e in d.Examples)
            {
                list.Add(new(HitKind.Section, $"{d.Name} · {e.Title}", e.Description, $"{d.Url}#{e.Id}"));
            }
            foreach (Type t in d.Types)
            {
                foreach (ParamInfo p in ApiReflector.Parameters(t, docs))
                {
                    list.Add(new(HitKind.Parameter, $"{ApiReflector.TagName(t)}.{p.Name}",
                        $"{p.TypeName}{(p.Summary is null ? "" : " — " + p.Summary)}", $"{d.Url}#api"));
                }
            }
        }
        return list;
    }
}
