using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Data;

/// <summary>
/// daisyUI replacement for <c>MudDataGrid&lt;T&gt;</c>. Renders a <c>.table</c> with a sortable
/// header and a paged body. Supports client-side paging/sort over <see cref="Items"/> or
/// server-side data via <see cref="ServerData"/> / <see cref="ServerDataFunc"/>.
/// </summary>
/// <typeparam name="T">Row item type.</typeparam>
public partial class DataGrid<T>
{
    private readonly List<Column<T>> _columns = [];
    private List<T> _currentRows = [];
    private int _totalItems;
    private bool _initialServerLoadDone;

    /// <summary>Local item source. Mutually exclusive with the server-data callbacks.</summary>
    [Parameter]
    public IEnumerable<T>? Items { get; set; }

    /// <summary>Server-data loader (with cancellation). Mirrors the wrapper's signature.</summary>
    [Parameter]
    public Func<GridState<T>, CancellationToken, Task<GridData<T>>>? ServerData { get; set; }

    /// <summary>Server-data loader (no cancellation token).</summary>
    [Parameter]
    public Func<GridState<T>, Task<GridData<T>>>? ServerDataFunc { get; set; }

    /// <summary>Column declarations.</summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>Alternative slot for column declarations.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Pager content (typically a <see cref="DataGridPager{T}"/>).</summary>
    [Parameter]
    public RenderFragment? PagerContent { get; set; }

    /// <summary>Content shown when there are no rows.</summary>
    [Parameter]
    public RenderFragment? NoRecordsContent { get; set; }

    /// <summary>Text shown in the built-in empty state when there are no rows and no <see cref="NoRecordsContent"/>.</summary>
    [Parameter]
    public string EmptyText { get; set; } = "No records found";

    /// <summary>Content shown while <see cref="Loading"/> is true.</summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>Compact row styling.</summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>Highlight rows on hover.</summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>Render outer/inner borders.</summary>
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>Zebra striping.</summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>Loading state (shows <see cref="LoadingContent"/> when set, else skeleton rows).</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>Read-only flag (accepted for MudBlazor parity; no effect on rendering).</summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>Rows per page. <see cref="PageSize"/> is an alias.</summary>
    [Parameter]
    public int RowsPerPage { get; set; } = 10;

    /// <summary>Alias for <see cref="RowsPerPage"/>.</summary>
    [Parameter]
    public int? PageSize { get; set; }

    /// <summary>Page-size options offered by the pager.</summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 25, 50];

    /// <summary>Elevation (accepted for MudBlazor parity; mapped to a shadow class).</summary>
    [Parameter]
    public int Elevation { get; set; }

    /// <summary>Number of skeleton rows rendered while loading without a <see cref="LoadingContent"/>.</summary>
    [Parameter]
    public int SkeletonRows { get; set; } = 5;

    /// <summary>Raised when a row is clicked.</summary>
    [Parameter]
    public EventCallback<DataGridRowClickEventArgs<T>> RowClick { get; set; }

    /// <summary>Inline style applied to each body row (e.g. "cursor: pointer;").</summary>
    [Parameter]
    public string? RowStyle { get; set; }

    /// <summary>CSS class applied to the table wrapper.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline style applied to the table wrapper.</summary>
    [Parameter]
    public string? Style { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    /// <summary>Current zero-based page index.</summary>
    public int Page { get; private set; }

    /// <summary>Active sort definitions keyed by column identifier (mirrors MudBlazor).</summary>
    public Dictionary<string, SortDefinition<T>> SortDefinitions { get; } = [];

    /// <summary>Total number of rows across all pages.</summary>
    public int TotalItems => _totalItems;

    /// <summary>Effective rows per page.</summary>
    public int EffectivePageSize => PageSize ?? RowsPerPage;

    /// <summary>True when bound to a server-data source.</summary>
    public bool IsServerData => ServerData is not null || ServerDataFunc is not null;

    /// <summary>Index of the first row shown on the current page (1-based; 0 when empty).</summary>
    public int FirstItemIndex => _totalItems == 0 ? 0 : (Page * EffectivePageSize) + 1;

    /// <summary>Index of the last row shown on the current page.</summary>
    public int LastItemIndex => Math.Min((Page + 1) * EffectivePageSize, _totalItems);

    /// <summary>Total number of pages.</summary>
    public int PageCount => _totalItems == 0
        ? 1
        : (int)Math.Ceiling(_totalItems / (double)EffectivePageSize);

    /// <summary>Registers a column (called by child columns on init).</summary>
    public void AddColumn(Column<T> column)
    {
        if (!_columns.Contains(column))
        {
            _columns.Add(column);
            StateHasChanged();
        }
    }

    /// <summary>Forces a reload from the server-data source (no-op for client-side data).</summary>
    public async Task ReloadServerData()
    {
        if (IsServerData)
        {
            await LoadServerDataAsync();
            StateHasChanged();
        }
    }

    /// <summary>Sets the current page and refreshes the body.</summary>
    public async Task SetPageAsync(int page)
    {
        int target = Math.Clamp(page, 0, Math.Max(0, PageCount - 1));
        if (target == Page)
        {
            return;
        }

        Page = target;
        await RefreshAsync();
    }

    /// <summary>Lets a child pager supply the page-size options without externally setting the parameter.</summary>
    public void UsePageSizeOptions(int[] options)
    {
        if (options is { Length: > 0 })
        {
            PageSizeOptions = options;
        }
    }

    /// <summary>Changes the page size, resets to the first page, and refreshes.</summary>
    public async Task SetPageSizeAsync(int size)
    {
        if (size <= 0 || size == EffectivePageSize)
        {
            return;
        }

        if (PageSize is not null)
        {
            PageSize = size;
        }

        RowsPerPage = size;
        Page = 0;
        await RefreshAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsServerData)
        {
            if (!_initialServerLoadDone)
            {
                _initialServerLoadDone = true;
                await LoadServerDataAsync();
            }
        }
        else
        {
            ApplyClientData();
        }
    }

