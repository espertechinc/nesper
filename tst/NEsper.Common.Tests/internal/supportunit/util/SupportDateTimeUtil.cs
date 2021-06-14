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

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportDateTimeUtil
    {
        public static void CompareDate(
            DateTimeEx dtx,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millis,
            string timeZoneId)
        {
            CompareDate(dtx, year, month, day, hour, minute, second, millis);
            Assert.AreEqual(timeZoneId, dtx.TimeZone.Id);
        }

        public static void CompareDate(
            DateTimeEx dtx,
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millis)
        {
            Assert.AreEqual(year, dtx.Year);
            Assert.AreEqual(month, dtx.Month);
            Assert.AreEqual(day, dtx.Day);
            Assert.AreEqual(hour, dtx.Hour);
            Assert.AreEqual(minute, dtx.Minute);
            Assert.AreEqual(second, dtx.Second);
            Assert.AreEqual(millis, dtx.Millisecond);
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
