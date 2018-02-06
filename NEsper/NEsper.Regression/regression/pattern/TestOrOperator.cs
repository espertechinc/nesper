///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestOrOperator : SupportBeanConstants
    {
        [Test]
        public void TestOp() {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            CaseList testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase(
                    "a=" + EVENT_A_CLASS + " or a=" + EVENT_A_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "a=" + EVENT_A_CLASS + " or b=" + EVENT_B_CLASS + " or c="
                    + EVENT_C_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "every b=" + EVENT_B_CLASS + " or every d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCase.Add("D2", "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("D3", "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "a=" + EVENT_A_CLASS + " or b=" + EVENT_B_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "a=" + EVENT_A_CLASS + " or every b=" + EVENT_B_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "every a=" + EVENT_A_CLASS + " or d=" + EVENT_D_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCase.Add("A2", "a", events.GetEvent("A2"));
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "every (every b=" + EVENT_B_CLASS + "() or d=" + EVENT_D_CLASS
                    + "())");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            for (int i = 0; i < 4; i++) {
                testCase.Add("D1", "d", events.GetEvent("D1"));
            }
            for (int i = 0; i < 4; i++) {
                testCase.Add("D2", "d", events.GetEvent("D2"));
            }
            for (int i = 0; i < 4; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            for (int i = 0; i < 8; i++) {
                testCase.Add("D3", "d", events.GetEvent("D3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "every (b=" + EVENT_B_CLASS + "() or every d=" + EVENT_D_CLASS
                    + "())");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCase.Add("D2", "d", events.GetEvent("D2"));
            testCase.Add("D2", "d", events.GetEvent("D2"));
            for (int i = 0; i < 4; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            for (int i = 0; i < 4; i++) {
                testCase.Add("D3", "d", events.GetEvent("D3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase(
                    "every (every d=" + EVENT_D_CLASS + "() or every b="
                    + EVENT_B_CLASS + "())");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            for (int i = 0; i < 4; i++) {
                testCase.Add("D1", "d", events.GetEvent("D1"));
            }
            for (int i = 0; i < 8; i++) {
                testCase.Add("D2", "d", events.GetEvent("D2"));
            }
            for (int i = 0; i < 16; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            for (int i = 0; i < 32; i++) {
                testCase.Add("D3", "d", events.GetEvent("D3"));
            }
            testCaseList.AddTest(testCase);

            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
    
            util.RunTest();
        }
    
        [Test]
        public void TestOrAndNotAndZeroStart()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            config.AddEventType("A", typeof(SupportBean_A).FullName);
            config.AddEventType("B", typeof(SupportBean_B).FullName);
            config.AddEventType("C", typeof(SupportBean_C).FullName);
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            TryOrAndNot(epService, "(a=A -> b=B) or (a=A -> not b=B)");
            TryOrAndNot(epService, "a=A -> (b=B or not B)");
    
            // try zero-time start
            SupportUpdateListener listener = new SupportUpdateListener();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("select * from pattern [timer:interval(0) or every timer:interval(1 min)]").AddEventHandlerWithReplay(listener.Update);
            Assert.IsTrue(listener.IsInvoked);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void TryOrAndNot(EPServiceProvider epService, String pattern)
        {
            String expression = "select * " + "from pattern [" + pattern + "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            statement.Events += listener.Update;
    
            Object eventA1 = new SupportBean_A("A1");
    
            epService.EPRuntime.SendEvent(eventA1);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
    
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.IsNull(theEvent.Get("b"));
    
            Object eventB1 = new SupportBean_B("B1");
    
            epService.EPRuntime.SendEvent(eventB1);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.AreEqual(eventB1, theEvent.Get("b"));
    
            statement.Dispose();
        }
    }
}
