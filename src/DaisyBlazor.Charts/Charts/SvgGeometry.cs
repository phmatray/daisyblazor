using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DaisyBlazor.Charts;

/// <summary>
/// Pure-math helpers that turn point lists into SVG path data. No DOM, no JS — every chart
/// component composes its geometry from these so the math stays in one testable place.
/// </summary>
internal static class SvgGeometry
{
    /// <summary>Builds a straight "M …L …" polyline path through the given points.</summary>
    public static string PolyPath(IReadOnlyList<(double X, double Y)> points, CultureInfo ci)
    {
        if (points is null || points.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();
        for (int i = 0; i < points.Count; i++)
        {
            sb.Append(i == 0 ? "M " : " L ");
            sb.Append(points[i].X.ToString(ci));
            sb.Append(' ');
            sb.Append(points[i].Y.ToString(ci));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a smoothed cubic-Bézier path through the points using monotone cubic interpolation
    /// (Fritsch-Carlson). Unlike Catmull-Rom it never overshoots the neighbouring values, so a
    /// curve cannot dip below the baseline or bulge above the plot top. Falls back to a straight
    /// path for fewer than three points.
    /// </summary>
    public static string SmoothPath(IReadOnlyList<(double X, double Y)> points, CultureInfo ci)
    {
        if (points is null || points.Count < 3)
        {
            return PolyPath(points!, ci);
        }

        int n = points.Count;
        double[] secant = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            double dx = points[i + 1].X - points[i].X;
            secant[i] = dx == 0 ? 0 : (points[i + 1].Y - points[i].Y) / dx;
        }

        double[] tangent = new double[n];
        tangent[0] = secant[0];
        tangent[n - 1] = secant[n - 2];
        for (int i = 1; i < n - 1; i++)
        {
            tangent[i] = secant[i - 1] * secant[i] <= 0 ? 0 : (secant[i - 1] + secant[i]) / 2;
        }

        for (int i = 0; i < n - 1; i++)
        {
            if (secant[i] == 0)
            {
                tangent[i] = 0;
                tangent[i + 1] = 0;
                continue;
            }

            double a = tangent[i] / secant[i];
            double b = tangent[i + 1] / secant[i];
            double s = a * a + b * b;
            if (s > 9)
            {
                double tau = 3 / System.Math.Sqrt(s);
                tangent[i] = tau * a * secant[i];
                tangent[i + 1] = tau * b * secant[i];
            }
        }

        StringBuilder sb = new();
        sb.Append("M ").Append(points[0].X.ToString(ci)).Append(' ').Append(points[0].Y.ToString(ci));
        for (int i = 0; i < n - 1; i++)
        {
            double third = (points[i + 1].X - points[i].X) / 3;
            sb.Append(" C ")
              .Append((points[i].X + third).ToString(ci)).Append(' ')
              .Append((points[i].Y + tangent[i] * third).ToString(ci)).Append(' ')
              .Append((points[i + 1].X - third).ToString(ci)).Append(' ')
              .Append((points[i + 1].Y - tangent[i + 1] * third).ToString(ci)).Append(' ')
              .Append(points[i + 1].X.ToString(ci)).Append(' ')
              .Append(points[i + 1].Y.ToString(ci));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Rounds a value range out to "nice" axis bounds (steps of 1, 2 or 5 x 10^n) with at most
    /// about <paramref name="maxTicks"/> ticks, so axes read 0/10/20/30 instead of 0/11/22/33.
    /// </summary>
    public static (double Min, double Max, double Step) NiceScale(double min, double max, int maxTicks = 5)
    {
        if (!(max > min))
        {
            max = min + 1;
        }

        double range = NiceNumber(max - min, round: false);
        double step = NiceNumber(range / (maxTicks - 1), round: true);
        double niceMin = System.Math.Floor(min / step) * step;
        double niceMax = System.Math.Ceiling(max / step) * step;
        return (niceMin, niceMax, step);
    }

    private static double NiceNumber(double x, bool round)
    {
        double exp = System.Math.Floor(System.Math.Log10(x));
        double pow = System.Math.Pow(10, exp);
        double f = x / pow;
        double nice = round
            ? (f < 1.5 ? 1 : f < 3 ? 2 : f < 7 ? 5 : 10)
            : (f <= 1 ? 1 : f <= 2 ? 2 : f <= 5 ? 5 : 10);
        return nice * pow;
    }

    /// <summary>Tick values from <paramref name="min"/> to <paramref name="max"/> in <paramref name="step"/> increments.</summary>
    public static List<double> Ticks(double min, double max, double step)
    {
        List<double> ticks = new();
        int count = (int)System.Math.Round((max - min) / step);
        for (int i = 0; i <= count; i++)
        {
            ticks.Add(System.Math.Round(min + step * i, 10));
        }

        return ticks;
    }

    /// <summary>Compact axis label: 1500 -> "1.5k", 2400000 -> "2.4M", 2.5 -> "2.5".</summary>
    public static string FormatTick(double value, CultureInfo ci)
    {
        double abs = System.Math.Abs(value);
        if (abs >= 1_000_000)
        {
            return (value / 1_000_000).ToString("0.##", ci) + "M";
        }

        if (abs >= 1_000)
        {
            return (value / 1_000).ToString("0.##", ci) + "k";
        }

        return value.ToString("0.##", ci);
    }

    /// <summary>Formats a raw data value for tooltips and screen-reader descriptions.</summary>
    public static string FormatValue(double value, CultureInfo ci) => value.ToString("#,0.##", ci);

    /// <summary>Point on a circle of <paramref name="radius"/> at <paramref name="angleRad"/> (0 = 12 o'clock, clockwise).</summary>
    public static (double X, double Y) PointOnCircle(double cx, double cy, double radius, double angleRad)
    {
        // SVG y grows downward; offset by -90° so 0 starts at the top and sweeps clockwise.
        double x = cx + radius * System.Math.Sin(angleRad);
        double y = cy - radius * System.Math.Cos(angleRad);
        return (x, y);
    }

    /// <summary>
    /// Builds an SVG arc/wedge path for a slice spanning [<paramref name="startRad"/>,
    /// <paramref name="endRad"/>]. When <paramref name="innerRadius"/> &gt; 0 a donut ring segment
    /// is produced; otherwise a pie wedge from the center.
    /// </summary>
    public static string ArcPath(double cx, double cy, double outerRadius, double innerRadius, double startRad, double endRad, CultureInfo ci)
    {
        bool largeArc = (endRad - startRad) > System.Math.PI;
        int large = largeArc ? 1 : 0;

        (double X, double Y) oStart = PointOnCircle(cx, cy, outerRadius, startRad);
        (double X, double Y) oEnd = PointOnCircle(cx, cy, outerRadius, endRad);

        StringBuilder sb = new();

        if (innerRadius <= 0)
        {
            // Pie wedge: center -> outer start -> arc -> close.
            sb.Append("M ").Append(cx.ToString(ci)).Append(' ').Append(cy.ToString(ci));
            sb.Append(" L ").Append(oStart.X.ToString(ci)).Append(' ').Append(oStart.Y.ToString(ci));
            sb.Append(" A ").Append(outerRadius.ToString(ci)).Append(' ').Append(outerRadius.ToString(ci))
              .Append(" 0 ").Append(large).Append(" 1 ")
              .Append(oEnd.X.ToString(ci)).Append(' ').Append(oEnd.Y.ToString(ci));
            sb.Append(" Z");
        }
        else
        {
            // Donut ring segment: outer arc forward, inner arc back.
            (double X, double Y) iStart = PointOnCircle(cx, cy, innerRadius, startRad);
            (double X, double Y) iEnd = PointOnCircle(cx, cy, innerRadius, endRad);

            sb.Append("M ").Append(oStart.X.ToString(ci)).Append(' ').Append(oStart.Y.ToString(ci));
            sb.Append(" A ").Append(outerRadius.ToString(ci)).Append(' ').Append(outerRadius.ToString(ci))
              .Append(" 0 ").Append(large).Append(" 1 ")
              .Append(oEnd.X.ToString(ci)).Append(' ').Append(oEnd.Y.ToString(ci));
            sb.Append(" L ").Append(iEnd.X.ToString(ci)).Append(' ').Append(iEnd.Y.ToString(ci));
            sb.Append(" A ").Append(innerRadius.ToString(ci)).Append(' ').Append(innerRadius.ToString(ci))
              .Append(" 0 ").Append(large).Append(" 0 ")
              .Append(iStart.X.ToString(ci)).Append(' ').Append(iStart.Y.ToString(ci));
            sb.Append(" Z");
        }

        return sb.ToString();
    }
}
