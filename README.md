# TimeZone
**Afk.ZoneInfo** is an alternative to the framework 3.5 class *System.TimeZoneInfo*.

The *System.TimeZoneInfo* is really powerfull, unfortunatly if your server dont update the cumulative time zone update for Windows operating systems it can quickly be wrong.
The Samoa time zone is a good sample, in december 2011 they jump forwards in time from 29 december to 31 december. Link to [wikipedia](https://en.wikipedia.org/wiki/Time_in_Samoa).
Another weakness is that you need to wait cumulative update to fix wrong convertion and when daylight saving time are not predictible like [Morocco Ramadan DST changes](https://support.microsoft.com/en-us/kb/3062740), the update can be late.

The **Afk.ZoneInfo** is a .Net library 3.5 which use the Iana timezone database (previously named Olson Database).
By default the library is linked with a database files. You can override it with recent files, for this set the environment variable **TZDIR** with the new directory of database files.

# HowTo

# External Links
[Tz database on wikipedia](http://en.wikipedia.org/wiki/Tz_database)
[Iana timezone database](http://www.iana.org/time-zones)
[Mapping from windows timezone IDs to TZIDS](http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html)