    private async Task RefreshAsync()
    {
        if (IsServerData)
        {
            await LoadServerDataAsync();
        }
        else
        {
            ApplyClientData();
        }

        StateHasChanged();
    }

    private async Task ToggleSortAsync(Column<T> column)
    {
        if (!column.CanSort)
        {
            return;
        }

        Func<T, object?>? func = column.GetSortFunc();
        if (func is null)
        {
            return;
        }

        string key = column.Identifier;

        if (SortDefinitions.TryGetValue(key, out SortDefinition<T>? existing))
        {
            if (!existing.Descending)
            {
                existing.Descending = true;
            }
            else
            {
                SortDefinitions.Remove(key);
            }
        }
        else
        {
            // Single-sort behaviour: replace any existing sort.
            SortDefinitions.Clear();
            SortDefinitions[key] = new SortDefinition<T>(
                key,
                descending: false,
                index: 0,
                sortFunc: item => func(item) ?? string.Empty);
        }

        Page = 0;
        await RefreshAsync();
    }

    private SortDirection GetSortDirection(Column<T> column)
    {
        if (SortDefinitions.TryGetValue(column.Identifier, out SortDefinition<T>? def))
        {
            return def.Descending ? SortDirection.Descending : SortDirection.Ascending;
        }

        return SortDirection.None;
    }

    private void ApplyClientData()
    {
        IEnumerable<T> source = Items ?? [];
        List<T> list = source.ToList();

        if (SortDefinitions.Count > 0)
        {
            IOrderedEnumerable<T>? ordered = null;
            foreach (SortDefinition<T> def in SortDefinitions.Values.OrderBy(d => d.Index))
            {
                if (ordered is null)
                {
                    ordered = def.Descending
                        ? list.OrderByDescending(def.SortFunc)
                        : list.OrderBy(def.SortFunc);
                }
                else
                {
                    ordered = def.Descending
                        ? ordered.ThenByDescending(def.SortFunc)
                        : ordered.ThenBy(def.SortFunc);
                }
            }

            if (ordered is not null)
            {
                list = ordered.ToList();
            }
        }

        _totalItems = list.Count;

        if (Page > 0 && Page >= PageCount)
        {
            Page = PageCount - 1;
        }

        _currentRows = list
            .Skip(Page * EffectivePageSize)
            .Take(EffectivePageSize)
            .ToList();
    }

    private async Task LoadServerDataAsync()
    {
        GridState<T> state = new()
        {
            Page = Page,
            PageSize = EffectivePageSize,
            SortDefinitions = SortDefinitions.Values.OrderBy(d => d.Index).ToList()
        };

        GridData<T> result;
        if (ServerData is not null)
        {
            result = await ServerData(state, CancellationToken.None);
        }
        else if (ServerDataFunc is not null)
        {
            result = await ServerDataFunc(state);
        }
        else
        {
            return;
        }

        _currentRows = result.Items.ToList();
        _totalItems = result.TotalItems;
    }

    private async Task OnRowClickedAsync(T item, int rowIndex)
    {
        if (RowClick.HasDelegate)
        {
            await RowClick.InvokeAsync(new DataGridRowClickEventArgs<T> { Item = item, RowIndex = rowIndex });
        }
    }

    private string _tableClass => CssClass.Merge(
        "table",
        Dense ? "table-sm" : null,
        Striped ? "table-zebra" : null,
        Class);

    private string? _rowClass => Hover ? "hover" : null;
}
