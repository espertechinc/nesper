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
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorEvery : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionEveryAndNot(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
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
            for (int i = 0; i < 3; i++) {
                testCase.Add("B2", "b", events.GetEvent("B2"));
            }
            for (int i = 0; i < 9; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (every b=" + EVENT_B_CLASS + "())");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            for (int i = 0; i < 4; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("Every( every (every (every b=" + EVENT_B_CLASS + "())))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            for (int i = 0; i < 4; i++) {
                testCase.Add("B2", "b", events.GetEvent("B2"));
            }
            for (int i = 0; i < 16; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"));
            }
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionEveryAndNot(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string expression =
                    "select 'No event within 6 seconds' as alert\n" +
                            "from pattern [ every (timer:interval(6) and not " + typeof(SupportBean).FullName + ") ]";
    
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(expression);
            Assert.IsFalse(statement.StatementContext.IsStatelessSelect);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendTimer(epService, 2000);
            epService.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(epService, 6000);
            SendTimer(epService, 7000);
            SendTimer(epService, 7999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 8000);
            Assert.AreEqual("No event within 6 seconds", listener.AssertOneGetNewAndReset().Get("alert"));
    
            SendTimer(epService, 12000);
            epService.EPRuntime.SendEvent(new SupportBean());
            SendTimer(epService, 13000);
            epService.EPRuntime.SendEvent(new SupportBean());
    
            SendTimer(epService, 18999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 19000);
            Assert.AreEqual("No event within 6 seconds", listener.AssertOneGetNewAndReset().Get("alert"));
    
            statement.Dispose();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
