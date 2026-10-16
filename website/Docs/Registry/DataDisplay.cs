using DaisyBlazor.Docs.Examples.DataDisplay;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> DataDisplay() =>
    [
        new("Avatar", Category.DataDisplay,
            "A round daisyUI avatar for people and things. Shows an image when Image is set, otherwise a coloured placeholder with initials or an icon; drops in for MudAvatar.",
            [typeof(Avatar)],
            [
                new("Images", "Image and Alt render a photo; Size picks Small, Medium or Large.", typeof(AvatarImagesExample)),
                new("Initials and icons", "Without an image, ChildContent sits on a semantic Color.", typeof(AvatarInitialsExample)),
                new("Group", "Wrap avatars in daisyUI's avatar-group to stack them with an overlap.", typeof(AvatarGroupExample)),
            ])
        {
            DaisyClass = "avatar",
            MudEquivalent = "MudAvatar",
            Portrait = typeof(AvatarCoverExample),
            Playground = new(typeof(Avatar), "Color", "Size", "Image")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Size"] = Size.Large },
                ChildText = "AS",
            },
        },

        new("Badge", Category.DataDisplay,
            "A small count or label. By default it overlays its child as a daisyUI indicator (numbers above Max show as \"99+\"); set Inline for a plain badge in the text flow.",
            [typeof(Badge)],
            [
                new("Inline", "Inline renders a standalone badge; Color follows the theme.", typeof(BadgeInlineExample)),
                new("Overlay", "Wrap any element to pin a count on it; Max caps numbers, Dot shows a plain dot and Bordered rings it.", typeof(BadgeOverlayExample)),
                new("Visibility", "Bind Visible to hide the badge when there is nothing to count.", typeof(BadgeVisibilityExample)),
            ])
        {
            DaisyClass = "badge",
            MudEquivalent = "MudBadge",
            Portrait = typeof(BadgeCoverExample),
            Playground = new(typeof(Badge), "Color", "Inline", "Dot", "Bordered", "Visible")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Secondary, ["Inline"] = true },
                ChildText = "New",
            },
        },

        new("Card", Category.DataDisplay,
            "A daisyUI card surface that groups a header, body and actions. Outlined swaps the shadow for a border, Elevation sets the depth, and OnClick makes the whole card keyboard-accessible.",
            [typeof(Card), typeof(CardHeader), typeof(CardContent), typeof(CardActions)],
            [
                new("Header, content and actions", "CardHeader takes CardHeaderAvatar and CardHeaderContent slots, as in MudBlazor.", typeof(CardBasicExample)),
                new("With images", "Put a figure first for a media card, or add daisyUI's image-full class to lay the text over it.", typeof(CardImageExample)),
                new("Surfaces", "Outlined, flat and raised cards from the same component.", typeof(CardSurfacesExample)),
                new("Clickable", "With OnClick the card gets button semantics and answers Enter and Space.", typeof(CardClickableExample)),
            ])
        {
            DaisyClass = "card",
            MudEquivalent = "MudCard",
            Portrait = typeof(CardCoverExample),
        },

        new("Carousel", Category.DataDisplay,
            "A CSS scroll-snap carousel: CarouselItems scroll sideways (or up and down with Vertical) and snap into place, with no JavaScript.",
            [typeof(Carousel), typeof(CarouselItem)],
            [
                new("Basic", "Items keep their natural width and snap to the start.", typeof(CarouselBasicExample)),
                new("Snap and width", "Snap centres each stop; Width sizes the items so the next one peeks in.", typeof(CarouselSnapExample)),
                new("Vertical", "Vertical stacks the items and scrolls up and down; give the carousel a height.", typeof(CarouselVerticalExample)),
            ])
        {
            DaisyClass = "carousel",
            Portrait = typeof(CarouselCoverExample),
        },

        new("ChatBubble", Category.DataDisplay,
            "One message in a daisyUI chat thread. Placement puts it on the start or end side; Image, Header and Footer add an avatar, sender line and delivery status.",
            [typeof(ChatBubble)],
            [
                new("Conversation", "Alternate Placement between Start and End to build a thread.", typeof(ChatBubbleConversationExample)),
                new("Colors", "Color maps to chat-bubble-* so bubbles follow the theme.", typeof(ChatBubbleColorsExample)),
                new("Avatar, header and footer", "Image takes any fragment; Header and Footer are plain text.", typeof(ChatBubbleDetailsExample)),
            ])
        {
            Slug = "chatbubble",
            DaisyClass = "chat",
            Portrait = typeof(ChatBubbleCoverExample),
            Playground = new(typeof(ChatBubble), "Placement", "Color", "Header", "Footer")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Header"] = "Tom · 18:04", ["Footer"] = "Seen" },
                ChildText = "Staging is green, deploying at nine.",
            },
        },

        new("Chip", Category.DataDisplay,
            "A compact label built on daisyUI's badge, with Color, Variant, Size and a leading Icon. Becomes a button with OnClick and gets a close button with OnClose; replaces MudChip.",
            [typeof(Chip)],
            [
                new("Colors", "Every semantic Color, from Default to Dark.", typeof(ChipColorsExample)),
                new("Variants and sizes", "Filled, Outlined and Text (soft), in three sizes.", typeof(ChipVariantsExample)),
                new("Closable", "Handle OnClose to show the close button; the chip passes itself so you can read its Value.", typeof(ChipClosableExample)),
                new("Filter chips", "OnClick turns a chip into a toggle button.", typeof(ChipFilterExample)),
            ])
        {
            DaisyClass = "badge",
            MudEquivalent = "MudChip",
            Portrait = typeof(ChipCoverExample),
            Playground = new(typeof(Chip), "Color", "Variant", "Size", "Icon")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Size"] = Size.Large, ["Icon"] = Icons.Material.Filled.Tag },
                ChildText = "Billing",
            },
        },

        new("Countdown", Category.DataDisplay,
            "An animated two-digit counter cell (0–99) using daisyUI's countdown. Combine cells for clocks; Countdown.Digits splits seconds into hours, minutes and seconds.",
            [typeof(Countdown)],
            [
                new("Cells with labels", "One Countdown per unit; Label prints underneath.", typeof(CountdownBasicExample)),
                new("Live timer", "Tick a value from a timer and the digits roll; Digits does the arithmetic.", typeof(CountdownLiveExample)),
                new("Styled boxes", "Size and surface come from ordinary utilities.", typeof(CountdownBoxesExample)),
            ])
        {
            DaisyClass = "countdown",
            Portrait = typeof(CountdownCoverExample),
            Playground = new(typeof(Countdown), "Value", "Label")
            {
                Initial = new Dictionary<string, object?> { ["Value"] = 42, ["Label"] = "sec" },
            },
        },

        new("Diff", Category.DataDisplay,
            "A before/after comparison with a draggable divider, built on daisyUI's diff. Item1 and Item2 take any content; AspectClass sets the frame ratio.",
            [typeof(Diff)],
            [
                new("Images", "The classic use: two versions of the same picture.", typeof(DiffImagesExample)),
                new("Any content", "Panes can hold text or components: style the same word twice and drag to reveal it. AspectClass changes the ratio.", typeof(DiffTextExample)),
            ])
        {
            DaisyClass = "diff",
            Portrait = typeof(DiffCoverExample),
        },

        new("Kbd", Category.DataDisplay,
            "A keyboard key cap for shortcuts and instructions, with three sizes. Use it inline in text or combine keys for chords.",
            [typeof(Kbd)],
            [
                new("Sizes", "Small, Medium (default) and Large.", typeof(KbdSizesExample)),
                new("Shortcuts", "Join keys with a plus, or drop one into a sentence.", typeof(KbdShortcutsExample)),
                new("Arrow keys", "Lay keys out with flex to draw a cluster.", typeof(KbdArrowsExample)),
            ])
        {
            DaisyClass = "kbd",
            Portrait = typeof(KbdCoverExample),
            Playground = new(typeof(Kbd), "Size")
            {
                Initial = new Dictionary<string, object?> { ["Size"] = Size.Large },
                ChildText = "⌘ K",
            },
        },

        new("List", Category.DataDisplay,
            "A vertical daisyUI list of rows. ListRow offers a ready Icon/Title/Subtitle layout or free columns; ListItem keeps MudListItem's Icon, Text, Href and OnClick.",
            [typeof(List), typeof(ListRow), typeof(ListItem)],
            [
                new("Rows", "Icon, Title and Subtitle build the row; TrailingContent adds a column on the right, here a status badge.", typeof(ListRowsExample)),
                new("Custom columns", "Without the convenience parameters, ListRow lays out your own columns; list-col-grow takes the space.", typeof(ListCustomExample)),
                new("Items", "ListItem renders a link or a button with an icon, and can be Disabled.", typeof(ListItemsExample)),
            ])
        {
            DaisyClass = "list",
            MudEquivalent = "MudList",
            Portrait = typeof(ListCoverExample),
        },

        new("Stats", Category.DataDisplay,
            "A row (or column) of headline numbers using daisyUI stats. Each Stat has a Title, Value and Desc, plus optional Figure and Actions slots.",
            [typeof(Stats), typeof(Stat)],
            [
                new("Basic", "Figure holds an icon; DescColor tints the description for trends.", typeof(StatsBasicExample)),
                new("Vertical", "Vertical stacks the stats; Shadow lifts the container.", typeof(StatsVerticalExample)),
                new("Actions", "The Actions slot sits under the description.", typeof(StatsActionsExample)),
            ])
        {
            DaisyClass = "stats",
            Portrait = typeof(StatsCoverExample),
        },

        new("Status", Category.DataDisplay,
            "A tiny status dot for online, busy or offline states, in five sizes. Animated adds a ping ring; set AriaLabel when the dot stands alone.",
            [typeof(Status)],
            [
                new("Colors", "Every semantic colour.", typeof(StatusColorsExample)),
                new("Sizes", "From ExtraSmall to ExtraLarge.", typeof(StatusSizesExample)),
                new("With labels", "Pair the dot with text, and animate the one that matters.", typeof(StatusLabelsExample)),
            ])
        {
            DaisyClass = "status",
            Portrait = typeof(StatusCoverExample),
            Playground = new(typeof(Status), "Color", "Size", "Animated")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Success, ["Size"] = StatusSize.ExtraLarge, ["Animated"] = true },
            },
        },

        new("Table", Category.DataDisplay,
            "A typed data table: give it Items, a HeaderContent of Th cells and a RowTemplate of Td cells. Adds striping, dense rows, row clicks and client-side paging with TablePager; replaces MudTable.",
            [typeof(Table<>), typeof(Th), typeof(Td), typeof(TablePager)],
            [
                new("Basic", "Items, HeaderContent and RowTemplate; context is the row's item.", typeof(TableBasicExample)),
                new("Striped, dense and hover", "Three flags for the common looks; cells can hold any component.", typeof(TableStylesExample)),
                new("Paging", "RowsPerPage pages the rows; a TablePager in PagerContent changes page and page size.", typeof(TablePagingExample)),
                new("Row click", "OnRowClick passes the item and its index.", typeof(TableRowClickExample)),
            ])
        {
            DaisyClass = "table",
            MudEquivalent = "MudTable",
            Portrait = typeof(TableCoverExample),
        },

        new("SimpleTable", Category.DataDisplay,
            "A daisyUI table around markup you write yourself. Use it for static or hand-built tables; reach for Table when rows come from a collection.",
            [typeof(SimpleTable)],
            [
                new("Basic", "Plain thead and tbody inside, styled by daisyUI.", typeof(SimpleTableBasicExample)),
                new("Styles", "Striped, Hover, Dense and Bordered.", typeof(SimpleTableStylesExample)),
                new("Fixed header", "FixedHeader caps the height and scrolls; table-pin-rows keeps the header visible.", typeof(SimpleTableFixedHeaderExample)),
            ])
        {
            Slug = "simpletable",
            DaisyClass = "table",
            MudEquivalent = "MudSimpleTable",
            Portrait = typeof(SimpleTableCoverExample),
        },

        new("Timeline", Category.DataDisplay,
            "A vertical daisyUI timeline of events. Each TimelineItem has a coloured dot or an Icon, a boxed main content and an optional ItemOpposite.",
            [typeof(Timeline), typeof(TimelineItem)],
            [
                new("Basic", "Color and Size style the dot; ChildContent goes in the box.", typeof(TimelineBasicExample)),
                new("Icons", "Icon replaces the dot.", typeof(TimelineIconsExample)),
                new("Opposite content", "ItemOpposite becomes a label above each box, since the timeline always renders compact.", typeof(TimelineOppositeExample)),
            ])
        {
            DaisyClass = "timeline",
            Portrait = typeof(TimelineCoverExample),
        },

        new("Typography", Category.DataDisplay,
            "Text with a typed scale: Typo picks the element and size (h1–h6 render headings, the rest a paragraph) and Color the theme colour. Replaces MudText.",
            [typeof(Typography)],
            [
                new("Headings", "h1 to h6 render the matching heading element.", typeof(TypographyHeadingsExample)),
                new("Body and small text", "Subtitles, body, button, caption and overline.", typeof(TypographyTextExample)),
                new("Colors", "Color uses daisyUI text colours.", typeof(TypographyColorsExample)),
            ])
        {
            MudEquivalent = "MudText",
            MigrationNote = "MudText becomes Typography; Typo and Color keep their values.",
            Portrait = typeof(TypographyCoverExample),
            Playground = new(typeof(Typography), "Typo", "Color")
            {
                Initial = new Dictionary<string, object?> { ["Typo"] = Typo.h3 },
                ChildText = "Monthly revenue report",
            },
        },

        new("Image", Category.DataDisplay,
            "A plain img with typed helpers: Width and Height in pixels, ObjectFit for cropping, and Fluid to scale with its container.",
            [typeof(Image)],
            [
                new("Object fit", "In a fixed box, ObjectFit chooses cover, contain or fill.", typeof(ImageFitExample)),
                new("Fluid", "Fluid scales down to the container and keeps the ratio.", typeof(ImageFluidExample)),
                new("Shapes", "Class takes rounded corners or daisyUI masks.", typeof(ImageMaskExample)),
            ])
        {
            Portrait = typeof(ImageCoverExample),
            Playground = new(typeof(Image), "Src", "Alt", "Width", "Height", "ObjectFit", "Fluid")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Src"] = "https://img.daisyui.com/images/stock/photo-1572635148818-ef6fd45eb394.webp",
                    ["Alt"] = "A lemon on a yellow background",
                    ["Width"] = 260,
                    ["Height"] = 180,
                    ["ObjectFit"] = "cover",
                },
            },
        },

        new("Indicator", Category.DataDisplay,
            "Pins an Item (badge, status dot, anything) to a corner, edge or centre of its content using daisyUI's indicator. Horizontal and Vertical choose among nine positions.",
            [typeof(Indicator)],
            [
                new("Positions", "Every combination of Vertical and Horizontal.", typeof(IndicatorPositionsExample)),
                new("Content", "Badges on buttons and cards, a status dot on an avatar.", typeof(IndicatorContentExample)),
            ])
        {
            DaisyClass = "indicator",
            Portrait = typeof(IndicatorCoverExample),
        },
    ];
}
