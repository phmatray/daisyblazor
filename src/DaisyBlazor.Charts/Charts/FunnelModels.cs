namespace DaisyBlazor.Charts;

/// <summary>How a <see cref="FunnelChart"/> draws its stages.</summary>
public enum FunnelShape
{
    /// <summary>Equal-height stages, top to bottom, each as wide as its value and narrowing to the next.</summary>
    Funnel,

    /// <summary>A triangle cut into stages whose heights follow their values, the first stage at the base.</summary>
    Pyramid,

    /// <summary>Equal-height stages, top to bottom, each a centred rectangle as wide as its value, with no slanted sides.</summary>
    Rectangle,
}

/// <summary>Geometry of a <see cref="FunnelChart"/>.</summary>
public sealed record FunnelLayout(IReadOnlyList<FunnelLayoutStage> Stages);

/// <summary>
/// A placed stage (<paramref name="Index"/> is its position in the input): its outline, its <paramref name="Conversion"/> from the previous stage (null for
/// the first), its <paramref name="Share"/> of the first stage, and where its labels go.
/// </summary>
public sealed record FunnelLayoutStage(
    int Index, string Label, double Value, double? Conversion, double Share,
    double Y0, double Y1, string Path, double LabelY, double NameX, double ValueX);
