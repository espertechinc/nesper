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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLPatternQueries : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionWhere_OM(epService);
            RunAssertionWhere_Compile(epService);
            RunAssertionWhere(epService);
            RunAssertionAggregation(epService);
            RunAssertionFollowedByAndWindow(epService);
        }
    
        private void RunAssertionWhere_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .AddWithAsProvidedName("s0.id", "idS0").AddWithAsProvidedName("s1.id", "idS1");
            PatternExpr pattern = Patterns.Or()
                    .Add(Patterns.EveryFilter(typeof(SupportBean_S0).FullName, "s0"))
                    .Add(Patterns.EveryFilter(typeof(SupportBean_S1).FullName, "s1")
                    );
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model.WhereClause = Expressions.Or()
                    .Add(Expressions.And()
                            .Add(Expressions.IsNotNull("s0.id"))
                            .Add(Expressions.Lt("s0.id", 100))
                    )
                    .Add(Expressions.And()
                            .Add(Expressions.IsNotNull("s1.id"))
                            .Add(Expressions.Ge("s1.id", 100))
                    );
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string reverse = model.ToEPL();
            string stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where s0.id is not null and s0.id<100 or s1.id is not null and s1.id>=100";
            Assert.AreEqual(stmtText, reverse);
    
            EPStatement statement = epService.EPAdministrator.Create(model);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS0(epService, 1);
            AssertEventIds(updateListener, 1, null);
    
            SendEventS0(epService, 101);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 1);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 100);
            AssertEventIds(updateListener, null, 100);
    
            statement.Dispose();
        }
    
        private void RunAssertionWhere_Compile(EPServiceProvider epService) {
            string stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where s0.id is not null and s0.id<100 or s1.id is not null and s1.id>=100";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string reverse = model.ToEPL();
            Assert.AreEqual(stmtText, reverse);
    
            EPStatement statement = epService.EPAdministrator.Create(model);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS0(epService, 1);
            AssertEventIds(updateListener, 1, null);
    
            SendEventS0(epService, 101);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 1);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 100);
            AssertEventIds(updateListener, null, 100);
    
            statement.Dispose();
        }
    
        private void RunAssertionWhere(EPServiceProvider epService) {
            string stmtText = "select s0.id as idS0, s1.id as idS1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "] " +
                    "where (s0.id is not null and s0.id < 100) or (s1.id is not null and s1.id >= 100)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS0(epService, 1);
            AssertEventIds(updateListener, 1, null);
    
            SendEventS0(epService, 101);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 1);
            Assert.IsFalse(updateListener.IsInvoked);
    
            SendEventS1(epService, 100);
            AssertEventIds(updateListener, null, 100);
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregation(EPServiceProvider epService) {
            string stmtText = "select sum(s0.id) as sumS0, sum(s1.id) as sumS1, sum(s0.id + s1.id) as sumS0S1 " +
                    "from pattern [every s0=" + typeof(SupportBean_S0).FullName +
                    " or every s1=" + typeof(SupportBean_S1).FullName + "]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS0(epService, 1);
            AssertEventSums(updateListener, 1, null, null);
    
            SendEventS1(epService, 2);
            AssertEventSums(updateListener, 1, 2, null);
    
            SendEventS1(epService, 10);
            AssertEventSums(updateListener, 1, 12, null);
    
            SendEventS0(epService, 20);
            AssertEventSums(updateListener, 21, 12, null);
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedByAndWindow(EPServiceProvider epService) {
            string stmtText = "select irstream a.id as idA, b.id as idB, " +
                    "a.p00 as p00A, b.p00 as p00B from pattern [every a=" + typeof(SupportBean_S0).FullName +
                    " -> every b=" + typeof(SupportBean_S0).FullName + "(p00=a.p00)]#time(1)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var updateListener = new SupportUpdateListener();
    
            statement.Events += updateListener.Update;
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            SendEvent(epService, 1, "e1a");
            Assert.IsFalse(updateListener.IsInvoked);
            SendEvent(epService, 2, "e1a");
            AssertNewEvent(updateListener, 1, 2, "e1a");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(500));
            SendEvent(epService, 10, "e2a");
            SendEvent(epService, 11, "e2b");
            SendEvent(epService, 12, "e2c");
            Assert.IsFalse(updateListener.IsInvoked);
            SendEvent(epService, 13, "e2b");
            AssertNewEvent(updateListener, 11, 13, "e2b");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            AssertOldEvent(updateListener, 1, 2, "e1a");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            AssertOldEvent(updateListener, 11, 13, "e2b");
    
            statement.Dispose();
        }
    
        private void AssertNewEvent(SupportUpdateListener updateListener, int idA, int idB, string p00) {
            EventBean eventBean = updateListener.AssertOneGetNewAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }
    
        private void AssertOldEvent(SupportUpdateListener updateListener, int idA, int idB, string p00) {
            EventBean eventBean = updateListener.AssertOneGetOldAndReset();
            CompareEvent(eventBean, idA, idB, p00);
        }
    
        private void CompareEvent(EventBean eventBean, int idA, int idB, string p00) {
            Assert.AreEqual(idA, eventBean.Get("idA"));
            Assert.AreEqual(idB, eventBean.Get("idB"));
            Assert.AreEqual(p00, eventBean.Get("p00A"));
            Assert.AreEqual(p00, eventBean.Get("p00B"));
        }
    
        private void SendEvent(EPServiceProvider epService, int id, string p00) {
            var theEvent = new SupportBean_S0(id, p00);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventS0(EPServiceProvider epService, int id) {
            var theEvent = new SupportBean_S0(id);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventS1(EPServiceProvider epService, int id) {
            var theEvent = new SupportBean_S1(id);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void AssertEventIds(SupportUpdateListener updateListener, int? idS0, int? idS1) {
            EventBean eventBean = updateListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(idS0, eventBean.Get("idS0"));
            Assert.AreEqual(idS1, eventBean.Get("idS1"));
            updateListener.Reset();
        }
    
        private void AssertEventSums(SupportUpdateListener updateListener, int? sumS0, int? sumS1, int? sumS0S1) {
            EventBean eventBean = updateListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(sumS0, eventBean.Get("sumS0"));
            Assert.AreEqual(sumS1, eventBean.Get("sumS1"));
            Assert.AreEqual(sumS0S1, eventBean.Get("sumS0S1"));
            updateListener.Reset();
        }
    }
} // end of namespace
