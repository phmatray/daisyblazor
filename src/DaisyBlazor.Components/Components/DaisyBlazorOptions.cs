namespace DaisyBlazor;

/// <summary>App-wide defaults of the DaisyBlazor components, set with <c>AddDaisyBlazor(o =&gt; …)</c>.</summary>
public sealed class DaisyBlazorOptions
{
    /// <summary>
    /// Variant of every <see cref="Button"/> that does not set one. Defaults to <see cref="Variant.Text"/>
    /// (a ghost button, as MudButton); <see cref="Variant.Filled"/> gives daisyUI's own solid <c>.btn</c>.
    /// </summary>
    public Variant DefaultButtonVariant { get; set; } = Variant.Text;
}
