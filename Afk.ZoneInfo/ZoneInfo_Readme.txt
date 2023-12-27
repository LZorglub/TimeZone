Extract from file zic.8.txt from tzode*.tar.gz distribution    
	   
       A rule line has the form

            Rule  NAME  FROM  TO    -  IN   ON       AT     SAVE   LETTER/S

       For example:

            Rule  US    1967  1973  -  Apr  lastSun  2:00w  1:00d  D

       The fields that make up a rule line are:

       NAME    Gives the name of the rule set that contains this line.  The
               name must start with a character that is neither an ASCII digit
               nor "-" nor "+".  To allow for future extensions, an unquoted
               name should not contain characters from the set
               "!$%&'()*,/:;<=>?@[\]^`{|}~".

       FROM    Gives the first year in which the rule applies.  Any signed
               integer year can be supplied; the proleptic Gregorian calendar
               is assumed, with year 0 preceding year 1.  The word minimum (or
               an abbreviation) means the indefinite past.  The word maximum
               (or an abbreviation) means the indefinite future.  Rules can
               describe times that are not representable as time values, with
               the unrepresentable times ignored; this allows rules to be
               portable among hosts with differing time value types.

       TO      Gives the final year in which the rule applies.  In addition to
               minimum and maximum (as above), the word only (or an
               abbreviation) may be used to repeat the value of the FROM
               field.

       -       Is a reserved field and should always contain "-" for
               compatibility with older versions of zic.  It was previously
               known as the TYPE field, which could contain values to allow a
               separate script to further restrict in which "types" of years
               the rule would apply.

       IN      Names the month in which the rule takes effect.  Month names
               may be abbreviated.

       ON      Gives the day on which the rule takes effect.  Recognized forms
               include:

                    5        the fifth of the month
                    lastSun  the last Sunday in the month
                    lastMon  the last Monday in the month
                    Sun>=8   first Sunday on or after the eighth
                    Sun<=25  last Sunday on or before the 25th

               A weekday name (e.g., Sunday) or a weekday name preceded by
               "last" (e.g., lastSunday) may be abbreviated or spelled out in
               full.  There must be no white space characters within the ON
               field.  The "<=" and ">=" constructs can result in a day in the
               neighboring month; for example, the IN-ON combination "Oct
               Sun>=31" stands for the first Sunday on or after October 31,
               even if that Sunday occurs in November.

       AT      Gives the time of day at which the rule takes effect, relative
               to 00:00, the start of a calendar day.  Recognized forms
               include:

                    2            time in hours
                    2:00         time in hours and minutes
                    01:28:14     time in hours, minutes, and seconds
                    00:19:32.13  time with fractional seconds
                    12:00        midday, 12 hours after 00:00
                    15:00        3 PM, 15 hours after 00:00
                    24:00        end of day, 24 hours after 00:00
                    260:00       260 hours after 00:00
                    -2:30        2.5 hours before 00:00
                    -            equivalent to 0

               Although zic rounds times to the nearest integer second
               (breaking ties to the even integer), the fractions may be
               useful to other applications requiring greater precision.  The
               source format does not specify any maximum precision.  Any of
               these forms may be followed by the letter w if the given time
               is local or "wall clock" time, s if the given time is standard
               time without any adjustment for daylight saving, or u (or g or
               z) if the given time is universal time; in the absence of an
               indicator, local (wall clock) time is assumed.  These forms
               ignore leap seconds; for example, if a leap second occurs at
               00:59:60 local time, "1:00" stands for 3601 seconds after local
               midnight instead of the usual 3600 seconds.  The intent is that
               a rule line describes the instants when a clock/calendar set to
               the type of time specified in the AT field would show the
               specified date and time of day.

       SAVE    Gives the amount of time to be added to local standard time
               when the rule is in effect, and whether the resulting time is
               standard or daylight saving.  This field has the same format as
               the AT field except with a different set of suffix letters: s
               for standard time and d for daylight saving time.  The suffix
               letter is typically omitted, and defaults to s if the offset is
               zero and to d otherwise.  Negative offsets are allowed; in
               Ireland, for example, daylight saving time is observed in
               winter and has a negative offset relative to Irish Standard
               Time.  The offset is merely added to standard time; for
               example, zic does not distinguish a 10:30 standard time plus an
               0:30 SAVE from a 10:00 standard time plus a 1:00 SAVE.

       LETTER/S
               Gives the "variable part" (for example, the "S" or "D" in "EST"
               or "EDT") of time zone abbreviations to be used when this rule
               is in effect.  If this field is "-", the variable part is null.

       A zone line has the form

            Zone  NAME        STDOFF  RULES   FORMAT  [UNTIL]

       For example:

            Zone  Asia/Amman  2:00    Jordan  EE%sT   2017 Oct 27 01:00

       The fields that make up a zone line are:

       NAME  The name of the timezone.  This is the name used in creating the
             time conversion information file for the timezone.  It should not
             contain a file name component "." or ".."; a file name component
             is a maximal substring that does not contain "/".

       STDOFF
             The amount of time to add to UT to get standard time, without any
             adjustment for daylight saving.  This field has the same format
             as the AT and SAVE fields of rule lines, except without suffix
             letters; begin the field with a minus sign if time must be
             subtracted from UT.

       RULES The name of the rules that apply in the timezone or,
             alternatively, a field in the same format as a rule-line SAVE
             column, giving the amount of time to be added to local standard
             time and whether the resulting time is standard or daylight
             saving.  If this field is - then standard time always applies.
             When an amount of time is given, only the sum of standard time
             and this amount matters.

       FORMAT
             The format for time zone abbreviations.  The pair of characters
             %s is used to show where the "variable part" of the time zone
             abbreviation goes.  Alternatively, a format can use the pair of
             characters %z to stand for the UT offset in the form +-hh,
             +-hhmm, or +-hhmmss, using the shortest form that does not lose
             information, where hh, mm, and ss are the hours, minutes, and
             seconds east (+) or west (-) of UT.  Alternatively, a slash (/)
             separates standard and daylight abbreviations.  To conform to
             POSIX, a time zone abbreviation should contain only alphanumeric
             ASCII characters, "+" and "-".  By convention, the time zone
             abbreviation "-00" is a placeholder that means local time is
             unspecified.

       UNTIL The time at which the UT offset or the rule(s) change for a
             location.  It takes the form of one to four fields YEAR [MONTH
             [DAY [TIME]]].  If this is specified, the time zone information
             is generated from the given UT offset and rule change until the
             time specified, which is interpreted using the rules in effect
             just before the transition.  The month, day, and time of day have
             the same format as the IN, ON, and AT fields of a rule; trailing
             fields can be omitted, and default to the earliest possible value
             for the missing fields.

             The next line must be a "continuation" line; this has the same
             form as a zone line except that the string "Zone" and the name
             are omitted, as the continuation line will place information
             starting at the time specified as the "until" information in the
             previous line in the file used by the previous line.
             Continuation lines may contain "until" information, just as zone
             lines do, indicating that the next line is a further
             continuation.

       A link line has the form

            Link  TARGET           LINK-NAME

       For example:

            Link  Europe/Istanbul  Asia/Istanbul

       The TARGET field should appear as the NAME field in some zone line or
       as the LINK-NAME field in some link line.  The LINK-NAME field is used
       as an alternative name for that zone; it has the same syntax as a zone
       line's NAME field.  Links can chain together, although the behavior is
       unspecified if a chain of one or more links does not terminate in a
       Zone name.  A link line can appear before the line that defines the
       link target.  For example:

         Link  Greenwich  G_M_T
         Link  Etc/GMT    Greenwich
         Zone  Etc/GMT  0  -  GMT

       The two links are chained together, and G_M_T, Greenwich, and Etc/GMT
       all name the same zone.

       Except for continuation lines, lines may appear in any order in the
       input.  However, the behavior is unspecified if multiple zone or link
       lines define the same name.

       The file that describes leap seconds can have leap lines and an
       expiration line.  Leap lines have the following form:

            Leap  YEAR  MONTH  DAY  HH:MM:SS  CORR  R/S

       For example:

            Leap  2016  Dec    31   23:59:60  +     S

       The YEAR, MONTH, DAY, and HH:MM:SS fields tell when the leap second
       happened.  The CORR field should be "+" if a second was added or "-" if
       a second was skipped.  The R/S field should be (an abbreviation of)
       "Stationary" if the leap second time given by the other fields should
       be interpreted as UTC or (an abbreviation of) "Rolling" if the leap
       second time given by the other fields should be interpreted as local
       (wall clock) time.