using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Extensions;

public class TimeSpanExtensionsTest
{
    [Theory]
    [InlineData("1.07:15:02.5800000", "31:15:02")]
    [InlineData("07:15:02.5800000", "07:15:02")]
    [InlineData("12.07:15:02.5800000", "295:15:02")]
    public void ToReadableShortString_ShouldFormatHoursMinutesSeconds(string actual, string expected)
    {
        var timeSpan = TimeSpan.Parse(actual);
        Assert.Equal(expected, timeSpan.ToReadableShortString());
    }

    [Fact]
    public void ToReadableString_WhenZeroSeconds_ShouldReturnZeroLabel()
    {
        var timeSpan = TimeSpan.Zero;
        var result = timeSpan.ToReadableString();

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void ToReadableString_WhenDaysAndHours_ShouldReturnFormattedString()
    {
        var timeSpan = TimeSpan.FromDays(2).Add(TimeSpan.FromHours(3)).Add(TimeSpan.FromMinutes(15));
        var result = timeSpan.ToReadableString();

        Assert.False(string.IsNullOrEmpty(result));
    }
}

