using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Charts;

/// <summary>
/// Base of the hierarchy charts that can drill down (<see cref="TreemapChart"/>,
/// <see cref="SunburstChart"/>, <see cref="IcicleChart"/>): the layout draws the node named by the
/// focus path as if it were the root, and clicks on parents move that focus.
/// </summary>
public abstract class HierarchyChart : D3Chart<HierarchyLayout>
{
    private string? _focus;
    private string? _focusParameter;

    /// <summary>
    /// Click (or Enter on) a node with children to show only that branch, at full size; a leaf below
    /// the first level opens its parent, so a treemap whose leaves cover their parents still drills.
    /// A breadcrumb above the chart goes back up. Needs an interactive render mode: under static rendering the
    /// chart shows <see cref="Focus"/> (or the whole tree) with no buttons.
    /// </summary>
    [Parameter] public bool Drilldown { get; set; }

    /// <summary>
    /// The node drawn as the root, as the names below the root joined by '/', e.g. "Platform/Logs";
    /// null for the whole tree. Bind it with <c>@bind-Focus</c> to set or follow the drilled node.
    /// An unknown path shows the whole tree and a leaf shows its parent. Applies under static rendering too.
    /// </summary>
    [Parameter] public string? Focus { get; set; }

    /// <summary>Raised with the new <see cref="Focus"/> when a click or the breadcrumb moves it (interactive render modes only).</summary>
    [Parameter] public EventCallback<string?> FocusChanged { get; set; }

    /// <summary>
    /// Raised with the clicked node of <c>Root</c>, leaves included, when set; nodes become buttons
    /// even without <see cref="Drilldown"/>. Needs an interactive render mode.
    /// </summary>
    [Parameter] public EventCallback<HierarchyNode> OnNodeClick { get; set; }

    /// <summary>The tree being drawn (the chart's <c>Root</c>).</summary>
    protected abstract HierarchyNode? Tree { get; }

    /// <summary>The focus path to send to the layout, or null for the whole tree.</summary>
    protected string? SpecFocus => string.IsNullOrEmpty(_focus) ? null : _focus;

    /// <summary>The breadcrumb's names, root first, once a drill-down chart is drawn interactively; null otherwise.</summary>
    protected IReadOnlyList<string>? Trail =>
        Drilldown && Layout is not null && RendererInfo.IsInteractive
            ? [Tree?.Name ?? string.Empty, .. Segments(Layout.Focus)]
            : null;

    /// <summary>Classes for the SVG while nodes are buttons: labels let clicks through to their node.</summary>
    protected string? SvgClass => Interactive ? "[&_text]:pointer-events-none" : null;

    private bool Interactive => (Drilldown || OnNodeClick.HasDelegate) && RendererInfo.IsInteractive;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Focus != _focusParameter)
        {
            _focusParameter = Focus;
            _focus = Focus;
        }

        base.OnParametersSet();
    }

    /// <summary>
    /// The button attributes of a drawn node (role, tab stop, accessible name, click and Enter/Space
    /// handlers), or null when the node does nothing on click. <paramref name="label"/> is its tooltip text.
    /// </summary>
    protected IReadOnlyDictionary<string, object>? Mark(HierarchyLayoutNode node, string label)
    {
        if (!Interactive)
        {
            return null;
        }

        bool up = node.Depth == 0 && !string.IsNullOrEmpty(Layout?.Focus);
        // A leaf opens its parent, unless that parent is already the drawn root.
        bool drills = Drilldown && (node.Depth == 0 ? up : !node.Leaf || node.Depth > 1);
        if (!drills && !OnNodeClick.HasDelegate)
        {
            return null;
        }

        IReadOnlyList<string>? trail = Trail;
        return new Dictionary<string, object>
        {
            ["role"] = "button",
            ["tabindex"] = "0",
            ["aria-label"] = drills && up && trail is { Count: > 1 } ? $"{label}. Back to {trail[^2]}" : label,
            ["class"] = "cursor-pointer focus-visible:outline-none focus-visible:stroke-base-content focus-visible:stroke-3 focus-visible:[stroke-opacity:1]",
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, () => Select(node, drills)),
            // ponytail: Space may also scroll the page; Blazor cannot cancel it for one key only.
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, e => e.Key is "Enter" or " " ? Select(node, drills) : Task.CompletedTask),
        };
    }

    /// <summary>Goes back to the breadcrumb at <paramref name="index"/> (0 is the root).</summary>
    protected Task Climb(int index) => Drill(string.Join('/', Segments(Layout?.Focus).Take(index)));

    private async Task Select(HierarchyLayoutNode node, bool drills)
    {
        // Paths join names with " / " in tooltips; the layout's root node is the current focus.
        List<string> path = node.Depth == 0 ? [.. Segments(Layout?.Focus)] : [.. node.Path.Split(" / ")];
        if (OnNodeClick.HasDelegate && Find(path) is { } found)
        {
            await OnNodeClick.InvokeAsync(found);
        }

        if (drills)
        {
            await Drill(string.Join('/', node.Depth == 0 || node.Leaf ? path.Take(path.Count - 1) : path));
        }
    }

    private async Task Drill(string path)
    {
        string? next = path.Length == 0 ? null : path;
        if (next == SpecFocus)
        {
            return;
        }

        _focus = next;
        base.OnParametersSet();
        await FocusChanged.InvokeAsync(next);
    }

    private HierarchyNode? Find(IEnumerable<string> path)
    {
        HierarchyNode? node = Tree;
        foreach (string name in path)
        {
            node = node?.Children?.FirstOrDefault(c => c.Name == name);
        }

        return node;
    }

    private static string[] Segments(string? path) =>
        string.IsNullOrEmpty(path) ? [] : path.Split('/');
}
