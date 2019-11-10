using System;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Définit le positionnement d'un jour d'une règle
    /// </summary>
    struct DayOfRule
    {
        private DayWorkKind _dwork;
        private DayOfWeek _dweek;
        private int _dmonth;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="DayOfRule"/>
        /// </summary>
        /// <param name="dayWork">Type de positionnement</param>
        /// <param name="dayWeek">Jour de la semaine à respecter</param>
        /// <param name="dayMonth">Jour du mois à respecter</param>
        public DayOfRule(DayWorkKind dayWork, DayOfWeek dayWeek, int dayMonth)
        {
            _dwork = dayWork; _dweek = dayWeek; _dmonth = dayMonth;
        }

        /// <summary>
        /// Obtient le type de positionnement
        /// </summary>
        public DayWorkKind DayWork { get { return _dwork; } }

        /// <summary>
        /// Obtient la date de la journée
        /// </summary>
        /// <remarks>La propriété est le positionnement minimum/maximum
        /// si <see cref="DayWork"/> est égal à <see cref="DayWorkKind.DowGeq"/> ou <see cref="DayWorkKind.DowLeq"/>
        /// Sun>=8 sera le premier dimanche après le 8 du mois.
        /// </remarks>
        public int DayOfMonth { get { return _dmonth; } }

        /// <summary>
        /// Obtient le jour de la semaine correspondant à la date
        /// </summary>
        /// <remarks>La propriété est ignorée si DayWork = DayWorkKind.Dom</remarks>
        public DayOfWeek DayWeek { get { return _dweek; } }
    }
}
