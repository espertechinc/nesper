///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.expression.time;

using NUnit.Framework;

namespace com.espertech.esper.epl.datetime
{
    [TestFixture]
    public class TestCalendarOpPlusFastAddHelper
    {
        [Test]
        public void TestCompute()
        {
            var defaultCurrent = DateTimeParser.ParseDefaultMSec("2002-05-30T9:51:01.150");

            // millisecond adds
            var oneMsec = new TimePeriod().SetMillis(1);
            AssertCompute(
                defaultCurrent, oneMsec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60*60*1000L), "2002-05-30T09:51:01.151");
            AssertCompute(
                defaultCurrent, oneMsec, "2001-06-01T0:00:00.000",
                new LongAssertionAtLeast(363*24*60*60*1000L), "2002-05-30T09:51:01.151");

            // 10-millisecond adds
            var tenMsec = new TimePeriod().SetMillis(10);
            AssertCompute(
                defaultCurrent, tenMsec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60*60*100L), "2002-05-30T09:51:01.160");

            // 100-millisecond adds
            var hundredMsec = new TimePeriod().SetMillis(100);
            AssertCompute(
                defaultCurrent, hundredMsec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60*60*10L), "2002-05-30T09:51:01.200");

            // 1-hour-in-millisecond adds
            var oneHourInMsec = new TimePeriod().SetMillis(60*60*1000);
            AssertCompute(
                defaultCurrent, oneHourInMsec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24), "2002-05-30T10:00:00.000");

            // second adds
            var oneSec = new TimePeriod().SetSeconds(1);
            AssertCompute(
                defaultCurrent, oneSec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60*60L), "2002-05-30T9:51:02.000");
            AssertCompute(
                defaultCurrent, oneSec, "2002-05-30T9:51:00.150",
                new LongAssertionAtLeast(2), "2002-05-30T9:51:02.150");
            AssertCompute(
                defaultCurrent, oneSec, "2002-05-30T9:51:00.151",
                new LongAssertionAtLeast(1), "2002-05-30T9:51:01.151");
            AssertCompute(
                defaultCurrent, oneSec, "2002-05-30T9:51:01.149",
                new LongAssertionAtLeast(1), "2002-05-30T9:51:02.149");
            AssertCompute(
                defaultCurrent, oneSec, "2002-05-30T9:51:01.150",
                new LongAssertionAtLeast(1), "2002-05-30T9:51:02.150");
            AssertCompute(
                defaultCurrent, oneSec, "2002-05-30T9:51:01.151",
                new LongAssertionAtLeast(0), "2002-05-30T9:51:01.151");

            // 10-second adds
            var tenSec = new TimePeriod().SetSeconds(10);
            AssertCompute(
                defaultCurrent, tenSec, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60*6L), "2002-05-30T09:51:10.000");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:50:00.000",
                new LongAssertionExact(7L), "2002-05-30T9:51:10.000");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:50:51.149",
                new LongAssertionExact(2L), "2002-05-30T09:51:11.149");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:50:51.150",
                new LongAssertionExact(2L), "2002-05-30T09:51:11.150");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:50:51.151",
                new LongAssertionExact(1L), "2002-05-30T09:51:01.151");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:51:00.149",
                new LongAssertionExact(1L), "2002-05-30T9:51:10.149");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:51:01.149",
                new LongAssertionExact(1L), "2002-05-30T9:51:11.149");
            AssertCompute(
                defaultCurrent, tenSec, "2002-05-30T9:51:01.150",
                new LongAssertionExact(1L), "2002-05-30T9:51:11.150");

            // minute adds
            var oneMin = new TimePeriod().SetMinutes(1);
            AssertCompute(
                defaultCurrent, oneMin, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*60), "2002-05-30T9:52:00.000");

            // 10-minute adds
            var tenMin = new TimePeriod().SetMinutes(10);
            AssertCompute(
                defaultCurrent, tenMin, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24*6), "2002-05-30T10:00:00.000");

            // 1-hour adds
            var oneHour = new TimePeriod().SetHours(1);
            AssertCompute(
                defaultCurrent, oneHour, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365*24), "2002-05-30T10:00:00.000");

            // 1-day adds
            var oneDay = new TimePeriod().SetDays(1);
            AssertCompute(
                defaultCurrent, oneDay, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*365), "2002-05-31T00:00:00.000");

            // 1-month adds
            var oneMonth = new TimePeriod().SetMonths(1);
            AssertCompute(
                defaultCurrent, oneMonth, "1980-01-01T0:00:00.000",
                new LongAssertionAtLeast(22*12), "2002-06-01T00:00:00.000");

            // 1-year adds
            var oneYear = new TimePeriod().SetYears(1);
            AssertCompute(
                defaultCurrent, oneYear, "1980-01-01T0:00:00.000",
                new LongAssertionExact(23), "2003-01-01T00:00:00.000");

            // Uneven adds
            var unevenOne = new TimePeriod().SetYears(1).SetMonths(2).SetDays(3);
            AssertCompute(
                defaultCurrent, unevenOne, "1980-01-01T0:00:00.000",
                new LongAssertionExact(20), "2003-06-30T00:00:00.000");
            AssertCompute(
                defaultCurrent, unevenOne, "2002-01-01T0:00:00.000",
                new LongAssertionExact(1), "2003-03-04T00:00:00.000");
            AssertCompute(
                defaultCurrent, unevenOne, "2001-01-01T0:00:00.000",
                new LongAssertionExact(2), "2003-05-07T00:00:00.000");
        }

        private void AssertCompute(
            long current,
            TimePeriod timePeriod,
            string reference,
            LongAssertion factorAssertion,
            string expectedTarget)
        {
            var referenceDate = DateTimeParser.ParseDefaultEx(reference);
            // new DateTimeEx(DateTimeParser.ParseDefault(reference), TimeZoneInfo.Local);
            var result = CalendarOpPlusFastAddHelper.ComputeNextDue(current, timePeriod, referenceDate, TimeAbacusMilliseconds.INSTANCE, 0);
            Assert.AreEqual(
                DateTimeParser.ParseDefaultEx(expectedTarget), result.Scheduled,
                string.Format("\nExpected {0}\n" + "Received {1}\n", expectedTarget, DateTimeHelper.Print(result.Scheduled.DateTime)));
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
            private readonly long _atLeast;

            internal LongAssertionAtLeast(long atLeast)
            {
                _atLeast = atLeast;
            }

            public void AssertLong(long value)
            {
                Assert.IsTrue(value >= _atLeast);
            }
        }
    }
} // end of namespace