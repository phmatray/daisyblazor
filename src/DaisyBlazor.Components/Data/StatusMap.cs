namespace DaisyBlazor.Data;

/// <summary>The visual treatment a status value resolves to.</summary>
/// <param name="Color">Semantic colour for the chip.</param>
/// <param name="Icon">Optional leading Material Symbols ligature.</param>
public readonly record struct StatusStyle(Color Color, string? Icon = null);

/// <summary>
/// A declarative status → colour/icon map, so a status value can be rendered without every call
/// site computing its own colour.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="StatusChip"/> previously required <c>Color</c>, which meant it demanded the answer to
/// the only hard part and added nothing over a plain <see cref="Chip"/> — measurably so: in a
/// reference consumer it was used 4 times against 122 for <c>Chip</c>, with the colour supplied by
/// hand-written <c>switch</c> helpers duplicated across list, card and detail views.
/// </para>
/// <para>
/// The map deliberately covers colour and icon only. <b>Labels stay with the caller</b>, because in
/// real apps they are localized and this library has no localizer — trying to own them would make
/// the component unusable exactly where it matters.
/// </para>
/// <para>
/// Build one per status type, once, and reuse it:
/// <code>
/// public static readonly StatusMap FlightStatus = StatusMap.For&lt;FlightStatus&gt;()
///     .Add(FlightStatus.OnTime, Color.Success)
///     .Add([FlightStatus.Departed, FlightStatus.InFlight], Color.Primary)
///     .Add(FlightStatus.Delayed, Color.Warning, Icons.Material.Filled.Schedule)
///     .Add(FlightStatus.Cancelled, Color.Error)
///     .Default(Color.Default)
///     .Build();
/// </code>
/// </para>
/// </remarks>
public sealed class StatusMap
{
    private readonly IReadOnlyDictionary<object, StatusStyle> _entries;
    private readonly StatusStyle _fallback;

    internal StatusMap(IReadOnlyDictionary<object, StatusStyle> entries, StatusStyle fallback)
    {
        _entries = entries;
        _fallback = fallback;
    }

    /// <summary>Starts building a map for <typeparamref name="TStatus"/>.</summary>
    /// <remarks>
    /// The builder is generic so entries are type-checked at construction; the built map is not,
    /// so it can be passed to a non-generic component parameter without forcing every consumer of
    /// <see cref="StatusChip"/> to specify a type argument (which would break existing usage).
    /// </remarks>
    public static StatusMapBuilder<TStatus> For<TStatus>() where TStatus : notnull => new();

    /// <summary>
    /// The style for <paramref name="status"/>, or the configured default when it is <c>null</c>
    /// or unmapped.
    /// </summary>
    public StatusStyle Resolve(object? status) =>
        status is not null && _entries.TryGetValue(status, out StatusStyle style) ? style : _fallback;
}

/// <summary>Fluent builder for a <see cref="StatusMap"/>.</summary>
/// <typeparam name="TStatus">The status type being mapped — typically an enum.</typeparam>
public sealed class StatusMapBuilder<TStatus> where TStatus : notnull
{
    private readonly Dictionary<object, StatusStyle> _entries = [];
    private StatusStyle _fallback = new(Color.Default);

    /// <summary>Maps one status value.</summary>
    public StatusMapBuilder<TStatus> Add(TStatus value, Color color, string? icon = null)
    {
        _entries[value] = new StatusStyle(color, icon);
        return this;
    }

    /// <summary>Maps several status values to the same treatment.</summary>
    public StatusMapBuilder<TStatus> Add(IEnumerable<TStatus> values, Color color, string? icon = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (TStatus value in values)
        {
            _entries[value] = new StatusStyle(color, icon);
        }

        return this;
    }

    /// <summary>Treatment for unmapped or <c>null</c> values. Defaults to <see cref="Color.Default"/>.</summary>
    public StatusMapBuilder<TStatus> Default(Color color, string? icon = null)
    {
        _fallback = new StatusStyle(color, icon);
        return this;
    }

    /// <summary>Produces the immutable map.</summary>
    public StatusMap Build() => new(_entries, _fallback);
}
