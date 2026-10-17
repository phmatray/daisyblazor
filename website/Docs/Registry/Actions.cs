using DaisyBlazor.Docs.Examples.Actions;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Actions() =>
    [
        new("Button", Category.Actions,
            "A daisyUI btn with typed Color, Variant and Size. Renders a link when Href is set, shows a spinner while Loading, and drops in for MudButton.",
            [typeof(Button)],
            [
                new("Colors", "Semantic Color values map to daisyUI's btn-* colours, so they follow every theme.", typeof(ButtonColorsExample)),
                new("Variants", "Filled, Outlined and Text map to btn, btn-outline and btn-ghost.", typeof(ButtonVariantsExample)),
                new("Sizes", "Small, Medium (default) and Large.", typeof(ButtonSizesExample)),
                new("Icons", "StartIcon and EndIcon take any Icons.Material ligature.", typeof(ButtonIconsExample)),
                new("Loading and disabled", "Loading swaps in a spinner and blocks clicks; bind it to your own async work.", typeof(ButtonStatesExample)),
            ])
        {
            DaisyClass = "btn",
            MudEquivalent = "MudButton",
            MigrationNote = "Since daisyUI 5.7 (the version the preset bundles), any .btn with aria-disabled=\"true\" gets pointer-events: none, so it no longer receives clicks or hover. That is what Disabled wants, but a button you deliberately keep clickable while marked aria-disabled (to explain why the action is unavailable) goes dead. Put the explanation on a wrapper instead (a Tooltip around the disabled Button still shows on hover), or opt back in with your own rule after the preset import, e.g. .btn.btn-explain[aria-disabled=\"true\"] { pointer-events: auto; }.",
            Portrait = typeof(ButtonCoverExample),
            Playground = new(typeof(Button), "Color", "Variant", "Size", "StartIcon", "EndIcon", "Disabled", "Loading", "FullWidth")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Variant"] = Variant.Filled },
                ChildText = "Save changes",
            },
        },

        new("IconButton", Category.Actions,
            "A round ghost btn holding a single icon, for toolbars and compact controls. Color tints the icon, Href turns it into a link; always give it an aria-label.",
            [typeof(IconButton)],
            [
                new("Basic", "Pass any Icons.Material ligature; Title adds a native tooltip and aria-label names it for screen readers.", typeof(IconButtonBasicExample)),
                new("Colors", "Color tints the glyph with the theme's semantic colours while the button stays ghost.", typeof(IconButtonColorsExample)),
                new("Sizes", "Small, Medium (default) and Large map to btn-sm and btn-lg.", typeof(IconButtonSizesExample)),
                new("Toggle, disabled and link", "Drive Color from state for a toggle, Disabled blocks clicks, and Href renders an anchor.", typeof(IconButtonStatesExample)),
            ])
        {
            DaisyClass = "btn-circle",
            MudEquivalent = "MudIconButton",
            Portrait = typeof(IconButtonCoverExample),
            Playground = new(typeof(IconButton), "Icon", "Color", "Size", "Disabled", "Title")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Icon"] = Icons.Material.Filled.Favorite,
                    ["Color"] = Color.Error,
                    ["Size"] = Size.Large,
                    ["Title"] = "Add to favourites",
                },
            },
        },

        new("ButtonGroup", Category.Actions,
            "A daisyUI join that fuses adjacent buttons into one segmented bar. Each child keeps its own Color, Variant and Size; add join-vertical to stack them.",
            [typeof(ButtonGroup)],
            [
                new("Basic", "Wrap Buttons in a ButtonGroup and their inner corners square off.", typeof(ButtonGroupBasicExample)),
                new("Pagination", "Highlight the current item by switching its Color and Variant from state.", typeof(ButtonGroupPagesExample)),
                new("Vertical", "Class=\"join-vertical\" stacks the group.", typeof(ButtonGroupVerticalExample)),
            ])
        {
            DaisyClass = "join",
            MudEquivalent = "MudButtonGroup",
            MigrationNote = "Variant, Color, Size and OverrideStyles on the group are accepted for source compatibility but ignored: set them on each child Button.",
            Portrait = typeof(ButtonGroupCoverExample),
        },

        new("Fab", Category.Actions,
            "A floating action button pinned to the bottom-right corner that opens a speed dial of FabAction buttons on focus, in a stack or a Flower arc. Pure CSS, no JavaScript.",
            [typeof(Fab), typeof(FabAction)],
            [
                new("Speed dial", "FabAction children appear above the trigger with optional tooltips and their own OnClick.", typeof(FabSpeedDialExample)),
                new("Flower", "Flower=\"true\" fans up to five actions out in a quarter circle.", typeof(FabFlowerExample)),
                new("Custom icon, no close", "Icon swaps the trigger glyph; ShowClose=\"false\" keeps the trigger instead of the close button while open.", typeof(FabCustomExample)),
            ])
        {
            DaisyClass = "fab",
            MudEquivalent = "MudFab",
            MigrationNote = "MudFab is a single button; Fab is daisyUI's fixed-position speed dial. For a lone round action inside the layout, use IconButton or a Button with Class=\"btn-circle\".",
            Portrait = typeof(FabCoverExample),
        },

        new("Dropdown", Category.Actions,
            "A CSS-only menu built on details/summary: the Activator opens a panel of menu items below it. Use Placement, Hover or Open to control it; for click-to-close menus with Blazor state, reach for Menu.",
            [typeof(Dropdown)],
            [
                new("Basic", "Activator is the trigger, ChildContent holds the li items of the menu.", typeof(DropdownBasicExample)),
                new("Placement", "Placement puts the panel on any side of the trigger, or aligns it to the End.", typeof(DropdownPlacementExample)),
                new("Hover", "Hover=\"true\" opens the panel on mouse-over, with no click.", typeof(DropdownHoverExample)),
                new("Open with a custom panel", "Open forces the panel open, and ContentClass styles it (here wider, with titles and icons).", typeof(DropdownOpenExample)),
            ])
        {
            DaisyClass = "dropdown",
            Portrait = typeof(DropdownCoverExample),
        },

        new("Swap", Category.Actions,
            "Toggles between two pieces of content, On and Off, with an optional rotate or flip animation. Bind Active for two-way state.",
            [typeof(Swap)],
            [
                new("Rotate", "Animation=\"SwapAnimation.Rotate\" spins between the icons; @bind-Active keeps your field in sync.", typeof(SwapRotateExample)),
                new("Flip", "SwapAnimation.Flip turns the content over like a card.", typeof(SwapFlipExample)),
                new("Text, no animation", "On and Off take any markup; without an animation the switch is instant.", typeof(SwapTextExample)),
            ])
        {
            DaisyClass = "swap",
            Portrait = typeof(SwapCoverExample),
        },

        new("ThemeController", Category.Actions,
            "A checkbox or radio input that switches the daisyUI theme of the whole page through CSS alone. Value names the theme; radios sharing a Name pick one of many.",
            [typeof(ThemeController)],
            [
                new("Toggle", "The checkbox variant applies Value while checked and falls back to the page theme when unchecked.", typeof(ThemeControllerToggleExample)),
                new("Radio group", "Type=\"ThemeControllerType.Radio\" with a shared Name lets the reader choose between several themes.", typeof(ThemeControllerRadioExample)),
            ])
        {
            DaisyClass = "theme-controller",
            Portrait = typeof(ThemeControllerCoverExample),
        },

        new("ToggleGroup", Category.Actions,
            "A segmented control: a join of ToggleItem buttons where one value is selected at a time. Generic over T and bound with @bind-Value.",
            [typeof(ToggleGroup<string>), typeof(ToggleItem<string>)],
            [
                new("Basic", "Each ToggleItem carries a Value; the group's Value is the selected one.", typeof(ToggleGroupBasicExample)),
                new("Icons", "Icon alone makes a compact icon toggle; Icon plus Text puts both side by side.", typeof(ToggleGroupIconsExample)),
                new("Sizes and colours", "Size and Color are set once on the group; Color paints the selected item.", typeof(ToggleGroupSizesExample)),
                new("Disabled", "Disable a single ToggleItem or the whole group.", typeof(ToggleGroupDisabledExample)),
            ])
        {
            DaisyClass = "join",
            Portrait = typeof(ToggleGroupCoverExample),
        },
    ];
}
