using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Feedback;

/// <summary>
/// Page-level "access denied" state: a centred lock icon, a title, an explanation, and a way back.
/// </summary>
/// <remarks>
/// <para>
/// Conceptually a sibling of <see cref="EmptyState"/> — both render a page that has nothing to show,
/// for different reasons — which is why it lives here rather than with the <c>Feature*</c> family.
/// It is not tied to a "feature": any gated page can use it.
/// </para>
/// <para>
/// Every string is a parameter and there is no localizer dependency, matching how
/// <see cref="EmptyState"/> and <see cref="ErrorAlert"/> work: the library owns the shape, the
/// application owns the words. Pass localized values in.
/// </para>
/// <para>
/// <see cref="FeatureHomePage.AccessDeniedContent"/> already existed as a slot with nothing to put
/// in it; this is the component that fills it.
/// </para>
/// <example>
/// <code>
/// &lt;AccessDenied Title="@L["AccessDenied_Title"]"
///               Message="@L["AccessDenied_Message"]"
///               ActionText="@L["AccessDenied_BackToHome"]"
///               ActionHref="/" /&gt;
/// </code>
/// </example>
/// </remarks>
public partial class AccessDenied
{
    /// <summary>Headline. Defaults to a plain English string — pass a localized one in a localized app.</summary>
    [Parameter]
    public string Title { get; set; } = "Access denied";

    /// <summary>Optional explanation beneath the title.</summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>
    /// Material Symbols ligature for the icon. Defaults to a lock; set to <c>null</c> or empty to
    /// drop the icon entirely.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; } = Icons.Material.Filled.Lock;

    /// <summary>Colour of the icon. Defaults to <see cref="Color.Error"/>.</summary>
    [Parameter]
    public Color IconColor { get; set; } = Color.Error;

    /// <summary>
    /// Label for the escape-hatch button. The button is only rendered when this is set — a page that
    /// genuinely offers no way out simply omits it.
    /// </summary>
    [Parameter]
    public string? ActionText { get; set; }

    /// <summary>Where the action button navigates. Ignored when <see cref="OnAction"/> is used instead.</summary>
    [Parameter]
    public string? ActionHref { get; set; } = "/";

    /// <summary>Click handler for the action button, as an alternative to <see cref="ActionHref"/>.</summary>
    [Parameter]
    public EventCallback OnAction { get; set; }

    /// <summary>
    /// Optional extra content between the message and the action — for example the roles the page
    /// requires, or who to ask for them.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Width constraint of the centred column. Defaults to <see cref="DaisyBlazor.MaxWidth.Small"/>.</summary>
    [Parameter]
    public MaxWidth MaxWidth { get; set; } = MaxWidth.Small;

    /// <summary>
    /// Minimum height of the block, so it occupies the page rather than hugging the top.
    /// Any CSS length.
    /// </summary>
    [Parameter]
    public string MinHeight { get; set; } = "400px";

    /// <summary>Extra CSS classes appended to the container.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Extra inline style appended after the <c>min-height</c> declaration.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Unmatched attributes splatted onto the container.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
