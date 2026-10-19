using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsSpecial() =>
    [
        new("PolarChart", Category.Charts,
            "Points at a numeric angle and radius, drawn as lines, splines, filled areas or markers over circular radius gridlines and angle spokes. The angle axis is any full turn: degrees, the 24 hours of a day, the months of a year. Tooltips give each point's angle and radius.",
            [typeof(PolarChart)],
            [
                new("Basic", "Each PolarSeries holds PolarPoint values (angle, radius). AngleMax=\"24\" makes a full turn a day, AngleStep spaces the spokes and AngleFormat writes their labels; Closed joins the last point back to the first.", typeof(PolarChartBasicExample)),
                new("Areas and splines", "Mode=\"PolarChartMode.Area\" fills each series (Marker draws dots only) and Smooth turns the lines into splines. StartAngle and Clockwise move and turn the angle axis.", typeof(PolarChartAreaExample)),
            ])
        {
            Portrait = typeof(PolarChartCoverExample),
        },

        new("ShapeChart", Category.Charts,
            "Polygons and polylines given in data coordinates on shared X and Y axes: a floor plan, a site map, zones on a diagram. Closed shapes are filled (an inner ring is a hole), open ones are drawn as lines; shapes of the same Group share a colour and a legend entry.",
            [typeof(ShapeChart)],
            [
                new("Basic", "Each ChartShape has a Name, its Rings of ShapePoint and an optional Group, here the rack's occupancy. KeepAspect makes a metre as long across as up.", typeof(ShapeChartBasicExample)),
                new("Holes and polylines", "A second ring inside a closed shape cuts a hole; Closed: false draws each ring as an open line, such as a cable run. Color takes any CSS colour and ShowGrid=\"false\" hides the grid.", typeof(ShapeChartCablesExample)),
            ])
        {
            Portrait = typeof(ShapeChartCoverExample),
        },

        new("SmithChart", Category.Charts,
            "The RF engineer's chart of impedance: each normalized impedance r + jx sits at its reflection coefficient Γ inside the unit circle, over a grid of constant-resistance circles and constant-reactance arcs. The centre is a perfect match, the right edge an open circuit, the left a short. Tooltips give z, |Γ| and its angle.",
            [typeof(SmithChart)],
            [
                new("Basic", "Each SmithSeries holds SmithPoint values (resistance, reactance, optional label), already divided by the reference impedance; the line follows the points in order.", typeof(SmithChartBasicExample)),
                new("Admittance grid", "Mode=\"SmithChartMode.Admittance\" mirrors the grid into circles of constant conductance and arcs of constant susceptance; the points stay where their reflection coefficient puts them. ShowLine=\"false\" keeps the markers only.", typeof(SmithChartAdmittanceExample)),
            ])
        {
            Portrait = typeof(SmithChartCoverExample),
        },

        new("Column3DChart", Category.Charts,
            "Columns (or bars) drawn as 3D prisms on a floor in front of a back wall, side by side or stacked, for a dashboard that wants depth. The top and side of each column are shaded from its colour with color-mix, so they follow any theme. D3 scales place the columns.",
            [typeof(Column3DChart)],
            [
                new("Basic", "Labels name the categories and each ChartSeries is a colour, one column per series in each category; the gridlines run across the back wall.", typeof(Column3DChartBasicExample)),
                new("Stacked, sideways", "Stacked=\"true\" piles the series into one column, Horizontal=\"true\" lays the bars out top to bottom, and Depth (0 to 1, a share of the column's width) sets how deep the prisms are.", typeof(Column3DChartStackedExample)),
            ])
        {
            Portrait = typeof(Column3DChartCoverExample),
        },

        new("ChoroplethMap", Category.Charts,
            "Values by country or region as shades of one colour on a map, such as active customers per country. Features is a GeoJSON FeatureCollection (a JsonElement); only each feature's key, name and geometry go to the layout, and d3-geo fits the projection to the box.",
            [typeof(ChoroplethMap)],
            [
                new("Basic", "Load the GeoJSON with HttpClient (GetFromJsonAsync<JsonElement>); Values is keyed by IdProperty, here the country name. Countries without a value get NoDataColor; the legend shows the range.", typeof(ChoroplethMapBasicExample)),
                new("Projection and colour", "Projection picks the d3-geo projection (Equal Earth keeps areas fair); with the default IdProperty the keys are the features' GeoJSON ids. Color and NoDataColor take any CSS colour.", typeof(ChoroplethMapProjectionExample)),
            ])
        {
            Portrait = typeof(ChoroplethMapCoverExample),
        },
    ];
}
