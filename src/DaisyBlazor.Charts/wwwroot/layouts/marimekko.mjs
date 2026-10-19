import { scaleLinear } from '../d3/d3-scale.mjs';
import { ticks } from './_shared.mjs';

// spec: { cells: [{ column, segment, value }], width, height, gap }
// Variable-width 100 % stacked columns: each column is as wide as its share of the grand total, and
// its segments stack bottom-up by their share of the column. Columns and segments keep the order in
// which they first appear; only positive values count.
export default function marimekko({ cells, width, height, gap = 2 }) {
    const valid = (cells ?? []).filter(c => c && c.column != null && c.segment != null && Number.isFinite(c.value) && c.value > 0);
    if (valid.length === 0) {
        return { columns: [], cells: [], segments: [], ticks: [], plot: null };
    }

    const columns = [...new Set(valid.map(c => c.column))];
    const segments = [...new Set(valid.map(c => c.segment))];
    const totals = columns.map(name => valid.filter(c => c.column === name).reduce((s, c) => s + c.value, 0));
    const grand = totals.reduce((s, v) => s + v, 0);

    // Room on the left for the 0–100 % ticks and two lines under the columns for name and share.
    const plot = { left: 44, top: 10, right: width - 6, bottom: height - 40 };
    const x = scaleLinear().domain([0, grand]).range([plot.left, plot.right]);
    const y = scaleLinear().domain([0, 1]).range([plot.bottom, plot.top]);

    let before = 0;
    const outColumns = columns.map((label, i) => {
        const x0 = x(before) + gap / 2, x1 = x(before + totals[i]) - gap / 2;
        before += totals[i];
        return { label, total: totals[i], share: totals[i] / grand, x0, x1: Math.max(x0 + 1, x1), labelX: (x0 + x1) / 2 };
    });

    const outCells = [];
    columns.forEach((name, ci) => {
        let stacked = 0;
        segments.forEach((segment, si) => {
            const value = valid.filter(c => c.column === name && c.segment === segment).reduce((s, c) => s + c.value, 0);
            if (value <= 0) {
                return;
            }
            const share = value / totals[ci];
            const col = outColumns[ci];
            const y0 = y(stacked + share), y1 = y(stacked);
            stacked += share;
            outCells.push({ column: ci, segment: si, value, share, x: col.x0, y: y0, width: col.x1 - col.x0, height: Math.max(0, y1 - y0 - (stacked < 1 - 1e-9 ? gap : 0)) });
        });
    });

    return {
        columns: outColumns,
        cells: outCells,
        segments,
        ticks: ticks(y, 4),
        plot,
    };
}
