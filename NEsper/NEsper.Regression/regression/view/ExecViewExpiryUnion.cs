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

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewExpiryUnion : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
    
            RunAssertionFirstUniqueAndLengthOnDelete(epService);
            RunAssertionFirstUniqueAndFirstLength(epService);
            RunAssertionBatchWindow(epService);
            RunAssertionUnionAndDerivedValue(epService);
            RunAssertionUnionGroupBy(epService);
            RunAssertionUnionSubselect(epService);
            RunAssertionUnionThreeUnique(epService);
            RunAssertionUnionPattern(epService);
            RunAssertionUnionTwoUnique(epService);
            RunAssertionUnionSorted(epService);
            RunAssertionUnionTimeWin(epService);
            RunAssertionUnionTimeWinSODA(epService);
            RunAssertionUnionTimeWinNamedWindow(epService);
            RunAssertionUnionTimeWinNamedWindowDelete(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionFirstUniqueAndLengthOnDelete(EPServiceProvider epService) {
            EPStatement nwstmt = epService.EPAdministrator.CreateEPL("create window MyWindowOne#firstunique(TheString)#firstlength(3) retain-union as SupportBean");
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
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 99}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 99});
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 99}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], "TheString".Split(','), new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], "TheString".Split(','), new object[]{"E1"});
            listener.Reset();
    
            SendEvent(epService, "E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(nwstmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 3}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFirstUniqueAndFirstLength(EPServiceProvider epService) {
            string epl = "select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#firstunique(TheString) retain-union";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionFirstUniqueAndFirstLength(epService, listener, stmt);
    
            stmt.Dispose();
            listener.Reset();
    
            epl = "select irstream TheString, IntPrimitive from SupportBean#firstunique(TheString)#firstlength(3) retain-union";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            TryAssertionFirstUniqueAndFirstLength(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertionFirstUniqueAndFirstLength(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"TheString", "IntPrimitive"};
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            SendEvent(epService, "E1", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 2});
    
            SendEvent(epService, "E2", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1});
    
            SendEvent(epService, "E2", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}});
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
    
            SendEvent(epService, "E3", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}, new object[] {"E3", 3}});
            Assert.IsFalse(listener.GetAndClearIsInvoked());
        }
    
        private void RunAssertionBatchWindow(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#length_batch(3)#unique(IntPrimitive) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendEvent(epService, "E4", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            SendEvent(epService, "E6", 4);     // remove stream is E1, E2, E3
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6"});
    
            SendEvent(epService, "E7", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E7"});
    
            SendEvent(epService, "E8", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E5", "E4", "E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E8"});
    
            SendEvent(epService, "E9", 7);     // remove stream is E4, E5, E6; E4 and E5 get removed as their
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E6", "E7", "E8", "E9"));
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E9"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionAndDerivedValue(EPServiceProvider epService) {
            var fields = new string[]{"total"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#uni(DoublePrimitive) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(100d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100d});
    
            SendEvent(epService, "E2", 2, 20, 50d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(150d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{150d});
    
            SendEvent(epService, "E3", 1, 20, 20d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr(170d));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{170d});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionGroupBy(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            string text = "select irstream TheString from SupportBean#groupwin(IntPrimitive)#length(2)#unique(IntBoxed) retain-union";
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
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendEvent(epService, "E5", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            SendEvent(epService, "E6", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            SendEvent(epService, "E7", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E7"});
            listener.Reset();
    
            SendEvent(epService, "E8", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E8"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionSubselect(EPServiceProvider epService) {
            string text = "select * from SupportBean_S0 where p00 in (select TheString from SupportBean#length(2)#unique(IntPrimitive) retain-union)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1);
            SendEvent(epService, "E2", 2);
            SendEvent(epService, "E3", 3);
            SendEvent(epService, "E4", 2); // throws out E1
            SendEvent(epService, "E5", 1); // retains E3
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E3"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E4"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionThreeUnique(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#unique(DoublePrimitive) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 10, 200d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 2, 20, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendEvent(epService, "E4", 1, 30, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionPattern(EPServiceProvider epService) {
            var fields = new string[]{"string"};
    
            string text = "select irstream a.p00||b.p10 as string from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]#unique(a.id)#unique(b.id) retain-union";
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
    
        private void RunAssertionUnionTwoUnique(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E3"});
            listener.Reset();
    
            SendEvent(epService, "E4", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
            SendEvent(epService, "E5", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
    
            SendEvent(epService, "E6", 3, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E6"});
            listener.Reset();
    
            SendEvent(epService, "E7", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E7"});
    
            SendEvent(epService, "E8", 4, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E7", "E8"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[]{"E6"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E8"});
            listener.Reset();
    
            SendEvent(epService, "E9", 3, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E5", "E7", "E8", "E9"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E9"});
    
            SendEvent(epService, "E10", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E4", "E8", "E9", "E10"));
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E5"});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[]{"E7"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E10"});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionSorted(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#sort(2, IntPrimitive)#sort(2, IntBoxed) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            SendEvent(epService, "E2", 2, 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
    
            SendEvent(epService, "E3", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendEvent(epService, "E4", -1, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E3", "E4"));
            Assert.AreEqual(2, listener.LastOldData.Length);
            object[] result = {listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new string[]{"E1", "E2"});
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[]{"E4"});
            listener.Reset();
    
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
    
        private void RunAssertionUnionTimeWin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#unique(IntPrimitive)#time(10 sec) retain-union");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionTimeWinSODA(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream TheString from SupportBean#time(10 seconds)#unique(IntPrimitive) retain-union";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnionTimeWinNamedWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MyWindowTwo#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowTwo select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 delete from MyWindowTwo where IntBoxed = id");
            var listener = new SupportUpdateListener();
            stmtWindow.Events += listener.Update;
    
            TryAssertionTimeWinUnique(epService, listener, stmtWindow);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnionTimeWinNamedWindowDelete(EPServiceProvider epService) {
            SendTimer(epService, 0);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowThree#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean");
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
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3", "E4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
    
            SendTimer(epService, 4000);
            SendEvent(epService, "E5", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
            SendEvent(epService, "E6", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3", "E4", "E5", "E6"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(50));
            Assert.AreEqual(2, listener.LastOldData.Length);
            object[] result = {listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString")};
            EPAssertionUtil.AssertEqualsAnyOrder(result, new string[]{"E5", "E6"});
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E3", "E4"));
    
            SendTimer(epService, 12999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 13000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E4"));
    
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
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3"});
    
            SendTimer(epService, 4000);
            SendEvent(epService, "E4", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
            SendEvent(epService, "E5", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5"));
            SendEvent(epService, "E6", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E6"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6"));
    
            SendTimer(epService, 5000);
            SendEvent(epService, "E7", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E7"});
            SendEvent(epService, "E8", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E8"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8"));
    
            SendTimer(epService, 6000);
            SendEvent(epService, "E9", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E9"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));
    
            SendTimer(epService, 12999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 13000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E4", "E5", "E6", "E7", "E8", "E9"));
    
            SendTimer(epService, 14000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E5", "E6", "E7", "E8", "E9"));
    
            SendTimer(epService, 15000);
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"E7"});
            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[]{"E8"});
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E5", "E6", "E9"));
    
            SendTimer(epService, 1000000);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, ToArr("E2", "E5", "E6", "E9"));
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string text = null;
    
            text = "select TheString from SupportBean#groupwin(TheString)#unique(TheString)#merge(IntPrimitive) retain-union";
            TryInvalid(epService, text, "Error starting statement: Error attaching view to parent view: Groupwin view for this merge view could not be found among parent views [select TheString from SupportBean#groupwin(TheString)#unique(TheString)#merge(IntPrimitive) retain-union]");
    
            text = "select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union";
            TryInvalid(epService, text, "Error starting statement: Multiple groupwin views are not allowed in conjuntion with multiple data windows [select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union]");
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
