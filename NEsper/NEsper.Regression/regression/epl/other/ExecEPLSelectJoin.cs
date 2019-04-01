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

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLSelectJoin : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            RunAssertionJoinUniquePerId(epService);
            RunAssertionJoinNonUniquePerId(epService);
        }
    
        private void RunAssertionJoinUniquePerId(EPServiceProvider epService) {
            SelectJoinHolder holder = SetupStmt(epService);
    
            SendEvent(epService, holder.eventsA[0]);
            SendEvent(epService, holder.eventsB[1]);
            Assert.IsNull(holder.listener.LastNewData);
    
            // Test join new B with id 0
            SendEvent(epService, holder.eventsB[0]);
            Assert.AreSame(holder.eventsA[0], holder.listener.LastNewData[0].Get("streamA"));
            Assert.AreSame(holder.eventsB[0], holder.listener.LastNewData[0].Get("streamB"));
            Assert.IsNull(holder.listener.LastOldData);
            holder.listener.Reset();
    
            // Test join new A with id 1
            SendEvent(epService, holder.eventsA[1]);
            Assert.AreSame(holder.eventsA[1], holder.listener.LastNewData[0].Get("streamA"));
            Assert.AreSame(holder.eventsB[1], holder.listener.LastNewData[0].Get("streamB"));
            Assert.IsNull(holder.listener.LastOldData);
            holder.listener.Reset();
    
            SendEvent(epService, holder.eventsA[2]);
            Assert.IsNull(holder.listener.LastOldData);
    
            // Test join old A id 0 leaves length window of 3 events
            SendEvent(epService, holder.eventsA[3]);
            Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[0].Get("streamA"));
            Assert.AreSame(holder.eventsB[0], holder.listener.LastOldData[0].Get("streamB"));
            Assert.IsNull(holder.listener.LastNewData);
            holder.listener.Reset();
    
            // Test join old B id 1 leaves window
            SendEvent(epService, holder.eventsB[4]);
            Assert.IsNull(holder.listener.LastOldData);
            SendEvent(epService, holder.eventsB[5]);
            Assert.AreSame(holder.eventsA[1], holder.listener.LastOldData[0].Get("streamA"));
            Assert.AreSame(holder.eventsB[1], holder.listener.LastOldData[0].Get("streamB"));
            Assert.IsNull(holder.listener.LastNewData);
    
            holder.stmt.Dispose();
        }
    
        private void RunAssertionJoinNonUniquePerId(EPServiceProvider epService) {
            SelectJoinHolder holder = SetupStmt(epService);
    
            SendEvent(epService, holder.eventsA[0]);
            SendEvent(epService, holder.eventsA[1]);
            SendEvent(epService, holder.eventsASetTwo[0]);
            Assert.IsTrue(holder.listener.LastOldData == null && holder.listener.LastNewData == null);
    
            SendEvent(epService, holder.eventsB[0]); // Event B id 0 joins to A id 0 twice
            EventBean[] data = holder.listener.LastNewData;
            Assert.IsTrue(holder.eventsASetTwo[0] == data[0].Get("streamA") || holder.eventsASetTwo[0] == data[1].Get("streamA"));    // Order arbitrary
            Assert.AreSame(holder.eventsB[0], data[0].Get("streamB"));
            Assert.IsTrue(holder.eventsA[0] == data[0].Get("streamA") || holder.eventsA[0] == data[1].Get("streamA"));
            Assert.AreSame(holder.eventsB[0], data[1].Get("streamB"));
            Assert.IsNull(holder.listener.LastOldData);
            holder.listener.Reset();
    
            SendEvent(epService, holder.eventsB[2]);
            SendEvent(epService, holder.eventsBSetTwo[0]);  // Ignore events generated
            holder.listener.Reset();
    
            SendEvent(epService, holder.eventsA[3]);  // Pushes A id 0 out of window, which joins to B id 0 twice
            data = holder.listener.LastOldData;
            Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[0].Get("streamA"));
            Assert.IsTrue(holder.eventsB[0] == data[0].Get("streamB") || holder.eventsB[0] == data[1].Get("streamB"));    // B order arbitrary
            Assert.AreSame(holder.eventsA[0], holder.listener.LastOldData[1].Get("streamA"));
            Assert.IsTrue(holder.eventsBSetTwo[0] == data[0].Get("streamB") || holder.eventsBSetTwo[0] == data[1].Get("streamB"));
            Assert.IsNull(holder.listener.LastNewData);
            holder.listener.Reset();
    
            SendEvent(epService, holder.eventsBSetTwo[2]);  // Pushes B id 0 out of window, which joins to A set two id 0
            Assert.AreSame(holder.eventsASetTwo[0], holder.listener.LastOldData[0].Get("streamA"));
            Assert.AreSame(holder.eventsB[0], holder.listener.LastOldData[0].Get("streamB"));
            Assert.AreEqual(1, holder.listener.LastOldData.Length);
    
            holder.stmt.Dispose();
        }
    
        private SelectJoinHolder SetupStmt(EPServiceProvider epService) {
            var holder = new SelectJoinHolder();
    
            string epl = "select irstream * from A#length(3) as streamA, B#length(3) as streamB where streamA.id = streamB.id";
            holder.stmt = epService.EPAdministrator.CreateEPL(epl);
            holder.listener = new SupportUpdateListener();
            holder.stmt.Events += holder.listener.Update;
    
            Assert.AreEqual(typeof(SupportBean_A), holder.stmt.EventType.GetPropertyType("streamA"));
            Assert.AreEqual(typeof(SupportBean_B), holder.stmt.EventType.GetPropertyType("streamB"));
            Assert.AreEqual(2, holder.stmt.EventType.PropertyNames.Length);
    
            holder.eventsA = new SupportBean_A[10];
            holder.eventsASetTwo = new SupportBean_A[10];
            holder.eventsB = new SupportBean_B[10];
            holder.eventsBSetTwo = new SupportBean_B[10];
            for (int i = 0; i < holder.eventsA.Length; i++) {
                holder.eventsA[i] = new SupportBean_A(Convert.ToString(i));
                holder.eventsASetTwo[i] = new SupportBean_A(Convert.ToString(i));
                holder.eventsB[i] = new SupportBean_B(Convert.ToString(i));
                holder.eventsBSetTwo[i] = new SupportBean_B(Convert.ToString(i));
            }
            return holder;
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        public class SelectJoinHolder
        {
            public EPStatement stmt;
            public SupportUpdateListener listener;
            public SupportBean_A[] eventsA;
            public SupportBean_A[] eventsASetTwo;
            public SupportBean_B[] eventsB;
            public SupportBean_B[] eventsBSetTwo;
        }
    }
} // end of namespace
