///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.datetime;

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
        /// Number of ticks per microsecond
        /// </summary>

        public const int TICKS_PER_MICRO = 10;

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
        /// Converts microseconds to ticks
        /// </summary>
        /// <param name="micros"></param>
        /// <returns></returns>

        public static long MicrosToTicks(long micros)
        {
            return micros * TICKS_PER_MICRO;
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
            if (dateTime.Kind == DateTimeKind.Utc) {
                return TicksToMillis(dateTime.Ticks) - DateTimeConstants.Boundary;
            }
            else if (dateTime.Kind == DateTimeKind.Local) {
                return TicksToMillis(dateTime.ToUniversalTime().Ticks) - DateTimeConstants.Boundary;
            }
            else {
                // We interpret any datetime that does not have this specified as assumed UTC.
                return TicksToMillis(dateTime.Ticks) - DateTimeConstants.Boundary;
            }
        }

        public static DateTime UtcFromMillis(this long millis)
        {
            return new DateTime(MillisToTicks(millis + DateTimeConstants.Boundary), DateTimeKind.Utc);
        }

        public static DateTime UtcFromMicros(this long micros)
        {
            return new DateTime(MicrosToTicks(micros + DateTimeConstants.Boundary * 1000), DateTimeKind.Utc);
        }

        public static DateTime TimeFromMillis(this long millis)
        {
            return UtcFromMillis(millis).ToLocalTime();
        }

        public static DateTime FromMicros(this long micros)
        {
            return UtcFromMicros(micros).ToLocalTime();
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
            if (timeZoneInfo == null) {
                timeZoneInfo = TimeZoneInfo.Utc;
            }

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

        // --------------------------------------------------------------------------------

        public static DateTime WithYear(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                value,
                dt.Month,
                dt.Day,
                dt.Hour,
                dt.Minute,
                dt.Second,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithMonth(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                value,
                dt.Day,
                dt.Hour,
                dt.Minute,
                dt.Second,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithDay(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                value,
                dt.Hour,
                dt.Minute,
                dt.Second,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithHour(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                value,
                dt.Minute,
                dt.Second,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithMinute(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                dt.Hour,
                value,
                dt.Second,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithSecond(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                dt.Hour,
                dt.Minute,
                value,
                dt.Millisecond,
                dt.Kind);
        }

        public static DateTime WithMilli(
            this DateTime dt,
            int value)
        {
            return new DateTime(
                dt.Year,
                dt.Month,
                dt.Day,
                dt.Hour,
                dt.Minute,
                dt.Second,
                value,
                dt.Kind);
        }

        public static DateTime With(this DateTime dt, DateTimeFieldEnum field, int value)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return WithMilli(dt, value);
                case DateTimeFieldEnum.SECOND:
                    return WithSecond(dt, value);
                case DateTimeFieldEnum.MINUTE:
                    return WithMinute(dt, value);
                case DateTimeFieldEnum.HOUR:
                    return WithHour(dt, value);
                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                    return WithDay(dt, value);
                case DateTimeFieldEnum.MONTH:
                    return WithMonth(dt, value);
                case DateTimeFieldEnum.YEAR:
                    return WithYear(dt, value);
                default:
                    throw new NotSupportedException();
            }
        }

        // --------------------------------------------------------------------------------

        public static DateTime TruncatedTo(this DateTime dt, DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC:
                    return dt;

                case DateTimeFieldEnum.SECOND:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        dt.Day,
                        dt.Hour,
                        dt.Minute,
                        dt.Second,
                        0,
                        dt.Kind);

                case DateTimeFieldEnum.MINUTE:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        dt.Day,
                        dt.Hour,
                        dt.Minute,
                        0,
                        0,
                        dt.Kind);

                case DateTimeFieldEnum.HOUR:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        dt.Day,
                        dt.Hour,
                        0,
                        0,
                        0,
                        dt.Kind);

                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        dt.Day,
                        0,
                        0,
                        0,
                        0,
                        dt.Kind);

                case DateTimeFieldEnum.MONTH:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        1,
                        0,
                        0,
                        0,
                        0,
                        dt.Kind);

                case DateTimeFieldEnum.YEAR:
                    return new DateTime(
                        dt.Year,
                        1,
                        1,
                        0,
                        0,
                        0,
                        0,
                        dt.Kind);

                default:
                    throw new NotSupportedException();
            }
        }

        // --------------------------------------------------------------------------------

        public static ValueRange<int> Range(
            this DateTime dt,
            DateTimeFieldEnum field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return new ValueRange<int>(0, 999);
                case DateTimeFieldEnum.SECOND:
                    return new ValueRange<int>(0, 59);
                case DateTimeFieldEnum.MINUTE:
                    return new ValueRange<int>(0, 59);
                case DateTimeFieldEnum.HOUR:
                    return new ValueRange<int>(0, 23);
                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                    return new ValueRange<int>(1, DateTime.DaysInMonth(dt.Year, dt.Month));
                case DateTimeFieldEnum.MONTH:
                    return new ValueRange<int>(1, 12);
                case DateTimeFieldEnum.YEAR:
                    return new ValueRange<int>(
                        DateTime.MinValue.Year,
                        DateTime.MaxValue.Year);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
