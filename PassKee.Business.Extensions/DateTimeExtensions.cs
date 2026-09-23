using System.Globalization;
using PassKee.Business.Extensions.Resources;

namespace PassKee.Business.Extensions
{
    public static class DateTimeExtensions
    {
        public enum RoundTo
        {
            Second,
            Minute,
            Hour, 
            Day
        }
        
        public static long ToUnixTime(this DateTime d)
        {
            return (long)(
                d - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
        }

        public static string TimeAgo(this DateTime dt, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            string resultTemplate;
            var timeNow = dateTimeKind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            TimeSpan span = timeNow - dt;
            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format(years == 1 ? R.YearAgo : R.YearsAgo, years);
            }

            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                if (months == 1)
                    resultTemplate = R.MonthAgo;
                else if (months >= 2 && months <= 4)
                    resultTemplate = R.MonthsAgoV2;
                else
                    resultTemplate = R.MonthsAgo;
                return String.Format(resultTemplate, months);
            }

            if (span.Days > 0)
            {
                if (span.Days == 1)
                    resultTemplate = R.DayAgo;
                else if (span.Days >= 2 && span.Days <= 4
                         || span.Days >= 22 && span.Days <= 24)
                {
                    resultTemplate = R.DaysAgoV2;
                }
                else
                    resultTemplate = R.DaysAgo;

                return String.Format(resultTemplate, span.Days);
            }

            if (span.Hours > 0)
            {
                if (span.Hours == 1)
                    resultTemplate = R.HourAgo;
                else if (span.Hours >= 2 && span.Hours <= 4
                         || span.Hours >= 22 && span.Hours <= 24)
                {
                    resultTemplate = R.HoursAgoV2;
                }
                else
                    resultTemplate = R.HoursAgo;

                return String.Format(resultTemplate, span.Hours);
            }

            if (span.Minutes > 0)
            {
                if (span.Minutes == 1)
                    resultTemplate = R.MinuteAgo;
                else if (span.Minutes >= 2 && span.Minutes <= 4
                         || span.Minutes >= 22 && span.Minutes <= 24
                         || span.Minutes >= 32 && span.Minutes <= 34
                         || span.Minutes >= 42 && span.Minutes <= 44
                         || span.Minutes >= 52 && span.Minutes <= 54)
                {
                    resultTemplate = R.MinutesAgoV2;
                }
                else
                    resultTemplate = R.MinutesAgo;

                return String.Format(resultTemplate, span.Minutes);
            }

            if (span.Seconds > 5)
            {
                if (span.Seconds >= 22 && span.Seconds <= 24
                    || span.Seconds >= 32 && span.Seconds <= 34
                    || span.Seconds >= 42 && span.Seconds <= 44
                    || span.Seconds >= 52 && span.Seconds <= 54)
                {
                    resultTemplate = R.SecondsAgoV2;
                }
                else
                    resultTemplate = R.SecondsAgo;

                return String.Format(resultTemplate, span.Seconds);
            }

            if (span.Seconds <= 5)
                return R.JustNow;
            return string.Empty;
        }

        public static string ToSimpleFormattedString(this DateTime dt)
        {
            return dt.ToString("hh:mm dd-MM-yyyy");
        }

        public static DateTime Round(this DateTime date, RoundTo roundTo)
        {
            DateTime dtRounded = new DateTime();

            switch (roundTo)
            {
                case RoundTo.Second:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                    if (date.Millisecond >= 500) dtRounded = dtRounded.AddSeconds(1);
                    break;
                case RoundTo.Minute:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    if (date.Second >= 30) dtRounded = dtRounded.AddMinutes(1);
                    break;
                case RoundTo.Hour:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                    if (date.Minute >= 30) dtRounded = dtRounded.AddHours(1);
                    break;
                case RoundTo.Day:
                    dtRounded = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                    if (date.Hour >= 12) dtRounded = dtRounded.AddDays(1);
                    break;
            }

            return dtRounded;
        }
        
