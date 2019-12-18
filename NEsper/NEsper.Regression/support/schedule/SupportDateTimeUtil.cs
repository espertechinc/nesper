///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.schedule
{
    public class SupportDateTimeUtil
    {
        public static void CompareDate(
            DateTimeEx dateTimeEx,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millis,
            string timeZoneId)
        {
            CompareDate(dateTimeEx, year, month, day, hour, minute, second, millis);
            Assert.AreEqual(timeZoneId, dateTimeEx.TimeZone.Id);
        }

        public static void CompareDate(
            DateTimeEx dateTimeEx,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millis)
        {
            Assert.AreEqual(year, dateTimeEx.Year);
            Assert.AreEqual(month, dateTimeEx.Month);
            Assert.AreEqual(day, dateTimeEx.Day);
            Assert.AreEqual(hour, dateTimeEx.Hour);
            Assert.AreEqual(minute, dateTimeEx.Minute);
            Assert.AreEqual(second, dateTimeEx.Second);
            Assert.AreEqual(millis, dateTimeEx.Millisecond);
        }

        public static long TimePlusMonth(
            long timeInMillis,
            int monthToAdd)
        {
            return DateTimeEx
                .GetInstance(TimeZoneInfo.Utc, timeInMillis)
                .AddMonths(monthToAdd)
                .UtcMillis;
        }
    }
} // end of namespace