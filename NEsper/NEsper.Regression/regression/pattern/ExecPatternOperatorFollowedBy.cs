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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorFollowedBy : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionFollowedByWithNot(epService);
            RunAssertionFollowedByTimer(epService);
            RunAssertionMemoryRFIDEvent(epService);
            RunAssertionRFIDZoneExit(epService);
            RunAssertionRFIDZoneEnter(epService);
            RunAssertionFollowedNotEvery(epService);
            RunAssertionFollowedEveryMultiple(epService);
            RunAssertionFilterGreaterThen(epService);
            RunAssertionFollowedOrPermFalse(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> (d=" + EVENT_D_CLASS + " or not d=" + EVENT_D_CLASS + ")");
            testCase.Add("B1", "b", events.GetEvent("B1"), "d", null);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -[1000]> (d=" + EVENT_D_CLASS + " or not d=" + EVENT_D_CLASS + ")");
            testCase.Add("B1", "b", events.GetEvent("B1"), "d", null);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> every d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> not d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -[1000]> not d=" + EVENT_D_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " -> every d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " -[10]> d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " -> every d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (a_1=" + EVENT_A_CLASS + "() -> b=" + EVENT_B_CLASS + " -> a_2=" + EVENT_A_CLASS + ")");
            testCase.Add("A2", "a_1", events.GetEvent("A1"), "b", events.GetEvent("B1"), "a_2", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("c=" + EVENT_C_CLASS + "() -> d=" + EVENT_D_CLASS + " -> a=" + EVENT_A_CLASS);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (a_1=" + EVENT_A_CLASS + "() -> b=" + EVENT_B_CLASS + "() -> a_2=" + EVENT_A_CLASS + "())");
            testCase.Add("A2", "a_1", events.GetEvent("A1"), "b", events.GetEvent("B1"), "a_2", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (a_1=" + EVENT_A_CLASS + "() -[10]> b=" + EVENT_B_CLASS + "() -[10]> a_2=" + EVENT_A_CLASS + "())");
            testCase.Add("A2", "a_1", events.GetEvent("A1"), "b", events.GetEvent("B1"), "a_2", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every ( every a=" + EVENT_A_CLASS + " -> every b=" + EVENT_B_CLASS + ")");
            testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
            testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
            testCase.Add("B3", "a", events.GetEvent("A1"), "b", events.GetEvent("B3"));
            testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
            testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
            testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (a=" + EVENT_A_CLASS + "() -> every b=" + EVENT_B_CLASS + "())");
            testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
            testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
            testCase.Add("B3", "a", events.GetEvent("A1"), "b", events.GetEvent("B3"));
            testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
            testCase.Add("B3", "a", events.GetEvent("A2"), "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionFollowedByWithNot(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
            epService.EPAdministrator.Configuration.AddEventType("C", typeof(SupportBean_C));
    
            string stmt =
                    "select * from pattern [" +
                            " every a=A -> (timer:interval(10 seconds) and not (B(id=a.id) or C(id=a.id)))" +
                            "] ";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmt);
            Assert.AreEqual(StatementType.SELECT, ((EPStatementSPI) statement).StatementMetadata.StatementType);
            statement.Events += listener.Update;
    
            SupportBean_A eventA;
            EventBean received;
            SendTimer(0, epService);
    
            // test case where no Completed or Cancel event arrives
            eventA = SendA("A1", epService);
            SendTimer(9999, epService);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(10000, epService);
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventA, received.Get("a"));
    
            // test case where Completed event arrives within the time set
            SendTimer(20000, epService);
            SendA("A2", epService);
            SendTimer(29999, epService);
            SendB("A2", epService);
            SendTimer(30000, epService);
            Assert.IsFalse(listener.IsInvoked);
    
            // test case where Cancelled event arrives within the time set
            SendTimer(30000, epService);
            SendA("A3", epService);
            SendTimer(30000, epService);
            SendC("A3", epService);
            SendTimer(40000, epService);
            Assert.IsFalse(listener.IsInvoked);
    
            // test case where no matching Completed or Cancel event arrives
            eventA = SendA("A4", epService);
            SendB("B4", epService);
            SendC("A5", epService);
            SendTimer(50000, epService);
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventA, received.Get("a"));
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedByTimer(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("CallEvent", typeof(SupportCallEvent));
    
            string expression = "select * from pattern " +
                    "[every A=CallEvent -> every B=CallEvent(dest=A.dest, startTime in [A.startTime:A.endTime]) where timer:within (7200000)]" +
                    "where B.source != A.source";
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SupportCallEvent eventOne = SendEvent(epService.EPRuntime, 2000002601, "18", "123456789014795", DateToLong("2005-09-26 13:02:53.200"), DateToLong("2005-09-26 13:03:34.400"));
            SupportCallEvent eventTwo = SendEvent(epService.EPRuntime, 2000002607, "20", "123456789014795", DateToLong("2005-09-26 13:03:17.300"), DateToLong("2005-09-26 13:03:58.600"));
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreSame(eventOne, theEvent.Get("A"));
            Assert.AreSame(eventTwo, theEvent.Get("B"));
    
            SupportCallEvent eventThree = SendEvent(epService.EPRuntime, 2000002610, "22", "123456789014795", DateToLong("2005-09-26 13:03:31.300"), DateToLong("2005-09-26 13:04:12.100"));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(2, listener.LastNewData.Length);
            theEvent = listener.LastNewData[0];
            Assert.AreSame(eventOne, theEvent.Get("A"));
            Assert.AreSame(eventThree, theEvent.Get("B"));
            theEvent = listener.LastNewData[1];
            Assert.AreSame(eventTwo, theEvent.Get("A"));
            Assert.AreSame(eventThree, theEvent.Get("B"));
    
            statement.Dispose();
        }
    
        private void RunAssertionMemoryRFIDEvent(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("LR", typeof(SupportRFIDEvent));
    
            string expression =
                    "select 'Tag May Be Broken' as alert, " +
                            "tagMayBeBroken.mac, " +
                            "tagMayBeBroken.zoneID " +
                            "from pattern [" +
                            "every tagMayBeBroken=LR -> (timer:interval(10 sec) and not LR(mac=tagMayBeBroken.mac))" +
                            "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            for (int i = 0; i < 10; i++) {
                /*
                if (i % 1000 == 0)
                {
                    Log.Info(".testMemoryRFIDEvent now at " + i);
                }
                */
                var theEvent = new SupportRFIDEvent("a", "111");
                epService.EPRuntime.SendEvent(theEvent);
    
                theEvent = new SupportRFIDEvent("a", "111");
                epService.EPRuntime.SendEvent(theEvent);
            }
    
            statement.Dispose();
        }
    
        private void RunAssertionRFIDZoneExit(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("LR", typeof(SupportRFIDEvent));

            // Every LR event with a zone of '1' activates a new sub-expression after
            // the followed-by operator. The sub-expression instance can end two different ways:
            // It ends when a LR for the same mac and a different exit-zone comes in, or
            // it ends when a LR for the same max and the same zone come in. The latter also starts the
            // sub-expression again.

            string expression =
                    "select * " +
                            "from pattern [" +
                            "every a=LR(zoneID='1') -> (b=LR(mac=a.mac,zoneID!='1') and not LR(mac=a.mac,zoneID='1'))" +
                            "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var theEvent = new SupportRFIDEvent("a", "1");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("a", "2");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Get("b"));
    
            theEvent = new SupportRFIDEvent("b", "1");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("b", "1");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("b", "2");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Get("b"));
    
            statement.Dispose();
        }
    
        private void RunAssertionRFIDZoneEnter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("LR", typeof(SupportRFIDEvent));
    
            // Every LR event with a zone other then '1' activates a new sub-expression after
            // the followed-by operator. The sub-expression instance can end two different ways:
            // It ends when a LR for the same mac and the enter-zone comes in, or
            // it ends when a LR for the same max and the same zone come in. The latter also starts the
            // sub-expression again.

            string expression =
                    "select * " +
                            "from pattern [" +
                            "every a=LR(zoneID!='1') -> (b=LR(mac=a.mac,zoneID='1') and not LR(mac=a.mac,zoneID=a.zoneID))" +
                            "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var theEvent = new SupportRFIDEvent("a", "2");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("a", "1");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Get("b"));
    
            theEvent = new SupportRFIDEvent("b", "2");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("b", "2");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(listener.IsInvoked);
    
            theEvent = new SupportRFIDEvent("b", "1");
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Get("b"));
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedNotEvery(EPServiceProvider epService) {
            string expression = "select * from pattern [every A=" + typeof(SupportBean).FullName +
                    " -> (timer:interval(1 seconds) and not " + typeof(SupportBean_A).FullName + ")]";
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var eventOne = new SupportBean();
            epService.EPRuntime.SendEvent(eventOne);
    
            var eventTwo = new SupportBean();
            epService.EPRuntime.SendEvent(eventTwo);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(2, listener.NewDataList[0].Length);
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedEveryMultiple(EPServiceProvider epService) {
            string expression = "select * from pattern [every a=" + typeof(SupportBean_A).FullName +
                    " -> b=" + typeof(SupportBean_B).FullName +
                    " -> c=" + typeof(SupportBean_C).FullName +
                    " -> d=" + typeof(SupportBean_D).FullName +
                    "]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var events = new Object[10];
            events[0] = new SupportBean_A("A1");
            epService.EPRuntime.SendEvent(events[0]);
    
            events[1] = new SupportBean_A("A2");
            epService.EPRuntime.SendEvent(events[1]);
    
            events[2] = new SupportBean_B("B1");
            epService.EPRuntime.SendEvent(events[2]);
    
            events[3] = new SupportBean_C("C1");
            epService.EPRuntime.SendEvent(events[3]);
            Assert.IsFalse(listener.IsInvoked);
    
            events[4] = new SupportBean_D("D1");
            epService.EPRuntime.SendEvent(events[4]);
            Assert.AreEqual(2, listener.LastNewData.Length);
            var fields = new string[]{"a", "b", "c", "d"};
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{events[0], events[2], events[3], events[4]});
            EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[]{events[1], events[2], events[3], events[4]});
    
            statement.Dispose();
        }
    
        private void RunAssertionFilterGreaterThen(EPServiceProvider epService) {
            // ESPER-411
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement statement = epService.EPAdministrator.CreatePattern("every a=SupportBean -> b=SupportBean(b.IntPrimitive <= a.IntPrimitive)");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
            statement = epService.EPAdministrator.CreatePattern("every a=SupportBean -> b=SupportBean(a.IntPrimitive >= b.IntPrimitive)");
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedOrPermFalse(EPServiceProvider epService) {
    
            // ESPER-451
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            string pattern = "every s=SupportBean(TheString='E') -> " +
                    "(timer:interval(10) and not SupportBean(TheString='C1'))" +
                    "or" +
                    "(SupportBean(TheString='C2') and not timer:interval(10))";
            EPStatement statement = epService.EPAdministrator.CreatePattern(pattern);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E", 0));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10999));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
            Assert.IsTrue(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private long DateToLong(string dateText) {
            var date = DateTimeOffset.Parse(dateText);
            Log.Debug(".dateToLong out=" + date);
            return date.TimeInMillis();
        }
    
        private SupportCallEvent SendEvent(EPRuntime runtime, long callId, string source, string destination, long startTime, long endTime) {
            var theEvent = new SupportCallEvent(callId, source, destination, startTime, endTime);
            runtime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_A SendA(string id, EPServiceProvider epService) {
            var a = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(a);
            return a;
        }
    
        private void SendB(string id, EPServiceProvider epService) {
            var b = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(b);
        }
    
        private void SendC(string id, EPServiceProvider epService) {
            var c = new SupportBean_C(id);
            epService.EPRuntime.SendEvent(c);
        }
    
        private void SendTimer(long time, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(time);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
