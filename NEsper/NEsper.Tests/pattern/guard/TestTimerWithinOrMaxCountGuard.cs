///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.schedule;
using com.espertech.esper.support.guard;
using com.espertech.esper.support.pattern;
using com.espertech.esper.support.schedule;
using com.espertech.esper.support.view;
using com.espertech.esper.timer;

using NUnit.Framework;



namespace com.espertech.esper.pattern.guard
{
    [TestFixture]
    public class TestTimerWithinOrMaxCountGuard  {
        private TimerWithinOrMaxCountGuard guard;
        private SchedulingService scheduleService;
        private SupportQuitable quitable;
    
        [SetUp]
        public void SetUp()
        {
            StatementContext stmtContext = SupportStatementContextFactory.MakeContext(new SchedulingServiceImpl(new TimeSourceServiceImpl()));
            scheduleService = stmtContext.SchedulingService;
            PatternAgentInstanceContext agentInstanceContext = SupportPatternContextFactory.MakePatternAgentInstanceContext(scheduleService);
    
            quitable = new SupportQuitable(agentInstanceContext);
    
            guard =  new TimerWithinOrMaxCountGuard(1000, 2, quitable);
        }
    
        [Test]
        public void TestInspect() {
            Assert.IsTrue(guard.Inspect(null));
        }
    
        [Test]
        public void TestInspect_max_count_exceeeded() {
            Assert.IsTrue(guard.Inspect(null));
            Assert.IsTrue(guard.Inspect(null));
            Assert.IsFalse(guard.Inspect(null));
        }
    
        [Test]
        public void TestStartAndTrigger_count() {
            guard.StartGuard();
    
            Assert.AreEqual(0, quitable.GetAndResetQuitCounter());
    
            guard.Inspect(null);
            guard.Inspect(null);
            guard.Inspect(null);
            scheduleService.Time = 1000;
    
            Assert.AreEqual(1, quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndTrigger_time() {
            scheduleService.Time = 0;
    
            guard.StartGuard();
    
            Assert.AreEqual(0, quitable.GetAndResetQuitCounter());
    
            scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(scheduleService);
    
            Assert.AreEqual(1, quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndTrigger_time_and_count() {
            scheduleService.Time = 0;
    
            guard.StartGuard();
    
            Assert.AreEqual(0, quitable.GetAndResetQuitCounter());
            guard.Inspect(null);
            guard.Inspect(null);
            guard.Inspect(null);
    
            scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(scheduleService);
    
            Assert.AreEqual(1, quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndStop() {
            scheduleService.Time = 0;
    
            guard.StartGuard();
    
            guard.StopGuard();
    
            scheduleService.Time = 1001;
            SupportSchedulingServiceImpl.EvaluateSchedule(scheduleService);
    
            Assert.AreEqual(0, quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestInvalid() {
            try {
                guard.StartGuard();
                guard.StartGuard();
                Assert.Fail();
            }
            catch (IllegalStateException ex) {
                // Expected exception
            }
        }
    }
}
