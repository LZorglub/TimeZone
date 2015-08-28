using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afk.ZoneInfo {
	/// <summary>
	/// Represents a country. 
	/// </summary>
	/// <remarks>There are one or more time zone under a country.</remarks>
	/// <example>This sample list all the time zone for the US country :
	/// <code>
	/// Country country = TzTimeInfo.GetCountry("US");
	///	foreach (string tz in country.TZ)
	///		Console.WriteLine(tz);
	/// </code>
	/// </example>
	public class Country {
		private string _code;
		private List<_TZ> _tz;

		internal struct _TZ {
			public string coordinates;
			public string TZName;
			public string comment;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="Country"/>
		/// </summary>
		private Country() {
			_tz = new List<_TZ>();
		}

		/// <summary>
		/// Initializes a new instance of <see cref="Country"/>
		/// </summary>
		/// <param name="code">Country code</param>
		internal Country(string code)
			: this() {
			_code = code;
		}

		/// <summary>
		/// Gets the name of country
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the ISO 3166-1 code of country
		/// </summary>
		public string Code { get { return _code; } }

		/// <summary>
		/// Gets the list of time zone name
		/// </summary>
		public string[] TZ { get { return _tz.Select(e => e.TZName).ToArray(); } }

		/// <summary>
		/// Add a new zone to the country
		/// </summary>
		/// <param name="tzname">Zone name</param>
		/// <param name="coordinates">Zone's coordinates</param>
		/// <param name="comment">Zone's comment</param>
		internal void AddTZ(string tzname, string coordinates, string comment) {
			_tz.Add(new _TZ() { TZName = tzname, coordinates = coordinates, comment = comment });
		}

		/// <summary>
		/// Gets the time zone list of country
		/// </summary>
		internal List<_TZ> TZone { get { return _tz; } }
	}
}
