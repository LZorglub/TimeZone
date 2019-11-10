using System;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Provides information about a time zone adjustment, such as the transition to and from daylight saving time.
    /// </summary>
    public sealed class TzAdjustmentRule
    {
        /// <summary>
        /// Initialize a new instance of <see cref="TzAdjustmentRule"/>
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        internal TzAdjustmentRule(DateTime dateStart, DateTime dateEnd)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;
        }

        /// <summary>
        /// Gets the date when the adjustment rule takes effect.
        /// </summary>
        public DateTime DateStart { get; private set; }

        /// <summary>
        /// Gets the date when the adjustment rule ceases to be in effect.
        /// </summary>
        public DateTime DateEnd { get; private set; }
    }
}
