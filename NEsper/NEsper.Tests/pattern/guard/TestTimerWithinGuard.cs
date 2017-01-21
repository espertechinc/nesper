///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class TestTimerWithinGuard 
    {
        private TimerWithinGuard _guard;
        private SchedulingService _scheduleService;
        private SupportQuitable _quitable;
    
        [SetUp]
        public void SetUp()
        {
            StatementContext stmtContext = SupportStatementContextFactory.MakeContext(new SchedulingServiceImpl(new TimeSourceServiceImpl()));
            _scheduleService = stmtContext.SchedulingService;
            PatternAgentInstanceContext agentInstanceContext = SupportPatternContextFactory.MakePatternAgentInstanceContext(_scheduleService);
    
            _quitable = new SupportQuitable(agentInstanceContext);
    
            _guard =  new TimerWithinGuard(1000, _quitable);
        }
    
        [Test]
        public void TestInspect()
        {
            Assert.IsTrue(_guard.Inspect(null));
        }
    
        /// <summary>Make sure the timer calls guardQuit after the set time period </summary>
        [Test]
        public void TestStartAndTrigger()
        {
            _scheduleService.Time = 0;
    
            _guard.StartGuard();
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
    
            _scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
    
            Assert.AreEqual(1, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestStartAndStop()
        {
            _scheduleService.Time = 0;
    
            _guard.StartGuard();
    
            _guard.StopGuard();
    
            _scheduleService.Time = 1001;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);        
    
            Assert.AreEqual(0, _quitable.GetAndResetQuitCounter());
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                _guard.StartGuard();
                _guard.StartGuard();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                // Expected exception
            }
        }
    }
}
