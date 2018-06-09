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
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;
using com.espertech.esper.type;

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// For a crontab-like schedule, this class computes the next occurance given a Start time and a specification of
    /// what the schedule looks like.
    /// The resolution at which this works is at the second level. The next occurance
    /// is always at least 1 second ahead.
    /// The class implements an algorithm that Starts at the highest precision (seconds) and
    /// continues to the lowest precicion (month). For each precision level the
    /// algorithm looks at the list of valid values and finds a value for each that is equal to or greater then
    /// the valid values supplied. If no equal or
    /// greater value was supplied, it will reset all higher precision elements to its minimum value.
    /// </summary>

    public static class ScheduleComputeHelper
    {
        /// <summary>
        /// Computes the next lowest date in milliseconds based on a specification and the
        /// from-time passed in.
        /// </summary>
        /// <param name="spec">defines the schedule</param>
        /// <param name="afterTimeInMillis">defines the start time</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="timeAbacus">The time abacus.</param>
        /// <returns>
        /// a long date tick value for the next schedule occurance matching the spec
        /// </returns>

        public static long ComputeNextOccurance(
            ScheduleSpec spec,
            long afterTimeInMillis,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled)
            {
                Log.Debug(
                    ".computeNextOccurance Computing next occurance," +
                    "  afterTimeInTicks=" + afterTimeInMillis.TimeFromMillis(timeZone) +
                    "  as long=" + afterTimeInMillis +
                    "  spec=" + spec);
            }

            // Add the minimum resolution to the Start time to ensure we don't get the same exact time
            if (spec.UnitValues.ContainsKey(ScheduleUnit.SECONDS))
            {
                afterTimeInMillis += timeAbacus.OneSecond;
            }
            else
            {
                afterTimeInMillis += 60 * timeAbacus.OneSecond;
            }

            return Compute(spec, afterTimeInMillis, timeZone, timeAbacus);
        }

        /// <summary>
        /// Computes the next lowest date in milliseconds based on a specification and the
        /// from-time passed in and returns the delta from the current time.
        /// </summary>
        /// <param name="spec">The schedule.</param>
        /// <param name="afterTimeInMillis">defines the start time.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="timeAbacus">The time abacus.</param>
        /// <returns>
        /// a long millisecond value representing the delta between current time and the next schedule occurance matching the spec
        /// </returns>
        public static long ComputeDeltaNextOccurance(
            ScheduleSpec spec,
            long afterTimeInMillis,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            return ComputeNextOccurance(spec, afterTimeInMillis, timeZone, timeAbacus) - afterTimeInMillis;
        }

        private static long Compute(
            ScheduleSpec spec,
            long afterTimeInMillis,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            long remainderMicros = -1;

            while (true)
            {
                DateTimeEx after;

                if (spec.OptionalTimeZone != null)
                {
                    try
                    {
                        timeZone = TimeZoneHelper.GetTimeZoneInfo(spec.OptionalTimeZone);
                        after = DateTimeEx.GetInstance(timeZone);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        // this behavior ensures we are consistent with Java, but IMO, it's bad behavior...
                        // basically, if the timezone is not found, we default to UTC.
                        timeZone = TimeZoneInfo.Utc;
                        after = DateTimeEx.GetInstance(timeZone);
                    }
                }
                else
                {
                    after = DateTimeEx.GetInstance(timeZone);
                }

                var remainder = timeAbacus.CalendarSet(afterTimeInMillis, after);
                if (remainderMicros == -1)
                {
                    remainderMicros = remainder;
                }

                var result = new ScheduleCalendar { Milliseconds = after.Millisecond };

                ICollection<int> minutesSet = spec.UnitValues.Get(ScheduleUnit.MINUTES);
                ICollection<int> hoursSet = spec.UnitValues.Get(ScheduleUnit.HOURS);
                ICollection<int> monthsSet = spec.UnitValues.Get(ScheduleUnit.MONTHS);
                ICollection<int> secondsSet = null;

                bool isSecondsSpecified = false;

                if (spec.UnitValues.ContainsKey(ScheduleUnit.SECONDS))
                {
                    isSecondsSpecified = true;
                    secondsSet = spec.UnitValues.Get(ScheduleUnit.SECONDS);
                }

                if (isSecondsSpecified)
                {
                    result.Second = NextValue(secondsSet, after.Second);
                    if (result.Second == -1)
                    {
                        result.Second = NextValue(secondsSet, 0);
                        after.AddMinutes(1);
                    }
                }

                result.Minute = NextValue(minutesSet, after.Minute);
                if (result.Minute != after.Minute)
                {
                    result.Second = NextValue(secondsSet, 0);
                }
                if (result.Minute == -1)
                {
                    result.Minute = NextValue(minutesSet, 0);
                    after.AddHours(1);
                }

                result.Hour = NextValue(hoursSet, after.Hour);
                if (result.Hour != after.Hour)
                {
                    result.Second = NextValue(secondsSet, 0);
                    result.Minute = NextValue(minutesSet, 0);
                }
                if (result.Hour == -1)
                {
                    result.Hour = NextValue(hoursSet, 0);
                    after.AddDays(1, DateTimeMathStyle.Java);
                }

                // This call may change second, minute and/or hour parameters
                // They may be reset to minimum values if the day rolled
                result.DayOfMonth = DetermineDayOfMonth(spec, after, result);

                bool dayMatchRealDate = false;
                while (!dayMatchRealDate)
                {
                    if (CheckDayValidInMonth(timeZone, result.DayOfMonth, after.Month, after.Year))
                    {
                        dayMatchRealDate = true;
                    }
                    else
                    {
                        after.AddMonths(1, DateTimeMathStyle.Java);
                    }
                }

                int currentMonth = after.Month;
                result.Month = NextValue(monthsSet, currentMonth);
                if (result.Month != currentMonth)
                {
                    result.Second = NextValue(secondsSet, 0);
                    result.Minute = NextValue(minutesSet, 0);
                    result.Hour = NextValue(hoursSet, 0);
                    result.DayOfMonth = DetermineDayOfMonth(spec, after, result);
                }
                if (result.Month == -1)
                {
                    result.Month = NextValue(monthsSet, 0);
                    after.AddYears(1);
                }

                // Perform a last valid date check, if failing, try to compute a new date based on this altered after date
                int year = after.Year;
                if (!CheckDayValidInMonth(timeZone, result.DayOfMonth, result.Month, year))
                {
                    afterTimeInMillis = timeAbacus.CalendarGet(after, remainder);
                    continue;
                }

                return GetTime(result, after.Year, spec.OptionalTimeZone, timeZone, timeAbacus, remainder);
            }
        }

        /// <summary>
        /// Determine the next valid day of month based on the given specification of valid days in month and
        /// valid days in week. If both days in week and days in month are supplied, the days are OR-ed.
        /// </summary>
        /// <param name="spec"></param>
        /// <param name="after"></param>
        /// <param name="result"></param>
        /// <returns></returns>

        private static int DetermineDayOfMonth(ScheduleSpec spec, DateTimeEx after, ScheduleCalendar result)
        {
            ICollection<Int32> daysOfMonthSet = spec.UnitValues.Get(ScheduleUnit.DAYS_OF_MONTH);
            ICollection<Int32> daysOfWeekSet = spec.UnitValues.Get(ScheduleUnit.DAYS_OF_WEEK);
            ICollection<Int32> secondsSet = spec.UnitValues.Get(ScheduleUnit.SECONDS);
            ICollection<Int32> minutesSet = spec.UnitValues.Get(ScheduleUnit.MINUTES);
            ICollection<Int32> hoursSet = spec.UnitValues.Get(ScheduleUnit.HOURS);

            int dayOfMonth;

            // If days of week is a wildcard, just go by days of month
            if (spec.OptionalDayOfMonthOperator != null || spec.OptionalDayOfWeekOperator != null)
            {
                var isWeek = false;
                var op = spec.OptionalDayOfMonthOperator;
                if (spec.OptionalDayOfMonthOperator == null)
                {
                    op = spec.OptionalDayOfWeekOperator;
                    isWeek = true;
                }

                // may return the current day or a future day in the same month,
                // and may advance the "after" date to the next month
                int currentYYMMDD = GetTimeYYYYMMDD(after);
                IncreaseAfterDayOfMonthSpecialOp(op.Operator, op.Day, op.Month, isWeek, after);
                int rolledYYMMDD = GetTimeYYYYMMDD(after);

                // if rolled then reset time portion
                if (rolledYYMMDD > currentYYMMDD)
                {
                    result.Second = (NextValue(secondsSet, 0));
                    result.Minute = (NextValue(minutesSet, 0));
                    result.Hour = (NextValue(hoursSet, 0));
                    return after.GetFieldValue(DateTimeFieldEnum.DAY_OF_MONTH);
                }
                // rolling backwards is not allowed
                else if (rolledYYMMDD < currentYYMMDD)
                {
                    throw new IllegalStateException("Failed to evaluate special date op, rolled date less then current date");
                }
                else
                {
                    var work = new DateTimeEx(after);
                    work.SetFieldValue(DateTimeFieldEnum.SECOND, result.Second);
                    work.SetFieldValue(DateTimeFieldEnum.MINUTE, result.Minute);
                    work.SetFieldValue(DateTimeFieldEnum.HOUR_OF_DAY, result.Hour);
                    if (work <= after)
                    {    // new date is not after current date, so bump
                        after.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, 1);
                        result.Second = NextValue(secondsSet, 0);
                        result.Minute = NextValue(minutesSet, 0);
                        result.Hour = NextValue(hoursSet, 0);
                        IncreaseAfterDayOfMonthSpecialOp(op.Operator, op.Day, op.Month, isWeek, after);
                    }
                    return after.GetFieldValue(DateTimeFieldEnum.DAY_OF_MONTH);
                }
            }
            else if (daysOfWeekSet == null)
            {
                dayOfMonth = NextValue(daysOfMonthSet, after.Day);
                if (dayOfMonth != after.Day)
                {
                    result.Second = NextValue(secondsSet, 0);
                    result.Minute = NextValue(minutesSet, 0);
                    result.Hour = NextValue(hoursSet, 0);
                }
                if (dayOfMonth == -1)
                {
                    dayOfMonth = NextValue(daysOfMonthSet, 0);
                    after.AddMonths(1, DateTimeMathStyle.Java);
                }
            }
            // If days of weeks is not a wildcard and days of month is a wildcard, go by days of week only
            else if (daysOfMonthSet == null)
            {
                // Loop to find the next day of month that works for the specified day of week values
                while (true)
                {
                    dayOfMonth = after.Day;
                    int dayOfWeek = (int)after.DayOfWeek;

                    // TODO
                    //
                    // Check the DayOfWeek logic in this section.  The former code reads something
                    // like the following:
                    //
                    // Calendar.Get(after, SupportClass.CalendarManager.DAY_OF_WEEK) - 1;
                    //
                    // Java calendars are one based which means that subtracting one makes them
                    // zero-based.  CLR DateTimes are zero-based so there should be no need to
                    // tweak the dates to make this work.

                    // If the day matches neither the day of month nor the day of week
                    if (!daysOfWeekSet.Contains(dayOfWeek))
                    {
                        result.Second = NextValue(secondsSet, 0);
                        result.Minute = NextValue(minutesSet, 0);
                        result.Hour = NextValue(hoursSet, 0);
                        after.AddDays(1, DateTimeMathStyle.Java);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Both days of weeks and days of month are not a wildcard
            else
            {
                // Loop to find the next day of month that works for either day of month  OR   day of week
                while (true)
                {
                    dayOfMonth = after.Day;
                    int dayOfWeek = (int)after.DayOfWeek;

                    // TODO
                    //
                    // See my discussion above about day of week conversion

                    // If the day matches neither the day of month nor the day of week
                    if ((!daysOfWeekSet.Contains(dayOfWeek)) && (!daysOfMonthSet.Contains(dayOfMonth)))
                    {
                        result.Second = NextValue(secondsSet, 0);
                        result.Minute = NextValue(minutesSet, 0);
                        result.Hour = NextValue(hoursSet, 0);
                        after.AddDays(1, DateTimeMathStyle.Java);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return dayOfMonth;
        }

        private static long GetTime(
            ScheduleCalendar result,
            int year,
            string optionalTimeZone,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus,
            long remainder)
        {
            // Here again we have a case of 1-based vs. 0-based indexing.
            // 		Java months are 0-based.
            // 		CLR  months are 1-based.

            if (optionalTimeZone != null)
            {
                try
                {
                    timeZone = TimeZoneHelper.GetTimeZoneInfo(optionalTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    // this behavior ensures we are consistent with Java, but IMO, it's bad behavior...
                    // basically, if the timezone is not found, we default to UTC.
                    timeZone = TimeZoneInfo.Utc;
                }
            }

            if (timeZone == null)
            {
                timeZone = TimeZoneInfo.Local;
            }

            var baseDateTime = new DateTime(
                year,
                result.Month,
                result.DayOfMonth,
                result.Hour,
                result.Minute,
                result.Second,
                result.Milliseconds,
                new GregorianCalendar()
                );

            var baseDateTimeOffset = timeZone.GetUtcOffset(baseDateTime);

            var dateTime = new DateTimeOffset(
                year,
                result.Month,
                result.DayOfMonth,
                result.Hour,
                result.Minute,
                result.Second,
                result.Milliseconds,
                new GregorianCalendar(),
                baseDateTimeOffset);

            var dateTimeEx = new DateTimeEx(dateTime, timeZone);

            return timeAbacus.CalendarGet(dateTimeEx, remainder);
        }

        /// <summary>
        /// Check if this is a valid date.
        /// </summary>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="day">The day.</param>
        /// <param name="month">The month.</param>
        /// <param name="year">The year.</param>
        /// <returns></returns>

        private static bool CheckDayValidInMonth(TimeZoneInfo timeZone, int day, int month, int year)
        {
            try
            {
                DateTimeOffsetHelper.CreateDateTime(year, month, day, 0, 0, 0, 0, timeZone);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determine if in the supplied valueSet there is a value after the given Start value.
        /// Return -1 to indicate that there is no value after the given StartValue.
        /// If the valueSet passed is null it is treated as a wildcard and the same StartValue is returned
        /// </summary>
        /// <param name="valueSet"></param>
        /// <param name="startValue"></param>
        /// <returns></returns>

        private static int NextValue(ICollection<int> valueSet, int startValue)
        {
            if ((valueSet == null) || (valueSet.IsEmpty()))
            {
                return startValue;
            }

            if (valueSet.Contains(startValue))
            {
                return startValue;
            }

            var minValue = startValue + 1;
            var tail = valueSet.Where(value => value >= minValue).GetEnumerator();
            return tail.MoveNext() ? tail.Current : -1;
        }

        private static int GetTimeYYYYMMDD(DateTimeEx calendar)
        {
            return 10000 * calendar.Year + 100 * (calendar.Month + 1) + calendar.Day;
        }

        private static void IncreaseAfterDayOfMonthSpecialOp(CronOperatorEnum @operator, int? day, int? month, bool week, DateTimeEx after)
        {
            DateChecker checker;
            if (@operator == CronOperatorEnum.LASTDAY)
            {
                if (!week)
                {
                    checker = new DateCheckerLastDayOfMonth(day, month);
                }
                else
                {
                    if (day == null)
                    {
                        checker = new DateCheckerLastDayOfWeek(month);
                    }
                    else
                    {
                        checker = new DateCheckerLastSpecificDayWeek(day.Value, month);
                    }
                }
            }
            else if (@operator == CronOperatorEnum.LASTWEEKDAY)
            {
                checker = new DateCheckerLastWeekday(day, month);
            }
            else
            {
                checker = new DateCheckerMonthWeekday(day, month);
            }

            int dayCount = 0;
            while (!checker.Fits(after))
            {
                after.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, 1);
                dayCount++;
                if (dayCount > 10000)
                {
                    throw new ArgumentException("Invalid crontab expression: failed to find match day");
                }
            }
        }

        internal interface DateChecker
        {
            bool Fits(DateTimeEx dateTime);
        }

        internal class DateCheckerLastSpecificDayWeek : DateChecker
        {
            private readonly DayOfWeek _dayCode;
            private readonly int? _month;

            internal DateCheckerLastSpecificDayWeek(int day, int? month)
            {
                if (day < 0 || day > 7)
                {
                    throw new ArgumentException("Last xx day of the month has to be a day of week (0-7)");
                }
                _dayCode = (DayOfWeek)day;
                _month = month;
            }

            public bool Fits(DateTimeEx dateTime)
            {
                if (_dayCode != dateTime.DayOfWeek)
                {
                    return false;
                }
                if (_month != null && _month != dateTime.Month)
                {
                    return false;
                }
                // e.g. 31=Sun,30=Sat,29=Fri,28=Thu,27=Wed,26=Tue,25=Mon
                // e.g. 31-7 = 24
                return dateTime.Day > dateTime.GetActualMaximum(DateTimeFieldEnum.DAY_OF_MONTH) - 7;
            }
        }

        internal class DateCheckerLastDayOfMonth : DateChecker
        {
            private readonly DayOfWeek? _dayCode;
            private readonly int? _month;

            internal DateCheckerLastDayOfMonth(int? day, int? month)
            {
                if (day != null)
                {
                    if (day < 0 || day > 7)
                    {
                        throw new ArgumentException("Last xx day of the month has to be a day of week (0-7)");
                    }
                    _dayCode = (DayOfWeek)day;
                }
                else
                {
                    _dayCode = null;
                }
                _month = month;
            }

            public bool Fits(DateTimeEx dateTime)
            {
                if (_dayCode != null && _dayCode != dateTime.DayOfWeek)
                {
                    return false;
                }
                if (_month != null && _month != dateTime.Month)
                {
                    return false;
                }
                return (dateTime.Day == dateTime.GetActualMaximum(DateTimeFieldEnum.DAY_OF_MONTH));
            }
        }

        internal class DateCheckerLastDayOfWeek : DateChecker
        {
            private readonly int? _month;

            internal DateCheckerLastDayOfWeek(int? month)
            {
                _month = month;
            }

            public bool Fits(DateTimeEx dateTime)
            {
                if (_month != null && _month != dateTime.Month)
                {
                    return false;
                }
                return (dateTime.DayOfWeek == DayOfWeek.Saturday);
            }
        }

        internal class DateCheckerLastWeekday : DateChecker
        {
            private readonly DayOfWeek? _dayCode;
            private readonly int? _month;

            internal DateCheckerLastWeekday(int? day, int? month)
            {
                if (day != null)
                {
                    if (day < 0 || day > 7)
                    {
                        throw new ArgumentException("Last xx day of the month has to be a day of week (0-7)");
                    }
                    _dayCode = (DayOfWeek)day;
                }
                else
                {
                    _dayCode = null;
                }
                _month = month;
            }

            public bool Fits(DateTimeEx dateTime)
            {
                if (_dayCode != null && _dayCode != dateTime.DayOfWeek)
                {
                    return false;
                }
                if (_month != null && _month != dateTime.Month)
                {
                    return false;
                }
                if (!IsWeekday(dateTime))
                {
                    return false;
                }
                int day = dateTime.Day;
                int max = dateTime.GetActualMaximum(DateTimeFieldEnum.DAY_OF_MONTH);
                if (day == max)
                {
                    return true;
                }
                var dayOfWeek = dateTime.DayOfWeek;
                return day >= max - 2 && dayOfWeek == DayOfWeek.Friday;
            }
        }

        internal class DateCheckerMonthWeekday : DateChecker
        {
            private readonly int? _day;
            private readonly int? _month;

            internal DateCheckerMonthWeekday(int? day, int? month)
            {
                if (day != null)
                {
                    if (day < 1 || day > 31)
                    {
                        throw new ArgumentException("xx day of the month has to be a in range (1-31)");
                    }
                }
                _day = day;
                _month = month;
            }

            public bool Fits(DateTimeEx dateTime)
            {
                if (_month != null && _month != dateTime.Month)
                {
                    return false;
                }
                if (!IsWeekday(dateTime))
                {
                    return false;
                }
                if (_day == null)
                {
                    return true;
                }

                var work = new DateTimeEx(dateTime);
                var target = ComputeNearestWeekdayDay(_day.Value, work);
                return dateTime.Day == target;
            }

            private static int ComputeNearestWeekdayDay(int day, DateTimeEx work)
            {
                int max = work.GetActualMaximum(DateTimeFieldEnum.DAY_OF_MONTH);
                if (day <= max)
                {
                    work = work.SetFieldValue(DateTimeFieldEnum.DAY_OF_MONTH, day);
                }
                else
                {
                    work = work.SetFieldValue(DateTimeFieldEnum.DAY_OF_MONTH, max);
                }

                if (IsWeekday(work))
                {
                    return work.Day;
                }
                if (work.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (work.Day > 1)
                    {
                        work = work.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, -1);
                        return work.Day;
                    }
                    else
                    {
                        work = work.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, 2);
                        return work.Day;
                    }
                }
                else
                {
                    // handle Sunday
                    if (max == work.Day)
                    {
                        work = work.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, -2);
                        return work.Day;
                    }
                    else
                    {
                        work = work.AddUsingField(DateTimeFieldEnum.DAY_OF_MONTH, 1);
                        return work.Day;
                    }
                }
            }
        }

        private static bool IsWeekday(DateTimeEx dateTime)
        {
            var dayOfWeek = dateTime.DayOfWeek;
            return !(dayOfWeek < DayOfWeek.Monday || dayOfWeek > DayOfWeek.Friday);
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
