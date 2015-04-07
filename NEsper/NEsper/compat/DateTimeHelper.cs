///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Assistant class to help with conversions between Java-style and
    /// granularity dates and CLR-style DateTime.
    /// </summary>

    public static class DateTimeHelper
    {
        /// <summary>
        /// Number of ticks per millisecond
        /// </summary>

        public const int TICKS_PER_MILLI = 10000;

        /// <summary>
        /// Number of nanoseconds per tick
        /// </summary>

        public const int NANOS_PER_TICK = 100;

        /// <summary>
        /// Converts ticks to milliseconds
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>

        public static long TicksToMillis(long ticks)
        {
            return ticks / TICKS_PER_MILLI;
        }

        /// <summary>
        /// Converts ticks to nanoseconds
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>

        public static long TicksToNanos(long ticks)
        {
            return ticks * NANOS_PER_TICK;
        }

        /// <summary>
        /// Converts milliseconds to ticks
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>

        public static long MillisToTicks(long millis)
        {
            return millis * TICKS_PER_MILLI;
        }

        /// <summary>
        /// Nanoses to ticks.
        /// </summary>
        /// <param name="nanos">The nanos.</param>
        public static long NanosToTicks(long nanos)
        {
            return nanos / NANOS_PER_TICK;
        }

        /// <summary>
        /// Converts milliseconds to DateTime 
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <returns></returns>
        public static DateTime MillisToDateTime(long millis)
        {
            return new DateTime(MillisToTicks(millis));
        }

        /// <summary>
        /// Gets the number of nanoseconds needed to represent
        /// the datetime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long TimeInNanos(DateTime dateTime)
        {
            return TicksToNanos(dateTime.Ticks);
        }

        /// <summary>
        /// Gets the number of nanoseconds.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long InNanos(this DateTime dateTime)
        {
            return TicksToNanos(dateTime.Ticks);
        }

        /// <summary>
        /// Gets the number of milliseconds needed to represent
        /// the datetime.  This is needed to convert from Java
        /// datetime granularity (milliseconds) to CLR datetimes.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>

        public static long TimeInMillis(this DateTime dateTime)
        {
            return TicksToMillis(dateTime.Ticks);
        }

        /// <summary>
        /// Gets the number of milliseconds needed to represent
        /// the datetime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long InMillis(this DateTime dateTime)
        {
            return TicksToMillis(dateTime.Ticks);
        }

        /// <summary>
        /// Gets the datetime that matches the number of milliseconds provided.
        /// As with TimeInMillis, this is needed to convert from Java datetime
        /// granularity to CLR granularity.
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>

        public static DateTime TimeFromMillis(this long millis)
        {
            return new DateTime(MillisToTicks(millis));
        }

        public static DateTime GetCurrentTimeUniversal()
        {
            return DateTime.UtcNow;
        }

        public static DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Returns the current time in millis
        /// </summary>

        public static long GetCurrentTimeMillis()
        {
            return DateTime.Now.Ticks / TICKS_PER_MILLI;
        }

        /// <summary>
        /// Returns the current time in millis
        /// </summary>

        public static long CurrentTimeMillis
        {
            get { return TimeInMillis(DateTime.Now); }
        }

        /// <summary>
        /// Gets the current time in nanoseconds.
        /// </summary>
        /// <value>The current time nanos.</value>
        public static long CurrentTimeNanos
        {
            get { return TimeInNanos(DateTime.Now); }
        }

        /// <summary>
        /// Converts millis in CLR to millis in Java
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>

        public static long MillisToJavaMillis(long millis)
        {
            return millis - 62135575200000L;
        }

        /// <summary>
        /// Converts milliseconds in Java to milliseconds in CLR
        /// </summary>
        /// <param name="javaMillis"></param>
        /// <returns></returns>

        public static long JavaMillisToMillis(long javaMillis)
        {
            return javaMillis + 62135575200000L;
        }

        /// <summary>
        /// Returns a datetime for the week given on the specified day of the week.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <returns></returns>
        public static DateTime MoveToDayOfWeek( DateTime from, DayOfWeek dayOfWeek )
        {
            DayOfWeek current = from.DayOfWeek;
            return from.AddDays(dayOfWeek - current);    
        }

        /// <summary>
        /// Returns a datetime for the end of the month.
        /// </summary>
        /// <param name="from">From.</param>
        /// <returns></returns>
        public static DateTime EndOfMonth(DateTime from)
        {
            return new DateTime(
                from.Year,
                from.Month,
                DateTime.DaysInMonth(from.Year, from.Month),
                0,
                0,
                0);
        }

        public static DateTime ToLocalTime( DateTime source )
        {
            return source.ToLocalTime();
        }

        public static DateTime ToUniversalTime( DateTime source )
        {
            return source.ToUniversalTime();
        }

        public static DateTime ToTimeZone( DateTime source, string targetTimeZone )
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, targetTimeZone);
        }

        public static DateTime ToTimeZone(DateTime source, string sourceTimeZone, string targetTimeZone)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, sourceTimeZone, targetTimeZone);
        }

        private static readonly Calendar Calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

        public static int GetWeekOfYear(this DateTime dateTime)
        {
            return Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime GetWithMaximumDay(this DateTime dateTime)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth, dateTime.Hour, dateTime.Minute,
                                dateTime.Second, dateTime.Millisecond);
        }

        public static DateTime GetWithMaximumMonth(this DateTime dateTime)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, 12);
            if (dateTime.Day < daysInMonth)
                daysInMonth = dateTime.Day;

            return new DateTime(dateTime.Year, 12, daysInMonth, dateTime.Hour, dateTime.Minute,
                                dateTime.Second, dateTime.Millisecond);
        }

        public static DateTime MoveToWeek(this DateTime dateTime, int targetWeek)
        {
            if ((targetWeek < 1) || (targetWeek > 52))
                throw new ArgumentException("invalid target week", "targetWeek");

            var week = GetWeekOfYear(dateTime);
            if (week == targetWeek)
                return dateTime;
            for (; week > targetWeek; week = GetWeekOfYear(dateTime))
                dateTime = dateTime.AddDays(-7);
            for (; week < targetWeek; week = GetWeekOfYear(dateTime)) 
                dateTime = dateTime.AddDays(7);

            return dateTime;
        }

        public static DateTime GetWithMaximumWeek(this DateTime dateTime)
        {
            do
            {
                var nextTime = dateTime.AddDays(7);
                if ((GetWeekOfYear(dateTime) > 2) && (GetWeekOfYear(nextTime) <= 2))
                {
                    return dateTime;
                }

                dateTime = nextTime;
            } while (true);
        }

        public static DateTime GetWithMinimumWeek(this DateTime dateTime)
        {
            var week = GetWeekOfYear(dateTime);
            if (week == 1)
            {
                return dateTime;
            }

            do
            {
                var nextTime = dateTime.AddDays(-7);
                if (GetWeekOfYear(dateTime) == 2)
                {
                    // See if this day in the previous week is still in week 1.  It's
                    // possible that a week started with a day like Friday and that the
                    // date in question was a Thursday.  Technically, Thursday would
                    // have begun on week 2 not 1.
                    if (GetWeekOfYear(nextTime) == 1)
                        return nextTime;
                    // First occurrence of this date occurred on week 2
                    return dateTime;
                }

                dateTime = nextTime;
            } while (true);
        }

        public static DateTime ParseDefaultDate(string dateTimeString)
        {
            return ParseDefault(dateTimeString);
        }

        public static DateTime ParseDefault(string dateTimeString)
        {
            DateTime dateTime;

            var match = Regex.Match(dateTimeString, @"^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)\.(\d+)$");
            if (match != Match.Empty)
            {
                dateTimeString = string.Format(
                    "{0}-{1}-{2} {3}:{4}:{5}.{6}",
                    int.Parse(match.Groups[1].Value).ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'),
                    int.Parse(match.Groups[2].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(match.Groups[3].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(match.Groups[4].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(match.Groups[5].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    int.Parse(match.Groups[6].Value).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                    match.Groups[7].Value);
            }

            if ((DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.fff", null, DateTimeStyles.None, out dateTime)) ||
                (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ff", null, DateTimeStyles.None, out dateTime)))
                return dateTime;

            // there is an odd situation where we intend to parse down to milliseconds but someone passes a four digit value
            // - in this case, Java interprets this as a millisecond value but the CLR will interpret this as a tenth of a
            // - millisecond value.  to be consistent, I've made our implementation behave in a fashion similar to the java
            // - implementation.

            if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ffff", null, DateTimeStyles.None, out dateTime))
            {
                var millis = (dateTime.Ticks%10000000)/1000;
                dateTime = dateTime.AddMilliseconds(-millis / 10).AddMilliseconds(millis);
                return dateTime;
            }

            return DateTime.Parse(dateTimeString);
        }

        public static DateTime ParseDefaultWZone(string dateTimeWithZone)
        {
            var match = Regex.Match(dateTimeWithZone, @"^(\d{1,4}-\d{1,2}-\d{1,2})[T ](\d{1,2}:\d{1,2}:\d{1,2})(\.\d{1,4}|)(.*)$");
            if (match != Match.Empty)
            {
                var provider = System.Globalization.CultureInfo.InvariantCulture;
                var dateTimeText = match.Groups[1].Value + ' ' + match.Groups[2].Value + match.Groups[3].Value;

                DateTime dateTime;

                // quick rewrite
                dateTimeWithZone = match.Groups[1].Value + ' ' + match.Groups[2].Value + match.Groups[3].Value + match.Groups[4].Value;
                if ((DateTime.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.ffff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTime.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.fff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTime.TryParseExact(dateTimeWithZone, "yyyy-MM-dd HH:mm:ss.ff'GMT'zzz", provider, DateTimeStyles.None, out dateTime)))
                {
                    return dateTime;
                }

                if ((DateTime.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ffff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTime.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.fff", provider, DateTimeStyles.None, out dateTime)) ||
                    (DateTime.TryParseExact(dateTimeText, "yyyy-MM-dd HH:mm:ss.ff", provider, DateTimeStyles.None, out dateTime)))
                {
                    var timeZoneText = match.Groups[3].Value;
                    if (timeZoneText != string.Empty)
                    {
                        var timeZoneInfo = TimeZoneInfo.FromSerializedString(timeZoneText);
                        dateTime = TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo, TimeZoneInfo.Local);
                    }

                    return dateTime;
                }
            }

            return DateTime.Parse(dateTimeWithZone);
        }

        public static long ParseDefaultMSec(string dateTimeString)
        {
            return TimeInMillis(ParseDefault(dateTimeString));
        }

        public static long ParseDefaultMSecWZone(string dateTimeWithZone)
        {
            return TimeInMillis(ParseDefaultWZone(dateTimeWithZone));
        }

        public static string Print(this long timeInMillis)
        {
            return Print(TimeFromMillis(timeInMillis));
        }

        public static string Print(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }

        public static String PrintWithZone(this DateTime date)
        {
            return Print(date, "yyyy-MM-dd HH:mm:ss.fff%K");
        }
    }
}
