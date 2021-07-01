///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.schedule
{
    [TestFixture]
    public class TestScheduleComputeHelper : AbstractCommonTest
    {
        private static readonly SimpleDateFormat timeFormat = new SimpleDateFormat("yyyy-M-d H:mm:ss");

        public void CheckCorrect(
            ScheduleSpec spec,
            string now,
            string expected)
        {
            var nowDate = timeFormat.Parse(now);
            var expectedDate = timeFormat.Parse(expected);

            var result = ScheduleComputeHelper.ComputeNextOccurance(
                spec, nowDate.UtcMillis, TimeZoneInfo.Utc, TimeAbacusMilliseconds.INSTANCE);
            var resultDate = DateTimeEx.GetInstance(TimeZoneInfo.Utc, result);

            if (!resultDate.Equals(expectedDate))
            {
                Log.Debug(".checkCorrect Difference in result found, spec=" + spec);
                Log.Debug(
                    ".checkCorrect      now=" + timeFormat.Format(nowDate) +
                    " long=" + nowDate.UtcMillis);
                Log.Debug(
                    ".checkCorrect expected=" + timeFormat.Format(expectedDate) +
                    " long=" + expectedDate.UtcMillis);
                Log.Debug(
                    ".checkCorrect   result=" + timeFormat.Format(resultDate) +
                    " long=" + resultDate.UtcMillis);
                Assert.IsTrue(false);
            }
        }

        public void CheckCorrectWZone(
            ScheduleSpec spec,
            string nowWZone,
            string expectedWZone)
        {
            var nowDate = DateTimeParsingFunctions.ParseDefaultMSecWZone(nowWZone);
            var expectedDate = DateTimeParsingFunctions.ParseDefaultMSecWZone(expectedWZone);

            var result = ScheduleComputeHelper.ComputeNextOccurance(
                spec, nowDate,
                TimeZoneInfo.Utc,
                TimeAbacusMilliseconds.INSTANCE);
            var resultDate = DateTimeEx.GetInstance(TimeZoneInfo.Utc, result);

            if (result != expectedDate)
            {
                Log.Debug(
                    ".checkCorrect Difference in result found, spec=" + spec);
                Log.Debug(
                    ".checkCorrect now=" + timeFormat.Format(nowDate.TimeFromMillis()) +
                    " long=" + nowDate);
                Log.Debug(
                    ".checkCorrect expected=" + timeFormat.Format(expectedDate.TimeFromMillis()) +
                    " long=" + expectedDate);
                Log.Debug(
                    ".checkCorrect result=" + timeFormat.Format(resultDate) +
                    " long=" + resultDate.UtcMillis);
                Assert.IsTrue(false);
            }
        }

        [Test]
        public void TestCompute()
        {
            ScheduleSpec spec = null;

            // Try next "5 minutes past the hour"
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MINUTES, 5);

            CheckCorrect(spec, "2004-12-9 15:45:01", "2004-12-9 16:05:00");
            CheckCorrect(spec, "2004-12-9 16:04:59", "2004-12-9 16:05:00");
            CheckCorrect(spec, "2004-12-9 16:05:00", "2004-12-9 17:05:00");
            CheckCorrect(spec, "2004-12-9 16:05:01", "2004-12-9 17:05:00");
            CheckCorrect(spec, "2004-12-9 16:05:01", "2004-12-9 17:05:00");
            CheckCorrect(spec, "2004-12-9 23:58:01", "2004-12-10 00:05:00");

            // Try next "5, 10 and 15 minutes past the hour"
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MINUTES, 5);
            spec.AddValue(ScheduleUnit.MINUTES, 10);
            spec.AddValue(ScheduleUnit.MINUTES, 15);

            CheckCorrect(spec, "2004-12-9 15:45:01", "2004-12-9 16:05:00");
            CheckCorrect(spec, "2004-12-9 16:04:59", "2004-12-9 16:05:00");
            CheckCorrect(spec, "2004-12-9 16:05:00", "2004-12-9 16:10:00");
            CheckCorrect(spec, "2004-12-9 16:10:00", "2004-12-9 16:15:00");
            CheckCorrect(spec, "2004-12-9 16:14:59", "2004-12-9 16:15:00");
            CheckCorrect(spec, "2004-12-9 16:15:00", "2004-12-9 17:05:00");

            // Try next "0 and 30 and 59 minutes past the hour"
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MINUTES, 0);
            spec.AddValue(ScheduleUnit.MINUTES, 30);
            spec.AddValue(ScheduleUnit.MINUTES, 59);

            CheckCorrect(spec, "2004-12-9 15:45:01", "2004-12-9 15:59:00");
            CheckCorrect(spec, "2004-12-9 15:59:01", "2004-12-9 16:00:00");
            CheckCorrect(spec, "2004-12-9 16:04:59", "2004-12-9 16:30:00");
            CheckCorrect(spec, "2004-12-9 16:30:00", "2004-12-9 16:59:00");
            CheckCorrect(spec, "2004-12-9 16:59:30", "2004-12-9 17:00:00");

            // Try minutes combined with seconds
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MINUTES, 0);
            spec.AddValue(ScheduleUnit.MINUTES, 30);
            spec.AddValue(ScheduleUnit.SECONDS, 0);
            spec.AddValue(ScheduleUnit.SECONDS, 30);

            CheckCorrect(spec, "2004-12-9 15:59:59", "2004-12-9 16:00:00");
            CheckCorrect(spec, "2004-12-9 16:00:00", "2004-12-9 16:00:30");
            CheckCorrect(spec, "2004-12-9 16:00:29", "2004-12-9 16:00:30");
            CheckCorrect(spec, "2004-12-9 16:00:30", "2004-12-9 16:30:00");
            CheckCorrect(spec, "2004-12-9 16:29:59", "2004-12-9 16:30:00");
            CheckCorrect(spec, "2004-12-9 16:30:00", "2004-12-9 16:30:30");
            CheckCorrect(spec, "2004-12-9 17:00:00", "2004-12-9 17:00:30");

            // Try hours combined with seconds
            spec = new ScheduleSpec();
            for (var i = 10; i <= 14; i++)
            {
                spec.AddValue(ScheduleUnit.HOURS, i);
            }

            spec.AddValue(ScheduleUnit.SECONDS, 15);

            CheckCorrect(spec, "2004-12-9 15:59:59", "2004-12-10 10:00:15");
            CheckCorrect(spec, "2004-12-10 10:00:15", "2004-12-10 10:01:15");
            CheckCorrect(spec, "2004-12-10 10:01:15", "2004-12-10 10:02:15");
            CheckCorrect(spec, "2004-12-10 14:01:15", "2004-12-10 14:02:15");
            CheckCorrect(spec, "2004-12-10 14:59:15", "2004-12-11 10:00:15");

            // Try hours combined with minutes
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.HOURS, 9);
            spec.AddValue(ScheduleUnit.MINUTES, 5);

            CheckCorrect(spec, "2004-12-9 15:59:59", "2004-12-10 9:05:00");
            CheckCorrect(spec, "2004-11-30 15:59:59", "2004-12-1 9:05:00");
            CheckCorrect(spec, "2004-11-30 9:04:59", "2004-11-30 9:05:00");
            CheckCorrect(spec, "2004-12-31 9:05:01", "2005-01-01 9:05:00");

            // Try day of month as the 31st
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 31);

            CheckCorrect(spec, "2004-11-30 15:59:59", "2004-12-31 0:00:00");
            CheckCorrect(spec, "2004-12-30 15:59:59", "2004-12-31 0:00:00");
            CheckCorrect(spec, "2004-12-31 00:00:00", "2004-12-31 0:01:00");
            CheckCorrect(spec, "2005-01-01 00:00:00", "2005-01-31 0:00:00");
            CheckCorrect(spec, "2005-02-01 00:00:00", "2005-03-31 0:00:00");
            CheckCorrect(spec, "2005-04-01 00:00:00", "2005-05-31 0:00:00");

            // Try day of month as the 29st, for february testing
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 29);

            CheckCorrect(spec, "2004-11-30 15:59:59", "2004-12-29 0:00:00");
            CheckCorrect(spec, "2004-12-29 00:00:00", "2004-12-29 0:01:00");
            CheckCorrect(spec, "2004-12-29 00:01:00", "2004-12-29 0:02:00");
            CheckCorrect(spec, "2004-02-28 15:59:59", "2004-02-29 0:00:00");
            CheckCorrect(spec, "2003-02-28 15:59:59", "2003-03-29 0:00:00");
            CheckCorrect(spec, "2005-02-27 15:59:59", "2005-03-29 0:00:00");

            // Try 4:00 every day
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.HOURS, 16);
            spec.AddValue(ScheduleUnit.MINUTES, 0);

            CheckCorrect(spec, "2004-10-01 15:59:59", "2004-10-01 16:00:00");
            CheckCorrect(spec, "2004-10-01 00:00:00", "2004-10-01 16:00:00");
            CheckCorrect(spec, "2004-09-30 16:00:00", "2004-10-01 16:00:00");
            CheckCorrect(spec, "2004-12-30 16:00:00", "2004-12-31 16:00:00");
            CheckCorrect(spec, "2004-12-31 16:00:00", "2005-01-01 16:00:00");

            // Try every weekday at 10 am - scrum time!
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.HOURS, 10);
            spec.AddValue(ScheduleUnit.MINUTES, 0);
            for (var i = 1; i <= 5; i++)
            {
                spec.AddValue(ScheduleUnit.DAYS_OF_WEEK, i);
            }

            CheckCorrect(spec, "2004-12-05 09:50:59", "2004-12-06 10:00:00");
            CheckCorrect(spec, "2004-12-06 09:59:59", "2004-12-06 10:00:00");
            CheckCorrect(spec, "2004-12-07 09:50:00", "2004-12-07 10:00:00");
            CheckCorrect(spec, "2004-12-08 09:00:00", "2004-12-08 10:00:00");
            CheckCorrect(spec, "2004-12-09 08:00:00", "2004-12-09 10:00:00");
            CheckCorrect(spec, "2004-12-10 09:50:50", "2004-12-10 10:00:00");
            CheckCorrect(spec, "2004-12-11 00:00:00", "2004-12-13 10:00:00");
            CheckCorrect(spec, "2004-12-12 09:00:50", "2004-12-13 10:00:00");
            CheckCorrect(spec, "2004-12-13 09:50:50", "2004-12-13 10:00:00");
            CheckCorrect(spec, "2004-12-13 10:00:00", "2004-12-14 10:00:00");
            CheckCorrect(spec, "2004-12-13 10:00:01", "2004-12-14 10:00:00");

            // Every Monday and also on the 1st and 15th of each month, at midnight
            // (tests the or between DAYS_OF_MONTH and DAYS_OF_WEEK)
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 1);
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 15);
            spec.AddValue(ScheduleUnit.HOURS, 0);
            spec.AddValue(ScheduleUnit.MINUTES, 0);
            spec.AddValue(ScheduleUnit.SECONDS, 0);
            spec.AddValue(ScheduleUnit.DAYS_OF_WEEK, 1);

            CheckCorrect(spec, "2004-12-05 09:50:59", "2004-12-06 00:00:00");
            CheckCorrect(spec, "2004-12-06 00:00:00", "2004-12-13 00:00:00");
            CheckCorrect(spec, "2004-12-07 01:20:00", "2004-12-13 00:00:00");
            CheckCorrect(spec, "2004-12-12 23:00:00", "2004-12-13 00:00:00");
            CheckCorrect(spec, "2004-12-13 23:00:00", "2004-12-15 00:00:00");
            CheckCorrect(spec, "2004-12-14 23:00:00", "2004-12-15 00:00:00");
            CheckCorrect(spec, "2004-12-15 23:00:00", "2004-12-20 00:00:00");
            CheckCorrect(spec, "2004-12-18 23:00:00", "2004-12-20 00:00:00");
            CheckCorrect(spec, "2004-12-20 00:01:00", "2004-12-27 00:00:00");
            CheckCorrect(spec, "2004-12-27 00:01:00", "2005-01-01 00:00:00");
            CheckCorrect(spec, "2005-01-01 00:01:00", "2005-01-03 00:00:00");
            CheckCorrect(spec, "2005-01-03 00:01:00", "2005-01-10 00:00:00");
            CheckCorrect(spec, "2005-01-10 00:01:00", "2005-01-15 00:00:00");
            CheckCorrect(spec, "2005-01-15 00:01:00", "2005-01-17 00:00:00");
            CheckCorrect(spec, "2005-01-17 00:01:00", "2005-01-24 00:00:00");
            CheckCorrect(spec, "2005-01-24 00:01:00", "2005-01-31 00:00:00");
            CheckCorrect(spec, "2005-01-31 00:01:00", "2005-02-01 00:00:00");

            // Every second month on every second weekday
            spec = new ScheduleSpec();
            for (var i = 1; i <= 12; i += 2)
            {
                spec.AddValue(ScheduleUnit.MONTHS, i);
            }

            for (var i = 0; i <= 6; i += 2) // Adds Sunday, Tuesday, Thursday, Saturday
            {
                spec.AddValue(ScheduleUnit.DAYS_OF_WEEK, i);
            }

            CheckCorrect(spec, "2004-09-01 00:00:00", "2004-09-02 00:00:00"); // Sept 1 2004 is a Wednesday
            CheckCorrect(spec, "2004-09-02 00:00:00", "2004-09-02 00:01:00");
            CheckCorrect(spec, "2004-09-02 23:59:00", "2004-09-04 00:00:00");
            CheckCorrect(spec, "2004-09-04 23:59:00", "2004-09-05 00:00:00"); // Sept 5 2004 is a Sunday
            CheckCorrect(spec, "2004-09-05 23:57:00", "2004-09-05 23:58:00");
            CheckCorrect(spec, "2004-09-05 23:58:00", "2004-09-05 23:59:00");
            CheckCorrect(spec, "2004-09-05 23:59:00", "2004-09-07 00:00:00");
            CheckCorrect(spec, "2004-09-30 23:58:00", "2004-09-30 23:59:00"); // Sept 30 in a Thursday
            CheckCorrect(spec, "2004-09-30 23:59:00", "2004-11-02 00:00:00");

            // Every second month on every second weekday
            spec = new ScheduleSpec();
            for (var i = 1; i <= 12; i += 2)
            {
                spec.AddValue(ScheduleUnit.MONTHS, i);
            }

            for (var i = 0; i <= 6; i += 2) // Adds Sunday, Tuesday, Thursday, Saturday
            {
                spec.AddValue(ScheduleUnit.DAYS_OF_WEEK, i);
            }

            CheckCorrect(spec, "2004-09-01 00:00:00", "2004-09-02 00:00:00"); // Sept 1 2004 is a Wednesday
            CheckCorrect(spec, "2004-09-02 00:00:00", "2004-09-02 00:01:00");
            CheckCorrect(spec, "2004-09-02 23:59:00", "2004-09-04 00:00:00");
            CheckCorrect(spec, "2004-09-04 23:59:00", "2004-09-05 00:00:00"); // Sept 5 2004 is a Sunday
            CheckCorrect(spec, "2004-09-05 23:57:00", "2004-09-05 23:58:00");
            CheckCorrect(spec, "2004-09-05 23:58:00", "2004-09-05 23:59:00");
            CheckCorrect(spec, "2004-09-05 23:59:00", "2004-09-07 00:00:00");

            // Every 5 seconds, between 9am and until 4pm, all weekdays except Saturday and Sunday
            spec = new ScheduleSpec();
            for (var i = 0; i <= 59; i += 5)
            {
                spec.AddValue(ScheduleUnit.SECONDS, i);
            }

            for (var i = 1; i <= 5; i++)
            {
                spec.AddValue(ScheduleUnit.DAYS_OF_WEEK, i);
            }

            for (var i = 9; i <= 15; i++)
            {
                spec.AddValue(ScheduleUnit.HOURS, i);
            }

            CheckCorrect(spec, "2004-12-12 20:00:00", "2004-12-13 09:00:00"); // Dec 12 2004 is a Sunday
            CheckCorrect(spec, "2004-12-13 09:00:01", "2004-12-13 09:00:05");
            CheckCorrect(spec, "2004-12-13 09:00:05", "2004-12-13 09:00:10");
            CheckCorrect(spec, "2004-12-13 09:00:11", "2004-12-13 09:00:15");
            CheckCorrect(spec, "2004-12-13 09:00:15", "2004-12-13 09:00:20");
            CheckCorrect(spec, "2004-12-13 09:00:24", "2004-12-13 09:00:25");
            CheckCorrect(spec, "2004-12-13 15:59:50", "2004-12-13 15:59:55");
            CheckCorrect(spec, "2004-12-13 15:59:55", "2004-12-14 09:00:00");
            CheckCorrect(spec, "2004-12-14 12:27:35", "2004-12-14 12:27:40");
            CheckCorrect(spec, "2004-12-14 12:29:55", "2004-12-14 12:30:00");
            CheckCorrect(spec, "2004-12-17 00:03:00", "2004-12-17 09:00:00");
            CheckCorrect(spec, "2004-12-17 15:59:50", "2004-12-17 15:59:55");
            CheckCorrect(spec, "2004-12-17 15:59:55", "2004-12-20 09:00:00");

            // Feb 14, 12pm
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MONTHS, 2);
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 14);
            spec.AddValue(ScheduleUnit.HOURS, 12);
            spec.AddValue(ScheduleUnit.MINUTES, 0);

            CheckCorrect(spec, "2004-12-12 20:00:00", "2005-02-14 12:00:00");
            CheckCorrect(spec, "2003-12-12 20:00:00", "2004-02-14 12:00:00");
            CheckCorrect(spec, "2004-02-01 20:00:00", "2004-02-14 12:00:00");

            // Dec 31, 23pm and 50 seconds (countdown)
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MONTHS, 12);
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 31);
            spec.AddValue(ScheduleUnit.HOURS, 23);
            spec.AddValue(ScheduleUnit.MINUTES, 59);
            spec.AddValue(ScheduleUnit.SECONDS, 50);

            CheckCorrect(spec, "2004-12-12 20:00:00", "2004-12-31 23:59:50");
            CheckCorrect(spec, "2004-12-31 23:59:55", "2005-12-31 23:59:50");

            // CST timezone 7:00:00am
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.HOURS, 7);
            spec.AddValue(ScheduleUnit.MINUTES, 0);
            spec.AddValue(ScheduleUnit.SECONDS, 0);
            spec.OptionalTimeZone = "Central Standard Time";

            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-10:00", "2008-02-02T03:00:00.000GMT-10:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-9:00", "2008-02-02T04:00:00.000GMT-9:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-8:00", "2008-02-02T05:00:00.000GMT-8:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-7:00", "2008-02-02T06:00:00.000GMT-7:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-6:00", "2008-02-01T07:00:00.000GMT-6:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-5:00", "2008-02-01T08:00:00.000GMT-5:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-4:00", "2008-02-01T09:00:00.000GMT-4:00");

            // EST timezone 7am, any minute
            spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.HOURS, 7);
            spec.AddValue(ScheduleUnit.SECONDS, 0);
            spec.OptionalTimeZone = "Eastern Standard Time";

            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-7:00", "2008-02-02T05:00:00.000GMT-7:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-6:00", "2008-02-01T06:01:00.000GMT-6:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-5:00", "2008-02-01T07:00:00.000GMT-5:00");
            CheckCorrectWZone(spec, "2008-02-01T06:00:00.000GMT-4:00", "2008-02-01T08:00:00.000GMT-4:00");
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
