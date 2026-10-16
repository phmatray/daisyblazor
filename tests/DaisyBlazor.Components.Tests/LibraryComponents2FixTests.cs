namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-2 fix batch.</summary>
public class LibraryComponents2FixTests : BunitContext
{
    [Fact]
    public void ProgressLinear_shifts_the_range_by_Min()
    {
        IRenderedComponent<ProgressLinear> cut = Render<ProgressLinear>(ps => ps
            .Add(p => p.Min, 20)
            .Add(p => p.Value, 50)
            .Add(p => p.Max, 100));

        AngleSharp.Dom.IElement bar = cut.Find("progress");
        bar.GetAttribute("value").ShouldBe("30");
        bar.GetAttribute("max").ShouldBe("80");
    }

    [Fact]
    public void ProgressLinear_has_aria_label_neutral_color_and_thin_small_size()
    {
        IRenderedComponent<ProgressLinear> cut = Render<ProgressLinear>(ps => ps
            .Add(p => p.AriaLabel, "Upload")
            .Add(p => p.Color, Color.Dark)
            .Add(p => p.Size, Size.Small));

        AngleSharp.Dom.IElement bar = cut.Find("progress");
        bar.GetAttribute("aria-label").ShouldBe("Upload");
        bar.ClassList.ShouldContain("progress-neutral");
        bar.ClassList.ShouldContain("h-1");
    }

    [Fact]
    public void Field_value_is_text_sm_and_named_by_its_caption_not_an_orphan_label()
    {
        IRenderedComponent<Field> cut = Render<Field>(ps => ps
            .Add(p => p.Label, "Invoice")
            .AddChildContent("INV-2041"));

        cut.FindAll("label").Count.ShouldBe(0);
        string id = cut.Find(".label").GetAttribute("id")!;
        AngleSharp.Dom.IElement body = cut.Find("[role=group]");
        body.GetAttribute("aria-labelledby").ShouldBe(id);
        body.ClassList.ShouldContain("text-sm");
    }

    [Fact]
    public void Field_renders_AdornmentText()
    {
        IRenderedComponent<Field> cut = Render<Field>(ps => ps
            .Add(p => p.Adornment, Adornment.End)
            .Add(p => p.AdornmentText, "EUR")
            .AddChildContent("120"));

        cut.Markup.ShouldContain("EUR");
    }

    [Fact]
    public void RadioGroup_horizontal_drops_the_column_layout()
    {
        IRenderedComponent<RadioGroup<int>> cut = Render<RadioGroup<int>>(ps => ps.Add(p => p.Horizontal, true));

        AngleSharp.Dom.IElement root = cut.Find("[role=radiogroup]");
        root.ClassList.ShouldContain("flex-row");
        root.ClassList.ShouldNotContain("flex-col");
    }

    [Fact]
    public void RadioGroup_Label_names_the_radiogroup()
    {
        IRenderedComponent<RadioGroup<int>> cut = Render<RadioGroup<int>>(ps => ps.Add(p => p.Label, "Billing cycle"));

        string labelledBy = cut.Find("[role=radiogroup]").GetAttribute("aria-labelledby")!;
        cut.Find($"#{labelledBy}").TextContent.ShouldBe("Billing cycle");
    }

    [Fact]
    public void TablePager_controls_have_accessible_names()
    {
        IRenderedComponent<TablePager> cut = Render<TablePager>(ps => ps
            .AddCascadingValue<ITablePagerHost>(new FakeHost()));

        cut.Find("select").GetAttribute("aria-label").ShouldBe("Rows per page");
        IReadOnlyList<AngleSharp.Dom.IElement> buttons = cut.FindAll("button");
        buttons[0].GetAttribute("aria-label").ShouldBe("Previous page");
        buttons[1].GetAttribute("aria-label").ShouldBe("Next page");
    }

    [Fact]
    public void Timeline_alternates_sides_and_puts_opposite_across_the_line()
    {
        IRenderedComponent<Timeline> cut = Render<Timeline>(ps => ps
            .AddChildContent<TimelineItem>(ip => ip
                .Add(p => p.ItemOpposite, "v1")
                .AddChildContent("one"))
            .AddChildContent<TimelineItem>(ip => ip.AddChildContent("two")));

        cut.Find("ul").ClassList.ShouldNotContain("timeline-compact");
        IReadOnlyList<AngleSharp.Dom.IElement> boxes = cut.FindAll(".timeline-box");
        boxes[0].ClassList.ShouldContain("timeline-end");
        boxes[1].ClassList.ShouldContain("timeline-start");
        cut.Find("li .timeline-start:not(.timeline-box)").TextContent.ShouldBe("v1");
    }

    [Fact]
    public void Timeline_reverse_start_and_align_are_honoured()
    {
        IRenderedComponent<Timeline> reversed = Render<Timeline>(ps => ps
            .Add(p => p.Reverse, true)
            .AddChildContent<TimelineItem>(ip => ip
                .Add(p => p.TimelineAlign, TimelineAlign.Start)
                .AddChildContent("one")));

        AngleSharp.Dom.IElement box = reversed.Find(".timeline-box");
        box.ClassList.ShouldContain("timeline-start");
        box.ClassList.ShouldContain("self-start");

        IRenderedComponent<Timeline> compact = Render<Timeline>(ps => ps
            .Add(p => p.TimelinePosition, TimelinePosition.Start)
            .AddChildContent<TimelineItem>(ip => ip.AddChildContent("one")));

        compact.Find("ul").ClassList.ShouldContain("timeline-compact");
    }

    [Fact]
    public void TimelineItem_icon_follows_Size()
    {
        IRenderedComponent<TimelineItem> cut = Render<TimelineItem>(ps => ps
            .Add(p => p.Icon, "home")
            .Add(p => p.Size, Size.Large));

        cut.FindComponent<Icon>().Instance.Size.ShouldBe(Size.Large);
    }

    [Fact]
    public void MockupWindow_title_sits_on_the_dots_row()
    {
        IRenderedComponent<MockupWindow> cut = Render<MockupWindow>(ps => ps.Add(p => p.Title, "Save changes"));

        AngleSharp.Dom.IElement title = cut.Find(".mockup-window > div");
        title.ClassList.ShouldContain("absolute");
        title.ClassList.ShouldNotContain("border-b");
        title.TextContent.Trim().ShouldBe("Save changes");
    }

    private sealed class FakeHost : ITablePagerHost
    {
        public int TotalItems => 30;
        public int CurrentPage => 1;
        public int PageCount => 3;
        public int PageSize => 10;
        public void SetPage(int page) { }
        public void SetPageSize(int size) { }
    }
}
