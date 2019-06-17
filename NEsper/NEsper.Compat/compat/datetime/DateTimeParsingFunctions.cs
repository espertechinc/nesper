///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace com.espertech.esper.compat.datetime
{
    public static class DateTimeParsingFunctions
    {
        public static long ParseDefaultMSec(string dateTimeString)
        {
            return ParseDefaultEx(dateTimeString).TimeInMillis;
        }

        public static DateTimeOffset ParseDefaultDate(string dateTimeString)
        {
            return ParseDefaultEx(dateTimeString).DateTime;
        }

        public static DateTimeOffset ParseDefault(string dateTimeString)
        {
            return ParseDefaultEx(dateTimeString).DateTime;
        }

        public static DateTimeEx ParseDefaultEx(string dateTimeString)
        {
            DateTimeOffset dateTime;

            var match = Regex.Match(dateTimeString, @"^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)\.(\d+)$");
            if (match != Match.Empty)
            {
                dateTimeString = String.Format(
                    "{0}-{1}-{2} {3}:{4}:{5}.{6}",
                    Int32.Parse(match.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'),
                    Int32.Parse(match.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    Int32.Parse(match.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    Int32.Parse(match.Groups[4].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    Int32.Parse(match.Groups[5].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    Int32.Parse(match.Groups[6].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    match.Groups[7].Value);
            }

            var timeZone = dateTimeString.EndsWith("Z") ? TimeZoneInfo.Utc : TimeZoneInfo.Local;

            if ((DateTimeOffset.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.fff", null, DateTimeStyles.None, out dateTime)) ||
                (DateTimeOffset.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ff", null, DateTimeStyles.None, out dateTime)))
                return new DateTimeEx(dateTime, timeZone);

            // there is an odd situation where we intend to parse down to milliseconds but someone passes a four digit value
            // - in this case, Java interprets this as a millisecond value but the CLR will interpret this as a tenth of a
            // - millisecond value.  to be consistent, I've made our implementation behave in a fashion similar to the java
            // - implementation.

            if (DateTimeOffset.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ffff", null, DateTimeStyles.None, out dateTime))
            {
                var millis = (dateTime.Ticks % 10000000) / 1000;
                dateTime = dateTime.AddMilliseconds(-millis / 10).AddMilliseconds(millis);
                return new DateTimeEx(dateTime, timeZone);
            }

            return new DateTimeEx(DateTimeOffset.Parse(dateTimeString), timeZone);
        }

        public static DateTimeEx ParseDefaultExWZone(string dateTimeWithZone)
        {
            var match = Regex.Match(dateTimeWithZone, @"^(\d{1,4}-\d{1,2}-\d{1,2})[T ](\d{1,2}:\d{1,2}:\d{1,2})(\.\d{1,4}|)(.*)$");
            if (match != Match.Empty)
            {
                var matchDate = Regex.Match(match.Groups[1].Value, @"(\d{1,4})-(\d{1,2})-(\d{1,2})");
                var rwDate = string.Format("{0}-{1}-{2}",
                    int.Parse(matchDate.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'),
                    int.Parse(matchDate.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchDate.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));

                var matchTime = Regex.Match(match.Groups[2].Value, @"(\d{1,2}):(\d{1,2}):(\d{1,2})");
                var rwTime = string.Format("{0}:{1}:{2}",
                    int.Parse(matchTime.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchTime.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchTime.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));

                var provider = CultureInfo.InvariantCulture;
                var dateTimeText = rwDate + ' ' + rwTime + match.Groups[3].Value;

                DateTimeOffset dateTime;

                // quick rewrite
                dateTimeWithZone = rwDate + ' ' + rwTime + match.Groups[3].Value + match.Groups[4].Value;
                if ((DateTimeOffset.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.ffff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.fff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.ff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)))
                {
                    var timeZoneText = match.Groups[4].Value;
                    var timeZone = (timeZoneText != string.Empty)
                        ? TimeZoneHelper.GetTimeZoneInfo(timeZoneText)
                        : TimeZoneInfo.Local;
                    return new DateTimeEx(dateTime, timeZone);
                }

                if ((DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ffff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.fff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ff", provider, DateTimeStyles.None, out dateTime)))
                {
                    var timeZoneText = match.Groups[3].Value;
                    var timeZone = (timeZoneText != string.Empty)
                        ? TimeZoneHelper.GetTimeZoneInfo(timeZoneText)
                        : TimeZoneInfo.Local;
                    return new DateTimeEx(dateTime, timeZone);
                }
            }

            var timeZoneEx = dateTimeWithZone.EndsWith("Z") ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            return new DateTimeEx(DateTimeOffset.Parse(dateTimeWithZone), timeZoneEx);
        }

        public static long ParseDefaultMSecWZone(string dateTimeWithZone)
        {
            return ParseDefaultExWZone(dateTimeWithZone).TimeInMillis;
        }
    }
}
