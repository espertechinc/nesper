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
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewGroupBy
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _priceLast3StatsListener = new SupportUpdateListener();
            _priceAllStatsListener = new SupportUpdateListener();
            _volumeLast3StatsListener = new SupportUpdateListener();
            _volumeAllStatsListener = new SupportUpdateListener();

            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _priceLast3StatsListener = null;
            _priceAllStatsListener = null;
            _volumeLast3StatsListener = null;
            _volumeAllStatsListener = null;
            _listener = null;
        }

        #endregion

        private const String SYMBOL_CISCO = "CSCO.O";
        private const String SYMBOL_IBM = "IBM.Count";
        private const String SYMBOL_GE = "GE.Count";

        private EPServiceProvider _epService;

        private SupportUpdateListener _listener;
        private SupportUpdateListener _priceLast3StatsListener;
        private SupportUpdateListener _priceAllStatsListener;
        private SupportUpdateListener _volumeLast3StatsListener;
        private SupportUpdateListener _volumeAllStatsListener;

        private void SendProductNew(String product, int size)
        {
            IDictionary<String, Object> theEvent = new Dictionary<String, Object>();
            theEvent["product"] = product;
            theEvent["productsize"] = size;
            _epService.EPRuntime.SendEvent(theEvent, "Product");
        }

        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void SendEvent(String symbol, double price)
        {
            SendEvent(symbol, price, -1);
        }

        private void SendEvent(String symbol, double price, long volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private List<IDictionary<String, Object>> MakeMap(String symbol, double average)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();

            result["Symbol"] = symbol;
            result["average"] = average;

            var vec = new List<IDictionary<String, Object>>();
            vec.Add(result);

            return vec;
        }

        [Test]
        public void TestCorrel()
        {
            // further math tests can be found in the view unit test
            EPAdministrator admin = _epService.EPAdministrator;
            admin.Configuration.AddEventType("Market", typeof (SupportMarketDataBean));
            EPStatement statement =
                admin.CreateEPL(
                    "select * from Market.std:groupwin(Symbol).win:length(1000000).stat:correl(Price, Volume, feed)");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("correlation"));

            var fields = new String[] {"Symbol", "correlation", "feed"};

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 1000L, "f1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"ABC", Double.NaN, "f1"});

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 2L, "f2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"DEF", Double.NaN, "f2"});

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 2.0, 4L, "f3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {"DEF", 1.0, "f3"});

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 20.0, 2000L, "f4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {"ABC", 1.0, "f4"});
        }

        [Test]
        public void TestInvalidGroupByNoChild()
        {
            String stmtText = "select avg(Price), Symbol from " + typeof (SupportMarketDataBean).FullName +
                              ".win:length(100).std:groupwin(Symbol)";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Invalid use of the 'std:groupwin' view, the view requires one or more child views to group, or consider using the group-by clause [select avg(Price), Symbol from com.espertech.esper.support.bean.SupportMarketDataBean.win:length(100).std:groupwin(Symbol)]",
                    ex.Message);
            }
        }

        [Test]
        public void TestLengthWindowGrouped()
        {
            String stmtText = "select Symbol, Price from " + typeof (SupportMarketDataBean).FullName +
                              ".std:groupwin(Symbol).win:length(2)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEvent("IBM", 100);
        }

        [Test]
        public void TestExpressionGrouped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanTimestamp>();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBeanTimestamp.std:groupwin(timestamp.getDayOfWeek()).win:length(2)");
            stmt.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E1", DateTimeParser.ParseDefaultMSec("2002-01-01T9:0:00.000")));
            _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E2", DateTimeParser.ParseDefaultMSec("2002-01-08T9:0:00.000")));
            _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E3", DateTimeParser.ParseDefaultMSec("2002-01-015T9:0:00.000")));
            Assert.AreEqual(1, _listener.GetDataListsFlattened().Second.Length);
        }

        [Test]
        public void TestLinest()
        {
            // further math tests can be found in the view unit test
            var admin = _epService.EPAdministrator;
            admin.Configuration.AddEventType("Market", typeof (SupportMarketDataBean));
            var statement = admin.CreateEPL("select * from Market.std:groupwin(Symbol).win:length(1000000).stat:linest(Price, Volume, feed)");
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("slope"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YIntercept"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("XAverage"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("XStandardDeviationPop"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("XStandardDeviationSample"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("XSum"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("XVariance"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YAverage"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YStandardDeviationPop"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YStandardDeviationSample"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YSum"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("YVariance"));
            Assert.AreEqual(typeof (long?), statement.EventType.GetPropertyType("dataPoints"));
            Assert.AreEqual(typeof (long?), statement.EventType.GetPropertyType("n"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("sumX"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("sumXSq"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("sumXY"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("sumY"));
            Assert.AreEqual(typeof (double?), statement.EventType.GetPropertyType("sumYSq"));

            var fields = new String[] {"Symbol", "slope", "YIntercept", "feed"};

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 50000L, "f1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"ABC", Double.NaN, Double.NaN, "f1"});

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 1L, "f2"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fields, new Object[] {"DEF", Double.NaN, Double.NaN, "f2"});
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

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 2.0, 2L, "f3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {"DEF", 1.0, 0.0, "f3"});

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 11.0, 50100L, "f4"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"ABC", 100.0, 49000.0, "f4"});
        }

        [Test]
        public void TestObjectArrayEvent()
        {
            String[] fields = "p1,sp2".Split(',');

            EPAdministrator administrator = _epService.EPAdministrator;
            EPRuntime runtime = _epService.EPRuntime;

            administrator.Configuration.AddEventType(
                "MyOAEvent",
                new String[]
                {
                    "p1", "p2"
                },
                new Object[]
                {
                    typeof (string), typeof (int)
                }
                );

            administrator.CreateEPL("select p1,sum(p2) as sp2 from MyOAEvent.std:groupwin(p1).win:length(2)")
                .Events += _listener.Update;

            runtime.SendEvent(new Object[]
            {
                "A", 10
            }
                              , "MyOAEvent");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "A", 10
                                        }
                );

            runtime.SendEvent(new Object[]
            {
                "B", 11
            }
                              , "MyOAEvent");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "B", 21
                                        }
                );

            runtime.SendEvent(new Object[]
            {
                "A", 12
            }
                              , "MyOAEvent");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "A", 33
                                        }
                );

            runtime.SendEvent(new Object[]
            {
                "A", 13
            }
                              , "MyOAEvent");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "A", 36
                                        }
                );
        }

        [Test]
        public void TestReclaimAgedHint()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // exclude from test, too much data

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));
            String epl = "@Hint('reclaim_group_aged=5,reclaim_group_freq=1') " +
                         "select * from SupportBean.std:groupwin(TheString).win:keepall()";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            int maxSlots = 10;
            int maxEventsPerSlot = 1000;
            for (int timeSlot = 0; timeSlot < maxSlots; timeSlot++)
            {
                _epService.EPRuntime.SendEvent(new CurrentTimeEvent(timeSlot*1000 + 1));

                for (int i = 0; i < maxEventsPerSlot; i++)
                {
                    _epService.EPRuntime.SendEvent(new SupportBean("E" + timeSlot, 0));
                }
            }

            EventBean[] iterator = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
            Assert.IsTrue(iterator.Length <= 6*maxEventsPerSlot);
        }

        [Test]
        public void TestReclaimTimeWindow()
        {
            SendTimer(0);

            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));
            _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') " +
                                                 "select LongPrimitive, count(*) from SupportBean.std:groupwin(TheString).win:time(3000000)");

            for (int i = 0; i < 10; i++)
            {
                var theEvent = new SupportBean(Convert.ToString(i), i);
                _epService.EPRuntime.SendEvent(theEvent);
            }

            var spi = (EPServiceProviderSPI) _epService;
            int handleCountBefore = spi.SchedulingService.ScheduleHandleCount;
            Assert.AreEqual(10, handleCountBefore);

            SendTimer(1000000);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            int handleCountAfter = spi.SchedulingService.ScheduleHandleCount;
            Assert.AreEqual(1, handleCountAfter);
        }

        [Test]
        public void TestSelfJoin()
        {
            // ESPER-528
            _epService.EPAdministrator.CreateEPL(
                EventRepresentationEnum.MAP.GetAnnotationText() +
                " create schema Product (product string, productsize int)");

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            String query =
                " @Hint('reclaim_group_aged=1,reclaim_group_freq=1') select Product.product as product, Product.productsize as productsize from Product unidirectional" +
                " left outer join Product.win:time(3 seconds).std:groupwin(product,productsize).std:size() PrevProduct on Product.product=PrevProduct.product and Product.productsize=PrevProduct.productsize" +
                " having PrevProduct.size<2";
            _epService.EPAdministrator.CreateEPL(query);

            // Set to larger number of executions and monitor memory
            for (int i = 0; i < 10; i++)
            {
                SendProductNew(
                    "The id of this product is deliberately very very long so that we can use up more memory per instance of this event sent into Esper " +
                    i, i);
                _epService.EPRuntime.SendEvent(new CurrentTimeEvent(i*100));
                //if (i % 2000 == 0) {
                //    Console.WriteLine("i=" + i + "; Allocated: " + Runtime.Runtime.TotalMemory() / 1024 / 1024 + "; Free: " + Runtime.Runtime.FreeMemory() / 1024 / 1024);
                //}
            }
        }

        [Test]
        public void TestStats()
        {
            EPAdministrator epAdmin = _epService.EPAdministrator;
            String filter = "select * from " + typeof (SupportMarketDataBean).FullName;

            EPStatement priceLast3Stats =
                epAdmin.CreateEPL(filter + ".std:groupwin(Symbol).win:length(3).stat:uni(Price) order by Symbol asc");
            priceLast3Stats.Events += _priceLast3StatsListener.Update;

            EPStatement volumeLast3Stats =
                epAdmin.CreateEPL(filter + ".std:groupwin(Symbol).win:length(3).stat:uni(Volume) order by Symbol asc");
            volumeLast3Stats.Events += _volumeLast3StatsListener.Update;

            EPStatement priceAllStats = epAdmin.CreateEPL(filter + ".std:groupwin(Symbol).stat:uni(Price) order by Symbol asc");
            priceAllStats.Events += _priceAllStatsListener.Update;

            EPStatement volumeAllStats = epAdmin.CreateEPL(filter + ".std:groupwin(Symbol).stat:uni(Volume) order by Symbol asc");
            volumeAllStats.Events += _volumeAllStatsListener.Update;

            var expectedList = new List<IDictionary<String, Object>>();
            for (int i = 0; i < 3; i++)
            {
                expectedList.Add(new Dictionary<String, Object>());
            }

            SendEvent(SYMBOL_CISCO, 25, 50000);
            SendEvent(SYMBOL_CISCO, 26, 60000);
            SendEvent(SYMBOL_IBM, 10, 8000);
            SendEvent(SYMBOL_IBM, 10.5, 8200);
            SendEvent(SYMBOL_GE, 88, 1000);

            EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 88));
            EPAssertionUtil.AssertPropsPerRow(_priceAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 88));
            EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 1000));
            EPAssertionUtil.AssertPropsPerRow(_volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 1000));

            SendEvent(SYMBOL_CISCO, 27, 70000);
            SendEvent(SYMBOL_CISCO, 28, 80000);

            EPAssertionUtil.AssertPropsPerRow(_priceAllStatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 26.5d));
            EPAssertionUtil.AssertPropsPerRow(_volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 65000d));
            EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 27d));
            EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_CISCO, 70000d));

            SendEvent(SYMBOL_IBM, 11, 8700);
            SendEvent(SYMBOL_IBM, 12, 8900);

            EPAssertionUtil.AssertPropsPerRow(_priceAllStatsListener.LastNewData, MakeMap(SYMBOL_IBM, 10.875d));
            EPAssertionUtil.AssertPropsPerRow(_volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_IBM, 8450d));
            EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 11d + 1/6d));
            EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 8600d));

            SendEvent(SYMBOL_GE, 85.5, 950);
            SendEvent(SYMBOL_GE, 85.75, 900);
            SendEvent(SYMBOL_GE, 89, 1250);
            SendEvent(SYMBOL_GE, 86, 1200);
            SendEvent(SYMBOL_GE, 85, 1150);

            double averageGE = (88d + 85.5d + 85.75d + 89d + 86d + 85d)/6d;
            EPAssertionUtil.AssertPropsPerRow(_priceAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, averageGE));
            EPAssertionUtil.AssertPropsPerRow(_volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 1075d));
            EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 86d + 2d/3d));
            EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 1200d));

            // Check iterator results
            expectedList[0]["Symbol"] = SYMBOL_CISCO;
            expectedList[0]["average"] = 26.5d;
            expectedList[1]["Symbol"] = SYMBOL_GE;
            expectedList[1]["average"] = averageGE;
            expectedList[2]["Symbol"] = SYMBOL_IBM;
            expectedList[2]["average"] = 10.875d;
            EPAssertionUtil.AssertPropsPerRow(priceAllStats.GetEnumerator(), expectedList);

            expectedList[0]["Symbol"] = SYMBOL_CISCO;
            expectedList[0]["average"] = 27d;
            expectedList[1]["Symbol"] = SYMBOL_GE;
            expectedList[1]["average"] = 86d + 2d / 3d;
            expectedList[2]["Symbol"] = SYMBOL_IBM;
            expectedList[2]["average"] = 11d + 1 / 6d; 
            EPAssertionUtil.AssertPropsPerRow(priceLast3Stats.GetEnumerator(), expectedList);
        }
    }
}
