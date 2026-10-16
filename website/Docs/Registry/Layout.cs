using DaisyBlazor.Docs.Examples.Layout;
using DaisyBlazor.Layout;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Layout() =>
    [
        new("Container", Category.Layout,
            "A centred, max-width wrapper for page content with optional side gutters. MaxWidth caps it at a Tailwind breakpoint; drops in for MudContainer.",
            [typeof(Container)],
            [
                new("Max width", "MaxWidth picks the breakpoint the column stops growing at; MaxWidth.False removes the cap so you can set your own.", typeof(ContainerMaxWidthExample)),
                new("Gutters", "Gutters (on by default) adds 1rem of side padding; turn it off when the child brings its own.", typeof(ContainerGuttersExample)),
            ])
        {
            MudEquivalent = "MudContainer",
            Portrait = typeof(ContainerCoverExample),
        },

        new("Grid", Category.Layout,
            "A 12-column CSS grid whose Item cells span columns per breakpoint (xs, sm, md, lg, xl). Use it for responsive page and card layouts; drops in for MudGrid.",
            [typeof(Grid), typeof(Item)],
            [
                new("Responsive spans", "Each Item takes a column span per breakpoint, so cells stack on phones and sit side by side on desktops.", typeof(GridResponsiveExample)),
                new("Spacing", "Spacing sets the gap between cells, on Tailwind's spacing scale.", typeof(GridSpacingExample)),
                new("Alignment", "AlignItems aligns cells of different heights along the row.", typeof(GridAlignExample)),
            ])
        {
            MudEquivalent = "MudGrid",
            Portrait = typeof(GridCoverExample),
        },

        new("Stack", Category.Layout,
            "A flex container with direction, gap, wrapping and alignment as parameters, so you rarely hand-write flex utilities. Column by default; drops in for MudStack.",
            [typeof(Stack)],
            [
                new("Direction", "Children stack in a column by default; Row lays them out horizontally.", typeof(StackDirectionExample)),
                new("Justify", "Justify distributes children along the main axis.", typeof(StackJustifyExample)),
                new("Wrap", "Wrap lets a row break onto several lines instead of overflowing.", typeof(StackWrapExample)),
            ])
        {
            MudEquivalent = "MudStack",
            Portrait = typeof(StackCoverExample),
        },

        new("Spacer", Category.Layout,
            "An empty flex-grow element that pushes its siblings apart inside a row. Use it in toolbars and headers; drops in for MudSpacer.",
            [typeof(Spacer)],
            [
                new("Toolbar", "One Spacer pushes the actions to the far end of the row.", typeof(SpacerToolbarExample)),
                new("Centring", "Two Spacers share the free space, centring whatever sits between them.", typeof(SpacerCenterExample)),
            ])
        {
            MudEquivalent = "MudSpacer",
            Portrait = typeof(SpacerCoverExample),
        },

        new("Divider", Category.Layout,
            "A daisyUI divider line, horizontal by default or vertical between side-by-side content, with an optional label in the middle. Drops in for MudDivider.",
            [typeof(Divider)],
            [
                new("Basic", "A plain rule between two blocks of content.", typeof(DividerBasicExample)),
                new("With text", "Child content sits in the middle of the line; daisyUI's divider-* classes colour it.", typeof(DividerTextExample)),
                new("Vertical", "Vertical draws the rule between siblings laid out in a row.", typeof(DividerVerticalExample)),
            ])
        {
            DaisyClass = "divider",
            MudEquivalent = "MudDivider",
            Portrait = typeof(DividerCoverExample),
        },

        new("Drawer", Category.Layout,
            "A daisyUI drawer: a side panel next to the main content, opened through two-way bound Open. Ships an accessible toggle button, closes on Escape and moves focus in and out of the panel.",
            [typeof(Drawer)],
            [
                new("Built-in toggle", "ToggleContent renders a real button wired to the panel with aria-expanded; bind Open to follow the state.", typeof(DrawerToggleExample)),
                new("From the end", "End slides the panel in from the right; open and close it from your own buttons.", typeof(DrawerEndExample)),
                new("Responsive sidebar", "Add lg:drawer-open to keep the panel docked on large screens, and hide the toggle there.", typeof(DrawerResponsiveExample)),
            ])
        {
            DaisyClass = "drawer",
            MudEquivalent = "MudDrawer",
            MigrationNote = "Drawer wraps both the panel and the page content, like daisyUI's drawer, rather than living inside a MudLayout.",
            Portrait = typeof(DrawerCoverExample),
        },

        new("Footer", Category.Layout,
            "A daisyUI footer that lays out nav sections, each headed by a FooterTitle. Choose its direction and centring, and colour it with a semantic Color.",
            [typeof(Footer), typeof(FooterTitle)],
            [
                new("Link columns", "Each nav becomes a column; Horizontal keeps them side by side at every width.", typeof(FooterColumnsExample)),
                new("Centred", "Center stacks and centres everything, here on a Primary background.", typeof(FooterCenterExample)),
                new("Vertical", "Horizontal=\"false\" forces a single column; Padding overrides the default p-10.", typeof(FooterVerticalExample)),
            ])
        {
            DaisyClass = "footer",
            Portrait = typeof(FooterCoverExample),
        },

        new("Hero", Category.Layout,
            "A daisyUI hero: a large, centred banner for a page's opening message. MinHeight sets its height (full screen by default) and BackgroundImage adds a photo behind an overlay.",
            [typeof(Hero)],
            [
                new("Centred", "A short hero with a headline, a line of copy and a call to action.", typeof(HeroBasicExample)),
                new("With a figure", "ContentClass styles the inner hero-content; flex-col md:flex-row puts the figure beside the text on wider screens.", typeof(HeroFigureExample)),
                new("Background image", "BackgroundImage sets the image as the hero's background and adds a hero-overlay above it for legible text.", typeof(HeroBackgroundExample)),
            ])
        {
            DaisyClass = "hero",
            Portrait = typeof(HeroCoverExample),
        },

        new("Paper", Category.Layout,
            "A base-100 surface with the theme's box radius and an elevation-driven shadow, or a border when Outlined. Everything from cards to sheets sits on it; drops in for MudPaper.",
            [typeof(Paper)],
            [
                new("Elevation", "Elevation maps to Tailwind's shadow scale; 0 removes the shadow.", typeof(PaperElevationExample)),
                new("Outlined and square", "Outlined swaps the shadow for a border; Square drops the rounding.", typeof(PaperOutlinedExample)),
                new("Clickable", "OnClick makes the whole surface a target.", typeof(PaperClickableExample)),
            ])
        {
            MudEquivalent = "MudPaper",
            MigrationNote = "Width, Height and MaxWidth are accepted but not applied yet; size the surface with Class or Style.",
            Portrait = typeof(PaperCoverExample),
            Playground = new(typeof(Paper), "Elevation", "Outlined", "Square")
            {
                Initial = new Dictionary<string, object?> { ["Elevation"] = 6, ["Class"] = "px-10 py-8 font-display text-xl font-semibold" },
                ChildText = "Monthly report",
            },
        },

        new("Mask", Category.Layout,
            "Clips its content into a daisyUI mask shape: squircle, circle, heart, star, hexagon and more. Use it for avatars, artwork and decorative badges; Half clips one half for ratings.",
            [typeof(Mask)],
            [
                new("Shapes", "Every MaskShape value; give the mask a size and a background or an image.", typeof(MaskShapesExample)),
                new("Around content", "Any content can be clipped, such as initials for team avatars.", typeof(MaskContentExample)),
                new("Half masks", "Half 1 and 2 keep the left or right half of the shape, which is how half-star scores are built.", typeof(MaskHalfExample)),
            ])
        {
            DaisyClass = "mask",
            Portrait = typeof(MaskCoverExample),
            Playground = new(typeof(Mask), "Shape")
            {
                Initial = new Dictionary<string, object?> { ["Shape"] = MaskShape.Heart, ["Class"] = "grid size-40 place-items-center bg-primary font-display text-xl font-bold text-primary-content" },
                ChildText = "Pro",
            },
        },

        new("ExpansionPanels", Category.Layout,
            "Collapsible daisyUI panels built on native details elements, so they work without JavaScript. One panel open at a time by default, or several with MultiExpansion; drops in for MudExpansionPanels.",
            [typeof(ExpansionPanels), typeof(ExpansionPanel)],
            [
                new("Accordion", "By default the panels share a group, so opening one closes the others; IsExpanded opens one up front.", typeof(ExpansionPanelsAccordionExample)),
                new("Multiple open", "MultiExpansion lets several panels stay open; Icon adds a leading symbol.", typeof(ExpansionPanelsMultiExample)),
                new("Custom title", "TitleContent replaces the plain Text title with any markup.", typeof(ExpansionPanelsTitleContentExample)),
            ])
        {
            Slug = "expansionpanels",
            DaisyClass = "collapse",
            MudEquivalent = "MudExpansionPanels",
            Portrait = typeof(ExpansionPanelsCoverExample),
        },

        new("PageHeader", Category.Layout,
            "A page-level header row: title, optional icon and subtitle on the left, action buttons on the right. HeadingLevel picks the heading element so the outline stays correct.",
            [typeof(PageHeader)],
            [
                new("Title and subtitle", "The minimal header: a title and a short line underneath.", typeof(PageHeaderBasicExample)),
                new("Icon and actions", "Icon leads the title; Actions holds right-aligned buttons.", typeof(PageHeaderActionsExample)),
                new("Heading level", "HeadingLevel (1 to 6, default 4) sets both the element and its size.", typeof(PageHeaderLevelExample)),
            ])
        {
            Portrait = typeof(PageHeaderCoverExample),
            Playground = new(typeof(PageHeader), "Title", "Subtitle", "Icon", "HeadingLevel")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Title"] = "Projects",
                    ["Subtitle"] = "Every project in your workspace",
                    ["Icon"] = Icons.Material.Filled.Folder,
                    ["HeadingLevel"] = 3,
                },
            },
        },

        new("DetailLayout", Category.Layout,
            "A detail-page scaffold: back button, title with a status slot, main content with an optional sidebar, and a related area. DetailCard and DetailRow fill it with label and value tables.",
            [typeof(DetailLayout), typeof(DetailCard), typeof(DetailRow)],
            [
                new("Detail page", "The full scaffold; BackRoute is where the back button navigates.", typeof(DetailLayoutPageExample)),
                new("Detail cards", "DetailCard is a titled, dense table of DetailRow; ValueContent renders anything in place of a plain Value.", typeof(DetailLayoutCardExample)),
            ])
        {
            Portrait = typeof(DetailLayoutCoverExample),
        },

        new("SectionCard", Category.Layout,
            "A Paper surface with an icon and title header, an optional rule and header actions, and a free-form body. Reach for it to group a section of a page; DetailCard is built on it.",
            [typeof(SectionCard)],
            [
                new("Basic", "A title, an icon and any body content.", typeof(SectionCardBasicExample)),
                new("Header actions", "HeaderActions puts controls opposite the title; wrap the body in ChildContent when you use it.", typeof(SectionCardActionsExample)),
                new("Compact and flat", "ShowDivider, TitleTypo and Elevation tune the chrome.", typeof(SectionCardCompactExample)),
            ])
        {
            Portrait = typeof(SectionCardCoverExample),
            Playground = new(typeof(SectionCard), "Title", "Icon", "ShowDivider", "TitleTypo", "Elevation")
            {
                Initial = new Dictionary<string, object?> { ["Title"] = "Release notes", ["Icon"] = Icons.Material.Filled.Description, ["Class"] = "max-w-sm" },
                ChildText = "Version 2.4 brings faster deploys and a new billing page.",
            },
        },
    ];
}
