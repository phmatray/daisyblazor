namespace DaisyBlazor.Charts;

/// <summary>The map projection of a <see cref="ChoroplethMap"/> (all from d3-geo).</summary>
public enum MapProjection
{
    /// <summary>Natural Earth: a compromise projection that looks right for world maps.</summary>
    NaturalEarth,

    /// <summary>Equal Earth: an equal-area world projection, so areas compare fairly.</summary>
    EqualEarth,

    /// <summary>Mercator: the web-map projection; keeps shapes, inflates areas near the poles.</summary>
    Mercator,

    /// <summary>Equirectangular (plate carrée): longitude and latitude as plain x and y.</summary>
    Equirectangular,

    /// <summary>Albers: a conic equal-area projection for a mid-latitude region, set up for the United States.</summary>
    Albers,
}

/// <summary>Geometry of a <see cref="ChoroplethMap"/>: the projected features, their fills and the value range.</summary>
/// <param name="Sphere">Outline of the globe for a world map, else empty.</param>
/// <param name="Min">Lowest value found on the map, or null when no feature has one.</param>
/// <param name="Max">Highest value found on the map, or null when no feature has one.</param>
/// <param name="Low">Fill of the lowest value.</param>
/// <param name="High">Fill of the highest value.</param>
/// <param name="Features">The features with a visible shape.</param>
public sealed record ChoroplethLayout(string Sphere, double? Min, double? Max, string Low, string High, IReadOnlyList<ChoroplethFeature> Features);

/// <summary>A projected feature: its key, name, value (null for no data), SVG path and fill.</summary>
public sealed record ChoroplethFeature(string Id, string Name, double? Value, string Path, string Fill);
