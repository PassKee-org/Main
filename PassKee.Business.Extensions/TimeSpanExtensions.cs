using PassKee.Business.Extensions.Resources;

namespace PassKee.Business.Extensions;

public static class TimeSpanExtensions
{
    public static string ToReadableString(this TimeSpan span)
    {
        string formatted = string.Format("{0}{1}{2}{3}",
            span.Duration().Days > 0 
                ? string.Format(
                    span.Days == 1 ? R.TimeSpan_Pattern_Days : R.TimeSpan_Pattern_Plural_Days, 
                    span.Days
                ) 
                : string.Empty,
            span.Duration().Hours > 0 
                ? string.Format(
                    span.Hours == 1 ? R.TimeSpan_Pattern_Hours : R.TimeSpan_Pattern_Plural_Hours, 
                    span.Hours
                ) 
                : string.Empty,
            span.Duration().Minutes > 0 
                ? string.Format(
                    span.Minutes == 1 ? R.TimeSpan_Pattern_Minutes : R.TimeSpan_Pattern_Plural_Minutes, 
                    span.Minutes
                ) 
                : string.Empty,
            span.Duration().Seconds > 0 
                ? string.Format(
                    span.Seconds == 1 ? R.TimeSpan_Pattern_Seconds : R.TimeSpan_Pattern_Plural_Seconds, 
                    span.Seconds
                ) 
                : string.Empty
            );

        if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

        if (string.IsNullOrEmpty(formatted)) formatted = R.TimeSpan_ZeroSeconds;

        return formatted;
    }
    
    public static string ToReadableShortString(this TimeSpan span)
    {
        return $"{span.Duration().TotalHours:00}:{span.Duration().Minutes:00}:{span.Duration().Seconds:00}";
    }
}
