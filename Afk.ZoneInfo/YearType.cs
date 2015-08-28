using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afk.ZoneInfo {
	/// <summary>
	/// Définit les contraintes possibles sur une année
	/// </summary>
	enum YearType {
		/// <summary>
		/// No constraints
		/// </summary>
		none,
		/// <summary>
		/// Année paire
		/// </summary>
		even,
		/// <summary>
		/// Année impaire
		/// </summary>
		odd,
		/// <summary>
		/// The rule applies in years other than U.S. Presidential election years
		/// </summary>
 		nonpres,
		/// <summary>
		/// The rule applies in years other than U.S. Presidential election years
		/// </summary>
		nonuspres,
		/// <summary>
		/// The rule applies in U.S. Presidential election years
		/// </summary>
		uspres,
	}
}
