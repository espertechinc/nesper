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
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertTrue;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupWin : RegressionExecution {
        private static readonly string SYMBOL_CISCO = "CSCO.O";
        private static readonly string SYMBOL_IBM = "IBM.N";
        private static readonly string SYMBOL_GE = "GE.N";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionObjectArrayEvent(epService);
            RunAssertionSelfJoin(epService);
            RunAssertionReclaimTimeWindow(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionReclaimAgedHint(epService);
            }
            RunAssertionInvalidGroupByNoChild(epService);
            RunAssertionStats(epService);
            RunAssertionLengthWindowGrouped(epService);
            RunAssertionExpressionGrouped(epService);
            RunAssertionCorrel(epService);
            RunAssertionLinest(epService);
        }
    
        private void RunAssertionObjectArrayEvent(EPServiceProvider epService) {
            string[] fields = "p1,sp2".Split(',');
            epService.EPAdministrator.Configuration.AddEventType("MyOAEvent", new string[]{"p1", "p2"}, new Object[]{typeof(string), typeof(int)});
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select p1,sum(p2) as sp2 from MyOAEvent#Groupwin(p1)#length(2)").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Object[]{"A", 10}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"A", 10});
    
            epService.EPRuntime.SendEvent(new Object[]{"B", 11}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"B", 21});
    
            epService.EPRuntime.SendEvent(new Object[]{"A", 12}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"A", 33});
    
            epService.EPRuntime.SendEvent(new Object[]{"A", 13}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"A", 36});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSelfJoin(EPServiceProvider epService) {
            // ESPER-528
            epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create schema Product (product string, productsize int)");
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string query =
                    " @Hint('reclaim_group_aged=1,reclaim_group_freq=1') select Product.product as product, Product.productsize as productsize from Product unidirectional" +
                            " left outer join Product#Time(3 seconds)#Groupwin(product,productsize)#size PrevProduct on Product.product=PrevProduct.product and Product.productsize=PrevProduct.productsize" +
                            " having PrevProduct.size<2";
            epService.EPAdministrator.CreateEPL(query);
    
            // Set to larger number of executions and monitor memory
            for (int i = 0; i < 10; i++) {
                SendProductNew(epService, "The id of this product is deliberately very very long so that we can use up more memory per instance of this event sent into Esper " + i, i);
                epService.EPRuntime.SendEvent(new CurrentTimeEvent(i * 100));
                //if (i % 2000 == 0) {
                //    Log.Info("i=" + i + "; Allocated: " + Runtime.Runtime.TotalMemory() / 1024 / 1024 + "; Free: " + Runtime.Runtime.FreeMemory() / 1024 / 1024);
                //}
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionReclaimTimeWindow(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') " +
                    "select longPrimitive, count(*) from SupportBean#Groupwin(theString)#Time(3000000)");
    
            for (int i = 0; i < 10; i++) {
                var theEvent = new SupportBean(Convert.ToString(i), i);
                epService.EPRuntime.SendEvent(theEvent);
            }
    
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            int handleCountBefore = spi.SchedulingService.ScheduleHandleCount;
            Assert.AreEqual(10, handleCountBefore);
    
            SendTimer(epService, 1000000);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            int handleCountAfter = spi.SchedulingService.ScheduleHandleCount;
            Assert.AreEqual(1, handleCountAfter);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionReclaimAgedHint(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "@Hint('reclaim_group_aged=5,reclaim_group_freq=1') " +
                    "select * from SupportBean#Groupwin(theString)#keepall";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
    
            int maxSlots = 10;
            int maxEventsPerSlot = 1000;
            for (int timeSlot = 0; timeSlot < maxSlots; timeSlot++) {
                epService.EPRuntime.SendEvent(new CurrentTimeEvent(timeSlot * 1000 + 1));
    
                for (int i = 0; i < maxEventsPerSlot; i++) {
                    epService.EPRuntime.SendEvent(new SupportBean("E" + timeSlot, 0));
                }
            }
    
            EventBean[] iterator = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.IsTrue(iterator.Length <= 6 * maxEventsPerSlot);
            stmt.Dispose();
        }
    
        private void RunAssertionInvalidGroupByNoChild(EPServiceProvider epService) {
            string stmtText = "select avg(price), symbol from " + typeof(SupportMarketDataBean).FullName + "#length(100)#Groupwin(symbol)";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Invalid use of the 'groupwin' view, the view requires one or more child views to group, or consider using the group-by clause [");
            }
        }
    
        private void RunAssertionStats(EPServiceProvider epService) {
            EPAdministrator epAdmin = epService.EPAdministrator;
            string filter = "select * from " + typeof(SupportMarketDataBean).FullName;
    
            EPStatement priceLast3Stats = epAdmin.CreateEPL(filter + "#Groupwin(symbol)#length(3)#Uni(price) order by symbol asc");
            var priceLast3StatsListener = new SupportUpdateListener();
            priceLast3Stats.AddListener(priceLast3StatsListener);
    
            EPStatement volumeLast3Stats = epAdmin.CreateEPL(filter + "#Groupwin(symbol)#length(3)#Uni(volume) order by symbol asc");
            var volumeLast3StatsListener = new SupportUpdateListener();
            volumeLast3Stats.AddListener(volumeLast3StatsListener);
    
            EPStatement priceAllStats = epAdmin.CreateEPL(filter + "#Groupwin(symbol)#Uni(price) order by symbol asc");
            var priceAllStatsListener = new SupportUpdateListener();
            priceAllStats.AddListener(priceAllStatsListener);
    
            EPStatement volumeAllStats = epAdmin.CreateEPL(filter + "#Groupwin(symbol)#Uni(volume) order by symbol asc");
            var volumeAllStatsListener = new SupportUpdateListener();
            volumeAllStats.AddListener(volumeAllStatsListener);
    
            var expectedList = new Vector<>();
            for (int i = 0; i < 3; i++) {
                expectedList.Add(new Dictionary<string, object>());
            }
    
            SendEvent(epService, SYMBOL_CISCO, 25, 50000);
            SendEvent(epService, SYMBOL_CISCO, 26, 60000);
            SendEvent(epService, SYMBOL_IBM, 10, 8000);
            SendEvent(epService, SYMBOL_IBM, 10.5, 8200);
            SendEvent(epService, SYMBOL_GE, 88, 1000);
    
            EPAssertionUtil.AssertPropsPerRow(priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 88));
            EPAssertionUtil.AssertPropsPerRow(priceAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 88));
            EPAssertionUtil.AssertPropsPerRow(volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 1000));
            EPAssertionUtil.AssertPropsPerRow(volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 1000));
    
            SendEvent(epService, SYMBOL_CISCO, 27, 70000);
            SendEvent(epService, SYMBOL_CISCO, 28, 80000);
    
            EPAssertionUtil.AssertPropsPerRow(priceAllStatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 26.5d));
            EPAssertionUtil.AssertPropsPerRow(volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 65000d));
            EPAssertionUtil.AssertPropsPerRow(priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 27d));
            EPAssertionUtil.AssertPropsPerRow(volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 70000d));
    
            SendEvent(epService, SYMBOL_IBM, 11, 8700);
            SendEvent(epService, SYMBOL_IBM, 12, 8900);
    
            EPAssertionUtil.AssertPropsPerRow(priceAllStatsListener.LastNewData, MakeMap(SYMBOL_IBM, 10.875d));
            EPAssertionUtil.AssertPropsPerRow(volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_IBM, 8450d));
            EPAssertionUtil.AssertPropsPerRow(priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 11d + 1 / 6d));
            EPAssertionUtil.AssertPropsPerRow(volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 8600d));
    
            SendEvent(epService, SYMBOL_GE, 85.5, 950);
            SendEvent(epService, SYMBOL_GE, 85.75, 900);
            SendEvent(epService, SYMBOL_GE, 89, 1250);
            SendEvent(epService, SYMBOL_GE, 86, 1200);
            SendEvent(epService, SYMBOL_GE, 85, 1150);
    
            double averageGE = (88d + 85.5d + 85.75d + 89d + 86d + 85d) / 6d;
            EPAssertionUtil.AssertPropsPerRow(priceAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, averageGE));
            EPAssertionUtil.AssertPropsPerRow(volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 1075d));
            EPAssertionUtil.AssertPropsPerRow(priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 86d + 2d / 3d));
            EPAssertionUtil.AssertPropsPerRow(volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 1200d));
    
            // Check iterator results
            expectedList[0].Put("symbol", SYMBOL_CISCO);
            expectedList[0].Put("average", 26.5d);
            expectedList[1].Put("symbol", SYMBOL_GE);
            expectedList[1].Put("average", averageGE);
            expectedList[2].Put("symbol", SYMBOL_IBM);
            expectedList[2].Put("average", 10.875d);
            EPAssertionUtil.AssertPropsPerRow(priceAllStats.GetEnumerator(), expectedList);
    
            expectedList[0].Put("symbol", SYMBOL_CISCO);
            expectedList[0].Put("average", 27d);
            expectedList[1].Put("symbol", SYMBOL_GE);
            expectedList[1].Put("average", 86d + 2d / 3d);
            expectedList[2].Put("symbol", SYMBOL_IBM);
            expectedList[2].Put("average", 11d + 1 / 6d);
            EPAssertionUtil.AssertPropsPerRow(priceLast3Stats.GetEnumerator(), expectedList);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLengthWindowGrouped(EPServiceProvider epService) {
            string stmtText = "select symbol, price from " + typeof(SupportMarketDataBean).FullName + "#Groupwin(symbol)#length(2)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendEvent(epService, "IBM", 100);
    
            stmt.Dispose();
        }
    
        private void RunAssertionExpressionGrouped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBeanTimestamp#Groupwin(timestamp.DayOfWeek)#length(2)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E1", DateTimeParser.ParseDefaultMSec("2002-01-01T09:0:00.000")));
            epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E2", DateTimeParser.ParseDefaultMSec("2002-01-08T09:0:00.000")));
            epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E3", DateTimeParser.ParseDefaultMSec("2002-01-015T09:0:00.000")));
            Assert.AreEqual(1, listener.GetDataListsFlattened().Second.Length);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCorrel(EPServiceProvider epService) {
            // further math tests can be found in the view unit test
            EPAdministrator admin = epService.EPAdministrator;
            admin.Configuration.AddEventType("Market", typeof(SupportMarketDataBean));
            EPStatement statement = admin.CreateEPL("select * from Market#Groupwin(symbol)#length(1000000)#Correl(price, volume, feed)");
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("correlation"));
    
            var fields = new string[]{"symbol", "correlation", "feed"};
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 1000L, "f1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"ABC", Double.NaN, "f1"});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 2L, "f2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"DEF", Double.NaN, "f2"});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 2.0, 4L, "f3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"DEF", 1.0, "f3"});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 20.0, 2000L, "f4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"ABC", 1.0, "f4"});
    
            statement.Destroy();
        }
    
        private void RunAssertionLinest(EPServiceProvider epService) {
            // further math tests can be found in the view unit test
            EPAdministrator admin = epService.EPAdministrator;
            admin.Configuration.AddEventType("Market", typeof(SupportMarketDataBean));
            EPStatement statement = admin.CreateEPL("select * from Market#Groupwin(symbol)#length(1000000)#Linest(price, volume, feed)");
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("slope"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YIntercept"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XAverage"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XStandardDeviationPop"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XStandardDeviationSample"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XSum"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("XVariance"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YAverage"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YStandardDeviationPop"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YStandardDeviationSample"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YSum"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("YVariance"));
            Assert.AreEqual(typeof(long), statement.EventType.GetPropertyType("dataPoints"));
            Assert.AreEqual(typeof(long), statement.EventType.GetPropertyType("n"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumX"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXSq"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXY"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumY"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumYSq"));
    
            var fields = new string[]{"symbol", "slope", "YIntercept", "feed"};
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 50000L, "f1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"ABC", Double.NaN, Double.NaN, "f1"});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 1L, "f2"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fields, new Object[]{"DEF", Double.NaN, Double.NaN, "f2"});
            Assert.AreEqual(1d, theEvent.Get("XAverage"));
            Assert.AreEqual(0d, theEvent.Get("XStandardDeviationPop"));
            Assert.AreEqual(Double.NaN, theEvent.Get("XStandardDeviationSample"));
            Assert.AreEqual(1d, theEvent.Get("XSum"));
            Assert.AreEqual(Double.NaN, theEvent.Get("XVariance"));
            Assert.AreEqual(1d, theEvent.Get("YAverage"));
            Assert.AreEqual(0d, theEvent.Get("YStandardDeviationPop"));
            Assert.AreEqual(Double.NaN, theEvent.Get("YStandardDeviationSample"));
            Assert.AreEqual(1d, theEvent.Get("YSum"));
            Assert.AreEqual(Double.NaN, theEvent.Get("YVariance"));
            Assert.AreEqual(1L, theEvent.Get("dataPoints"));
            Assert.AreEqual(1L, theEvent.Get("n"));
            Assert.AreEqual(1d, theEvent.Get("sumX"));
            Assert.AreEqual(1d, theEvent.Get("sumXSq"));
            Assert.AreEqual(1d, theEvent.Get("sumXY"));
            Assert.AreEqual(1d, theEvent.Get("sumY"));
            Assert.AreEqual(1d, theEvent.Get("sumYSq"));
            // above computed values tested in more detail in RegressionBean test
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 2.0, 2L, "f3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"DEF", 1.0, 0.0, "f3"});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 11.0, 50100L, "f4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"ABC", 100.0, 49000.0, "f4"});
    
            statement.Destroy();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            SendEvent(epService, symbol, price, -1);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private List<IDictionary<string, Object>> MakeMap(string symbol, double average) {
            var result = new Dictionary<string, object>();
    
            result.Put("symbol", symbol);
            result.Put("average", average);
    
            var vec = new List<>();
            vec.Add(result);
    
            return vec;
        }
    
        private void SendProductNew(EPServiceProvider epService, string product, int size) {
            var theEvent = new Dictionary<string, object>();
            theEvent.Put("product", product);
            theEvent.Put("productsize", size);
            epService.EPRuntime.SendEvent(theEvent, "Product");
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
