using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afk.ZoneInfo {
	/// <summary>
	/// Représente une règle d'une zone
	/// </summary>
	sealed class TzTimeZoneRule {

		/// <summary>
		/// Initialise une nouvelle instance de <see cref="TzTimeZoneRule"/>
		/// </summary>
		internal TzTimeZoneRule() {
			GmtOffset = TimeSpan.Zero;
			FixedStandardOffset = TimeSpan.Zero;
		}

		/// <summary>
		/// Obtient le temps qu'il faut ajouter à UTC pour obtenir la date standard de la zone
		/// </summary>
		public TimeSpan GmtOffset { get; internal set; }

		/// <summary>
		/// Obtient le format de d'abbreviation de la time zone
		/// </summary>
		public string Format { get; internal set; }

		/// <summary>
		/// Obtient le nom de la règle à appliquer
		/// </summary>
		/// <remarks>
		/// Peut représenter aussi le temps qu'il faut ajouter à la date standard (utiliser StandardOffset)
		/// Si aucune règle, le temps standard est appliquée à la zone
		/// </remarks>
		public string RuleName { get; internal set; }

		/// <summary>
		/// Obtient le temps qu'il faut ajouter à la date standard
		/// </summary>
		/// <remarks>
		/// Cette propriété n'est valable que pour les zone définissant directement l'offset et
		/// non pas un nom de règle
		/// </remarks>
		public TimeSpan FixedStandardOffset { get; internal set; }

		/// <summary>
		/// Obtient la règle défissant la date butoir d'effet de la zone
		/// </summary>
		public TzTimeZoneRuleUntil Until { get; internal set; }

		/// <summary>
		/// Obtient la date de début de définition de la zone
		/// </summary>
		/// <remarks>
		/// StandardOffset représente l'offset définit avant la date d'effet.
		/// GmtOffset représente l'offset définit avant la date d'effet.
		/// </remarks>
		internal TzTimeZoneRuleDate StartZone { get; set; }

		/// <summary>
		/// Obtient la date de fin de définition de la zone
		/// </summary>
		/// <remarks>
		/// StandardOffset représente l'offset définit avant la date d'effet.
		/// GmtOffset représente l'offset définit avant la date d'effet.
		/// </remarks>
		internal TzTimeZoneRuleDate EndZone { get; set; }
	}
}
