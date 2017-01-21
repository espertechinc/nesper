///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestEveryOperator : SupportBeanConstants
    {
        [Test]
        public void TestOp()
        {
            var events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (every (every b=" + EVENT_B_CLASS + "))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            for (var i = 0; i < 3; i++)
            {
                testCase.Add("B2", "b", events.GetEvent("B2"));
            }
            for (var i = 0; i < 9; i++)
            {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (every b=" + EVENT_B_CLASS + "())");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            for (var i = 0; i < 4; i++)
            {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every( every (every (every b=" + EVENT_B_CLASS + "())))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            for (var i = 0; i < 4; i++)
            {
                testCase.Add("B2", "b", events.GetEvent("B2"));
            }
            for (var i = 0; i < 16; i++)
            {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);

            var util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        [Test]
        public void TestEveryAndNot()
        {
            var config = SupportConfigFactory.GetConfiguration();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            SendTimer(engine, 0);
            var expression =
                "select 'No event within 6 seconds' as alert\n" +
                        "from pattern [ every (timer:interval(6) and not " + typeof(SupportBean).FullName + ") ]";
    
            var statement = (EPStatementSPI) engine.EPAdministrator.CreateEPL(expression);
            Assert.IsFalse(statement.StatementContext.IsStatelessSelect);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendTimer(engine, 2000);
            engine.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(engine, 6000);
            SendTimer(engine, 7000);
            SendTimer(engine, 7999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(engine, 8000);
            Assert.AreEqual("No event within 6 seconds", listener.AssertOneGetNewAndReset().Get("alert"));
    
            SendTimer(engine, 12000);
            engine.EPRuntime.SendEvent(new SupportBean());
            SendTimer(engine, 13000);
            engine.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(engine, 18999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(engine, 19000);
            Assert.AreEqual("No event within 6 seconds", listener.AssertOneGetNewAndReset().Get("alert"));
    
            engine.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void SendTimer(EPServiceProvider engine, long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = engine.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
