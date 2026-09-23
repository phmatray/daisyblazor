using AngleSharp.Dom;
using DaisyBlazor;
using DaisyBlazor.Data;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Guards the ergonomics fix from #9: components whose type parameter was never used by the
/// component itself are non-generic, so a display-only call site needs no <c>T</c> annotation.
/// Every render below deliberately omits a type argument — if a generic sneaks back in, these
/// stop compiling, which is the point.
/// </summary>
/// <remarks>
/// The parallel half of #13 — <c>[CascadingTypeParameter]</c> on <c>Select</c>/<c>RadioGroup</c> —
/// cannot be guarded from here: C# has no way to *omit* a type argument, so only Razor markup can
/// witness the inference. The gallery call sites are that witness, and CI compiles them. What the
/// tests below guard instead is the behaviour the cascade could plausibly disturb — child
/// registration order and selection.
/// </remarks>
public class GenericErgonomicsTests : BunitContext
{
    [Fact]
    public void Switch_renders_without_an_explicit_type_argument()
    {
        IRenderedComponent<Switch> cut = Render<Switch>(ps => ps
            .Add(p => p.Label, "Send me updates")
            .Add(p => p.Value, true));

        cut.Markup.ShouldContain("toggle");
        cut.Markup.ShouldContain("Send me updates");
        cut.Find("input").HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Switch_reports_the_toggled_value_as_a_bool()
    {
        bool? observed = null;

        IRenderedComponent<Switch> cut = Render<Switch>(ps => ps
            .Add(p => p.Value, false)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => observed = v)));

        cut.Find("input").Change(true);

