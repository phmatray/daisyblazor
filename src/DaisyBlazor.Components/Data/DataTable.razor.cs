using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Data;

/// <summary>
/// Reusable <see cref="DataGrid{T}"/> wrapper with sensible defaults for server-side
/// paging, sort state, skeleton loading, and an empty-state slot.
/// </summary>
/// <typeparam name="T">Row item type.</typeparam>
public partial class DataTable<T>
{
    /// <summary>Column declarations (use <c>PropertyColumn</c>/<c>TemplateColumn</c>).</summary>
    [Parameter, EditorRequired]
    public RenderFragment Columns { get; set; } = null!;

    /// <summary>Optional per-row action template rendered as the last column.</summary>
    [Parameter]
    public RenderFragment<T>? Actions { get; set; }

    /// <summary>Server-data loader. Mutually exclusive with <see cref="Items"/>.</summary>
    [Parameter]
    public Func<GridState<T>, CancellationToken, Task<GridData<T>>>? ServerData { get; set; }

    /// <summary>Local items source. Mutually exclusive with <see cref="ServerData"/>.</summary>
    [Parameter]
    public IEnumerable<T>? Items { get; set; }

    /// <summary>Icon shown in the empty state. Defaults to <see cref="Icons.Material.Filled.SearchOff"/>.</summary>
    [Parameter]
    public string? EmptyIcon { get; set; }

    /// <summary>Empty-state message. Defaults to <c>"No records found"</c>; pass a localized string to override.</summary>
    [Parameter]
    public string? EmptyMessage { get; set; }

    /// <summary>When true, replaces row content with skeleton placeholders.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>When true, applies dense styling.</summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>Number of skeleton rows rendered while <see cref="Loading"/>.</summary>
    [Parameter]
    public int SkeletonRows { get; set; } = 5;

    /// <summary>Elevation for the grid surface.</summary>
    [Parameter]
    public int Elevation { get; set; } = 0;

    /// <summary>Page-size options shown in the pager.</summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 25, 50];

    /// <summary>Initial rows per page. Defaults to the first entry of <see cref="PageSizeOptions"/>.</summary>
    [Parameter]
    public int? RowsPerPage { get; set; }

    /// <summary>Header text of the <see cref="Actions"/> column. Defaults to <c>"Actions"</c>.</summary>
    [Parameter]
    public string ActionsTitle { get; set; } = "Actions";

    /// <summary>Additional CSS classes applied to the grid.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline style applied to the grid.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Arbitrary attributes splatted onto the grid.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    private DataGrid<T>? _grid;
    private int _rowsPerPage;
    private int? _lastRowsPerPage;
    private bool _pageSizeSeeded;

    protected override void OnParametersSet()
    {
        // Seed once (and again only if the caller changes RowsPerPage); afterwards the pager owns it.
        if (!_pageSizeSeeded || RowsPerPage != _lastRowsPerPage)
        {
            _rowsPerPage = RowsPerPage ?? (PageSizeOptions is { Length: > 0 } ? PageSizeOptions[0] : 10);
            _lastRowsPerPage = RowsPerPage;
            _pageSizeSeeded = true;
        }
    }

    /// <summary>Current sort definitions (proxied from the underlying grid).</summary>
    public Dictionary<string, SortDefinition<T>> SortDefinitions
        => _grid?.SortDefinitions ?? [];

    /// <summary>Forces the underlying grid to re-call <see cref="ServerData"/>.</summary>
    public async Task ReloadServerDataAsync()
    {
        if (_grid is not null)
        {
            await _grid.ReloadServerData();
        }
    }
}
