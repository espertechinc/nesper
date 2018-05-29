///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    using Map = IDictionary<string, object>;

    public class ExecSubselectAggregatedMultirowAndColumn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            RunAssertionMultirowGroupedNoDataWindowUncorrelated(epService);
            RunAssertionMultirowGroupedNamedWindowSubqueryIndexShared(epService);
            RunAssertionMultirowGroupedUncorrelatedIteratorAndExpressionDef(epService);
            RunAssertionMultirowGroupedCorrelatedWithEnumMethod(epService);
            RunAssertionMultirowGroupedUncorrelatedWithEnumerationMethod(epService);
            RunAssertionMultirowGroupedCorrelatedWHaving(epService);
    
            RunAssertionMulticolumnGroupedUncorrelatedUnfiltered(epService);
            RunAssertionMulticolumnGroupedContextPartitioned(epService);
            RunAssertionMulticolumnGroupedWHaving(epService);
    
            // Invalid tests
            string epl;
            // not fully aggregated
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by IntPrimitive) from S0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause [select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by IntPrimitive) from S0]");
    
            // correlated group-by not allowed
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.id) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (property 'id' is not) [select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.id) from S0 as s0]");
            epl = "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.get_P00()) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (expression 's0.get_P00()' against stream 1 is not)");
    
            // aggregations not allowed in group-by
            epl = "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from S0 as s0]");
    
            // "prev" not allowed in group-by
            epl = "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from S0 as s0]");
        }
    
        private void RunAssertionMulticolumnGroupedWHaving(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select (select TheString as c0, sum(IntPrimitive) as c1 from SupportBean#keepall group by TheString having sum(IntPrimitive) > 10) as subq from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset("subq", listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 5);
            AssertMapFieldAndReset("subq", listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 6);
            AssertMapFieldAndReset("subq", listener, fields, new object[]{"E2", 11});
    
            SendSBEventAndTrigger(epService, "E1", 1);
            AssertMapFieldAndReset("subq", listener, fields, null);
        }
    
        private void RunAssertionMulticolumnGroupedContextPartitioned(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            epService.EPAdministrator.CreateEPL(
                    "create context MyCtx partition by TheString from SupportBean, p00 from S0");
    
            string stmtText = "context MyCtx select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " group by TheString) as subq " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P1"));
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"P1", 100});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"P2", 200});
    
            epService.EPRuntime.SendEvent(new SupportBean("P2", 205));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"P2", 405});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMulticolumnGroupedUncorrelatedUnfiltered(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
            string eplNoDelete = "select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean#keepall " +
                    "group by TheString) as subq " +
                    "from S0 as s0";
            EPStatement stmtNoDelete = epService.EPAdministrator.CreateEPL(eplNoDelete);
            var listener = new SupportUpdateListener();
            stmtNoDelete.Events += listener.Update;
            RunAssertionNoDelete(epService, listener, fieldName, fields);
            stmtNoDelete.Dispose();
    
            // try SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplNoDelete);
            Assert.AreEqual(eplNoDelete, model.ToEPL());
            stmtNoDelete = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtNoDelete.Text, eplNoDelete);
            stmtNoDelete.Events += listener.Update;
            RunAssertionNoDelete(epService, listener, fieldName, fields);
            stmtNoDelete.Dispose();
    
            // test named window with delete/remove
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on S1 delete from MyWindow where id = IntPrimitive");
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("@Hint('disable_reclaim_group') select (select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from MyWindow group by TheString) as subq from S0 as s0");
            stmtDelete.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E1", 10});
    
            SendS1EventAndTrigger(epService, 10);     // delete 10
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 20);
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E2", 20});
    
            SendSBEventAndTrigger(epService, "E2", 21);
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E2", 41});
    
            SendSBEventAndTrigger(epService, "E1", 30);
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendS1EventAndTrigger(epService, 30);     // delete 30
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E2", 41});
    
            SendS1EventAndTrigger(epService, 20);     // delete 20
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E2", 21});
    
            SendSBEventAndTrigger(epService, "E1", 31);    // two groups
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendS1EventAndTrigger(epService, 21);     // delete 21
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E1", 31});
            stmtDelete.Dispose();
    
            // test multiple group-by criteria
            string[] fieldsMultiGroup = "c0,c1,c2,c3,c4".Split(',');
            string eplMultiGroup = "select " +
                    "(select TheString as c0, IntPrimitive as c1, TheString||'x' as c2, " +
                    "    IntPrimitive * 1000 as c3, sum(LongPrimitive) as c4 " +
                    " from SupportBean#keepall " +
                    " group by TheString, IntPrimitive) as subq " +
                    "from S0 as s0";
            EPStatement stmtMultiGroup = epService.EPAdministrator.CreateEPL(eplMultiGroup);
            stmtMultiGroup.Events += listener.Update;
    
            SendSBEventAndTrigger(epService, "G1", 1, 100L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, new object[]{"G1", 1, "G1x", 1000, 100L});
    
            SendSBEventAndTrigger(epService, "G1", 1, 101L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, new object[]{"G1", 1, "G1x", 1000, 201L});
    
            SendSBEventAndTrigger(epService, "G2", 1, 200L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, null);
    
            stmtMultiGroup.Dispose();
        }
    
        private void RunAssertionMultirowGroupedCorrelatedWHaving(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            string eplEnumCorrelated = "select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where IntPrimitive = s0.id " +
                    " group by TheString" +
                    " having sum(IntPrimitive) > 10).take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 20}, new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 55));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 20}, new object[] {"E2", 20}});
    
            stmtEnumUnfiltered.Dispose();
        }
    
        private void RunAssertionMultirowGroupedCorrelatedWithEnumMethod(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            string eplEnumCorrelated = "select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where IntPrimitive = s0.id " +
                    " group by TheString).take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E2", 100}});
    
            stmtEnumUnfiltered.Dispose();
        }
    
        private void RunAssertionNoDelete(EPServiceProvider epService, SupportUpdateListener listener, string fieldName, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E1", 10});
    
            SendSBEventAndTrigger(epService, "E1", 20);
            AssertMapFieldAndReset(fieldName, listener, fields, new object[]{"E1", 30});
    
            // second group - this returns null as subquerys cannot return multiple rows (unless enumerated) (sql standard)
            SendSBEventAndTrigger(epService, "E2", 5);
            AssertMapFieldAndReset(fieldName, listener, fields, null);
        }
    
        private void RunAssertionMultirowGroupedNamedWindowSubqueryIndexShared(EPServiceProvider epService) {
            // test uncorrelated
            epService.EPAdministrator.CreateEPL("@Hint('enable_window_subquery_indexshare')" +
                    "create window SBWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into SBWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
    
            EPStatement stmtUncorrelated = epService.EPAdministrator.CreateEPL("select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 from SBWindow group by TheString).take(10) as e1 from S0");
            var listener = new SupportUpdateListener();
            stmtUncorrelated.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new object[][]{new object[] {"E1", 30}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new object[][]{new object[] {"E1", 30}, new object[] {"E2", 200}});
            stmtUncorrelated.Dispose();
    
            // test correlated
            EPStatement stmtCorrelated = epService.EPAdministrator.CreateEPL("select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 from SBWindow where TheString = s0.p00 group by TheString).take(10) as e1 from S0 as s0");
            stmtCorrelated.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new object[][]{new object[] {"E1", 30}});
    
            stmtCorrelated.Dispose();
        }
    
        private void RunAssertionMultirowGroupedNoDataWindowUncorrelated(EPServiceProvider epService) {
            string stmtText = "select (select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString).take(10) as subq from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, new object[][]{new object[] {"G1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, new object[][]{new object[] {"G1", 10}, new object[] {"G2", 20}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultirowGroupedUncorrelatedIteratorAndExpressionDef(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "expression getGroups {" +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    "  from SupportBean#keepall group by TheString)" +
                    "}" +
                    "select getGroups() as e1, getGroups().take(10) as e2 from S0#lastevent()";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBEventAndTrigger(epService, "E1", 20);
            foreach (EventBean @event in new EventBean[]{listener.AssertOneGetNew(), stmt.First()}) {
                AssertMapField("e1", @event, fields, new object[]{"E1", 20});
                AssertMapMultiRow("e2", @event, "c0", fields, new object[][]{new object[] {"E1", 20}});
            }
            listener.Reset();
    
            SendSBEventAndTrigger(epService, "E2", 30);
            foreach (EventBean @event in new EventBean[]{listener.AssertOneGetNew(), stmt.First()}) {
                AssertMapField("e1", @event, fields, null);
                AssertMapMultiRow("e2", @event, "c0", fields, new object[][]{new object[] {"E1", 20}, new object[] {"E2", 30}});
            }
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultirowGroupedUncorrelatedWithEnumerationMethod(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            // test unfiltered
            string eplEnumUnfiltered = "select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " group by TheString).take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumUnfiltered);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 10}});
    
            SendSBEventAndTrigger(epService, "E1", 20);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 30}});
    
            SendSBEventAndTrigger(epService, "E2", 100);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 30}, new object[] {"E2", 100}});
    
            SendSBEventAndTrigger(epService, "E3", 2000);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 30}, new object[] {"E2", 100}, new object[] {"E3", 2000}});
            stmtEnumUnfiltered.Dispose();
    
            // test filtered
            string eplEnumFiltered = "select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where IntPrimitive > 100 " +
                    " group by TheString).take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumFiltered = epService.EPAdministrator.CreateEPL(eplEnumFiltered);
            stmtEnumFiltered.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 200);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 200}});
    
            SendSBEventAndTrigger(epService, "E1", 11);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 200}});
    
            SendSBEventAndTrigger(epService, "E1", 201);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 401}});
    
            SendSBEventAndTrigger(epService, "E2", 300);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new object[][]{new object[] {"E1", 401}, new object[] {"E2", 300}});
    
            stmtEnumFiltered.Dispose();
        }
    
        private void SendSBEventAndTrigger(EPServiceProvider epService, string theString, int intPrimitive) {
            SendSBEventAndTrigger(epService, theString, intPrimitive, 0);
        }
    
        private void SendSBEventAndTrigger(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
        }
    
        private void SendS1EventAndTrigger(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new SupportBean_S1(id, "x"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
        }
    
        private void AssertMapFieldAndReset(string fieldName, SupportUpdateListener listener, string[] names, object[] values) {
            AssertMapField(fieldName, listener.AssertOneGetNew(), names, values);
            listener.Reset();
        }
    
        private void AssertMapMultiRowAndReset(string fieldName, SupportUpdateListener listener, string sortKey, string[] names, object[][] values) {
            AssertMapMultiRow(fieldName, listener.AssertOneGetNew(), sortKey, names, values);
            listener.Reset();
        }
    
        private void AssertMapField(string fieldName, EventBean @event, string[] names, object[] values) {
            IDictionary<string, Object> subq = (IDictionary<string, Object>) @event.Get(fieldName);
            if (values == null && subq == null) {
                return;
            }
            EPAssertionUtil.AssertPropsMap(subq, names, values);
        }
    
        internal static void AssertMapMultiRow(string fieldName, EventBean @event, string sortKey, string[] names, object[][] values)
        {
            var subx = @event.Get(fieldName).Unwrap<object>();
            if (subx == null || values == null) {
                return;
            }

            var maps = subx
                .Select(item => (Map)item)
                .OrderBy(map => map.Get(sortKey))
                .ToArray();
            
            //Arrays.Sort(maps, new ProxyComparator<Map>() {
            //    ProcCompare = (o1) => {
            //        return ((IComparable) o1.Get(sortKey)).CompareTo(o2.Get(sortKey));
            //    };
            //});

            EPAssertionUtil.AssertPropsPerRow(maps, names, values);
        }
    }
} // end of namespace
