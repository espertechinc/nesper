///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.timer
{
	public class SupportDateTimeUtil
    {
        public static void CompareDate(DateTimeOffset sourceDate, int year, int month, int day, int hour, int minute, int second, int millis, string timeZoneId)
        {
            var timeZoneInfoTarget = TimeZoneHelper.GetTimeZoneInfo(timeZoneId);
            var targetDate = sourceDate.TranslateTo(timeZoneInfoTarget);

	        CompareDate(targetDate, year, month, day, hour, minute, second, millis);
	        // Assert.AreEqual(timeZoneId, cal.TimeZone.ID);
	    }

        public static void CompareDate(DateTimeOffset date, int year, int month, int day, int hour, int minute, int second, int millis)
        {
            Assert.AreEqual(year, date.Year);
            Assert.AreEqual(month, date.Month);
            Assert.AreEqual(day, date.Day);
            Assert.AreEqual(hour, date.Hour);
            Assert.AreEqual(minute, date.Minute);
            Assert.AreEqual(second, date.Second);
            Assert.AreEqual(millis, date.Millisecond);
        }
	}
} // end of namespace
