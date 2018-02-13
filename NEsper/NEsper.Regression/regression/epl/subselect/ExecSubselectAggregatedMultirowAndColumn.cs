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

// using static org.junit.Assert.assertEquals;

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
            epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by intPrimitive) from S0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause [select (select theString, sum(longPrimitive) from SupportBean#keepall group by intPrimitive) from S0]");
    
            // correlated group-by not allowed
            epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.id) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (property 'id' is not) [select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.id) from S0 as s0]");
            epl = "select (select theString, sum(longPrimitive) from SupportBean#keepall group by theString, s0.P00) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (expression 's0.P00' against stream 1 is not)");
    
            // aggregations not allowed in group-by
            epl = "select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by sum(intPrimitive)) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by sum(intPrimitive)) from S0 as s0]");
    
            // "prev" not allowed in group-by
            epl = "select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by Prev(1, intPrimitive)) from S0 as s0";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select intPrimitive, sum(longPrimitive) from SupportBean#keepall group by Prev(1, intPrimitive)) from S0 as s0]");
        }
    
        private void RunAssertionMulticolumnGroupedWHaving(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select (select theString as c0, sum(intPrimitive) as c1 from SupportBean#keepall group by theString having sum(intPrimitive) > 10) as subq from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset("subq", listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 5);
            AssertMapFieldAndReset("subq", listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 6);
            AssertMapFieldAndReset("subq", listener, fields, new Object[]{"E2", 11});
    
            SendSBEventAndTrigger(epService, "E1", 1);
            AssertMapFieldAndReset("subq", listener, fields, null);
        }
    
        private void RunAssertionMulticolumnGroupedContextPartitioned(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            epService.EPAdministrator.CreateEPL(
                    "create context MyCtx partition by theString from SupportBean, p00 from S0");
    
            string stmtText = "context MyCtx select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " group by theString) as subq " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("P1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P1"));
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"P1", 100});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("P2", 200));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"P2", 200});
    
            epService.EPRuntime.SendEvent(new SupportBean("P2", 205));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "P2"));
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"P2", 405});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMulticolumnGroupedUncorrelatedUnfiltered(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
            string eplNoDelete = "select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    "from SupportBean#keepall " +
                    "group by theString) as subq " +
                    "from S0 as s0";
            EPStatement stmtNoDelete = epService.EPAdministrator.CreateEPL(eplNoDelete);
            var listener = new SupportUpdateListener();
            stmtNoDelete.AddListener(listener);
            RunAssertionNoDelete(epService, listener, fieldName, fields);
            stmtNoDelete.Dispose();
    
            // try SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplNoDelete);
            Assert.AreEqual(eplNoDelete, model.ToEPL());
            stmtNoDelete = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtNoDelete.Text, eplNoDelete);
            stmtNoDelete.AddListener(listener);
            RunAssertionNoDelete(epService, listener, fieldName, fields);
            stmtNoDelete.Dispose();
    
            // test named window with delete/remove
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on S1 delete from MyWindow where id = intPrimitive");
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("@Hint('disable_reclaim_group') select (select theString as c0, sum(intPrimitive) as c1 " +
                    " from MyWindow group by theString) as subq from S0 as s0");
            stmtDelete.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E1", 10});
    
            SendS1EventAndTrigger(epService, 10);     // delete 10
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E2", 20);
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E2", 20});
    
            SendSBEventAndTrigger(epService, "E2", 21);
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E2", 41});
    
            SendSBEventAndTrigger(epService, "E1", 30);
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendS1EventAndTrigger(epService, 30);     // delete 30
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E2", 41});
    
            SendS1EventAndTrigger(epService, 20);     // delete 20
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E2", 21});
    
            SendSBEventAndTrigger(epService, "E1", 31);    // two groups
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendS1EventAndTrigger(epService, 21);     // delete 21
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E1", 31});
            stmtDelete.Dispose();
    
            // test multiple group-by criteria
            string[] fieldsMultiGroup = "c0,c1,c2,c3,c4".Split(',');
            string eplMultiGroup = "select " +
                    "(select theString as c0, intPrimitive as c1, theString||'x' as c2, " +
                    "    intPrimitive * 1000 as c3, sum(longPrimitive) as c4 " +
                    " from SupportBean#keepall " +
                    " group by theString, intPrimitive) as subq " +
                    "from S0 as s0";
            EPStatement stmtMultiGroup = epService.EPAdministrator.CreateEPL(eplMultiGroup);
            stmtMultiGroup.AddListener(listener);
    
            SendSBEventAndTrigger(epService, "G1", 1, 100L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, new Object[]{"G1", 1, "G1x", 1000, 100L});
    
            SendSBEventAndTrigger(epService, "G1", 1, 101L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, new Object[]{"G1", 1, "G1x", 1000, 201L});
    
            SendSBEventAndTrigger(epService, "G2", 1, 200L);
            AssertMapFieldAndReset(fieldName, listener, fieldsMultiGroup, null);
    
            stmtMultiGroup.Dispose();
        }
    
        private void RunAssertionMultirowGroupedCorrelatedWHaving(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            string eplEnumCorrelated = "select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where intPrimitive = s0.id " +
                    " group by theString" +
                    " having sum(intPrimitive) > 10).Take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 20}, new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 55));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 20}, new object[] {"E2", 20}});
    
            stmtEnumUnfiltered.Dispose();
        }
    
        private void RunAssertionMultirowGroupedCorrelatedWithEnumMethod(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            string eplEnumCorrelated = "select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where intPrimitive = s0.id " +
                    " group by theString).Take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumCorrelated);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E2", 100}});
    
            stmtEnumUnfiltered.Dispose();
        }
    
        private void RunAssertionNoDelete(EPServiceProvider epService, SupportUpdateListener listener, string fieldName, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapFieldAndReset(fieldName, listener, fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E1", 10});
    
            SendSBEventAndTrigger(epService, "E1", 20);
            AssertMapFieldAndReset(fieldName, listener, fields, new Object[]{"E1", 30});
    
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
                    "(select theString as c0, sum(intPrimitive) as c1 from SBWindow group by theString).Take(10) as e1 from S0");
            var listener = new SupportUpdateListener();
            stmtUncorrelated.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][]{new object[] {"E1", 30}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][]{new object[] {"E1", 30}, new object[] {"E2", 200}});
            stmtUncorrelated.Dispose();
    
            // test correlated
            EPStatement stmtCorrelated = epService.EPAdministrator.CreateEPL("select " +
                    "(select theString as c0, sum(intPrimitive) as c1 from SBWindow where theString = s0.p00 group by theString).Take(10) as e1 from S0 as s0");
            stmtCorrelated.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            AssertMapMultiRow("e1", listener.AssertOneGetNewAndReset(), "c0", "c0,c1".Split(','), new Object[][]{new object[] {"E1", 30}});
    
            stmtCorrelated.Dispose();
        }
    
        private void RunAssertionMultirowGroupedNoDataWindowUncorrelated(EPServiceProvider epService) {
            string stmtText = "select (select theString as c0, sum(intPrimitive) as c1 from SupportBean group by theString).Take(10) as subq from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, new Object[][]{new object[] {"G1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
            ExecSubselectAggregatedMultirowAndColumn.AssertMapMultiRow("subq", listener.AssertOneGetNewAndReset(), "c0", fields, new Object[][]{new object[] {"G1", 10}, new object[] {"G2", 20}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultirowGroupedUncorrelatedIteratorAndExpressionDef(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "expression getGroups {" +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    "  from SupportBean#keepall group by theString)" +
                    "}" +
                    "select GetGroups() as e1, GetGroups().Take(10) as e2 from S0#Lastevent()";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendSBEventAndTrigger(epService, "E1", 20);
            foreach (EventBean @event in new EventBean[]{listener.AssertOneGetNew(), stmt.First()}) {
                AssertMapField("e1", @event, fields, new Object[]{"E1", 20});
                AssertMapMultiRow("e2", @event, "c0", fields, new Object[][]{new object[] {"E1", 20}});
            }
            listener.Reset();
    
            SendSBEventAndTrigger(epService, "E2", 30);
            foreach (EventBean @event in new EventBean[]{listener.AssertOneGetNew(), stmt.First()}) {
                AssertMapField("e1", @event, fields, null);
                AssertMapMultiRow("e2", @event, "c0", fields, new Object[][]{new object[] {"E1", 20}, new object[] {"E2", 30}});
            }
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultirowGroupedUncorrelatedWithEnumerationMethod(EPServiceProvider epService) {
            string fieldName = "subq";
            string[] fields = "c0,c1".Split(',');
    
            // test unfiltered
            string eplEnumUnfiltered = "select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " group by theString).Take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumUnfiltered = epService.EPAdministrator.CreateEPL(eplEnumUnfiltered);
            var listener = new SupportUpdateListener();
            stmtEnumUnfiltered.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 10}});
    
            SendSBEventAndTrigger(epService, "E1", 20);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 30}});
    
            SendSBEventAndTrigger(epService, "E2", 100);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 30}, new object[] {"E2", 100}});
    
            SendSBEventAndTrigger(epService, "E3", 2000);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 30}, new object[] {"E2", 100}, new object[] {"E3", 2000}});
            stmtEnumUnfiltered.Dispose();
    
            // test filtered
            string eplEnumFiltered = "select " +
                    "(select theString as c0, sum(intPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " where intPrimitive > 100 " +
                    " group by theString).Take(100) as subq " +
                    "from S0 as s0";
            EPStatement stmtEnumFiltered = epService.EPAdministrator.CreateEPL(eplEnumFiltered);
            stmtEnumFiltered.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 10);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, null);
    
            SendSBEventAndTrigger(epService, "E1", 200);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 200}});
    
            SendSBEventAndTrigger(epService, "E1", 11);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 200}});
    
            SendSBEventAndTrigger(epService, "E1", 201);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 401}});
    
            SendSBEventAndTrigger(epService, "E2", 300);
            AssertMapMultiRowAndReset(fieldName, listener, "c0", fields, new Object[][]{new object[] {"E1", 401}, new object[] {"E2", 300}});
    
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
    
        private void AssertMapFieldAndReset(string fieldName, SupportUpdateListener listener, string[] names, Object[] values) {
            AssertMapField(fieldName, listener.AssertOneGetNew(), names, values);
            listener.Reset();
        }
    
        private void AssertMapMultiRowAndReset(string fieldName, SupportUpdateListener listener, string sortKey, string[] names, Object[][] values) {
            AssertMapMultiRow(fieldName, listener.AssertOneGetNew(), sortKey, names, values);
            listener.Reset();
        }
    
        private void AssertMapField(string fieldName, EventBean @event, string[] names, Object[] values) {
            IDictionary<string, Object> subq = (IDictionary<string, Object>) @event.Get(fieldName);
            if (values == null && subq == null) {
                return;
            }
            EPAssertionUtil.AssertPropsMap(subq, names, values);
        }
    
        internal static void AssertMapMultiRow(string fieldName, EventBean @event, string sortKey, string[] names, Object[][] values) {
            ICollection<Map> subq = (ICollection<Map>) @event.Get(fieldName);
            if (values == null && subq == null) {
                return;
            }
            var maps = subq
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
