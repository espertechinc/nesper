///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.timer
{
    /// <summary>
    ///     Ensure that TimeSourceMills and TimeSourceMills
    ///     agree on wall clock time.
    /// </summary>
    /// <author>Jerry Shea</author>
    [TestFixture]
    public class TestTimeSource : AbstractRuntimeTest
    {
        [TearDown]
        public void TearDown()
        {
            TimeSourceServiceImpl.IsSystemCurrentTime = true;
        }

        private void AssertTimeWithinTolerance(
            long TOLERANCE_MILLISECS,
            TimeSourceService nanos,
            TimeSourceService millis)
        {
            TimeSourceServiceImpl.IsSystemCurrentTime = true;
            var nanosWallClockTime = nanos.TimeMillis;

            TimeSourceServiceImpl.IsSystemCurrentTime = false;
            var millisWallClockTime = millis.TimeMillis;

            var diff = nanosWallClockTime - millisWallClockTime;
            log.Info("diff=" + diff + " between " + nanos + " and " + millis);

            Assert.That(Math.Abs(diff), Is.LessThan(TOLERANCE_MILLISECS));
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test, RunInApplicationDomain]
        public void TestWallClock()
        {
            // allow a tolerance as TimeSourceMillis resolution may be around 16ms
            int TOLERANCE_MILLISECS = 50, DELAY_MILLISECS = 100;

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
    }
} // end of namespace
