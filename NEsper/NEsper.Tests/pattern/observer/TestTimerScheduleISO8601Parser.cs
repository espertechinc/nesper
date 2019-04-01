///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.timer;

using NUnit.Framework;

namespace com.espertech.esper.pattern.observer
{
    [TestFixture]
	public class TestTimerScheduleISO8601Parser
    {
        [Test]
	    public void TestParseDateFormats()
        {
	        // with timezone, without msec
	        AssertTimeParse("1997-07-16T19:20:30+01:00", 1997, 7, 16, 19, 20, 30, 0, "GMT+01:00");

	        // with timezone, with msec
	        AssertTimeParse("1997-07-16T19:20:30.12+01:00", 1997, 7, 16, 19, 20, 30, 120, "GMT+01:00");
	        AssertTimeParse("1997-07-16T19:20:30.12+04:30", 1997, 7, 16, 19, 20, 30, 120, "GMT+04:30");

	        // with timezone UTC, without msec
	        AssertTimeParse("1997-07-16T19:20:30Z", 1997, 7, 16, 19, 20, 30, 0, "GMT+00:00");

	        // with timezone UTC, with msec
	        AssertTimeParse("1997-07-16T19:20:30.12Z", 1997, 7, 16, 19, 20, 30, 120, "GMT+00:00");
	        AssertTimeParse("1997-07-16T19:20:30.1Z", 1997, 7, 16, 19, 20, 30, 100, "GMT+00:00");
	        AssertTimeParse("1997-07-16T19:20:30.123Z", 1997, 7, 16, 19, 20, 30, 123, "GMT+00:00");

	        // local timezone, with and without msec
	        AssertTimeParse("1997-07-16T19:20:30.123", 1997, 7, 16, 19, 20, 30, 123, TimeZoneInfo.Local.StandardName);
            AssertTimeParse("1997-07-16T19:20:30", 1997, 7, 16, 19, 20, 30, 0, TimeZoneInfo.Local.StandardName);
	    }

        [Test]
	    public void TestInvalid() {
	        // date-only tests
	        TryInvalid("5",
	                "Failed to parse '5': Exception parsing date '5', the date is not a supported ISO 8601 date");
	        TryInvalid(null,
	                "Received a null value");
	        TryInvalid("",
	                "Received an empty string");
	        TryInvalid("/",
	                "Failed to parse '/': Invalid number of parts");

	        // period-only tests
	        TryInvalidPeriod("P");
	        TryInvalidPeriod("P1");
	        TryInvalidPeriod("P1D1D");
	        TryInvalidPeriod("PT1D");
	        TryInvalidPeriod("PD");
	        TryInvalidPeriod("P0.1D");
	        TryInvalidPeriod("P-10D");
	        TryInvalidPeriod("P0D");

	        // "date/period" tests
	        TryInvalid("1997-07-16T19:20:30.12Z/x",
	                "Failed to parse '1997-07-16T19:20:30.12Z/x': Invalid period 'x'");
	        TryInvalid("1997-07-16T19:20:30.12Z/PT1D",
	                "Failed to parse '1997-07-16T19:20:30.12Z/PT1D': Invalid period 'PT1D'");
	        TryInvalid("dum-07-16T19:20:30.12Z/P1D",
	                "Failed to parse 'dum-07-16T19:20:30.12Z/P1D': Exception parsing date 'dum-07-16T19:20:30.12Z', the date is not a supported ISO 8601 date");
	        TryInvalid("/P1D",
	                "Failed to parse '/P1D': Expected either a recurrence or a date but received an empty string");
	        TryInvalid("1997-07-16T19:20:30.12Z/",
	                "Failed to parse '1997-07-16T19:20:30.12Z/': Missing the period part");

	        // "recurrence/period" tests
	        TryInvalid("Ra/P1D",
	                "Failed to parse 'Ra/P1D': Invalid repeat 'Ra', expecting an long-typed value but received 'a'");
	        TryInvalid("R0.1/P1D",
	                "Failed to parse 'R0.1/P1D': Invalid repeat 'R0.1', expecting an long-typed value but received '0.1'");
	        TryInvalid("R100000000000000000000000000000/P1D",
	                "Failed to parse 'R100000000000000000000000000000/P1D': Invalid repeat 'R100000000000000000000000000000', expecting an long-typed value but received '100000000000000000000000000000'");

	        // "recurrence/date/period" tests
	        TryInvalid("R/dummy/PT1M",
	                "Failed to parse 'R/dummy/PT1M': Exception parsing date 'dummy', the date is not a supported ISO 8601 date");
	        TryInvalid("Rx/1997-07-16T19:20:30.12Z/PT1M",
	                "Failed to parse 'Rx/1997-07-16T19:20:30.12Z/PT1M': Invalid repeat 'Rx', expecting an long-typed value but received 'x'");
	        TryInvalid("R1/1997-07-16T19:20:30.12Z/PT1D",
	                "Failed to parse 'R1/1997-07-16T19:20:30.12Z/PT1D': Invalid period 'PT1D'");
	    }

