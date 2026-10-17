using DaisyBlazor.Layout;

namespace DaisyBlazor.Components.Tests;

/// <summary>Tests for <see cref="PageHeader.SubtitleColor"/> (issue #70).</summary>
public class PageHeaderSubtitleColorTests : BunitContext
{
    [Fact]
    public void Subtitle_is_muted_by_default()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Orders")
            .Add(p => p.Subtitle, "Last 30 days"));

        cut.Find("p").ClassList.ShouldContain("text-base-content/70");
    }

    [Fact]
    public void SubtitleColor_replaces_the_muted_class()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Orders")
            .Add(p => p.Subtitle, "3 payments failed")
            .Add(p => p.SubtitleColor, Color.Error));

        cut.Find("p").ClassList.ShouldContain("text-error");
        cut.Find("p").ClassList.ShouldNotContain("text-base-content/70");
    }

    [Fact]
    public void Inherit_adds_no_colour_class()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Orders")
            .Add(p => p.Subtitle, "Last 30 days")
            .Add(p => p.SubtitleColor, Color.Inherit));

        cut.Find("p").ClassList.ShouldNotContain(c => c.StartsWith("text-base-content"));
    }
}
