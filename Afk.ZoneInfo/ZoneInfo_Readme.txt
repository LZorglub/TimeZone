Extract from file zic.8.txt from tzode*.tar.gz distribution    
	   
	   A rule line has the form

            Rule  NAME  FROM  TO    TYPE  IN   ON       AT    SAVE  LETTER/S

       For example:

            Rule  US    1967  1973  -     Apr  lastSun  2:00  1:00  D

       The fields that make up a rule line are:

       NAME    Gives the (arbitrary) name of the set of rules this rule is
               part of.

       FROM    Gives the first year in which the rule applies.  Any integer
               year can be supplied; the Gregorian calendar is assumed.  The
               word minimum (or an abbreviation) means the minimum year
               representable as an integer.  The word maximum (or an
               abbreviation) means the maximum year representable as an
               integer.  Rules can describe times that are not representable
               as time values, with the unrepresentable times ignored; this
               allows rules to be portable among hosts with differing time
               value types.

       TO      Gives the final year in which the rule applies.  In addition to
               minimum and maximum (as above), the word only (or an
               abbreviation) may be used to repeat the value of the FROM
               field.

       TYPE    Gives the type of year in which the rule applies.  If TYPE is -
               then the rule applies in all years between FROM and TO
               inclusive.  If TYPE is something else, then zic executes the
               command
                    yearistype year type
               to check the type of a year: an exit status of zero is taken to
               mean that the year is of the given type; an exit status of one
               is taken to mean that the year is not of the given type.

       IN      Names the month in which the rule takes effect.  Month names
               may be abbreviated.

       ON      Gives the day on which the rule takes effect.  Recognized forms
               include:

                    5        the fifth of the month
                    lastSun  the last Sunday in the month
                    lastMon  the last Monday in the month
                    Sun>=8   first Sunday on or after the eighth
                    Sun<=25  last Sunday on or before the 25th

               Names of days of the week may be abbreviated or spelled out in
               full.  Note that there must be no spaces within the ON field.

       AT      Gives the time of day at which the rule takes effect.
               Recognized forms include:

                    2        time in hours
                    2:00     time in hours and minutes
                    15:00    24-hour format time (for times after noon)
                    1:28:14  time in hours, minutes, and seconds
                    -        equivalent to 0

               where hour 0 is midnight at the start of the day, and hour 24
               is midnight at the end of the day.  Any of these forms may be
               followed by the letter w if the given time is local "wall
               clock" time, s if the given time is local "standard" time, or u
               (or g or z) if the given time is universal time; in the absence
               of an indicator, wall clock time is assumed.

       SAVE    Gives the amount of time to be added to local standard time
               when the rule is in effect.  This field has the same format as
               the AT field (although, of course, the w and s suffixes are not
               used).

       LETTER/S
               Gives the "variable part" (for example, the "S" or "D" in "EST"
               or "EDT") of time zone abbreviations to be used when this rule
               is in effect.  If this field is -, the variable part is null.

       A zone line has the form

            Zone  NAME                GMTOFF  RULES/SAVE  FORMAT  [UNTILYEAR [MONTH [DAY [TIME]]]]

       For example:

            Zone  Australia/Adelaide  9:30    Aus         CST     1971 Oct 31 2:00

       The fields that make up a zone line are:

       NAME  The name of the time zone.  This is the name used in creating the
             time conversion information file for the zone.

       GMTOFF
             The amount of time to add to UTC to get standard time in this
             zone.  This field has the same format as the AT and SAVE fields
             of rule lines; begin the field with a minus sign if time must be
             subtracted from UTC.

       RULES/SAVE
             The name of the rule(s) that apply in the time zone or,
             alternately, an amount of time to add to local standard time.  If
             this field is - then standard time always applies in the time
             zone.

       FORMAT
             The format for time zone abbreviations in this time zone.  The
             pair of characters %s is used to show where the "variable part"
             of the time zone abbreviation goes.  Alternately, a slash (/)
             separates standard and daylight abbreviations.

       UNTILYEAR [MONTH [DAY [TIME]]]
             The time at which the UTC offset or the rule(s) change for a
             location.  It is specified as a year, a month, a day, and a time
             of day.  If this is specified, the time zone information is
             generated from the given UTC offset and rule change until the
             time specified.  The month, day, and time of day have the same
             format as the IN, ON, and AT fields of a rule; trailing fields
             can be omitted, and default to the earliest possible value for
             the missing fields.

             The next line must be a "continuation" line; this has the same
             form as a zone line except that the string "Zone" and the name
             are omitted, as the continuation line will place information
             starting at the time specified as the "until" information in the
             previous line in the file used by the previous line.
             Continuation lines may contain "until" information, just as zone
             lines do, indicating that the next line is a further
             continuation.

       A link line has the form

            Link  LINK-FROM        LINK-TO

       For example:

            Link  Europe/Istanbul  Asia/Istanbul

       The LINK-FROM field should appear as the NAME field in some zone line;
       the LINK-TO field is used as an alternate name for that zone.

       Except for continuation lines, lines may appear in any order in the
       input.

       Lines in the file that describes leap seconds have the following form:

            Leap  YEAR  MONTH  DAY  HH:MM:SS  CORR  R/S

       For example:

            Leap  1974  Dec    31   23:59:60  +     S

       The YEAR, MONTH, DAY, and HH:MM:SS fields tell when the leap second
       happened.  The CORR field should be "+" if a second was added or "-" if
       a second was skipped.  The R/S field should be (an abbreviation of)
       "Stationary" if the leap second time given by the other fields should
       be interpreted as UTC or (an abbreviation of) "Rolling" if the leap
       second time given by the other fields should be interpreted as local
       wall clock time.