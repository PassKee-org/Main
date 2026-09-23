using System.Globalization;
using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Extensions;

public class DateTimeExtensionsTest
{
    [Theory]
    [InlineData("2022-10-31T00:00:00.0000000", "2022-01-01T00:00:00.0000000")]
    [InlineData("2023-10-30T00:00:00.0000000", "2023-01-01T00:00:00.0000000")]
    public void StartOfYear_ShouldReturnFirstDayOfYear(string dateString, string expectDateString)
    {
        var date = DateTime.Parse(dateString);
        Assert.Equal(expectDateString, date.StartOfYear().ToString("o"));
    }

    [Theory]
    [InlineData("2022-10-31T00:00:00.0000000", "2022-12-31T00:00:00.0000000")]
    [InlineData("2023-10-30T00:00:00.0000000", "2023-12-31T00:00:00.0000000")]
    public void EndOfYear_ShouldReturnLastDayOfYear(string dateString, string expectDateString)
    {
        var date = DateTime.Parse(dateString);
        Assert.Equal(expectDateString, date.EndOfYear().ToString("o"));
    }

    [Theory]
    [InlineData("2022-10-31T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
    [InlineData("2022-10-30T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
    [InlineData("2022-10-21T00:00:00.0000000", "2022-10-01T00:00:00.0000000")]
    public void StartOfMonth_ShouldReturnFirstDayOfMonth(string dateString, string expectDateString)
    {
        var date = DateTime.Parse(dateString);
        Assert.Equal(expectDateString, date.StartOfMonth().ToString("o"));
    }

    [Theory]
    [InlineData("2022-10-31T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
    [InlineData("2022-10-30T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
    [InlineData("2022-10-21T00:00:00.0000000", "2022-10-31T00:00:00.0000000")]
    public void EndOfMonth_ShouldReturnLastDayOfMonth(string dateString, string expectDateString)
    {
        var date = DateTime.Parse(dateString);
        Assert.Equal(expectDateString, date.EndOfMonth().ToString("o"));
    }

    [Theory]
    [InlineData("2007-01-05T01:00:00Z", 1)]
    [InlineData("2007-03-01T01:00:00Z", 9)]
    [InlineData("2007-12-25T01:00:00Z", 52)]
    [InlineData("2007-12-30T01:00:00Z", 52)]
    public void GetIso8601WeekOfYear_ShouldGetCorrectWeekNumber(string dateString, int expectWeekNumber)
    {
        var date = DateTime.Parse(dateString);
        Assert.Equal(expectWeekNumber, date.GetIso8601WeekOfYear());
    }

    [Theory]
    [InlineData("2022-10-31T00:00:00.0000000Z", "2022-10-31T00:00:00.0000000Z")]
    [InlineData("2022-10-30T00:00:00.0000000Z", "2022-10-24T00:00:00.0000000Z")]
    [InlineData("2022-10-21T00:00:00.0000000Z", "2022-10-17T00:00:00.0000000Z")]
    public void StartOfWeek_ShouldReturnMonday(string dateString, string expectDateString)
    {
        var date = DateTime.Parse(dateString).ToUniversalTime();
        Assert.Equal(expectDateString, date.StartOfWeek().ToString("o"));
    }

    [Fact]
    public void WithDate_ShouldKeepTimeAndKindForDateTime()
    {
        var source = new DateTime(2026, 4, 4, 14, 30, 15, DateTimeKind.Utc);
        var newDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Local);

        var actual = source.WithDate(newDate);

        Assert.Equal(new DateTime(2026, 5, 1, 14, 30, 15, DateTimeKind.Utc), actual);
    }

    [Fact]
    public void WithDate_ShouldReturnNullForNullableDateTimeWhenSourceIsNull()
    {
        DateTime? source = null;
        var actual = source.WithDate(new DateTime(2026, 5, 1));

        Assert.Null(actual);
    }

    [Fact]
    public void WithDate_ShouldKeepTimeAndKindForNullableDateTime()
    {
        DateTime? source = new DateTime(2026, 4, 4, 6, 45, 0, DateTimeKind.Local);
        var actual = source.WithDate(new DateTime(2026, 5, 1));

        Assert.Equal(new DateTime(2026, 5, 1, 6, 45, 0, DateTimeKind.Local), actual);
    }

    [Fact]
    public void ToUnixTime_ShouldReturnMillisecondsSinceEpoch()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(0, epoch.ToUnixTime());

        var date = new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc);
        Assert.Equal(1000, date.ToUnixTime());
    }

    [Fact]
    public void StartOfDay_And_EndOfDay_ShouldReturnBoundaries()
    {
        var date = new DateTime(2026, 3, 15, 14, 25, 30);
        Assert.Equal(new DateTime(2026, 3, 15, 0, 0, 0), date.StartOfDay());
        Assert.Equal(new DateTime(2026, 3, 15, 23, 59, 59).AddTicks(9999999), date.EndOfDay());
    }

    [Fact]
    public void Round_ShouldRoundCorrectly()
    {
        var date = new DateTime(2026, 3, 15, 10, 30, 45, 600);

        var roundSec = date.Round(DateTimeExtensions.RoundTo.Second);
        Assert.Equal(46, roundSec.Second);

        var roundMin = date.Round(DateTimeExtensions.RoundTo.Minute);
        Assert.Equal(31, roundMin.Minute);

        var roundHour = date.Round(DateTimeExtensions.RoundTo.Hour);
        Assert.Equal(11, roundHour.Hour);

        var roundDay = date.Round(DateTimeExtensions.RoundTo.Day);
        Assert.Equal(15, roundDay.Day);
    }

    [Fact]
    public void ToDateAndRemoveTimeZone_ShouldReturnDateOnly()
    {
        var dt = new DateTime(2026, 7, 20, 15, 30, 0);
        var dateOnly = dt.ToDateAndRemoveTimeZone();

        Assert.Equal(new DateOnly(2026, 7, 20), dateOnly);
    }

    [Fact]
    public void ToDateOnly_ShouldConvertCorrectly()
    {
        var dt = new DateTime(2026, 12, 25, 8, 0, 0);
        Assert.Equal(new DateOnly(2026, 12, 25), dt.ToDateOnly());
    }

    [Fact]
    public void GetDateRange_WhenEndBeforeStart_ShouldThrowArgumentException()
    {
        var start = new DateTime(2026, 5, 10);
        var end = new DateTime(2026, 5, 5);

        Assert.Throws<ArgumentException>(() => start.GetDateRange(end).ToList());
    }

    [Fact]
    public void GetDateRange_ShouldReturnInclusiveRange()
    {
        var start = new DateTime(2026, 5, 1);
        var end = new DateTime(2026, 5, 3);

        var range = start.GetDateRange(end).ToList();

        Assert.Equal(3, range.Count);
        Assert.Equal(new DateTime(2026, 5, 1), range[0]);
        Assert.Equal(new DateTime(2026, 5, 2), range[1]);
        Assert.Equal(new DateTime(2026, 5, 3), range[2]);
    }

    [Fact]
    public void TimeAgo_JustNow_ShouldReturnResourceString()
    {
        var now = DateTime.UtcNow;
        var result = now.TimeAgo(DateTimeKind.Utc);

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void ToSimpleFormattedString_ShouldReturnFormatted()
    {
        var dt = new DateTime(2026, 9, 23, 14, 5, 0);
        var result = dt.ToSimpleFormattedString();

        Assert.Equal(dt.ToString("hh:mm dd-MM-yyyy"), result);
    }

    [Fact]
    public void ToTimeZone_ShouldConvertUtcTimeToTargetZone()
    {
        var utcTime = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var converted = utcTime.ToTimeZone("UTC");

        Assert.Equal(12, converted.Hour);
    }

    [Fact]
    public void ToTimeZone_WhenTimeZoneEmpty_ShouldDefaultToUtc()
    {
        var utcTime = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var converted = utcTime.ToTimeZone(string.Empty);

        Assert.Equal(12, converted.Hour);
    }

    [Fact]
    public void ToTimeZone_Nullable_WhenNull_ShouldReturnNull()
    {
        DateTime? nullDate = null;
        Assert.Null(nullDate.ToTimeZone("UTC"));
    }

    [Fact]
    public void ToDateTimeOffset_ShouldPreserveOffset()
    {
        var utcTime = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var offset = utcTime.ToDateTimeOffset("UTC");

        Assert.Equal(TimeSpan.Zero, offset.Offset);
        Assert.Equal(12, offset.Hour);
    }

    [Fact]
    public void ToDateTimeOffset_Nullable_WhenNull_ShouldReturnNull()
    {
        DateTime? nullDate = null;
        Assert.Null(nullDate.ToDateTimeOffset("UTC"));
    }
}

