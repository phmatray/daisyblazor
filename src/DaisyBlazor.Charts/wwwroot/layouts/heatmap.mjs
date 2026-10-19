import { scaleBand, scaleSequential, extent } from '../d3.mjs';
import { finite, textWidth, fit, r2 } from './_time.mjs';

const EMPTY = { rows: [], columns: [], cells: [], min: 0, max: 0, plot: { x0: 0, y0: 0, x1: 0, y1: 0 } };

// spec: { cells: [{ row, column, value }], rows, columns, width, height }
// Rows and columns are the given orders, or else the distinct names in order of first appearance.
// A repeated (row, column) pair is summed. Each cell gets t in [0, 1] on a sequential scale from
// the smallest to the largest value; the component turns t into a theme colour.
export default function heatmap({ cells, rows, columns, width, height }) {
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

    // Row names on the left, column names on top; both margins follow the longest name.
    const left = Math.min(width * 0.3, Math.max(...rowNames.map(n => textWidth(n)))) + 8;
    const plot = { x0: left, y0: 20, x1: width - 2, y1: height - 2 };
    const x = scaleBand().domain(colNames).range([plot.x0, plot.x1]).padding(0.06);
    const y = scaleBand().domain(rowNames).range([plot.y0, plot.y1]).padding(0.06);
    const [min, max] = extent(entries, e => e.value);
    const t = scaleSequential(v => v).domain([min, max > min ? max : min + 1]).clamp(true);

    // Column labels are thinned so neighbours never touch.
    const every = Math.max(1, Math.ceil((Math.max(...colNames.map(n => textWidth(n, 10))) + 6) / Math.max(1, x.step())));

    return {
        rows: rowNames.map(name => ({ name, label: fit(name, left - 8), position: r2(y(name) + y.bandwidth() / 2), show: true })),
        columns: colNames.map((name, i) => ({ name, label: name, position: r2(x(name) + x.bandwidth() / 2), show: i % every === 0 })),
        cells: entries.filter(e => x(e.column) !== undefined && y(e.row) !== undefined).map(e => ({
            row: e.row, column: e.column, value: e.value, t: r2(t(e.value)),
            x: r2(x(e.column)), y: r2(y(e.row)), w: r2(x.bandwidth()), h: r2(y.bandwidth()),
        })),
        min, max,
        plot,
    };
}
