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


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeGroupByEventPerRow : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCriteriaByDotMethod(epService);
            RunAssertionIterateUnbound(epService);
            RunAssertionUnaggregatedHaving(epService);
            RunAssertionWildcard(epService);
            RunAssertionAggregationOverGroupedProps(epService);
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
            RunAssertionInsertInto(epService);
        }
    
        private void RunAssertionCriteriaByDotMethod(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "select sb.LongPrimitive as c0, sum(IntPrimitive) as c1 from SupportBean#length_batch(2) as sb group by sb.TheString";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            MakeSendSupportBean(epService, "E1", 10, 100L);
            MakeSendSupportBean(epService, "E1", 20, 200L);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new object[][]{new object[] {100L, 30}, new object[] {200L, 30}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIterateUnbound(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
            string epl = "@IterableUnbound select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnaggregatedHaving(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select TheString from SupportBean group by TheString having IntPrimitive > 5");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWildcard(EPServiceProvider epService) {
    
            // test no output limit
            string[] fields = "TheString, IntPrimitive, minval".Split(',');
            string epl = "select *, min(IntPrimitive) as minval from SupportBean#length(2) group by TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10, 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 9, 9});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G1", 11, 9});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregationOverGroupedProps(EPServiceProvider epService) {
            // test for ESPER-185
            string[] fields = "volume,symbol,price,mycount".Split(',');
            string epl = "select irstream volume,symbol,price,count(price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol, price";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, SYMBOL_DELL, 1000, 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1000L, "DELL", 10.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1000L, "DELL", 10.0, 1L}});
    
            SendEvent(epService, SYMBOL_DELL, 900, 11);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{900L, "DELL", 11.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1000L, "DELL", 10.0, 1L}, new object[] {900L, "DELL", 11.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 1500, 10);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{1500L, "DELL", 10.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L}, new object[] {1500L, "DELL", 10.0, 2L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 500, 5);
            Assert.AreEqual(1, listener.NewDataList.Count);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{500L, "IBM", 5.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L}, new object[] {1500L, "DELL", 10.0, 2L}, new object[] {500L, "IBM", 5.0, 1L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 600, 5);
            Assert.AreEqual(1, listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{600L, "IBM", 5.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1000L, "DELL", 10.0, 2L}, new object[] {900L, "DELL", 11.0, 1L}, new object[] {1500L, "DELL", 10.0, 2L}, new object[] {500L, "IBM", 5.0, 2L}, new object[] {600L, "IBM", 5.0, 2L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 500, 5);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{500L, "IBM", 5.0, 3L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{1000L, "DELL", 10.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {900L, "DELL", 11.0, 1L}, new object[] {1500L, "DELL", 10.0, 1L}, new object[] {500L, "IBM", 5.0, 3L}, new object[] {600L, "IBM", 5.0, 3L}, new object[] {500L, "IBM", 5.0, 3L}});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_IBM, 600, 5);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[]{600L, "IBM", 5.0, 4L});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{900L, "DELL", 11.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {1500L, "DELL", 10.0, 1L}, new object[] {500L, "IBM", 5.0, 4L}, new object[] {600L, "IBM", 5.0, 4L}, new object[] {500L, "IBM", 5.0, 4L}, new object[] {600L, "IBM", 5.0, 4L}});
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select irstream symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionSum(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            string epl = "select irstream symbol, volume, sum(price) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionSum(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInsertInto(EPServiceProvider epService) {
            var listenerOne = new SupportUpdateListener();
            string eventType = typeof(SupportMarketDataBean).FullName;
            string stmt = " select symbol as symbol, avg(price) as average, sum(volume) as sumation from " + eventType + "#length(3000)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10D, 20000L, null));
            EventBean eventBean = listenerOne.LastNewData[0];
            Assert.AreEqual("IBM", eventBean.Get("symbol"));
            Assert.AreEqual(10d, eventBean.Get("average"));
            Assert.AreEqual(20000L, eventBean.Get("sumation"));
    
            // create insert into statements
            stmt = "insert into StockAverages select symbol as symbol, avg(price) as average, sum(volume) as sumation " +
                    "from " + eventType + "#length(3000)";
            statement = epService.EPAdministrator.CreateEPL(stmt);
            var listenerTwo = new SupportUpdateListener();
            statement.Events += listenerTwo.Update;
    
            stmt = " select * from StockAverages";
            statement = epService.EPAdministrator.CreateEPL(stmt);
            var listenerThree = new SupportUpdateListener();
            statement.Events += listenerThree.Update;
    
            // send event
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 20D, 40000L, null));
            eventBean = listenerOne.LastNewData[0];
            Assert.AreEqual("IBM", eventBean.Get("symbol"));
            Assert.AreEqual(15d, eventBean.Get("average"));
            Assert.AreEqual(60000L, eventBean.Get("sumation"));
    
            Assert.AreEqual(1, listenerThree.NewDataList.Count);
            Assert.AreEqual(1, listenerThree.LastNewData.Length);
            eventBean = listenerThree.LastNewData[0];
            Assert.AreEqual("IBM", eventBean.Get("symbol"));
            Assert.AreEqual(20d, eventBean.Get("average"));
            Assert.AreEqual(40000L, eventBean.Get("sumation"));
        }
    
        private void TryAssertionSum(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"symbol", "volume", "mySum"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("volume"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("mySum"));
    
            SendEvent(epService, SYMBOL_DELL, 10000, 51);
            AssertEvents(listener, SYMBOL_DELL, 10000, 51);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                new object[] {"DELL", 10000L, 51d}});
    
            SendEvent(epService, SYMBOL_DELL, 20000, 52);
            AssertEvents(listener, SYMBOL_DELL, 20000, 103);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                new object[] {"DELL", 10000L, 103d},
                new object[] {"DELL", 20000L, 103d}});
    
            SendEvent(epService, SYMBOL_IBM, 30000, 70);
            AssertEvents(listener, SYMBOL_IBM, 30000, 70);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                new object[] {"DELL", 10000L, 103d},
                new object[] {"DELL", 20000L, 103d},
                new object[] {"IBM", 30000L, 70d}});
    
            SendEvent(epService, SYMBOL_IBM, 10000, 20);
            AssertEvents(listener, SYMBOL_DELL, 10000, 52, SYMBOL_IBM, 10000, 90);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                new object[] {"DELL", 20000L, 52d},
                new object[] {"IBM", 30000L, 90d},
                new object[] {"IBM", 10000L, 90d}});
    
            SendEvent(epService, SYMBOL_DELL, 40000, 45);
            AssertEvents(listener, SYMBOL_DELL, 20000, 45, SYMBOL_DELL, 40000, 45);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{
                new object[] {"IBM", 10000L, 90d},
                new object[] {"IBM", 30000L, 90d},
                new object[] {"DELL", 40000L, 45d}});
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbol, long volume, double sum) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(volume, newData[0].Get("volume"));
            Assert.AreEqual(sum, newData[0].Get("mySum"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbolOld, long volumeOld, double sumOld,
                                  string symbolNew, long volumeNew, double sumNew) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbolOld, oldData[0].Get("symbol"));
            Assert.AreEqual(volumeOld, oldData[0].Get("volume"));
            Assert.AreEqual(sumOld, oldData[0].Get("mySum"));
    
            Assert.AreEqual(symbolNew, newData[0].Get("symbol"));
            Assert.AreEqual(volumeNew, newData[0].Get("volume"));
            Assert.AreEqual(sumNew, newData[0].Get("mySum"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private SupportBean MakeSendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace
