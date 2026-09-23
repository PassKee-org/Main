using System.Globalization;

namespace PassKee.Business.Extensions;

public static class DecimalExtensions
{
    public static string ToMoneyFormat(this decimal decimalValue, string? symbol = null)  
    {
        var formattedAmount = decimalValue.ToString("#,##0.00", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(symbol)
            ? formattedAmount
            : $"{formattedAmount} {symbol}";
    }
}
