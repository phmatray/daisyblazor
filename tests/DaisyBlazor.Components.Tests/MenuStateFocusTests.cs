using DaisyBlazor;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Menu uncontrolled state (#108), focus management (#109), IsOpen (#110), tabindex activator (#111).</summary>
public class MenuStateFocusTests : BunitContext
{
    public MenuStateFocusTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Uncontrolled_menu_stays_open_across_a_parent_render()
    {
        List<bool> changes = [];
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.Label, "Actions")
            .Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, v => changes.Add(v)))
            .AddChildContent("<li>Item</li>"));

        cut.Find("button").Click();
        cut.Render(ps => ps.Add(p => p.Dense, true)); // parent re-render, Open still unbound (false)

        changes.ShouldBe([true]);
        cut.FindAll("ul[role=menu]").Count.ShouldBe(1);
    }

    [Fact]
    public void Opening_and_closing_move_focus_through_menu_js()
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps.Add(p => p.Label, "Actions").AddChildContent("<li>Item</li>"));

        cut.Find("button").Click();
        JSInterop.VerifyInvoke("open");
        cut.Find(".dropdown").KeyDown(key: "Escape");
        JSInterop.VerifyInvoke("close");
        cut.FindAll("ul[role=menu]").Count.ShouldBe(0);
    }

    [Fact]
    public void Custom_activator_reads_IsOpen_for_aria_expanded()
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.ActivatorContent, (MenuActivator m) => b =>
            {
                b.OpenElement(0, "button");
                b.AddAttribute(1, "tabindex", "-1");
                b.AddAttribute(2, "aria-expanded", m.IsOpen ? "true" : "false");
                b.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, m.ToggleAsync));
                b.CloseElement();
            })
            .AddChildContent("<li>Item</li>"));

        cut.Find("button").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find("button").Click();
        cut.Find("button").GetAttribute("aria-expanded").ShouldBe("true");
        // daisyUI's `.dropdown:focus-within > [tabindex]:first-child { pointer-events: none }` is overridden.
        cut.Find(".dropdown").ClassList.ShouldContain("[&>[tabindex]:first-child]:pointer-events-auto!");
    }
}
