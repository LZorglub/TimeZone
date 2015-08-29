using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Afk.ZoneInfo {
	/// <summary>
	/// Provide access to time zone
	/// </summary>
	public abstract class TzTimeInfo {

		private const string LINK = "Link";
		private const string RULE = "Rule";
		private const string ZONE = "Zone";

		private static Dictionary<string, string> _links;
		private static List<Country> _countryCode;
		private static Dictionary<string, List<Rule>> _rules;
		private static Dictionary<string, TzTimeZone> _zones;
		private static List<KeyValuePair<string, string>> _mappingWindowsTZID;

		#region Constructeur
		/// <summary>
		/// Initializes a new instance of the <see cref="TzTimeInfo"/> class.
		/// </summary>
		static TzTimeInfo() {
			_links = new Dictionary<string, string>();
			_countryCode = new List<Country>();
			_rules = new Dictionary<string, List<Rule>>();
			_zones = new Dictionary<string, TzTimeZone>();
            _mappingWindowsTZID = new List<KeyValuePair<string, string>>();

			string tzdir = Environment.GetEnvironmentVariable("TZDIR");

			// Charge les ressources prédéfinies dans l'assembly
			if (string.IsNullOrEmpty(tzdir)) {
				LoadRessources();
			}
			else {
				LoadDirectory(tzdir);
			}

			BuildZone();
		}
		#endregion

		/// <summary>
		/// Gets all the rules
		/// </summary>
		internal static Dictionary<string, List<Rule>> Rules {
			get { return _rules; }
		}

		/// <summary>
		/// Load resources
		/// </summary>
		private static void LoadRessources() {
			Assembly a = Assembly.GetExecutingAssembly();

			string[] resnames = a.GetManifestResourceNames();

			foreach (string resource in resnames) {
				if (resource.StartsWith("Afk.ZoneInfo.data.")) {
					string[] parts = resource.Split(new char[] { '.' }, 4);
					if (parts.Length == 4) {
						using (Stream stream = a.GetManifestResourceStream(resource)) {
							using (TextReader reader = new StreamReader(stream)) {

								if (string.IsNullOrEmpty(Path.GetExtension(parts[3]))) {
									LoadFile(reader, parts[3]);
								}
								else if (string.Compare(Path.GetFileName(parts[3]), "iso3166.tab", true) == 0) {
									LoadCountryCode(reader);
								}
								else if (string.Compare(Path.GetFileName(parts[3]), "zone1970.tab", true) == 0) {
									LoadZoneName(reader);
								}
								else if (string.Compare(Path.GetFileName(parts[3]), "windowsZones.tab", true) == 0) {
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
		private static void LoadDirectory(string directory) {
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("directory");

			string[] files = Directory.GetFiles(directory);

			foreach (string file in files) {
				using (FileStream fs = new FileStream(file, FileMode.Open)) {
					using (TextReader reader = new StreamReader(fs)) {
						if (string.IsNullOrEmpty(Path.GetExtension(file)))
							LoadFile(reader, file);
						else if (string.Compare(Path.GetFileName(file), "iso3166.tab", true) == 0)
							LoadCountryCode(reader);
						else if (string.Compare(Path.GetFileName(file), "zone1970.tab", true) == 0)
							LoadZoneName(reader);
						else if (string.Compare(Path.GetFileName(file), "windowsZones.tab", true) == 0) {
							LoadMappingZones(reader);
						}
					}
				}
			}

			// Si le mapping windows/TZID n'est pas présent on utilise la ressource incorporée
			if (_mappingWindowsTZID.Count == 0) {
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Afk.ZoneInfo.data.windowsZones.tab")) {
					using (TextReader reader = new StreamReader(stream)) {
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
		private static void LoadFile(TextReader reader, string filename) {
			if (reader == null)
				throw new ArgumentNullException("reader");

			string line = null;

			bool zoneContinuation = false;
			TzTimeZone lastZone = null;
			int index = 0;

			while ((line = reader.ReadLine()) != null) {
				index++;

				if (string.IsNullOrEmpty(line)) continue;

				List<string> cutline = TzUtilities.GetFields(line, -1);

				if (cutline == null || cutline.Count == 0)
					continue;
				else {
					if (zoneContinuation) {
						zoneContinuation = TzUtilities.GetZone(cutline, ref lastZone, true);
					}
					else {
						switch (cutline[0]) {
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
		private static void LoadCountryCode(TextReader reader) {
			if (reader== null)
				throw new ArgumentNullException("reader");

			string line = null;

			while ((line = reader.ReadLine()) != null) {
				if (string.IsNullOrEmpty(line)) continue;

				List<string> cutline = TzUtilities.GetFields(line, 2);

				if (cutline == null || cutline.Count != 2)
					continue;
				else {
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
		private static void LoadZoneName(TextReader reader) {
			if (reader == null)
				throw new ArgumentNullException("reader");

			string line = null;

			while ((line = reader.ReadLine()) != null) {
				if (string.IsNullOrEmpty(line)) continue;

				List<string> cutline = TzUtilities.GetFields(line, 4);

				if (cutline == null || cutline.Count < 3)
					continue;
				else {
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
                throw new ArgumentNullException("reader");

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
		private static void BuildZone() {

			var ienum = _zones.GetEnumerator();

			while (ienum.MoveNext()) {
				TzTimeZone temp = ienum.Current.Value;

				TzTimeZoneRuleDate start = TzTimeZoneRuleDate.MinValue;
				TimeSpan stdoff = TimeSpan.Zero;

				// Coordonnées de la zone
				var tz = _countryCode.SelectMany(e => e.TZone).FirstOrDefault(e => e.TZName == temp.Name);
				temp.Coordinates = tz.coordinates;
				temp.Comment = tz.comment;
				
				// Pour chaque règle composant une zone on calcule précisement la date de début et de fin
				// d'application de la règle en UTC et en Local.
				foreach (TzTimeZoneRule zr in temp.ZoneRules) {
					// Si le nom de la règle n'est pas - et n'est pas connu alors il s'agit directement
					// du décalage standard à appliquer.
					if (zr.RuleName != "-" && !_rules.ContainsKey(zr.RuleName)) {
						zr.FixedStandardOffset = TzUtilities.GetHMS(zr.RuleName);

						// Dans ce cas précis le formatage % n'est pas autorisé
						if (zr.Format.Contains("%"))
							throw new ArgumentException("%s in ruleless zone " + temp.Name + "(" + temp.Filename + "," + temp.LineNumber + ")");
					}

					// La date de départ de la règle est celle de la règle précédente
					zr.StartZone = (TzTimeZoneRuleDate)start.Clone();

					// Calcul de la date de fin de zone rule
					if (zr.Until == null) {
						// Max time
						zr.EndZone = (TzTimeZoneRuleDate)TzTimeZoneRuleDate.MaxValue.Clone();
					}
					else if (zr.RuleName == "-") {
						stdoff = TimeSpan.Zero;
						zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, stdoff, DateTimeKind.Utc), zr.GmtOffset, stdoff);
					}
					else if (!_rules.ContainsKey(zr.RuleName)) {
						// Zone sans règle
						// Si la règle est - alors stdoff = TimeSpan.zero
						zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, zr.FixedStandardOffset, DateTimeKind.Utc), zr.GmtOffset, zr.FixedStandardOffset);
						stdoff = zr.FixedStandardOffset;
					}
					else {
						Rule lastRule = TzTimeZone.GetLastStandardOffset(_rules[zr.RuleName], zr.Until, zr.StartZone, zr.GmtOffset);
						if (lastRule != null)
							stdoff = lastRule.StandardOffset;
						zr.EndZone = new TzTimeZoneRuleDate(TzUtilities.GetDateTime(zr.Until, zr.Until.Year, zr.GmtOffset, stdoff, DateTimeKind.Utc), zr.GmtOffset, stdoff);
					}
					start = zr.EndZone;
				}
			}
		}

		/// <summary>
		/// Gets the time zone name
		/// </summary>
		/// <returns><b>string[]</b> of time zone name.</returns>
		public static string[] GetZoneNames() {
			return _zones.Keys.OrderBy(e => e).ToArray();
		}

		/// <summary>
		/// Obtient le nom de tous les liens
		/// </summary>
		/// <returns>Tableau des liens</returns>
		internal static string[] GetBackward() {
			return _links.Keys.OrderBy(e => e).ToArray();
		}

		/// <summary>
		/// Gets the country code.
		/// </summary>
		/// <returns><b>string[]</b> of country code.</returns>
		public static string[] GetCountriesCode() {
			return _countryCode.Select(e => e.Code).OrderBy(e => e).ToArray();
		}

		/// <summary>
		/// Gets the country
		/// </summary>
		/// <param name="code">Country code</param>
		/// <returns>A <see cref="Afk.ZoneInfo.Country"/> object that represent the country.</returns>
		public static Country GetCountries(string code) {
			return _countryCode.FirstOrDefault(e => e.Code == code);
		}

		/// <summary>
		/// Gets the <see cref="TzTimeZone"/> that represents the time zone.
		/// </summary>
		/// <param name="zoneName">The time zone name, for sample "Europe/Paris"</param>
		/// <exception cref="ArgumentNullException">zoneName is null or empty.</exception>
		/// <returns>An <see cref="TzTimeZone"/> object whose name is the value of the zoneName parameter, null otherwise</returns>
		public static TzTimeZone GetZone(string zoneName) {
			if (string.IsNullOrEmpty(zoneName)) throw new ArgumentNullException("zoneName");

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
		public static TzTimeZone[] GetZones() {
			return _zones.Values.ToArray();
		}

		/// <summary>
		/// Retrieve a <see cref="TzTimeZone"/> object from its windows identifier
		/// </summary>
		/// <param name="id">The windows time zone identifier.</param>
		/// <exception cref="ArgumentException">id is null or empty.</exception>
		/// <returns>An <see cref="TzTimeZone"/> object whose windows identifier is the value of the id parameter, otherwise null.</returns>
		public static TzTimeZone FindSystemTzTimeZoneById(string id) {
			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("id");

            foreach (var keys in _mappingWindowsTZID)
            {
                if (keys.Key == id)
                    return GetZone(keys.Value);
            }

			return null;
		}
	}
}
