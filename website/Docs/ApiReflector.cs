using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Docs.Docs;

public enum ParamKind { Property, Event, Content }

/// <summary>One public <c>[Parameter]</c> of a component, as shown in the API table and the playground.</summary>
public sealed record ParamInfo(PropertyInfo Property, ParamKind Kind, string TypeName, string? Default, string? Summary)
{
    public string Name => Property.Name;
    public Type Type => Property.PropertyType;
}

/// <summary>Reflects component parameters (with defaults read from a fresh instance) for the API tables.</summary>
public static class ApiReflector
{
    private static readonly Dictionary<Type, IReadOnlyList<ParamInfo>> Cache = [];

    public static IReadOnlyList<ParamInfo> Parameters(Type component, XmlDocs docs)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(component, out IReadOnlyList<ParamInfo>? hit)) return hit;
        }

        object? instance = null, second = null;
        try
        {
            // Open generics (Table<>) are listed so the API shows T; close them over object to read defaults.
            Type closed = component.IsGenericTypeDefinition
                ? component.MakeGenericType([.. component.GetGenericArguments().Select(_ => typeof(object))])
                : component;
            instance = Activator.CreateInstance(closed);
            second = Activator.CreateInstance(closed);
        }
        catch { /* components with required ctor args: no defaults */ }

        // Type names come from the open definition, so DataGrid<string> documents Items as IEnumerable<T>.
        Type definition = component.IsGenericType ? component.GetGenericTypeDefinition() : component;

        var list = new List<ParamInfo>();
        foreach (PropertyInfo p in component.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ParameterAttribute? attr = p.GetCustomAttribute<ParameterAttribute>();
            if (attr is null || attr.CaptureUnmatchedValues) continue;

            ParamKind kind = IsEvent(p.PropertyType) ? ParamKind.Event
                : IsContent(p.PropertyType) ? ParamKind.Content
                : ParamKind.Property;

            string? def = null;
            if (p.GetCustomAttribute<EditorRequiredAttribute>() is not null)
            {
                def = "required";
            }
            else if (instance is not null && kind == ParamKind.Property)
            {
                // Read through the instance's own (closed) type: an open-generic PropertyInfo can't GetValue.
                try
                {
                    PropertyInfo read = component.IsGenericTypeDefinition ? instance.GetType().GetProperty(p.Name)! : p;
                    def = FormatValue(read.GetValue(instance), p.PropertyType);
                    // A default that differs per instance (Id = "drawer-" + Guid) is generated, not a value to show.
                    if (second is not null && def != FormatValue(read.GetValue(second), p.PropertyType)) def = "generated";
                }
                catch { }
            }

            Type declared = definition.GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance)?.PropertyType ?? p.PropertyType;
            list.Add(new ParamInfo(p, kind, FriendlyName(declared), def, docs.Summary(p)));
        }

        IReadOnlyList<ParamInfo> result = [.. list.OrderBy(x => x.Kind).ThenBy(x => Rank(x.Name)).ThenBy(x => x.Name, StringComparer.Ordinal)];
        lock (Cache) Cache[component] = result;
        return result;
    }

    /// <summary>Common parameters sink to the bottom of each group.</summary>
    private static int Rank(string name) => name is "Class" or "Style" ? 1 : 0;

    /// <summary>EventCallback and Action handlers; other delegates (Func data loaders) are plain parameters.</summary>
    public static bool IsEvent(Type t) =>
        t == typeof(EventCallback) || t == typeof(Action)
        || (t.IsGenericType && t.GetGenericTypeDefinition() is var g
            && (g == typeof(EventCallback<>) || g.FullName?.StartsWith("System.Action`", StringComparison.Ordinal) == true));

    public static bool IsContent(Type t) =>
        t == typeof(RenderFragment) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RenderFragment<>));

    /// <summary>Formats a default value as it would be written in Razor/C#.</summary>
    public static string? FormatValue(object? value, Type type)
    {
        if (value is null) return Nullable.GetUnderlyingType(type) is not null || !type.IsValueType ? "null" : null;
        return value switch
        {
            string s => s.Length == 0 ? "\"\"" : $"\"{s}\"",
            bool b => b ? "true" : "false",
            Enum e => $"{e.GetType().Name}.{e}",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            System.Collections.IEnumerable items =>
                $"[{string.Join(", ", items.Cast<object?>().Select(x => FormatValue(x, x?.GetType() ?? typeof(object))))}]",
            _ => value.ToString(),
        };
    }

    public static string FriendlyName(Type t)
    {
        if (Nullable.GetUnderlyingType(t) is { } u) return FriendlyName(u) + "?";
        if (t == typeof(string)) return "string";
        if (t == typeof(bool)) return "bool";
        if (t == typeof(int)) return "int";
        if (t == typeof(long)) return "long";
        if (t == typeof(double)) return "double";
        if (t == typeof(decimal)) return "decimal";
        if (t == typeof(float)) return "float";
        if (t == typeof(object)) return "object";
        if (t.IsArray) return FriendlyName(t.GetElementType()!) + "[]";
        if (t.IsGenericParameter) return t.Name;
        if (!t.IsGenericType) return t.Name;
        string name = t.Name[..t.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", t.GetGenericArguments().Select(FriendlyName))}>";
    }

    /// <summary>Display name of a component type, e.g. "Select&lt;T&gt;".</summary>
    public static string ComponentName(Type t)
    {
        if (!t.IsGenericType) return t.Name;
        Type def = t.GetGenericTypeDefinition();
        string name = def.Name[..def.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", def.GetGenericArguments().Select(a => a.Name))}>";
    }

    /// <summary>Tag name used in markup, e.g. "Select".</summary>
    public static string TagName(Type t) => t.IsGenericType ? t.Name[..t.Name.IndexOf('`')] : t.Name;
}
