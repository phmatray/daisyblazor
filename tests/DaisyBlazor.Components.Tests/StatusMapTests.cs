using DaisyBlazor.Data;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Tests for StatusMap and the StatusChip that consumes it.
/// </summary>
/// <remarks>
/// StatusChip used to require <c>Color</c>, so it demanded the answer to the only hard part and
/// added nothing over a plain Chip. Measured in a reference consumer: StatusChip used 4 times
/// against 122 for Chip, with the colour coming from hand-written switch helpers duplicated across
/// list, card and detail views. These tests pin the resolution rules — and, just as importantly,
/// that the old explicit-Color usage still renders exactly as before.
/// </remarks>
public enum Shipment
{
    Pending,
    InTransit,
    Delivered,
    Lost
}

public class StatusMapTests
{
    private static StatusMap Build() => StatusMap.For<Shipment>()
        .Add(Shipment.Delivered, Color.Success)
        .Add([Shipment.Pending, Shipment.InTransit], Color.Info, "local_shipping")
        .Add(Shipment.Lost, Color.Error, "error")
        .Default(Color.Default, "help")
        .Build();

    [Fact]
    public void Resolves_a_mapped_value()
    {
        Build().Resolve(Shipment.Delivered).ShouldBe(new StatusStyle(Color.Success));
    }

    [Fact]
    public void Maps_several_values_to_one_treatment()
    {
        StatusMap map = Build();

        map.Resolve(Shipment.Pending).ShouldBe(new StatusStyle(Color.Info, "local_shipping"));
        map.Resolve(Shipment.InTransit).ShouldBe(new StatusStyle(Color.Info, "local_shipping"));
    }

    [Fact]
    public void Falls_back_for_null_and_unmapped_values()
    {
        StatusMap map = StatusMap.For<Shipment>()
            .Add(Shipment.Delivered, Color.Success)
            .Default(Color.Warning, "help")
            .Build();

        map.Resolve(null).ShouldBe(new StatusStyle(Color.Warning, "help"));
        map.Resolve(Shipment.Lost).ShouldBe(new StatusStyle(Color.Warning, "help"));
    }

    [Fact]
    public void Default_fallback_is_Color_Default_when_not_configured()
    {
        StatusMap map = StatusMap.For<Shipment>().Add(Shipment.Lost, Color.Error).Build();

        map.Resolve(Shipment.Pending).ShouldBe(new StatusStyle(Color.Default));
    }

    [Fact]
    public void Later_entries_win_over_earlier_ones()
    {
        StatusMap map = StatusMap.For<Shipment>()
            .Add(Shipment.Lost, Color.Warning)
            .Add(Shipment.Lost, Color.Error)
            .Build();

        map.Resolve(Shipment.Lost).Color.ShouldBe(Color.Error);
    }

    [Fact]
    public void A_different_enum_with_the_same_ordinal_does_not_collide()
    {
        // Boxed enum equality compares type as well as value; this guards the object-keyed lookup.
        StatusMap map = StatusMap.For<Shipment>().Add(Shipment.Pending, Color.Info).Build();

        map.Resolve(DayOfWeek.Sunday).ShouldBe(new StatusStyle(Color.Default));
    }
}

public class StatusChipRenderTests : BunitContext
{
    private static StatusMap Map => StatusMap.For<Shipment>()
        .Add(Shipment.Delivered, Color.Success)
        .Add(Shipment.Lost, Color.Error, "error")
        .Default(Color.Default)
        .Build();

    [Fact]
    public void Resolves_colour_from_the_map_so_the_call_site_does_not_have_to()
    {
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Status, Shipment.Delivered)
            .Add(p => p.Map, Map)
            .Add(p => p.Label, "Delivered"));

        cut.Markup.ShouldContain("badge-success");
        cut.Markup.ShouldContain("Delivered");
    }

    [Fact]
    public void Resolves_the_icon_from_the_map_too()
    {
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Status, Shipment.Lost)
            .Add(p => p.Map, Map)
            .Add(p => p.Label, "Lost"));

        cut.Markup.ShouldContain("badge-error");
        cut.Markup.ShouldContain("error");
    }

    [Fact]
    public void Explicit_Color_beats_the_map()
    {
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Status, Shipment.Delivered)   // map says Success
            .Add(p => p.Map, Map)
            .Add(p => p.Color, Color.Warning)
            .Add(p => p.Label, "Overridden"));

        cut.Markup.ShouldContain("badge-warning");
        cut.Markup.ShouldNotContain("badge-success");
    }

    [Fact]
    public void The_pre_existing_Label_plus_Color_usage_is_unchanged()
    {
        // Every existing call site looks like this. It must keep working with no Status and no Map.
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Label, "Boarding")
            .Add(p => p.Color, Color.Info));

        cut.Markup.ShouldContain("badge-info");
        cut.Markup.ShouldContain("Boarding");
    }

    [Fact]
    public void Label_falls_back_to_the_status_text()
    {
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Status, Shipment.InTransit)
            .Add(p => p.Map, Map));

        cut.Markup.ShouldContain("InTransit");
    }

    [Fact]
    public void An_unmapped_status_uses_the_maps_default()
    {
        IRenderedComponent<StatusChip> cut = Render<StatusChip>(ps => ps
            .Add(p => p.Status, Shipment.Pending)
            .Add(p => p.Map, Map)
            .Add(p => p.Label, "Pending"));

        cut.Markup.ShouldContain("Pending");
        cut.Markup.ShouldNotContain("badge-success");
        cut.Markup.ShouldNotContain("badge-error");
    }
}
