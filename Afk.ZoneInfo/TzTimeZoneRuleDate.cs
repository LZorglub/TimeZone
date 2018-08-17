using System;

namespace Afk.ZoneInfo
{

    /// <summary>
    /// Represents a timezone rule date
    /// </summary>
	class TzTimeZoneRuleDate {
		public static readonly TzTimeZoneRuleDate MinValue;
		public static readonly TzTimeZoneRuleDate MaxValue;

        /// <summary>
        /// Initialize static instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
		static TzTimeZoneRuleDate() {
			MinValue = new TzTimeZoneRuleDate();
			MinValue._utc = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			MinValue._local = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
			MinValue._stdoff = TimeSpan.Zero;
			MinValue._gmtoff = TimeSpan.Zero;

			MaxValue = new TzTimeZoneRuleDate();
			MaxValue._utc = new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
			MaxValue._local = new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Local);
			MaxValue._stdoff = TimeSpan.Zero;
			MaxValue._gmtoff = TimeSpan.Zero;
		}

		private DateTime _utc;
		private DateTime _local;
		private TimeSpan _stdoff;
		private TimeSpan _gmtoff;

		public DateTime UtcDate { get { return _utc; } }

		public TimeSpan StandardOffset { get { return _stdoff; } }

		public TimeSpan GmtOffset { get { return _gmtoff; } }

        /// <summary>
        /// Initialize a new instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="gmtOffset"></param>
        /// <param name="standardOffset"></param>
		public TzTimeZoneRuleDate(DateTime dateTime, TimeSpan gmtOffset, TimeSpan standardOffset) {
			if (dateTime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", "dateTime");

			_stdoff = standardOffset; _gmtoff = gmtOffset;
			if (dateTime.Kind == DateTimeKind.Local) {
				_local = dateTime;
				_utc = TzUtilities.GetDateTime(_local, _gmtoff, _stdoff, DateTimeKind.Utc);
			}
			else {
				_utc = dateTime;
				_local = TzUtilities.GetDateTime(_utc, _gmtoff, _stdoff, DateTimeKind.Local);
			}
		}

        /// <summary>
        /// Initialize a new instance of <see cref="TzTimeZoneRuleDate"/>
        /// </summary>
        /// <param name="utc"></param>
        /// <param name="local"></param>
        /// <param name="gmtOffset"></param>
        /// <param name="standardOffset"></param>
		public TzTimeZoneRuleDate(DateTime utc, DateTime local, TimeSpan gmtOffset, TimeSpan standardOffset) {
			if (utc.Kind != DateTimeKind.Utc) throw new ArgumentException("Datetime kind utc expected", "utc");
			if (local.Kind != DateTimeKind.Local) throw new ArgumentException("Datetime kind local expected", "local");

			_stdoff = standardOffset; _gmtoff = gmtOffset;
			_utc = utc; _local = local;
		}

		private TzTimeZoneRuleDate() {
		}

        /// <summary>
        /// Gets the local datetime of current instance
        /// </summary>
        /// <returns></returns>
		public DateTime ToLocalTime() {
			return _local;
		}

		/// <summary>
		/// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est inférieur à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns></returns>
		public static bool operator<(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2) {
			return (date1.UtcDate < date2.UtcDate) && (date1._local < date2._local);
		}

		/// <summary>
		/// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est supérieur à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns></returns>
		public static bool operator >(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2) {
			return (date1.UtcDate > date2.UtcDate) && (date1._local > date2._local);
		}

		/// <summary>
		/// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est inférieur ou égal à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns></returns>
		public static bool operator <=(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2) {
			return (date1.UtcDate <= date2.UtcDate) && (date1._local <= date2._local);
		}

		/// <summary>
		/// Détermine si un <see cref="TzTimeZoneRuleDate"/> spécifié est supérieur ou égal à un autre <see cref="TzTimeZoneRuleDate"/> spécifié
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns></returns>
		public static bool operator >=(TzTimeZoneRuleDate date1, TzTimeZoneRuleDate date2) {
			return (date1.UtcDate >= date2.UtcDate) && (date1._local >= date2._local);
		}

		public static bool operator <(DateTime date1, TzTimeZoneRuleDate date2) {
			Func<DateTime, DateTime, bool> method = ((a, b) => a < b);
			return Op(date1, date2, method);
		}

		public static bool operator <(TzTimeZoneRuleDate date1, DateTime date2) {
			Func<DateTime, DateTime, bool> method = ((a, b) => b < a);
			return Op(date2, date1, method);
		}

		public static bool operator >(DateTime date1, TzTimeZoneRuleDate date2) {
			Func<DateTime, DateTime, bool> method = ((a, b) => a > b);
			return Op(date1, date2, method);
		}

		public static bool operator >(TzTimeZoneRuleDate date1, DateTime date2) {
			Func<DateTime, DateTime, bool> method = ((a, b) => b > a);
			return Op(date2, date1, method);
		}

		private static bool Op(DateTime date1, TzTimeZoneRuleDate date2, Func<DateTime, DateTime, bool> func) {
			if (date2 == null)
				throw new ArgumentNullException("date2");

			if (date1.Kind == DateTimeKind.Unspecified)
				throw new ArgumentException("Unspecified kind date", "date1");

			if (date1.Kind == DateTimeKind.Local)
				return func(date1, date2._local);

			return func(date1, date2._utc);
		}
	}
}
