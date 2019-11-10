namespace Afk.ZoneInfo
{
    /// <summary>
    /// Représente une règle de fin de zone.
    /// </summary>
    /// <remarks>Cette règle définie une date fixe par l'association de l'année.</remarks>
    class TzTimeZoneRuleUntil : RuleDate
    {

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="TzTimeZoneRuleUntil"/>
        /// </summary>
        internal TzTimeZoneRuleUntil()
        {
        }

        /// <summary>
        /// Obtient l'année de borne supérieure de la règle de zone
        /// </summary>
        public int Year { get; internal set; }
    }
}
