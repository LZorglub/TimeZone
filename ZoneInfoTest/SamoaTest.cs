using Afk.ZoneInfo;
using System;
using System.Linq;
using Xunit;

namespace ZoneInfoTest
{
     /// <summary>
    /// // Refer to https://en.wikipedia.org/wiki/Time_in_Samoa for explanation on 30th december 2011
    /// </summary>   public class SamoaTest
    public class SamoaTest
    {
        [Fact]
        public void ShouldLostOneDayInDec2011()
        {
            DateTime utc = new DateTime(2011, 12, 30, 7, 0, 0, DateTimeKind.Utc);
            DateTime expected = new DateTime(2011, 12, 29, 21, 0, 0, DateTimeKind.Local);

            var zone = TzTimeInfo.GetZones().Single(z => z.Name == "Pacific/Apia");

            for (int i = 0; i < 3; i++)
            {
                var actual = zone.ToLocalTime(utc);
                Assert.Equal(expected, actual);
                utc = utc.AddHours(1);
                expected = expected.AddHours(1);
            }

            // Lost one day
            expected = new DateTime(2011, 12, 31, 0, 0, 0, DateTimeKind.Local);
            Assert.Equal(expected, zone.ToLocalTime(utc));
        }

        [Fact]
        public void TestSamoa2011()
        {

            // The 30th december 2011 dont exists
            DateTime utc = new DateTime(2011, 12, 28, 0, 0, 0, DateTimeKind.Utc);

            // windows
            TimeZoneInfo samoaZone = TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time");

            // ZoneInfo
            var zoneInfo = TzTimeInfo.FindSystemTzTimeZoneById("Samoa Standard Time");

            while (utc.Year <= 2012)
            {
                var localWindows = TimeZoneInfo.ConvertTimeFromUtc(utc, samoaZone);
                var localZoneInfo = zoneInfo.ToLocalTime(utc, false);

                // Windows fails on the 30th
                if (localWindows.Day == 30 && localZoneInfo.Day == 31)
                {
                    break;
                }
                Assert.Equal(localWindows, localZoneInfo);
                utc = utc.AddHours(1);
            }
        }

        [Fact]
        public void TestOptimizeUtc()
        {
            DateTime utc = new DateTime(2011, 1, 1, 7, 0, 0, DateTimeKind.Utc);

            var zone = TzTimeInfo.GetZones().Single(z => z.Name == "Pacific/Apia");

            while (utc.Year < 2020)
            {
                DateTime expected = zone.ToLocalTime(utc, false);
                DateTime actual = zone.ToLocalTime(utc, true);

                Assert.Equal(expected, actual);
                utc = utc.AddHours(1);
            }
        }

        [Fact]
        public void TestOptimizeLocal()
        {
            DateTime local = new DateTime(2011, 1, 1, 7, 0, 0, DateTimeKind.Local);

            var zone = TzTimeInfo.GetZones().Single(z => z.Name == "Pacific/Apia");

            while (local.Year < 2020)
            {
                // The 30th december 2011 didn't exists in samoa
                if (local.Day == 30 && local.Month == 12 && local.Year == 2011)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => zone.ToUniversalTime(local, false));
                    local = local.AddDays(1);
                }

                DateTime expected = zone.ToUniversalTime(local, false);
                DateTime actual = zone.ToUniversalTime(local, true);

                Assert.Equal(expected, actual);
                local = local.AddHours(1);
            }
        }
    }
}
