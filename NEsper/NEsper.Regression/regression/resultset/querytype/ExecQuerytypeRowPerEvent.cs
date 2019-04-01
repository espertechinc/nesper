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
    public class ExecQuerytypeRowPerEvent : RegressionExecution {
        private const string JOIN_KEY = "KEY";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionAggregatedSelectTriggerEvent(epService);
            RunAssertionAggregatedSelectUnaggregatedHaving(epService);
            RunAssertionSumOneView(epService);
            RunAssertionSumJoin(epService);
            RunAssertionSumAvgWithWhere(epService);
        }
    
        private void RunAssertionAggregatedSelectTriggerEvent(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            string epl = "select window(s0.*) as rows, sb " +
                    "from SupportBean#keepall as sb, SupportBean_S0#keepall as s0 " +
                    "where sb.TheString = s0.p00";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "K1", "V1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "K1", "V2"));
    
            // test SB-direction
            var b1 = new SupportBean("K1", 0);
            epService.EPRuntime.SendEvent(b1);
            EventBean[] events = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, events.Length);
            foreach (EventBean eventX in events) {
                Assert.AreEqual(b1, eventX.Get("sb"));
                Assert.AreEqual(2, ((SupportBean_S0[]) eventX.Get("rows")).Length);
            }
    
            // test S0-direction
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "K1", "V3"));
            EventBean @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(b1, @event.Get("sb"));
            Assert.AreEqual(3, ((SupportBean_S0[]) @event.Get("rows")).Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAggregatedSelectUnaggregatedHaving(EPServiceProvider epService) {
            // ESPER-571
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "select max(IntPrimitive) as val from SupportBean#time(1) having max(IntPrimitive) > IntBoxed";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "E1", 10, 1);
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent(epService, "E2", 10, 11);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E3", 15, 11);
            Assert.AreEqual(15, listener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent(epService, "E4", 20, 11);
            Assert.AreEqual(20, listener.AssertOneGetNewAndReset().Get("val"));
    
            SendEvent(epService, "E5", 25, 25);
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumOneView(EPServiceProvider epService) {
            string epl = "select irstream LongPrimitive, sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBean).FullName + "#length(3)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssert(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumJoin(EPServiceProvider epService) {
            string epl = "select irstream LongPrimitive, sum(LongBoxed) as mySum " +
                    "from " + typeof(SupportBeanString).FullName + "#length(3) as one, " +
                    typeof(SupportBean).FullName + "#length(3) as two " +
                    "where one.TheString = two.TheString";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));
    
            TryAssert(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumAvgWithWhere(EPServiceProvider epService) {
            string epl = "select 'IBM stats' as title, volume, avg(volume) as myAvg, sum(volume) as mySum " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3)" +
                    "where symbol='IBM'";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketDataEvent(epService, "GE", 10L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketDataEvent(epService, "IBM", 20L);
            AssertPostedNew(listener, 20d, 20L);
    
            SendMarketDataEvent(epService, "XXX", 10000L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketDataEvent(epService, "IBM", 30L);
            AssertPostedNew(listener, 25d, 50L);
    
            stmt.Dispose();
        }
    
        private void AssertPostedNew(SupportUpdateListener listener, double? newAvg, long newSum) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual("IBM stats", newData[0].Get("title"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
    
            listener.Reset();
        }
    
        private void TryAssert(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            var fields = new string[]{"LongPrimitive", "mySum"};
    
            // assert select result type
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            var eventCount = new AtomicLong();
    
            SendEvent(epService, eventCount, 10);
            Assert.AreEqual(10L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L, 10L}});
    
            SendEvent(epService, eventCount, 15);
            Assert.AreEqual(25L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L, 25L}, new object[] {2L, 25L}});
    
            SendEvent(epService, eventCount, -5);
            Assert.AreEqual(20L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {1L, 20L}, new object[] {2L, 20L}, new object[] {3L, 20L}});
    
            SendEvent(epService, eventCount, -2);
            Assert.AreEqual(8L, listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(8L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4L, 8L}, new object[] {2L, 8L}, new object[] {3L, 8L}});
    
            SendEvent(epService, eventCount, 100);
            Assert.AreEqual(93L, listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(93L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4L, 93L}, new object[] {5L, 93L}, new object[] {3L, 93L}});
    
            SendEvent(epService, eventCount, 1000);
            Assert.AreEqual(1098L, listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(1098L, listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][]{new object[] {4L, 1098L}, new object[] {5L, 1098L}, new object[] {6L, 1098L}});
        }
    
        private void SendEvent(EPServiceProvider epService, long longBoxed, int intBoxed, short shortBoxed, AtomicLong eventCount) {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            bean.LongPrimitive = eventCount.IncrementAndGet();
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketDataEvent(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, AtomicLong eventCount, long longBoxed) {
            SendEvent(epService, longBoxed, 0, (short) 0, eventCount);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive, int intBoxed) {
            var theEvent = new SupportBean(theString, intPrimitive);
            theEvent.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
