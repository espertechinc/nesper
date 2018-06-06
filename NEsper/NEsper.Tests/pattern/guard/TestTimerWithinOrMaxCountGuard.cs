///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.guard;
using com.espertech.esper.supportunit.pattern;
using com.espertech.esper.supportunit.schedule;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;
using com.espertech.esper.timer;

using NUnit.Framework;



namespace com.espertech.esper.pattern.guard
{
    [TestFixture]
    public class TestTimerWithinOrMaxCountGuard
    {
        private TimerWithinOrMaxCountGuard _guard;
        private SchedulingService _scheduleService;
        private SupportQuitable _quitable;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var stmtContext = SupportStatementContextFactory.MakeContext(
                _container, new SchedulingServiceImpl(new TimeSourceServiceImpl(), _container));
            _scheduleService = stmtContext.SchedulingService;
            var agentInstanceContext = SupportPatternContextFactory.MakePatternAgentInstanceContext(_scheduleService);
    
            _quitable = new SupportQuitable(agentInstanceContext);
    
            _guard =  new TimerWithinOrMaxCountGuard(1000, 2, _quitable);
        }
    
        [Test]
        public void TestInspect() {
            Assert.IsTrue(_guard.Inspect(null));
        }
    
        [Test]
        public void TestInspect_max_count_exceeeded() {
            Assert.IsTrue(_guard.Inspect(null));
            Assert.IsTrue(_guard.Inspect(null));
            Assert.IsFalse(_guard.Inspect(null));
        }
    
        [Test]
        public void TestStartAndTrigger_count() {
            _guard.StartGuard();
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
    
            _guard.Inspect(null);
            _guard.Inspect(null);
            _guard.Inspect(null);
            _scheduleService.Time = 1000;
    
            Assert.AreEqual(1, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndTrigger_time() {
            _scheduleService.Time = 0;
    
            _guard.StartGuard();
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
    
            _scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
    
            Assert.AreEqual(1, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndTrigger_time_and_count() {
            _scheduleService.Time = 0;
    
            _guard.StartGuard();
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
            _guard.Inspect(null);
            _guard.Inspect(null);
            _guard.Inspect(null);
    
            _scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
    
            Assert.AreEqual(1, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndStop() {
            _scheduleService.Time = 0;
    
            _guard.StartGuard();
    
            _guard.StopGuard();
    
            _scheduleService.Time = 1001;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestInvalid() {
            try {
                _guard.StartGuard();
                _guard.StartGuard();
                Assert.Fail();
            }
            catch (IllegalStateException) {
                // Expected exception
            }
        }
    }
}
