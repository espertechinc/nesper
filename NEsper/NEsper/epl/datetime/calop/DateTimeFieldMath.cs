///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.calop
{
    public static class DateTimeFieldMath
    {
        /// <summary>
        /// Gets the actual minimum.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetActualMinimum(this DateTime dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return 0;
                case DateTimeFieldEnum.SECOND:
                    return 0;
                case DateTimeFieldEnum.MINUTE:
                    return 0;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return 0;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return 1;
                case DateTimeFieldEnum.MONTH:
                    return 1;
                case DateTimeFieldEnum.YEAR:
                    return DateTime.MinValue.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Gets the actual minimum.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetActualMinimum(this DateTimeOffset dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return 0;
                case DateTimeFieldEnum.SECOND:
                    return 0;
                case DateTimeFieldEnum.MINUTE:
                    return 0;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return 0;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return 1;
                case DateTimeFieldEnum.MONTH:
                    return 1;
                case DateTimeFieldEnum.YEAR:
                    return DateTime.MinValue.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Gets the actual maximum.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetActualMaximum(this DateTime dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return 999;
                case DateTimeFieldEnum.SECOND:
                    return 59;
                case DateTimeFieldEnum.MINUTE:
                    return 59;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return 23;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.GetWithMaximumDay().Day;
                case DateTimeFieldEnum.MONTH:
                    return dateTime.GetWithMaximumMonth().Month;
                case DateTimeFieldEnum.YEAR:
                    return DateTime.MaxValue.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Gets the actual maximum.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetActualMaximum(this DateTimeOffset dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return 999;
                case DateTimeFieldEnum.SECOND:
                    return 59;
                case DateTimeFieldEnum.MINUTE:
                    return 59;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return 23;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.GetWithMaximumDay().Day;
                case DateTimeFieldEnum.MONTH:
                    return dateTime.GetWithMaximumMonth().Month;
                case DateTimeFieldEnum.YEAR:
                    return DateTime.MaxValue.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateTime SetFieldValue(this DateTime dateTime, int field, int value)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        value);
                case DateTimeFieldEnum.SECOND:
                    return new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        value,
                        dateTime.Millisecond);
                case DateTimeFieldEnum.MINUTE:
                    return new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        value,
                        dateTime.Second,
                        dateTime.Millisecond);
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        value,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond);
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        value,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond);
                case DateTimeFieldEnum.MONTH:
                    return new DateTime(
                        dateTime.Year,
                        value,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond);
                case DateTimeFieldEnum.YEAR:
                    return new DateTime(
                        value,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond);
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">invalid datetime</exception>
        public static DateTimeOffset SetFieldValue(this DateTimeOffset dateTime, int field, int value, TimeZoneInfo timeZone)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        value,
                        timeZone);
                case DateTimeFieldEnum.SECOND:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        value,
                        dateTime.Millisecond,
                        timeZone);
                case DateTimeFieldEnum.MINUTE:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        value,
                        dateTime.Second,
                        dateTime.Millisecond,
                        timeZone);
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        value,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond,
                        timeZone);
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        dateTime.Month,
                        value,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond,
                        timeZone);
                case DateTimeFieldEnum.MONTH:
                    return DateTimeOffsetHelper.CreateDateTime(
                        dateTime.Year,
                        value,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond,
                        timeZone);
                case DateTimeFieldEnum.YEAR:
                    return DateTimeOffsetHelper.CreateDateTime(
                        value,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second,
                        dateTime.Millisecond,
                        timeZone);
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetFieldValue(this DateTime dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.Millisecond;
                case DateTimeFieldEnum.SECOND:
                    return dateTime.Second;
                case DateTimeFieldEnum.MINUTE:
                    return dateTime.Minute;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return dateTime.Hour;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.Day;
                case DateTimeFieldEnum.MONTH:
                    return dateTime.Month;
                case DateTimeFieldEnum.YEAR:
                    return dateTime.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static int GetFieldValue(this DateTimeOffset dateTime, int field)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.Millisecond;
                case DateTimeFieldEnum.SECOND:
                    return dateTime.Second;
                case DateTimeFieldEnum.MINUTE:
                    return dateTime.Minute;
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return dateTime.Hour;
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.Day;
                case DateTimeFieldEnum.MONTH:
                    return dateTime.Month;
                case DateTimeFieldEnum.YEAR:
                    return dateTime.Year;
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Adds using field to indicate which datetime field to add to.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static DateTime AddUsingField(this DateTime dateTime, int field, int amount)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.AddMilliseconds(amount);
                case DateTimeFieldEnum.SECOND:
                    return dateTime.AddSeconds(amount);
                case DateTimeFieldEnum.MINUTE:
                    return dateTime.AddMinutes(amount);
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return dateTime.AddHours(amount);
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.AddDays(amount);
                case DateTimeFieldEnum.MONTH:
                    return dateTime.AddMonths(amount);
                case DateTimeFieldEnum.YEAR:
                    return dateTime.AddYears(amount);
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }

        /// <summary>
        /// Adds using field to indicate which datetime field to add to.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="field">The field.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static DateTimeOffset AddUsingField(this DateTimeOffset dateTime, int field, int amount)
        {
            switch (field)
            {
                case DateTimeFieldEnum.MILLISEC:
                    return dateTime.AddMilliseconds(amount);
                case DateTimeFieldEnum.SECOND:
                    return dateTime.AddSeconds(amount);
                case DateTimeFieldEnum.MINUTE:
                    return dateTime.AddMinutes(amount);
                case DateTimeFieldEnum.HOUR_OF_DAY:
                    return dateTime.AddHours(amount);
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.DAY_OF_MONTH:
                    return dateTime.AddDays(amount);
                case DateTimeFieldEnum.MONTH:
                    return dateTime.AddMonthsLikeJava(amount);
                case DateTimeFieldEnum.YEAR:
                    return dateTime.AddYears(amount);
                default:
                    throw new ArgumentException("invalid datetime");
            }
        }
    }
}
