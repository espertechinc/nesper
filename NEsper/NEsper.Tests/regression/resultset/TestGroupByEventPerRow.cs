///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestGroupByEventPerRow 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestCriteriaByDotMethod() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var epl = "select sb.get_LongPrimitive() as c0, sum(IntPrimitive) as c1 from SupportBean.win:length_batch(2) as sb group by sb.get_TheString()";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        MakeSendSupportBean("E1", 10, 100L);
	        MakeSendSupportBean("E1", 20, 200L);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new object[][] { new object[] { 100L, 30 }, new object[] { 200L, 30 } });
	    }

        [Test]
	    public void TestIterateUnbound() {
	        var fields = "c0,c1".Split(',');
	        var epl = "@IterableUnbound select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] {"E1", 10},  new object[] {"E2", 20}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] {"E1", 21},  new object[] {"E2", 20}});
	    }

        [Test]
	    public void TestUnaggregatedHaving() {
	        var stmt = _epService.EPAdministrator.CreateEPL("select TheString from SupportBean group by TheString having IntPrimitive > 5");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 5));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
	        Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("TheString"));

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
	        Assert.AreEqual("E3", _listener.AssertOneGetNewAndReset().Get("TheString"));
	    }

        [Test]
	    public void TestWildcard() {

	        // test no output limit
	        var fields = "TheString, IntPrimitive, minval".Split(',');
	        var epl = "select *, min(IntPrimitive) as minval from SupportBean.win:length(2) group by TheString";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(epl);
	        selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 10, 10});

	        _epService.EPRuntime.SendEvent(new SupportBean("G1", 9));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 9, 9});

	        _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 11, 9});
	    }

        [Test]
	    public void TestAggregationOverGroupedProps()
	    {
	        // test for ESPER-185
	        var fields = "Volume,Symbol,Price,mycount".Split(',');
	        var viewExpr = "select irstream Volume,Symbol,Price,count(Price) as mycount " +
	                          "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
	                          "group by Symbol, Price";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        SendEvent(SYMBOL_DELL, 1000, 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1000L, "DELL", 10.0, 1L});
	        EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][]{ new object[] {1000L, "DELL", 10.0, 1L}});

	        SendEvent(SYMBOL_DELL, 900, 11);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{900L, "DELL", 11.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1000L, "DELL", 10.0, 1L }, new object[] { 900L, "DELL", 11.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_DELL, 1500, 10);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{1500L, "DELL", 10.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L }, new object[] { 1500L, "DELL", 10.0, 2L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 500, 5);
	        Assert.AreEqual(1, _listener.NewDataList.Count);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{500L, "IBM", 5.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L }, new object[] { 1500L, "DELL", 10.0, 2L }, new object[] { 500L, "IBM", 5.0, 1L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 600, 5);
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{600L, "IBM", 5.0, 2L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1000L, "DELL", 10.0, 2L }, new object[] { 900L, "DELL", 11.0, 1L }, new object[] { 1500L, "DELL", 10.0, 2L }, new object[] { 500L, "IBM", 5.0, 2L }, new object[] { 600L, "IBM", 5.0, 2L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 500, 5);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{500L, "IBM", 5.0, 3L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{1000L, "DELL", 10.0, 1L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 900L, "DELL", 11.0, 1L }, new object[] { 1500L, "DELL", 10.0, 1L }, new object[] { 500L, "IBM", 5.0, 3L }, new object[] { 600L, "IBM", 5.0, 3L }, new object[] { 500L, "IBM", 5.0, 3L } });
	        _listener.Reset();

	        SendEvent(SYMBOL_IBM, 600, 5);
	        EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new object[]{600L, "IBM", 5.0, 4L});
	        EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new object[]{900L, "DELL", 11.0, 0L});
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new object[][] { new object[] { 1500L, "DELL", 10.0, 1L }, new object[] { 500L, "IBM", 5.0, 4L }, new object[] { 600L, "IBM", 5.0, 4L }, new object[] { 500L, "IBM", 5.0, 4L }, new object[] { 600L, "IBM", 5.0, 4L } });
	        _listener.Reset();
	    }

        [Test]
	    public void TestSumOneView()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        var viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
                              "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        var viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
                              "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
                                        typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	                          "  and one.TheString = two.Symbol " +
	                          "group by Symbol";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestInsertInto()
	    {
	        var listenerOne = new SupportUpdateListener();
            var eventType = typeof(SupportMarketDataBean).FullName;
	        var stmt = " select Symbol as Symbol, avg(Price) as average, sum(Volume) as sumation from " + eventType + ".win:length(3000)";
	        var statement = _epService.EPAdministrator.CreateEPL(stmt);
	        statement.AddListener(listenerOne);

	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10D, 20000L, null));
	        var eventBean = listenerOne.LastNewData[0];
	        Assert.AreEqual("IBM", eventBean.Get("Symbol"));
	        Assert.AreEqual(10d, eventBean.Get("average"));
	        Assert.AreEqual(20000L, eventBean.Get("sumation"));

	        // create insert into statements
	        stmt =  "insert into StockAverages select Symbol as Symbol, avg(Price) as average, sum(Volume) as sumation " +
	                    "from " + eventType + ".win:length(3000)";
	        statement = _epService.EPAdministrator.CreateEPL(stmt);
	        var listenerTwo = new SupportUpdateListener();
	        statement.AddListener(listenerTwo);

	        stmt = " select * from StockAverages";
	        statement = _epService.EPAdministrator.CreateEPL(stmt);
	        var listenerThree = new SupportUpdateListener();
	        statement.AddListener(listenerThree);

	        // send event
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 20D, 40000L, null));
	        eventBean = listenerOne.LastNewData[0];
	        Assert.AreEqual("IBM", eventBean.Get("Symbol"));
	        Assert.AreEqual(15d, eventBean.Get("average"));
	        Assert.AreEqual(60000L, eventBean.Get("sumation"));

	        Assert.AreEqual(1, listenerThree.NewDataList.Count);
	        Assert.AreEqual(1, listenerThree.LastNewData.Length);
	        eventBean = listenerThree.LastNewData[0];
	        Assert.AreEqual("IBM", eventBean.Get("Symbol"));
	        Assert.AreEqual(20d, eventBean.Get("average"));
	        Assert.AreEqual(40000L, eventBean.Get("sumation"));
	    }

	    private void RunAssertion(EPStatement selectTestView)
	    {
	        var fields = new string[] {"Symbol", "Volume", "mySum"};
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);

	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));

	        SendEvent(SYMBOL_DELL, 10000, 51);
	        AssertEvents(SYMBOL_DELL, 10000, 51);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[] {"DELL", 10000L, 51d}});

	        SendEvent(SYMBOL_DELL, 20000, 52);
	        AssertEvents(SYMBOL_DELL, 20000, 103);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[] {"DELL", 10000L, 103d}, new object[] {"DELL", 20000L, 103d}});

	        SendEvent(SYMBOL_IBM, 30000, 70);
	        AssertEvents(SYMBOL_IBM, 30000, 70);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[] {"DELL", 10000L, 103d}, new object[] {"DELL", 20000L, 103d}, new object[] {"IBM", 30000L, 70d}});

	        SendEvent(SYMBOL_IBM, 10000, 20);
	        AssertEvents(SYMBOL_DELL, 10000, 52, SYMBOL_IBM, 10000, 90);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[] {"DELL", 20000L, 52d}, new object[] {"IBM", 30000L, 90d}, new object[] {"IBM", 10000L, 90d}});

	        SendEvent(SYMBOL_DELL, 40000, 45);
	        AssertEvents(SYMBOL_DELL, 20000, 45, SYMBOL_DELL, 40000, 45);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new object[][]{
	                new object[] {"IBM", 10000L, 90d}, new object[] {"IBM", 30000L, 90d}, new object[] {"DELL", 40000L, 45d}});
	    }

	    private void AssertEvents(string symbol, long volume, double sum)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(symbol, newData[0].Get("Symbol"));
	        Assert.AreEqual(volume, newData[0].Get("Volume"));
	        Assert.AreEqual(sum, newData[0].Get("mySum"));

	        _listener.Reset();
	        Assert.IsFalse(_listener.IsInvoked);
	    }

	    private void AssertEvents(string symbolOld, long volumeOld, double sumOld,
	                              string symbolNew, long volumeNew, double sumNew)
	    {
	        var oldData = _listener.LastOldData;
	        var newData = _listener.LastNewData;

	        Assert.AreEqual(1, oldData.Length);
	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(symbolOld, oldData[0].Get("Symbol"));
	        Assert.AreEqual(volumeOld, oldData[0].Get("Volume"));
	        Assert.AreEqual(sumOld, oldData[0].Get("mySum"));

	        Assert.AreEqual(symbolNew, newData[0].Get("Symbol"));
	        Assert.AreEqual(volumeNew, newData[0].Get("Volume"));
	        Assert.AreEqual(sumNew, newData[0].Get("mySum"));

	        _listener.Reset();
	        Assert.IsFalse(_listener.IsInvoked);
	    }

	    private void SendEvent(string symbol, long volume, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private SupportBean MakeSendSupportBean(string theString, int intPrimitive, long longPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	        return bean;
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