        observed.ShouldBe(true);
    }

    [Fact]
    public void Switch_honours_the_Checked_alias()
    {
        bool? observed = null;

        IRenderedComponent<Switch> cut = Render<Switch>(ps => ps
            .Add(p => p.Checked, false)
            .Add(p => p.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => observed = v)));

        cut.Find("input").Change(true);

        observed.ShouldBe(true);
    }

    [Fact]
    public void ItemList_renders_bare_ListItem_children_without_a_type_argument()
    {
        IRenderedComponent<ItemList> cut = Render<ItemList>(ps => ps
            .AddChildContent<ListItem>(ip => ip
                .Add(p => p.Icon, "inbox")
                .Add(p => p.Text, "Inbox"))
            .AddChildContent<ListItem>(ip => ip
                .Add(p => p.Text, "Sent")));

        cut.Markup.ShouldContain("menu");
        cut.FindAll("li").Count.ShouldBe(2);
        cut.Markup.ShouldContain("Inbox");
        cut.Markup.ShouldContain("Sent");
    }

    [Fact]
    public void ItemList_still_honours_Dense()
    {
        IRenderedComponent<ItemList> cut = Render<ItemList>(ps => ps.Add(p => p.Dense, true));

        cut.Markup.ShouldContain("menu-sm");
    }

    [Fact]
    public void ListItem_renders_a_link_when_given_an_Href()
    {
        IRenderedComponent<ListItem> cut = Render<ListItem>(ps => ps
            .Add(p => p.Href, "/inbox")
            .Add(p => p.Text, "Inbox"));

        cut.Find("a").GetAttribute("href").ShouldBe("/inbox");
    }

    [Fact]
    public void ListItem_still_carries_an_untyped_Value()
    {
        IRenderedComponent<ListItem> cut = Render<ListItem>(ps => ps
            .Add(p => p.Value, Guid.Empty)
            .Add(p => p.Text, "Inbox"));

        cut.Instance.Value.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Checkbox_renders_without_an_explicit_type_argument()
    {
        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps
            .Add(p => p.Label, "Accept terms")
            .Add(p => p.Value, true));

        cut.Markup.ShouldContain("checkbox");
        cut.Markup.ShouldContain("Accept terms");
        cut.Find("input").HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Checkbox_reports_the_toggled_value_as_a_bool()
    {
        bool? observed = null;

        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps
            .Add(p => p.Value, false)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<bool>(this, v => observed = v)));

        cut.Find("input").Change(true);

        observed.ShouldBe(true);
    }

    [Fact]
    public void Checkbox_honours_the_Checked_alias()
    {
        bool? observed = null;

        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps
            .Add(p => p.Checked, false)
            .Add(p => p.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => observed = v)));

        cut.Find("input").Change(true);

        observed.ShouldBe(true);
    }

    [Fact]
    public void Checkbox_honours_an_uncontrolled_Value()
    {
        // Regression: the old `Checked is not null` guard was always true for T = bool, so an
        // uncontrolled <Checkbox Value="true" /> read `Checked` (default false) and rendered
        // unchecked. Same bug #12 removed from Switch.
        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps.Add(p => p.Value, true));

        cut.Find("input").HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Chip_renders_without_an_explicit_type_argument()
    {
        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Text, "Draft")
            .Add(p => p.Color, Color.Primary));

        cut.Markup.ShouldContain("badge");
        cut.Markup.ShouldContain("Draft");
    }

    [Fact]
    public void Chip_renders_a_leading_icon_and_stays_a_span_without_OnClick()
    {
        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Text, "Starred")
            .Add(p => p.Icon, "star"));

        cut.Find("span.badge").ShouldNotBeNull();
        cut.FindAll("button").ShouldBeEmpty();
    }

    [Fact]
    public void Chip_hands_its_own_instance_to_OnClose()
    {
        Chip? closed = null;

        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Text, "Closable")
            .Add(p => p.OnClose, EventCallback.Factory.Create<Chip>(this, c => closed = c)));

        cut.Find("button").Click();

        closed.ShouldBeSameAs(cut.Instance);
    }

    [Fact]
    public void StatusChip_does_not_splat_a_stray_type_attribute()
    {
        // A leftover T="string" on a now-non-generic Chip still *compiles* — it is swallowed by
        // CaptureUnmatchedValues and splatted into the DOM as a bogus attribute. Only a rendered
        // assertion catches that, so pin it here for the two in-library Chip consumers.
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Label, "Active")
            .Add(p => p.Color, Color.Success));

        cut.Find("span.badge").HasAttribute("T").ShouldBeFalse();
    }

    [Fact]
    public void Chip_still_carries_an_untyped_Value()
    {
        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Value, 42)
            .Add(p => p.Text, "Answer"));

        cut.Instance.Value.ShouldBe(42);
    }

    [Fact]
    public void Select_registers_its_items_in_markup_order()
    {
        IRenderedComponent<Select<string>> cut = Render<Select<string>>(ps => ps
            .Add(p => p.Value, "banana")
            .AddChildContent<SelectItem<string>>(ip => ip
                .Add(p => p.Value, "apple")
                .AddChildContent("Apple"))
            .AddChildContent<SelectItem<string>>(ip => ip
                .Add(p => p.Value, "banana")
                .AddChildContent("Banana")));

        IReadOnlyList<IElement> options = cut.FindAll("option");
        options.Count.ShouldBe(2);
        options[0].TextContent.ShouldBe("Apple");
        options[1].TextContent.ShouldBe("Banana");

        // Index-based mapping: the selected option must be the one matching Value.
        options[1].HasAttribute("selected").ShouldBeTrue();
        options[0].HasAttribute("selected").ShouldBeFalse();
    }

    [Fact]
    public void RadioGroup_marks_the_child_matching_its_value()
    {
        IRenderedComponent<RadioGroup<string>> cut = Render<RadioGroup<string>>(ps => ps
            .Add(p => p.Value, "phone")
            .AddChildContent<Radio<string>>(ip => ip
                .Add(p => p.Value, "email")
                .Add(p => p.Label, "Email"))
            .AddChildContent<Radio<string>>(ip => ip
                .Add(p => p.Value, "phone")
                .Add(p => p.Label, "Phone")));

        IReadOnlyList<IElement> radios = cut.FindAll("input[type=radio]");
        radios.Count.ShouldBe(2);
        radios[0].HasAttribute("checked").ShouldBeFalse();
        radios[1].HasAttribute("checked").ShouldBeTrue();

        // All children share the group's generated name so the browser enforces single selection.
        radios[0].GetAttribute("name").ShouldBe(radios[1].GetAttribute("name"));
    }
}
