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


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeLengthBatch : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var events = new SupportMarketDataBean[100];
            for (int i = 0; i < events.Length; i++) {
                events[i] = new SupportMarketDataBean("S" + Convert.ToString(i), "id_" + Convert.ToString(i), i);
            }
    
            RunAssertionTimeLengthBatch(epService, events);
            RunAssertionTimeLengthBatchForceOutput(epService, events);
            RunAssertionTimeLengthBatchForceOutputSum(epService, events);
            RunAssertionForceOutputStartEagerSum(epService, events);
            RunAssertionForceOutputStartNoEagerSum(epService);
            RunAssertionPreviousAndPrior(epService, events);
            RunAssertionGroupBySumStartEager(epService);
        }
    
        private void RunAssertionTimeLengthBatch(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // Send 3 events in batch
            engine.SendEvent(events[0]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[1]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[2]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send another 3 events in batch
            engine.SendEvent(events[3]);
            engine.SendEvent(events[4]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[5]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[3], events[4], events[5]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Expire the last 3 events by moving time
            SendTimer(epService, startTime + 9999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 10000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[3], events[4], events[5]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 10001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send an event, let the timer send the batch
            SendTimer(epService, startTime + 10100);
            engine.SendEvent(events[6]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 19999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 20000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[6]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 20001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send two events, let the timer send the batch
            SendTimer(epService, startTime + 29998);
            engine.SendEvent(events[7]);
            engine.SendEvent(events[8]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 29999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 30000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[6]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[7], events[8]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send three events, the the 3 events batch
            SendTimer(epService, startTime + 30001);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[9]);
            engine.SendEvent(events[10]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[11]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[7], events[8]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[9], events[10], events[11]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send 1 event, let the timer to do the batch
            SendTimer(epService, startTime + 39000 + 9999);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[12]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 10000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[9], events[10], events[11]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[12]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 10001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send no events, let the timer to do the batch
            SendTimer(epService, startTime + 39000 + 19999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 20000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[12]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 20001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send no events, let the timer to do NO batch
            SendTimer(epService, startTime + 39000 + 29999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 30000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 30001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send 1 more event
            SendTimer(epService, startTime + 90000);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[13]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 99999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 100000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[13]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeLengthBatchForceOutput(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'FORCE_UPDATE')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // Send 3 events in batch
            engine.SendEvent(events[0]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[1]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[2]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send another 3 events in batch
            engine.SendEvent(events[3]);
            engine.SendEvent(events[4]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[5]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[3], events[4], events[5]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Expire the last 3 events by moving time
            SendTimer(epService, startTime + 9999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 10000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[3], events[4], events[5]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 10001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send an event, let the timer send the batch
            SendTimer(epService, startTime + 10100);
            engine.SendEvent(events[6]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 19999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 20000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[6]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 20001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send two events, let the timer send the batch
            SendTimer(epService, startTime + 29998);
            engine.SendEvent(events[7]);
            engine.SendEvent(events[8]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 29999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 30000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[6]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[7], events[8]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send three events, the the 3 events batch
            SendTimer(epService, startTime + 30001);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[9]);
            engine.SendEvent(events[10]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[11]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[7], events[8]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[9], events[10], events[11]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send 1 event, let the timer to do the batch
            SendTimer(epService, startTime + 39000 + 9999);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[12]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 10000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[9], events[10], events[11]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[12]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 10001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send no events, let the timer to do the batch
            SendTimer(epService, startTime + 39000 + 19999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 20000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[12]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 20001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send no events, let the timer do a batch
            SendTimer(epService, startTime + 39000 + 29999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 30000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 30001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send no events, let the timer do a batch
            SendTimer(epService, startTime + 39000 + 39999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 39000 + 40000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000 + 40001);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send 1 more event
            SendTimer(epService, startTime + 80000);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(events[13]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 88999);   // 10 sec from last batch
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 89000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[13]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send 3 more events
            SendTimer(epService, startTime + 90000);
            engine.SendEvent(events[14]);
            engine.SendEvent(events[15]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 92000);
            engine.SendEvent(events[16]);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[13]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[14], events[15], events[16]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            // Send no events, let the timer do a batch
            SendTimer(epService, startTime + 101999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 102000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[14], events[15], events[16]}, listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeLengthBatchForceOutputSum(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select sum(price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'FORCE_UPDATE')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // Send 1 events in batch
            engine.SendEvent(events[10]);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 10000);
            Assert.AreEqual(10.0, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            SendTimer(epService, startTime + 20000);
            Assert.AreEqual(null, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            SendTimer(epService, startTime + 30000);
            Assert.AreEqual(null, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            SendTimer(epService, startTime + 40000);
            Assert.AreEqual(null, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionForceOutputStartEagerSum(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select sum(price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'force_update, start_eager')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 9999);
            Assert.IsFalse(listener.IsInvoked);
    
            // Send batch off
            SendTimer(epService, startTime + 10000);
            Assert.AreEqual(null, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            // Send batch off
            SendTimer(epService, startTime + 20000);
            Assert.AreEqual(null, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            engine.SendEvent(events[11]);
            engine.SendEvent(events[12]);
            SendTimer(epService, startTime + 30000);
            Assert.AreEqual(23.0, listener.LastNewData[0].Get("sum(price)"));
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionForceOutputStartNoEagerSum(EPServiceProvider epService) {
            long startTime = 1000;
            SendTimer(epService, startTime);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select sum(price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'force_update')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // No batch as we are not start eager
            SendTimer(epService, startTime + 10000);
            Assert.IsFalse(listener.IsInvoked);
    
            // No batch as we are not start eager
            SendTimer(epService, startTime + 20000);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousAndPrior(EPServiceProvider epService, SupportMarketDataBean[] premades) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select price, prev(1, price) as prevPrice, prior(1, price) as priorPrice from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // Send 3 events in batch
            engine.SendEvent(premades[0]);
            engine.SendEvent(premades[1]);
            Assert.IsFalse(listener.IsInvoked);
    
            engine.SendEvent(premades[2]);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EventBean[] events = listener.LastNewData;
            AssertData(events[0], 0, null, null);
            AssertData(events[1], 1.0, 0.0, 0.0);
            AssertData(events[2], 2.0, 1.0, 1.0);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupBySumStartEager(EPServiceProvider epService) {
            long startTime = 1000;
            SendTimer(epService, startTime);
    
            EPRuntime engine = epService.EPRuntime;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select symbol, sum(price) as s from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(5, 10, \"START_EAGER\") group by symbol order by symbol asc");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, startTime + 4000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 6000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EventBean[] events = listener.LastNewData;
            Assert.IsNull(events);
            listener.Reset();
    
            SendTimer(epService, startTime + 7000);
            engine.SendEvent(new SupportMarketDataBean("S1", "e1", 10d));
    
            SendTimer(epService, startTime + 8000);
            engine.SendEvent(new SupportMarketDataBean("S2", "e2", 77d));
    
            SendTimer(epService, startTime + 9000);
            engine.SendEvent(new SupportMarketDataBean("S1", "e3", 1d));
    
            SendTimer(epService, startTime + 10000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 11000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            events = listener.LastNewData;
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("S1", events[0].Get("symbol"));
            Assert.AreEqual(11d, events[0].Get("s"));
            Assert.AreEqual("S2", events[1].Get("symbol"));
            Assert.AreEqual(77d, events[1].Get("s"));
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void AssertData(EventBean theEvent, double price, double? prevPrice, double? priorPrice) {
            Assert.AreEqual(price, theEvent.Get("price"));
            Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
            Assert.AreEqual(priorPrice, theEvent.Get("priorPrice"));
        }
    }
} // end of namespace
