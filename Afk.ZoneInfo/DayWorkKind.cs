namespace Afk.ZoneInfo
{
    /// <summary>
    /// Définit le type de jour 
    /// </summary>
    enum DayWorkKind
    {
        /// <summary>
        /// Day of month. Exact day date.
        /// </summary>
        Dom,
        /// <summary>
        /// Day of Week Greater or Equal
        /// </summary>
        DowGeq,
        /// <summary>
        /// Day of Week Less or Equal
        /// </summary>
        DowLeq,
    }
}
