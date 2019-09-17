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

    public static class DateTimeOffsetHelper
    {
        private static readonly TimeSpan BaseUtcOffset = TimeZoneInfo.Utc.BaseUtcOffset;

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
        public static long UtcMillis(this DateTimeOffset dateTime)
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
            timeZone = timeZone ?? TimeZoneInfo.Utc;
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
            {
                return dateTime;
            }

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
            timeZone = timeZone ?? TimeZoneInfo.Utc;
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
        }

        // --------------------------------------------------------------------------------

        public static DateTimeOffset WithYear(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(value, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithMonth(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, value, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithDay(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, dto.Month, value, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithHour(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, dto.Month, dto.Day, value, dto.Minute, dto.Second, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithMinute(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, value, dto.Second, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithSecond(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, value, dto.Millisecond, dto.Offset);
        }

        public static DateTimeOffset WithMilli(
            this DateTimeOffset dto,
            int value)
        {
            return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, value, dto.Offset);
        }

        public static DateTimeOffset With(this DateTimeOffset dt, DateTimeFieldEnum field, int value)
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

        public static DateTimeOffset TruncatedTo(this DateTimeOffset dto, DateTimeFieldEnum field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return dto;
                case DateTimeFieldEnum.SECOND:
                    return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, 0, dto.Offset);
                case DateTimeFieldEnum.MINUTE:
                    return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, 0, 0, dto.Offset);
                case DateTimeFieldEnum.HOUR:
                    return new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, 0, 0, 0, dto.Offset);
                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                    return new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, 0, dto.Offset);
                case DateTimeFieldEnum.MONTH:
                    return new DateTimeOffset(dto.Year, dto.Month, 1, 0, 0, 0, 0, dto.Offset);
                case DateTimeFieldEnum.YEAR:
                    return new DateTimeOffset(dto.Year, 1, 1, 0, 0, 0, 0, dto.Offset);
                default:
                    throw new NotSupportedException();
            }
        }

        // --------------------------------------------------------------------------------

        public static ValueRange<int> Range(
            this DateTimeOffset dt,
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
                    return new ValueRange<int>(0, int.MaxValue);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}