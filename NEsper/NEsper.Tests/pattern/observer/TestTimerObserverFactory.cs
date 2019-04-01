///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.supportunit.pattern;
using com.espertech.esper.view;

using NUnit.Framework;

namespace com.espertech.esper.pattern.observer
{
    [TestFixture]
    public class TestTimerObserverFactory 
    {
        private PatternAgentInstanceContext _patternContext;
    
        [SetUp]
        public void SetUp()
        {
            _patternContext = SupportPatternContextFactory.MakePatternAgentInstanceContext();
        }
    
        [Test]
        public void TestIntervalWait()
        {
            var factory = new TimerIntervalObserverFactory();
            factory.SetObserverParameters(TestViewSupport.ToExprListBean(new Object[] {1}), new SupportMatchedEventConvertor(), null);
            var eventObserver = factory.MakeObserver(_patternContext, null, new SupportObserverEventEvaluator(_patternContext), null, null, false);
    
            Assert.IsTrue(eventObserver is TimerIntervalObserver);
        }

        private class SupportObserverEventEvaluator : ObserverEventEvaluator
        {
            private readonly PatternAgentInstanceContext _patternContext;

            public SupportObserverEventEvaluator(PatternAgentInstanceContext patternContext)
            {
                _patternContext = patternContext;
            }

            public void ObserverEvaluateTrue(MatchedEventMap matchEvent, bool quitted)
            {
            }

            public void ObserverEvaluateFalse(bool restartable)
            {
            }

            public PatternAgentInstanceContext Context
            {
                get { return _patternContext; }
            }
        }
    }
}
