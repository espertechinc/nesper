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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectExists : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionExistsInSelect(epService);
            RunAssertionExistsInSelectOM(epService);
            RunAssertionExistsInSelectCompile(epService);
            RunAssertionExists(epService);
            RunAssertionExistsFiltered(epService);
            RunAssertionTwoExistsFiltered(epService);
            RunAssertionNotExists_OM(epService);
            RunAssertionNotExists_Compile(epService);
            RunAssertionNotExists(epService);
        }
    
        private void RunAssertionExistsInSelect(EPServiceProvider epService) {
            string stmtText = "select exists (select * from S1#length(1000)) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunTestExistsInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionExistsInSelectOM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.CreateWildcard();
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1")
                .AddView(View.Create("length", Expressions.Constant(1000))));
    
            var model = new EPStatementObjectModel();
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model.SelectClause = SelectClause.Create()
                .Add(Expressions.SubqueryExists(subquery), "value");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select exists (select * from S1#length(1000)) as value from S0";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunTestExistsInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionExistsInSelectCompile(EPServiceProvider epService) {
            string stmtText = "select exists (select * from S1#length(1000)) as value from S0";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunTestExistsInSelect(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunTestExistsInSelect(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(false, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void RunAssertionExists(EPServiceProvider epService) {
            string stmtText = "select id from S0 where exists (select * from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionExistsFiltered(EPServiceProvider epService) {
            string stmtText = "select id from S0 as s0 where exists (select * from S1#length(1000) as s1 where s1.id=s0.id)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(-2));
            Assert.AreEqual(-2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionTwoExistsFiltered(EPServiceProvider epService) {
            string stmtText = "select id from S0 as s0 where " +
                    "exists (select * from S1#length(1000) as s1 where s1.id=s0.id) " +
                    "and " +
                    "exists (select * from S2#length(1000) as s2 where s2.id=s0.id) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            epService.EPRuntime.SendEvent(new SupportBean_S2(1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotExists_OM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.CreateWildcard();
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1")
                .AddView("length", Expressions.Constant(1000)));
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("id");
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model.WhereClause = Expressions.Not(Expressions.SubqueryExists(subquery));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select id from S0 where not exists (select * from S1#length(1000))";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotExists_Compile(EPServiceProvider epService) {
            string stmtText = "select id from S0 where not exists (select * from S1#length(1000))";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionNotExists(EPServiceProvider epService) {
            string stmtText = "select id from S0 where not exists (select * from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    }
} // end of namespace
