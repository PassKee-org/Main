namespace PassKee.Business.Common.Constants
{
    public static class GlobalConstants
    {
        public static bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }

        }
        
        /// <summary>
        /// Default currency code
        /// </summary>
        public const string DefaultCurrencyCode = "USD";
        
        /// <summary>
        /// Default Time Zone
        /// </summary>
        public const string DefaultTimeZone = "UTC";
        
        /// <summary>
        /// Page size
        /// </summary>
        public const int DefaultListPageSize = 20;
        
        /// <summary>
        /// Page size
        /// </summary>
        public const int ListPageSize = 30;

        /// <summary>
        /// Page size for day-grouped time entry pagination
        /// </summary>
        public const int TimeEntryGroupedByDayPageSize = 3;

        /// <summary>
        /// The maximum number of users who will be given access
        /// </summary>
        public const int ApplicationMaxShares = 20;
        
        public static readonly TimeSpan EndOfDay = TimeSpan.FromHours(23)
            .Add(TimeSpan.FromMinutes(59))
            .Add(TimeSpan.FromSeconds(59))
            .Add(TimeSpan.FromMilliseconds(999));
        
        /// <summary>
        /// Number of minutes before which a notification must be sent to the user
        /// </summary>
        public static readonly TimeSpan TaskReminderTimeout = TimeSpan.FromMinutes(35);
    }
}
