///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace com.espertech.esper.compat.datetime
{
    public static class DateTimeParsingFunctions
    {
        public static long ParseDefaultMSec(string dateTimeString)
        {
            var utcDate = ParseDefaultEx(dateTimeString);
            return utcDate.UtcMillis;
        }

        public static DateTimeOffset ParseDefaultDateTimeOffset(string dateTimeString)
        {
            return ParseDefaultEx(dateTimeString).DateTime;
        }

        public static DateTimeOffset ParseDefault(string dateTimeString)
        {
            return ParseDefaultEx(dateTimeString).DateTime;
        }

        public static DateTimeEx ParseIso8601Ex(string dateTimeString)
        {
            //var timeZone = dateTimeString.EndsWith("Z") ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            var timeZone = TimeZoneInfo.Utc;
            if (DateTimeOffset.TryParseExact(dateTimeString, "s", null, DateTimeStyles.None, out var dateTime)) {
                return DateTimeEx.GetInstance(timeZone, dateTime);
            }

            throw new ArgumentException(nameof(dateTimeString));
        }

        public static DateTimeEx ParseDefaultEx(string dateTimeString)
        {
            DateTimeOffset dateTime;

            var timeZone = TimeZoneInfo.Utc;
            var provider = CultureInfo.InvariantCulture;
            var dateTimeInputs = new List<string>();

            var match = Regex.Match(
                dateTimeString,
                @"^(\d{1,4}-\d{1,2}-\d{1,2})[T ](\d{1,2}:\d{1,2}:\d{1,2})(\.\d{1,4}|)(.*)$");
            if (match != Match.Empty) {
                var matchDate = Regex.Match(match.Groups[1].Value, @"(\d{1,4})-(\d{1,2})-(\d{1,2})");
                var rwDate = string.Format(
                    "{0}-{1}-{2}",
                    int.Parse(matchDate.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'),
                    int.Parse(matchDate.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchDate.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));

                var matchTime = Regex.Match(match.Groups[2].Value, @"(\d{1,2}):(\d{1,2}):(\d{1,2})");
                var rwTime = string.Format(
                    "{0}:{1}:{2}",
                    int.Parse(matchTime.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchTime.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(matchTime.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));

                provider = CultureInfo.InvariantCulture;

                var dateTimeText = rwDate + ' ' + rwTime + match.Groups[3].Value;

                // quick rewrite
                dateTimeString = rwDate + ' ' + rwTime + match.Groups[3].Value;
                if (match.Groups[4].Value == "Z") {
                    dateTimeString += "Z"; // UTC
                }
                else {
                    dateTimeString += match.Groups[4].Value;
                }

                dateTimeInputs.Add(dateTimeString);
                dateTimeInputs.Add(dateTimeText);

                timeZone = TimeZoneHelper.GetTimeZoneInfoOrDefault(match.Groups[4].Value);
            }
            else {
                dateTimeInputs.Add(dateTimeString);
            }

            string[] zoneFormats = {
                "yyyy-MM-dd HH:mm:ss.ffffZ",
                "yyyy-MM-dd HH:mm:ss.fffZ",
                "yyyy-MM-dd HH:mm:ss.ffZ",
                "yyyy-MM-dd HH:mm:ss.fZ",
                "yyyy-MM-dd HH:mm:ssZ",
                "yyyy-MM-dd HH:mm:ss.ffffzzz",
                "yyyy-MM-dd HH:mm:ss.fffzzz",
                "yyyy-MM-dd HH:mm:ss.ffzzz",
                "yyyy-MM-dd HH:mm:ss.fzzz",
                "yyyy-MM-dd HH:mm:sszzz",
            };

            string[] nonZoneFormats = {
                "yyyy-MM-dd HH:mm:ss.ffff",
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss.ff",
                "yyyy-MM-dd HH:mm:ss.f",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "HH:mm:ss"
            };

            foreach (var dateTimeInput in dateTimeInputs) {
                // Full-parse, including zone information
                if (DateTimeOffset.TryParseExact(
                    dateTimeInput,
                    zoneFormats,
                    provider,
                    DateTimeStyles.AllowWhiteSpaces,
                    out dateTime)) {
                    return new DateTimeEx(dateTime, timeZone);
                }

                // Partial-parse, missing zone information
                if (DateTimeOffset.TryParseExact(
                    dateTimeInput,
                    nonZoneFormats,
                    provider,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                    out dateTime)) {
                    return new DateTimeEx(dateTime, TimeZoneInfo.Utc);
                }
            }

            // Unable to parse, throw an exception
            throw new ArgumentException("unable to parse value", nameof(dateTimeString));
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
                        : TimeZoneInfo.Utc;
                    return new DateTimeEx(dateTime, timeZone);
                }

                if ((DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ffff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.fff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTimeOffset.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ff", provider, DateTimeStyles.None, out dateTime)))
                {
                    var timeZoneText = match.Groups[3].Value;
                    var timeZone = (timeZoneText != string.Empty)
                        ? TimeZoneHelper.GetTimeZoneInfo(timeZoneText)
                        : TimeZoneInfo.Utc;
                    return new DateTimeEx(dateTime, timeZone);
                }
            }

            var timeZoneEx = TimeZoneInfo.Utc;
            //var timeZoneEx = dateTimeWithZone.EndsWith("Z") ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            return new DateTimeEx(DateTimeOffset.Parse(dateTimeWithZone), timeZoneEx);
        }

        public static long ParseDefaultMSecWZone(string dateTimeWithZone)
        {
            return ParseDefaultExWZone(dateTimeWithZone).UtcMillis;
        }
    }
}
