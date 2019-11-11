using Afk.ZoneInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ZoneInfoTest
{
    public class ZoneTest
    {
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestYear2000(bool prefetch)
        {
            DateTime utc = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // windows
            TimeZoneInfo parisZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");

            // ZoneInfo
            var zoneInfo = Zones.Europe.Paris;

            while (utc.Year == 2000)
            {
                var localWindows = TimeZoneInfo.ConvertTimeFromUtc(utc, parisZone);
                var localZoneInfo = zoneInfo.ToLocalTime(utc, prefetch);

                Assert.Equal(localWindows, localZoneInfo);

                utc = utc.AddHours(1);
            }
        }

        [Fact]
        public void TestNewZealand()
        {
            DateTime utc = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // windows
            TimeZoneInfo nzZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");

            // ZoneInfo
            var zoneInfo = TzTimeInfo.FindSystemTzTimeZoneById("New Zealand Standard Time");

            while (utc.Year <= 2017)
            {
                var localWindows = TimeZoneInfo.ConvertTimeFromUtc(utc, nzZone);
                var localZoneInfo = zoneInfo.ToLocalTime(utc);

                Assert.Equal(localWindows, localZoneInfo);

                utc = utc.AddHours(1);
            }

            Assert.Equal(2018, utc.Year);
        }

        [Fact]
        public void TestTicks()
        {
            var zone = TzTimeInfo.GetZones().Single(z => z.Name == "America/Argentina/Buenos_Aires");

            var start = new DateTime(1998, 12, 31, 23, 56, 15, DateTimeKind.Utc);

            long expected = 630507345750000000;

            for (int i = 0; i < 10; i++)
            {
                start = start.AddTicks(i);
                expected = expected + i;

                var actual = zone.ToLocalTime(start).Ticks;
                Assert.Equal(expected, actual);
            }
        }
        
        [Theory]
        [InlineData("Europe/Paris")]
        [InlineData("America/Grand_Turk")]
        public void TestPrefetch(string zoneName)
        {
            var zone = TzTimeInfo.GetZones().Single(z => z.Name == zoneName);

            var start = new DateTime(2015, 01, 1, 0, 0, 0, DateTimeKind.Utc);

            while (start.Year < 2020)
            {
                start = start.AddHours(1);
                DateTime d1 = zone.ToLocalTime(start, false);
                DateTime d2 = zone.ToLocalTime(start, true);
                Assert.Equal(d1, d2);
            }
        }

        [Fact]
        public void TestGrandTruk()
        {
            DateTime utc = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            // windows
            TimeZoneInfo turkCaico = TimeZoneInfo.FindSystemTimeZoneById("Turks And Caicos Standard Time");

            // ZoneInfo
            var zoneInfo = TzTimeInfo.GetZones().Single(z => z.Name == "America/Grand_Turk");
            //var zoneInfo = TzTimeInfo.FindSystemTzTimeZoneById("Turks And Caicos Standard Time");
            //Assert.AreEqual(zoneInfo.Name, "America/Grand_Turk");

            while (utc.Year <= 2020)
            {
                var localWindows = TimeZoneInfo.ConvertTimeFromUtc(utc, turkCaico);
                var localZoneInfo = zoneInfo.ToLocalTime(utc, false);

                Assert.Equal(localWindows, localZoneInfo);
                utc = utc.AddHours(1);
            }
        }
    }
}
