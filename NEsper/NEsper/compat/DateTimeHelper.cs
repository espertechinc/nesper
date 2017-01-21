///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
        /// Gets the number of nanoseconds needed to represent
        /// the datetime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long UtcNanos(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return TicksToNanos(dateTime.Ticks) - (DateTimeConstants.Boundary * 100000);
            else if (dateTime.Kind == DateTimeKind.Local)
                return TicksToNanos(dateTime.ToUniversalTime().Ticks) - (DateTimeConstants.Boundary * 1000000);

            throw new ArgumentException("dateTime does not have kind specified");
        }

        /// <summary>
        /// Gets the number of milliseconds needed to represent
        /// the datetime.  This is needed to convert from Java
        /// datetime granularity (milliseconds) to CLR datetimes.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>

        public static long UtcMillis(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return TicksToMillis(dateTime.Ticks) - DateTimeConstants.Boundary;
            else if (dateTime.Kind == DateTimeKind.Local)
                return TicksToMillis(dateTime.ToUniversalTime().Ticks) - DateTimeConstants.Boundary;

            throw new ArgumentException("dateTime does not have kind specified");
        }

        public static DateTime UtcFromMillis(this long millis)
        {
            return new DateTime(MillisToTicks(millis + DateTimeConstants.Boundary), DateTimeKind.Utc);
        }

        public static DateTime FromMillis(this long millis)
        {
            return UtcFromMillis(millis).ToLocalTime();
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
            return UtcMillis(DateTime.UtcNow);
        }

        /// <summary>
        /// Returns the current time in millis
        /// </summary>

        public static long CurrentTimeMillis
        {
            get { return UtcMillis(DateTime.UtcNow); }
        }

        /// <summary>
        /// Gets the current time in nanoseconds.
        /// </summary>
        /// <value>The current time nanos.</value>
        public static long CurrentTimeNanos
        {
            get { return UtcNanos(DateTime.UtcNow); }
        }

        public static DateTimeOffset TranslateTo(this DateTime dateTime, TimeZoneInfo timeZone)
        {
            if (timeZone == null)
                return dateTime;
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), timeZone);
        }

        public static string Print(this long timeInMillis, TimeZoneInfo timeZoneInfo = null)
        {
            if (timeZoneInfo == null)
                timeZoneInfo = TimeZoneInfo.Local;

            return Print(DateTimeOffsetHelper.TimeFromMillis(timeInMillis, timeZoneInfo));
        }

        public static string Print(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }

        public static string Print(this DateTimeOffset dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }

        public static String PrintWithZone(this DateTimeOffset date)
        {
            return Print(date, "yyyy-MM-dd HH:mm:ss.fff%K");
        }

        public static string ToShortDateString(this DateTimeOffset dateTime)
        {
            return dateTime.Date.ToShortDateString();
        }

        public static string ToShortTimeString(this DateTimeOffset dateTime)
        {
            return dateTime.Date.ToShortTimeString();
        }
    }
}
