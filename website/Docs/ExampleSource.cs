using System.Reflection;

namespace DaisyBlazor.Docs.Docs;

/// <summary>The exact Razor source of an example, embedded at build time (see the csproj).</summary>
public static class ExampleSource
{
    private const string RepoBlob = "https://github.com/phmatray/daisyblazor/blob/main/";
    private const string Prefix = "example:";

    private static readonly Assembly Asm = typeof(ExampleSource).Assembly;

    /// <summary>Resource names look like "example:Actions/ButtonColorsExample" (separator depends on the build OS).</summary>
    private static readonly Dictionary<string, string> Resources = Asm.GetManifestResourceNames()
        .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal))
        .ToDictionary(n => n[(n.LastIndexOfAny(['/', '\\', ':']) + 1)..], n => n);

    public static string Get(Type example)
    {
        if (!Resources.TryGetValue(example.Name, out string? res)) return $"@* source for {example.Name} not embedded *@";
        using Stream s = Asm.GetManifestResourceStream(res)!;
        using var reader = new StreamReader(s);
        return reader.ReadToEnd().Replace("\r\n", "\n").Trim();
    }

    /// <summary>Repository path, e.g. "website/Examples/Actions/ButtonColorsExample.razor".</summary>
    public static string Path(Type example)
    {
        string rel = Resources.TryGetValue(example.Name, out string? res)
            ? res[Prefix.Length..].Replace('\\', '/')
            : example.Name;
        return $"website/Examples/{rel}.razor";
    }

    public static string GitHubUrl(Type example) => RepoBlob + Path(example);
}
