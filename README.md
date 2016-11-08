# TimeZone
**Afk.ZoneInfo** is an alternative to the framework class *System.TimeZoneInfo*.

The **Afk.ZoneInfo** is a .Net library 3.5 which use the Iana timezone database (previously named Olson Database).
By default the library is linked with a database files. You can override it with recent files, for this set the environment variable **TZDIR** with the new directory of database files.

# NUGET
The easiest way to install is by using [NuGet](https://www.nuget.org/packages/Afk.ZoneInfo/).

# HowTo
Local time zone can be retrieve by the static *TzTimeZone.CurrentTzTimeZone* member. The following code uses the CurrentTzTimeZone to return the local times that correspond to UTC.
```
TzTimeZone local = TzTimeZone.CurrentTzTimeZone;
Console.WriteLine(local.ToLocalTime(DateTime.UtcNow));
```

Specific time zone can be retrieve by the static *TzTimeInfo.GetZone* function and translation are provide by ToLocalTime and ToUniversalTime functions.
```
DateTime dateLocal = new DateTime(2011, 1, 5, 10, 0, 0, DateTimeKind.Local);
TzTimeZone paris = TzTimeInfo.GetZone("Europe/Paris");
TzTimeZone ny = TzTimeInfo.GetZone("America/New_York");
DateTime dateUtc = paris.ToUniversalTime(dateLocal);
Console.WriteLine("Local time at paris : " + dateLocal + " UTC : " + dateUtc);
Console.WriteLine("Local time at new york : " + ny.ToLocalTime(dateUtc));
Console.WriteLine("");
Console.WriteLine("Paris : " + dateLocal + " New york " + paris.ToTimeZone(dateLocal, ny));
```

The previous code generates the following output :
```
Local time at paris : 05/01/2011 10:00:00 UTC : 05/01/2011 09:00:00
Local time at new york : 05/01/2011 04:00:00
Paris : 05/01/2011 10:00:00 New york : 05/01/2011 04:00:00
```
## Retrieve TimeZone
*TzTimeZone* can be retrieve directly by members of class *Afk.ZoneInfo.Zones*.
```
TzTimeZone timeZone = Afk.ZoneInfo.Zones.America.New_York;
```

However you can use the static functions of class *Afk.ZoneInfo.TzTimeInfo* to enumerate all available zones.

| Function | Description |
|----------|-------------|
| GetCountries | Gets all countries |
| GetCountriesCode | Gets all countries code |
| GetZone | Gets the TzTimeZone related to the zone name specified |
| GetZones | Gets all TzTimeZone |
| GetZoneNames | Gets all timezone name |
| FindSystemTzTimeZoneById | Gets the TzTimeZone related to the windows id specified |

# External Links
* [Tz database on wikipedia](http://en.wikipedia.org/wiki/Tz_database)
* [Iana timezone database](http://www.iana.org/time-zones)
* [Mapping from windows timezone IDs to TZIDS](http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html)
* [Samoa Time Zone](https://en.wikipedia.org/wiki/Time_in_Samoa)
* [Morocco Ramadan DST changes](https://support.microsoft.com/en-us/kb/3062740)
