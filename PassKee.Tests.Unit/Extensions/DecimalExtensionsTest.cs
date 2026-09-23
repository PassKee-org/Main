using PassKee.Business.Extensions;

namespace PassKee.Tests.Unit.Extensions;

public class DecimalExtensionsTest
{
    [Theory]
    [InlineData(12, null, "12.00")]
    [InlineData(100000, null, "100,000.00")]
    [InlineData(1234567.89, null, "1,234,567.89")]
    [InlineData(100000, "$", "100,000.00 $")]
    [InlineData(0, null, "0.00")]
    [InlineData(-12.5, null, "-12.50")]
    [InlineData(-1500.5, "EUR", "-1,500.50 EUR")]
    public void ShouldFormatMoneyWithThousandsSeparator(decimal value, string? symbol, string expected)
    {
        var result = value.ToMoneyFormat(symbol);

        Assert.Equal(expected, result);
    }
}

