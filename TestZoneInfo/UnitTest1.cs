using Afk.ZoneInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TestZoneInfo
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: ajoutez ici la logique du constructeur
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Obtient ou définit le contexte de test qui fournit
        ///des informations sur la série de tests active ainsi que ses fonctionnalités.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributs de tests supplémentaires
        //
        // Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
        //
        // Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test de la classe
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Utilisez ClassCleanup pour exécuter du code une fois que tous les tests d'une classe ont été exécutés
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [DataTestMethod]
        [DataRow(false, DisplayName = "No prefetch")]
        [DataRow(true, DisplayName = "Prefetch")]
        
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

                Assert.AreEqual(localWindows, localZoneInfo);

                utc = utc.AddHours(1);
            }
        }

        [TestMethod]
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

                Assert.AreEqual(localWindows, localZoneInfo);

                utc = utc.AddHours(1);
            }

            Assert.AreEqual(utc.Year, 2018);
        }

        [TestMethod]
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
                Assert.AreEqual(expected, actual);
            }
        }

        [DataTestMethod]
        [DataRow("Europe/Paris", DisplayName = "France")]
        [DataRow("America/Grand_Turk", DisplayName = "Grand Turk")]
        public void TestPrefetch(string zoneName)
        {
            var zone = TzTimeInfo.GetZones().Single(z => z.Name == zoneName);

            var start = new DateTime(2015, 01, 1, 0, 0, 0, DateTimeKind.Utc);

            while (start.Year < 2020)
            {
                start = start.AddHours(1);
                DateTime d1 = zone.ToLocalTime(start, false);
                DateTime d2 = zone.ToLocalTime(start, true);
                Assert.AreEqual(d1, d2);
            }
        }

        [TestMethod]
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

                Assert.AreEqual(localWindows, localZoneInfo);
                utc = utc.AddHours(1);
            }
        }

        // TODO Rule Port (Europe) with changes the 29 Feb

    }
}
