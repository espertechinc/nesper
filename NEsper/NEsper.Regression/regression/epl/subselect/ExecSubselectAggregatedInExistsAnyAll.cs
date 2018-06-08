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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectAggregatedInExistsAnyAll : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
    
            foreach (var clazz in Collections.List(typeof(SupportBean), typeof(SupportValueEvent), typeof(SupportIdAndValueEvent))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
    
            RunAssertionInSimple(epService);
            RunAssertionExistsSimple(epService);
    
            RunAssertionUngroupedWOHavingWRelOpAllAnySome(epService);
            RunAssertionUngroupedWOHavingWEqualsAllAnySome(epService);
            RunAssertionUngroupedWOHavingWIn(epService);
            RunAssertionUngroupedWOHavingWExists(epService);
    
            RunAssertionUngroupedWHavingWRelOpAllAnySome(epService);
            RunAssertionUngroupedWHavingWEqualsAllAnySome(epService);
            RunAssertionUngroupedWHavingWIn(epService);
            RunAssertionUngroupedWHavingWExists(epService);
    
            RunAssertionGroupedWOHavingWRelOpAllAnySome(epService);
            RunAssertionGroupedWOHavingWEqualsAllAnySome(epService);
            RunAssertionGroupedWOHavingWIn(epService);
            RunAssertionGroupedWOHavingWExists(epService);
    
            RunAssertionGroupedWHavingWRelOpAllAnySome(epService);
            RunAssertionGroupedWHavingWEqualsAllAnySome(epService);
            RunAssertionGroupedWHavingWIn(epService);
            RunAssertionGroupedWHavingWExists(epService);
        }
    
        private void RunAssertionUngroupedWHavingWIn(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select value in (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c0," +
                    "value not in (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c1 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", -1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWHavingWIn(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select value in (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) != 'E1') as c0," +
                    "value not in (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) != 'E1') as c1 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWOHavingWIn(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select value in (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0," +
                    "value not in (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
            SendVEAndAssert(epService, listener, fields, 11, new object[]{true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWOHavingWIn(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select value in (select sum(IntPrimitive) from SupportBean#keepall) as c0," +
                    "value not in (select sum(IntPrimitive) from SupportBean#keepall) as c1 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", -1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false});
    
            stmt.Dispose();
        }
    
    
        private void RunAssertionGroupedWOHavingWRelOpAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value < all (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0, " +
                    "value < any (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1, " +
                    "value < some (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWHavingWRelOpAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value < all (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c0, " +
                    "value < any (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c1, " +
                    "value < some (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 9));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWOHavingWEqualsAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value = all (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0, " +
                    "value = any (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1, " +
                    "value = some (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWOHavingWEqualsAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value = all (select sum(IntPrimitive) from SupportBean#keepall) as c0, " +
                    "value = any (select sum(IntPrimitive) from SupportBean#keepall) as c1, " +
                    "value = some (select sum(IntPrimitive) from SupportBean#keepall) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWHavingWEqualsAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value = all (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c0, " +
                    "value = any (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c1, " +
                    "value = some (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWHavingWEqualsAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value = all (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c0, " +
                    "value = any (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c1, " +
                    "value = some (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, false, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, true, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWHavingWExists(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select exists (select sum(IntPrimitive) from SupportBean having sum(IntPrimitive) < 15) as c0," +
                    "not exists (select sum(IntPrimitive) from SupportBean  having sum(IntPrimitive) < 15) as c1 from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendVEAndAssert(epService, listener, fields, new object[]{true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWOHavingWExists(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "select exists (select sum(IntPrimitive) from SupportBean) as c0," +
                    "not exists (select sum(IntPrimitive) from SupportBean) as c1 from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, new object[]{true, false});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendVEAndAssert(epService, listener, fields, new object[]{true, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWOHavingWExists(EPServiceProvider epService) {
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (key string, anint int)");
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyWindow(key, anint) select id, value from SupportIdAndValueEvent");
    
            string[] fields = "c0,c1".Split(',');
            string epl = "select exists (select sum(anint) from MyWindow group by key) as c0," +
                    "not exists (select sum(anint) from MyWindow group by key) as c1 from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E1", 19));
            SendVEAndAssert(epService, listener, fields, new object[]{true, false});
    
            epService.EPRuntime.ExecuteQuery("delete from MyWindow");
    
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            stmt.Dispose();
            stmtNamedWindow.Dispose();
            stmtInsert.Dispose();
        }
    
        private void RunAssertionGroupedWHavingWExists(EPServiceProvider epService) {
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (key string, anint int)");
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyWindow(key, anint) select id, value from SupportIdAndValueEvent");
    
            string[] fields = "c0,c1".Split(',');
            string epl = "select exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c0," +
                    "not exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c1 from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E1", 19));
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            epService.EPRuntime.SendEvent(new SupportIdAndValueEvent("E2", 12));
            SendVEAndAssert(epService, listener, fields, new object[]{true, false});
    
            epService.EPRuntime.ExecuteQuery("delete from MyWindow");
    
            SendVEAndAssert(epService, listener, fields, new object[]{false, true});
    
            stmt.Dispose();
            stmtNamedWindow.Dispose();
            stmtInsert.Dispose();
        }
    
        private void RunAssertionUngroupedWHavingWRelOpAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value < all (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c0, " +
                    "value < any (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c1, " +
                    "value < some (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 19));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 9));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", -1000));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUngroupedWOHavingWRelOpAllAnySome(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "value < all (select sum(IntPrimitive) from SupportBean#keepall) as c0, " +
                    "value < any (select sum(IntPrimitive) from SupportBean#keepall) as c1, " +
                    "value < some (select sum(IntPrimitive) from SupportBean#keepall) as c2 " +
                    "from SupportValueEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendVEAndAssert(epService, listener, fields, 10, new object[]{null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{true, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1000));
            SendVEAndAssert(epService, listener, fields, 10, new object[]{false, false, false});
    
            stmt.Dispose();
        }
    
        private void RunAssertionExistsSimple(EPServiceProvider epService) {
            string stmtText = "select id from S0 where exists (select max(id) from S1#length(3))";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0(epService, 1);
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("id"));
    
            SendEventS1(epService, 100);
            SendEventS0(epService, 2);
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionInSimple(EPServiceProvider epService) {
            string stmtText = "select id from S0 where id in (select max(id) from S1#length(2))";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEventS0(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventS1(epService, 100);
            SendEventS0(epService, 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventS0(epService, 100);
            Assert.AreEqual(100, listener.AssertOneGetNewAndReset().Get("id"));
    
            SendEventS0(epService, 200);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEventS1(epService, -1);
            SendEventS1(epService, -1);
            SendEventS0(epService, -1);
            Assert.AreEqual(-1, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void SendVEAndAssert(EPServiceProvider epService, SupportUpdateListener listener, string[] fields, int value, object[] expected) {
            epService.EPRuntime.SendEvent(new SupportValueEvent(value));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
        }
    
        private void SendVEAndAssert(EPServiceProvider epService, SupportUpdateListener listener, string[] fields, object[] expected) {
            epService.EPRuntime.SendEvent(new SupportValueEvent(-1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
        }
    
        private void SendEventS0(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(id));
        }
    
        private void SendEventS1(EPServiceProvider epService, int id) {
            epService.EPRuntime.SendEvent(new SupportBean_S1(id));
        }
    }
} // end of namespace
