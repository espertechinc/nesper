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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.type;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinUnidirectionalStream : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPatternUnidirectionalOuterJoinNoOn(epService);
            RunAssertion2TableJoinGrouped(epService);
            RunAssertion2TableJoinRowForAll(epService);
            RunAssertion3TableOuterJoinVar1(epService);
            RunAssertion3TableOuterJoinVar2(epService);
            RunAssertionPatternJoin(epService);
            RunAssertionPatternJoinOutputRate(epService);
            RunAssertion3TableJoinVar1(epService);
            RunAssertion3TableJoinVar2A(epService);
            RunAssertion3TableJoinVar2B(epService);
            RunAssertion3TableJoinVar3(epService);
            RunAssertion2TableFullOuterJoin(epService);
            RunAssertion2TableFullOuterJoinCompile(epService);
            RunAssertion2TableFullOuterJoinOM(epService);
            RunAssertion2TableFullOuterJoinBackwards(epService);
            RunAssertion2TableJoin(epService);
            RunAssertion2TableBackwards(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionPatternUnidirectionalOuterJoinNoOn(EPServiceProvider epService) {
            // test 2-stream left outer join and SODA
            //
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            string stmtTextLO = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from pattern [every timer:interval(1 seconds)] unidirectional " +
                    "left outer join " +
                    "SupportBean#keepall";
            EPStatement stmtLO = epService.EPAdministrator.CreateEPL(stmtTextLO);
            var listener = new SupportUpdateListener();
            stmtLO.Events += listener.Update;
    
            TryAssertionPatternUniOuterJoinNoOn(epService, listener, 0);
    
            stmtLO.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtTextLO);
            Assert.AreEqual(stmtTextLO, model.ToEPL());
            stmtLO = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTextLO, stmtLO.Text);
            stmtLO.Events += listener.Update;
    
            TryAssertionPatternUniOuterJoinNoOn(epService, listener, 100000);
    
            stmtLO.Dispose();
    
            // test 2-stream inner join
            //
            string[] fieldsIJ = "c0,c1".Split(',');
            string stmtTextIJ = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from SupportBean_S0 unidirectional " +
                    "inner join " +
                    "SupportBean#keepall";
            EPStatement stmtIJ = epService.EPAdministrator.CreateEPL(stmtTextIJ);
            stmtIJ.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsIJ, new object[]{100, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S0_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsIJ, new object[]{300, 2L});
            stmtIJ.Dispose();
    
            // test 2-stream inner join with group-by
            TryAssertion2StreamInnerWGroupBy(epService, listener);
    
            // test 3-stream inner join
            //
            string[] fields3IJ = "c0,c1".Split(',');
            string stmtText3IJ = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from " +
                    "SupportBean_S0#keepall " +
                    "inner join " +
                    "SupportBean_S1#keepall " +
                    "inner join " +
                    "SupportBean#keepall";
    
            EPStatement stmt3IJ = epService.EPAdministrator.CreateEPL(stmtText3IJ);
            stmt3IJ.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "S1_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields3IJ, new object[]{50, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 51));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields3IJ, new object[]{101, 2L});
    
            stmt3IJ.Dispose();
    
            // test 3-stream full outer join
            //
            string[] fields3FOJ = "p00,p10,TheString".Split(',');
            string stmtText3FOJ = "select p00, p10, TheString " +
                    "from " +
                    "SupportBean_S0#keepall " +
                    "full outer join " +
                    "SupportBean_S1#keepall " +
                    "full outer join " +
                    "SupportBean#keepall";
    
            EPStatement stmt3FOJ = epService.EPAdministrator.CreateEPL(stmtText3FOJ);
            stmt3FOJ.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields3FOJ, new object[]{"S0_1", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E10", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields3FOJ, new object[]{null, null, "E10"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields3FOJ, new object[][]{new object[] {"S0_2", null, null}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "S1_0"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields3FOJ, new object[][]{new object[] {"S0_1", "S1_0", "E10"}, new object[] {"S0_2", "S1_0", "E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields3FOJ, new object[][]{new object[] {"S0_3", "S1_0", "E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E11", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields3FOJ, new object[][]{new object[] {"S0_1", "S1_0", "E11"}, new object[] {"S0_2", "S1_0", "E11"}, new object[] {"S0_3", "S1_0", "E11"}});
            Assert.AreEqual(6, EPAssertionUtil.EnumeratorCount(stmt3FOJ.GetEnumerator()));
    
            stmt3FOJ.Dispose();
    
            // test 3-stream full outer join with where-clause
            //
            string[] fields3FOJW = "p00,p10,TheString".Split(',');
            string stmtText3FOJW = "select p00, p10, TheString " +
                    "from " +
                    "SupportBean_S0#keepall as s0 " +
                    "full outer join " +
                    "SupportBean_S1#keepall as s1 " +
                    "full outer join " +
                    "SupportBean#keepall as sb " +
                    "where s0.p00 = s1.p10";
    
            EPStatement stmt3FOJW = epService.EPAdministrator.CreateEPL(stmtText3FOJW);
            stmt3FOJW.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "X1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "Y1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "Y1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields3FOJW, new object[]{"Y1", "Y1", null});
    
            stmt3FOJW.Dispose();
        }
    
        private void TryAssertion2StreamInnerWGroupBy(EPServiceProvider epService, SupportUpdateListener listener) {
            epService.EPAdministrator.CreateEPL("create objectarray schema E1 (id string, grp string, value int)");
            epService.EPAdministrator.CreateEPL("create objectarray schema E2 (id string, value2 int)");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select count(*) as c0, sum(E1.value) as c1, E1.id as c2 " +
                    "from E1 unidirectional inner join E2#keepall on E1.id = E2.id group by E1.grp");
            stmt.Events += listener.Update;
            string[] fields = "c0,c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(new object[]{"A", 100}, "E2");
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new object[]{"A", "X", 10}, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 10, "A"});
    
            epService.EPRuntime.SendEvent(new object[]{"A", "Y", 20}, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1L, 20, "A"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionPatternUniOuterJoinNoOn(EPServiceProvider epService, SupportUpdateListener listener, long startTime) {
            string[] fields = "c0,c1".Split(',');
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 2000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 3000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 4000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{21, 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 5000));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{33, 3L});
        }
    
        private void RunAssertion2TableJoinGrouped(EPServiceProvider epService) {
            string stmtText = "select irstream symbol, count(*) as cnt " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName + "#keepall " +
                    "where TheString = symbol group by TheString, symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryUnsupportedIterator(stmt);
    
            // send event, expect result
            SendEventMD(epService, "E1", 1L);
            string[] fields = "symbol,cnt".Split(',');
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E1", 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 2L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E1", 0L});
            listener.Reset();
    
            SendEvent(epService, "E1", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 3L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E1", 2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E1", 0L});
            listener.Reset();
    
            try {
                stmt.GetSafeEnumerator();
                Assert.Fail();
            } catch (UnsupportedOperationException ex) {
                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
            }
            // assure lock given up by sending more events
    
            SendEvent(epService, "E2", 40);
            SendEventMD(epService, "E2", 4L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E2", 1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E2", 0L});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertion2TableJoinRowForAll(EPServiceProvider epService) {
            string stmtText = "select irstream count(*) as cnt " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName + "#keepall " +
                    "where TheString = symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryUnsupportedIterator(stmt);
    
            // send event, expect result
            SendEventMD(epService, "E1", 1L);
            string[] fields = "cnt".Split(',');
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E1", 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 2L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            listener.Reset();
    
            SendEvent(epService, "E1", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 3L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{2L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
            listener.Reset();
    
            SendEvent(epService, "E2", 40);
            SendEventMD(epService, "E2", 4L);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{0L});
    
            stmt.Dispose();
        }
    
        private void RunAssertion3TableOuterJoinVar1(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    " full outer join " + typeof(SupportBean_S1).FullName + "#keepall as s1" +
                    " on p00 = p10 " +
                    " full outer join " + typeof(SupportBean_S2).FullName + "#keepall as s2" +
                    " on p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableOuterJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertion3TableOuterJoinVar2(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    " left outer join " + typeof(SupportBean_S1).FullName + "#keepall as s1 " +
                    " on p00 = p10 " +
                    " left outer join " + typeof(SupportBean_S2).FullName + "#keepall as s2 " +
                    " on p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableOuterJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertionPatternJoin(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            // no iterator allowed
            string stmtText = "select count(*) as num " +
                    "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                    "SupportBean(IntPrimitive=1)#unique(TheString) a,\n" +
                    "SupportBean(IntPrimitive=2)#unique(TheString) b\n" +
                    "where a.TheString = b.TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            SendEvent(epService, "A", 2);
            SendEvent(epService, "B", 1);
            SendEvent(epService, "B", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(70000));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("num"));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(140000));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("num"));
        }
    
        private void RunAssertionPatternJoinOutputRate(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            // no iterator allowed
            string stmtText = "select count(*) as num " +
                    "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                    "SupportBean(IntPrimitive=1)#unique(TheString) a,\n" +
                    "SupportBean(IntPrimitive=2)#unique(TheString) b\n" +
                    "where a.TheString = b.TheString output every 2 minutes";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "A", 1);
            SendEvent(epService, "A", 2);
            SendEvent(epService, "B", 1);
            SendEvent(epService, "B", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(70000));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(140000));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(210000));
            Assert.AreEqual(2L, listener.LastNewData[0].Get("num"));
            Assert.AreEqual(2L, listener.LastNewData[1].Get("num"));
        }
    
        private void Try3TableOuterJoin(EPServiceProvider epService, EPStatement statement) {
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            string[] fields = "s0.id,s1.id,s2.id".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, null, null});
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 20, null});
            epService.EPRuntime.SendEvent(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(300, "E3"));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, null, null});
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(31, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 21, 31});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(32, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertion3TableJoinVar1(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S1).FullName + "#keepall as s1, " +
                    typeof(SupportBean_S2).FullName + "#keepall as s2 " +
                    "where p00 = p10 and p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertion3TableJoinVar2A(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S1).FullName + "#keepall as s1, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S2).FullName + "#keepall as s2 " +
                    "where p00 = p10 and p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertion3TableJoinVar2B(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S2).FullName + "#keepall as s2, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S1).FullName + "#keepall as s1 " +
                    "where p00 = p10 and p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertion3TableJoinVar3(EPServiceProvider epService) {
            string stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S1).FullName + "#keepall as s1, " +
                    typeof(SupportBean_S2).FullName + "#keepall as s2, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    "where p00 = p10 and p10 = p20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(epService, stmt);
            stmt.Dispose();
        }
    
        private void Try3TableJoin(EPServiceProvider epService, EPStatement statement) {
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(300, "E3"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E3"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(31, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E4"));
            string[] fields = "s0.id,s1.id,s2.id".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{11, 21, 31});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(32, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertion2TableFullOuterJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    "#keepall on TheString = symbol";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            TryFullOuterPassive2Stream(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertion2TableFullOuterJoinCompile(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    "#keepall on TheString = symbol";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
    
            TryFullOuterPassive2Stream(epService, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertion2TableFullOuterJoinOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol", "volume", "TheString", "IntPrimitive");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).Unidirectional(true));
            model.FromClause.Add(FilterStream.Create(typeof(SupportBean).FullName).AddView("keepall"));
            model.FromClause.Add(OuterJoinQualifier.Create("TheString", OuterJoinType.FULL, "symbol"));
    
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    "#keepall on TheString = symbol";
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
    
            TryFullOuterPassive2Stream(epService, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertion2TableFullOuterJoinBackwards(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportBean).FullName +
                    "#keepall full outer join " +
                    typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "on TheString = symbol";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
    
            TryFullOuterPassive2Stream(epService, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertion2TableJoin(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName +
                    "#keepall where TheString = symbol";
    
            TryJoinPassive2Stream(epService, stmtText);
        }
    
        private void RunAssertion2TableBackwards(EPServiceProvider epService) {
            string stmtText = "select symbol, volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportBean).FullName + "#keepall, " +
                    typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "where TheString = symbol";
    
            TryJoinPassive2Stream(epService, stmtText);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string text = "select * from " + typeof(SupportBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportMarketDataBean).FullName + "#keepall unidirectional " +
                    "on TheString = symbol";
            TryInvalid(epService, text, "Error starting statement: The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");
    
            text = "select * from " + typeof(SupportBean).FullName + "#length(2) unidirectional " +
                    "full outer join " +
                    typeof(SupportMarketDataBean).FullName + "#keepall " +
                    "on TheString = symbol";
            TryInvalid(epService, text, "Error starting statement: The unidirectional keyword requires that no views are declared onto the stream");
        }
    
        private void TryFullOuterPassive2Stream(EPServiceProvider epService, EPStatement stmt) {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryUnsupportedIterator(stmt);
    
            // send event, expect result
            SendEventMD(epService, "E1", 1L);
            string[] fields = "symbol,volume,TheString,IntPrimitive".Split(',');
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L, null, null});
    
            SendEvent(epService, "E1", 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 2L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2L, "E1", 10});
    
            SendEvent(epService, "E1", 20);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void TryJoinPassive2Stream(EPServiceProvider epService, string stmtText) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            TryUnsupportedIterator(stmt);
    
            // send event, expect result
            SendEventMD(epService, "E1", 1L);
            string[] fields = "symbol,volume,TheString,IntPrimitive".Split(',');
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E1", 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventMD(epService, "E1", 2L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2L, "E1", 10});
    
            SendEvent(epService, "E1", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string s, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEventMD(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void TryUnsupportedIterator(EPStatement stmt) {
            try {
                stmt.GetEnumerator();
                Assert.Fail();
            } catch (UnsupportedOperationException ex) {
                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
            }
        }
    }
} // end of namespace
