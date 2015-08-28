using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afk.ZoneInfo {
	/// <summary>
	/// Représente une règle de changement d'heure
	/// </summary>
	/// <remarks>
	/// A rule line has the form
	/// <p><b>Rule</b>  NAME  FROM  TO    TYPE  IN   ON       AT    SAVE  LETTER/S</p>
	/// <para>
	/// For example:
	///
	/// <code>Rule  US    1967  1973  -     Apr  lastSun  2:00  1:00  D</code>
	/// </para>
	/// The fields that make up a rule line are:
	/// <list type="table">
	/// <listheader><term>Name</term><description>Description</description></listheader> 
	/// <item><term>NAME</term><description>Gives the (arbitrary) name of the set of rules this
	/// rule is part of.</description></item>
	/// <item><term>FROM</term><description>Gives the first year in which the rule applies. Any
	/// integer year can be supplied; the Gregorian calendar
	/// is assumed.  The word minimum (or an abbreviation)
	/// means the minimum year representable as an integer.
	/// The word maximum (or an abbreviation) means the
	/// maximum year representable as an integer.  Rules can
	/// describe times that are not representable as time
	/// values, with the unrepresentable times ignored; this
	/// allows rules to be portable among hosts with
	/// differing time value types.</description></item>
	///
	/// <item><term>TO</term><description>Gives the final year in which the rule applies. In
	/// addition to minimum and maximum (as above), the word
	/// only (or an abbreviation) may be used to repeat the
	/// value of the FROM field.</description></item>
	///
	/// <item><term>TYPE</term><description>Gives the type of year in which the rule applies.
	/// <para>If TYPE is - then the rule applies in all years
	/// between FROM and TO inclusive.</para><para>If TYPE is something
	/// else, then zic executes the command</para><para>yearistype year type</para>
	/// to check the type of a year: an exit status of zero
	/// is taken to mean that the year is of the given type;
	/// an exit status of one is taken to mean that the year
	/// is not of the given type.</description></item>
	///
	/// <item><term>IN</term><description>Names the month in which the rule takes effect.
	/// Month names may be abbreviated.</description></item>
	///
	/// <item><term>ON</term><description>Gives the day on which the rule takes effect.
	/// Recognized forms include:
	/// <list type="bullet">
	/// <item><term>5</term><description>the fifth of the month</description></item>
	/// <item><term>lastSun</term><description>the last Sunday in the month</description></item>
	/// <item><term>lastMon</term><description>the last Monday in the month</description></item>
	/// <item><term>Sun>=8</term><description>first Sunday on or after the eighth</description></item>
	/// <item><term>Sun&gt;=25</term><description>last Sunday on or before the 25th</description></item>
	/// </list>
	/// <para>Names of days of the week may be abbreviated or
	/// spelled out in full.  Note that there must be no
	/// spaces within the ON field.</para></description></item>
	///
	/// <item><term>AT</term><description>Gives the time of day at which the rule takes effect.
	/// <para>Recognized forms include:</para>
	/// <list type="bullet">
	/// <item><term>2</term><description>time in hours</description></item>
	/// <item><term>2:00</term><description>time in hours and minutes</description></item>
	/// <item><term>15:00</term><description>24-hour format time (for times after noon)</description></item>
	/// <item><term>1:28:14</term><description>time in hours, minutes, and seconds</description></item>
	/// <item><term>-</term><description>equivalent to 0</description></item>
	/// </list>
	/// <para>where hour 0 is midnight at the start of the day,
	/// and hour 24 is midnight at the end of the day.  Any
	/// of these forms may be followed by the letter w if
	/// the given time is local "wall clock" time, s if the
	/// given time is local "standard" time, or u (or g or
	/// z) if the given time is universal time; in the
	/// absence of an indicator, wall clock time is assumed.</para></description></item>
	///
	/// <item><term>SAVE</term><description>Gives the amount of time to be added to local
	/// standard time when the rule is in effect. This
	/// field has the same format as the AT field (although,
	/// of course, the w and s suffixes are not used).</description></item>
	///
	/// <item><term>LETTER/S</term><description>Gives the "variable part" (for example, the "S" or
	/// "D" in "EST" or "EDT") of time zone abbreviations to
	/// be used when this rule is in effect.  If this field
	/// is -, the variable part is null.</description></item>
	/// </list>
	/// </remarks>
	class Rule : RuleDate {

		/// <summary>
		/// Initialise une nouvelle instance de <see cref="Rule"/>
		/// </summary>
		internal Rule() {
		}

		/// <summary>
		/// Obtient le nom de la règle
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Obtient le nom du fichier définissant la règle
		/// </summary>
		public string Filename { get; internal set; }

		/// <summary>
		/// Obtient la ligne du fichier définissant la règle
		/// </summary>
		public int LineNumber { get; internal set; }

		/// <summary>
		/// Obtient l'offset à ajouter à l'heure standard
		/// </summary>
		public TimeSpan StandardOffset { get; internal set; }

		/// <summary>
		/// Obtient la lettre défissant la règle
		/// </summary>
		public string Letter { get; internal set; }

		/// <summary>
		/// Obtient l'année de début de définition de la règle
		/// </summary>
		public int LowerYear { get; internal set; }

		/// <summary>
		/// Obtient l'année de fin de définition de la règle
		/// </summary>
		public int HighYear { get; internal set; }

		/// <summary>
		/// Obtient le type d'année que respecte la règle
		/// </summary>
		public YearType YearType { get; internal set; }
	}
}
