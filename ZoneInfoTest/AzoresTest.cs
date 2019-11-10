using Afk.ZoneInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ZoneInfoTest
{
    public class AzoresTest
    {
        [Fact]
        public void Test1920()
        {
            DateTime local = new DateTime(1920, 02, 29, 21, 0, 0, DateTimeKind.Local);
            DateTime utc = new DateTime(1920, 02, 29, 23, 0, 0, 0, DateTimeKind.Utc);

            var zoneInfo = TzTimeInfo.GetZones().Single(z => z.Name == "Atlantic/Azores");

            Assert.Equal(utc, zoneInfo.ToUniversalTime(local, true));

            local = local.AddHours(1);
            utc = utc.AddHours(1);
            Assert.Equal(utc, zoneInfo.ToUniversalTime(local, true));

            local = local.AddHours(2); // DST Changes
            utc = utc.AddHours(1);
            Assert.Equal(utc, zoneInfo.ToUniversalTime(local, true));
        }
    }
}
