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
using com.espertech.esper.type;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprPrevious : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionExprNameAndTypeAndSODA(epService);
            RunAssertionPrevStream(epService);
            RunAssertionPrevCountStarWithStaticMethod(epService);
            RunAssertionPrevCountStar(epService);
            RunAssertionPerGroupTwoCriteria(epService);
            RunAssertionSortWindowPerGroup(epService);
            RunAssertionTimeBatchPerGroup(epService);
            RunAssertionLengthBatchPerGroup(epService);
            RunAssertionTimeWindowPerGroup(epService);
            RunAssertionExtTimeWindowPerGroup(epService);
            RunAssertionLengthWindowPerGroup(epService);
            RunAssertionPreviousTimeWindow(epService);
            RunAssertionPreviousExtTimedWindow(epService);
            RunAssertionPreviousTimeBatchWindow(epService);
            RunAssertionPreviousTimeBatchWindowJoin(epService);
            RunAssertionPreviousLengthWindow(epService);
            RunAssertionPreviousLengthBatch(epService);
            RunAssertionPreviousLengthWindowWhere(epService);
            RunAssertionPreviousLengthWindowDynamic(epService);
            RunAssertionPreviousSortWindow(epService);
            RunAssertionPreviousExtTimedBatch(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionExprNameAndTypeAndSODA(EPServiceProvider epService) {
            string epl = "select " +
                    "prev(1,IntPrimitive), " +
                    "prev(1,sb), " +
                    "prevtail(1,IntPrimitive), " +
                    "prevtail(1,sb), " +
                    "prevwindow(IntPrimitive), " +
                    "prevwindow(sb), " +
                    "prevcount(IntPrimitive), " +
                    "prevcount(sb) " +
                    "from SupportBean#time(1 minutes) as sb";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EventBean resultBean = listener.GetNewDataListFlattened()[1];
    
            var rows = new object[][]{
                new object[] {"prev(1,IntPrimitive)", typeof(int?)},
                new object[] {"prev(1,sb)", typeof(SupportBean)},
                new object[] {"prevtail(1,IntPrimitive)", typeof(int?) },
                new object[] {"prevtail(1,sb)", typeof(SupportBean)},
                new object[] {"prevwindow(IntPrimitive)", typeof(int?[])},
                new object[] {"prevwindow(sb)", typeof(SupportBean[])},
                new object[] {"prevcount(IntPrimitive)", typeof(long?) },
                new object[] {"prevcount(sb)", typeof(long?) }
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "For prop '" + rows[i][0] + "'";
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1].GetBoxedType(), prop.PropertyType.GetBoxedType(), message);
                Object result = resultBean.Get(prop.PropertyName);
                Assert.AreEqual(prop.PropertyType.GetBoxedType(), result.GetType().GetBoxedType(), message);
            }
    
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            stmt = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(stmt.Text, epl);
            stmt.Dispose();
        }
    
        private void RunAssertionPrevStream(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            string text = "select prev(1, s0) as result, " +
                    "prevtail(0, s0) as tailresult," +
                    "prevwindow(s0) as windowresult," +
                    "prevcount(s0) as countresult " +
                    "from S0#length(2) as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "result,tailresult,windowresult,countresult".Split(',');
    
            var e1 = new SupportBean_S0(1);
            epService.EPRuntime.SendEvent(e1);
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{null, e1, new object[]{e1}, 1L});
    
            var e2 = new SupportBean_S0(2);
            epService.EPRuntime.SendEvent(e2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{e1, e1, new object[]{e2, e1}, 2L});
            Assert.AreEqual(typeof(SupportBean_S0), stmt.EventType.GetPropertyType("result"));
    
            var e3 = new SupportBean_S0(3);
            epService.EPRuntime.SendEvent(e3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{e2, e2, new object[]{e3, e2}, 2L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrevCountStarWithStaticMethod(EPServiceProvider epService) {
            string text = "select irstream count(*) as total, " +
                    "prev(" + typeof(ExecExprPrevious).FullName + ".IntToLong(count(*)) - 1, price) as firstPrice from " + typeof(SupportMarketDataBean).FullName + "#time(60)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertPrevCount(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrevCountStar(EPServiceProvider epService) {
            string text = "select irstream count(*) as total, " +
                    "prev(count(*) - 1, price) as firstPrice from " + typeof(SupportMarketDataBean).FullName + "#time(60)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            AssertPrevCount(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPerGroupTwoCriteria(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("MDBean", typeof(SupportMarketDataBean));
            string epl = "select symbol, feed, " +
                    "prev(1, price) as prevPrice, " +
                    "prevtail(price) as tailPrice, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from MDBean#groupwin(symbol, feed)#length(2)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "symbol,feed,prevPrice,tailPrice,countPrice,windowPrice".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10, 0L, "F1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", "F1", null, 10d, 1L, SplitDoubles("10d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 11, 0L, "F1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", "F1", 10d, 10d, 2L, SplitDoubles("11d,10d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("MSFT", 100, 0L, "F2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"MSFT", "F2", null, 100d, 1L, SplitDoubles("100d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 12, 0L, "F2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", "F2", null, 12d, 1L, SplitDoubles("12d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 13, 0L, "F1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", "F1", 11d, 11d, 2L, SplitDoubles("13d,11d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("MSFT", 101, 0L, "F2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"MSFT", "F2", 100d, 100d, 2L, SplitDoubles("101d,100d")});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 17, 0L, "F2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"IBM", "F2", 12d, 12d, 2L, SplitDoubles("17d,12d")});
    
            // test length window overflow
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("select prev(5,IntPrimitive) as val0 from SupportBean#groupwin(TheString)#length(5)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 11));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 12));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 13));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 14));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 15));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 20));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 21));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 22));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 23));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 24));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 31));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 25));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 16));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("val0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionSortWindowPerGroup(EPServiceProvider epService) {
            // descending sort
            string epl = "select " +
                    "symbol, " +
                    "prev(1, price) as prevPrice, " +
                    "prev(2, price) as prevPrevPrice, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#sort(10, price asc) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail1Price"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("countPrice"));
            Assert.AreEqual(typeof(double?[]), stmt.EventType.GetPropertyType("windowPrice"));
    
            SendMarketEvent(epService, "IBM", 75);
            AssertReceived(listener, "IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));
            SendMarketEvent(epService, "IBM", 80);
            AssertReceived(listener, "IBM", 80d, null, 80d, 75d, 2L, SplitDoubles("75d,80d"));
            SendMarketEvent(epService, "IBM", 79);
            AssertReceived(listener, "IBM", 79d, 80d, 80d, 79d, 3L, SplitDoubles("75d,79d,80d"));
            SendMarketEvent(epService, "IBM", 81);
            AssertReceived(listener, "IBM", 79d, 80d, 81d, 80d, 4L, SplitDoubles("75d,79d,80d,81d"));
            SendMarketEvent(epService, "IBM", 79.5);
            AssertReceived(listener, "IBM", 79d, 79.5d, 81d, 80d, 5L, SplitDoubles("75d,79d,79.5,80d,81d"));    // 75, 79, 79.5, 80, 81
    
            SendMarketEvent(epService, "MSFT", 10);
            AssertReceived(listener, "MSFT", null, null, 10d, null, 1L, SplitDoubles("10d"));
            SendMarketEvent(epService, "MSFT", 20);
            AssertReceived(listener, "MSFT", 20d, null, 20d, 10d, 2L, SplitDoubles("10d,20d"));
            SendMarketEvent(epService, "MSFT", 21);
            AssertReceived(listener, "MSFT", 20d, 21d, 21d, 20d, 3L, SplitDoubles("10d,20d,21d")); // 10, 20, 21
    
            SendMarketEvent(epService, "IBM", 74d);
            AssertReceived(listener, "IBM", 75d, 79d, 81d, 80d, 6L, SplitDoubles("74d,75d,79d,79.5,80d,81d"));  // 74, 75, 79, 79.5, 80, 81
    
            SendMarketEvent(epService, "MSFT", 19);
            AssertReceived(listener, "MSFT", 19d, 20d, 21d, 20d, 4L, SplitDoubles("10d,19d,20d,21d")); // 10, 19, 20, 21
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchPerGroup(EPServiceProvider epService) {
            string epl = "select " +
                    "symbol, " +
                    "prev(1, price) as prevPrice, " +
                    "prev(2, price) as prevPrevPrice, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#time_batch(1 sec) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail1Price"));
    
            SendTimer(epService, 0);
            SendMarketEvent(epService, "IBM", 75);
            SendMarketEvent(epService, "MSFT", 40);
            SendMarketEvent(epService, "IBM", 76);
            SendMarketEvent(epService, "CIC", 1);
            SendTimer(epService, 1000);
    
            EventBean[] events = listener.LastNewData;
            // order not guaranteed as timed batch, however for testing the order is reliable as schedule buckets are created
            // in a predictable order
            // Previous is looking at the same batch, doesn't consider outside of window
            AssertReceived(events[0], "IBM", null, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
            AssertReceived(events[1], "IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
            AssertReceived(events[2], "MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));
            AssertReceived(events[3], "CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));
    
            // Next batch, previous is looking only within the same batch
            SendMarketEvent(epService, "MSFT", 41);
            SendMarketEvent(epService, "IBM", 77);
            SendMarketEvent(epService, "IBM", 78);
            SendMarketEvent(epService, "CIC", 2);
            SendMarketEvent(epService, "MSFT", 42);
            SendMarketEvent(epService, "CIC", 3);
            SendMarketEvent(epService, "CIC", 4);
            SendTimer(epService, 2000);
    
            events = listener.LastNewData;
            AssertReceived(events[0], "IBM", null, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
            AssertReceived(events[1], "IBM", 77d, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
            AssertReceived(events[2], "MSFT", null, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
            AssertReceived(events[3], "MSFT", 41d, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
            AssertReceived(events[4], "CIC", null, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertReceived(events[5], "CIC", 2d, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertReceived(events[6], "CIC", 3d, 2d, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionLengthBatchPerGroup(EPServiceProvider epService) {
            // Also testing the alternative syntax here of "prev(property)" and "prev(property, index)" versus "prev(index, property)"
            string epl = "select irstream " +
                    "symbol, " +
                    "prev(price) as prevPrice, " +
                    "prev(price, 2) as prevPrevPrice, " +
                    "prevtail(price, 0) as prevTail0Price, " +
                    "prevtail(price, 1) as prevTail1Price, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#length_batch(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail1Price"));
    
            SendMarketEvent(epService, "IBM", 75);
            SendMarketEvent(epService, "MSFT", 50);
            SendMarketEvent(epService, "IBM", 76);
            SendMarketEvent(epService, "CIC", 1);
            Assert.IsFalse(listener.IsInvoked);
            SendMarketEvent(epService, "IBM", 77);
    
            EventBean[] eventsNew = listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "IBM", null, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            AssertReceived(eventsNew[1], "IBM", 75d, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            AssertReceived(eventsNew[2], "IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            listener.Reset();
    
            // Next batch, previous is looking only within the same batch
            SendMarketEvent(epService, "MSFT", 51);
            SendMarketEvent(epService, "IBM", 78);
            SendMarketEvent(epService, "IBM", 79);
            SendMarketEvent(epService, "CIC", 2);
            SendMarketEvent(epService, "CIC", 3);
    
            eventsNew = listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "CIC", null, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertReceived(eventsNew[1], "CIC", 1d, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertReceived(eventsNew[2], "CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            listener.Reset();
    
            SendMarketEvent(epService, "MSFT", 52);
    
            eventsNew = listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "MSFT", null, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            AssertReceived(eventsNew[1], "MSFT", 50d, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            AssertReceived(eventsNew[2], "MSFT", 51d, 50d, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            listener.Reset();
    
            SendMarketEvent(epService, "IBM", 80);
    
            eventsNew = listener.LastNewData;
            EventBean[] eventsOld = listener.LastOldData;
            Assert.AreEqual(3, eventsNew.Length);
            Assert.AreEqual(3, eventsOld.Length);
            AssertReceived(eventsNew[0], "IBM", null, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsNew[1], "IBM", 78d, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsNew[2], "IBM", 79d, 78d, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsOld[0], "IBM", null, null, null, null, null, null);
            AssertReceived(eventsOld[1], "IBM", null, null, null, null, null, null);
            AssertReceived(eventsOld[2], "IBM", null, null, null, null, null, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeWindowPerGroup(EPServiceProvider epService) {
            string epl = "select " +
                    "symbol, " +
                    "prev(1, price) as prevPrice, " +
                    "prev(2, price) as prevPrevPrice, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#time(20 sec) ";
            AssertPerGroup(epl, epService);
        }
    
        private void RunAssertionExtTimeWindowPerGroup(EPServiceProvider epService) {
            string epl = "select " +
                    "symbol, " +
                    "prev(1, price) as prevPrice, " +
                    "prev(2, price) as prevPrevPrice, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as countPrice, " +
                    "prevwindow(price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#ext_timed(volume, 20 sec) ";
            AssertPerGroup(epl, epService);
        }
    
        private void RunAssertionLengthWindowPerGroup(EPServiceProvider epService) {
            string epl =
                    "select symbol, " +
                            "prev(1, price) as prevPrice, " +
                            "prev(2, price) as prevPrevPrice, " +
                            "prevtail(price, 0) as prevTail0Price, " +
                            "prevtail(price, 1) as prevTail1Price, " +
                            "prevcount(price) as countPrice, " +
                            "prevwindow(price) as windowPrice " +
                            "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#length(10) ";
            AssertPerGroup(epl, epService);
        }
    
        private void RunAssertionPreviousTimeWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prev(2, symbol) as prevSymbol, " +
                    " prev(2, price) as prevPrice, " +
                    " prevtail(0, symbol) as prevTailSymbol, " +
                    " prevtail(0, price) as prevTailPrice, " +
                    " prevtail(1, symbol) as prevTail1Symbol, " +
                    " prevtail(1, price) as prevTail1Price, " +
                    " prevcount(price) as prevCountPrice, " +
                    " prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D1", 1);
            AssertNewEventWTail(listener, "D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));
    
            SendTimer(epService, 1000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D2", 2);
            AssertNewEventWTail(listener, "D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));
    
            SendTimer(epService, 2000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D3", 3);
            AssertNewEventWTail(listener, "D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));
    
            SendTimer(epService, 3000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D4", 4);
            AssertNewEventWTail(listener, "D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));
    
            SendTimer(epService, 4000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D5", 5);
            AssertNewEventWTail(listener, "D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));
    
            SendTimer(epService, 30000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "D6", 6);
            AssertNewEventWTail(listener, "D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));
    
            // Test remove stream, always returns null as previous function
            // returns null for remove stream for time windows
            SendTimer(epService, 60000);
            AssertOldEventWTail(listener, "D1", null, null, null, null, null, null, null, null);
            SendTimer(epService, 61000);
            AssertOldEventWTail(listener, "D2", null, null, null, null, null, null, null, null);
            SendTimer(epService, 62000);
            AssertOldEventWTail(listener, "D3", null, null, null, null, null, null, null, null);
            SendTimer(epService, 63000);
            AssertOldEventWTail(listener, "D4", null, null, null, null, null, null, null, null);
            SendTimer(epService, 64000);
            AssertOldEventWTail(listener, "D5", null, null, null, null, null, null, null, null);
            SendTimer(epService, 90000);
            AssertOldEventWTail(listener, "D6", null, null, null, null, null, null, null, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousExtTimedWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prev(2, symbol) as prevSymbol, " +
                    " prev(2, price) as prevPrice, " +
                    " prevtail(0, symbol) as prevTailSymbol, " +
                    " prevtail(0, price) as prevTailPrice, " +
                    " prevtail(1, symbol) as prevTail1Symbol, " +
                    " prevtail(1, price) as prevTail1Price, " +
                    " prevcount(price) as prevCountPrice, " +
                    " prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#ext_timed(volume, 1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prevTailSymbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTailPrice"));
    
            SendMarketEvent(epService, "D1", 1, 0);
            AssertNewEventWTail(listener, "D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));
    
            SendMarketEvent(epService, "D2", 2, 1000);
            AssertNewEventWTail(listener, "D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));
    
            SendMarketEvent(epService, "D3", 3, 3000);
            AssertNewEventWTail(listener, "D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));
    
            SendMarketEvent(epService, "D4", 4, 4000);
            AssertNewEventWTail(listener, "D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));
    
            SendMarketEvent(epService, "D5", 5, 5000);
            AssertNewEventWTail(listener, "D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));
    
            SendMarketEvent(epService, "D6", 6, 30000);
            AssertNewEventWTail(listener, "D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));
    
            SendMarketEvent(epService, "D7", 7, 60000);
            AssertEventWTail(listener.LastNewData[0], "D7", "D5", 5d, "D2", 2d, "D3", 3d, 6L, SplitDoubles("7d,6d,5d,4d,3d,2d"));
            AssertEventWTail(listener.LastOldData[0], "D1", null, null, null, null, null, null, null, null);
            listener.Reset();
    
            SendMarketEvent(epService, "D8", 8, 61000);
            AssertEventWTail(listener.LastNewData[0], "D8", "D6", 6d, "D3", 3d, "D4", 4d, 6L, SplitDoubles("8d,7d,6d,5d,4d,3d"));
            AssertEventWTail(listener.LastOldData[0], "D2", null, null, null, null, null, null, null, null);
            listener.Reset();
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousTimeBatchWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    " prev(2, symbol) as prevSymbol, " +
                    " prev(2, price) as prevPrice, " +
                    " prevtail(0, symbol) as prevTailSymbol, " +
                    " prevtail(0, price) as prevTailPrice, " +
                    " prevtail(1, symbol) as prevTail1Symbol, " +
                    " prevtail(1, price) as prevTail1Price, " +
                    " prevcount(price) as prevCountPrice, " +
                    " prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time_batch(1 min) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 60000);
            Assert.AreEqual(2, listener.LastNewData.Length);
            AssertEventWTail(listener.LastNewData[0], "A", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            AssertEventWTail(listener.LastNewData[1], "B", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(epService, 80000);
            SendMarketEvent(epService, "C", 3);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 120000);
            Assert.AreEqual(1, listener.LastNewData.Length);
            AssertEventWTail(listener.LastNewData[0], "C", null, null, "C", 3d, null, null, 1L, SplitDoubles("3d"));
            Assert.AreEqual(2, listener.LastOldData.Length);
            AssertEventWTail(listener.LastOldData[0], "A", null, null, null, null, null, null, null, null);
            listener.Reset();
    
            SendTimer(epService, 300000);
            SendMarketEvent(epService, "D", 4);
            SendMarketEvent(epService, "E", 5);
            SendMarketEvent(epService, "F", 6);
            SendMarketEvent(epService, "G", 7);
            SendTimer(epService, 360000);
            Assert.AreEqual(4, listener.LastNewData.Length);
            AssertEventWTail(listener.LastNewData[0], "D", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(listener.LastNewData[1], "E", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(listener.LastNewData[2], "F", "D", 4d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(listener.LastNewData[3], "G", "E", 5d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousTimeBatchWindowJoin(EPServiceProvider epService) {
            string epl = "select TheString as currSymbol, " +
                    " prev(2, symbol) as prevSymbol, " +
                    " prev(1, price) as prevPrice, " +
                    " prevtail(0, symbol) as prevTailSymbol, " +
                    " prevtail(0, price) as prevTailPrice, " +
                    " prevtail(1, symbol) as prevTail1Symbol, " +
                    " prevtail(1, price) as prevTail1Price, " +
                    " prevcount(price) as prevCountPrice, " +
                    " prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportBean).FullName + "#keepall, " +
                    typeof(SupportMarketDataBean).FullName + "#time_batch(1 min)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
    
            SendTimer(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 2);
            SendBeanEvent(epService, "X1");
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 60000);
            Assert.AreEqual(2, listener.LastNewData.Length);
            AssertEventWTail(listener.LastNewData[0], "X1", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            AssertEventWTail(listener.LastNewData[1], "X1", null, 1d, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendMarketEvent(epService, "C1", 11);
            SendMarketEvent(epService, "C2", 12);
            SendMarketEvent(epService, "C3", 13);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 120000);
            Assert.AreEqual(3, listener.LastNewData.Length);
            AssertEventWTail(listener.LastNewData[0], "X1", null, null, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
            AssertEventWTail(listener.LastNewData[1], "X1", null, 11d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
            AssertEventWTail(listener.LastNewData[2], "X1", "C1", 12d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousLengthWindow(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    "prev(0, symbol) as prev0Symbol, " +
                    "prev(1, symbol) as prev1Symbol, " +
                    "prev(2, symbol) as prev2Symbol, " +
                    "prev(0, price) as prev0Price, " +
                    "prev(1, price) as prev1Price, " +
                    "prev(2, price) as prev2Price," +
                    "prevtail(0, symbol) as prevTail0Symbol, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, symbol) as prevTail1Symbol, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as prevCountPrice, " +
                    "prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prev0Price"));
    
            SendMarketEvent(epService, "A", 1);
            AssertNewEvents(listener, "A", "A", 1d, null, null, null, null, "A", 1d, null, null, 1L, SplitDoubles("1d"));
            SendMarketEvent(epService, "B", 2);
            AssertNewEvents(listener, "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            SendMarketEvent(epService, "C", 3);
            AssertNewEvents(listener, "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            SendMarketEvent(epService, "D", 4);
            EventBean newEvent = listener.LastNewData[0];
            EventBean oldEvent = listener.LastOldData[0];
            AssertEventProps(listener, newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "B", 2d, "C", 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertEventProps(listener, oldEvent, "A", null, null, null, null, null, null, null, null, null, null, null, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousLengthBatch(EPServiceProvider epService) {
            string epl = "select irstream symbol as currSymbol, " +
                    "prev(0, symbol) as prev0Symbol, " +
                    "prev(1, symbol) as prev1Symbol, " +
                    "prev(2, symbol) as prev2Symbol, " +
                    "prev(0, price) as prev0Price, " +
                    "prev(1, price) as prev1Price, " +
                    "prev(2, price) as prev2Price, " +
                    "prevtail(0, symbol) as prevTail0Symbol, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, symbol) as prevTail1Symbol, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as prevCountPrice, " +
                    "prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length_batch(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prev0Price"));
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "C", 3);
            EventBean[] newEvents = listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEventProps(listener, newEvents[0], "A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertEventProps(listener, newEvents[1], "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertEventProps(listener, newEvents[2], "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            listener.Reset();
    
            SendMarketEvent(epService, "D", 4);
            SendMarketEvent(epService, "E", 5);
            Assert.IsFalse(listener.IsInvoked);
    
            SendMarketEvent(epService, "F", 6);
            newEvents = listener.LastNewData;
            EventBean[] oldEvents = listener.LastOldData;
            Assert.AreEqual(3, newEvents.Length);
            Assert.AreEqual(3, oldEvents.Length);
            AssertEventProps(listener, newEvents[0], "D", "D", 4d, null, null, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(listener, newEvents[1], "E", "E", 5d, "D", 4d, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(listener, newEvents[2], "F", "F", 6d, "E", 5d, "D", 4d, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(listener, oldEvents[0], "A", null, null, null, null, null, null, null, null, null, null, null, null);
            AssertEventProps(listener, oldEvents[1], "B", null, null, null, null, null, null, null, null, null, null, null, null);
            AssertEventProps(listener, oldEvents[2], "C", null, null, null, null, null, null, null, null, null, null, null, null);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousLengthWindowWhere(EPServiceProvider epService) {
            string epl = "select prev(2, symbol) as currSymbol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(100) " +
                    "where prev(2, price) > 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketEvent(epService, "A", 1);
            SendMarketEvent(epService, "B", 130);
            SendMarketEvent(epService, "C", 10);
            Assert.IsFalse(listener.IsInvoked);
            SendMarketEvent(epService, "D", 5);
            Assert.AreEqual("B", listener.AssertOneGetNewAndReset().Get("currSymbol"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousLengthWindowDynamic(EPServiceProvider epService) {
            string epl = "select prev(IntPrimitive, TheString) as sPrev " +
                    "from " + typeof(SupportBean).FullName + "#length(100)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanEvent(epService, "A", 1);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("sPrev"));
    
            SendBeanEvent(epService, "B", 0);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sPrev"));
    
            SendBeanEvent(epService, "C", 2);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sPrev"));
    
            SendBeanEvent(epService, "D", 1);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("C", theEvent.Get("sPrev"));
    
            SendBeanEvent(epService, "E", 4);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sPrev"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousSortWindow(EPServiceProvider epService) {
            string epl = "select symbol as currSymbol, " +
                    " prev(0, symbol) as prev0Symbol, " +
                    " prev(1, symbol) as prev1Symbol, " +
                    " prev(2, symbol) as prev2Symbol, " +
                    " prev(0, price) as prev0Price, " +
                    " prev(1, price) as prev1Price, " +
                    " prev(2, price) as prev2Price, " +
                    " prevtail(0, symbol) as prevTail0Symbol, " +
                    " prevtail(0, price) as prevTail0Price, " +
                    " prevtail(1, symbol) as prevTail1Symbol, " +
                    " prevtail(1, price) as prevTail1Price, " +
                    " prevcount(price) as prevCountPrice, " +
                    " prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#sort(100, symbol asc)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prev0Price"));
    
            SendMarketEvent(epService, "COX", 30);
            AssertNewEvents(listener, "COX", "COX", 30d, null, null, null, null, "COX", 30d, null, null, 1L, SplitDoubles("30d"));
    
            SendMarketEvent(epService, "IBM", 45);
            AssertNewEvents(listener, "IBM", "COX", 30d, "IBM", 45d, null, null, "IBM", 45d, "COX", 30d, 2L, SplitDoubles("30d,45d"));
    
            SendMarketEvent(epService, "MSFT", 33);
            AssertNewEvents(listener, "MSFT", "COX", 30d, "IBM", 45d, "MSFT", 33d, "MSFT", 33d, "IBM", 45d, 3L, SplitDoubles("30d,45d,33d"));
    
            SendMarketEvent(epService, "XXX", 55);
            AssertNewEvents(listener, "XXX", "COX", 30d, "IBM", 45d, "MSFT", 33d, "XXX", 55d, "MSFT", 33d, 4L, SplitDoubles("30d,45d,33d,55d"));
    
            SendMarketEvent(epService, "CXX", 56);
            AssertNewEvents(listener, "CXX", "COX", 30d, "CXX", 56d, "IBM", 45d, "XXX", 55d, "MSFT", 33d, 5L, SplitDoubles("30d,56d,45d,33d,55d"));
    
            SendMarketEvent(epService, "GE", 1);
            AssertNewEvents(listener, "GE", "COX", 30d, "CXX", 56d, "GE", 1d, "XXX", 55d, "MSFT", 33d, 6L, SplitDoubles("30d,56d,1d,45d,33d,55d"));
    
            SendMarketEvent(epService, "AAA", 1);
            AssertNewEvents(listener, "AAA", "AAA", 1d, "COX", 30d, "CXX", 56d, "XXX", 55d, "MSFT", 33d, 7L, SplitDoubles("1d,30d,56d,1d,45d,33d,55d"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionPreviousExtTimedBatch(EPServiceProvider epService) {
            string[] fields = "currSymbol,prev0Symbol,prev0Price,prev1Symbol,prev1Price,prev2Symbol,prev2Price,prevTail0Symbol,prevTail0Price,prevTail1Symbol,prevTail1Price,prevCountPrice,prevWindowPrice".Split(',');
            string epl = "select irstream symbol as currSymbol, " +
                    "prev(0, symbol) as prev0Symbol, " +
                    "prev(0, price) as prev0Price, " +
                    "prev(1, symbol) as prev1Symbol, " +
                    "prev(1, price) as prev1Price, " +
                    "prev(2, symbol) as prev2Symbol, " +
                    "prev(2, price) as prev2Price," +
                    "prevtail(0, symbol) as prevTail0Symbol, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, symbol) as prevTail1Symbol, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as prevCountPrice, " +
                    "prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#ext_timed_batch(volume, 10, 0L) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketEvent(epService, "A", 1, 1000);
            SendMarketEvent(epService, "B", 2, 1001);
            SendMarketEvent(epService, "C", 3, 1002);
            SendMarketEvent(epService, "D", 4, 10000);
    
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{
                        new object[] {"A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
                        new object[] {"B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
                        new object[] {"C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")}
                    },
                    null);
    
            SendMarketEvent(epService, "E", 5, 20000);
    
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{
                        new object[] {"D", "D", 4d, null, null, null, null, "D", 4d, null, null, 1L, SplitDoubles("4d")},
                    },
                    new object[][]{
                        new object[] {"A", null, null, null, null, null, null, null, null, null, null, null, null},
                        new object[] {"B", null, null, null, null, null, null, null, null, null, null, null, null},
                        new object[] {"C", null, null, null, null, null, null, null, null, null, null, null, null},
                    }
            );
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            TryInvalid(epService, "select prev(0, average) " +
                            "from " + typeof(SupportMarketDataBean).FullName + "#length(100)#uni(price)",
                    "Error starting statement: Previous function requires a single data window view onto the stream [");
    
            TryInvalid(epService, "select count(*) from SupportBean#keepall where prev(0, IntPrimitive) = 5",
                    "Error starting statement: The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean#keepall where prev(0, IntPrimitive) = 5]");
    
            TryInvalid(epService, "select count(*) from SupportBean#keepall having prev(0, IntPrimitive) = 5",
                    "Error starting statement: The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean#keepall having prev(0, IntPrimitive) = 5]");
        }

        private void AssertEventWTail(
            EventBean eventBean,
            string currSymbol,
            string prevSymbol,
            double? prevPrice,
            string prevTailSymbol,
            double? prevTailPrice,
            string prevTail1Symbol,
            double? prevTail1Price,
            long? prevcount,
            object[] prevwindow)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prevSymbol, eventBean.Get("prevSymbol"));
            Assert.AreEqual(prevPrice, eventBean.Get("prevPrice"));
            Assert.AreEqual(prevTailSymbol, eventBean.Get("prevTailSymbol"));
            Assert.AreEqual(prevTailPrice, eventBean.Get("prevTailPrice"));
            Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
            Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
            Assert.AreEqual(prevcount, eventBean.Get("prevCountPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(eventBean.Get("prevWindowPrice").Unwrap<object>(), prevwindow);
        }

        private void AssertNewEvents(
            SupportUpdateListener listener, string currSymbol,
            string prev0Symbol,
            double? prev0Price,
            string prev1Symbol,
            double? prev1Price,
            string prev2Symbol,
            double? prev2Price,
            string prevTail0Symbol,
            double? prevTail0Price,
            string prevTail1Symbol,
            double? prevTail1Price,
            long prevCount,
            object[] prevWindow)
        {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
            AssertEventProps(listener, newData[0], currSymbol, prev0Symbol, prev0Price, prev1Symbol, prev1Price, prev2Symbol, prev2Price,
                    prevTail0Symbol, prevTail0Price, prevTail1Symbol, prevTail1Price, prevCount, prevWindow);
    
            listener.Reset();
        }

        private void AssertEventProps(
            SupportUpdateListener listener,
            EventBean eventBean,
            string currSymbol,
            string prev0Symbol,
            double? prev0Price,
            string prev1Symbol,
            double? prev1Price,
            string prev2Symbol,
            double? prev2Price,
            string prevTail0Symbol,
            double? prevTail0Price,
            string prevTail1Symbol,
            double? prevTail1Price,
            long? prevCount,
            object[] prevWindow)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prev0Symbol, eventBean.Get("prev0Symbol"));
            Assert.AreEqual(prev0Price, eventBean.Get("prev0Price"));
            Assert.AreEqual(prev1Symbol, eventBean.Get("prev1Symbol"));
            Assert.AreEqual(prev1Price, eventBean.Get("prev1Price"));
            Assert.AreEqual(prev2Symbol, eventBean.Get("prev2Symbol"));
            Assert.AreEqual(prev2Price, eventBean.Get("prev2Price"));
            Assert.AreEqual(prevTail0Symbol, eventBean.Get("prevTail0Symbol"));
            Assert.AreEqual(prevTail0Price, eventBean.Get("prevTail0Price"));
            Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
            Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
            Assert.AreEqual(prevCount, eventBean.Get("prevCountPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(eventBean.Get("prevWindowPrice").Unwrap<object>(), prevWindow);
    
            listener.Reset();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendMarketEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendBeanEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }

        private void AssertNewEventWTail(
            SupportUpdateListener listener, string currSymbol,
            string prevSymbol,
            double? prevPrice,
            string prevTailSymbol,
            double? prevTailPrice,
            string prevTail1Symbol,
            double? prevTail1Price,
            long prevcount,
            object[] prevwindow)
        {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            AssertEventWTail(newData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);
    
            listener.Reset();
        }

        private void AssertOldEventWTail(
            SupportUpdateListener listener,
            string currSymbol,
            string prevSymbol,
            double? prevPrice,
            string prevTailSymbol,
            double? prevTailPrice,
            string prevTail1Symbol,
            double? prevTail1Price,
            long? prevcount,
            object[] prevwindow)
        {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);
    
            AssertEventWTail(oldData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);
    
            listener.Reset();
        }
    
        private void AssertPerGroup(string statement, EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("prevTail1Price"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("countPrice"));
            Assert.AreEqual(typeof(double?[]), stmt.EventType.GetPropertyType("windowPrice"));
    
            SendMarketEvent(epService, "IBM", 75);
            AssertReceived(listener, "IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));
    
            SendMarketEvent(epService, "MSFT", 40);
            AssertReceived(listener, "MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));
    
            SendMarketEvent(epService, "IBM", 76);
            AssertReceived(listener, "IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
    
            SendMarketEvent(epService, "CIC", 1);
            AssertReceived(listener, "CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));
    
            SendMarketEvent(epService, "MSFT", 41);
            AssertReceived(listener, "MSFT", 40d, null, 40d, 41d, 2L, SplitDoubles("41d,40d"));
    
            SendMarketEvent(epService, "IBM", 77);
            AssertReceived(listener, "IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
    
            SendMarketEvent(epService, "IBM", 78);
            AssertReceived(listener, "IBM", 77d, 76d, 75d, 76d, 4L, SplitDoubles("78d,77d,76d,75d"));
    
            SendMarketEvent(epService, "CIC", 2);
            AssertReceived(listener, "CIC", 1d, null, 1d, 2d, 2L, SplitDoubles("2d,1d"));
    
            SendMarketEvent(epService, "MSFT", 42);
            AssertReceived(listener, "MSFT", 41d, 40d, 40d, 41d, 3L, SplitDoubles("42d,41d,40d"));
    
            SendMarketEvent(epService, "CIC", 3);
            AssertReceived(listener, "CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
    
            stmt.Dispose();
        }
    
        private void AssertReceived(
            SupportUpdateListener listener,
            string symbol,
            double? prevPrice,
            double? prevPrevPrice,
            double? prevTail1Price, 
            double? prevTail2Price,
            long? countPrice,
            object[] windowPrice)
        {
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertReceived(theEvent, symbol, prevPrice, prevPrevPrice, prevTail1Price, prevTail2Price, countPrice, windowPrice);
        }

        private void AssertReceived(
            EventBean theEvent, 
            string symbol, 
            double? prevPrice, 
            double? prevPrevPrice,
            double? prevTail0Price,
            double? prevTail1Price,
            long? countPrice,
            object[] windowPrice)
        {
            Assert.AreEqual(symbol, theEvent.Get("symbol"));
            Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
            Assert.AreEqual(prevPrevPrice, theEvent.Get("prevPrevPrice"));
            Assert.AreEqual(prevTail0Price, theEvent.Get("prevTail0Price"));
            Assert.AreEqual(prevTail1Price, theEvent.Get("prevTail1Price"));
            Assert.AreEqual(countPrice, theEvent.Get("countPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(windowPrice, theEvent.Get("windowPrice").Unwrap<object>());
        }
    
        private void AssertCountAndPrice(EventBean theEvent, long total, double? price) {
            Assert.AreEqual(total, theEvent.Get("total"));
            Assert.AreEqual(price, theEvent.Get("firstPrice"));
        }
    
        private void AssertPrevCount(EPServiceProvider epService, SupportUpdateListener listener) {
            SendTimer(epService, 0);
            SendMarketEvent(epService, "IBM", 75);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 1L, 75D);
    
            SendMarketEvent(epService, "IBM", 76);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 2L, 75D);
    
            SendTimer(epService, 10000);
            SendMarketEvent(epService, "IBM", 77);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 3L, 75D);
    
            SendTimer(epService, 20000);
            SendMarketEvent(epService, "IBM", 78);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 4L, 75D);
    
            SendTimer(epService, 50000);
            SendMarketEvent(epService, "IBM", 79);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 5L, 75D);
    
            SendTimer(epService, 60000);
            Assert.AreEqual(1, listener.OldDataList.Count);
            EventBean[] oldData = listener.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            AssertCountAndPrice(oldData[0], 3L, null);
            listener.Reset();
    
            SendMarketEvent(epService, "IBM", 80);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 4L, 77D);
    
            SendTimer(epService, 65000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 70000);
            Assert.AreEqual(1, listener.OldDataList.Count);
            oldData = listener.LastOldData;
            Assert.AreEqual(1, oldData.Length);
            AssertCountAndPrice(oldData[0], 3L, null);
            listener.Reset();
    
            SendTimer(epService, 80000);
            listener.Reset();
    
            SendMarketEvent(epService, "IBM", 81);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 3L, 79D);
    
            SendTimer(epService, 120000);
            listener.Reset();
    
            SendMarketEvent(epService, "IBM", 82);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 2L, 81D);
    
            SendTimer(epService, 300000);
            listener.Reset();
    
            SendMarketEvent(epService, "IBM", 83);
            AssertCountAndPrice(listener.AssertOneGetNewAndReset(), 1L, 83D);
        }
    
        // Don't remove me, I'm dynamically referenced by EPL
        public static int? IntToLong(long? longValue) {
            if (longValue == null) {
                return null;
            } else {
                return longValue.AsInt();
            }
        }
    
        private object[] SplitDoubles(string doubleList) {
            string[] doubles = doubleList.Split(',');
            var result = new Object[doubles.Length];
            for (int i = 0; i < result.Length; i++) {
                result[i] = DoubleValue.ParseString(doubles[i]);
            }
            return result;
        }
    }
} // end of namespace
