using Microsoft.AspNetCore.Components;

namespace PassKee.Web.Core.Ui.Shared.Components.Calendar;

public partial class AppCalendar : ComponentBase
{
    [Parameter]
    public DateTime? Value { get; set; }

    [Parameter]
    public EventCallback<DateTime> ValueChanged { get; set; }

    [Parameter]
    public Func<DateTime, bool>? DisabledDateFunc { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    protected DateTime _displayMonth = DateTime.Today;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Value.HasValue && Value.Value != default)
        {
            _displayMonth = new DateTime(Value.Value.Year, Value.Value.Month, 1);
        }
    }

    protected void PreviousMonth()
    {
        _displayMonth = _displayMonth.AddMonths(-1);
    }

    protected void NextMonth()
    {
        _displayMonth = _displayMonth.AddMonths(1);
    }

    protected async Task SelectDate(DateTime date)
    {
        if (date.Month != _displayMonth.Month)
        {
            _displayMonth = new DateTime(date.Year, date.Month, 1);
        }
        await ValueChanged.InvokeAsync(date);
    }

    protected List<DateTime> GetCalendarDays()
    {
        var firstDayOfMonth = new DateTime(_displayMonth.Year, _displayMonth.Month, 1);
        var dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        // Adjust so Monday is 0, Sunday is 6
        var offset = (dayOfWeek == 0 ? 7 : dayOfWeek) - 1;

        var startDate = firstDayOfMonth.AddDays(-offset);
        var days = new List<DateTime>(42);

        for (int i = 0; i < 42; i++)
        {
            days.Add(startDate.AddDays(i));
        }

        return days;
    }

    protected static string GetDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mo",
            DayOfWeek.Tuesday => "Tu",
            DayOfWeek.Wednesday => "We",
            DayOfWeek.Thursday => "Th",
            DayOfWeek.Friday => "Fr",
            DayOfWeek.Saturday => "Sa",
            DayOfWeek.Sunday => "Su",
            _ => string.Empty
        };
    }
}
