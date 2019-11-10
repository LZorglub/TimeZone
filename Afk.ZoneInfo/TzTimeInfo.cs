using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Provide access to time zone
    /// </summary>
    public abstract class TzTimeInfo
    {

        private const string LINK = "Link";
        private const string RULE = "Rule";
        private const string ZONE = "Zone";

        private static Dictionary<string, string> _links;
        private static List<Country> _countryCode;
        private static Dictionary<string, List<Rule>> _rules;
        private static Dictionary<string, TzTimeZone> _zones;
        private static List<KeyValuePair<string, string>> _mappingWindowsTZID;

        #region Constructeur
#pragma warning disable CA1810 // Initialisez les champs static de type référence inline
        /// <summary>
        /// Initializes a new instance of the <see cref="TzTimeInfo"/> class.
        /// </summary>
        static TzTimeInfo()
#pragma warning restore CA1810 // Initialisez les champs static de type référence inline
        {
            _links = new Dictionary<string, string>();
            _countryCode = new List<Country>();
            _rules = new Dictionary<string, List<Rule>>();
            _zones = new Dictionary<string, TzTimeZone>();
            _mappingWindowsTZID = new List<KeyValuePair<string, string>>();

            string tzdir = Environment.GetEnvironmentVariable("TZDIR");

            // Charge les ressources prédéfinies dans l'assembly
            if (string.IsNullOrEmpty(tzdir))
            {
                LoadRessources();
            }
            else
            {
                LoadDirectory(tzdir);
            }

            BuildZone();
        }
        #endregion

        /// <summary>
        /// Gets all the rules
        /// </summary>
        internal static Dictionary<string, List<Rule>> Rules
        {
            get { return _rules; }
        }

        /// <summary>
        /// Load resources
        /// </summary>
        private static void LoadRessources()
        {
            Assembly a = MultiPlatform.GetCurrentAssembly();


            string[] resnames = a.GetManifestResourceNames();

            foreach (string resource in resnames)
            {
                if (resource.StartsWith("Afk.ZoneInfo.data.", StringComparison.InvariantCulture))
                {
                    string[] parts = resource.Split(new char[] { '.' }, 4);
                    if (parts.Length == 4)
                    {
                        using (Stream stream = a.GetManifestResourceStream(resource))
                        {
                            using (TextReader reader = new StreamReader(stream))
                            {

                                if (string.IsNullOrEmpty(Path.GetExtension(parts[3])))
                                {
                                    LoadFile(reader, parts[3]);
                                }
                                else if (string.Compare(Path.GetFileName(parts[3]), "iso3166.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    LoadCountryCode(reader);
                                }
                                else if (string.Compare(Path.GetFileName(parts[3]), "zone1970.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    LoadZoneName(reader);
                                }
                                else if (string.Compare(Path.GetFileName(parts[3]), "windowsZones.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    LoadMappingZones(reader);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Charge la base olson définie dans le répertoire spécifié
        /// </summary>
        /// <param name="directory">Répertoire de la base olson</param>
        private static void LoadDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));

            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    using (TextReader reader = new StreamReader(fs))
                    {
                        if (string.IsNullOrEmpty(Path.GetExtension(file)))
                            LoadFile(reader, file);
                        else if (string.Compare(Path.GetFileName(file), "iso3166.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                            LoadCountryCode(reader);
                        else if (string.Compare(Path.GetFileName(file), "zone1970.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                            LoadZoneName(reader);
                        else if (string.Compare(Path.GetFileName(file), "windowsZones.tab", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            LoadMappingZones(reader);
                        }
                    }
                }
            }

            // Si le mapping windows/TZID n'est pas présent on utilise la ressource incorporée
            if (_mappingWindowsTZID.Count == 0)
            {
                using (Stream stream = MultiPlatform.GetCurrentAssembly().GetManifestResourceStream("Afk.ZoneInfo.data.windowsZones.tab"))
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        LoadMappingZones(reader);
                    }
                }
            }
        }

        #region Chargement de la database
        /// <summary>
        /// Charge un ensemble de zone/rule/link
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="filename"></param>
        private static void LoadFile(TextReader reader, string filename)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            string line = null;

            bool zoneContinuation = false;
            TzTimeZone lastZone = null;
            int index = 0;

            while ((line = reader.ReadLine()) != null)
            {
                index++;

                if (string.IsNullOrEmpty(line)) continue;

                List<string> cutline = TzUtilities.GetFields(line, -1);

                if (cutline == null || cutline.Count == 0)
                    continue;
                else
                {
                    if (zoneContinuation)
                    {
                        zoneContinuation = TzUtilities.GetZone(cutline, ref lastZone, true);
                    }
                    else
                    {
                        switch (cutline[0])
                        {
                            case RULE:
                                Debug.Assert(cutline.Count == 10);
                                Rule r = TzUtilities.GetRule(cutline);
                                //if (!filename.StartsWith("solar") && r.StandardOffset != TimeSpan.Zero && r.StandardOffset != TimeSpan.FromHours(1))
                                //    Console.WriteLine(r.ToString());

                                r.Filename = filename; r.LineNumber = index;
                                if (!_rules.ContainsKey(r.Name))
                                    _rules.Add(r.Name, new List<Rule>());
                                _rules[r.Name].Add(r);

                                break;
                            case LINK:
                                Debug.Assert(cutline.Count == 3);
                                _links.Add(cutline[2], cutline[1]);
                                break;
                            case ZONE:
                                TzTimeZone z = new TzTimeZone();
                                z.Filename = filename; z.LineNumber = index;
                                zoneContinuation = TzUtilities.GetZone(cutline, ref z, false);
                                _zones.Add(z.Name, z);
                                lastZone = z;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Charge le code de chaque région
        /// </summary>
        /// <param name="reader"></param>
        private static void LoadCountryCode(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cutline = TzUtilities.GetFields(line, 2);

                if (cutline == null || cutline.Count != 2)
                    continue;
                else
                {
                    var country = _countryCode.FirstOrDefault(e => e.Code == cutline[0]);
                    if (country == null)
                        _countryCode.Add(new Country(cutline[0]) { Name = cutline[1] });
                    else
                        country.Name = cutline[1];
                }
            }
        }

        /// <summary>
        /// Charge la définition des régions
        /// </summary>
        /// <param name="reader"></param>
        private static void LoadZoneName(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cutline = TzUtilities.GetFields(line, 4);

                if (cutline == null || cutline.Count < 3)
                    continue;
                else
                {
                    string[] countriesCode = cutline[0].Split(',');
                    foreach (string countryCode in countriesCode)
                    {
                        var country = _countryCode.FirstOrDefault(e => e.Code == countryCode);
                        if (country == null)
                        {
                            country = new Country(countryCode);
                            _countryCode.Add(country);
                        }
                        country.AddTZ(cutline[2], cutline[1], (cutline.Count > 3) ? cutline[3] : string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Charge la table de mappage entre les zone windows et tzid
        /// </summary>
        /// <param name="reader"></param>
        private static void LoadMappingZones(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line)) continue;

                List<string> cutline = TzUtilities.GetFields(line, 2);

                if (cutline == null || cutline.Count < 2)
                    continue;
                else
                {
                    _mappingWindowsTZID.Add(new KeyValuePair<string, string>(cutline[1], cutline[0]));
                }
            }
        }
        #endregion

        /// <summary>
        /// Construit l'ensemble des règles chargées
        /// </summary>
        /// <remarks>
        /// La date de début d'une zone ne correspond pas forcement à la date de fin de zone précédente. Exemple avec Pacific/Samoa ou
        /// on perd 1 journée le 30 décembre 2011
        /// Zone Pacific/Apia	 12:33:04 -	LMT	1892 Jul  5
		///	-11:26:56 -	LMT	1911
		///	-11:30	-	-1130	1950
		///	-11:00	WS	-11/-10	2011 Dec 29 24:00
		///	 13:00	WS	+13/+14
        ///	 
        /// Certaines dates de fin peuvent coincider avec des règles de changement
        /// Exemple : America/Grand_Turk
        /// -5:00	US	E%sT	2015 Nov Sun>=1 2:00 coincide avec la régle Rule	US	2007	max	-	Nov	Sun>=1	2:00	0	S
        /// -4:00	-	AST	2018 Mar 11 3:00  
        /// 1/11/2015 01:00 EDT (sunday locale) => 05:00u
        /// 1/11/2015 02:00 AST (sunday locale) => 06:00u
        /// 
        /// -4:00	-	AST	2018 Mar 11 3:00 coincide avec la régle Rule	US	2007	max	-	Mar	Sun>=8	2:00 (07:00 +5)	1:00	D
        /// -5:00	US	E%sT
        /// 11/03/2018 02:00 AST => 06:00u
        /// 11/03/2018 03:00 EDT => 07:00u
        /// 11/03/2018 04:00 EDT => 08:00u
        /// 
        /// La date de début UTC de la zone suivante est la date de fin UTC de la zone précédente, ce n'est pas vrai pour ce qui concerne les
        /// dates locales. Exemple de samoa qui avance de 24 heures.
        /// 
        /// Date de début : On construit la date à partir de la date utc précédente et on applique la régle courante pour obtenir la date locale
        /// Date de fin : On construit la date à partir de la date until en appliquant le changement DST précédent strictement inférieure
        /// </remarks>
        private static void BuildZone()
        {
            var ienum = _zones.GetEnumerator();

            while (ienum.MoveNext())
            {
                TzTimeZone temp = ienum.Current.Value;

                TimeSpan stdoff = TimeSpan.Zero;

                // Coordonnées de la zone
                var tz = _countryCode.SelectMany(e => e.TZone).FirstOrDefault(e => e.TZName == temp.Name);
                temp.Coordinates = tz.coordinates;
                temp.Comment = tz.comment;

                for (int index = 0; index < temp.ZoneRules.Count; index++)
                {
                    TzTimeZoneRule zr = temp.ZoneRules[index];

                    // Zone avec un décalage fixe
                    if (zr.RuleName != "-" && !_rules.ContainsKey(zr.RuleName))
                    {
                        zr.FixedStandardOffset = TzUtilities.GetHMS(zr.RuleName);

                        // Dans ce cas précis le formatage % n'est pas autorisé
                        if (zr.Format.Contains("%"))
                            throw new ArgumentException("%s in ruleless zone " + temp.Name + "(" + temp.Filename + "," + temp.LineNumber + ")");
                    }

                    #region Start date
                    if (index == 0)
                    {
                        zr.StartZone = TzTimeZoneRuleDate.MinValue;
                    }
                    else
                    {
                        DateTime previousUTCDate = temp.ZoneRules[index - 1].EndZone.UtcDate;
                        TimeSpan previousStandardOffset = temp.ZoneRules[index - 1].EndZone.StandardOffset;

                        if (zr.RuleName == "-")
                        {
                            zr.StartZone = new TzTimeZoneRuleDate(previousUTCDate, zr.GmtOffset, TimeSpan.Zero);
                        }
                        else if (!_rules.ContainsKey(zr.RuleName))
                        {
                            zr.StartZone = new TzTimeZoneRuleDate(previousUTCDate, zr.GmtOffset, zr.FixedStandardOffset);
                        }
                        else
                        {
                            // Il faut rechercher si pour la date UTC on aurait une régle qui s'applique
                            Rule lastRule = TzTimeZone.GetRuleAtPoint(_rules[zr.RuleName], previousUTCDate, zr.GmtOffset, previousStandardOffset);
                            zr.StartZone = new TzTimeZoneRuleDate(previousUTCDate, zr.GmtOffset, lastRule?.StandardOffset ?? previousStandardOffset);
                        }
                    }
                    #endregion

                    #region End date
                    if (zr.Until == null)
                    {
                        zr.EndZone = TzTimeZoneRuleDate.MaxValue;
                    }
                    else if (zr.RuleName == "-")
                    {
                        stdoff = TimeSpan.Zero;
                        zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, stdoff, DateTimeKind.Utc), zr.GmtOffset, stdoff);
                    }
                    else if (!_rules.ContainsKey(zr.RuleName))
                    {
                        // Zone avec décalage fixe
                        stdoff = zr.FixedStandardOffset;
                        zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, stdoff, DateTimeKind.Utc), zr.GmtOffset, stdoff);
                    }
                    else
                    {
                        Rule lastRule = TzTimeZone.GetLastStandardOffset(_rules[zr.RuleName], zr.Until, zr.StartZone, zr.GmtOffset, RuleSearchKind.LessThan);
                        if (lastRule != null)
                            stdoff = lastRule.StandardOffset;
                        zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, stdoff, DateTimeKind.Utc), zr.GmtOffset, stdoff);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Gets the time zone name
        /// </summary>
        /// <returns><b>string[]</b> of time zone name.</returns>
        public static string[] GetZoneNames()
        {
            return _zones.Keys.OrderBy(e => e).ToArray();
        }

        /// <summary>
        /// Obtient le nom de tous les liens
        /// </summary>
        /// <returns>Tableau des liens</returns>
        internal static string[] GetBackward()
        {
            return _links.Keys.OrderBy(e => e).ToArray();
        }

        /// <summary>
        /// Gets the country code.
        /// </summary>
        /// <returns><b>string[]</b> of country code.</returns>
        public static string[] GetCountriesCode()
        {
            return _countryCode.Select(e => e.Code).OrderBy(e => e).ToArray();
        }

        /// <summary>
        /// Gets the country
        /// </summary>
        /// <param name="code">Country code</param>
        /// <returns>A <see cref="Afk.ZoneInfo.Country"/> object that represent the country.</returns>
        public static Country GetCountries(string code)
        {
            return _countryCode.FirstOrDefault(e => e.Code == code);
        }

        /// <summary>
        /// Gets the <see cref="TzTimeZone"/> that represents the time zone.
        /// </summary>
        /// <param name="zoneName">The time zone name, for sample "Europe/Paris"</param>
        /// <exception cref="ArgumentNullException">zoneName is null or empty.</exception>
        /// <returns>An <see cref="TzTimeZone"/> object whose name is the value of the zoneName parameter, null otherwise</returns>
        public static TzTimeZone GetZone(string zoneName)
        {
            if (string.IsNullOrEmpty(zoneName)) throw new ArgumentNullException(nameof(zoneName));

            if (_zones.ContainsKey(zoneName))
                return _zones[zoneName];
            else if (_links.ContainsKey(zoneName) && _zones.ContainsKey(_links[zoneName]))
                return _zones[_links[zoneName]];

            return null;
        }

        /// <summary>
        /// Gets the list of <see cref="TzTimeZone"/>.
        /// </summary>
        /// <returns>Array of <see cref="TzTimeZone"/>.</returns>
        public static TzTimeZone[] GetZones()
        {
            return _zones.Values.ToArray();
        }

        /// <summary>
        /// Retrieve a <see cref="TzTimeZone"/> object from its windows identifier
        /// </summary>
        /// <param name="id">The windows time zone identifier.</param>
        /// <exception cref="ArgumentException">id is null or empty.</exception>
        /// <returns>An <see cref="TzTimeZone"/> object whose windows identifier is the value of the id parameter, otherwise null.</returns>
        public static TzTimeZone FindSystemTzTimeZoneById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            foreach (var keys in _mappingWindowsTZID)
            {
                if (keys.Key == id)
                    return GetZone(keys.Value);
            }

            return null;
        }
    }
}