        [Test]
	    public void TestParse() {
            AssertParse("R3/2012-10-01T05:52:00Z/PT2S", 3L, "2012-10-01T05:52:00.000GMT-0:00", new TimePeriod().SetSeconds(2));
	        AssertParse("2012-10-01T05:52:00Z", null, "2012-10-01T05:52:00.000GMT-0:00", null);
	        AssertParse("R3/PT2S", 3L, null, new TimePeriod().SetSeconds(2));

	        AssertParseRepeat("R", -1);
	        AssertParseRepeat("R0", 0);
	        AssertParseRepeat("R1", 1);
	        AssertParseRepeat("R10", 10);
	        AssertParseRepeat("R365", 365);
	        AssertParseRepeat("R10000000000000", 10000000000000L);

            AssertParsePeriod("1Y", new TimePeriod().SetYears(1));
            AssertParsePeriod("5M", new TimePeriod().SetMonths(5));
            AssertParsePeriod("6W", new TimePeriod().SetWeeks(6));
            AssertParsePeriod("10D", new TimePeriod().SetDays(10));

            AssertParsePeriod("T3H", new TimePeriod().SetHours(3));
            AssertParsePeriod("T4M", new TimePeriod().SetMinutes(4));
            AssertParsePeriod("T5S", new TimePeriod().SetSeconds(5));
            AssertParsePeriod("T2S", new TimePeriod().SetSeconds(2));
            AssertParsePeriod("T1S", new TimePeriod().SetSeconds(1));
            AssertParsePeriod("T10S", new TimePeriod().SetSeconds(10));

            AssertParsePeriod("1YT30M", new TimePeriod().SetYears(1).SetMinutes(30));
            AssertParsePeriod("1Y2M10DT2H30M", new TimePeriod().SetYears(1).SetMonths(2).SetDays(10).SetHours(2).SetMinutes(30));
            AssertParsePeriod("T10H20S", new TimePeriod().SetHours(10).SetSeconds(20));
            AssertParsePeriod("100Y2000M801W100DT29800H3000M304394S", new TimePeriod()
                .SetYears(100).SetMonths(2000).SetWeeks(801).SetDays(100).SetHours(29800).SetMinutes(3000).SetSeconds(304394));
	    }

	    private void AssertParsePeriod(string period, TimePeriod expected) {
	        AssertParse("R/2012-10-01T05:52:00Z/P" + period, -1L, "2012-10-01T05:52:00.000GMT-0:00", expected);
	    }

	    private void AssertParseRepeat(string repeat, long expected) {
            AssertParse(repeat + "/2012-10-01T05:52:00Z/PT2S", expected, "2012-10-01T05:52:00.000GMT-0:00", new TimePeriod().SetSeconds(2));
	    }

	    private void AssertParse(string text, long? expectedNumRepeats, string expectedDate, TimePeriod expectedTimePeriod) {
	        TimerScheduleSpec spec = TimerScheduleISO8601Parser.Parse(text);
	        Assert.AreEqual(expectedNumRepeats, (object) spec.OptionalRepeatCount);
	        if (expectedTimePeriod == null) {
	            Assert.IsNull(spec.OptionalTimePeriod);
	        }
	        else {
	            Assert.AreEqual(expectedTimePeriod, spec.OptionalTimePeriod,
                    string.Format("expected '{0}' got '{1}'", expectedTimePeriod.ToStringISO8601(), spec.OptionalTimePeriod.ToStringISO8601()));
	        }
	        if (expectedDate == null) {
	            Assert.IsNull(spec.OptionalDate);
	        }
	        else {
	            Assert.AreEqual(DateTimeParser.ParseDefaultMSecWZone(expectedDate), spec.OptionalDate.TimeInMillis);
	        }
	    }

	    private void AssertTimeParse(string date, int year, int month, int day, int hour, int minute, int second, int millis, string zone)
	    {
	        TimeZoneInfo timeZoneInfoTarget = TimeZoneHelper.GetTimeZoneInfo(zone);
            TimeZoneInfo timeZoneInfoLocal = TimeZoneHelper.Local;
	        TimerScheduleSpec spec = TimerScheduleISO8601Parser.Parse(date);
            // the date time will be parsed by the ISO8601 parser but the time returned will be in "offset" terms
	        DateTimeOffset specDate = spec.OptionalDate.DateTime.TranslateTo(timeZoneInfoTarget);

	        SupportDateTimeUtil.CompareDate(specDate, year, month, day, hour, minute, second, millis);
	        //Assert.AreEqual(zone, spec.OptionalDate.TimeZone.DisplayName);
	    }

	    private void TryInvalidPeriod(string period) {
	        TryInvalid(period,
	                "Failed to parse '" + period + "': Invalid period '" + period + "'");
	    }

	    private void TryInvalid(string iso8601, string message) {
	        try {
	            TimerScheduleISO8601Parser.Parse(iso8601);
	            Assert.Fail();
	        }
	        catch (ScheduleParameterException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }
	}
} // end of namespace
