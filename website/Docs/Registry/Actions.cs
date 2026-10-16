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
            Portrait = typeof(ButtonCoverExample),
            Playground = new(typeof(Button), "Color", "Variant", "Size", "StartIcon", "EndIcon", "Disabled", "Loading", "FullWidth")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Variant"] = Variant.Filled },
                ChildText = "Play the record",
            },
        },
    ];
}
