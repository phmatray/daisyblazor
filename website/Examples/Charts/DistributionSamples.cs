using DaisyBlazor.Charts;

namespace DaisyBlazor.Docs.Examples.Charts;

/// <summary>
/// Repeatable demo values for the distribution chart examples: a seeded generator, so every page load
/// (and every screenshot) shows the same data.
/// </summary>
public static class DistributionSamples
{
    /// <summary><paramref name="count"/> values around <paramref name="mean"/>, spread by <paramref name="sd"/> (normal distribution), no lower than <paramref name="min"/>.</summary>
    public static double[] Normal(int seed, int count, double mean, double sd, double min = double.MinValue, int decimals = 1)
    {
        Random rng = new(seed);
        return [.. Enumerable.Range(0, count).Select(_ => Math.Max(min, Math.Round(mean + sd * Gauss(rng), decimals)))];
    }

    /// <summary>Right-skewed values (log-normal) with the given median, as response times and durations usually are.</summary>
    public static double[] Skewed(int seed, int count, double median, double spread = 0.4)
    {
        Random rng = new(seed);
        return [.. Enumerable.Range(0, count).Select(_ => Math.Round(median * Math.Exp(spread * Gauss(rng)), 1))];
    }

    /// <summary>Points scattered around the line y = intercept + slope · x, with x spread normally around <paramref name="xMean"/>; both stay at or above zero, like the counts and durations they stand for.</summary>
    public static ScatterPoint[] Cloud(int seed, int count, double xMean, double xSd, double intercept, double slope, double noise, string? series = null)
    {
        Random rng = new(seed);
        return [.. Enumerable.Range(0, count).Select(_ =>
        {
            double x = Math.Max(0, Math.Round(xMean + xSd * Gauss(rng), 1));
            return new ScatterPoint(x, Math.Max(0, Math.Round(intercept + slope * x + noise * Gauss(rng), 1)), Series: series);
        })];
    }

    // Box-Muller transform.
    private static double Gauss(Random rng) =>
        Math.Sqrt(-2 * Math.Log(1 - rng.NextDouble())) * Math.Cos(2 * Math.PI * rng.NextDouble());
}
