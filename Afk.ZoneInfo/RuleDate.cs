using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Représente un positionnement de date sans spécification d'année
    /// </summary>
    class RuleDate
    {
        private ConcurrentDictionary<int, DateTime> dYears;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="RuleDate"/>
        /// </summary>
        internal RuleDate()
        {
            dYears = new ConcurrentDictionary<int, DateTime>();
        }

        /// <summary>
        /// Obtient la spécification du jour de la règle
        /// </summary>
        public DayOfRule Day { get; internal set; }

        /// <summary>
        /// Obtient le mois de la règle
        /// </summary>
        public int Month { get; internal set; }

        /// <summary>
        /// Obtient l'heure de la règle
        /// </summary>
        public TimeSpan At { get; internal set; }

        /// <summary>
        /// Obtient le type de date spécifié par la règle
        /// </summary>
        public TimeKind AtKind { get; internal set; }

        /// <summary>
        /// Obtient une date sans spécification de fuseau en fonction de la définition de la règle
        /// </summary>
        /// <param name="year">Année de positionnement de la date</param>
        /// <returns>Datetime représentant la date de la règle</returns>
        /// <remarks>La date est de type DateTimeKind.Unspecified</remarks>
        public DateTime ToUnspecifiedTime(int year)
        {

            if (dYears.ContainsKey(year)) return dYears[year];

            DateTime result = DateTime.MinValue;

            if (Day.DayWork == DayWorkKind.Dom)
            {
                result = new DateTime(year, this.Month, Day.DayOfMonth, 0, 0, 0, DateTimeKind.Unspecified);
            }
            else if (Day.DayWork == DayWorkKind.DowLeq)
            {
                if (Day.DayOfMonth == -1)
                {
                    DateTime leq = new DateTime(year, this.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1).AddDays(-1);
                    while (leq.DayOfWeek != Day.DayWeek) leq = leq.AddDays(-1);
                    result = leq;
                }
                else
                {
                    DateTime leq = new DateTime(year, this.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1).AddDays(-1);
                    while (leq.Day > Day.DayOfMonth || leq.DayOfWeek != Day.DayWeek) leq = leq.AddDays(-1);
                    result = leq;
                }
            }
            else
            {
                DateTime geq = new DateTime(year, this.Month, Day.DayOfMonth, 0, 0, 0, DateTimeKind.Unspecified);
                while (geq.DayOfWeek != Day.DayWeek) geq = geq.AddDays(1);
                result = geq;
            }

            if (result != DateTime.MinValue)
                result = result.Add(this.At);
            dYears[year] = result;
            return result;
        }
    }
}
