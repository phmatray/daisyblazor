using System.Globalization;
using AngleSharp.Dom;
using DaisyBlazor;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The visible field follows DateFormat (or the current culture), never the browser's locale, which
/// only drives the native picker kept invisibly behind the calendar glyph.
/// </summary>
public class DatePickerTests : BunitContext
{
    private static readonly DateTime March5 = new(2026, 3, 5);

    private IElement Text(IRenderedComponent<DatePicker> cut) => cut.Find("input[type=text]");

    [Fact]
    public void Shows_the_date_in_DateFormat()
    {
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, March5)
            .Add(p => p.DateFormat, "dd/MM/yyyy"));

        Text(cut).GetAttribute("value").ShouldBe("05/03/2026");
        cut.Find("input[type=date]").GetAttribute("value").ShouldBe("2026-03-05");
    }

    [Fact]
    public void Defaults_to_the_current_culture_short_date()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        try
        {
            IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps.Add(p => p.Date, March5));

            Text(cut).GetAttribute("value").ShouldBe(March5.ToString("d", CultureInfo.CurrentCulture));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("05/03/2026")]
    [InlineData("06/03/2026")]
    public void Typed_date_in_the_format_is_accepted(string typed)
    {
        DateTime? bound = March5;
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, bound)
            .Add(p => p.DateFormat, "dd/MM/yyyy")
            .Add(p => p.DateChanged, (DateTime? d) => bound = d));

        Text(cut).Change(typed);

        bound.ShouldBe(DateTime.ParseExact(typed, "dd/MM/yyyy", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Garbage_snaps_back_and_keeps_the_date()
    {
        DateTime? bound = March5;
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, bound)
            .Add(p => p.DateFormat, "dd/MM/yyyy")
            .Add(p => p.DateChanged, (DateTime? d) => bound = d));

        Text(cut).Change("not a date");

        bound.ShouldBe(March5);
        Text(cut).GetAttribute("value").ShouldBe("05/03/2026");
    }

    [Fact]
    public void Native_picker_sets_the_date()
    {
        DateTime? bound = March5;
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, bound)
            .Add(p => p.DateChanged, (DateTime? d) => bound = d));

        cut.Find("input[type=date]").Change("2026-04-01");

        bound.ShouldBe(new DateTime(2026, 4, 1));
    }

    [Fact]
    public void Out_of_range_entry_is_rejected()
    {
        DateTime? bound = March5;
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, bound)
            .Add(p => p.DateFormat, "dd/MM/yyyy")
            .Add(p => p.MaxDate, new DateTime(2026, 3, 31))
            .Add(p => p.DateChanged, (DateTime? d) => bound = d));

        Text(cut).Change("01/04/2026");

        bound.ShouldBe(March5);
    }

    [Fact]
    public void Clearable_empty_text_clears_the_date()
    {
        DateTime? bound = March5;
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, bound)
            .Add(p => p.Clearable, true)
            .Add(p => p.DateChanged, (DateTime? d) => bound = d));

        Text(cut).Change(string.Empty);

        bound.ShouldBeNull();
    }

    [Fact]
    public void Not_editable_makes_the_text_read_only()
    {
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Date, March5)
            .Add(p => p.Editable, false));

        Text(cut).HasAttribute("readonly").ShouldBeTrue();
        cut.FindAll("input[type=date]").Count.ShouldBe(1);
    }
}
