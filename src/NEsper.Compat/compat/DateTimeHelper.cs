///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

        public static DateTime UtcFromNanos(this long nanos)
        {
            return new DateTime(NanosToTicks(nanos + DateTimeConstants.Boundary * 1000000), DateTimeKind.Utc);
        }

        public static DateTime TimeFromMillis(this long millis)
        {
            return UtcFromMillis(millis).ToLocalTime();
        }

        public static DateTime TimeFromMicros(this long micros)
        {
            return UtcFromMicros(micros).ToLocalTime();
        }

        public static DateTime TimeFromNanos(this long nanos)
        {
            return UtcFromNanos(nanos).ToLocalTime();
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

        public static long CurrentTimeMillis => UtcMillis(DateTime.UtcNow);

        /// <summary>
        /// Gets the current time in nanoseconds.
        /// </summary>
        /// <value>The current time nanos.</value>
        public static long CurrentTimeNanos => UtcNanos(DateTime.UtcNow);

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

        public static DateTime Truncate(this DateTime dt, DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC:
                    return new DateTime(
                        dt.Year,
                        dt.Month,
                        dt.Day,
                        dt.Hour,
                        dt.Minute,
                        dt.Second,
                        dt.Millisecond,
                        dt.Kind);

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

        public static DateTime Round(this DateTime dt, DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC: {
                    dt = dt.AddTicks(5000);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
                }
                case DateTimeFieldEnum.SECOND: {
                    dt = dt.AddMilliseconds(500);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
                }

                case DateTimeFieldEnum.MINUTE: {
                    dt = dt.AddSeconds(30);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.HOUR: {
                    dt = dt.AddMinutes(30);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE: {
                    dt = dt.Add(TimeSpan.FromSeconds(43200));
                    return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.MONTH: {
                    var stMonth = dt.Month;
                    var stYear = dt.Year;
                    var stDate = new DateTime(stYear, stMonth, 1, 0, 0, 0, 0, dt.Kind);

                    var edMonth = stMonth == 12 ? 1 : stMonth + 1;
                    var edYear = stMonth == 12 ? stYear + 1 : stYear;
                    var edDate = new DateTime(edYear, edMonth, 1, 0, 0, 0, 0, dt.Kind);

                    var ticksInHalfMonth = (edDate.Ticks - stDate.Ticks) / 2;

                    dt = dt.AddTicks(ticksInHalfMonth);
                    return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.YEAR: {
                    var stYear = dt.Year;
                    var stDate = new DateTime(stYear, 1, 1, 0, 0, 0, 0, dt.Kind);
                    var edDate = new DateTime(stYear + 1, 1, 1, 0, 0, 0, 0, dt.Kind);

                    var ticksInHalfYear = (edDate.Ticks - stDate.Ticks) / 2;

                    dt = dt.AddTicks(ticksInHalfYear);
                    return new DateTime(dt.Year, 1, 1, 0, 0, 0, 0, dt.Kind);
                }

                default:
                    throw new NotSupportedException();
            }
        }
        
        // --------------------------------------------------------------------------------

        public static DateTime Ceiling(this DateTime dt, DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC: {
                    dt = dt.AddTicks(9999);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
                }
                case DateTimeFieldEnum.SECOND: {
                    dt = dt.AddMilliseconds(999);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
                }

                case DateTimeFieldEnum.MINUTE: {
                    dt = dt.AddSeconds(59);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.HOUR: {
                    dt = dt.AddMinutes(59);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind);
                }

                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE: {
                    var baseDt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
                    if (baseDt == dt) {
                        return baseDt;
                    }

                    return baseDt.AddDays(1);
                }

                case DateTimeFieldEnum.MONTH: {
                    var baseDt = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Kind);
                    if (baseDt == dt) {
                        return baseDt;
                    }

                    return baseDt.AddMonths(1);
                }

                case DateTimeFieldEnum.YEAR: {
                    var baseDt = new DateTime(dt.Year, 1, 1, 0, 0, 0, 0, dt.Kind);
                    if (baseDt == dt) {
                        return baseDt;
                    }

                    return baseDt.AddYears(1);
                }

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
        
        // --------------------------------------------------------------------------------

        private static void Normalize(
            ref int year,
            ref int month,
            ref int day,
            ref int hour,
            ref int minute,
            ref int second,
            ref int milliseconds)
        {
            if (milliseconds >= 1000) {
                second += milliseconds / 1000;
                milliseconds = milliseconds % 1000;
            }

            if (second >= 60) {
                minute += second / 60;
                second %= 60;
            }

            if (minute >= 60) {
                hour += minute / 60;
                minute %= 60;
            }

            if (hour >= 24) {
                day += hour / 24;
                hour %= 24;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            do {
                if (day > daysInMonth) {
                    month++;
                    day -= daysInMonth - 1;
                }
                daysInMonth = DateTime.DaysInMonth(year, month);
            } while (day > daysInMonth);
            
            throw new NotSupportedException();
        }
    }
}
