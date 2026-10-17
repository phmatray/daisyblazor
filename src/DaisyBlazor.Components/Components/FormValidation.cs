namespace DaisyBlazor;

/// <summary>An input that a cascaded <see cref="Form"/> can validate.</summary>
internal interface IFormField
{
    /// <summary>The field's current validation error, or <c>null</c> when valid. Pure: shows nothing.</summary>
    string? GetError();

    /// <summary>Displays <paramref name="error"/> under the field (<c>null</c> clears it).</summary>
    void ShowError(string? error);
}

/// <summary>MudBlazor-style field validation shared by TextField, NumericField and Select.</summary>
internal static class FormValidation
{
    /// <summary>
    /// Runs the Required check, then <paramref name="validation"/> (falling back to
    /// <see cref="Form.Validation"/>). Supported shapes, as in MudBlazor:
    /// <c>Func&lt;T, string?&gt;</c>, <c>Func&lt;T, bool&gt;</c>, <c>Func&lt;T, IEnumerable&lt;string&gt;&gt;</c>, and
    /// <c>Func&lt;object, string, IEnumerable&lt;string&gt;&gt;</c> called with (Form.Model, FieldName).
    /// </summary>
    public static string? Run<T>(Form? form, bool required, string? requiredError, object? validation, T value, string? fieldName)
    {
        if (required && IsEmpty(value))
        {
            return string.IsNullOrEmpty(requiredError) ? "Required" : requiredError;
        }

        return (validation ?? form?.Validation) switch
        {
            Func<T, string?> f => f(value),
            Func<T, bool> f => f(value) ? null : "Invalid",
            Func<T, IEnumerable<string>> f => f(value)?.FirstOrDefault(),
            Func<object, string, IEnumerable<string>> f when form?.Model is { } model && fieldName is not null
                => f(model, fieldName)?.FirstOrDefault(),
            _ => null,
        };
    }

    public static bool IsEmpty(object? value) => value switch
    {
        null => true,
        string s => string.IsNullOrWhiteSpace(s),
        System.Collections.IEnumerable e => !e.GetEnumerator().MoveNext(),
        _ => false,
    };
}
