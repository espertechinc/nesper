///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Text;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public enum CalendarFieldEnum
    {
        MILLISEC,
        SECOND,
        MINUTE,
        HOUR,
        DAY,
        MONTH,
        WEEK,
        YEAR
    }

    public static class CalendarFieldEnumExtensions
    {
        public static int GetCalendarField(this CalendarFieldEnum @enum)
        {
            switch (@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return DateTimeFieldEnum.MILLISEC;
                case CalendarFieldEnum.SECOND:
                    return DateTimeFieldEnum.SECOND;
                case CalendarFieldEnum.MINUTE:
                    return DateTimeFieldEnum.MINUTE;
                case CalendarFieldEnum.HOUR:
                    return DateTimeFieldEnum.HOUR;
                case CalendarFieldEnum.DAY:
                    return DateTimeFieldEnum.DAY;
                case CalendarFieldEnum.MONTH:
                    return DateTimeFieldEnum.MONTH;
                case CalendarFieldEnum.WEEK:
                    return DateTimeFieldEnum.WEEK;
                case CalendarFieldEnum.YEAR:
                    return DateTimeFieldEnum.YEAR;
            }

            throw new ArgumentException("invalid value", "enum");
        }

        public static string[] GetNames(this CalendarFieldEnum @enum)
        {
            switch (@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return new[] { "msec", "millisecond", "milliseconds" };
                case CalendarFieldEnum.SECOND:
                    return new[] { "sec", "second", "seconds" };
                case CalendarFieldEnum.MINUTE:
                    return new[] { "min", "minute", "minutes" };
                case CalendarFieldEnum.HOUR:
                    return new[] { "hour", "hours" };
                case CalendarFieldEnum.DAY:
                    return new[] { "day", "days" };
                case CalendarFieldEnum.MONTH:
                    return new[] { "month", "months" };
                case CalendarFieldEnum.WEEK:
                    return new[] { "week", "weeks" };
                case CalendarFieldEnum.YEAR:
                    return new[] { "year", "years" };
            }

            throw new ArgumentException("invalid value", "enum");
        }

        public static string ValidList {
            get {
                var builder = new StringBuilder();
                var delimiter = "";

                var values = Enum.GetValues(typeof(CalendarFieldEnum));
                foreach (CalendarFieldEnum value in values) {
                    foreach (String name in GetNames(value)) {
                        builder.Append(delimiter);
                        builder.Append(name);
                        delimiter = ",";
                    }
                }

                return builder.ToString();
            }
        }

        public static CalendarFieldEnum FromString(string field)
        {
            string compareTo = field.Trim().ToLower();

            var values = Enum.GetValues(typeof(CalendarFieldEnum));
            foreach (CalendarFieldEnum value in values)
            {
                if (GetNames(value).Any(name => name == field))
                {
                    return value;
                }
            }

            throw new ArgumentException("value not found", "field");
        }

        public static int ToDateTimeFieldEnum(this CalendarFieldEnum @enum)
        {
            switch (@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return DateTimeFieldEnum.MILLISEC;
                case CalendarFieldEnum.SECOND:
                    return DateTimeFieldEnum.SECOND;
                case CalendarFieldEnum.MINUTE:
                    return DateTimeFieldEnum.MINUTE;
                case CalendarFieldEnum.HOUR:
                    return DateTimeFieldEnum.HOUR;
                case CalendarFieldEnum.DAY:
                    return DateTimeFieldEnum.DAY;
                case CalendarFieldEnum.MONTH:
                    return DateTimeFieldEnum.MONTH;
                case CalendarFieldEnum.WEEK:
                    return DateTimeFieldEnum.WEEK;
                case CalendarFieldEnum.YEAR:
                    return DateTimeFieldEnum.YEAR;
            }

            throw new ArgumentException("invalid value", "enum");
        }

        public static ChronoUnit GetChronoUnit(this CalendarFieldEnum @enum)
        {
            switch (@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return ChronoUnit.MILLIS;
                case CalendarFieldEnum.SECOND:
                    return ChronoUnit.SECONDS;
                case CalendarFieldEnum.MINUTE:
                    return ChronoUnit.MINUTES;
                case CalendarFieldEnum.HOUR:
                    return ChronoUnit.HOURS;
                case CalendarFieldEnum.DAY:
                    return ChronoUnit.DAYS;
                case CalendarFieldEnum.MONTH:
                    return ChronoUnit.MONTHS;
                case CalendarFieldEnum.WEEK:
                    return ChronoUnit.WEEKS;
                case CalendarFieldEnum.YEAR:
                    return ChronoUnit.YEARS;
            }

            throw new ArgumentException("invalid value", "enum");
        }

        public static ChronoField GetChronoField(this CalendarFieldEnum @enum)
        {
            switch (@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return ChronoField.MILLI_OF_SECOND;
                case CalendarFieldEnum.SECOND:
                    return ChronoField.SECOND_OF_MINUTE;
                case CalendarFieldEnum.MINUTE:
                    return ChronoField.MINUTE_OF_HOUR;
                case CalendarFieldEnum.HOUR:
                    return ChronoField.HOUR_OF_DAY;
                case CalendarFieldEnum.DAY:
                    return ChronoField.DAY_OF_MONTH;
                case CalendarFieldEnum.MONTH:
                    return ChronoField.MONTH_OF_YEAR;
                case CalendarFieldEnum.WEEK:
                    return ChronoField.ALIGNED_WEEK_OF_YEAR;
                case CalendarFieldEnum.YEAR:
                    return ChronoField.YEAR;
            }

            throw new ArgumentException("invalid value", "enum");
        }
    }
}