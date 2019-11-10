using System;

namespace Afk.ZoneInfo
{

    /// <summary>
    /// Represents a timezone rule date
    /// </summary>
	class TzTimeZoneRuleDate
    {
        public static readonly TzTimeZoneRuleDate MinValue = new TzTimeZoneRuleDate()
        {
            UtcDate = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _local = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local),
            StandardOffset = TimeSpan.Zero,
            GmtOffset = TimeSpan.Zero,
        };
        public static readonly TzTimeZoneRuleDate MaxValue = new TzTimeZoneRuleDate()
        {
            UtcDate = new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc),
            _local = new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Local),
            StandardOffset = TimeSpan.Zero,
            GmtOffset = TimeSpan.Zero,
        };

        private DateTime _local;

        /// <summary>
        /// Gets the utc date of current instance
        /// </summary>
        public DateTime UtcDate { get; private set; }

        /// <summary>
        /// Gets the standard offset applicable to current instance
        /// </summary>
        public TimeSpan StandardOffset { get; private set; }

        /// <summary>
        /// Gets gmt offset applicable to current instance
        /// </summary>
        public TimeSpan GmtOffset { get; private set; }

        /// <summary>
        /// Initialize a new instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="gmtOffset"></param>
        /// <param name="standardOffset"></param>
		public TzTimeZoneRuleDate(DateTime dateTime, TimeSpan gmtOffset, TimeSpan standardOffset)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(dateTime));

            StandardOffset = standardOffset; GmtOffset = gmtOffset;
            if (dateTime.Kind == DateTimeKind.Local)
            {
                _local = dateTime;
                UtcDate = TzUtilities.GetDateTime(_local, GmtOffset, StandardOffset, DateTimeKind.Utc);
            }
            else
            {
                UtcDate = dateTime;
                _local = TzUtilities.GetDateTime(UtcDate, GmtOffset, StandardOffset, DateTimeKind.Local);
            }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
        /// <param name="utc"></param>
        /// <param name="local"></param>
        /// <param name="gmtOffset"></param>
        /// <param name="standardOffset"></param>
		public TzTimeZoneRuleDate(DateTime utc, DateTime local, TimeSpan gmtOffset, TimeSpan standardOffset)
        {
            if (utc.Kind != DateTimeKind.Utc) throw new ArgumentException("Datetime kind utc expected", nameof(utc));
            if (local.Kind != DateTimeKind.Local) throw new ArgumentException("Datetime kind local expected", nameof(local));

            StandardOffset = standardOffset; GmtOffset = gmtOffset;
            UtcDate = utc; _local = local;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
		private TzTimeZoneRuleDate()
        {
        }

        /// <summary>
        /// Gets the local datetime of current instance
        /// </summary>
        /// <returns></returns>
		public DateTime ToLocalTime()
        {
            return _local;
        }

        /// <summary>
        /// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est inférieur à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool operator <(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2)
        {
            return (date1.UtcDate < date2.UtcDate) && (date1._local < date2._local);
        }

        /// <summary>
        /// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est supérieur à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool operator >(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2)
        {
            return (date1.UtcDate > date2.UtcDate) && (date1._local > date2._local);
        }

        /// <summary>
        /// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est inférieur ou égal à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool operator <=(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2)
        {
            return (date1.UtcDate <= date2.UtcDate) && (date1._local <= date2._local);
        }

        /// <summary>
        /// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est supérieur ou égal à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool operator >=(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2)
        {
            return (date1.UtcDate >= date2.UtcDate) && (date1._local >= date2._local);
        }

        public static bool operator <(DateTime date1, TzTimeZoneRuleDate date2)
        {
            Func<DateTime, DateTime, bool> method = ((a, b) => a < b);
            return Op(date1, date2, method);
        }

        public static bool operator <(TzTimeZoneRuleDate date1, DateTime date2)
        {
            Func<DateTime, DateTime, bool> method = ((a, b) => b < a);
            return Op(date2, date1, method);
        }

        public static bool operator >(DateTime date1, TzTimeZoneRuleDate date2)
        {
            Func<DateTime, DateTime, bool> method = ((a, b) => a > b);
            return Op(date1, date2, method);
        }

        public static bool operator >(TzTimeZoneRuleDate date1, DateTime date2)
        {
            Func<DateTime, DateTime, bool> method = ((a, b) => b > a);
            return Op(date2, date1, method);
        }

        private static bool Op(DateTime date1, TzTimeZoneRuleDate date2, Func<DateTime, DateTime, bool> func)
        {
            if (date2 == null)
                throw new ArgumentNullException(nameof(date2));

            if (date1.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException("Unspecified kind date", nameof(date1));

            if (date1.Kind == DateTimeKind.Local)
                return func(date1, date2._local);

            return func(date1, date2.UtcDate);
        }
    }
}
