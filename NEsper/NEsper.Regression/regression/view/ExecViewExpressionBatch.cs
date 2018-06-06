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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewExpressionBatch : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionNewestEventOldestEvent(epService);
            RunAssertionLengthBatch(epService);
            RunAssertionTimeBatch(epService);
            RunAssertionVariableBatch(epService);
            RunAssertionDynamicTimeBatch(epService);
            RunAssertionUDFBuiltin(epService);
            RunAssertionInvalid(epService);
            RunAssertionNamedWindowDelete(epService);
            RunAssertionPrev(epService);
            RunAssertionEventPropBatch(epService);
            RunAssertionAggregation(epService);
        }
    
        private void RunAssertionNewestEventOldestEvent(EPServiceProvider epService) {
    
            // try with include-trigger-event
            var fields = new string[]{"TheString"};
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, false)");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1"}, new object[] {"E2"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E3"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}}, new object[][] {new object[] {"E3"}});
            stmtOne.Dispose();
    
            // try with include-trigger-event
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, true)");
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            stmtTwo.Dispose();
        }
    
        private void RunAssertionLengthBatch(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(current_count >= 3, true)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastOldData(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E7"}, new object[] {"E8"}, new object[] {"E9"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastOldData(), fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatch(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            var fields = new string[]{"TheString"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > 2000)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3100));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5100));
            epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5101));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastOldData(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionVariableBatch(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create variable bool POST = false");
    
            var fields = new string[]{"TheString"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(POST)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("POST", true);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1001));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E2"}}, new object[][] {new object[] {"E1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E3"}}, new object[][] {new object[] {"E2"}});
    
            epService.EPRuntime.SetVariableValue("POST", false);
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("POST", true);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2001));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}}, new object[][] {new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E6"}}, new object[][] {new object[] {"E4"}, new object[] {"E5"}});
    
            stmt.Stop();
        }
    
        private void RunAssertionDynamicTimeBatch(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.CreateEPL("create variable long SIZE = 1000");
    
            var fields = new string[]{"TheString"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > SIZE)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1900));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("SIZE", 500);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1901));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2300));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2500));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3100));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SetVariableValue("SIZE", 999);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3700));
            epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4100));
            epService.EPRuntime.SendEvent(new SupportBean("E8", 0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}}, new object[][] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUDFBuiltin(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("udf", typeof(ExecViewExpressionWindow.LocalUDF), "EvaluateExpiryUDF");
            epService.EPAdministrator.CreateEPL("select * from SupportBean#expr_batch(udf(TheString, view_reference, expired_count))");
    
            ExecViewExpressionWindow.LocalUDF.Result = true;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.AreEqual("E1", ExecViewExpressionWindow.LocalUDF.Key);
            Assert.AreEqual(0, (int) ExecViewExpressionWindow.LocalUDF.ExpiryCount);
            Assert.IsNotNull(ExecViewExpressionWindow.LocalUDF.Viewref);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            ExecViewExpressionWindow.LocalUDF.Result = false;
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.AreEqual("E3", ExecViewExpressionWindow.LocalUDF.Key);
            Assert.AreEqual(0, (int) ExecViewExpressionWindow.LocalUDF.ExpiryCount);
            Assert.IsNotNull(ExecViewExpressionWindow.LocalUDF.Viewref);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            TryInvalid(
                epService, "select * from SupportBean#expr_batch(1)",
                string.Format("Error starting statement: Error attaching view to event stream: Invalid return value for expiry expression, expected a bool return value but received {0} [select * from SupportBean#expr_batch(1)]",
                    typeof(int).GetCleanName()));

            TryInvalid(
                epService, "select * from SupportBean#expr_batch((select * from SupportBean#lastevent))",
                "Error starting statement: Error attaching view to event stream: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean#expr_batch((select * from SupportBean#lastevent))]");
        }
    
        private void RunAssertionNamedWindowDelete(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            var fields = new string[]{"TheString"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window NW#expr_batch(current_count > 3) as SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}}, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPrev(EPServiceProvider epService) {
            var fields = new string[]{"val0"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select prev(1, TheString) as val0 from SupportBean#expr_batch(current_count > 2)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {null}, new object[] {"E1"}, new object[] {"E2"}}, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionEventPropBatch(EPServiceProvider epService) {
            var fields = new string[]{"val0"};
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream TheString as val0 from SupportBean#expr_batch(IntPrimitive > 0)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E2"}}, new object[][] {new object[] {"E1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", -1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E3"}, new object[] {"E4"}}, new object[][] {new object[] {"E2"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregation(EPServiceProvider epService) {
            var fields = new string[]{"TheString"};
    
            // Test un-grouped
            EPStatement stmtUngrouped = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#expr_batch(sum(IntPrimitive) > 100)");
            var listener = new SupportUpdateListener();
            stmtUngrouped.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 90));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 101));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E4"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 99));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E7", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}}, new object[][] {new object[] {"E4"}});
            stmtUngrouped.Dispose();
    
            // Test grouped
            EPStatement stmtGrouped = epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean#groupwin(IntPrimitive)#expr_batch(sum(LongPrimitive) > 100)");
            stmtGrouped.Events += listener.Update;
    
            SendEvent(epService, "E1", 1, 10);
            SendEvent(epService, "E2", 2, 10);
            SendEvent(epService, "E3", 1, 90);
            SendEvent(epService, "E4", 2, 80);
            SendEvent(epService, "E5", 2, 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E6", 2, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E2"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}}, null);
    
            SendEvent(epService, "E7", 2, 50);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E8", 1, 2);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}, new object[] {"E3"}, new object[] {"E8"}}, null);
    
            SendEvent(epService, "E9", 2, 50);
            SendEvent(epService, "E10", 1, 101);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E10"}}, new object[][] {new object[] {"E1"}, new object[] {"E3"}, new object[] {"E8"}});
    
            SendEvent(epService, "E11", 2, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E7"}, new object[] {"E9"}, new object[] {"E11"}}, new object[][] {new object[] {"E2"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
    
            SendEvent(epService, "E12", 1, 102);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E12"}}, new object[][] {new object[] {"E10"}});
            stmtGrouped.Dispose();
    
            // Test on-delete
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window NW#expr_batch(sum(IntPrimitive) >= 10) as SupportBean");
            stmt.Events += listener.Update;
            epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 8));
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields, new object[][]{new object[] {"E1"}, new object[] {"E3"}, new object[] {"E4"}}, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
