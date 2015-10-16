///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace com.espertech.esper.compat
{
    public static class DateTimeMath
    {
        private static readonly Calendar Calendar = DateTimeFormatInfo.CurrentInfo.Calendar;

        public static DateTimeOffset AddMonthsLikeJava(this DateTimeOffset dateTime, int numMonths)
        {
            int month = dateTime.Month + numMonths;
            int year = dateTime.Year;

            if (month > 12)
                year += month/12;
            else if (month == 0)
                year--;
            else if (month < 0)
                year += 1 - (month / 12); // number will be negative

            // if we are moving the needle forward, then its simply a modulus
            // to determine what month we should end up at.
            if (month >= 0)
            {
                month %= 12;
                if (month == 0)
                {
                    month = 12;
                }
            }
            else
            {
                // negative months occur when the numMonths is negative...
                // reverse step requires that we do a modulus to see what
                // negative month we are in and then subtracting that from
                // twelve.

                month = month % 12 + 12;
            }

            // its not enough to set the new month, we actually need to know
            // how many days are in that month so that we avoid trying to set
            // a date that's invalid... for example, adding a month to Jan 31st
            // could end up with Feb 31s which is invalid.  However, we want
            // to ensure the date remains in the month of Feb, so we will
            // pull it back to the maximum date.

            var daysInMonth = Calendar.GetDaysInMonth(year, month);
            var dayOfMonth = dateTime.Day;
            if (dayOfMonth > daysInMonth)
                dayOfMonth = daysInMonth;

            return new DateTimeOffset(
                year,
                month,
                dayOfMonth,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond,
                dateTime.Offset
                );
        }

        /// <summary>
        /// Returns a datetime for the week given on the specified day of the week.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <returns></returns>
        public static DateTimeOffset MoveToDayOfWeek(DateTimeOffset from, DayOfWeek dayOfWeek)
        {
            var current = @from.DayOfWeek;
            return @from.AddDays(dayOfWeek - current);
        }

        /// <summary>
        /// Returns a datetime for the end of the month.
        /// </summary>
        /// <param name="from">From.</param>
        /// <returns></returns>
        public static DateTimeOffset EndOfMonth(DateTimeOffset from)
        {
            return new DateTimeOffset(
                @from.Year,
                @from.Month,
                DateTime.DaysInMonth(@from.Year, @from.Month),
                0,
                0,
                0,
                @from.Offset);
        }

        public static int GetWeekOfYear(this DateTime dateTime)
        {
            return Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static int GetWeekOfYear(this DateTimeOffset dateTime)
        {
            // we need the "offset" translated to a datetime because the calendar function does
            // not support datetime offset objects.
            var dateTimeRaw = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                0, 0, 0,
                DateTimeKind.Unspecified);

            return Calendar.GetWeekOfYear(dateTimeRaw, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime GetWithMaximumDay(this DateTime dateTime)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, dateTime.Month);
            return new DateTime(dateTime.Year, dateTime.Month, daysInMonth, dateTime.Hour, dateTime.Minute,
                                dateTime.Second, dateTime.Millisecond);
        }

        public static DateTimeOffset GetWithMaximumDay(this DateTimeOffset dateTime, TimeZoneInfo timeZone = null)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, dateTime.Month);
            if (timeZone == null)
            {
                return new DateTimeOffset(
                    dateTime.Year, dateTime.Month, daysInMonth, dateTime.Hour, dateTime.Minute,
                    dateTime.Second, dateTime.Millisecond, dateTime.Offset);
            }
             
            return DateTimeOffsetHelper.CreateDateTime(
                dateTime.Year, dateTime.Month, daysInMonth, dateTime.Hour, dateTime.Minute,
                dateTime.Second, dateTime.Millisecond, timeZone);
        }

        public static DateTime GetWithMaximumMonth(this DateTime dateTime)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, 12);
            if (dateTime.Day < daysInMonth)
                daysInMonth = dateTime.Day;

            return new DateTime(dateTime.Year, 12, daysInMonth, dateTime.Hour, dateTime.Minute,
                                dateTime.Second, dateTime.Millisecond);
        }

        public static DateTimeOffset GetWithMaximumMonth(this DateTimeOffset dateTime, TimeZoneInfo timeZone = null)
        {
            var daysInMonth = Calendar.GetDaysInMonth(dateTime.Year, 12);
            if (dateTime.Day < daysInMonth)
                daysInMonth = dateTime.Day;

            if (timeZone == null)
            {
                return new DateTimeOffset(
                    dateTime.Year, 12, daysInMonth, dateTime.Hour, dateTime.Minute,
                    dateTime.Second, dateTime.Millisecond, dateTime.Offset);
            }

            return DateTimeOffsetHelper.CreateDateTime(
                dateTime.Year, 12, daysInMonth, dateTime.Hour, dateTime.Minute,
                dateTime.Second, dateTime.Millisecond, timeZone);
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

        public static DateTimeOffset MoveToWeek(this DateTimeOffset dateTime, int targetWeek, TimeZoneInfo timeZone = null)
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
                if ((dateTime.GetWeekOfYear() > 2) && (nextTime.GetWeekOfYear() <= 2))
                {
                    return dateTime;
                }

                dateTime = nextTime;
            } while (true);
        }

        public static DateTimeOffset GetWithMaximumWeek(this DateTimeOffset dateTime, TimeZoneInfo timeZone = null)
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
            var week = dateTime.GetWeekOfYear();
            if (week == 1)
            {
                return dateTime;
            }

            do
            {
                var nextTime = dateTime.AddDays(-7);
                if (dateTime.GetWeekOfYear() == 2)
                {
                    // See if this day in the previous week is still in week 1.  It's
                    // possible that a week started with a day like Friday and that the
                    // date in question was a Thursday.  Technically, Thursday would
                    // have begun on week 2 not 1.
                    if (nextTime.GetWeekOfYear() == 1)
                        return nextTime;
                    // First occurrence of this date occurred on week 2
                    return dateTime;
                }

                dateTime = nextTime;
            } while (true);
        }

        public static DateTimeOffset GetWithMinimumWeek(this DateTimeOffset dateTime, TimeZoneInfo timeZone = null)
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
    }
}
