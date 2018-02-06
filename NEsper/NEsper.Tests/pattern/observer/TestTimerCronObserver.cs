///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.guard;
using com.espertech.esper.supportunit.pattern;
using com.espertech.esper.supportunit.schedule;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.timer;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.pattern.observer
{
    [TestFixture]
    public class TestTimerCronObserver 
    {
        private TimerAtObserver _observer;
        private SchedulingServiceImpl _scheduleService;
        private SupportObserverEvaluator _evaluator;
        private MatchedEventMap _beginState;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _beginState = new MatchedEventMapImpl(new MatchedEventMapMeta(new String[0], false));
    
            _scheduleService = new SchedulingServiceImpl(new TimeSourceServiceImpl(), _container);
            PatternAgentInstanceContext agentContext = SupportPatternContextFactory.MakePatternAgentInstanceContext(_scheduleService);
    
            ScheduleSpec scheduleSpec = new ScheduleSpec();
            scheduleSpec.AddValue(ScheduleUnit.SECONDS, 1);
    
            _evaluator = new SupportObserverEvaluator(agentContext);
    
            _observer =  new TimerAtObserver(scheduleSpec, _beginState, _evaluator);
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
            _scheduleService.Time = 60999;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(0, _evaluator.MatchEvents.Count);
    
            _scheduleService.Time = 61000; // 1 minute plus 1 second
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
            _scheduleService.Time = 61000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);
    
            _observer.StopObserve();
            _observer.StartObserve();
    
            _scheduleService.Time = 150000;
            SupportSchedulingServiceImpl.EvaluateSchedule(_scheduleService);
            Assert.AreEqual(_beginState, _evaluator.GetAndClearMatchEvents()[0]);
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
            catch (IllegalStateException)
            {
                // Expected exception
            }
        }
    
    }
}
