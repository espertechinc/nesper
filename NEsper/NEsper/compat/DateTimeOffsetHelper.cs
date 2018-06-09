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

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Assistant class to help with conversions between Java-style and
    /// granularity dates and CLR-style DateTime.
    /// </summary>

    public static class DateTimeOffsetHelper
    {
        private static readonly TimeSpan BaseUtcOffset = TimeZoneInfo.Utc.BaseUtcOffset ;

        /// <summary>
        /// Converts millis in CLR to millis in Java.  In this case, it assumes that your
        /// millis are the result of a conversion from UtcTicks.
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>

        public static long MillisToJavaMillis(long millis)
        {
            return millis - 62135596800000L;
        }

        /// <summary>
        /// Converts milliseconds into a datetime offset.
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <returns></returns>
        public static DateTimeOffset MillisToDateTimeOffset(long millis)
        {
            return new DateTimeOffset(DateTimeHelper.MillisToTicks(millis + DateTimeConstants.Boundary), BaseUtcOffset);
        }

        /// <summary>
        /// Gets the number of milliseconds needed to represent the datetime.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long TimeInMillis(this DateTimeOffset dateTime)
        {
            return DateTimeHelper.TicksToMillis(dateTime.UtcTicks) - DateTimeConstants.Boundary;
        }

        /// <summary>
        /// Gets the number of milliseconds needed to represent
        /// the datetime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static long InMillis(this DateTimeOffset dateTime)
        {
            return DateTimeHelper.TicksToMillis(dateTime.UtcTicks) - DateTimeConstants.Boundary;
        }

        /// <summary>
        /// Gets the datetime that matches the number of milliseconds provided.
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static DateTimeOffset TimeFromMillis(this long millis, TimeSpan offset)
        {
            return new DateTimeOffset(DateTimeHelper.MillisToTicks(millis + DateTimeConstants.Boundary), BaseUtcOffset)
                .ToOffset(offset);
        }

        /// <summary>
        /// Gets the datetime that matches the number of milliseconds provided.
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public static DateTimeOffset TimeFromMillis(this long millis, TimeZoneInfo timeZone)
        {
            timeZone = timeZone ?? TimeZoneInfo.Local;
            var baseDateTime = new DateTimeOffset(DateTimeHelper.MillisToTicks(millis + DateTimeConstants.Boundary), BaseUtcOffset);
            var timeZoneOffset = timeZone.GetUtcOffset(baseDateTime);
            return baseDateTime.ToOffset(timeZoneOffset);
        }

        /// <summary>
        /// Creates the date time within the specified timezone
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public static DateTimeOffset CreateDateTime(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millisecond,
            TimeZoneInfo timeZone)
        {
            var dateTimeOffset = timeZone.GetUtcOffset(new DateTime(year, month, day, hour, minute, second, millisecond));
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, dateTimeOffset);
        }

        public static DateTimeOffset CreateDateTime(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millisecond,
            TimeSpan offset)
        {
            return new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
        }

        /// <summary>
        /// Normalizes the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        public static DateTimeOffset Normalize(this DateTimeOffset dateTime, TimeZoneInfo timeZone)
        {
            if (timeZone == null)
                return dateTime;
            return dateTime.ToOffset(timeZone.GetUtcOffset(dateTime));
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, TimeZoneInfo timeZone)
        {
            return new DateTimeOffset(dateTime, timeZone.GetUtcOffset(dateTime));
        }

        public static DateTimeOffset TranslateTo(this DateTimeOffset dateTime, TimeZoneInfo timeZone)
        {
            return timeZone == null ? dateTime : TimeZoneInfo.ConvertTime(dateTime, timeZone);
        }

        public static DateTimeOffset Now(TimeZoneInfo timeZone)
        {
            timeZone = timeZone ?? TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
        }
    }
}
