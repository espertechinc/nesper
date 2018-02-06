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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinSingleOp3Stream : RegressionExecution {
        private static readonly string EVENT_A = typeof(SupportBean_A).FullName;
        private static readonly string EVENT_B = typeof(SupportBean_B).FullName;
        private static readonly string EVENT_C = typeof(SupportBean_C).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionJoinUniquePerId(epService);
            RunAssertionJoinUniquePerIdOM(epService);
            RunAssertionJoinUniquePerIdCompile(epService);
        }
    
        private void RunAssertionJoinUniquePerId(EPServiceProvider epService) {
            string epl = "select * from " +
                    EVENT_A + "#length(3) as streamA," +
                    EVENT_B + "#length(3) as streamB," +
                    EVENT_C + "#length(3) as streamC" +
                    " where (streamA.id = streamB.id) " +
                    "   and (streamB.id = streamC.id)" +
                    "   and (streamA.id = streamC.id)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunJoinUniquePerId(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinUniquePerIdOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            FromClause fromClause = FromClause.Create(
                    FilterStream.Create(EVENT_A, "streamA").AddView(View.Create("length", Expressions.Constant(3))),
                    FilterStream.Create(EVENT_B, "streamB").AddView(View.Create("length", Expressions.Constant(3))),
                    FilterStream.Create(EVENT_C, "streamC").AddView(View.Create("length", Expressions.Constant(3))));
            model.FromClause = fromClause;
            model.WhereClause = Expressions.And(
                    Expressions.EqProperty("streamA.id", "streamB.id"),
                    Expressions.EqProperty("streamB.id", "streamC.id"),
                    Expressions.EqProperty("streamA.id", "streamC.id"));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string epl = "select * from " +
                    EVENT_A + "#length(3) as streamA, " +
                    EVENT_B + "#length(3) as streamB, " +
                    EVENT_C + "#length(3) as streamC " +
                    "where streamA.id=streamB.id " +
                    "and streamB.id=streamC.id " +
                    "and streamA.id=streamC.id";
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            RunJoinUniquePerId(epService, updateListener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinUniquePerIdCompile(EPServiceProvider epService) {
            string epl = "select * from " +
                    EVENT_A + "#length(3) as streamA, " +
                    EVENT_B + "#length(3) as streamB, " +
                    EVENT_C + "#length(3) as streamC " +
                    "where streamA.id=streamB.id " +
                    "and streamB.id=streamC.id " +
                    "and streamA.id=streamC.id";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            EPStatement srmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            srmt.Events += listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            RunJoinUniquePerId(epService, listener);
    
            srmt.Dispose();
        }
    
        private void RunJoinUniquePerId(EPServiceProvider epService, SupportUpdateListener listener) {
            var eventsA = new SupportBean_A[10];
            var eventsB = new SupportBean_B[10];
            var eventsC = new SupportBean_C[10];
            for (int i = 0; i < eventsA.Length; i++) {
                eventsA[i] = new SupportBean_A(Convert.ToString(i));
                eventsB[i] = new SupportBean_B(Convert.ToString(i));
                eventsC[i] = new SupportBean_C(Convert.ToString(i));
            }
    
            // Test sending a C event
            SendEvent(epService, eventsA[0]);
            SendEvent(epService, eventsB[0]);
            Assert.IsNull(listener.LastNewData);
            SendEvent(epService, eventsC[0]);
            AssertEventsReceived(listener, eventsA[0], eventsB[0], eventsC[0]);
    
            // Test sending a B event
            SendEvent(epService, new object[]{eventsA[1], eventsB[2], eventsC[3]});
            SendEvent(epService, eventsC[1]);
            Assert.IsNull(listener.LastNewData);
            SendEvent(epService, eventsB[1]);
            AssertEventsReceived(listener, eventsA[1], eventsB[1], eventsC[1]);
    
            // Test sending a C event
            SendEvent(epService, new object[]{eventsA[4], eventsA[5], eventsB[4], eventsB[3]});
            Assert.IsNull(listener.LastNewData);
            SendEvent(epService, eventsC[4]);
            AssertEventsReceived(listener, eventsA[4], eventsB[4], eventsC[4]);
            Assert.IsNull(listener.LastNewData);
        }
    
        private void AssertEventsReceived(SupportUpdateListener updateListener, SupportBean_A eventA, SupportBean_B eventB, SupportBean_C eventC) {
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreSame(eventA, updateListener.LastNewData[0].Get("streamA"));
            Assert.AreSame(eventB, updateListener.LastNewData[0].Get("streamB"));
            Assert.AreSame(eventC, updateListener.LastNewData[0].Get("streamC"));
            updateListener.Reset();
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService, object[] events) {
            for (int i = 0; i < events.Length; i++) {
                epService.EPRuntime.SendEvent(events[i]);
            }
        }
    }
} // end of namespace
