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


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewExpiryIntersect : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
    
            RunAssertionUniqueAndFirstLength(epService);
            RunAssertionFirstUniqueAndFirstLength(epService);
            RunAssertionFirstUniqueAndLengthOnDelete(epService);
            RunAssertionBatchWindow(epService);
            RunAssertionIntersectAndDerivedValue(epService);
            RunAssertionIntersectGroupBy(epService);
            RunAssertionIntersectSubselect(epService);
            RunAssertionIntersectThreeUnique(epService);
            RunAssertionIntersectPattern(epService);
            RunAssertionIntersectTwoUnique(epService);
            RunAssertionIntersectSorted(epService);
            RunAssertionIntersectTimeWin(epService);
            RunAssertionIntersectTimeWinReversed(epService);
            RunAssertionIntersectTimeWinSODA(epService);
            RunAssertionIntersectTimeWinNamedWindow(epService);
            RunAssertionIntersectTimeWinNamedWindowDelete(epService);
        }
    
        private void RunAssertionUniqueAndFirstLength(EPServiceProvider epService) {
            string epl = "select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#unique(TheString)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionUniqueAndFirstLength(epService, listener, stmt);
    
            stmt.Dispose();
            listener.Reset();
    
            epl = "select irstream TheString, IntPrimitive from SupportBean#unique(TheString)#firstlength(3)";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionUniqueAndFirstLength(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionFirstUniqueAndFirstLength(EPServiceProvider epService) {
            string epl = "select irstream TheString, IntPrimitive from SupportBean#firstunique(TheString)#firstlength(3)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionFirstUniqueAndLength(epService, listener, stmt);
    
            stmt.Dispose();
            epl = "select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#firstunique(TheString)";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionFirstUniqueAndLength(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionFirstUniqueAndLengthOnDelete(EPServiceProvider epService) {
            EPStatement nwstmt = epService.EPAdministrator.CreateEPL("create window MyWindowOne#firstunique(TheString)#firstlength(3) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindowOne where TheString = p00");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from MyWindowOne");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"TheString", "IntPrimitive"};
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            SendEvent(epService, "E1", 99);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
    
            SendEvent(epService, "E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3});
    
            SendEvent(epService, "E1", 99);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
    
            SendEvent(epService, "E3", 98);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionBatchWindow(EPServiceProvider epService) {
            EPStatement stmt;
            var listener = new SupportUpdateListener();
    
            // test window
            stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#length_batch(3)#unique(IntPrimitive) order by TheString asc");
            stmt.Events += listener.Update;
            TryAssertionUniqueAndBatch(epService, listener, stmt);
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#length_batch(3) order by TheString asc");
            stmt.Events += listener.Update;
            TryAssertionUniqueAndBatch(epService, listener, stmt);
            stmt.Dispose();
    
            // test aggregation with window
            stmt = epService.EPAdministrator.CreateEPL("select count(*) as c0, sum(IntPrimitive) as c1 from SupportBean#unique(TheString)#length_batch(3)");
            stmt.Events += listener.Update;
            TryAssertionUniqueBatchAggreation(epService, listener);
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select count(*) as c0, sum(IntPrimitive) as c1 from SupportBean#length_batch(3)#unique(TheString)");
            stmt.Events += listener.Update;
            TryAssertionUniqueBatchAggreation(epService, listener);
            stmt.Dispose();
    
            // test first-unique
            stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#firstunique(TheString)#length_batch(3)");
            stmt.Events += listener.Update;
            TryAssertionLengthBatchAndFirstUnique(epService, listener);
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#length_batch(3)#firstunique(TheString)");
            stmt.Events += listener.Update;
            TryAssertionLengthBatchAndFirstUnique(epService, listener);
            stmt.Dispose();
    
            // test time-based expiry
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#unique(TheString)#time_batch(1)");
            stmt.Events += listener.Update;
            TryAssertionTimeBatchAndUnique(epService, listener, 0);
            stmt.Dispose();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(100000));
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1)#unique(TheString)");
            stmt.Events += listener.Update;
            TryAssertionTimeBatchAndUnique(epService, listener, 100000);
            stmt.Dispose();
    
            try {
                epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1)#length_batch(10)");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot combined multiple batch data windows into an intersection [select * from SupportBean#time_batch(1)#length_batch(10)]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIntersectAndDerivedValue(EPServiceProvider epService) {
            var fields = new string[]{"total"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#uni(DoublePrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(100d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100d});
    
            SendEvent(epService, "E2", 2, 20, 50d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(150d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{150d});
    
            SendEvent(epService, "E3", 1, 20, 20d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(20d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{20d});
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectGroupBy(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            string text = "select irstream TheString from SupportBean#groupwin(IntPrimitive)#length(2)#unique(IntBoxed) retain-intersection";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendEvent(epService, "E4", 1, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
            SendEvent(epService, "E5", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E5"});
            listener.Reset();
    
            SendEvent(epService, "E6", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            SendEvent(epService, "E7", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E7"});
            listener.Reset();
    
            SendEvent(epService, "E8", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E8"});
            listener.Reset();
    
            // another combination
            stmt.Dispose();
            epService.EPAdministrator.CreateEPL("select * from SupportBean#groupwin(TheString)#time(.0083 sec)#firstevent").Dispose();
        }
    
        private void RunAssertionIntersectSubselect(EPServiceProvider epService) {
            string text = "select * from SupportBean_S0 where p00 in (select TheString from SupportBean#length(2)#unique(IntPrimitive) retain-intersection)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1);
            SendEvent(epService, "E2", 2);
            SendEvent(epService, "E3", 3); // throws out E1
            SendEvent(epService, "E4", 2); // throws out E2
            SendEvent(epService, "E5", 1); // throws out E3
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E3"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E4"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectThreeUnique(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#unique(DoublePrimitive) retain-intersection");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 10, 200d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E2"});
            listener.Reset();
    
            SendEvent(epService, "E3", 2, 20, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E3"});
            listener.Reset();
    
            SendEvent(epService, "E4", 1, 30, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5", 3, 40, 400d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            SendEvent(epService, "E6", 3, 40, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E6"));
            object[] result = {listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new string[]{"E4", "E5"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectPattern(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            string text = "select irstream a.p00||b.p10 as TheString from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]#unique(a.id)#unique(b.id) retain-intersection";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E3"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E4"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1E2", "E3E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3E4"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E6"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3E4", "E5E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E5E6"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectTwoUnique(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed) retain-intersection");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E2"});
            listener.Reset();
    
            SendEvent(epService, "E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendEvent(epService, "E4", 3, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
            SendEvent(epService, "E5", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E5"});
            listener.Reset();
    
            SendEvent(epService, "E6", 3, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            SendEvent(epService, "E7", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E7"));
            Assert.AreEqual(2, listener.LastOldData.Length);
            object[] result = {listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new string[]{"E5", "E6"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E7"});
            listener.Reset();
    
            SendEvent(epService, "E8", 4, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E7", "E8"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E8"});
    
            SendEvent(epService, "E9", 3, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E9"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E7"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E9"});
            listener.Reset();
    
            SendEvent(epService, "E10", 2, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E10"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E9"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E10"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectSorted(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#sort(2, IntPrimitive)#sort(2, IntBoxed) retain-intersection");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3"));
            object[] result = {listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new string[]{"E1", "E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E3"});
            listener.Reset();
    
            SendEvent(epService, "E4", -1, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5", 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            Assert.AreEqual(1, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E5"});
            listener.Reset();
    
            SendEvent(epService, "E6", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E6"));
            Assert.AreEqual(1, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectTimeWin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#time(10 sec) retain-intersection");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectTimeWinReversed(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#time(10 sec)#unique(IntPrimitive) retain-intersection");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectTimeWinSODA(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream TheString from SupportBean#time(10 seconds)#unique(IntPrimitive) retain-intersection";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionIntersectTimeWinNamedWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MyWindowTwo#time(10 sec)#unique(IntPrimitive) retain-intersection as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindowTwo where IntBoxed = id");
            var listener = new SupportUpdateListener();
            stmtWindow.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmtWindow);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIntersectTimeWinNamedWindowDelete(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowThree#time(10 sec)#unique(IntPrimitive) retain-intersection as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowThree select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindowThree where IntBoxed = id");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var fields = new string[]{"TheString"};
    
            SendTimer(epService, 1000);
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendTimer(epService, 2000);
            SendEvent(epService, "E2", 2, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
    
            SendTimer(epService, 3000);
            SendEvent(epService, "E3", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
            SendEvent(epService, "E4", 3, 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
            SendTimer(epService, 4000);
            SendEvent(epService, "E5", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
            SendEvent(epService, "E6", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4", "E6"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(50));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E6"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4"));
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4"));
    
            SendTimer(epService, 12999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 13000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr());
    
            SendTimer(epService, 10000000);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionTimeWinUnique(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"TheString"};
    
            SendTimer(epService, 1000);
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendTimer(epService, 2000);
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendTimer(epService, 3000);
            SendEvent(epService, "E3", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E3"});
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3"));
    
            SendTimer(epService, 4000);
            SendEvent(epService, "E4", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4"));
            SendEvent(epService, "E5", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E5"});
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E5"));
    
            SendTimer(epService, 11999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 12000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E5"));
    
            SendTimer(epService, 12999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 13000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5"));
    
            SendTimer(epService, 13999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 14000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr());
        }
    
        private void TryAssertionUniqueBatchAggreation(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 11));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L, 10 + 11 + 12});
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 13));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 14));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 15));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L, 13 + 14 + 15});
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 16));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 17));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 18));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L, 16 + 17 + 18});
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 19));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 20));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 21));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 22));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 23));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L, 20 + 22 + 23});
        }
    
        private void TryAssertionUniqueAndBatch(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"TheString"};
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
            Assert.IsNull(listener.GetAndResetLastOldData());
    
            SendEvent(epService, "E4", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E5", 4); // throws out E5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E5"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E6", 4); // throws out E6
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E7", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E6", "E7"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E8", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
            listener.Reset();
    
            SendEvent(epService, "E8", 7);
            SendEvent(epService, "E9", 9);
            SendEvent(epService, "E9", 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E8", "E9"));
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E10", 11);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E10"}, new object[] {"E8"}, new object[] {"E9"}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
            listener.Reset();
        }
    
        private void TryAssertionUniqueAndFirstLength(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"TheString", "IntPrimitive"};
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            SendEvent(epService, "E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E1", 3});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E1", 1});
            listener.Reset();
    
            SendEvent(epService, "E3", 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}, new object[] {"E3", 30}});
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{"E3", 30});
            listener.Reset();
    
            SendEvent(epService, "E4", 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}, new object[] {"E3", 30}});
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void TryAssertionFirstUniqueAndLength(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
    
            var fields = new string[]{"TheString", "IntPrimitive"};
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            SendEvent(epService, "E2", 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
    
            SendEvent(epService, "E4", 4);
            SendEvent(epService, "E4", 5);
            SendEvent(epService, "E5", 5);
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void TryAssertionTimeBatchAndUnique(EPServiceProvider epService, SupportUpdateListener listener, long startTime) {
            string[] fields = "TheString,IntPrimitive".Split(',');
            listener.Reset();
    
            SendEvent(epService, "E1", 1);
            SendEvent(epService, "E2", 2);
            SendEvent(epService, "E1", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 1000));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E2", 2}, new object[] {"E1", 3}});
            Assert.IsNull(listener.GetAndResetLastOldData());
    
            SendEvent(epService, "E3", 3);
            SendEvent(epService, "E3", 4);
            SendEvent(epService, "E3", 5);
            SendEvent(epService, "E4", 6);
            SendEvent(epService, "E3", 7);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 2000));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E4", 6}, new object[] {"E3", 7}});
            Assert.IsNull(listener.GetAndResetLastOldData());
        }
    
        private void TryAssertionLengthBatchAndFirstUnique(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "TheString,IntPrimitive".Split(',');
    
            SendEvent(epService, "E1", 1);
            SendEvent(epService, "E2", 2);
            SendEvent(epService, "E1", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E3", 4);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
            Assert.IsNull(listener.GetAndResetLastOldData());
    
            SendEvent(epService, "E1", 5);
            SendEvent(epService, "E4", 7);
            SendEvent(epService, "E1", 6);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E5", 9);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 5}, new object[] {"E4", 7}, new object[] {"E5", 9}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastOldData(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
            listener.Reset();
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, int intBoxed, double doublePrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private object[][] ToArr(params object[] values) {
            var arr = new Object[values.Length][];
            for (int i = 0; i < values.Length; i++) {
                arr[i] = new object[]{values[i]};
            }
            return arr;
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
