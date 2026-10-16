namespace DaisyBlazor;

/// <summary>Stable element id for a form control so its <c>&lt;label for&gt;</c> can point at it.</summary>
internal static class FieldId
{
    public static string New() => "dz-field-" + Guid.NewGuid().ToString("N")[..8];

    /// <summary>A user-supplied <c>id</c> attribute wins (it is splatted last), else the generated one.</summary>
    public static string Resolve(IReadOnlyDictionary<string, object>? attrs, string generated)
        => attrs is not null && attrs.TryGetValue("id", out object? v) && v?.ToString() is { Length: > 0 } s ? s : generated;
}
