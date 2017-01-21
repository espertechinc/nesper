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

namespace com.espertech.esper.epl.datetime.calop
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
        public static string[] GetNames(this CalendarFieldEnum @enum)
        {
            switch(@enum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return new[] {"msec", "millisecond", "milliseconds"};
                case CalendarFieldEnum.SECOND:
                    return new[] {"sec", "second", "seconds"};
                case CalendarFieldEnum.MINUTE:
                    return new[] {"min", "minute", "minutes"};
                case CalendarFieldEnum.HOUR:
                    return new[] {"hour", "hours"};
                case CalendarFieldEnum.DAY:
                    return new[] {"day", "days"};
                case CalendarFieldEnum.MONTH:
                    return new[] {"month", "months"};
                case CalendarFieldEnum.WEEK:
                    return new[] {"week", "weeks"};
                case CalendarFieldEnum.YEAR:
                    return new[] {"year", "years"};
            }

            throw new ArgumentException("invalid value", "enum");
        }

        public static string GetValidList(this CalendarFieldEnum @enum)
        {
            var builder = new StringBuilder();
            var delimiter = "";

            var values = Enum.GetValues(typeof (CalendarFieldEnum));
            foreach (CalendarFieldEnum value in values)
            {
                foreach (String name in GetNames(value))
                {
                    builder.Append(delimiter);
                    builder.Append(name);
                    delimiter = ",";
                }
            }

            return builder.ToString();
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
    }
}
