using System.Text;
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
    // The drilled node as its names below the root; carried as a list end to end (spec, layout,
    // clicks), so a name may hold '/'. Only the public Focus string escapes it.
    private IReadOnlyList<string> _focus = [];
    private string? _focusParameter;
    private (HierarchyLayout? Layout, HierarchyNode? Tree, IReadOnlyList<string>? Trail) _trail;

    /// <summary>
    /// Click (or Enter on) a node with children to show only that branch, at full size; a leaf below
    /// the first level opens its parent, so a treemap whose leaves cover their parents still drills.
    /// A breadcrumb above the chart goes back up. Needs an interactive render mode: under static rendering the
    /// chart shows <see cref="Focus"/> (or the whole tree) with no buttons.
    /// </summary>
    [Parameter] public bool Drilldown { get; set; }

    /// <summary>
    /// The node drawn as the root, as the names below the root joined by '/', e.g. "Platform/Logs";
    /// null for the whole tree. A '/' inside a name is written "\/" and a backslash "\\", so the
    /// node "A / B" under "CI/CD" is "CI\/CD/A \/ B" (FocusChanged reports paths the same way).
    /// Bind it with <c>@bind-Focus</c> to set or follow the drilled node. An unknown path shows the
    /// whole tree and a leaf shows its parent. Applies under static rendering too.
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

    /// <summary>The focus to send to the layout, as the names below the root, or null for the whole tree.</summary>
    protected IReadOnlyList<string>? SpecFocus => _focus.Count == 0 ? null : _focus;

    /// <summary>The breadcrumb's names, root first, once a drill-down chart is drawn interactively; null otherwise.</summary>
    protected IReadOnlyList<string>? Trail
    {
        get
        {
            if (!Drilldown || Layout is not { } layout || !RendererInfo.IsInteractive)
            {
                return null;
            }

            // Built once per layout (Mark reads it for every node).
            if (!ReferenceEquals(_trail.Layout, layout) || !ReferenceEquals(_trail.Tree, Tree))
            {
                _trail = (layout, Tree, [Tree?.Name ?? string.Empty, .. layout.Focus ?? []]);
            }

            return _trail.Trail;
        }
    }

    /// <summary>Classes for the SVG while nodes are buttons: labels let clicks through to their node.</summary>
    protected string? SvgClass => Interactive ? "[&_text]:pointer-events-none" : null;

    private bool Interactive => (Drilldown || OnNodeClick.HasDelegate) && RendererInfo.IsInteractive;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Focus != _focusParameter)
        {
            _focusParameter = Focus;
            _focus = ParseFocus(Focus);
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

        bool up = node.Depth == 0 && Layout?.Focus is { Count: > 0 };
        // A leaf opens its parent, unless that parent is already the drawn root.
        bool drills = Drilldown && (node.Depth == 0 ? up : !node.Leaf || node.Depth > 1);
        if (!drills && !OnNodeClick.HasDelegate)
        {
            return null;
        }

        return new Dictionary<string, object>
        {
            ["role"] = "button",
            ["tabindex"] = "0",
            ["aria-label"] = drills && up && Trail is { Count: > 1 } trail ? $"{label}. Back to {trail[^2]}" : label,
            ["class"] = "cursor-pointer focus-visible:outline-none focus-visible:stroke-base-content focus-visible:stroke-3 focus-visible:[stroke-opacity:1]",
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, () => Select(node, drills)),
            // ponytail: Space may also scroll the page; Blazor cannot cancel it for one key only.
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, e => e.Key is "Enter" or " " ? Select(node, drills) : Task.CompletedTask),
        };
    }

    /// <summary>Goes back to the breadcrumb at <paramref name="index"/> (0 is the root).</summary>
    protected Task Climb(int index) => Drill([.. (Layout?.Focus ?? []).Take(index)]);

    private async Task Select(HierarchyLayoutNode node, bool drills)
    {
        // Every node carries its names in the whole tree, the drawn root (the current focus) included.
        IReadOnlyList<string> path = node.Names;
        if (OnNodeClick.HasDelegate && Find(path) is { } found)
        {
            await OnNodeClick.InvokeAsync(found);
        }

        if (drills)
        {
            await Drill([.. node.Depth == 0 || node.Leaf ? path.Take(path.Count - 1) : path]);
        }
    }

    private async Task Drill(IReadOnlyList<string> path)
    {
        if (path.SequenceEqual(_focus))
        {
            return;
        }

        _focus = path;
        base.OnParametersSet();
        await FocusChanged.InvokeAsync(FormatFocus(path));
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

    // The public Focus string: names joined by '/', with '\' and '/' inside a name escaped by a '\'.
    private static string? FormatFocus(IReadOnlyList<string> names) =>
        names.Count == 0 ? null : string.Join('/', names.Select(n => n.Replace(@"\", @"\\").Replace("/", @"\/")));

    private static List<string> ParseFocus(string? path)
    {
        List<string> names = [];
        if (string.IsNullOrEmpty(path))
        {
            return names;
        }

        StringBuilder name = new();
        for (int i = 0; i < path.Length; i++)
        {
            char c = path[i];
            if (c == '\\' && i + 1 < path.Length)
            {
                name.Append(path[++i]);
            }
            else if (c == '/')
            {
                names.Add(name.ToString());
                name.Clear();
            }
            else
            {
                name.Append(c);
            }
        }

        names.Add(name.ToString());
        return names;
    }
}
