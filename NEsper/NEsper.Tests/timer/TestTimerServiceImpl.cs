///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.timer;

using NUnit.Framework;

namespace com.espertech.esper.timer
{
    [TestFixture]
    public class TestTimerServiceImpl 
    {
        private SupportTimerCallback _callback;
        private TimerServiceImpl _service;
    
        [SetUp]
        public void SetUp()
        {
            _callback = new SupportTimerCallback();
            _service = new TimerServiceImpl(null, 100);
            _service.Callback = _callback.HandleTimerEvent;
        }
    
        [Test]
        public void TestClocking()
        {
            int RESOLUTION = (int) _service.MsecTimerResolution;
    
            // Wait .55 sec
            Assert.IsTrue(_callback.GetAndResetCount() == 0);
            _service.StartInternalClock();
            Sleep(RESOLUTION * 5 + RESOLUTION / 2);
            _service.StopInternalClock(true);
            Assert.AreEqual(6, _callback.GetAndResetCount());
    
            // Check if truely stopped
            Sleep(RESOLUTION);
            Assert.IsTrue(_callback.GetAndResetCount() == 0);
    
            // Loop for some clock cycles
            _service.StartInternalClock();
            Sleep(RESOLUTION / 10);
            Assert.IsTrue(_callback.GetAndResetCount() == 1);
            Sleep(_service.MsecTimerResolution * 20);
            long count = _callback.GetAndResetCount();
            Log.Debug(".testClocking count=" + count);
            Assert.IsTrue(count >= 19);
    
            // Stop and check again
            _service.StopInternalClock(true);
            Sleep(RESOLUTION);
            Assert.IsTrue(_callback.Count <= 1);
    
            // Try some starts and stops to see
            _service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            _service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            _service.StartInternalClock();
            Assert.IsTrue(_callback.GetAndResetCount() >= 1);
    
            Sleep(RESOLUTION / 5);
            Assert.AreEqual(0, _callback.Count);
            Sleep(RESOLUTION);
            Assert.IsTrue(_callback.Count >= 1);
            Sleep(RESOLUTION);
            Assert.IsTrue(_callback.Count >= 1);
    
            Sleep(RESOLUTION * 5);
            Assert.IsTrue(_callback.GetAndResetCount() >= 7);
    
            _service.StopInternalClock(true);
            _callback.GetAndResetCount();
            _service.StopInternalClock(true);
            Sleep(RESOLUTION * 2);
            Assert.IsTrue(_callback.Count == 0);
        }
    
        private static void Sleep(long msec)
        {
            Thread.Sleep((int) msec);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
