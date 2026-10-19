using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Native.Object;

namespace DaisyBlazor.Charts.Server;

/// <summary>
/// Runs the charts' D3 layout modules with Jint. The modules ship embedded in this assembly and are
/// written once to a temporary folder that Jint loads them from. A Jint engine is not thread-safe, so
/// each call rents one from a pool; the first call on an engine parses the D3 bundle (about half a
/// second), later calls take a few milliseconds (density contours and force layouts take longer),
/// and repeated specs come from a cache.
/// </summary>
public sealed partial class JintChartLayoutEngine : IChartLayoutEngine, IDisposable
{
    // ponytail: the cache is emptied when full; switch to an LRU if pages cycle through many specs.
    private const int CacheSize = 512;

    private readonly ConcurrentBag<Engine> _pool = [];
    private readonly ConcurrentDictionary<(string Kind, string Spec), string> _cache = new();
    private readonly Lazy<string> _root = new(ExtractModules);

    /// <inheritdoc />
    public string Layout(string kind, string specJson)
    {
        // Same rule as charts.mjs: a plain lowercase name, never a path or a shared "_" helper.
        if (!KindPattern().IsMatch(kind))
        {
            throw new ArgumentException($"Unknown chart layout \"{kind}\".", nameof(kind));
        }

        // Layouts are pure functions of their spec, so a repeated spec (the same chart on every
        // prerender of a page) is answered from the cache.
        if (_cache.TryGetValue((kind, specJson), out string? cached))
        {
            return cached;
        }

        Engine engine = _pool.TryTake(out Engine? pooled) ? pooled : CreateEngine();
        try
        {
            ObjectInstance module = engine.Modules.Import($"./layouts/{kind}.mjs");
            JsValue spec = new JsonParser(engine).Parse(specJson);
            JsValue result = engine.Invoke(module.Get("default"), spec);
            string json = new JsonSerializer(engine).Serialize(result).ToString();
            if (_cache.Count >= CacheSize)
            {
                _cache.Clear();
            }

            _cache[(kind, specJson)] = json;
            return json;
        }
        finally
        {
            _pool.Add(engine);
        }
    }

    // d3-timer (used by d3-force when a simulation is created) schedules with setTimeout and setInterval. Layouts stop
    // their simulations at once and tick them by hand, so timers that never fire are enough.
    private Engine CreateEngine()
    {
        Engine engine = new(o => o.EnableModules(_root.Value));
        engine.SetValue("setTimeout", new Func<JsValue, JsValue, int>((_, _) => 0));
        engine.SetValue("clearTimeout", new Action<JsValue>(_ => { }));
        engine.SetValue("setInterval", new Func<JsValue, JsValue, int>((_, _) => 0));
        engine.SetValue("clearInterval", new Action<JsValue>(_ => { }));
        return engine;
    }

    /// <summary>Parses the D3 bundle on one engine ahead of the first request.</summary>
    public void Warmup() => Layout("sankey", """{"links":[],"width":10,"height":10}""");

    /// <inheritdoc />
    public void Dispose()
    {
        while (_pool.TryTake(out Engine? engine))
        {
            engine.Dispose();
        }
    }

    // One folder per build of this assembly, so an upgrade never reads stale modules.
    private static string ExtractModules()
    {
        Assembly assembly = typeof(JintChartLayoutEngine).Assembly;
        string root = Path.Combine(Path.GetTempPath(), "daisyblazor-charts", assembly.ManifestModule.ModuleVersionId.ToString("N"));
        foreach (string name in assembly.GetManifestResourceNames().Where(n => n.StartsWith("charts/", StringComparison.Ordinal)))
        {
            string path = Path.Combine(root, name["charts/".Length..].Replace('\\', '/'));
            if (File.Exists(path))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using Stream source = assembly.GetManifestResourceStream(name)!;
            string temp = path + "." + Guid.NewGuid().ToString("N");
            using (FileStream target = File.Create(temp))
            {
                source.CopyTo(target);
            }

            File.Move(temp, path, overwrite: true);
        }

        return root;
    }

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex KindPattern();
}
