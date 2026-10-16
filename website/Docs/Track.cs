namespace DaisyBlazor.Docs.Docs;

/// <summary>A page section, listed as a jump link on the masthead and in the sticky contents.</summary>
public sealed record Track(string Id, string Title)
{
    public static Track Of(string title) => new(Slugify.Of(title), title);
}
