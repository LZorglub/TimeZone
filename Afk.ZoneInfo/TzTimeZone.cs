using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Represents a time zone.
    /// </summary>
    /// <remarks>
    /// The format of each line is:
    /// Zone	NAME		GMTOFF	RULES	FORMAT	[UNTIL]
    /// </remarks>
    public sealed class TzTimeZone
    {
        private List<TzTimeZoneRule> _zoneRules;
        private Dictionary<int, List<TzTimeZoneRuleDate>> _zoneDates;

        struct ZoneRuleAssociate
        {
            public TzTimeZoneRule zoneRule;
            public TimeSpan standardOffset;
            public string Letter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TzTimeZone"/> class.  
        /// </summary>
        internal TzTimeZone()
        {
            _zoneRules = new List<TzTimeZoneRule>();
            _zoneDates = new Dictionary<int, List<TzTimeZoneRuleDate>>();
        }

        /// <summary>
        /// Gets the time zone name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the time zone filename.
        /// </summary>
        public string Filename { get; internal set; }

        /// <summary>
        /// Gets the time zone coordinates.
        /// </summary>
        /// <remarks>Coordinates are latitude and longitude of the zone's principal location.</remarks>
        public string Coordinates { get; internal set; }

        /// <summary>
        /// Gets the time zone comment.
        /// </summary>
        public string Comment { get; internal set; }

        /// <summary>
        /// Gets the the time zone line number in the file.
        /// </summary>
        public int LineNumber { get; internal set; }

        /// <summary>
        /// Gets the time zone rules.
        /// </summary>
        internal List<TzTimeZoneRule> ZoneRules { get { return _zoneRules; } }

        /// <summary>
        /// Gets the time zone of the current computer.
        /// </summary>
        public static TzTimeZone CurrentTzTimeZone
        {
            get
            {
                return TzTimeInfo.FindSystemTzTimeZoneById(TimeZoneInfo.Local.Id);
            }
        }

        /// <summary>
        /// Obtient les dates de changements de l'année spécifiée
        /// </summary>
        /// <param name="year">Année de recherche</param>
        /// <returns><see cref="DateTime">DateTime[]</see> des changements qui surviennent dans l'année spécifié.</returns>
        internal DateTime[] GetDayChangeTime(int year)
        {
            List<DateTime> dates = new List<DateTime>();

            for (int index = ZoneRules.Count - 1; index >= 0; index--)
            {
                TzTimeZoneRule temp = ZoneRules[index];
                DateTime startTime = temp.StartZone.ToLocalTime();
                DateTime endTime = temp.EndZone.ToLocalTime();

                if (startTime.Year <= year && endTime.Year >= year)
                {
                    if (TzTimeInfo.Rules.ContainsKey(temp.RuleName))
                    {
                        List<Rule> rules = TzTimeInfo.Rules[temp.RuleName];

                        foreach (Rule rule in rules)
                        {
                            if (rule.AtKind == TimeKind.LocalWallTime)
                            {
                                // La règle est applicable dans le TimeKind recherché, on compare les années directement
                                if (rule.LowerYear <= year && rule.HighYear >= year && TzUtilities.IsYearType(year, rule.YearType))
                                {
                                    DateTime date = TzUtilities.GetDateTime(rule, year, temp.GmtOffset, TimeSpan.Zero, DateTimeKind.Local);
                                    if (date >= startTime && date <= endTime && !dates.Contains(date)) dates.Add(date);
                                }
                            }
                            else
                            {
                                // La règle doit être convertie en local, l'application de GMT+DST peut faire en sorte
                                // qu'on change d'année entre utc et local
                                if (rule.LowerYear <= year && rule.HighYear >= year && TzUtilities.IsYearType(year, rule.YearType))
                                {
                                    TimeSpan stdoff = GetLastSaveOffset(year, rules, rule, temp.StartZone, temp.GmtOffset);
                                    DateTime date = TzUtilities.GetDateTime(rule, year, temp.GmtOffset, stdoff, DateTimeKind.Local);
                                    if (date.Year == year && date >= startTime && date <= endTime && !dates.Contains(date))
                                    {
                                        dates.Add(date);
                                        continue;
                                    }
                                }

                                // La règle n'est pas applicable en prenant l'année courante mais peut le devenir si
                                // on est à cheval sur deux années. Année N-1 et règle en décembre ou année N+1 et règle en Janvier
#pragma warning disable CA1814 // Préférer les tableaux en escalier aux tableaux multidimensionnels
                                int[,] yrefs = { { year - 1, 12 }, { year + 1, 1 } };
#pragma warning restore CA1814 // Préférer les tableaux en escalier aux tableaux multidimensionnels
                                for (int y = 0; y < yrefs.GetLength(0); y++)
                                {
                                    if (rule.LowerYear <= yrefs[y, 0] && rule.HighYear >= yrefs[y, 0] && rule.Month == yrefs[y, 1] && TzUtilities.IsYearType(yrefs[y, 0], rule.YearType))
                                    {
                                        TimeSpan stdoff = GetLastSaveOffset(yrefs[y, 0], rules, rule, temp.StartZone, temp.GmtOffset);
                                        DateTime date = TzUtilities.GetDateTime(rule, yrefs[y, 0], temp.GmtOffset, stdoff, DateTimeKind.Local);
                                        if (date.Year == year && date >= startTime && date <= endTime && !dates.Contains(date))
                                        {
                                            dates.Add(date);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dates.ToArray();
        }

        //public bool IsAmbiguousTime(DateTime dateTime) {
        //    if (dateTime.Kind == DateTimeKind.Utc) return false;
        //    if (dateTime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", "datetime");

        //    ZoneRuleAssociate zr = GetZoneRule(dateTime);
        //    Debug.Assert(zr.zoneRule != null);

        //    return false;
        //}

        /// <summary>
        /// Obtient pour une année donnée l'ensemble des dates de changement avec leur gmt et standard offset
        /// applicable avant cette date.
        /// </summary>
        /// <param name="year"></param>
        private void UpdateDateChange(int year)
        {
            lock (_zoneDates)
            {
                if (!_zoneDates.ContainsKey(year))
                {
                    Dictionary<DateTime, TzTimeZoneRuleDate> dico = new Dictionary<DateTime, TzTimeZoneRuleDate>();

                    _zoneDates.Add(year, new List<TzTimeZoneRuleDate>());

                    for (int index = ZoneRules.Count - 1; index >= 0; index--)
                    {
                        TzTimeZoneRule temp = ZoneRules[index];

                        if ((temp.StartZone.UtcDate.Year <= year && temp.EndZone.UtcDate.Year >= year) ||
                            (temp.StartZone.ToLocalTime().Year <= year && temp.EndZone.ToLocalTime().Year >= year))
                        {
                            if (TzTimeInfo.Rules.ContainsKey(temp.RuleName))
                            {
                                List<Rule> rules = TzTimeInfo.Rules[temp.RuleName];

                                foreach (Rule rule in rules)
                                {
                                    // On calcule sur trois années pour couvrir les changements d'heure de fin d'année et début d'année
                                    for (int yearRef = year - 1; yearRef <= year + 1; yearRef++)
                                    {
                                        if (rule.LowerYear <= yearRef && rule.HighYear >= yearRef && TzUtilities.IsYearType(yearRef, rule.YearType))
                                        {
                                            // Standard offset applicable à cette règle
                                            TimeSpan stdoff = GetLastSaveOffset(yearRef, rules, rule, temp.StartZone, temp.GmtOffset);
                                            // Date UTC et Local d'application de cette règle
                                            DateTime utc = TzUtilities.GetDateTime(rule, yearRef, temp.GmtOffset, stdoff, DateTimeKind.Utc);
                                            DateTime local = TzUtilities.GetDateTime(rule, yearRef, temp.GmtOffset, stdoff, DateTimeKind.Local);

                                            // Contient les dates de la règle avec le décalage gmt et standard applicable AVANT cette date
                                            TzTimeZoneRuleDate ruleDate = new TzTimeZoneRuleDate(utc, local, temp.GmtOffset, stdoff);
                                            if (ruleDate > temp.StartZone && ruleDate <= temp.EndZone)
                                            {
                                                if (!dico.ContainsKey(ruleDate.UtcDate))
                                                    dico.Add(ruleDate.UtcDate, ruleDate);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // On ajoute les changements induits par les date until de zone
                    for (int index = ZoneRules.Count - 1; index >= 0; index--)
                    {
                        TzTimeZoneRule temp = ZoneRules[index];

                        if (temp.EndZone.UtcDate.Year == year || temp.EndZone.ToLocalTime().Year == year)
                        {
                            if (!dico.ContainsKey(temp.EndZone.UtcDate))
                                dico.Add(temp.EndZone.UtcDate, temp.EndZone);
                        }
                    }

                    _zoneDates[year].AddRange(dico.Values.OrderBy(e => e.UtcDate));
                }
            }
        }

        /// <summary>
        /// Returns the local time that corresponds to a specified date and time value. 
        /// </summary>
        /// <param name="datetime">A date and time.</param>
        /// <param name="optimize">Value which indicates whether optimize the convert</param>
        /// <returns>A <see cref="System.DateTime"/> object whose value is the local time that corresponds to time.</returns>
        public DateTime ToLocalTime(DateTime datetime, bool optimize = false)
        {
            if (datetime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(datetime));

            if (datetime.Kind == DateTimeKind.Local) return datetime;

            if (optimize)
            {
                UpdateDateChange(datetime.Year);
                List<TzTimeZoneRuleDate> knownDate = _zoneDates[datetime.Year];
                if (knownDate.Any())
                {
                    foreach (var elt in knownDate)
                    {
                        if ((datetime < elt.UtcDate && datetime.Kind == DateTimeKind.Utc) ||
                            (datetime < elt.ToLocalTime() && datetime.Kind == DateTimeKind.Local))
                            return TzUtilities.GetDateTime(datetime, elt.GmtOffset, elt.StandardOffset, DateTimeKind.Local);
                    }
                }
            }

            ZoneRuleAssociate zr = GetZoneRule(datetime);
            Debug.Assert(zr.zoneRule != null);

            TimeSpan gmtOffset = zr.zoneRule.GmtOffset;

            DateTime wallclock = datetime.Add(gmtOffset + zr.standardOffset);
            return new DateTime(wallclock.Ticks, DateTimeKind.Local);
        }

        /// <summary>
        /// Returns the Coordinated Universal Time (UTC) that corresponds to a specified time.
        /// </summary>
        /// <param name="datetime">A date and time.</param>
        /// <param name="optimize">Value which indicates whether to optimize the convert</param>
        /// <returns>A <see cref="DateTime"/> object whose value is the Coordinated Universal Time (UTC) that corresponds to time.</returns>
        public DateTime ToUniversalTime(DateTime datetime, bool optimize = false)
        {
            if (datetime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(datetime));

            if (datetime.Kind == DateTimeKind.Utc) return datetime;

            // Check that local date is cover by zone
            if (!ZoneRules.Any(z => datetime >= z.StartZone.ToLocalTime() && datetime < z.EndZone.ToLocalTime()))
            {
                throw new ArgumentOutOfRangeException(nameof(datetime));
            }

            if (optimize)
            {
                UpdateDateChange(datetime.Year);
                List<TzTimeZoneRuleDate> knownDate = _zoneDates[datetime.Year];
                if (knownDate.Any())
                {
                    foreach (var elt in knownDate)
                    {
                        if ((datetime.Kind == DateTimeKind.Utc && datetime < elt.UtcDate) ||
                            (datetime.Kind == DateTimeKind.Local && datetime < elt.ToLocalTime()))
                            return TzUtilities.GetDateTime(datetime, elt.GmtOffset, elt.StandardOffset, DateTimeKind.Utc);
                    }
                }
            }

            ZoneRuleAssociate zr = GetZoneRule(datetime);
            Debug.Assert(zr.zoneRule != null);

            TimeSpan gmtOffset = zr.zoneRule.GmtOffset;

            DateTime utcclock = datetime.Add(-gmtOffset - zr.standardOffset);
            return new DateTime(utcclock.Ticks, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns the local time in the specified time zone that correspond to the specified date in the current time zone.
        /// </summary>
        /// <param name="datetime">A date and time in the current time zone</param>
        /// <param name="zone">The time zone in which the local time is converted.</param>
        /// <returns>A local <see cref="DateTime"/> in the specified time zone.</returns>
        public DateTime ToTimeZone(DateTime datetime, TzTimeZone zone)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));
            if (datetime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(datetime));

            if (datetime.Kind == DateTimeKind.Utc) return datetime;

            DateTime utc = this.ToUniversalTime(datetime);
            return zone.ToLocalTime(utc);
        }

        /// <summary>
        /// Returns a string that represents the specified <see cref="DateTime"/> with the specified format.
        /// </summary>
        /// <param name="format">String that represents the date and time format.</param>
        /// <param name="datetime">A date and time.</param>
        /// <returns>A string that represents the <see cref="DateTime"/> in the specified format.</returns>
        public string ToString(string format, DateTime datetime)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (datetime == null) throw new ArgumentNullException(nameof(datetime));

            if (datetime.Kind == DateTimeKind.Unspecified) return datetime.ToString(format, CultureInfo.CurrentCulture);

            if (format == "o" || format == "O")
                format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK";
            else if (format == "u")
                format = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
            else if (format == "U" && datetime.Kind == DateTimeKind.Local)
                datetime = ToUniversalTime(datetime);

            if (format.Contains("K") || format.Contains("zzz") || format.Contains("#F"))
            {
                ZoneRuleAssociate zr = GetZoneRule(datetime);
                Debug.Assert(zr.zoneRule != null);

                TimeSpan eps = zr.zoneRule.GmtOffset + zr.standardOffset;
                string epss = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", eps.Hours, eps.Minutes);
                if (eps >= TimeSpan.Zero) epss = "+" + epss;

                string result = format.Replace("#F", string.Format(CultureInfo.InvariantCulture, zr.zoneRule.Format.Replace("%s", "{0}"), (zr.Letter == "-") ? "" : zr.Letter));
                if (datetime.Kind == DateTimeKind.Utc)
                    result = result.Replace("K", "Z").Replace("zzz", "Z");
                else
                    result = result.Replace("K", epss).Replace("zzz", epss);

                return datetime.ToString(result, CultureInfo.CurrentCulture);
            }

            return datetime.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Obtient la description de zone correspondant à la date spécifiée
        /// </summary>
        /// <param name="point"><see cref="DateTime"/> spécifié.</param>
        /// <returns><see cref="ZoneRuleAssociate"/> contenant le <see cref="DateTime"/> spécifié</returns>
        private ZoneRuleAssociate GetZoneRule(DateTime point)
        {
            if (point.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(point));

            ZoneRuleAssociate za = new ZoneRuleAssociate();
            za.standardOffset = TimeSpan.Zero;

            TimeSpan stdoff = TimeSpan.Zero;

            bool utc = (point.Kind == DateTimeKind.Utc);

            // Traduction de la date en règle
            TzTimeZoneRuleUntil rulePoint = new TzTimeZoneRuleUntil();
            rulePoint.Year = point.Year;
            rulePoint.At = point.TimeOfDay;
            rulePoint.AtKind = (utc) ? TimeKind.UniversalTime : TimeKind.LocalWallTime;
            rulePoint.Month = point.Month;
            rulePoint.Day = new DayOfRule(DayWorkKind.Dom, point.DayOfWeek, point.Day);

            for (int index = ZoneRules.Count - 1; index >= 0; index--)
            {
                TzTimeZoneRule temp = ZoneRules[index];

                DateTime startTime = (utc) ? temp.StartZone.UtcDate : temp.StartZone.ToLocalTime();
                DateTime endTime = (utc) ? temp.EndZone.UtcDate : temp.EndZone.ToLocalTime();
                stdoff = temp.StartZone.StandardOffset;

                if (point >= startTime && point < endTime)
                {
                    if (!TzTimeInfo.Rules.ContainsKey(temp.RuleName))
                    {
                        // Max time
                        za.zoneRule = temp;
                        za.standardOffset = temp.FixedStandardOffset;
                        break;
                    }
                    else
                    {
                        // Trouver la dernière règle applicable au point
                        Rule lastRule = GetLastStandardOffset(TzTimeInfo.Rules[temp.RuleName], rulePoint, temp.StartZone, temp.GmtOffset, RuleSearchKind.LessThanOrEqual);

                        za.zoneRule = temp;
                        za.standardOffset = (lastRule == null) ? stdoff : lastRule.StandardOffset;
                        za.Letter = (lastRule == null) ? string.Empty : lastRule.Letter;
                        break;
                    }
                }
            }
            return za;
        }

        /// <summary>
        /// Obtient la règle applicable à un point
        /// </summary>
        /// <param name="rules">Ensemble des règles à parcourir</param>
        /// <param name="date"><see cref="TzTimeZoneRuleUntil"/> représentant la date du point</param>
        /// <param name="startZone">Limite inférieure de validité des règles</param>
        /// <param name="gmtOffset"></param>
        /// <param name="ruleSearch">Indique si on recherche une régle inférieure ou égal au point</param>
        /// <returns></returns>
        internal static Rule GetLastStandardOffset(List<Rule> rules, TzTimeZoneRuleUntil date, TzTimeZoneRuleDate startZone, TimeSpan gmtOffset,
            RuleSearchKind ruleSearch)
        {
            Rule lastRule = null;
            TimeSpan range = TimeSpan.MaxValue;

            DateTime ruleTime;

            // En fonction du kind de la date Until on compare des dates UTC ou des dates locales
            DateTimeKind kind = (date.AtKind == TimeKind.LocalWallTime) ? DateTimeKind.Local : DateTimeKind.Utc;
            DateTime startTimeZone = (kind == DateTimeKind.Local) ? startZone.ToLocalTime() : startZone.UtcDate;

            // On pré-calcule la date de la règle dans le type permettant l'absence du daylight saving time
            // Une date en LocalWallTime sera exprimée en local
            // Une date en LocalStandardTime sera exprimée en UTC
            // Une date en UTC sera exprimée en UTC
            DateTime dateTime = TzUtilities.GetDateTime(date, date.Year, gmtOffset, TimeSpan.Zero, kind);

            int bestYear = -1;
            int yref = 0;

            // On parcourt toutes les règles car rien ne nous assure quelles sont dans un ordre spécifique
            for (int i = rules.Count - 1; i >= 0; i--)
            {
                Rule rule = rules[i];

                // Les années ne pouvant pas rivaliser avec la meilleure année obtenue sont ignorées
                if (rule.HighYear < bestYear) continue;

                // Certaines règles sont déclenchées le 1er janvier de l'année, avec le décalage
                // GMT on peut se retrouver dans le cas LowerYear = date.Year+1.
                // Exemple : je cherche la règle applicable le 31 décembre 2010 23:00 local, si on est sur une règle
                // applicable le 1 janvier 2011 utc alors le décalage gmt peut faire en sorte que la règle soit retenue
                if (rule.LowerYear == date.Year + 1 && rule.Month == 1 && date.Month == 12 && rule.AtKind != date.AtKind)
                {
                    yref = rule.LowerYear;
                }
                else
                {
                    // Année de début supérieure au point recherché, la règle n'est pas applicable
                    if (rule.LowerYear > date.Year) continue;

                    // On prend l'année qui convient le mieux au point
                    yref = (rule.HighYear > date.Year) ? date.Year : rule.HighYear;

                    // Cas particulier des changements à cheval sur deux années
                    // Si les dates à comparer ne sont pas de même kind la règle applicable en année N
                    // peut se retrouver applicable en N-1
                    if (yref == date.Year && yref < rule.HighYear && date.AtKind != rule.AtKind)
                    {
                        if (rule.Month == 1 && date.Month == 12) yref++;
                    }

                    // Au pire il faudra calculer la date pour l'année désirée et l'année précédente
                    // Par exemple si on recherche la dernière règle applicable au mois de janvier il
                    // faudra remonter au mois x de l'année précédente
                    // Exmple : on a une règle définie pour le mois de mars de 2000 à 2050, on recherche
                    // pour un point en janvier 2010. La date précédemment retenue est 2010, mais comme 
                    // janvier < mars si on reste en 2010 elle sera exclue. Il faut démarrer en 2009 pour
                    // cette règle

                    // On économise l'examen de l'année yref si la règle est applicable après le point recherché
                    // Seul une comparaison de même kind peut être effectuée.
                    if (yref == date.Year && rule.AtKind == date.AtKind &&
                        (rule.Month > date.Month ||
                          (rule.Month == date.Month && rule.Day.DayWork == DayWorkKind.Dom && date.Day.DayWork == DayWorkKind.Dom && rule.Day.DayOfMonth > date.Day.DayOfMonth))
                        )
                    {
                        if (yref > rule.LowerYear)
                            yref--;
                        else
                            continue;
                    }
                }

                for (; yref >= rule.LowerYear; yref--)
                {

                    if (!TzUtilities.IsYearType(yref, rule.YearType)) continue;

                    // On est obligé de rechercher la règle précédente si pour comparer les dates
                    // on a besoin du daylight saving time
                    if ((rule.AtKind == TimeKind.LocalWallTime && kind == DateTimeKind.Utc) ||
                        (rule.AtKind != TimeKind.LocalWallTime && kind == DateTimeKind.Local))
                    {
                        TimeSpan stdoff = GetLastSaveOffset(yref, rules, rule, startZone, gmtOffset);

                        ruleTime = TzUtilities.GetDateTime(rule, yref, gmtOffset, stdoff, kind);
                    }
                    else
                    {
                        // Si les deux règles sont en locales ou utc/standardoffset on peut les comparer directement
                        ruleTime = TzUtilities.GetDateTime(rule, yref, gmtOffset, TimeSpan.Zero, kind);
                    }

                    Debug.Assert(ruleTime.Kind == dateTime.Kind);

                    // Si la date d'application de la règle est supérieure au point recherché alors
                    // on recherche pour l'année précédente si on n'a pas atteint la borne inférieure
                    if (ruleTime > dateTime && ruleTime.Year > rule.LowerYear)
                    {
                        continue;
                    }

                    // Si la règle est comprise dans les bornes [startTimeZone, dateTime] ou [startTimeZone, dateTime[ alors elle
                    // est applicable, on la retient si elle est meilleure que celle déjà retenue.
                    if (ruleTime >= startTimeZone &&
                        ((ruleTime <= dateTime && ruleSearch == RuleSearchKind.LessThanOrEqual) ||
                        (ruleTime < dateTime && ruleSearch == RuleSearchKind.LessThan)))
                    {
                        TimeSpan diff = dateTime - ruleTime;
                        if (range > diff)
                        {
                            lastRule = rule;
                            range = diff;
                            bestYear = ruleTime.Year;
                        }
                    }
                    break;
                }
            }

            return lastRule;
        }

        /// <summary>
        /// Obtient une règle correspond à la date spécifiée
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="date"></param>
        /// <param name="gmtOffset"></param>
        /// <param name="standardOffset"></param>
        /// <returns></returns>
        internal static Rule GetRuleAtPoint(List<Rule> rules, DateTime date, TimeSpan gmtOffset, TimeSpan standardOffset)
        {
            if (date.Kind != DateTimeKind.Utc) throw new ArgumentNullException(nameof(date));

            DateTime ruleTime = DateTime.MinValue;
            int yref = 0;

            // On parcourt toutes les règles car rien ne nous assure quelles sont dans un ordre spécifique
            for (int i = rules.Count - 1; i >= 0; i--)
            {
                Rule rule = rules[i];

                // Certaines règles sont déclenchées le 1er janvier de l'année, avec le décalage
                // GMT on peut se retrouver dans le cas LowerYear = date.Year+1.
                // Exemple : je cherche la règle applicable le 31 décembre 2010 23:00 utc, si on est sur une règle
                // applicable le 1 janvier 2011 local alors le décalage gmt peut faire en sorte que la règle soit retenue
                if (rule.LowerYear == date.Year + 1 && rule.Month == 1 && date.Month == 12 && rule.AtKind != TimeKind.UniversalTime)
                {
                    yref = rule.LowerYear;
                }
                else
                {
                    // Année de début supérieure au point recherché, la règle n'est pas applicable
                    if (rule.LowerYear > date.Year) continue;

                    yref = (rule.HighYear > date.Year) ? date.Year : rule.HighYear;

                    // Cas particulier des changements à cheval sur deux années
                    // Si les dates à comparer ne sont pas de même kind la règle applicable en année N
                    // peut se retrouver applicable en N-1
                    if (yref < rule.HighYear && rule.AtKind != TimeKind.UniversalTime)
                    {
                        if (rule.Month == 1 && date.Month == 12) yref++;
                    }

                    // On recherche un changement d'heure à la date spécifiée, on peut donc éliminer les années outscope
                    if (yref < date.Year - 1) continue;
                }

                for (; yref >= rule.LowerYear; yref--)
                {
                    if (!TzUtilities.IsYearType(yref, rule.YearType)) continue;

                    // Pas la peine de rechercher une année antérieure au point
                    if (yref < date.Year - 1) break;

                    // On compare toujours en UTC
                    ruleTime = TzUtilities.GetDateTime(rule, yref, gmtOffset, standardOffset, DateTimeKind.Utc);

                    // Si la date d'application de la règle est égale au point recherché alors on retourne la règle
                    if (ruleTime == date)
                    {
                        return rule;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Obtient le standard offset applicable à une règle
        /// </summary>
        /// <param name="year"></param>
        /// <param name="rules"></param>
        /// <param name="rule"></param>
        /// <param name="zoneDate"></param>
        /// <param name="gmtOffset"></param>
        /// <returns></returns>
        /// <remarks>Cette fonction compare des règles définies, comme un changement d'heure ne se
        /// produit pas en même temps qu'un autre on peut appliquer le stdoff entre règles sans
        /// se soucier de point recouvrant.
        /// </remarks>
        private static TimeSpan GetLastSaveOffset(int year, List<Rule> rules, Rule rule, TzTimeZoneRuleDate zoneDate, TimeSpan gmtOffset)
        {
            int bestyear = -1;

            TimeSpan range = TimeSpan.MaxValue;
            TimeSpan stdoff = zoneDate.StandardOffset;

            for (int index = rules.Count - 1; index >= 0; index--)
            {
                if (rules[index] == rule) continue;

                if (rules[index].LowerYear > year || rules[index].HighYear < bestyear)
                {
                    continue;
                }

                int yref = (rules[index].HighYear > year) ? year : rules[index].HighYear;

                if (yref == year && rules[index].AtKind == rule.AtKind &&
                    (rules[index].Month > rule.Month || (rules[index].Month == rule.Month && rules[index].Day.DayWork == DayWorkKind.Dom && rule.Day.DayWork == DayWorkKind.Dom && rules[index].Day.DayOfMonth > rule.Day.DayOfMonth)))
                {
                    if (yref > rules[index].LowerYear)
                        yref--;
                    else
                        continue;
                }

                for (; yref >= rules[index].LowerYear; yref--)
                {
                    if (!TzUtilities.IsYearType(yref, rules[index].YearType)) continue;

                    DateTime precedent;
                    DateTime current;

                    // On compare la date avec la date limite de zone
                    if (rules[index].AtKind == TimeKind.LocalWallTime)
                    {
                        precedent = TzUtilities.GetDateTime(rules[index], yref, gmtOffset, TimeSpan.Zero, DateTimeKind.Local);
                        if (zoneDate.ToLocalTime() > precedent)
                            break;
                        // On exprime la date courante de la règle à vérifier en local
                        current = TzUtilities.GetDateTime(rule, year, gmtOffset, rules[index].StandardOffset, DateTimeKind.Local);
                    }
                    else
                    {
                        precedent = TzUtilities.GetDateTime(rules[index], yref, gmtOffset, TimeSpan.Zero, DateTimeKind.Utc);
                        if (zoneDate.UtcDate > precedent)
                            break;
                        // On exprime la date courante de la règle à vérifier en utc
                        current = TzUtilities.GetDateTime(rule, year, gmtOffset, rules[index].StandardOffset, DateTimeKind.Utc);
                    }

                    if (precedent > current)
                    {
                        continue;
                    }
                    else
                    {
                        TimeSpan diff = current - precedent;
                        if (range > diff)
                        {
                            stdoff = rules[index].StandardOffset;
                            range = diff;
                            bestyear = precedent.Year;
                        }
                        break;
                    }
                }
            }

            return stdoff;
        }

        #region UnixTime conversion
        // Gets form DateTimeOffset
        private const long UnixEpochTicks = 621355968000000000;
        private const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800
        private const long UnixEpochMilliseconds = UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <returns></returns>
        public long ToUnixTimeSeconds(DateTime datetime)
        {
            if (datetime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(datetime));
            DateTime d = (datetime.Kind == DateTimeKind.Utc) ? datetime : ToUniversalTime(datetime);

            long seconds = d.Ticks / TimeSpan.TicksPerSecond;
            return seconds - UnixEpochSeconds;
        }

        /// <summary>
        /// Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
        /// </summary>
        /// <returns></returns>
        public long ToUnixTimeMilliseconds(DateTime datetime)
        {
            if (datetime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", nameof(datetime));
            DateTime d = (datetime.Kind == DateTimeKind.Utc) ? datetime : ToUniversalTime(datetime);

            long milliseconds = d.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - UnixEpochMilliseconds;
        }
        #endregion

        /*
        /// <summary>
        /// Retrieves an array of <see cref="TzAdjustmentRule"/> objects that apply to the current <see cref="TzTimeZone"/> object.
        /// </summary>
        /// <returns></returns>
        public TzAdjustmentRule[] GetAdjustmentRules()
        {
            List<TzAdjustmentRule> adjustmentRules = new List<TzAdjustmentRule>();

            for (int index = 0; index < ZoneRules.Count; index++)
            {
                TzTimeZoneRule temp = ZoneRules[index];

                if (!TzTimeInfo.Rules.ContainsKey(temp.RuleName))
                {
                    // Max time
                    //za.zoneRule = temp;
                    //za.standardOffset = temp.FixedStandardOffset;
                    //break;
                }
                else
                {
                    // Trouver la dernière règle applicable au point
                    var rules = TzTimeInfo.Rules[temp.RuleName];

                    foreach (var rule in rules)
                    {
                        //rule.
                    }
                    //za.zoneRule = temp;
                    //za.standardOffset = (lastRule == null) ? stdoff : lastRule.StandardOffset;
                    //za.Letter = (lastRule == null) ? string.Empty : lastRule.Letter;
                    //break;
                }

                TzAdjustmentRule adjustment = new TzAdjustmentRule(temp.StartZone.ToLocalTime(),
                    temp.EndZone.ToLocalTime());
                adjustmentRules.Add(adjustment);
            }

            return adjustmentRules.ToArray();
        }
         * */
    }
}
