///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat.datetime;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    [TestFixture]
    public class TestCalendarOpPlusFastAddHelper : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestCompute()
        {
            long defaultCurrent = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:51:01.150");

            // millisecond adds
            TimePeriod oneMsec = new TimePeriod().SetMillis(1);
            AssertCompute(defaultCurrent, oneMsec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60 * 60 * 1000L), "2002-05-30T09:51:01.151");
            AssertCompute(defaultCurrent, oneMsec, "2001-06-01T00:00:00.000",
                    new LongAssertionAtLeast(363 * 24 * 60 * 60 * 1000L), "2002-05-30T09:51:01.151");

            // 10-millisecond adds
            TimePeriod tenMsec = new TimePeriod().SetMillis(10);
            AssertCompute(defaultCurrent, tenMsec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60 * 60 * 100L), "2002-05-30T09:51:01.160");

            // 100-millisecond adds
            TimePeriod hundredMsec = new TimePeriod().SetMillis(100);
            AssertCompute(defaultCurrent, hundredMsec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60 * 60 * 10L), "2002-05-30T09:51:01.200");

            // 1-hour-in-millisecond adds
            TimePeriod oneHourInMsec = new TimePeriod().SetMillis(60 * 60 * 1000);
            AssertCompute(defaultCurrent, oneHourInMsec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24), "2002-05-30T10:00:00.000");

            // second adds
            TimePeriod oneSec = new TimePeriod().SetSeconds(1);
            AssertCompute(defaultCurrent, oneSec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60 * 60L), "2002-05-30T09:51:02.000");
            AssertCompute(defaultCurrent, oneSec, "2002-05-30T09:51:00.150",
                    new LongAssertionAtLeast(2), "2002-05-30T09:51:02.150");
            AssertCompute(defaultCurrent, oneSec, "2002-05-30T09:51:00.151",
                    new LongAssertionAtLeast(1), "2002-05-30T09:51:01.151");
            AssertCompute(defaultCurrent, oneSec, "2002-05-30T09:51:01.149",
                    new LongAssertionAtLeast(1), "2002-05-30T09:51:02.149");
            AssertCompute(defaultCurrent, oneSec, "2002-05-30T09:51:01.150",
                    new LongAssertionAtLeast(1), "2002-05-30T09:51:02.150");
            AssertCompute(defaultCurrent, oneSec, "2002-05-30T09:51:01.151",
                    new LongAssertionAtLeast(0), "2002-05-30T09:51:01.151");

            // 10-second adds
            TimePeriod tenSec = new TimePeriod().SetSeconds(10);
            AssertCompute(defaultCurrent, tenSec, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60 * 6L), "2002-05-30T09:51:10.000");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:50:00.000",
                    new LongAssertionExact(7L), "2002-05-30T09:51:10.000");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:50:51.149",
                    new LongAssertionExact(2L), "2002-05-30T09:51:11.149");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:50:51.150",
                    new LongAssertionExact(2L), "2002-05-30T09:51:11.150");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:50:51.151",
                    new LongAssertionExact(1L), "2002-05-30T09:51:01.151");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:51:00.149",
                    new LongAssertionExact(1L), "2002-05-30T09:51:10.149");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:51:01.149",
                    new LongAssertionExact(1L), "2002-05-30T09:51:11.149");
            AssertCompute(defaultCurrent, tenSec, "2002-05-30T09:51:01.150",
                    new LongAssertionExact(1L), "2002-05-30T09:51:11.150");

            // minute adds
            TimePeriod oneMin = new TimePeriod().SetMinutes(1);
            AssertCompute(defaultCurrent, oneMin, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 60), "2002-05-30T09:52:00.000");

            // 10-minute adds
            TimePeriod tenMin = new TimePeriod().SetMinutes(10);
            AssertCompute(defaultCurrent, tenMin, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24 * 6), "2002-05-30T10:00:00.000");

            // 1-hour adds
            TimePeriod oneHour = new TimePeriod().SetHours(1);
            AssertCompute(defaultCurrent, oneHour, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365 * 24), "2002-05-30T10:00:00.000");

            // 1-day adds
            TimePeriod oneDay = new TimePeriod().SetDays(1);
            AssertCompute(defaultCurrent, oneDay, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 365), "2002-05-31T00:00:00.000");

            // 1-month adds
            TimePeriod oneMonth = new TimePeriod().SetMonths(1);
            AssertCompute(defaultCurrent, oneMonth, "1980-01-01T00:00:00.000",
                    new LongAssertionAtLeast(22 * 12), "2002-06-01T00:00:00.000");

            // 1-year adds
            TimePeriod oneYear = new TimePeriod().SetYears(1);
            AssertCompute(defaultCurrent, oneYear, "1980-01-01T00:00:00.000",
                    new LongAssertionExact(23), "2003-01-01T00:00:00.000");

            // Uneven adds
            TimePeriod unevenOne = new TimePeriod().SetYears(1).SetMonths(2).SetDays(3);
            AssertCompute(defaultCurrent, unevenOne, "1980-01-01T00:00:00.000",
                    new LongAssertionExact(20), "2003-06-30T00:00:00.000");
            AssertCompute(defaultCurrent, unevenOne, "2002-01-01T00:00:00.000",
                    new LongAssertionExact(1), "2003-03-04T00:00:00.000");
            AssertCompute(defaultCurrent, unevenOne, "2001-01-01T00:00:00.000",
                    new LongAssertionExact(2), "2003-05-07T00:00:00.000");
        }

        private void AssertCompute(long current, TimePeriod timePeriod, string reference,
                                   LongAssertion factorAssertion, string expectedTarget)
        {
            var referenceDate = DateTimeParsingFunctions.ParseDefaultEx(reference);
            CalendarOpPlusFastAddResult result = CalendarOpPlusFastAddHelper.ComputeNextDue(current, timePeriod, referenceDate, TimeAbacusMilliseconds.INSTANCE, 0);
            Assert.That(
                result.Scheduled,
                Is.EqualTo(DateTimeParsingFunctions.ParseDefaultEx(expectedTarget)));
            factorAssertion.AssertLong(result.Factor);
        }

        internal interface LongAssertion
        {
            void AssertLong(long value);
        }

        internal class LongAssertionExact : LongAssertion
        {
            private readonly long expected;

            internal LongAssertionExact(long expected)
            {
                this.expected = expected;
            }

            public void AssertLong(long value)
            {
                Assert.AreEqual(expected, value);
            }
        }

        internal class LongAssertionAtLeast : LongAssertion
        {
            private readonly long atLeast;

            internal LongAssertionAtLeast(long atLeast)
            {
                this.atLeast = atLeast;
            }

            public void AssertLong(long value)
            {
                Assert.IsTrue(value >= atLeast);
            }
        }
    }
} // end of namespace
