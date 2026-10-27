import { extent } from '../d3/d3-array.mjs';
import { scaleBand, scaleSequential, scaleQuantize } from '../d3/d3-scale.mjs';
import { finite, maxWidth, fit, r2, stride } from './_shared.mjs';

const EMPTY = { rows: [], columns: [], cells: [], min: 0, max: 0, plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { cells: [{ row, column, value }], rows, columns, min, max, width, height }
// Rows and columns are the given orders, or else the distinct names in order of first appearance.
// A repeated (row, column) pair is summed. Each cell gets t in [0, 1] on a sequential scale from
// min to max; the component turns t into a theme colour. min defaults to 0 (or the smallest value
// when one is negative), max to the largest value; values outside are clamped.
// levels (2 to 10) quantizes the values into that many equal steps between min and max: t is then
// level / (levels - 1), and the result carries the exact `thresholds` between the steps (levels - 1
// of them, unrounded). When min and max are the same there is a single step: no thresholds, and t is 1.
export default function heatmap({ cells, rows, columns, levels, min: lo, max: hi, width, height }) {
    const sums = new Map();
    for (const c of cells ?? []) {
        if (c && c.row != null && c.column != null && finite(c.value)) {
            const key = JSON.stringify([String(c.row), String(c.column)]);
            sums.set(key, (sums.get(key) ?? 0) + c.value);
        }
    }
    if (sums.size === 0) {
        return EMPTY;
    }

    const entries = [...sums].map(([key, value]) => {
        const [row, column] = JSON.parse(key);
        return { row, column, value };
    });
    const rowNames = rows?.length ? rows.map(String) : [...new Set(entries.map(e => e.row))];
    const colNames = columns?.length ? columns.map(String) : [...new Set(entries.map(e => e.column))];

    // Row names on the left, column names on top; both margins follow the longest name. Names end
    // 6 units left of the plot, with 6 more of slack for text wider than its estimate; they are
    // cut to the gutter itself (left - 12 could round below it and cut the longest name).
    const gutter = Math.min(width * 0.3, maxWidth(rowNames));
    const left = gutter + 12;
    const plot = { left, top: 20, right: width - 2, bottom: height - 2 };
    const x = scaleBand().domain(colNames).range([plot.left, plot.right]).padding(0.06);
    const y = scaleBand().domain(rowNames).range([plot.top, plot.bottom]).padding(0.06);
    const [low, high] = extent(entries, e => e.value);
    const min = finite(lo) ? lo : Math.min(0, low);
    const max = finite(hi) ? hi : high;
    const steps = finite(levels) ? Math.max(2, Math.min(10, Math.round(levels))) : null;
    // A flat domain with levels is a single step at full colour (its legend shows the one value).
    const flat = max <= min;
    const quantize = steps && !flat && scaleQuantize().domain([min, max]).range([...Array(steps).keys()]);
    const t = !steps ? scaleSequential(v => v).domain([min, max > min ? max : min + 1]).clamp(true)
        : flat ? () => 1 : v => quantize(v) / (steps - 1);

    // Column labels are thinned so neighbours never touch: a skipped column's label is empty.
    const every = stride(colNames, x.step(), 6, 10);

    const out = {
        rows: rowNames.map(name => ({ pos: r2(y(name) + y.bandwidth() / 2), label: fit(name, gutter), full: name })),
        columns: colNames.map((name, i) => ({ pos: r2(x(name) + x.bandwidth() / 2), label: i % every === 0 ? name : '', full: name })),
        cells: entries.filter(e => x(e.column) !== undefined && y(e.row) !== undefined).map(e => ({
            row: e.row, column: e.column, value: e.value, t: r2(t(e.value)),
            x: r2(x(e.column)), y: r2(y(e.row)), w: r2(x.bandwidth()), h: r2(y.bandwidth()),
        })),
        min, max,
        plot,
    };
    return steps ? { ...out, thresholds: flat ? [] : quantize.thresholds() } : out;
}
