///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.schedule;
using com.espertech.esper.support.guard;
using com.espertech.esper.support.pattern;
using com.espertech.esper.support.schedule;
using com.espertech.esper.support.view;
using com.espertech.esper.timer;

using NUnit.Framework;

namespace com.espertech.esper.pattern.observer
{
    [TestFixture]
    public class TestTimerIntervalObserver
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _beginState = new MatchedEventMapImpl(new MatchedEventMapMeta(new String[0], false));

            _scheduleService = new SchedulingServiceImpl(new TimeSourceServiceImpl());
            StatementContext stmtContext = SupportStatementContextFactory.MakeContext(_scheduleService);
            _context = new PatternContext(stmtContext, 1, new MatchedEventMapMeta(new String[0], false), false);
            _agentContext = SupportPatternContextFactory.MakePatternAgentInstanceContext(_scheduleService);

            _evaluator = new SupportObserverEvaluator(_agentContext);

            _observer = new TimerIntervalObserver(1000, _beginState, _evaluator);
        }

        #endregion

        private PatternContext _context;
        private PatternAgentInstanceContext _agentContext;

        private TimerIntervalObserver _observer;
        private SchedulingServiceImpl _scheduleService;
        private SupportObserverEvaluator _evaluator;
        private MatchedEventMap _beginState;

        [Test]
        public void TestImmediateTrigger()
        {
            // Should fireStatementStopped right away, wait time set to zero
            _observer = new TimerIntervalObserver(0, _beginState, _evaluator);

            _scheduleService.Time = 0;
            _observer.StartObserve();
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);
            _scheduleService.Time = 10000000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(0, _evaluator.GetAndClearMatchEvents().Count);
        }

        [Test]
        public void TestInvalid()
        {
            try
            {
                _observer.StartObserve();
                _observer.StartObserve();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                // Expected exception
            }
        }

        [Test]
        public void TestStartAndObserve()
        {
            _scheduleService.Time = 0;
            _observer.StartObserve();
            _scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);

            // Test start again
            _observer.StartObserve();
            _scheduleService.Time = 1999;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(0, _evaluator.MatchEvents.Count);

            _scheduleService.Time = 2000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);
        }

        [Test]
        public void TestStartAndStop()
        {
            // Start then stop
            _scheduleService.Time = 0;
            _observer.StartObserve();
            _observer.StopObserve();
            _scheduleService.Time = 1000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(0, _evaluator.GetAndClearMatchEvents().Count);

            // Test start again
            _observer.StartObserve();
            _scheduleService.Time = 2500;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);

            _observer.StopObserve();
            _observer.StartObserve();

            _scheduleService.Time = 3500;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);
        }
    }
}