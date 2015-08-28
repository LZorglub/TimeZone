using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Afk.ZoneInfo {

	static class TzUtilities {

		private static string[] Months = new string[]{
			"january","february","march","april","may","june","july","august","september","october","november","december"};

		private static string[] BeginYears = new string[] { "minimum", "maximum" };

		private static string[] EndYears = new string[] { "minimum", "maximum", "only" };

		private static string[] LastDays = new string[] {
			 "last-Sunday",	"last-Monday",	"last-Tuesday",	"last-Wednesday",	"last-Thursday",	
			 "last-Friday",	"last-Saturday"};

		private static string[] DayNames = new string[] {"Sunday", "Monday","Tuesday","Wednesday","Thursday","Friday","Saturday" };

		private static DayOfWeek[] DayWeek = new DayOfWeek[]{
			DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
			DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

		private static int[] LenOfMonths = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

		/// <summary>
		/// Découpe une ligne en ses différentes parties
		/// </summary>
		/// <param name="line">Ligne à découper</param>
		/// <param name="count">Nombre maximum de partie à renvoyer</param>
		/// <returns>Liste des différents éléments contenus dans la ligne spécifiée.</returns>
		public static List<string> GetFields(string line, int count) {
			if (string.IsNullOrEmpty(line)) return null;

			List<string> nsubs = new List<string>();
			StringBuilder sb = new StringBuilder();
			int index = 0; int startIndex = 0;

			do {
				while (index < line.Length && char.IsWhiteSpace(line[index]))
					index++;

				if (index >= line.Length || line[index] == '#')
					break;

				if (count > 0 && nsubs.Count + 1 == count) {
					nsubs.Add(line.Substring(index));
					break;
				}

				startIndex = index;
				do {
					if (line[index++] == '"') {
						do {
							if (index >= line.Length)
								throw new Exception("Odd number of quotation marks");
						} while (line[index++] != '"');
					}
				} while (index < line.Length && !char.IsWhiteSpace(line[index]) && line[index] != '#');

				if (index != startIndex)
					nsubs.Add(line.Substring(startIndex, (index - startIndex)));
			} while (true);

			return nsubs;
		}

		public static TimeSpan GetHMS(string time) {
			if (string.IsNullOrEmpty(time)) throw new ArgumentNullException("time");

			if (time == "-") return TimeSpan.Zero;

			string[] array = time.Split(':');

			// Get the hour and the sign(+/-) of the hour in seconds
			int hour = Convert.ToInt32(array[0]) * 3600;
			int min = 0;
			int sec = 0;
			int sign = Math.Sign(hour);

			if (array.Length > 1) {
				min = Convert.ToInt32(array[1]) * ((sign == 0) ? 1 : sign) * 60;
			}

			if (array.Length > 2) {
				sec = Convert.ToInt32(array[2]) * ((sign == 0) ? 1 : sign);
			}

			return new TimeSpan(0, 0, hour + min + sec);
		}

		/// <summary>
		/// Obtient l'indice du mois spécifié
		/// </summary>
		/// <param name="month"></param>
		/// <returns></returns>
		public static int GetMonth(string month) {
			if (string.IsNullOrEmpty(month)) throw new ArgumentNullException("month");

			string lowerMonth = month.ToLower();
			var select = 
				Months.Select((e, i) => new { Name = e, Pos = i + 1 })
					  .Where(e => itsabbr(lowerMonth, e.Name));

			if (select.Count() == 1)
				return select.ElementAt(0).Pos;
			else if (select.Count() > 1)
				throw new ArgumentException("Multiple inexact match", month);

			throw new ArgumentException("Invalid month", month);
		}

		/// <summary>
		/// Indique si l'année spécifiée est du type spécifié
		/// </summary>
		/// <param name="year">Année</param>
		/// <param name="yearType">Type d'année à vérifier</param>
		/// <returns></returns>
		public static bool IsYearType(int year, YearType yearType) {
			switch (yearType) {
				case YearType.none:
					return true;
				case YearType.even:
					return (year % 2) == 0;
				case YearType.odd:
					return (year % 2) == 1;
				// The rule applies in U.S. Presidential election years
				case YearType.uspres:
					return (year % 4) == 0;
				// The rule applies in years other than U.S. Presidential election years
				case YearType.nonpres:
				case YearType.nonuspres:
					return (year % 4) != 0;
			}
			return false;
		}

		/// <summary>
		/// Indique si une abbréviation vérifie un mot
		/// </summary>
		/// <param name="abbr">Abbréviation</param>
		/// <param name="word">Mot à vérifier</param>
		/// <returns>True si l'abbréviation peut se rapporter au mot</returns>
		private static bool itsabbr(string abbr, string word) {
			if (string.IsNullOrEmpty(abbr) || string.IsNullOrEmpty(word))
				return false;

			string labbr = abbr.ToLower();
			string lword = word.ToLower();

			if (labbr == lword) return true;
			if (labbr[0] != lword[0]) return false;
			int iword = 1;

			for (int index = 1; index < labbr.Length; index++) {
				do {
					if (iword >= word.Length) return false;
				} while (lword[iword++] != labbr[index]);
			}

			return true;
		}

		/// <summary>
		/// Obtient une règle en fonction de sa description
		/// </summary>
		/// <param name="ruleDescription"></param>
		/// <returns>
		/// # Rule	NAME	FROM	TO	TYPE	IN	ON	AT	SAVE	LETTER/S
		/// </returns>
		public static Rule GetRule(List<string> ruleDescription) {
			Rule r = new Rule();

			Debug.Assert(ruleDescription.Count == 10);
			Debug.Assert(ruleDescription[0] == "Rule");

			r.Name = ruleDescription[1];
			r.StandardOffset = GetHMS(ruleDescription[8]);
			r.Letter = ruleDescription[9];

			string loyearp = ruleDescription[2];
			string hiyearp = ruleDescription[3];
			string typep = ruleDescription[4];

			// Year work
			// Min Year (FROM)
			IEnumerable<string> by =
				BeginYears.Where(e => itsabbr(loyearp, e));
			if (by.Count() == 0) {
				r.LowerYear = Int32.Parse(loyearp);
			}
			else if (by.Count() == 1) {
				if (by.ElementAt(0) == BeginYears[0])
					r.LowerYear = DateTime.MinValue.Year;
				else
					r.LowerYear = DateTime.MaxValue.Year;
			}
			else {
				throw new ArgumentException("Multiple inexact match", loyearp);
			}

			// Max Year (TO)
			by = EndYears.Where(e => itsabbr(hiyearp, e));
			if (by.Count() == 0) {
				r.HighYear = Int32.Parse(hiyearp);
			}
			else if (by.Count() == 1) {
				if (by.ElementAt(0) == EndYears[0])
					r.HighYear = DateTime.MinValue.Year;
				else if (by.ElementAt(0) == EndYears[1])
					r.HighYear = DateTime.MaxValue.Year;
				else
					r.HighYear = r.LowerYear;
			}
			else {
				throw new ArgumentException("Multiple inexact match", hiyearp);
			}

			if (r.LowerYear > r.HighYear)
				throw new ArgumentException("Starting year greater than ending year");

			// Type (TYPE)
			switch (typep) {
				case "":
				case "-":
					r.YearType = YearType.none;
					break;
				case "even":
					r.YearType = YearType.even;
					break;
				case "odd":
					r.YearType = YearType.odd;
					break;
				case "uspres":
					r.YearType = YearType.uspres;
					break;
				case "nonpres":
					r.YearType = YearType.nonpres;
					break;
				case "nonuspres":
					r.YearType = YearType.nonuspres;
					break;
				default:
					throw new ArgumentException("Unknown type " + typep);
			}

			SetRuleSub(r, ruleDescription[5], ruleDescription[6], ruleDescription[7]);

			return r;
		}

		/// <summary>
		/// Remplit une règle avec les paramètres spécifiés
		/// </summary>
		/// <param name="rp">Règle à complèter</param>
		/// <param name="monthp">IN / UNTILMONTH</param>
		/// <param name="dayp">ON / UNTILDAY</param>
		/// <param name="timep">AT / UNTILTIME</param>
		private static void SetRuleSub(RuleDate rp, string monthp, string dayp, string timep) {
			// Month (IN)
			rp.Month = TzUtilities.GetMonth(monthp);

			// (AT)
			rp.AtKind = TimeKind.LocalWallTime;
			if (timep.EndsWith("s")) {
				rp.AtKind = TimeKind.LocalStandardTime;
				rp.At = TzUtilities.GetHMS(timep.Substring(0, timep.Length - 1));
			}
			else if (timep.EndsWith("w")) {
				rp.AtKind = TimeKind.LocalWallTime;
				rp.At = TzUtilities.GetHMS(timep.Substring(0, timep.Length - 1));
			}
			else if (timep.EndsWith("g") ||
				timep.EndsWith("u") ||
				timep.EndsWith("z")) {
				rp.AtKind = TimeKind.UniversalTime;
				rp.At = TzUtilities.GetHMS(timep.Substring(0, timep.Length - 1));
			}
			else {
				rp.At = TzUtilities.GetHMS(timep);
			}

			// Day work (ON)
			// Accept thing such as
			// 1
			// last-Sunday
			// Sun<=20
			// Sun>=7
			var lday = LastDays.Select((e, i) => new { Name = e, Day = DayWeek[i] }).Where(e => itsabbr(dayp, e.Name));
			if (lday.Count() == 1) {
				rp.Day = new DayOfRule(DayWorkKind.DowLeq, lday.ElementAt(0).Day, -1);
			}
			else {
				int index = 0;
				DayWorkKind dw = DayWorkKind.Dom;
				if ((index = dayp.IndexOf("<")) != -1)
					dw = DayWorkKind.DowLeq;
				else if ((index = dayp.IndexOf(">")) != -1)
					dw = DayWorkKind.DowGeq;
				else {
					index = 0;
					dw = DayWorkKind.Dom;
				}

				DayOfWeek dweek = DayOfWeek.Sunday;
				if (dw != DayWorkKind.Dom) {
					index++;
					if (dayp[index++] != '=')
						throw new ArgumentException("Invalid day of month", dayp);

					var lp = DayNames.Select((e, i) => new { Name = e, Day = DayWeek[i] }).Where(e => itsabbr(dayp.Substring(0, index - 2), e.Name));
					if (lp.Count() != 1) {
						throw new ArgumentException("Invalid weekday name", dayp);
					}
					dweek = lp.ElementAt(0).Day;
				}
				rp.Day = new DayOfRule(dw, dweek, Int32.Parse(dayp.Substring(index)));
				if (rp.Day.DayOfMonth <= 0 || rp.Day.DayOfMonth > LenOfMonths[rp.Month - 1])
					throw new ArgumentException("Invalid day of month", dayp);
			}
		}

		/// <summary>
		/// Obtient une zone en fonction de sa description
		/// </summary>
		/// <param name="zoneDescription">Ensemble de paramètres décrivant la zone</param>
		/// <param name="zone"><see cref="TzTimeZone"/> à renseigner en fonction de la description spécifiée.</param>
		/// <param name="append">Indique que la description est un ajout ou non à la zone spécifiée.</param>
		/// <returns><see cref="bool"/> spécifiant qu'un complément de description est attendu ou non.</returns>
		/// <remarks>
		/// The format of each line is:
		/// Zone	NAME		GMTOFF	RULES	FORMAT	[UNTIL]
		/// </remarks>
		public static bool GetZone(List<string> zoneDescription, ref TzTimeZone zone, bool append) {
			if (!append && (zoneDescription.Count < 5 || zoneDescription.Count > 9)) {
				throw new ArgumentException("Wrong number of fields on zone line");
			}

			if (append && (zoneDescription.Count < 3 || zoneDescription.Count > 7)) {
				throw new ArgumentException("Wrong number of fields on zone continuation line");
			}

			return GetZoneSub(zoneDescription, ref zone, append);
		}

		/// <summary>
		/// Renseigne une zone en fonction de sa description
		/// </summary>
		/// <param name="zoneDescription">Ensemble de paramètres décrivant la zone</param>
		/// <param name="zone"><see cref="TzTimeZone"/> à renseigner en fonction de la description spécifiée.</param>
		/// <param name="continuation">Indique que la description est un ajout ou non à la zone spécifiée.</param>
		/// <returns><see cref="bool"/> spécifiant qu'un complément de description est attendu ou non.</returns>
		public static bool GetZoneSub(List<string> zoneDescription, ref TzTimeZone zone, bool continuation) {
			int i_gmtoff, i_rule, i_format;
			int i_untilyear, i_untilmonth;
			int i_untilday, i_untiltime;

			TzTimeZone zcont = zone;

			if (continuation) {
				i_gmtoff = 0;
				i_rule = 1;
				i_format = 2;
				i_untilyear = 3; 
				i_untilmonth = 4;
				i_untilday = 5;
				i_untiltime = 6;
			}
			else {
				i_gmtoff = 2;
				i_rule = 3;
				i_format = 4;
				i_untilyear = 5;
				i_untilmonth = 6;
				i_untilday = 7;
				i_untiltime = 8;
				zcont.Name = zoneDescription[1];
			}

			TzTimeZoneRule zoneRule = new TzTimeZoneRule();
			zoneRule.GmtOffset = TzUtilities.GetHMS(zoneDescription[i_gmtoff]);
			zoneRule.Format = zoneDescription[i_format];
			zoneRule.RuleName = zoneDescription[i_rule];

			bool hashuntil = zoneDescription.Count > i_untilyear;
			if (hashuntil) {
				zoneRule.Until = new TzTimeZoneRuleUntil();
				zoneRule.Until.Year = Convert.ToInt32(zoneDescription[i_untilyear]);

				SetRuleSub(zoneRule.Until, 
					(zoneDescription.Count > i_untilmonth) ? zoneDescription[i_untilmonth] : "Jan",
					(zoneDescription.Count > i_untilday) ? zoneDescription[i_untilday] : "1",
					(zoneDescription.Count > i_untiltime) ? zoneDescription[i_untiltime] : "0");
			}
			zcont.ZoneRules.Add(zoneRule);

			return hashuntil;
		}

		/// <summary>
		/// Obtient une date locale en fonction des paramètres spécifiés
		/// </summary>
		/// <param name="rule"><see cref="RuleDate"/> décrivant la date</param>
		/// <param name="year">Année de la date</param>
		/// <param name="gmtOffset">Offset gmt dans lequel est exprimé la date</param>
		/// <param name="stdOffset">Offset standard dans lequel est exprimé la date</param>
		/// <returns><see cref="DateTime"/> de type local correspondant aux paramètres spécifiés</returns>
		public static DateTime GetWallClockTime(RuleDate rule, int year, TimeSpan gmtOffset, TimeSpan stdOffset) {
			DateTime du = rule.ToUnspecifiedTime(year);

			switch (rule.AtKind) {
				case TimeKind.LocalStandardTime:
					du = du.Add(stdOffset);
					break;
				case TimeKind.LocalWallTime:
					// Already wall clock
					break;
				case TimeKind.UniversalTime:
					du = du.Add(gmtOffset+stdOffset);
					break;
			}

			return DateTime.SpecifyKind(du, DateTimeKind.Local);
		}

		/// <summary>
		/// Obtient une date utc en fonction des paramètres spécifiés
		/// </summary>
		/// <param name="rule"><see cref="RuleDate"/> décrivant la date</param>
		/// <param name="year">Année de la date</param>
		/// <param name="gmtOffset">Offset gmt dans lequel est exprimé la date</param>
		/// <param name="stdOffset">Offset standard dans lequel est exprimé la date</param>
		/// <returns><see cref="DateTime"/> de type utc correspondant aux paramètres spécifiés</returns>
		public static DateTime GetUTCTime(RuleDate rule, int year, TimeSpan gmtOffset, TimeSpan stdOffset) {
			DateTime du = rule.ToUnspecifiedTime(year);

			switch (rule.AtKind) {
				case TimeKind.LocalStandardTime:
					du = du.Add(-gmtOffset);
					break;
				case TimeKind.LocalWallTime:
					du = du.Add(-(stdOffset+gmtOffset));
					break;
				case TimeKind.UniversalTime:
					// Already UTC
					break;
			}

			return DateTime.SpecifyKind(du, DateTimeKind.Utc);
		}

		/// <summary>
		/// Obtient une date dans le kind spécifié en fonction des paramètres spécifiés
		/// </summary>
		/// <param name="rule"><see cref="RuleDate"/> décrivant la date</param>
		/// <param name="year">Année de la date</param>
		/// <param name="gmtOffset">Offset gmt dans lequel est exprimé la date</param>
		/// <param name="stdOffset">Offset standard dans lequel est exprimé la date</param>
		/// <param name="dateTimeKind"><see cref="DateTimeKind"/> dans lequel exprimer la date</param>
		/// <returns><see cref="DateTime"/> du kind spécifié correspondant aux paramètres spécifiés</returns>
		public static DateTime GetDateTime(RuleDate rule, int year, TimeSpan gmtOffset, TimeSpan stdOffset, DateTimeKind dateTimeKind) {
			if (dateTimeKind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", "dateTimeKind");

			if (dateTimeKind == DateTimeKind.Local)
				return GetWallClockTime(rule, year, gmtOffset, stdOffset);
			return GetUTCTime(rule, year, gmtOffset, stdOffset);
		}

		/// <summary>
		/// Obtient une date dans le kind spécifié en fonction des paramètres spécifiés
		/// </summary>
		/// <param name="dateTime"><see cref="DateTime"/> décrivant la date</param>
		/// <param name="gmtOffset">Offset gmt dans lequel est exprimé la date</param>
		/// <param name="stdOffset">Offset standard dans lequel est exprimé la date</param>
		/// <param name="dateTimeKind"><see cref="DateTimeKind"/> dans lequel exprimer la date</param>
		/// <returns><see cref="DateTime"/> du kind spécifié correspondant aux paramètres spécifiés</returns>
		public static DateTime GetDateTime(DateTime dateTime, TimeSpan gmtOffset, TimeSpan stdOffset, DateTimeKind dateTimeKind) {
			if (dateTimeKind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", "dateTimeKind");
			if (dateTime.Kind == DateTimeKind.Unspecified) throw new ArgumentException("Unspecified date time kind", "dateTime");
			if (dateTime.Kind == dateTimeKind) return dateTime;

			DateTime to = DateTime.SpecifyKind(dateTime, dateTimeKind);

			if (dateTime.Kind == DateTimeKind.Utc) {
				// LocalWallTime
				to = to.Add(gmtOffset + stdOffset);
			}
			else {
				// UTC
				to = to.Subtract(gmtOffset + stdOffset);
			}
			return to;
		}
	}
}

