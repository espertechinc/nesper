///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.timer
{
    /// <summary>Ensure that TimeSourceMills and TimeSourceMills agree on wall clock time. </summary>
    /// <author>Jerry Shea</author>
    [TestFixture]
    public class TestTimeSource
    {
        [TearDown]
        public void TearDown()
        {
            TimeSourceServiceImpl.IsSystemCurrentTime = true;
        }

        [Test]
        public void TestWallClock()
        {
            // allow a tolerance as TimeSourceMillis resolution may be around 16ms
            const long TOLERANCE_MILLISECS = 50;
            const int DELAY_MILLISECS = 100;

            // This is a smoke test
            TimeSourceService nanos = new TimeSourceServiceImpl();
            TimeSourceService millis = new TimeSourceServiceImpl();

            AssertTimeWithinTolerance(TOLERANCE_MILLISECS, nanos, millis);
            Thread.Sleep(DELAY_MILLISECS);
            AssertTimeWithinTolerance(TOLERANCE_MILLISECS, nanos, millis);
            Thread.Sleep(DELAY_MILLISECS);
            AssertTimeWithinTolerance(TOLERANCE_MILLISECS, nanos, millis);
            Thread.Sleep(DELAY_MILLISECS);
            AssertTimeWithinTolerance(TOLERANCE_MILLISECS, nanos, millis);
        }

        private void AssertTimeWithinTolerance(long TOLERANCE_MILLISECS, TimeSourceService nanos, TimeSourceService millis)
        {
            TimeSourceServiceImpl.IsSystemCurrentTime = true;
            long nanosWallClockTime = nanos.GetTimeMillis();

            TimeSourceServiceImpl.IsSystemCurrentTime = false;
            long millisWallClockTime = millis.GetTimeMillis();

            long diff = nanosWallClockTime - millisWallClockTime;
            log.Info("diff=" + diff + " between " + nanos + " and " + millis);
            Assert.IsTrue(Math.Abs(diff) < TOLERANCE_MILLISECS, "Diff " + diff + " >= " + TOLERANCE_MILLISECS);
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
