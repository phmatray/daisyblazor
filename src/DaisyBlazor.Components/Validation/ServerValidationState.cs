namespace DaisyBlazor.Validation;

/// <summary>
/// Holds field-level validation errors returned by a server, and notifies the UI when they change.
/// </summary>
/// <remarks>
/// <para>
/// This deliberately knows nothing about any particular API contract — it takes plain
/// <c>field → message</c> pairs, so a caller can feed it from an ASP.NET
/// <c>ValidationProblemDetails</c>, a generated API client's validation DTO, or anything else,
/// without this library taking a dependency on that shape.
/// </para>
/// <para>
/// Typical use: submit, catch the failure, hand the errors over, and let
/// <see cref="ServerValidation"/> light up the fields.
/// <code>
/// _validation.SetErrors(problem.Errors.Select(e =&gt; new KeyValuePair&lt;string, string&gt;(e.Field, e.Message)));
/// </code>
/// </para>
/// <para>
/// Field lookup is <b>case-insensitive</b>, and falls back to matching the last dot-separated
/// segment — so a server that reports <c>request.Name</c> or <c>Model.Address.City</c> still
/// matches a field declared as <c>Name</c> or <c>City</c>. That is what makes the component usable
/// against real APIs, which rarely agree on how deeply they qualify field names.
/// </para>
/// </remarks>
public sealed class ServerValidationState
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised whenever the error set changes, so cascading consumers can re-render.</summary>
    public event Action? OnChange;

    /// <summary>True when at least one field currently carries an error.</summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>The field names currently carrying at least one error.</summary>
    public IReadOnlyCollection<string> Fields => _errors.Keys;

    /// <summary>Total number of error messages across all fields.</summary>
    public int Count => _errors.Sum(pair => pair.Value.Count);

    /// <summary>Replaces the whole error set with <paramref name="errors"/>.</summary>
    public void SetErrors(IEnumerable<KeyValuePair<string, string>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        _errors.Clear();
        foreach (KeyValuePair<string, string> error in errors)
        {
            AddCore(error.Key, error.Value);
        }

        OnChange?.Invoke();
    }

    /// <summary>Replaces the whole error set with <paramref name="errors"/> (tuple overload).</summary>
    public void SetErrors(IEnumerable<(string Field, string Message)> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        SetErrors(errors.Select(e => new KeyValuePair<string, string>(e.Field, e.Message)));
    }

    /// <summary>Adds one error message to a field, keeping any already recorded for it.</summary>
    public void AddError(string field, string message)
    {
        AddCore(field, message);
        OnChange?.Invoke();
    }

    /// <summary>Every message recorded for <paramref name="field"/>; empty when there is none.</summary>
    public IReadOnlyList<string> ErrorsFor(string? field)
    {
        if (string.IsNullOrWhiteSpace(field) || _errors.Count == 0)
        {
            return [];
        }

        if (_errors.TryGetValue(field, out List<string>? exact))
        {
            return exact;
        }

        // Fall back to the last dot-separated segment: a server reporting "request.Name"
        // should still light up a field declared as "Name".
        foreach (KeyValuePair<string, List<string>> pair in _errors)
        {
            if (LastSegment(pair.Key).Equals(field, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return [];
    }

    /// <summary>The first message recorded for <paramref name="field"/>, or <c>null</c>.</summary>
    public string? FirstErrorFor(string? field) => ErrorsFor(field).FirstOrDefault();

    /// <summary>True when <paramref name="field"/> carries at least one error.</summary>
    public bool HasErrorFor(string? field) => ErrorsFor(field).Count > 0;

    /// <summary>Clears every recorded error.</summary>
    public void Clear()
    {
        if (_errors.Count == 0)
        {
            return;
        }

        _errors.Clear();
        OnChange?.Invoke();
    }

    /// <summary>Clears the errors recorded for a single field.</summary>
    public void ClearField(string field)
    {
        if (!string.IsNullOrWhiteSpace(field) && _errors.Remove(field))
        {
            OnChange?.Invoke();
        }
    }

    private void AddCore(string field, string message)
    {
        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (!_errors.TryGetValue(field, out List<string>? messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    private static string LastSegment(string key)
    {
        int index = key.LastIndexOf('.');
        return index >= 0 && index < key.Length - 1 ? key[(index + 1)..] : key;
    }
}
