///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewGroupWin
    {
        private const string SYMBOL_CISCO = "CSCO.O";
        private const string SYMBOL_IBM = "IBM.N";
        private const string SYMBOL_GE = "GE.N";

        private EPServiceProvider _epService;

	    private SupportUpdateListener _listener;
	    private SupportUpdateListener _priceLast3StatsListener;
	    private SupportUpdateListener _priceAllStatsListener;
	    private SupportUpdateListener _volumeLast3StatsListener;
	    private SupportUpdateListener _volumeAllStatsListener;

        [SetUp]
	    public void SetUp() {
	        _listener = new SupportUpdateListener();
	        _priceLast3StatsListener = new SupportUpdateListener();
	        _priceAllStatsListener = new SupportUpdateListener();
	        _volumeLast3StatsListener = new SupportUpdateListener();
	        _volumeAllStatsListener = new SupportUpdateListener();

	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _priceLast3StatsListener = null;
	        _priceAllStatsListener = null;
	        _volumeLast3StatsListener = null;
	        _volumeAllStatsListener = null;
	        _listener = null;
	    }

        [Test]
	    public void TestObjectArrayEvent() {
	        var fields = "p1,sp2".SplitCsv();
	        _epService.EPAdministrator.Configuration.AddEventType("MyOAEvent", new string[] {"p1","p2"}, new object[] {typeof(string), typeof(int)});
	        _epService.EPAdministrator.CreateEPL("select p1,sum(p2) as sp2 from MyOAEvent#groupwin(p1)#length(2)").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {"A", 10}, "MyOAEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 10});

	        _epService.EPRuntime.SendEvent(new object[] {"B", 11}, "MyOAEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"B", 21});

	        _epService.EPRuntime.SendEvent(new object[] {"A", 12}, "MyOAEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 33});

	        _epService.EPRuntime.SendEvent(new object[] {"A", 13}, "MyOAEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 36});
	    }

        [Test]
	    public void TestSelfJoin() {
	        // ESPER-528
	        _epService.EPAdministrator.CreateEPL(EventRepresentationChoice.MAP.GetAnnotationText() + " create schema Product (product string, productsize int)");

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var query =
	            " @Hint('reclaim_group_aged=1,reclaim_group_freq=1') select Product.product as product, Product.productsize as productsize from Product unidirectional" +
	            " left outer join Product#time(3 seconds)#groupwin(product,productsize)#size PrevProduct on Product.product=PrevProduct.product and Product.productsize=PrevProduct.productsize" +
	            " having PrevProduct.size<2";
	        _epService.EPAdministrator.CreateEPL(query);

	        // Set to larger number of executions and monitor memory
	        for (var i = 0; i < 10; i++) {
	            SendProductNew("The id of this product is deliberately very very long so that we can use up more memory per instance of this event sent into Esper " + i, i);
	            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(i * 100));
	            //if (i % 2000 == 0) {
	            //    System.out.println("i=" + i + "; Allocated: " + Runtime.getRuntime().totalMemory() / 1024 / 1024 + "; Free: " + Runtime.getRuntime().freeMemory() / 1024 / 1024);
	            //}
	        }
	    }

	    private void SendProductNew(string product, int size) {
	        IDictionary<string, object> theEvent = new Dictionary<string, object>();
	        theEvent.Put("product", product);
	        theEvent.Put("productsize", size);
	        _epService.EPRuntime.SendEvent(theEvent, "Product");
	    }

        [Test]
	    public void TestReclaimTimeWindow() {
	        SendTimer(0);

	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') " +
	                "select longPrimitive, count(*) from SupportBean#groupwin(theString)#time(3000000)");

	        for (var i = 0; i < 10; i++) {
	            var theEvent = new SupportBean(Convert.ToString(i), i);
	            _epService.EPRuntime.SendEvent(theEvent);
	        }

	        var spi = (EPServiceProviderSPI) _epService;
	        var handleCountBefore = spi.SchedulingService.ScheduleHandleCount;
	        Assert.AreEqual(10, handleCountBefore);

	        SendTimer(1000000);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        var handleCountAfter = spi.SchedulingService.ScheduleHandleCount;
	        Assert.AreEqual(1, handleCountAfter);
	    }

	    private void SendTimer(long timeInMSec) {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }

        [Test]
	    public void TestReclaimAgedHint() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();   // exclude from test, too much data
	        }

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        var epl = "@Hint('reclaim_group_aged=5,reclaim_group_freq=1') " +
	                     "select * from SupportBean#groupwin(theString)#keepall";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);

	        var maxSlots = 10;
	        var maxEventsPerSlot = 1000;
	        for (var timeSlot = 0; timeSlot < maxSlots; timeSlot++) {
	            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(timeSlot * 1000 + 1));

	            for (var i = 0; i < maxEventsPerSlot; i++) {
	                _epService.EPRuntime.SendEvent(new SupportBean("E" + timeSlot, 0));
	            }
	        }

	        EventBean[] iterator = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
	        Assert.IsTrue(iterator.Length <= 6 * maxEventsPerSlot);
	    }

        [Test]
	    public void TestInvalidGroupByNoChild() {
	        var stmtText = "select avg(price), symbol from " + typeof(SupportMarketDataBean).FullName + "#length(100)#groupwin(symbol)";

	        try {
	            _epService.EPAdministrator.CreateEPL(stmtText);
	        } catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Invalid use of the 'groupwin' view, the view requires one or more child views to group, or consider using the group-by clause [");
	        }
	    }

        [Test]
	    public void TestStats() {
	        var epAdmin = _epService.EPAdministrator;
	        var filter = "select * from " + typeof(SupportMarketDataBean).FullName;

	        var priceLast3Stats = epAdmin.CreateEPL(filter + "#groupwin(symbol)#length(3)#uni(price) order by symbol asc");
	        priceLast3Stats.AddListener(_priceLast3StatsListener);

	        var volumeLast3Stats = epAdmin.CreateEPL(filter + "#groupwin(symbol)#length(3)#uni(volume) order by symbol asc");
	        volumeLast3Stats.AddListener(_volumeLast3StatsListener);

	        var priceAllStats = epAdmin.CreateEPL(filter + "#groupwin(symbol)#uni(price) order by symbol asc");
	        priceAllStats.AddListener(_priceAllStatsListener);

	        var volumeAllStats = epAdmin.CreateEPL(filter + "#groupwin(symbol)#uni(volume) order by symbol asc");
	        volumeAllStats.AddListener(_volumeAllStatsListener);

	        var expectedList = new List<IDictionary<string, object>>();
	        for (var i = 0; i < 3; i++) {
	            expectedList.Add(new Dictionary<string, object>());
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
	        EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 11d + 1 / 6d));
	        EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_IBM, 8600d));

	        SendEvent(SYMBOL_GE, 85.5, 950);
	        SendEvent(SYMBOL_GE, 85.75, 900);
	        SendEvent(SYMBOL_GE, 89, 1250);
	        SendEvent(SYMBOL_GE, 86, 1200);
	        SendEvent(SYMBOL_GE, 85, 1150);

	        var averageGE = (88d + 85.5d + 85.75d + 89d + 86d + 85d) / 6d;
	        EPAssertionUtil.AssertPropsPerRow(_priceAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, averageGE));
	        EPAssertionUtil.AssertPropsPerRow(_volumeAllStatsListener.LastNewData, MakeMap(SYMBOL_GE, 1075d));
	        EPAssertionUtil.AssertPropsPerRow(_priceLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 86d + 2d / 3d));
	        EPAssertionUtil.AssertPropsPerRow(_volumeLast3StatsListener.LastNewData, MakeMap(SYMBOL_GE, 1200d));

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
	    }

        [Test]
	    public void TestLengthWindowGrouped() {
	        var stmtText = "select symbol, price from " + Name.Of<SupportMarketDataBean>() + "#groupwin(symbol)#length(2)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        SendEvent("IBM", 100);
	    }

        [Test]
	    public void TestExpressionGrouped() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
	        var stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBeanTimestamp#groupwin(timestamp.getDayOfWeek())#length(2)");
	        stmt.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E1", DateTimeParser.ParseDefaultMSec("2002-01-01T09:00:00.000")));
            _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E2", DateTimeParser.ParseDefaultMSec("2002-01-08T09:00:00.000")));
	        _epService.EPRuntime.SendEvent(new SupportBeanTimestamp("E3", DateTimeParser.ParseDefaultMSec("2002-01-015T09:00:00.000")));
	        Assert.AreEqual(1, _listener.GetDataListsFlattened().Second.Length);
	    }

        [Test]
	    public void TestCorrel() {
	        // further math tests can be found in the view unit test
	        var admin = _epService.EPAdministrator;
	        admin.Configuration.AddEventType("Market", typeof(SupportMarketDataBean));
	        var statement = admin.CreateEPL("select * from Market#groupwin(symbol)#length(1000000)#correl(price, volume, feed)");
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("correlation"));

	        var fields = new string[] {"symbol", "correlation", "feed"};

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 1000L, "f1"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"ABC", Double.NaN, "f1"});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 2L, "f2"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"DEF", Double.NaN, "f2"});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 2.0, 4L, "f3"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"DEF", 1.0, "f3"});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 20.0, 2000L, "f4"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"ABC", 1.0, "f4"});
	    }

        [Test]
	    public void TestLinest() {
	        // further math tests can be found in the view unit test
	        var admin = _epService.EPAdministrator;
	        admin.Configuration.AddEventType("Market", typeof(SupportMarketDataBean));
	        var statement = admin.CreateEPL("select * from Market#groupwin(symbol)#length(1000000)#linest(price, volume, feed)");
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
	        Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("dataPoints"));
	        Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("n"));
	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumX"));
	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXSq"));
	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumXY"));
	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumY"));
	        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("sumYSq"));

	        var fields = new string[] {"symbol", "slope", "YIntercept", "feed"};

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 10.0, 50000L, "f1"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"ABC", Double.NaN, Double.NaN, "f1"});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DEF", 1.0, 1L, "f2"));
	        var theEvent = listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(theEvent, fields, new object[] {"DEF", Double.NaN, Double.NaN, "f2"});
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
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"DEF", 1.0, 0.0, "f3"});

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("ABC", 11.0, 50100L, "f4"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"ABC", 100.0, 49000.0, "f4"});
	    }

	    private void SendEvent(string symbol, double price) {
	        SendEvent(symbol, price, -1);
	    }

	    private void SendEvent(string symbol, double price, long volume) {
	        var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
	        _epService.EPRuntime.SendEvent(theEvent);
	    }

	    private IList<IDictionary<string, object>> MakeMap(string symbol, double average) {
	        IDictionary<string, object> result = new Dictionary<string, object>();

	        result.Put("symbol", symbol);
	        result.Put("average", average);

	        var vec = new List<IDictionary<string, object>>();
	        vec.Add(result);

	        return vec;
	    }
	}
} // end of namespace
