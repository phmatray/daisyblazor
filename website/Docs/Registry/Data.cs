using DaisyBlazor.Data;
using DaisyBlazor.Docs.Examples.Data;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Data() =>
    [
        new("DataGrid", Category.Data,
            "A typed, column-based grid on a daisyUI table with client-side sorting and paging, or server-side loading through ServerData. Mirrors MudDataGrid's shape: declare PropertyColumn and TemplateColumn children, add a DataGridPager for paging.",
            [typeof(DataGrid<string>), typeof(PropertyColumn<string, string>), typeof(TemplateColumn<string>), typeof(DataGridPager<string>)],
            [
                new("Property columns", "PropertyColumn binds a member by expression, is sortable by default and takes an optional Format string. Click a header to sort.", typeof(DataGridBasicExample)),
                new("Template columns", "TemplateColumn renders any markup per row through context.Item; pass SortBy to make it sortable.", typeof(DataGridTemplateColumnExample)),
                new("Paging and row clicks", "Put a DataGridPager in PagerContent and set RowsPerPage; RowClick hands you the clicked item.", typeof(DataGridPagingExample)),
                new("Styling and loading", "Dense, Striped, Bordered and Hover map to daisyUI table modifiers; Loading swaps the body for skeleton rows.", typeof(DataGridStylesExample)),
                new("Server data", "ServerDataFunc receives the page and sort state and returns only that page plus the total count.", typeof(DataGridServerDataExample)),
            ])
        {
            DaisyClass = "table",
            Portrait = typeof(DataGridCoverExample),
        },

        new("DataTable", Category.Data,
            "An opinionated DataGrid wrapper for list screens: hover and zebra rows, a built-in pager, skeleton loading, an empty state and an optional per-row Actions column.",
            [typeof(DataTable<string>)],
            [
                new("Items with row actions", "Declare columns as on DataGrid; the Actions template renders right-aligned per-row buttons, and paging comes built in.", typeof(DataTableBasicExample)),
                new("Loading and empty states", "Loading shows skeleton rows; with no items, EmptyIcon and EmptyMessage fill the body.", typeof(DataTableStatesExample)),
            ])
        {
            DaisyClass = "table",
            Portrait = typeof(DataTableCoverExample),
        },

        new("FilterPanel", Category.Data,
            "A ready-made filter surface: a title row, your filter inputs laid out on a 12-column Grid, removable active-filter chips and Clear/Filter buttons. You own the filter state; the panel raises OnApply, OnClear and OnRemoveFilter.",
            [typeof(FilterPanel)],
            [
                new("Filters with active chips", "Wrap inputs in Item cells; build the ActiveFilters list on apply and drop entries when a chip is closed.", typeof(FilterPanelBasicExample)),
                new("Custom labels", "Every piece of text is a parameter, so the panel localizes cleanly.", typeof(FilterPanelLabelsExample)),
            ])
        {
            Portrait = typeof(FilterPanelCoverExample),
        },

        new("KpiCard", Category.Data,
            "A headline-number tile: a caption, a large value and an optional subtitle, with a coloured left border. Color takes any CSS colour and defaults to the theme's primary.",
            [typeof(KpiCard)],
            [
                new("Basic", "Label and Value are required; Subtitle adds context under the number.", typeof(KpiCardBasicExample)),
                new("Colours", "Pass a daisyUI token such as var(--color-success) so the tile follows the active theme.", typeof(KpiCardColorsExample)),
            ])
        {
            Portrait = typeof(KpiCardCoverExample),
            Playground = new(typeof(KpiCard), "Label", "Value", "Subtitle", "Color")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Label"] = "Active users",
                    ["Value"] = "18,240",
                    ["Subtitle"] = "+12% vs. September",
                },
            },
        },

        new("StatusChip", Category.Data,
            "A filled chip for status values. Give it a Status and a StatusMap built once per status type, and it picks its own colour and icon; an explicit Label, Color or Icon always wins.",
            [typeof(StatusChip)],
            [
                new("Status map", "StatusMap.For<T>() maps each value to a colour and optional icon; the label falls back to the value's name.", typeof(StatusChipMapExample)),
                new("Overrides", "Labels stay with the caller (they are usually localized); Color and Icon override the map, and a chip works without one.", typeof(StatusChipOverridesExample)),
                new("Sizes", "Small (default), Medium and Large.", typeof(StatusChipSizesExample)),
            ])
        {
            DaisyClass = "badge",
            Portrait = typeof(StatusChipCoverExample),
            Playground = new(typeof(StatusChip), "Label", "Color", "Size", "Icon")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Label"] = "Shipped",
                    ["Color"] = Color.Primary,
                    ["Icon"] = Icons.Material.Filled.LocalShipping,
                },
            },
        },
    ];
}
