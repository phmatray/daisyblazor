// spec: { items: [{ label, value }], total, rows, columns, width, height, gap }
// A rows × columns grid of cells filled row by row from the top left. Each item gets cells in
// proportion to value / total (largest remainder, so rounding never loses or invents a cell); total
// defaults to the sum of the values, and a larger one leaves the remaining cells empty (item -1).
export default function waffle({ items, total, rows = 10, columns = 10, width, height, gap = 4 }) {
    const valid = (items ?? []).filter(d => d && Number.isFinite(d.value) && d.value > 0);
    const sum = valid.reduce((s, d) => s + d.value, 0);
    const whole = Number.isFinite(total) && total > sum ? total : sum;
    rows = Math.max(1, Math.floor(rows) || 10);
    columns = Math.max(1, Math.floor(columns) || 10);
    if (valid.length === 0) {
        return { items: [], cells: [], size: 0 };
    }

    const n = rows * columns;
    const exact = valid.map(d => (d.value / whole) * n);
    const counts = exact.map(Math.floor);
    const target = Math.min(n, Math.round(exact.reduce((s, v) => s + v, 0)));
    const byRemainder = exact.map((v, i) => i).sort((a, b) => (exact[b] - counts[b]) - (exact[a] - counts[a]) || a - b);
    for (let k = 0, left = target - counts.reduce((s, v) => s + v, 0); k < left; k++) {
        counts[byRemainder[k % byRemainder.length]]++;
    }

    const size = Math.max(1, Math.min((width - gap * (columns - 1)) / columns, (height - gap * (rows - 1)) / rows));
    const x0 = (width - (size * columns + gap * (columns - 1))) / 2;
    const y0 = (height - (size * rows + gap * (rows - 1))) / 2;
    const owner = counts.flatMap((c, i) => Array(c).fill(i));

    return {
        size,
        items: valid.map((d, i) => ({ label: d.label, value: d.value, share: d.value / whole, cells: counts[i] })),
        cells: Array.from({ length: n }, (_, k) => ({
            x: x0 + (k % columns) * (size + gap),
            y: y0 + Math.floor(k / columns) * (size + gap),
            item: owner[k] ?? -1,
        })),
    };
}
