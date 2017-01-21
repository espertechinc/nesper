///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.support.timer;

using NUnit.Framework;

namespace com.espertech.esper.timer
{
    [TestFixture]
    public class TestTimerServiceImpl 
    {
        private SupportTimerCallback callback;
        private TimerServiceImpl service;
    
        [SetUp]
        public void SetUp()
        {
            callback = new SupportTimerCallback();
            service = new TimerServiceImpl(null, 100);
            service.Callback = callback.HandleTimerEvent;
        }
    
        [Test]
        public void TestClocking()
        {
            int RESOLUTION = (int) service.MsecTimerResolution;
    
            // Wait .55 sec
            Assert.IsTrue(callback.GetAndResetCount() == 0);
            service.StartInternalClock();
            Sleep(RESOLUTION * 5 + RESOLUTION / 2);
            service.StopInternalClock(true);
            Assert.AreEqual(6, callback.GetAndResetCount());
    
            // Check if truely stopped
            Sleep(RESOLUTION);
            Assert.IsTrue(callback.GetAndResetCount() == 0);
    
            // Loop for some clock cycles
            service.StartInternalClock();
            Sleep(RESOLUTION / 10);
            Assert.IsTrue(callback.GetAndResetCount() == 1);
            Sleep(service.MsecTimerResolution * 20);
            long count = callback.GetAndResetCount();
            Log.Debug(".testClocking count=" + count);
            Assert.IsTrue(count >= 19);
    
            // Stop and check again
            service.StopInternalClock(true);
            Sleep(RESOLUTION);
            Assert.IsTrue(callback.Count <= 1);
    
            // Try some starts and stops to see
            service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            service.StartInternalClock();
            Assert.IsTrue(callback.GetAndResetCount() >= 1);
    
            Sleep(RESOLUTION / 5);
            Assert.AreEqual(0, callback.Count);
            Sleep(RESOLUTION);
            Assert.IsTrue(callback.Count >= 1);
            Sleep(RESOLUTION);
            Assert.IsTrue(callback.Count >= 1);
    
            Sleep(RESOLUTION * 5);
            Assert.IsTrue(callback.GetAndResetCount() >= 7);
    
            service.StopInternalClock(true);
            callback.GetAndResetCount();
            service.StopInternalClock(true);
            Sleep(RESOLUTION * 2);
            Assert.IsTrue(callback.Count == 0);
        }
    
        private static void Sleep(long msec)
        {
            Thread.Sleep((int) msec);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
