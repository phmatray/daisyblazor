using DaisyBlazor.Docs.Examples.Mockup;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Mockup() =>
    [
        new("MockupBrowser", Category.Mockup,
            "A daisyUI browser frame with an address bar, for showing a page or screenshot in context. Set Url for the bar or replace it with your own Toolbar.",
            [typeof(MockupBrowser)],
            [
                new("Basic", "Url fills the address bar; child content sits in the window.", typeof(MockupBrowserBasicExample)),
                new("Border colour", "BorderColor tints the frame with a semantic colour.", typeof(MockupBrowserBorderExample)),
                new("Custom toolbar", "Toolbar takes any markup, such as a search field.", typeof(MockupBrowserToolbarExample)),
            ])
        {
            DaisyClass = "mockup-browser",
            Portrait = typeof(MockupBrowserCoverExample),
            Playground = new(typeof(MockupBrowser), "Url", "BorderColor")
            {
                Initial = new Dictionary<string, object?> { ["Url"] = "https://disques-soleil.fr", ["BorderColor"] = Color.Primary, ["Class"] = "w-full max-w-md border-2" },
                ChildText = "Twelve thousand records, sorted by hand.",
            },
        },

        new("MockupCode", Category.Mockup,
            "A daisyUI terminal frame with a prefix on every line. Pass Lines for quick shell snippets, or write your own pre rows to vary prefixes and highlight lines.",
            [typeof(MockupCode)],
            [
                new("Lines", "Each string in Lines becomes a row prefixed with $.", typeof(MockupCodeLinesExample)),
                new("Prefix", "Prefix changes the marker; Class can recolour the frame.", typeof(MockupCodePrefixExample)),
                new("Custom rows", "Child content gives each pre its own data-prefix and classes, for numbered or highlighted lines.", typeof(MockupCodeCustomExample)),
            ])
        {
            DaisyClass = "mockup-code",
            Portrait = typeof(MockupCodeCoverExample),
        },

        new("MockupPhone", Category.Mockup,
            "A daisyUI phone frame with a camera notch around any screen content. It fills its container's width up to 462px, so size it with a width utility.",
            [typeof(MockupPhone)],
            [
                new("Basic", "A splash screen inside a phone sized with w-56.", typeof(MockupPhoneBasicExample)),
                new("App screens", "Phones of different widths keep the same proportions.", typeof(MockupPhoneScreenExample)),
            ])
        {
            DaisyClass = "mockup-phone",
            Portrait = typeof(MockupPhoneCoverExample),
        },

        new("MockupWindow", Category.Mockup,
            "A daisyUI desktop window frame with an optional title bar, for presenting dialogs and app screens. BorderColor tints the frame.",
            [typeof(MockupWindow)],
            [
                new("Basic", "Title fills the title bar; child content is centred in the window.", typeof(MockupWindowBasicExample)),
                new("Border colour", "BorderColor with a thicker border makes an error window.", typeof(MockupWindowBorderExample)),
            ])
        {
            DaisyClass = "mockup-window",
            Portrait = typeof(MockupWindowCoverExample),
            Playground = new(typeof(MockupWindow), "Title", "BorderColor")
            {
                Initial = new Dictionary<string, object?> { ["Title"] = "Jukebox", ["BorderColor"] = Color.Secondary, ["Class"] = "w-full max-w-md border-2" },
                ChildText = "Insert a coin to play a song.",
            },
        },
    ];
}
