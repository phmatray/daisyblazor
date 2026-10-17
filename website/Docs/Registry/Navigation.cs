using DaisyBlazor.Docs.Examples.Navigation;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Navigation() =>
    [
        new("Breadcrumbs", Category.Navigation,
            "A daisyUI breadcrumbs trail built from a List<BreadcrumbItem>. Items with an Href render as links, the last one is marked aria-current=\"page\", and long trails scroll horizontally.",
            [typeof(Breadcrumbs), typeof(BreadcrumbItem)],
            [
                new("Basic trail", "Pass the trail as Items; a null Href renders the current page as plain text.", typeof(BreadcrumbsBasicExample)),
                new("Icons", "Each BreadcrumbItem takes an optional Icon ligature shown before its text.", typeof(BreadcrumbsIconsExample)),
                new("Disabled items", "Disabled renders an item as dimmed text even when it has an Href.", typeof(BreadcrumbsDisabledExample)),
                new("Long trails", "When the trail is wider than its container it scrolls sideways instead of wrapping.", typeof(BreadcrumbsLongExample)),
            ])
        {
            DaisyClass = "breadcrumbs",
            MudEquivalent = "MudBreadcrumbs",
            MigrationNote = "Separator is accepted for source compatibility but ignored: daisyUI draws its own separators.",
            Portrait = typeof(BreadcrumbsCoverExample),
        },

        new("Dock", Category.Navigation,
            "A daisyUI dock: the bottom navigation bar of mobile apps, made of DockItem buttons or links with an icon and an optional label. It is fixed to the bottom of the viewport; add Class=\"relative\" to keep it inside a container.",
            [typeof(Dock), typeof(DockItem)],
            [
                new("Active item", "Active marks the selected DockItem; track it in your own state and set it from OnClick.", typeof(DockBasicExample)),
                new("Sizes", "Size maps to dock-xs through dock-xl; leave it null for daisyUI's default height.", typeof(DockSizesExample)),
                new("Icon-only links", "Href turns an item into a link; without a visible Label, AriaLabel names it for screen readers.", typeof(DockIconOnlyExample)),
            ])
        {
            DaisyClass = "dock",
            Portrait = typeof(DockCoverExample),
        },

        new("Menu", Category.Navigation,
            "A dropdown menu: an activator (a labelled button, an icon button, or your own markup) toggles a daisyUI menu of MenuItem rows. Open state lives in C#, an outside click closes it, and choosing an item dismisses it.",
            [typeof(Menu), typeof(MenuItem), typeof(MenuActivator)],
            [
                new("Label activator", "Label renders a ghost button with a dropdown arrow; each MenuItem takes an Icon, IconColor and OnClick.", typeof(MenuLabelExample)),
                new("Icon activator", "Icon renders an IconButton; Dense tightens the rows and a right AnchorOrigin aligns the list to the right edge.", typeof(MenuIconExample)),
                new("Custom activator", "ActivatorContent lets any trigger open the menu through context.ToggleAsync.", typeof(MenuActivatorExample)),
                new("Static list", "Outside a Menu, MenuItem renders a plain row of a daisyUI menu list.", typeof(MenuStaticListExample)),
            ])
        {
            DaisyClass = "menu",
            MudEquivalent = "MudMenu",
            Portrait = typeof(MenuCoverExample),
        },

        new("NavMenuLink", Category.Navigation,
            "A Blazor NavLink styled as a daisyUI menu row, for sidebars and nav menus. It highlights itself with menu-active and aria-current=\"page\" when the route matches; wrap each one in an <li> inside a ul.menu.",
            [typeof(NavMenuLink)],
            [
                new("Sidebar", "The link to the current page lights up on its own; try the links to move around the docs.", typeof(NavMenuLinkBasicExample)),
                new("Match", "Prefix (default) stays active on every page under the Href; NavLinkMatch.All needs an exact match.", typeof(NavMenuLinkMatchExample)),
                new("Target and disabled", "Target opens a link elsewhere; Disabled renders a dimmed, non-clickable row.", typeof(NavMenuLinkStatesExample)),
            ])
        {
            DaisyClass = "menu",
            MudEquivalent = "MudNavLink",
            MigrationNote = "Named NavMenuLink rather than NavLink to avoid clashing with the framework's NavLink.",
            Portrait = typeof(NavMenuLinkCoverExample),
        },

        new("Navbar", Category.Navigation,
            "A daisyUI navbar with Start, Center and End regions for app bars and site headers. Color sets the background, Bordered and Shadow separate it from the page, and ChildContent takes over when no region is given.",
            [typeof(Navbar)],
            [
                new("Regions", "Start, Center and End map to navbar-start, navbar-center and navbar-end; only the ones you set are rendered.", typeof(NavbarRegionsExample)),
                new("Colors", "Color maps to bg-primary, bg-secondary and friends; add the matching text-*-content class for readable ink.", typeof(NavbarColorsExample)),
                new("Custom content", "Without any region, ChildContent fills the bar directly, here a task review strip with a bottom border.", typeof(NavbarCustomExample)),
            ])
        {
            DaisyClass = "navbar",
            MudEquivalent = "MudAppBar",
            Portrait = typeof(NavbarCoverExample),
            Playground = new(typeof(Navbar), "Color", "Bordered", "Shadow")
            {
                Initial = new Dictionary<string, object?> { ["Shadow"] = true },
                ChildText = "Acme Cloud",
            },
        },

        new("Pagination", Category.Navigation,
            "Page navigation as a daisyUI join of buttons. Selected is 1-based and two-way bindable, and long ranges collapse into ellipses around the current page.",
            [typeof(Pagination)],
            [
                new("Binding", "Bind Selected to drive your own paging; Count is the number of pages.", typeof(PaginationBasicExample)),
                new("Colors", "Color paints the current page with the matching btn colour.", typeof(PaginationColorsExample)),
                new("Ranges and jumps", "ShowFirstButton and ShowLastButton add jump buttons; BoundaryCount and MiddleCount shape the visible range.", typeof(PaginationRangeExample)),
                new("Disabled", "Disabled freezes every button, for example while a page is loading.", typeof(PaginationDisabledExample)),
            ])
        {
            DaisyClass = "join",
            MudEquivalent = "MudPagination",
            Portrait = typeof(PaginationCoverExample),
            Playground = new(typeof(Pagination), "Count", "Selected", "Color", "ShowFirstButton", "ShowLastButton", "BoundaryCount", "MiddleCount", "Disabled")
            {
                Initial = new Dictionary<string, object?> { ["Count"] = 10, ["Selected"] = 4, ["Color"] = Color.Primary },
            },
        },

        new("Steps", Category.Navigation,
            "A daisyUI steps tracker for wizards, checkouts and timelines: a row (or column) of Step markers. Completed and Active give quick colours, Color and DataContent customise each marker.",
            [typeof(Steps), typeof(Step)],
            [
                new("Wizard", "Derive Completed and Active from your current step; the markers update as you move.", typeof(StepsWizardExample)),
                new("Colors and markers", "Color sets step-success, step-error and so on; DataContent swaps the number for a symbol.", typeof(StepsColorsExample)),
                new("Vertical", "Vertical stacks the steps in a column, handy in narrow side panels.", typeof(StepsVerticalExample)),
            ])
        {
            DaisyClass = "steps",
            Portrait = typeof(StepsCoverExample),
        },

        new("Tabs", Category.Navigation,
            "Accessible tabs: each Tab child declares a Title and its panel content, and the container renders the daisyUI tab strip plus the active panel. ActiveIndex is two-way bindable and the arrow, Home and End keys move between tabs.",
            [typeof(Tabs), typeof(Tab)],
            [
                new("Variants", "Box (default), Border and Lift map to tabs-box, tabs-border and tabs-lift.", typeof(TabsVariantsExample)),
                new("Binding and disabled", "Bind ActiveIndex to switch tabs from code; a Disabled tab cannot be selected.", typeof(TabsBindingExample)),
                new("Sizes", "Size maps to tabs-sm, tabs-md and tabs-lg.", typeof(TabsSizesExample)),
                new("Icons and counts", "TitleContent renders an icon or a count badge in the tab button; Title then becomes its aria-label.", typeof(TabsTitleContentExample)),
            ])
        {
            DaisyClass = "tabs",
            MudEquivalent = "MudTabs",
            Portrait = typeof(TabsCoverExample),
        },
    ];
}
