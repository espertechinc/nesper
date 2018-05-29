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
using com.espertech.esper.compat;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternGuardTimerWithin : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionInterval10Min(epService);
            RunAssertionInterval10MinVariable(epService);
            RunAssertionIntervalPrepared(epService);
            RunAssertionWithinFromExpression(epService);
            RunAssertionPatternNotFollowedBy(epService);
            RunAssertionWithinMayMaxMonthScoped(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:within(2 sec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:within(2001 msec)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:within(1999 msec)");
            testCaseList.AddTest(testCase);
    
            string text = "select * from pattern [b=" + EVENT_B_CLASS + "(id=\"B3\") where timer:within(10.001)]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Expression filter = Expressions.Eq("id", "B3");
            PatternExpr pattern = Patterns.TimerWithin(10.001, Patterns.Filter(Filter.Create(EVENT_B_CLASS, filter), "b"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B3\") where timer:within(10001 msec)");
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B3\") where timer:within(10 sec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B3\") where timer:within(9.999)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:within(2.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:within(4.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " where timer:within(2.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " where timer:within(2001 msec))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every ((every b=" + EVENT_B_CLASS + ") where timer:within(2.001))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every ((every b=" + EVENT_B_CLASS + ") where timer:within(6.001))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:within(11.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:within(4001 milliseconds)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + ") where timer:within(6.001)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + " where timer:within(4001 milliseconds)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() -> d=" + EVENT_D_CLASS + "() where timer:within(4 sec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() where timer:within (4.001) and d=" + EVENT_D_CLASS + "() where timer:within(6.001))");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2001 msec) and d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2001 msec) and d=" + EVENT_D_CLASS + "() where timer:within(6000 msec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2000 msec) and d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + " where timer:within(4000 msec)");
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> every d=" + EVENT_D_CLASS + " where timer:within(4000 msec)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() -> d=" + EVENT_D_CLASS + "() where timer:within(3999 msec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> (every d=" + EVENT_D_CLASS + ") where timer:within(2001 msec)");
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() -> d=" + EVENT_D_CLASS + "()) where timer:within(6001 msec)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2000 msec) or d=" + EVENT_D_CLASS + "() where timer:within(6000 msec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + "() where timer:within (2000 msec) or d=" + EVENT_D_CLASS + "() where timer:within(6000 msec)) where timer:within (1999 msec)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + "() where timer:within (2001 msec) and d=" + EVENT_D_CLASS + "() where timer:within(6001 msec))");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2001 msec) or d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() where timer:within (2000 msec) or d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCase.Add("D1", "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() where timer:within (2001 msec) and every d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:within (2000 msec) and every d=" + EVENT_D_CLASS + "() where timer:within(6001 msec)");
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionInterval10Min(EPServiceProvider epService) {
            // External clocking
            SendTimer(0, epService);
            Assert.AreEqual(0, epService.EPRuntime.CurrentTime);
    
            // Set up a timer:within
            EPStatement statement = epService.EPAdministrator.CreateEPL(
                    "select * from pattern [(every " + typeof(SupportBean).FullName +
                            ") where timer:within(1 days 2 hours 3 minutes 4 seconds 5 milliseconds)]");
    
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertion(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionInterval10MinVariable(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("D", typeof(double), 1);
            epService.EPAdministrator.Configuration.AddVariable("H", typeof(double), 2);
            epService.EPAdministrator.Configuration.AddVariable("M", typeof(double), 3);
            epService.EPAdministrator.Configuration.AddVariable("S", typeof(double), 4);
            epService.EPAdministrator.Configuration.AddVariable("MS", typeof(double), 5);
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            string stmtText = "select * from pattern [(every " + typeof(SupportBean).FullName +
                    ") where timer:within(D days H hours M minutes S seconds MS milliseconds)]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertion(epService, testListener);
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            statement.Dispose();
        }
    
        private void RunAssertionIntervalPrepared(EPServiceProvider epService) {
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(
                    "select * from pattern [(every " + typeof(SupportBean).FullName +
                            ") where timer:within(? days ? hours ? minutes ? seconds ? milliseconds)]");
            prepared.SetObject(1, 1);
            prepared.SetObject(2, 2);
            prepared.SetObject(3, 3);
            prepared.SetObject(4, 4);
            prepared.SetObject(5, 5);
            EPStatement statement = epService.EPAdministrator.Create(prepared);
    
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertion(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionWithinFromExpression(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // External clocking
            SendTimer(0, epService);
    
            // Set up a timer:within
            EPStatement statement = epService.EPAdministrator.CreateEPL("select b.TheString as id from pattern[a=SupportBean -> (every b=SupportBean) where timer:within(a.IntPrimitive seconds)]");
    
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            // seed
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
    
            SendTimer(2000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            Assert.AreEqual("E2", testListener.AssertOneGetNewAndReset().Get("id"));
    
            SendTimer(2999, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E3", -1));
            Assert.AreEqual("E3", testListener.AssertOneGetNewAndReset().Get("id"));
    
            SendTimer(3000, epService);
            Assert.IsFalse(testListener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionPatternNotFollowedBy(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SB", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("MD", typeof(SupportMarketDataBean));
            SendTimer(0, epService);
    
            string stmtText = "select * from pattern [ Every(SB -> (MD where timer:within(5 sec))) ]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            SendTimer(6000, epService);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("E5", "M1", 1d));
            Assert.IsTrue(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionWithinMayMaxMonthScoped(EPServiceProvider epService) {
            TryAssertionWithinMayMaxMonthScoped(epService, false);
            TryAssertionWithinMayMaxMonthScoped(epService, true);
        }
    
        private void TryAssertionWithinMayMaxMonthScoped(EPServiceProvider epService, bool hasMax) {
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from pattern [(every SupportBean) where " +
                    (hasMax ? "timer:withinmax(1 month, 10)" : "timer:within(1 month)") +
                    "]").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener testListener) {
            SendEvent(epService);
            testListener.AssertOneGetNewAndReset();
    
            long time = 24 * 60 * 60 * 1000 + 2 * 60 * 60 * 1000 + 3 * 60 * 1000 + 4 * 1000 + 5;
            SendTimer(time - 1, epService);
            Assert.AreEqual(time - 1, epService.EPRuntime.CurrentTime);
            SendEvent(epService);
            testListener.AssertOneGetNewAndReset();
    
            SendTimer(time, epService);
            SendEvent(epService);
            Assert.IsFalse(testListener.IsInvoked);
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService) {
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
