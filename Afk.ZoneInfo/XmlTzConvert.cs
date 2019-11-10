using System;
using System.Globalization;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Xml date conversion
    /// </summary>
    public static class XmlTzConvert
    {
        private static System.Text.RegularExpressions.Regex dateRegex = new System.Text.RegularExpressions.Regex(
            @"^(?<YEAR>\d{4})\x2D(?<MONTH>\d{2})\x2D(?<DAY>\d{2})T(?<HOUR>\d{2}):(?<MINUTE>\d{2}):(?<SEC>\d{2})(?:.(?<MS>\d{1,3}))?(?:(?<UTC>Z)|(?<SGN>[+|-])(?<SH>\d{2}):(?<SM>\d{2}))?$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// Converts the String to a DateTime equivalent using the specified time zone
        /// </summary>
        /// <param name="s"></param>
        /// <param name="timeZone"></param>
        /// <param name="dateTimeKind"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(string s, TzTimeZone timeZone, DateTimeKind dateTimeKind)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(nameof(s));
            if (timeZone == null) throw new ArgumentNullException(nameof(timeZone));
            if (dateTimeKind == DateTimeKind.Unspecified) throw new ArgumentException("Datetime kind unspecified", nameof(dateTimeKind));

            System.Text.RegularExpressions.Match m = dateRegex.Match(s);
            if (m.Success)
            {
                DateTime result = new DateTime(Convert.ToInt32(m.Groups["YEAR"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32(m.Groups["MONTH"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32(m.Groups["DAY"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32(m.Groups["HOUR"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32(m.Groups["MINUTE"].Value, CultureInfo.InvariantCulture),
                    Convert.ToInt32(m.Groups["SEC"].Value, CultureInfo.InvariantCulture), DateTimeKind.Local);
                if (m.Groups["MS"].Success)
                {
                    result = result.AddMilliseconds(Convert.ToInt32(m.Groups["MS"], CultureInfo.InvariantCulture));
                }

                // Date is local, check return 
                if (m.Groups["UTC"].Success)
                {
                    if (dateTimeKind == DateTimeKind.Utc)
                        return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                    else
                        return timeZone.ToLocalTime(DateTime.SpecifyKind(result, DateTimeKind.Utc));
                }
                else
                {
                    // Date string is local, check offset
                    if (m.Groups["SGN"].Success)
                    {
                        // Translate in utc
                        if (m.Groups["SGN"].Value == "+")
                            result = result.AddHours(-Convert.ToInt32(m.Groups["SH"].Value, CultureInfo.InvariantCulture))
                                .AddMinutes(-Convert.ToInt32(m.Groups["SM"].Value, CultureInfo.InvariantCulture));
                        else
                            result = result.AddHours(Convert.ToInt32(m.Groups["SH"].Value, CultureInfo.InvariantCulture))
                                .AddMinutes(Convert.ToInt32(m.Groups["SM"].Value, CultureInfo.InvariantCulture));

                        if (dateTimeKind == DateTimeKind.Utc)
                            return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                        else
                            return timeZone.ToLocalTime(DateTime.SpecifyKind(result, DateTimeKind.Utc));
                    }
                    else
                    {
                        // Zone unspecified, suppose to be local
                        if (dateTimeKind == DateTimeKind.Utc)
                            return timeZone.ToUniversalTime(result);
                        else
                            return result;
                    }
                }
            }

            throw new ArgumentException($"{s} is an invalid format");
        }
    }
}
