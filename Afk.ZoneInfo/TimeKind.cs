namespace Afk.ZoneInfo
{
    /// <summary>
    /// Définit les types de date possible
    /// </summary>
    enum TimeKind
    {
        /// <summary>
        /// Default. Wall clock time
        /// w : Wall
        /// </summary>
        LocalWallTime,

        /// <summary>
        /// Local standard time; winter time
        /// s : standard
        /// </summary>
        LocalStandardTime,

        /// <summary>
        /// UTC time
        /// g, u, z : Greenwich, Universal, Zulu
        /// </summary>
        UniversalTime
    }
}
