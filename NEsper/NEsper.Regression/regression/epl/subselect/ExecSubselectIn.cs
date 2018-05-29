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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectIn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInSelect(epService);
            RunAssertionInSelectOM(epService);
            RunAssertionInSelectCompile(epService);
            RunAssertionInSelectWhere(epService);
            RunAssertionInSelectWhereExpressions(epService);
            RunAssertionInWildcard(epService);
            RunAssertionInNullable(epService);
            RunAssertionInNullableCoercion(epService);
            RunAssertionInNullRow(epService);
            RunAssertionNotInNullRow(epService);
            RunAssertionNotInSelect(epService);
            RunAssertionNotInNullableCoercion(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionInSelect(EPServiceProvider epService) {
            string stmtText = "select id in (select id from S1#length(1000)) as value from S0";
    
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            RunTestInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInSelectOM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create("id");
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView(View.Create("length", Expressions.Constant(1000))));
    
            var model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryIn("id", subquery), "value");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select id in (select id from S1#length(1000)) as value from S0";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunTestInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInSelectCompile(EPServiceProvider epService) {
            string stmtText = "select id in (select id from S1#length(1000)) as value from S0";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunTestInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunTestInSelect(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void RunAssertionInSelectWhere(EPServiceProvider epService) {
            string stmtText = "select id in (select id from S1#length(1000) where id > 0) as value from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInSelectWhereExpressions(EPServiceProvider epService) {
            string stmtText = "select 3*id in (select 2*id from S1#length(1000)) as value from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(6));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInWildcard(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            string stmtText = "select s0.anyObject in (select * from S1#length(1000)) as value from ArrayBean s0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var s1 = new SupportBean_S1(100);
            var arrayBean = new SupportBeanArrayCollMap(s1);
            epService.EPRuntime.SendEvent(s1);
            epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            var s2 = new SupportBean_S2(100);
            arrayBean.AnyObject = s2;
            epService.EPRuntime.SendEvent(s2);
            epService.EPRuntime.SendEvent(arrayBean);
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInNullable(EPServiceProvider epService) {
            string stmtText = "select id from S0 as s0 where p00 in (select p10 from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "a"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, null));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, null));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A"));
            Assert.AreEqual(4, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2, null));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, null));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInNullableCoercion(EPServiceProvider epService) {
            string stmtText = "select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                    "where LongBoxed in " +
                    "(select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "A", 0, 0L);
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "B", null, null);
    
            SendBean(epService, "A", 0, 0L);
            Assert.IsFalse(listener.IsInvoked);
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "B", 99, null);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
            SendBean(epService, "A", null, 99L);
            Assert.AreEqual(99L, listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean(epService, "B", 98, null);
    
            SendBean(epService, "A", null, 98L);
            Assert.AreEqual(98L, listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInNullRow(EPServiceProvider epService) {
            string stmtText = "select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                    "where IntBoxed in " +
                    "(select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "B", 1, 1L);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "A", 1, 1L);
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("IntBoxed"));
    
            SendBean(epService, "B", null, null);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "A", 1, 1L);
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("IntBoxed"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotInNullRow(EPServiceProvider epService) {
            string stmtText = "select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                    "where IntBoxed not in " +
                    "(select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "B", 1, 1L);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "A", 1, 1L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "B", null, null);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "A", 1, 1L);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotInSelect(EPServiceProvider epService) {
            string stmtText = "select not id in (select id from S1#length(1000)) as value from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(5));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotInNullableCoercion(EPServiceProvider epService) {
            string stmtText = "select LongBoxed from " + typeof(SupportBean).FullName + "(TheString='A') as s0 " +
                    "where LongBoxed not in " +
                    "(select IntBoxed from " + typeof(SupportBean).FullName + "(TheString='B')#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBean(epService, "A", 0, 0L);
            Assert.AreEqual(0L, listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean(epService, "A", null, null);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("LongBoxed"));
    
            SendBean(epService, "B", null, null);
    
            SendBean(epService, "A", 1, 1L);
            Assert.IsFalse(listener.IsInvoked);
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "B", 99, null);
    
            SendBean(epService, "A", null, null);
            Assert.IsFalse(listener.IsInvoked);
            SendBean(epService, "A", null, 99L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "B", 98, null);
    
            SendBean(epService, "A", null, 98L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBean(epService, "A", null, 97L);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
            try {
                string stmtText = "select " +
                        "intArr in (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean";
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr in (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        private void SendBean(EPServiceProvider epService, string theString, int? intBoxed, long? longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
