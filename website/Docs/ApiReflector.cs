using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Docs.Docs;

public enum ParamKind { Property, Event, Content, Method }

/// <summary>
/// One documented member: a component <c>[Parameter]</c>, or a property, event or method of a service or options type.
/// For a method, <see cref="TypeName"/> holds its signature.
/// </summary>
public sealed record ParamInfo(MemberInfo Member, ParamKind Kind, string TypeName, string? Default, string? Summary)
{
    public string Name => Member is MethodInfo { IsGenericMethodDefinition: true } m
        ? $"{m.Name}<{string.Join(", ", m.GetGenericArguments().Select(a => a.Name))}>"
        : Member.Name;
    public PropertyInfo Property => (PropertyInfo)Member;
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
        if (!IsComponent(component))
        {
            IReadOnlyList<ParamInfo> members = Members(component, docs);
            lock (Cache) Cache[component] = members;
            return members;
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

        List<ParamInfo> list = new();
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

    public static bool IsComponent(Type t) => typeof(IComponent).IsAssignableFrom(t);

    /// <summary>
    /// Public surface of a service interface or options class: properties (settable ones only on classes,
    /// with defaults from a fresh instance), events and methods with their signatures.
    /// </summary>
    private static IReadOnlyList<ParamInfo> Members(Type type, XmlDocs docs)
    {
        object? instance = null;
        if (!type.IsInterface && !type.IsAbstract)
        {
            try { instance = Activator.CreateInstance(type); } catch { /* no parameterless ctor: no defaults */ }
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        List<ParamInfo> list = [];
        foreach (PropertyInfo p in type.GetProperties(flags))
        {
            if (!type.IsInterface && p.SetMethod?.IsPublic != true) continue;
            string? def = null;
            if (instance is not null)
            {
                try { def = FormatValue(p.GetValue(instance), p.PropertyType); } catch { }
            }
            list.Add(new ParamInfo(p, ParamKind.Property, FriendlyName(p.PropertyType), def, docs.Summary(p)));
        }
        foreach (EventInfo e in type.GetEvents(flags))
        {
            list.Add(new ParamInfo(e, ParamKind.Event, FriendlyName(e.EventHandlerType!), null, docs.Summary(e)));
        }
        foreach (MethodInfo m in type.GetMethods(flags))
        {
            if (m.IsSpecialName) continue;
            list.Add(new ParamInfo(m, ParamKind.Method, Signature(m), null, docs.Summary(m)));
        }
        return [.. list.OrderBy(x => x.Kind).ThenBy(x => x.Name, StringComparer.Ordinal)];
    }

    /// <summary>"(string message, Severity severity = Severity.Normal) → void".</summary>
    private static string Signature(MethodInfo m)
    {
        IEnumerable<string> args = m.GetParameters().Select(p =>
            $"{FriendlyName(p.ParameterType)} {p.Name}" + (p.HasDefaultValue ? $" = {FormatValue(p.DefaultValue, p.ParameterType) ?? "default"}" : ""));
        return $"({string.Join(", ", args)}) → {FriendlyName(m.ReturnType)}";
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
            // 16d / 9 reads better as 1.78 than 1.7777777777777777.
            double d => Math.Round(d, 2).ToString(CultureInfo.InvariantCulture),
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
        if (t == typeof(void)) return "void";
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
