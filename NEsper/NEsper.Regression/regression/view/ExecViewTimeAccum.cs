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


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeAccum : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            var events = new SupportMarketDataBean[100];
            for (int i = 0; i < events.Length; i++) {
                int group = i % 10;
                events[i] = new SupportMarketDataBean("S" + Convert.ToString(group), "id_" + Convert.ToString(i), i);
            }
    
            RunAssertionMonthScoped(epService);
            RunAssertionTimeAccum(epService, events);
            RunAssertionTimeAccumRStream(epService, events);
            RunAssertionPreviousAndPrior(epService, events);
            RunAssertionSum(epService, events);
            RunAssertionGroupedWindow(epService, events);
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select rstream * from SupportBean#time_accum(1 month)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "TheString".Split(','), new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeAccum(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_accum(10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            SendTimer(epService, startTime + 10000);
            Assert.IsFalse(listener.IsInvoked);
    
            // 1st at 10 sec
            engine.SendEvent(events[0]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[0]);
    
            // 2nd event at 14 sec
            SendTimer(epService, startTime + 14000);
            engine.SendEvent(events[1]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[1]);
    
            // 3nd event at 14 sec
            SendTimer(epService, startTime + 14000);
            engine.SendEvent(events[2]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[2]);
    
            // 3rd event at 23 sec
            SendTimer(epService, startTime + 23000);
            engine.SendEvent(events[3]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[3]);
    
            // no event till 33 sec
            SendTimer(epService, startTime + 32999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 33000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(4, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2], events[3]}, listener.GetOldDataListFlattened());
            listener.Reset();
    
            // no events till 50 sec
            SendTimer(epService, startTime + 50000);
            Assert.IsFalse(listener.IsInvoked);
    
            // next two events at 55 sec
            SendTimer(epService, startTime + 55000);
            engine.SendEvent(events[4]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[4]);
            engine.SendEvent(events[5]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[5]);
    
            // no event till 65 sec
            SendTimer(epService, startTime + 64999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 65000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[4], events[5]}, listener.GetOldDataListFlattened());
            listener.Reset();
    
            // next window
            engine.SendEvent(events[6]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[6]);
    
            SendTimer(epService, startTime + 74999);
            engine.SendEvent(events[7]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[7]);
    
            SendTimer(epService, startTime + 74999 + 10000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[6], events[7]}, listener.GetOldDataListFlattened());
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeAccumRStream(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select rstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_accum(10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            SendTimer(epService, startTime + 10000);
            Assert.IsFalse(listener.IsInvoked);
    
            // some events at 10 sec
            engine.SendEvent(events[0]);
            engine.SendEvent(events[1]);
            engine.SendEvent(events[2]);
            Assert.IsFalse(listener.IsInvoked);
    
            // flush out of the window
            SendTimer(epService, startTime + 20000);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[0], events[1], events[2]}, listener.GetNewDataListFlattened());
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousAndPrior(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream price, prev(1, price) as prevPrice, prior(1, price) as priorPrice from " + typeof(SupportMarketDataBean).FullName +
                            "#time_accum(10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // 1st event
            SendTimer(epService, startTime + 20000);
            engine.SendEvent(events[5]);
            AssertData(listener.AssertOneGetNewAndReset(), 5d, null, null);
    
            // 2nd event
            SendTimer(epService, startTime + 25000);
            engine.SendEvent(events[6]);
            AssertData(listener.AssertOneGetNewAndReset(), 6d, 5d, 5d);
    
            // 3nd event
            SendTimer(epService, startTime + 34000);
            engine.SendEvent(events[7]);
            AssertData(listener.AssertOneGetNewAndReset(), 7d, 6d, 6d);
    
            SendTimer(epService, startTime + 43999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 44000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(3, listener.LastOldData.Length);
            AssertData(listener.LastOldData[0], 5d, null, null);
            AssertData(listener.LastOldData[1], 6d, null, 5d);
            AssertData(listener.LastOldData[2], 7d, null, 6d);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionSum(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream sum(price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
                            "#time_accum(10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // 1st event
            SendTimer(epService, startTime + 20000);
            engine.SendEvent(events[5]);
            AssertData(listener.LastNewData[0], 5d);
            AssertData(listener.LastOldData[0], null);
            listener.Reset();
    
            // 2nd event
            SendTimer(epService, startTime + 25000);
            engine.SendEvent(events[6]);
            AssertData(listener.LastNewData[0], 11d);
            AssertData(listener.LastOldData[0], 5d);
            listener.Reset();
    
            SendTimer(epService, startTime + 34999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, startTime + 35000);
            AssertData(listener.LastNewData[0], null);
            AssertData(listener.LastOldData[0], 11d);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedWindow(EPServiceProvider epService, SupportMarketDataBean[] events) {
            long startTime = 1000;
            SendTimer(epService, startTime);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#groupwin(symbol)#time_accum(10 sec)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPRuntime engine = epService.EPRuntime;
    
            // 1st S1 event
            SendTimer(epService, startTime + 10000);
            engine.SendEvent(events[1]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[1]);
    
            // 1st S2 event
            SendTimer(epService, startTime + 12000);
            engine.SendEvent(events[2]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[2]);
    
            // 2nd S1 event
            SendTimer(epService, startTime + 15000);
            engine.SendEvent(events[11]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[11]);
    
            // 2nd S2 event
            SendTimer(epService, startTime + 18000);
            engine.SendEvent(events[12]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[12]);
    
            // 3rd S1 event
            SendTimer(epService, startTime + 21000);
            engine.SendEvent(events[21]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[21]);
    
            SendTimer(epService, startTime + 28000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(2, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[2], events[12]}, listener.GetOldDataListFlattened());
            listener.Reset();
    
            // 3rd S2 event
            SendTimer(epService, startTime + 29000);
            engine.SendEvent(events[32]);
            Assert.AreSame(listener.AssertOneGetNewAndReset().Underlying, events[32]);
    
            SendTimer(epService, startTime + 31000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(3, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[1], events[11], events[21]}, listener.GetOldDataListFlattened());
            listener.Reset();
    
            SendTimer(epService, startTime + 39000);
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(1, listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{events[32]}, listener.GetOldDataListFlattened());
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
    
        private void AssertData(EventBean theEvent, double? sumPrice) {
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
