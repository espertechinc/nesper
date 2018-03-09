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

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeWTimeBatch : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionTimeBatchRowForAllNoJoin(epService);
            RunAssertionTimeBatchRowForAllJoin(epService);
            RunAssertionTimeBatchAggregateAllNoJoin(epService);
            RunAssertionTimeBatchAggregateAllJoin(epService);
            RunAssertionTimeBatchRowPerGroupNoJoin(epService);
            RunAssertionTimeBatchRowPerGroupJoin(epService);
            RunAssertionTimeBatchAggrGroupedNoJoin(epService);
            RunAssertionTimeBatchAggrGroupedJoin(epService);
        }
    
        private void RunAssertionTimeBatchRowForAllNoJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream sum(price) as sumPrice from MarketData#time_batch(1 sec)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], 45d);
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], 20d);
    
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(1, oldEvents.Length);
            AssertEvent(oldEvents[0], 45d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchRowForAllJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream sum(price) as sumPrice from MarketData#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.symbol = S1.TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportEvent(epService, "DELL");
            SendSupportEvent(epService, "IBM");
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], 45d);
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], 20d);
    
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(1, oldEvents.Length);
            AssertEvent(oldEvents[0], 45d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchAggregateAllNoJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice from MarketData#time_batch(1 sec)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", 45d);
            AssertEvent(newEvents[1], "IBM", 45d);
            AssertEvent(newEvents[2], "DELL", 45d);
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], "IBM", 20d);
    
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(3, oldEvents.Length);
            AssertEvent(oldEvents[0], "DELL", 20d);
            AssertEvent(oldEvents[1], "IBM", 20d);
            AssertEvent(oldEvents[2], "DELL", 20d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchAggregateAllJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice from MarketData#time_batch(1 sec) as S0, SupportBean#keepall as S1 where S0.symbol = S1.TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportEvent(epService, "DELL");
            SendSupportEvent(epService, "IBM");
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", 45d);
            AssertEvent(newEvents[1], "IBM", 45d);
            AssertEvent(newEvents[2], "DELL", 45d);
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], "IBM", 20d);
    
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(3, oldEvents.Length);
            AssertEvent(oldEvents[0], "DELL", 20d);
            AssertEvent(oldEvents[1], "IBM", 20d);
            AssertEvent(oldEvents[2], "DELL", 20d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchRowPerGroupNoJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice from MarketData#time_batch(1 sec) group by symbol order by symbol asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(2, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", 30d);
            AssertEvent(newEvents[1], "IBM", 15d);
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            newEvents = listener.LastNewData;
            Assert.AreEqual(2, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", null);
            AssertEvent(newEvents[1], "IBM", 20d);
    
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(2, oldEvents.Length);
            AssertEvent(oldEvents[0], "DELL", 30d);
            AssertEvent(oldEvents[1], "IBM", 15d);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchRowPerGroupJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice " +
                    " from MarketData#time_batch(1 sec) as S0, SupportBean#keepall as S1" +
                    " where S0.symbol = S1.TheString " +
                    " group by symbol";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportEvent(epService, "DELL");
            SendSupportEvent(epService, "IBM");
    
            // send first batch
            SendMDEvent(epService, "DELL", 10, 0L);
            SendMDEvent(epService, "IBM", 15, 0L);
            SendMDEvent(epService, "DELL", 20, 0L);
            SendTimer(epService, 1000);
    
            string[] fields = "symbol,sumPrice".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"DELL", 30d}, new object[] {"IBM", 15d}});
    
            // send second batch
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.LastNewData, fields, new object[][]{new object[] {"DELL", null}, new object[] {"IBM", 20d}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastOldData(), fields, new object[][]{new object[] {"DELL", 30d}, new object[] {"IBM", 15d}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchAggrGroupedNoJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice, volume from MarketData#time_batch(1 sec) group by symbol";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMDEvent(epService, "DELL", 10, 200L);
            SendMDEvent(epService, "IBM", 15, 500L);
            SendMDEvent(epService, "DELL", 20, 250L);
    
            SendTimer(epService, 1000);
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", 30d, 200L);
            AssertEvent(newEvents[1], "IBM", 15d, 500L);
            AssertEvent(newEvents[2], "DELL", 30d, 250L);
    
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], "IBM", 20d, 600L);
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(3, oldEvents.Length);
            AssertEvent(oldEvents[0], "DELL", null, 200L);
            AssertEvent(oldEvents[1], "IBM", 20d, 500L);
            AssertEvent(oldEvents[2], "DELL", null, 250L);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchAggrGroupedJoin(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select irstream symbol, sum(price) as sumPrice, volume " +
                    "from MarketData#time_batch(1 sec) as S0, SupportBean#keepall as S1" +
                    " where S0.symbol = S1.TheString " +
                    " group by symbol";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportEvent(epService, "DELL");
            SendSupportEvent(epService, "IBM");
    
            SendMDEvent(epService, "DELL", 10, 200L);
            SendMDEvent(epService, "IBM", 15, 500L);
            SendMDEvent(epService, "DELL", 20, 250L);
    
            SendTimer(epService, 1000);
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEvent(newEvents[0], "DELL", 30d, 200L);
            AssertEvent(newEvents[1], "IBM", 15d, 500L);
            AssertEvent(newEvents[2], "DELL", 30d, 250L);
    
            SendMDEvent(epService, "IBM", 20, 600L);
            SendTimer(epService, 2000);
            newEvents = listener.LastNewData;
            Assert.AreEqual(1, newEvents.Length);
            AssertEvent(newEvents[0], "IBM", 20d, 600L);
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(3, oldEvents.Length);
            AssertEvent(oldEvents[0], "DELL", null, 200L);
            AssertEvent(oldEvents[1], "IBM", 20d, 500L);
            AssertEvent(oldEvents[2], "DELL", null, 250L);
    
            stmt.Dispose();
        }
    
        private void SendSupportEvent(EPServiceProvider epService, string theString) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, -1));
        }
    
        private void SendMDEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, null));
        }
    
        private void AssertEvent(EventBean theEvent, string symbol, double? sumPrice, long volume) {
            Assert.AreEqual(symbol, theEvent.Get("symbol"));
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
            Assert.AreEqual(volume, theEvent.Get("volume"));
        }
    
        private void AssertEvent(EventBean theEvent, string symbol, double? sumPrice) {
            Assert.AreEqual(symbol, theEvent.Get("symbol"));
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }
    
        private void AssertEvent(EventBean theEvent, double? sumPrice) {
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }
    
        private void SendTimer(EPServiceProvider epService, long time) {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
