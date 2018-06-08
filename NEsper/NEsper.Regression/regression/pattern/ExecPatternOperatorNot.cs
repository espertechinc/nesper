///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorNot : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
        }

        public override bool ExcludeWhenInstrumented() {
            return false;
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionUniformEvents(epService);
            RunAssertionNotTimeInterval(epService);
            RunAssertionNotFollowedBy(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and not d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            string text = "select * from pattern [every b=" + EVENT_B_CLASS + " and not g=" + EVENT_G_CLASS + "]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.And()
                    .Add(Patterns.EveryFilter(EVENT_B_CLASS, "b"))
                    .Add(Patterns.NotFilter(EVENT_G_CLASS, "g"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " and not g=" + EVENT_G_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " and not d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and not a=" + EVENT_A_CLASS + "(id='A1')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and not a2=" + EVENT_A_CLASS + "(id='A2')");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " and not b3=" + EVENT_B_CLASS + "(id='B3'))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " or not " + EVENT_D_CLASS + "())");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (every b=" + EVENT_B_CLASS + " and not " + EVENT_B_CLASS + "(id='B2'))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " and not " + EVENT_B_CLASS + "(id='B2'))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + ") and " +
                    " not " + EVENT_A_CLASS);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + ") and " +
                    " not " + EVENT_G_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + ") and " +
                    " not " + EVENT_G_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + ") and " +
                    " not " + EVENT_G_CLASS + "(id='x')");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionUniformEvents(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetSetTwoExternalClock(0, 1000);
            var results = new CaseList();
    
            var desc = new EventExpressionCase("every a=" + EVENT_A_CLASS + "() and not a1=" + EVENT_A_CLASS + "(id=\"A4\")");
            desc.Add("B1", "a", events.GetEvent("B1"));
            desc.Add("B2", "a", events.GetEvent("B2"));
            desc.Add("B3", "a", events.GetEvent("B3"));
            results.AddTest(desc);
    
            var util = new PatternTestHarness(events, results, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionNotTimeInterval(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("BBB", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("AAA", typeof(SupportMarketDataBean));
    
            string text = "select A.TheString as TheString from pattern " +
                    "[every A=BBB(IntPrimitive=123) -> (timer:interval(30 seconds) and not AAA(volume=123, symbol=A.TheString))]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendTimer(0, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 123));
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 123));
    
            SendTimer(20000, epService);
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("E1", 0, 123L, ""));
    
            SendTimer(30000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E3", 123));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(40000, epService);
            var fields = new string[]{"TheString"};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            statement.Stop();
        }
    
        private void RunAssertionNotFollowedBy(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SB", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("MD", typeof(SupportMarketDataBean));
    
            string stmtText = "select * from pattern [ Every( SB(IntPrimitive>0) -> (MD and not SB(IntPrimitive=0) ) ) ]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // A(a=1) A(a=2) A(a=0) A(a=3) ...
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("E5", "M1", 1d));
            Assert.IsTrue(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
