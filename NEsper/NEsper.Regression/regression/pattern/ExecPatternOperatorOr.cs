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
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorOr : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionOrAndNotAndZeroStart(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("a=" + EVENT_A_CLASS + " or a=" + EVENT_A_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("a=" + EVENT_A_CLASS + " or b=" + EVENT_B_CLASS + " or c=" + EVENT_C_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " or every d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCase.Add("D2", "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("D3", "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("a=" + EVENT_A_CLASS + " or b=" + EVENT_B_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("a=" + EVENT_A_CLASS + " or every b=" + EVENT_B_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + EVENT_A_CLASS + " or d=" + EVENT_D_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCase.Add("A2", "a", events.GetEvent("A2"));
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (every b=" + EVENT_B_CLASS + "() or d=" + EVENT_D_CLASS + "())");
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
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() or every d=" + EVENT_D_CLASS + "())");
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
    
            testCase = new EventExpressionCase("every (every d=" + EVENT_D_CLASS + "() or every b=" + EVENT_B_CLASS + "())");
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
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionOrAndNotAndZeroStart(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
            epService.EPAdministrator.Configuration.AddEventType("C", typeof(SupportBean_C));
    
            TryOrAndNot(epService, "(a=A -> b=B) or (a=A -> not b=B)");
            TryOrAndNot(epService, "a=A -> (b=B or not B)");
    
            // try zero-time start
            var listener = new SupportUpdateListener();
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("select * from pattern [timer:interval(0) or every timer:interval(1 min)]")
                .AddEventHandlerWithReplay(listener.Update);
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryOrAndNot(EPServiceProvider epService, string pattern) {
            string expression =
                    "select * " +
                            "from pattern [" + pattern + "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var eventA1 = new SupportBean_A("A1");
            epService.EPRuntime.SendEvent(eventA1);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.IsNull(theEvent.Get("b"));
    
            var eventB1 = new SupportBean_B("B1");
            epService.EPRuntime.SendEvent(eventB1);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventA1, theEvent.Get("a"));
            Assert.AreEqual(eventB1, theEvent.Get("b"));
    
            statement.Dispose();
        }
    }
} // end of namespace
