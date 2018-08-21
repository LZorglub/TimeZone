namespace Afk.ZoneInfo
{
    /// <summary>
    /// Définit le type de comparaison pour la recherche de règle.
    /// Règle applicable strictement avant la date ou règle applicable avant et égale à la date
    /// </summary>
    enum RuleSearchKind
    {
        /// <summary>
        /// Strictement inférieure
        /// </summary>
        LessThan,

        /// <summary>
        /// Inférieure ou égale
        /// </summary>
        LessThanOrEqual,
    }
}