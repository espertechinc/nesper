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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeOrderAndTimeToLive : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
    
            RunAssertionTimeToLive(epService);
            RunAssertionMonthScoped(epService);
            RunAssertionTimeOrderRemoveStream(epService);
            RunAssertionTimeOrder(epService);
            RunAssertionGroupedWindow(epService);
            RunAssertionInvalid(epService);
            RunAssertionPreviousAndPrior(epService);
        }
    
        private void RunAssertionTimeToLive(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            string[] fields = "id".Split(',');
            string epl = "select irstream * from SupportBeanTimestamp#timetolive(timestamp)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 1000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E2", 500);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E1"}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(499));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(500));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E3", 200);
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new object[]{"E3"}, new object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E4", 1200);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E4"}});
    
            SendEvent(epService, "E5", 1000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E4"}, new object[] {"E5"}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(999));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetDataListsFlattened(), fields, null, new object[][]{new object[] {"E1"}, new object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E4"}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1199));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1200));
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), fields, new object[]{"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E6", 1200);
            EPAssertionUtil.AssertProps(listener.AssertPairGetIRAndReset(), fields, new object[]{"E6"}, new object[]{"E6"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select rstream * from SupportBeanTimestamp#time_order(timestamp, 1 month)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", DateTimeParser.ParseDefaultMSec("2002-02-01T09:00:00.000"));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "id".Split(','), new object[][]{new object[] {"E1"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeOrderRemoveStream(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            epService.EPAdministrator.CreateEPL(
                    "insert rstream into OrderedStream select rstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(
                    "select * from OrderedStream");
            var listener = new SupportUpdateListener();
            stmtTwo.Events += listener.Update;
    
            // 1st event at 21 sec
            SendTimer(epService, 21000);
            SendEvent(epService, "E1", 21000);
    
            // 2nd event at 22 sec
            SendTimer(epService, 22000);
            SendEvent(epService, "E2", 22000);
    
            // 3nd event at 28 sec
            SendTimer(epService, 28000);
            SendEvent(epService, "E3", 28000);
    
            // 4th event at 30 sec, however is 27 sec (old 3 sec)
            SendTimer(epService, 30000);
            SendEvent(epService, "E4", 27000);
    
            // 5th event at 30 sec, however is 22 sec (old 8 sec)
            SendEvent(epService, "E5", 22000);
    
            // flush one
            SendTimer(epService, 30999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 31000);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E1", listener.LastNewData[0].Get("id"));
            listener.Reset();
    
            // 6th event at 31 sec, however is 21 sec (old 10 sec)
            SendEvent(epService, "E6", 21000);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E6", listener.LastNewData[0].Get("id"));
            listener.Reset();
    
            // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
            SendEvent(epService, "E7", 21300);
    
            // flush one
            SendTimer(epService, 31299);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 31300);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E7", listener.LastNewData[0].Get("id"));
            listener.Reset();
    
            // flush two
            SendTimer(epService, 31999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 32000);
    
            EventBean[] result = listener.GetNewDataListFlattened();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("E2", result[0].Get("id"));
            Assert.AreEqual("E5", result[1].Get("id"));
            listener.Reset();
    
            // flush one
            SendTimer(epService, 36999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 37000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E4", listener.LastNewData[0].Get("id"));
            listener.Reset();
    
            // rather old event
            SendEvent(epService, "E8", 21000);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E8", listener.LastNewData[0].Get("id"));
            listener.Reset();
    
            // 9-second old event for posting at 38 sec
            SendEvent(epService, "E9", 28000);
    
            // flush two
            SendTimer(epService, 37999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 38000);
            result = listener.GetNewDataListFlattened();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("E3", result[0].Get("id"));
            Assert.AreEqual("E9", result[1].Get("id"));
            listener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeOrder(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(epService, 21000);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // 1st event at 21 sec
            SendEvent(epService, "E1", 21000);
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}});
    
            // 2nd event at 22 sec
            SendTimer(epService, 22000);
            SendEvent(epService, "E2", 22000);
            Assert.AreEqual("E2", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            // 3nd event at 28 sec
            SendTimer(epService, 28000);
            SendEvent(epService, "E3", 28000);
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            // 4th event at 30 sec, however is 27 sec (old 3 sec)
            SendTimer(epService, 30000);
            SendEvent(epService, "E4", 27000);
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 5th event at 30 sec, however is 22 sec (old 8 sec)
            SendEvent(epService, "E5", 22000);
            Assert.AreEqual("E5", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(epService, 30999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 31000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E1", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 6th event at 31 sec, however is 21 sec (old 10 sec)
            SendEvent(epService, "E6", 21000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E6", listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E6", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
            SendEvent(epService, "E7", 21300);
            Assert.AreEqual("E7", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E7"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(epService, 31299);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 31300);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E7", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush two
            SendTimer(epService, 31999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 32000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            Assert.AreEqual("E2", listener.LastOldData[0].Get("id"));
            Assert.AreEqual("E5", listener.LastOldData[1].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(epService, 36999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 37000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E4", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}});
    
            // rather old event
            SendEvent(epService, "E8", 21000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E8", listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E8", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}});
    
            // 9-second old event for posting at 38 sec
            SendEvent(epService, "E9", 28000);
            Assert.AreEqual("E9", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}, new object[] {"E9"}});
    
            // flush two
            SendTimer(epService, 37999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 38000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            Assert.AreEqual("E3", listener.LastOldData[0].Get("id"));
            Assert.AreEqual("E9", listener.LastOldData[1].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // new event
            SendEvent(epService, "E10", 38000);
            Assert.AreEqual("E10", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E10"}});
    
            // flush last
            SendTimer(epService, 47999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 48000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E10", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // last, in the future
            SendEvent(epService, "E11", 70000);
            Assert.AreEqual("E11", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E11"}});
    
            SendTimer(epService, 80000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E11", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(epService, 100000);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupedWindow(EPServiceProvider epService) {
            SendTimer(epService, 20000);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#groupwin(groupId)#time_order(timestamp, 10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // 1st event is old
            SendEvent(epService, "E1", "G1", 10000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual("E1", listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E1", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // 2nd just fits
            SendEvent(epService, "E2", "G2", 10001);
            Assert.AreEqual("E2", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}});
    
            SendEvent(epService, "E3", "G3", 20000);
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendEvent(epService, "E4", "G2", 20000);
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(epService, 20001);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E2", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(epService, 22000);
            SendEvent(epService, "E5", "G2", 19000);
            Assert.AreEqual("E5", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(epService, 29000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual("E5", listener.LastOldData[0].Get("id"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(epService, 30000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.LastOldData, "id".Split(','), new object[][]{new object[] {"E4"}, new object[] {"E3"}});
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(epService, 100000);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBeanTimestamp).FullName + "#time_order(bump, 10 sec)",
                    "Error starting statement: Error attaching view to event stream: Invalid parameter expression 0 for Time-Order view: Failed to validate view parameter expression 'bump': Property named 'bump' is not valid in any stream [");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBeanTimestamp).FullName + "#time_order(10 sec)",
                    "Error starting statement: Error attaching view to event stream: Time-Order view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size [");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBeanTimestamp).FullName + "#time_order(timestamp, abc)",
                    "Error starting statement: Error attaching view to event stream: Invalid parameter expression 1 for Time-Order view: Failed to validate view parameter expression 'abc': Property named 'abc' is not valid in any stream (did you mean 'Id'?) [");
        }
    
        private void RunAssertionPreviousAndPrior(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream id, " +
                            " prev(0, id) as prevIdZero, " +
                            " prev(1, id) as prevIdOne, " +
                            " prior(1, id) as priorIdOne," +
                            " prevtail(0, id) as prevTailIdZero, " +
                            " prevtail(1, id) as prevTailIdOne, " +
                            " prevcount(id) as prevCountId, " +
                            " prevwindow(id) as prevWindowId " +
                            " from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
            var fields = new string[]{"id", "prevIdZero", "prevIdOne", "priorIdOne", "prevTailIdZero", "prevTailIdOne", "prevCountId"};
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 20000);
            SendEvent(epService, "E1", 25000);
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E2", 21000);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E2", theEvent.Get("id"));
            Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
            Assert.AreEqual("E1", theEvent.Get("prevIdOne"));
            Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
            Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
            Assert.AreEqual("E2", theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(2L, theEvent.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("prevWindowId"), new object[]{"E2", "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E2", "E1", "E1", "E1", "E2", 2L}, new object[] {"E1", "E2", "E1", null, "E1", "E2", 2L}});
    
            SendEvent(epService, "E3", 22000);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E3", theEvent.Get("id"));
            Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
            Assert.AreEqual("E3", theEvent.Get("prevIdOne"));
            Assert.AreEqual("E2", theEvent.Get("priorIdOne"));
            Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
            Assert.AreEqual("E3", theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(3L, theEvent.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("prevWindowId"), new object[]{"E2", "E3", "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E2", "E3", "E1", "E1", "E3", 3L}, new object[] {"E3", "E2", "E3", "E2", "E1", "E3", 3L}, new object[] {"E1", "E2", "E3", null, "E1", "E3", 3L}});
    
            SendTimer(epService, 31000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            theEvent = listener.LastOldData[0];
            Assert.AreEqual("E2", theEvent.Get("id"));
            Assert.AreEqual(null, theEvent.Get("prevIdZero"));
            Assert.AreEqual(null, theEvent.Get("prevIdOne"));
            Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
            Assert.AreEqual(null, theEvent.Get("prevTailIdZero"));
            Assert.AreEqual(null, theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(null, theEvent.Get("prevCountId"));
            Assert.AreEqual(null, theEvent.Get("prevWindowId"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3", "E3", "E1", "E2", "E1", "E3", 2L}, new object[] {"E1", "E3", "E1", null, "E1", "E3", 2L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBeanTimestamp SendEvent(EPServiceProvider epService, string id, string groupId, long timestamp) {
            var theEvent = new SupportBeanTimestamp(id, groupId, timestamp);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBeanTimestamp SendEvent(EPServiceProvider epService, string id, long timestamp) {
            var theEvent = new SupportBeanTimestamp(id, timestamp);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