        public static DateTime StartOfDay(this DateTime theDate)
        {
            return theDate.Date;
        }

        public static DateTime EndOfDay(this DateTime theDate)
        {
            return theDate.Date.AddDays(1).AddTicks(-1);
        }
        
        public static DateTime StartOfWeek(this DateTime now)
        {
            var startOfWeek = now.Date;
            while(startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            return startOfWeek;
        }
        
        public static DateTime StartOfMonth(this DateTime now)
        {
            return new DateTime(now.Year, now.Month, 1);
        }
        
        public static DateTime EndOfMonth(this DateTime now)
        {
            return now.StartOfMonth().AddMonths(1).AddDays(-1);
        }
        
        public static DateTime StartOfYear(this DateTime now)
        {
            return new DateTime(now.Year, 1, 1);
        }
        
        public static DateTime EndOfYear(this DateTime now)
        {
            return now.StartOfYear().AddYears(1).AddDays(-1);
        }
        
        /// <summary>
        /// This presumes that weeks start with Monday.
        /// Week 1 is the 1st week of the year with a Thursday in it.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime WithDate(this DateTime source, DateTime date)
        {
            var updated = date.Date.Add(source.TimeOfDay);
            return DateTime.SpecifyKind(updated, source.Kind);
        }

        public static DateTime? WithDate(this DateTime? source, DateTime date)
        {
            return source.HasValue ? source.Value.WithDate(date) : null;
        }
        
        /**
         * This method is used to get the date in the current time zone. It is necessary to
         * transfer to the server the local date of the client without the time zone
         */
        public static DateOnly ToDateAndRemoveTimeZone(this DateTime now)
        {
            return DateOnly.FromDateTime(new DateTime(now.Year, now.Month, now.Day));
        }
        
        public static IEnumerable<DateTime> GetDateRange(this DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");

            while (startDate <= endDate)
            {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }
        
        public static DateOnly ToDateOnly(this DateTime d)
        {
            return DateOnly.FromDateTime(d);
        }
        
        #region TimeZone

        private static TimeZoneInfo ResolveTimeZoneInfo(string? timeZone)
        {
            if (string.IsNullOrEmpty(timeZone))
                return TimeZoneInfo.Utc;
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            catch (Exception)
            {
                return TimeZoneInfo.Utc;
            }
        }
        
        public static DateTime? ToTimeZone(this DateTime? time, string timeZone)
        {
            return time?.ToTimeZone(timeZone);
        }

        /// <summary>
        /// Converts a UTC DateTime to the specified timezone.
        /// NOTE: The returned DateTime has Kind=Unspecified — this is a .NET limitation.
        /// DateTime cannot carry timezone info beyond Utc/Local/Unspecified.
        /// Use ToDateTimeOffset() when you need the offset (+05:00) preserved.
        /// </summary>
        public static DateTime ToTimeZone(this DateTime time, string timeZone)
        {
            if (time.Kind != DateTimeKind.Utc)
                time = time.ToUniversalTime();

            var tz = ResolveTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(time, DateTimeKind.Utc),
                tz
            );
            // Kind will be Unspecified (or Local if tz == system local) — by .NET design.
        }

        /// <summary>
        /// Converts a UTC DateTime to a DateTimeOffset in the specified timezone.
        /// The offset (+05:00 etc.) is preserved and visible.
        /// </summary>
        public static DateTimeOffset ToDateTimeOffset(this DateTime time, string timeZone)
        {
            if (time.Kind != DateTimeKind.Utc)
                time = time.ToUniversalTime();

            var tz = ResolveTimeZoneInfo(timeZone);
            var converted = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(time, DateTimeKind.Utc),
                tz
            );
            return new DateTimeOffset(converted, tz.GetUtcOffset(converted));
        }

        public static DateTimeOffset? ToDateTimeOffset(this DateTime? time, string timeZone)
        {
            return time?.ToDateTimeOffset(timeZone);
        }
        
        #endregion
    }
}
