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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectAggregatedSingleValue : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionUngroupedUncorrelatedInSelect(epService);
            RunAssertionUngroupedUncorrelatedTwoAggStopStart(epService);
            RunAssertionUngroupedUncorrelatedNoDataWindow(epService);
            RunAssertionUngroupedUncorrelatedWHaving(epService);
            RunAssertionUngroupedUncorrelatedInWhereClause(epService);
            RunAssertionUngroupedUncorrelatedInSelectClause(epService);
            RunAssertionUngroupedUncorrelatedFiltered(epService);
            RunAssertionUngroupedUncorrelatedWWhereClause(epService);
            RunAssertionUngroupedCorrelated(epService);
            RunAssertionUngroupedCorrelatedInWhereClause(epService);
            RunAssertionUngroupedCorrelatedWHaving(epService);
            RunAssertionUngroupedCorrelationInsideHaving(epService);
            RunAssertionUngroupedTableWHaving(epService);
            RunAssertionGroupedUncorrelatedWHaving(epService);
            RunAssertionGroupedCorrelatedWHaving(epService);
            RunAssertionGroupedTableWHaving(epService);
            RunAssertionGroupedCorrelationInsideHaving(epService);
    
            // invalid tests
            string stmtText;
    
            TryInvalid(epService, "", "Unexpected end-of-input []");
    
            stmtText = "select (select sum(s0.id) from S1#length(3) as s1) as value from S0 as s0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties");
    
            stmtText = "select (select s1.id + sum(s1.id) from S1#length(3) as s1) as value from S0 as s0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect properties must all be within aggregation functions");
    
            stmtText = "select (select sum(s0.id + s1.id) from S1#length(3) as s1) as value from S0 as s0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties");
    
            // having-clause cannot aggregate over properties from other streams
            stmtText = "select (select last(TheString) from SupportBean#keepall having sum(s0.p00) = 1) as c0 from S0 as s0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Failed to validate having-clause expression '(sum(s0.p00))=1': Implicit conversion from datatype 'String' to numeric is not allowed for aggregation function 'sum' [");
    
            // having-clause properties must be aggregated
            stmtText = "select (select last(TheString) from SupportBean#keepall having sum(IntPrimitive) = IntPrimitive) as c0 from S0 as s0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect having-clause requires that all properties are under aggregation, consider using the 'first' aggregation function instead");
    
            // having-clause not returning boolean
            stmtText = "select (select last(TheString) from SupportBean#keepall having sum(IntPrimitive)) as c0 from S0";
            TryInvalid(epService, stmtText, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subselect having-clause expression must return a boolean value ");
        }
    
        private void RunAssertionGroupedCorrelationInsideHaving(EPServiceProvider epService) {
            string epl = "select (select TheString from SupportBean#keepall group by TheString having sum(IntPrimitive) = s0.id) as c0 from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSB(epService, "E1", 100);
            SendSB(epService, "E2", 5);
            SendSB(epService, "E3", 20);
            SendEventS0Assert(epService, listener, 1, null);
            SendEventS0Assert(epService, listener, 5, "E2");
    
            SendSB(epService, "E2", 3);
            SendEventS0Assert(epService, listener, 5, null);
            SendEventS0Assert(epService, listener, 8, "E2");
            SendEventS0Assert(epService, listener, 20, "E3");
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedCorrelationInsideHaving(EPServiceProvider epService) {
            string epl = "select (select last(TheString) from SupportBean#keepall having sum(IntPrimitive) = s0.id) as c0 from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSB(epService, "E1", 100);
            SendEventS0Assert(epService, listener, 1, null);
            SendEventS0Assert(epService, listener, 100, "E1");
    
            SendSB(epService, "E2", 5);
            SendEventS0Assert(epService, listener, 100, null);
            SendEventS0Assert(epService, listener, 105, "E2");
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedTableWHaving(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTableWith2Keys(k1 string primary key, k2 string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table MyTableWith2Keys select p10 as k1, p11 as k2, sum(id) as total from S1 group by p10, p11");
    
            string epl = "select (select sum(total) from MyTableWith2Keys group by k1 having sum(total) > 100) as c0 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS1(epService, 50, "G1", "S1");
            SendEventS1(epService, 50, "G1", "S2");
            SendEventS1(epService, 50, "G2", "S1");
            SendEventS1(epService, 50, "G2", "S2");
            SendEventS0Assert(listener, epService, null);
    
            SendEventS1(epService, 1, "G2", "S3");
            SendEventS0Assert(listener, epService, 101);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUngroupedTableWHaving(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create table MyTable(total sum(int))");
            epService.EPAdministrator.CreateEPL("into table MyTable select sum(IntPrimitive) as total from SupportBean");
    
            string epl = "select (select sum(total) from MyTable having sum(total) > 100) as c0 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0Assert(listener, epService, null);
    
            SendSB(epService, "E1", 50);
            SendEventS0Assert(listener, epService, null);
    
            SendSB(epService, "E2", 55);
            SendEventS0Assert(listener, epService, 105);
    
            SendSB(epService, "E3", -5);
            SendEventS0Assert(listener, epService, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupedCorrelatedWHaving(EPServiceProvider epService) {
            string epl = "select (select sum(IntPrimitive) from SupportBean#keepall where s0.id = IntPrimitive group by TheString having sum(IntPrimitive) > 10) as c0 from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0Assert(epService, listener, 10, null);
    
            SendSB(epService, "G1", 10);
            SendSB(epService, "G2", 10);
            SendSB(epService, "G2", 2);
            SendSB(epService, "G1", 9);
            SendEventS0Assert(listener, epService, null);
    
            SendSB(epService, "G2", 10);
            SendEventS0Assert(epService, listener, 10, 20);
    
            SendSB(epService, "G1", 10);
            SendEventS0Assert(epService, listener, 10, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedUncorrelatedWHaving(EPServiceProvider epService) {
            string epl = "select (select sum(IntPrimitive) from SupportBean#keepall group by TheString having sum(IntPrimitive) > 10) as c0 from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0Assert(listener, epService, null);
    
            SendSB(epService, "G1", 10);
            SendSB(epService, "G2", 9);
            SendEventS0Assert(listener, epService, null);
    
            SendSB(epService, "G2", 2);
            SendEventS0Assert(listener, epService, 11);
    
            SendSB(epService, "G1", 3);
            SendEventS0Assert(listener, epService, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedCorrelatedWHaving(EPServiceProvider epService) {
            string epl = "select (select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.p00 having sum(IntPrimitive) > 10) as c0 from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0Assert(epService, listener, "G1", null);
    
            SendSB(epService, "G1", 10);
            SendEventS0Assert(epService, listener, "G1", null);
    
            SendSB(epService, "G2", 11);
            SendEventS0Assert(epService, listener, "G1", null);
            SendEventS0Assert(epService, listener, "G2", 11);
    
            SendSB(epService, "G1", 12);
            SendEventS0Assert(epService, listener, "G1", 22);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedFiltered(EPServiceProvider epService) {
            string stmtText = "select (select sum(id) from S1(id < 0)#length(3)) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunAssertionSumFilter(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedWWhereClause(EPServiceProvider epService) {
            string stmtText = "select (select sum(id) from S1#length(3) where id < 0) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunAssertionSumFilter(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCorrAggWhereGreater(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "p00".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 10));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "T1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "T1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
            epService.EPRuntime.SendEvent(new SupportBean_S0(21, "T1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(22, "T1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T1"});
        }
    
        private void RunAssertionSumFilter(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEventS0(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 1);
            SendEventS0(epService, 2);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 0);
            SendEventS0(epService, 3);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, -1);
            SendEventS0(epService, 4);
            Assert.AreEqual(-1, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, -3);
            SendEventS0(epService, 5);
            Assert.AreEqual(-4, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, -5);
            SendEventS0(epService, 6);
            Assert.AreEqual(-9, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, -2);   // note event leaving window
            SendEventS0(epService, 6);
            Assert.AreEqual(-10, listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void RunAssertionUngroupedUncorrelatedNoDataWindow(EPServiceProvider epService) {
            string stmtText = "select p00 as c0, (select sum(IntPrimitive) from SupportBean) as c1 from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 30});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedWHaving(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select *, " +
                    "(select sum(IntPrimitive) from SupportBean#keepall having sum(IntPrimitive) > 100) as c0," +
                    "exists (select sum(IntPrimitive) from SupportBean#keepall having sum(IntPrimitive) > 100) as c1 " +
                    "from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0Assert(epService, listener, fields, new object[]{null, false});
            SendSB(epService, "E1", 10);
            SendEventS0Assert(epService, listener, fields, new object[]{null, false});
            SendSB(epService, "E1", 91);
            SendEventS0Assert(epService, listener, fields, new object[]{101, true});
            SendSB(epService, "E1", 2);
            SendEventS0Assert(epService, listener, fields, new object[]{103, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedCorrelated(EPServiceProvider epService) {
            string stmtText = "select p00, " +
                    "(select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.p00) as sump00 " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "p00,sump00".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T1", 21});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "T2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T2", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("T2", -2));
            epService.EPRuntime.SendEvent(new SupportBean("T2", -7));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "T2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T2", -9});
            stmt.Dispose();
    
            // test distinct
            fields = "TheString,c0,c1,c2,c3".Split(',');
            string epl = "select TheString, " +
                    "(select count(sb.IntPrimitive) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c0, " +
                    "(select count(distinct sb.IntPrimitive) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c1, " +
                    "(select count(sb.IntPrimitive, true) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c2, " +
                    "(select count(distinct sb.IntPrimitive, true) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c3 " +
                    "from SupportBean as bean";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, 1L, 1L, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L, 1L, 1L, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L, 2L, 2L, 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 3L, 2L, 3L, 2L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedCorrelatedInWhereClause(EPServiceProvider epService) {
            string stmtText = "select p00 from S0 as s0 where id > " +
                    "(select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.p00)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            RunAssertionCorrAggWhereGreater(epService, listener);
            stmt.Dispose();
    
            stmtText = "select p00 from S0 as s0 where id > " +
                    "(select sum(IntPrimitive) from SupportBean#keepall where TheString||'X' = s0.p00||'X')";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
            RunAssertionCorrAggWhereGreater(epService, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedInWhereClause(EPServiceProvider epService) {
            string stmtText = "select * from MarketData " +
                    "where price > (select max(price) from MarketData(symbol='GOOG')#lastevent) ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventMD(epService, "GOOG", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "GOOG", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            Object theEvent = SendEventMD(epService, "IBM", 3);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedInSelectClause(EPServiceProvider epService) {
            string stmtText = "select (select s0.id + max(s1.id) from S1#length(3) as s1) as value from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 100);
            SendEventS0(epService, 2);
            Assert.AreEqual(102, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 30);
            SendEventS0(epService, 3);
            Assert.AreEqual(103, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedInSelect(EPServiceProvider epService) {
            string stmtText = "select (select max(id) from S1#length(3)) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 100);
            SendEventS0(epService, 2);
            Assert.AreEqual(100, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 200);
            SendEventS0(epService, 3);
            Assert.AreEqual(200, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 190);
            SendEventS0(epService, 4);
            Assert.AreEqual(200, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 180);
            SendEventS0(epService, 5);
            Assert.AreEqual(200, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 170);   // note event leaving window
            SendEventS0(epService, 6);
            Assert.AreEqual(190, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedUncorrelatedTwoAggStopStart(EPServiceProvider epService) {
            string stmtText = "select (select avg(id) + max(id) from S1#length(3)) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0(epService, 1);
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 100);
            SendEventS0(epService, 2);
            Assert.AreEqual(200.0, listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(epService, 200);
            SendEventS0(epService, 3);
            Assert.AreEqual(350.0, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Stop();
            SendEventS1(epService, 10000);
            SendEventS0(epService, 4);
            Assert.IsFalse(listener.IsInvoked);
            stmt.Start();
    
            SendEventS1(epService, 10);
            SendEventS0(epService, 5);
            Assert.AreEqual(20.0, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void SendEventS0(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(id));
        }
    
        private void SendEventS0(EPServiceProvider epService, int id, string p00) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(id, p00));
        }
    
        private void SendEventS1(EPServiceProvider epService, int id, string p10, string p11) {
            epService.EPRuntime.SendEvent(new SupportBean_S1(id, p10, p11));
        }
    
        private void SendEventS1(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new SupportBean_S1(id));
        }
    
        private Object SendEventMD(EPServiceProvider epService, string symbol, double price) {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendSB(EPServiceProvider epService, string theString, int intPrimitive) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
        }
    
        private void SendEventS0Assert(SupportUpdateListener listener, EPServiceProvider epService, Object expected) {
            SendEventS0Assert(epService, listener, 0, expected);
        }
    
        private void SendEventS0Assert(EPServiceProvider epService, SupportUpdateListener listener, int id, Object expected) {
            SendEventS0(epService, id, null);
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        private void SendEventS0Assert(EPServiceProvider epService, SupportUpdateListener listener, string p00, Object expected) {
            SendEventS0(epService, 0, p00);
            Assert.AreEqual(expected, listener.AssertOneGetNewAndReset().Get("c0"));
        }
    
        private void SendEventS0Assert(EPServiceProvider epService, SupportUpdateListener listener, string[] fields, object[] expected) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
        }
    }
} // end of namespace
