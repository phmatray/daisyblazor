namespace DaisyBlazor;

/// <summary>
/// The built-in texts and accessible names of the components, used whenever a component's own parameter
/// (<c>CloseLabel</c>, <c>AriaLabel</c>, …) is not set. Set once with
/// <c>AddDaisyBlazor(o =&gt; o.Strings = new DaisyBlazorStrings { Close = "Fermer", … })</c>.
/// The properties are virtual and read on every render, so an app that switches culture at runtime can
/// subclass this and return <c>IStringLocalizer</c> lookups for the current culture.
/// </summary>
public class DaisyBlazorStrings
{
    internal static readonly DaisyBlazorStrings Default = new();

    /// <summary>Close button and backdrop of <c>Modal</c>, <c>DialogProvider</c>, <c>SnackbarProvider</c> and <c>Fab</c>.</summary>
    public virtual string Close { get; init; } = "Close";
    /// <summary>Overlay that closes a <c>Drawer</c>.</summary>
    public virtual string CloseSidebar { get; init; } = "close sidebar";
    /// <summary>Busy indicator of <c>Loading</c> and <c>ProgressCircular</c>.</summary>
    public virtual string Loading { get; init; } = "Loading";
    /// <summary>Navigation landmark of <c>Pagination</c>.</summary>
    public virtual string Pagination { get; init; } = "Pagination";
    /// <summary>First-page button of <c>Pagination</c>.</summary>
    public virtual string FirstPage { get; init; } = "First page";
    /// <summary>Previous-page button of <c>Pagination</c> and <c>DataGridPager</c>.</summary>
    public virtual string PreviousPage { get; init; } = "Previous page";
    /// <summary>Next-page button of <c>Pagination</c> and <c>DataGridPager</c>.</summary>
    public virtual string NextPage { get; init; } = "Next page";
    /// <summary>Last-page button of <c>Pagination</c>.</summary>
    public virtual string LastPage { get; init; } = "Last page";
    /// <summary>Page-size label of <c>DataGridPager</c>.</summary>
    public virtual string RowsPerPage { get; init; } = "Rows per page:";
    /// <summary>Separator of the <c>DataGridPager</c> range ("1-10 of 42").</summary>
    public virtual string Of { get; init; } = "of";
    /// <summary>Empty result of <c>Autocomplete</c>.</summary>
    public virtual string NoItemsFound { get; init; } = "No items found";
    /// <summary>Clear button of <c>Autocomplete</c>.</summary>
    public virtual string Clear { get; init; } = "Clear";

    internal static DaisyBlazorStrings From(IServiceProvider services) =>
        (services.GetService(typeof(DaisyBlazorOptions)) as DaisyBlazorOptions)?.Strings ?? Default;
}
