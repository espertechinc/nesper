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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterJoinLeftWWhere : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionWhereNotNullIs(epService);
            RunAssertionWhereNotNullNE(epService);
            RunAssertionWhereNullIs(epService);
            RunAssertionWhereNullEq(epService);
            RunAssertionWhereJoinOrNull(epService);
            RunAssertionWhereJoin(epService);
            RunAssertionEventType(epService);
        }
    
        private void RunAssertionWhereNotNullIs(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s1.p11 is not null");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryWhereNotNull(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionWhereNotNullNE(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s1.p11 is not null");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryWhereNotNull(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionWhereNullIs(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s1.p11 is null");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryWhereNull(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionWhereNullEq(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s1.p11 is null");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryWhereNull(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionWhereJoinOrNull(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s0.p01 = s1.p11 or s1.p11 is null");
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            var eventS0 = new SupportBean_S0(0, "0", "[a]");
            SendEvent(eventS0, epService);
            CompareEvent(updateListener.AssertOneGetNewAndReset(), eventS0, null);
    
            // Send events to test the join for multiple rows incl. null value
            var s1_1 = new SupportBean_S1(1000, "5", "X");
            var s1_2 = new SupportBean_S1(1001, "5", "Y");
            var s1_3 = new SupportBean_S1(1002, "5", "X");
            var s1_4 = new SupportBean_S1(1003, "5", null);
            var s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(epService, new object[]{s1_1, s1_2, s1_3, s1_4, s0});
    
            Assert.AreEqual(3, updateListener.LastNewData.Length);
            var received = new Object[3];
            for (int i = 0; i < 3; i++) {
                Assert.AreSame(s0, updateListener.LastNewData[i].Get("s0"));
                received[i] = updateListener.LastNewData[i].Get("s1");
            }
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{s1_1, s1_3, s1_4}, received);
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhereJoin(EPServiceProvider epService) {
            EPStatement stmt = SetupStatement(epService, "where s0.p01 = s1.p11");
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            var eventsS0 = new SupportBean_S0[15];
            var eventsS1 = new SupportBean_S1[15];
            int count = 100;
            for (int i = 0; i < eventsS0.Length; i++) {
                eventsS0[i] = new SupportBean_S0(count++, Convert.ToString(i));
            }
            count = 200;
            for (int i = 0; i < eventsS1.Length; i++) {
                eventsS1[i] = new SupportBean_S1(count++, Convert.ToString(i));
            }
    
            // Send S0[0] p01=a
            eventsS0[0].P01 = "[a]";
            SendEvent(eventsS0[0], epService);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Send S1[1] p11=b
            eventsS1[1].P11 = "[b]";
            SendEvent(eventsS1[1], epService);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Send S0[1] p01=c, no match expected
            eventsS0[1].P01 = "[c]";
            SendEvent(eventsS0[1], epService);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Send S1[2] p11=d
            eventsS1[2].P11 = "[d]";
            SendEvent(eventsS1[2], epService);
            // Send S0[2] p01=d
            eventsS0[2].P01 = "[d]";
            SendEvent(eventsS0[2], epService);
            CompareEvent(updateListener.AssertOneGetNewAndReset(), eventsS0[2], eventsS1[2]);
    
            // Send S1[3] and S0[3] with differing props, no match expected
            eventsS1[3].P11 = "[e]";
            SendEvent(eventsS1[3], epService);
            eventsS0[3].P01 = "[e1]";
            SendEvent(eventsS0[3], epService);
            Assert.IsFalse(updateListener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private EPStatement SetupStatement(EPServiceProvider epService, string whereClause) {
            string joinStatement = "select * from " +
                    typeof(SupportBean_S0).FullName + "#length(5) as s0 " +
                    "left outer join " +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " on s0.p00 = s1.p10 " +
                    whereClause;
    
            return epService.EPAdministrator.CreateEPL(joinStatement);
        }
    
        private void RunAssertionEventType(EPServiceProvider epService) {
            EPStatement outerJoinView = SetupStatement(epService, "");
            EventType type = outerJoinView.EventType;
            Assert.AreEqual(typeof(SupportBean_S0), type.GetPropertyType("s0"));
            Assert.AreEqual(typeof(SupportBean_S1), type.GetPropertyType("s1"));
        }
    
        private void TryWhereNotNull(EPServiceProvider epService, SupportUpdateListener updateListener) {
            var s1_1 = new SupportBean_S1(1000, "5", "X");
            var s1_2 = new SupportBean_S1(1001, "5", null);
            var s1_3 = new SupportBean_S1(1002, "6", null);
            SendEvent(epService, new object[]{s1_1, s1_2, s1_3});
            Assert.IsFalse(updateListener.IsInvoked);
    
            var s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(s0, epService);
            CompareEvent(updateListener.AssertOneGetNewAndReset(), s0, s1_1);
        }
    
        private void TryWhereNull(EPServiceProvider epService, SupportUpdateListener updateListener) {
            var s1_1 = new SupportBean_S1(1000, "5", "X");
            var s1_2 = new SupportBean_S1(1001, "5", null);
            var s1_3 = new SupportBean_S1(1002, "6", null);
            SendEvent(epService, new object[]{s1_1, s1_2, s1_3});
            Assert.IsFalse(updateListener.IsInvoked);
    
            var s0 = new SupportBean_S0(1, "5", "X");
            SendEvent(s0, epService);
            CompareEvent(updateListener.AssertOneGetNewAndReset(), s0, s1_2);
        }
    
        private void CompareEvent(EventBean receivedEvent, SupportBean_S0 expectedS0, SupportBean_S1 expectedS1) {
            Assert.AreSame(expectedS0, receivedEvent.Get("s0"));
            Assert.AreSame(expectedS1, receivedEvent.Get("s1"));
        }
    
        private void SendEvent(EPServiceProvider epService, object[] events) {
            for (int i = 0; i < events.Length; i++) {
                SendEvent(events[i], epService);
            }
        }
    
        private void SendEvent(Object theEvent, EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
