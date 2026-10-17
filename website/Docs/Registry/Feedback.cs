using DaisyBlazor.Docs.Examples.Feedback;
using DaisyBlazor.Feedback;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Feedback() =>
    [
        new("Alert", Category.Feedback,
            "A daisyUI alert banner coloured by Severity, with a matching icon added for you. Use it for messages that stay on the page; reach for Snackbar when the message should come and go.",
            [typeof(Alert)],
            [
                new("Severities", "Normal, Info, Success, Warning and Error map to the theme's alert colours and pick a fitting icon.", typeof(AlertSeveritiesExample)),
                new("Variants", "Filled is the default; Text gives daisyUI's soft tint and Outlined keeps only the border.", typeof(AlertVariantsExample)),
                new("Icons and density", "Icon overrides the severity icon, NoIcon removes it, and Dense tightens the padding.", typeof(AlertIconsExample)),
            ])
        {
            DaisyClass = "alert",
            MudEquivalent = "MudAlert",
            Portrait = typeof(AlertCoverExample),
            Playground = new(typeof(Alert), "Severity", "Variant", "Dense", "NoIcon", "Icon")
            {
                Initial = new Dictionary<string, object?> { ["Severity"] = Severity.Success },
                ChildText = "Your invoice is on its way.",
            },
        },

        new("Loading", Category.Feedback,
            "A small animated indicator in six styles: spinner, dots, ring, ball, bars and infinity. Use it inline or inside a card while content loads.",
            [typeof(Loading)],
            [
                new("Types", "Type picks the animation; Color tints it with any theme colour.", typeof(LoadingTypesExample)),
                new("Sizes", "Size covers small to large; SizeOverride takes loading-xs or loading-xl for the extremes.", typeof(LoadingSizesExample)),
                new("In context", "Next to a line of text, or centred in the space the content will fill.", typeof(LoadingInlineExample)),
            ])
        {
            DaisyClass = "loading",
            Portrait = typeof(LoadingCoverExample),
            Playground = new(typeof(Loading), "Type", "Size", "Color", "SizeOverride")
            {
                Initial = new Dictionary<string, object?> { ["Type"] = LoadingType.Bars, ["Size"] = Size.Large, ["Color"] = Color.Primary },
            },
        },

        new("ProgressLinear", Category.Feedback,
            "A horizontal daisyUI progress bar. Set Value against Max to show how far a task has got, or Indeterminate when you can't tell.",
            [typeof(ProgressLinear)],
            [
                new("Colors", "Color maps to the progress-* classes, so the bar follows the active theme.", typeof(ProgressLinearColorsExample)),
                new("Sizes and max", "Size sets the height and Rounded softens the ends; Max changes the scale (here 40 of 60).", typeof(ProgressLinearSizesExample)),
                new("Indeterminate", "With no known total, the bar animates on its own.", typeof(ProgressLinearIndeterminateExample)),
                new("Live progress", "Bind Value to your own state and the bar moves as the work runs.", typeof(ProgressLinearLiveExample)),
            ])
        {
            DaisyClass = "progress",
            MudEquivalent = "MudProgressLinear",
            MigrationNote = "Striped is accepted but has no effect: daisyUI progress bars have no striped style.",
            Portrait = typeof(ProgressLinearCoverExample),
            Playground = new(typeof(ProgressLinear), "Value", "Max", "Color", "Size", "Indeterminate", "Rounded")
            {
                Initial = new Dictionary<string, object?> { ["Value"] = 62d, ["Color"] = Color.Primary, ["Size"] = Size.Large, ["Rounded"] = true, ["Class"] = "w-72" },
            },
        },

        new("ProgressCircular", Category.Feedback,
            "A circular spinner for work of unknown length, sized and coloured with the usual enums. It is always indeterminate; use RadialProgress to show a percentage.",
            [typeof(ProgressCircular)],
            [
                new("Colors", "Color tints the spinner with any theme colour.", typeof(ProgressCircularColorsExample)),
                new("Sizes", "Small, Medium and Large; AriaLabel names it for screen readers (defaults to \"loading\").", typeof(ProgressCircularSizesExample)),
            ])
        {
            DaisyClass = "loading",
            MudEquivalent = "MudProgressCircular",
            MigrationNote = "Value, Min and Max are accepted so existing markup compiles, but the spinner always renders indeterminate. Switch to RadialProgress for a determinate ring.",
            Portrait = typeof(ProgressCircularCoverExample),
            Playground = new(typeof(ProgressCircular), "Color", "Size")
            {
                Initial = new Dictionary<string, object?> { ["Color"] = Color.Primary, ["Size"] = Size.Large },
            },
        },

        new("RadialProgress", Category.Feedback,
            "A daisyUI ring that fills to Value percent, with the percentage in the middle unless you supply your own content. Good for scores, quotas and timers.",
            [typeof(RadialProgress)],
            [
                new("Values and colors", "Value runs from 0 to 100; Color tints the ring and its label.", typeof(RadialProgressValuesExample)),
                new("Size and thickness", "SizeRem and Thickness take any CSS length.", typeof(RadialProgressSizeExample)),
                new("Custom content", "ChildContent replaces the percentage, here with elapsed time on a 3:34 build.", typeof(RadialProgressContentExample)),
            ])
        {
            DaisyClass = "radial-progress",
            Portrait = typeof(RadialProgressCoverExample),
            Playground = new(typeof(RadialProgress), "Value", "Color", "SizeRem", "Thickness")
            {
                Initial = new Dictionary<string, object?> { ["Value"] = 72d, ["Color"] = Color.Primary, ["SizeRem"] = "8rem", ["Thickness"] = "10px" },
            },
        },

        new("Skeleton", Category.Feedback,
            "A pulsing placeholder block shaped like the content that is about to appear. Width and Height take any CSS length; SkeletonType.Circle rounds it.",
            [typeof(Skeleton)],
            [
                new("Shapes", "Text, Circle and Rectangle, sized with Width and Height.", typeof(SkeletonShapesExample)),
                new("Card placeholders", "Compose skeletons into the layout of the real cards so nothing jumps when they load.", typeof(SkeletonCardExample)),
                new("Swap in the content", "Show skeletons while loading, then render the real thing in their place.", typeof(SkeletonLoadingExample)),
            ])
        {
            DaisyClass = "skeleton",
            MudEquivalent = "MudSkeleton",
            MigrationNote = "Animation is accepted but ignored: daisyUI handles the pulse.",
            Portrait = typeof(SkeletonCoverExample),
            Playground = new(typeof(Skeleton), "SkeletonType", "Width", "Height")
            {
                Initial = new Dictionary<string, object?> { ["SkeletonType"] = SkeletonType.Rectangle, ["Width"] = "16rem", ["Height"] = "6rem" },
            },
        },

        new("Tooltip", Category.Feedback,
            "Wraps any element in a daisyUI tooltip that shows Text on hover or focus. Placement picks the side; extra classes such as tooltip-primary or tooltip-open go in Class.",
            [typeof(Tooltip)],
            [
                new("Placements", "Top (default), Bottom, Left and Right. Hover each button.", typeof(TooltipPlacementsExample)),
                new("Wrap anything", "Icon buttons, avatars and badges all work as the trigger.", typeof(TooltipContentExample)),
                new("Colors and always open", "Add tooltip-{color} for a theme colour and tooltip-open to keep it visible.", typeof(TooltipColorsExample)),
            ])
        {
            DaisyClass = "tooltip",
            MudEquivalent = "MudTooltip",
            MigrationNote = "Arrow is accepted but has no effect: daisyUI tooltips always have an arrow.",
            Portrait = typeof(TooltipCoverExample),
            Playground = new(typeof(Tooltip), "Text", "Placement")
            {
                Initial = new Dictionary<string, object?> { ["Text"] = "Last deployed 2 hours ago", ["Class"] = "tooltip-open" },
                ChildText = "Hover me",
            },
        },

        new("Dialog", Category.Feedback,
            "Modal dialogs opened from code through IDialogService and hosted by DialogProvider. ShowMessageBoxAsync covers quick questions; ShowAsync opens your own component built on Dialog and returns its result.",
            [typeof(Dialog), typeof(MessageBoxDialog), typeof(DialogProvider)],
            [
                new("Message box", "ShowMessageBoxAsync returns true for yes, false for no and null when cancelled or dismissed.", typeof(DialogMessageBoxExample)),
                new("A dialog component", "Lay out the content with Dialog's DialogContent and DialogActions, then close through the cascaded IMudDialogInstance.", typeof(DialogTaskEditorExample)),
                new("Open it with ShowAsync", "Pass parameters with DialogParameters<T> and await the result.", typeof(DialogShowAsyncExample)),
                new("Options", "DialogOptions sets the width, the close button and whether the backdrop or Escape dismisses it.", typeof(DialogOptionsExample)),
            ])
        {
            DaisyClass = "modal",
            MudEquivalent = "MudDialog",
            MigrationNote = "Mount <DialogProvider /> once in your layout, exactly where MudDialogProvider was. IDialogService, DialogParameters, DialogOptions and DialogResult keep their MudBlazor shapes.",
            Portrait = typeof(DialogCoverExample),
        },

        new("Modal", Category.Feedback,
            "A modal declared inline in your markup and shown with @bind-Open, no service needed. It renders the same accessible box as DialogProvider: focus moves in, the page behind is inert, Escape closes it and focus returns to the button that opened it.",
            [typeof(Modal)],
            [
                new("Bind Open", "@bind-Open shows the modal; Escape, the backdrop or your own button sets it back to false.", typeof(ModalBindOpenExample)),
                new("Confirmation", "AlertDialog sets role=\"alertdialog\". With BackdropClick and CloseOnEscapeKey off, only the buttons close it.", typeof(ModalConfirmExample)),
                new("Close button and OnClose", "CloseButton adds the corner (x), TitleContent takes custom title markup, Class widens the box and OnClose runs each time the modal dismisses itself.", typeof(ModalCloseButtonExample)),
            ])
        {
            DaisyClass = "modal",
            MudEquivalent = "MudDialog (inline)",
            MigrationNote = "An inline <MudDialog @bind-Visible> becomes <Modal @bind-Open> and keeps TitleContent, DialogContent and DialogActions. CloseButton, CloseOnEscapeKey and BackdropClick are plain parameters instead of DialogOptions.",
            Portrait = typeof(ModalCoverExample),
        },

        new("Snackbar", Category.Feedback,
            "Toast messages raised from code with ISnackbar.Add and shown bottom-right by SnackbarProvider. Each one auto-dismisses after a delay, pausing while it is hovered or focused.",
            [typeof(SnackbarProvider)],
            [
                new("Severities", "The Severity argument picks the alert colour and icon.", typeof(SnackbarSeveritiesExample)),
                new("Options", "The configure callback sets SnackbarOptions: VisibleStateDuration (ms, default 5000), ShowCloseIcon (default true) and PreventDuplicates (default true). ISnackbar.Remove(message) drops one toast, Clear removes them all and Shown lists the visible ones.", typeof(SnackbarOptionsExample)),
            ])
        {
            DaisyClass = "toast",
            MudEquivalent = "MudSnackbarProvider",
            MigrationNote = "Mount <SnackbarProvider /> once in your layout. Snackbar.Add(message, severity, configure) keeps MudBlazor's signature.",
            Portrait = typeof(SnackbarCoverExample),
        },

        new("ConfirmDialog", Category.Feedback,
            "A ready-made confirm or cancel dialog, opened with IDialogService.ShowAsync. Set the body text, the confirm label and its colour; the result is cancelled unless the user confirms.",
            [typeof(ConfirmDialog)],
            [
                new("Destructive action", "Color.Error marks a confirm button that can't be undone.", typeof(ConfirmDialogDestructiveExample)),
                new("Your own labels", "ButtonText and CancelText take localized strings.", typeof(ConfirmDialogLabelsExample)),
            ])
        {
            DaisyClass = "modal",
            Portrait = typeof(ConfirmDialogCoverExample),
        },

        new("EmptyState", Category.Feedback,
            "A centred icon, message and optional subtitle for a list, search or page with nothing to show. Put it where the content would be instead of leaving a blank space.",
            [typeof(EmptyState)],
            [
                new("Icon and subtitle", "Icon sits above the message; Subtitle suggests what to do next.", typeof(EmptyStateBasicExample)),
                new("Message only", "Message is the only required parameter.", typeof(EmptyStateMessageOnlyExample)),
                new("In a list", "Render it when the collection is empty and the list otherwise.", typeof(EmptyStateListExample)),
            ])
        {
            Portrait = typeof(EmptyStateCoverExample),
            Playground = new(typeof(EmptyState), "Message", "Icon", "Subtitle")
            {
                Initial = new Dictionary<string, object?>
                {
                    ["Message"] = "No invoices yet",
                    ["Icon"] = Icons.Material.Filled.ReceiptLong,
                    ["Subtitle"] = "Invoices will appear here once you subscribe.",
                },
            },
        },

        new("ErrorAlert", Category.Feedback,
            "A filled error Alert that renders nothing while Message is null, so it can sit unguarded above a form and appear only when something fails.",
            [typeof(ErrorAlert)],
            [
                new("Basic", "Pass the error text as Message. Class defaults to mb-4.", typeof(ErrorAlertBasicExample)),
                new("Null renders nothing", "Set Message to null and the alert disappears without an @if.", typeof(ErrorAlertToggleExample)),
                new("After a failed save", "Catch the exception and show its message.", typeof(ErrorAlertSaveExample)),
            ])
        {
            DaisyClass = "alert alert-error",
            Portrait = typeof(ErrorAlertCoverExample),
            Playground = new(typeof(ErrorAlert), "Message")
            {
                Initial = new Dictionary<string, object?> { ["Message"] = "The payment provider did not respond. Try again in a minute." },
            },
        },
    ];
}
