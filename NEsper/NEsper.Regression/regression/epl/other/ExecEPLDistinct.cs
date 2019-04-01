///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLDistinct : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_N", typeof(SupportBean_N));
    
            RunAssertionWildcardJoinPattern(epService);
            RunAssertionOnDemandAndOnSelect(epService);
            RunAssertionSubquery(epService);
            RunAssertionBeanEventWildcardThisProperty(epService);
            RunAssertionBeanEventWildcardSODA(epService);
            RunAssertionBeanEventWildcardPlusCols(epService);
            RunAssertionMapEventWildcard(epService);
            RunAssertionOutputSimpleColumn(epService);
            RunAssertionOutputLimitEveryColumn(epService);
            RunAssertionOutputRateSnapshotColumn(epService);
            RunAssertionBatchWindow(epService);
            RunAssertionBatchWindowJoin(epService);
            RunAssertionBatchWindowInsertInto(epService);
        }
    
        private void RunAssertionWildcardJoinPattern(EPServiceProvider epService) {
            string epl = "select distinct * from " +
                    "SupportBean(IntPrimitive=0) as fooB unidirectional " +
                    "inner join " +
                    "pattern [" +
                    "every-distinct(fooA.TheString) fooA=SupportBean(IntPrimitive=1)" +
                    "->" +
                    "every-distinct(wooA.TheString) wooA=SupportBean(IntPrimitive=2)" +
                    " where timer:within(1 hour)" +
                    "]#time(1 hour) as fooWooPair " +
                    "on fooB.LongPrimitive = fooWooPair.fooA.LongPrimitive";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10L);
            SendEvent(epService, "E1", 2, 10L);
    
            SendEvent(epService, "E2", 1, 10L);
            SendEvent(epService, "E2", 2, 10L);
    
            SendEvent(epService, "E3", 1, 10L);
            SendEvent(epService, "E3", 2, 10L);
    
            SendEvent(epService, "Query", 0, 10L);
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void RunAssertionOnDemandAndOnSelect(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            string query = "select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive";
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(query);
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_A select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive asc");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("x"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean where TheString in (select distinct id from SupportBean_A#keepall)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3});
    
            stmt.Dispose();
        }
    
        // Since the "this" property will always be unique, this test verifies that condition
        private void RunAssertionBeanEventWildcardThisProperty(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "select distinct * from SupportBean#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E1", 1}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBeanEventWildcardSODA(EPServiceProvider epService) {
            var fields = new string[]{"id"};
            string statementText = "select distinct * from SupportBean_A#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(statementText);
            Assert.AreEqual(statementText, model.ToEPL());
    
            model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard().Distinct(true);
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));
            Assert.AreEqual("select distinct * from SupportBean_A", model.ToEPL());
    
            stmt.Dispose();
        }
    
        private void RunAssertionBeanEventWildcardPlusCols(EPServiceProvider epService) {
            var fields = new string[]{"IntPrimitive", "val1", "val2"};
            string statementText = "select distinct *, IntBoxed%5 as val1, IntBoxed as val2 from SupportBean_N#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_N(1, 8));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, 3, 8}});
    
            epService.EPRuntime.SendEvent(new SupportBean_N(1, 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, 3, 8}, new object[] {1, 3, 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean_N(1, 8));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1, 3, 8}, new object[] {1, 3, 3}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMapEventWildcard(EPServiceProvider epService) {
            var def = new Dictionary<string, object>();
            def.Put("k1", typeof(string));
            def.Put("v1", typeof(int));
            epService.EPAdministrator.Configuration.AddEventType("MyMapType", def);
    
            var fields = new string[]{"k1", "v1"};
            string statementText = "select distinct * from MyMapType#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMapEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            SendMapEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            SendMapEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputSimpleColumn(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "select distinct TheString, IntPrimitive from SupportBean#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSimpleColumn(epService, listener, stmt, fields);
            stmt.Dispose();
    
            // test join
            statementText = "select distinct TheString, IntPrimitive from SupportBean#keepall a, SupportBean_A#keepall b where a.TheString = b.id";
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            TryAssertionSimpleColumn(epService, listener, stmt, fields);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputLimitEveryColumn(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "@IterableUnbound select distinct TheString, IntPrimitive from SupportBean output every 3 events";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionOutputEvery(epService, listener, stmt, fields);
            stmt.Dispose();
    
            // test join
            statementText = "select distinct TheString, IntPrimitive from SupportBean#lastevent a, SupportBean_A#keepall b where a.TheString = b.id output every 3 events";
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            TryAssertionOutputEvery(epService, listener, stmt, fields);
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputRateSnapshotColumn(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "select distinct TheString, IntPrimitive from SupportBean#keepall output snapshot every 3 events order by TheString asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSnapshotColumn(epService, listener, stmt, fields);
            stmt.Dispose();
    
            statementText = "select distinct TheString, IntPrimitive from SupportBean#keepall a, SupportBean_A#keepall b where a.TheString = b.id output snapshot every 3 events order by TheString asc";
            stmt = epService.EPAdministrator.CreateEPL(statementText);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            TryAssertionSnapshotColumn(epService, listener, stmt, fields);
    
            stmt.Dispose();
        }
    
        private void RunAssertionBatchWindow(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "select distinct TheString, IntPrimitive from SupportBean#length_batch(3)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E2", 2}, new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E2", 3}});
    
            stmt.Dispose();
    
            // test batch window with aggregation
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var fieldsTwo = new string[]{"c1", "c2"};
            string epl = "insert into ABC select distinct TheString as c1, first(IntPrimitive) as c2 from SupportBean#time_batch(1 second)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(epl);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fieldsTwo, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(listener.IsInvoked);
    
            stmtTwo.Dispose();
        }
    
        private void RunAssertionBatchWindowJoin(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "select distinct TheString, IntPrimitive from SupportBean#length_batch(3) a, SupportBean_A#keepall b where a.TheString = b.id";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 2}, new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 3}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBatchWindowInsertInto(EPServiceProvider epService) {
            var fields = new string[]{"TheString", "IntPrimitive"};
            string statementText = "insert into MyStream select distinct TheString, IntPrimitive from SupportBean#length_batch(3)";
            epService.EPAdministrator.CreateEPL(statementText);
    
            statementText = "select * from MyStream";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.GetNewDataListFlattened()[0], fields, new object[]{"E2", 2});
            EPAssertionUtil.AssertProps(listener.GetNewDataListFlattened()[1], fields, new object[]{"E3", 3});
    
            stmt.Dispose();
        }
    
        private void TryAssertionOutputEvery(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 2}, new object[] {"E1", 1}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 3}});
            listener.Reset();
        }
    
        private void TryAssertionSimpleColumn(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
        }
    
        private void TryAssertionSnapshotColumn(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt, string[] fields) {
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            listener.Reset();
        }
    
        private void SendMapEvent(EPServiceProvider epService, string s, int i) {
            var def = new Dictionary<string, object>();
            def.Put("k1", s);
            def.Put("v1", i);
            epService.EPRuntime.SendEvent(def, "MyMapType");
        }
    }
} // end of namespace
