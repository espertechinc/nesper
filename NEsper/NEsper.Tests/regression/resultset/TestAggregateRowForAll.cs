///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateRowForAll 
	{
	    private const string JOIN_KEY = "KEY";

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private EPStatement _selectTestView;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestSumOneView()
	    {
	        var viewExpr = "select irstream sum(longBoxed) as mySum " +
	                          "from " + typeof(SupportBean).Name + ".win:time(10 sec)";

	        SendTimerEvent(0);
	        _selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        _selectTestView.AddListener(_listener);

	        RunAssert();
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        var viewExpr = "select irstream sum(longBoxed) as mySum " +
	                          "from " + typeof(SupportBeanString).Name + ".win:keepall() as one, " +
	                                    typeof(SupportBean).Name + ".win:time(10 sec) as two " +
	                          "where one.theString = two.theString";

	        SendTimerEvent(0);
	        _selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        _selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

	        RunAssert();
	    }

	    private void RunAssert()
	    {
	        // assert select result type
	        Assert.AreEqual(typeof(long?), _selectTestView.EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { null } });

	        SendTimerEvent(0);
	        SendEvent(10);
	        Assert.AreEqual(10L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { 10L } });

	        SendTimerEvent(5000);
	        SendEvent(15);
	        Assert.AreEqual(25L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { 25L } });

	        SendTimerEvent(8000);
	        SendEvent(-5);
	        Assert.AreEqual(20L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
	        Assert.IsNull(_listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { 20L } });

	        SendTimerEvent(10000);
	        Assert.AreEqual(20L, _listener.LastOldData[0].Get("mySum"));
	        Assert.AreEqual(10L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { 10L } });

	        SendTimerEvent(15000);
	        Assert.AreEqual(10L, _listener.LastOldData[0].Get("mySum"));
	        Assert.AreEqual(-5L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { -5L } });

	        SendTimerEvent(18000);
	        Assert.AreEqual(-5L, _listener.LastOldData[0].Get("mySum"));
	        Assert.IsNull(_listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new string[] { "mySum" }, new object[][] { new object[] { null } });
	    }

        [Test]
	    public void TestAvgPerSym()
	    {
	        var stmt = _epService.EPAdministrator.CreateEPL(
	                "select irstream avg(price) as avgp, sym from " + typeof(SupportPriceEvent).Name + ".std:groupwin(sym).win:length(2)"
	        );
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        _epService.EPRuntime.SendEvent(new SupportPriceEvent(1, "A"));
	        var theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("A", theEvent.Get("sym"));
	        Assert.AreEqual(1.0, theEvent.Get("avgp"));

	        _epService.EPRuntime.SendEvent(new SupportPriceEvent(2, "B"));
	        theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("B", theEvent.Get("sym"));
	        Assert.AreEqual(1.5, theEvent.Get("avgp"));

	        _epService.EPRuntime.SendEvent(new SupportPriceEvent(9, "A"));
	        theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("A", theEvent.Get("sym"));
	        Assert.AreEqual((1 + 2 + 9) / 3.0, theEvent.Get("avgp"));

	        _epService.EPRuntime.SendEvent(new SupportPriceEvent(18, "B"));
	        theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("B", theEvent.Get("sym"));
	        Assert.AreEqual((1 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));

	        _epService.EPRuntime.SendEvent(new SupportPriceEvent(5, "A"));
	        theEvent = listener.LastNewData[0];
	        Assert.AreEqual("A", theEvent.Get("sym"));
	        Assert.AreEqual((2 + 9 + 18 + 5) / 4.0, theEvent.Get("avgp"));
	        theEvent = listener.LastOldData[0];
	        Assert.AreEqual("A", theEvent.Get("sym"));
	        Assert.AreEqual((5 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));
	    }

        [Test]
	    public void TestSelectStarStdGroupBy() {
	        var stmtText = "select istream * from "+ typeof(SupportMarketDataBean).Name
	                +".std:groupwin(symbol).win:length(2)";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1.0, _listener.LastNewData[0].Get("price"));
	        Assert.IsTrue(_listener.LastNewData[0].Underlying is SupportMarketDataBean);
	    }

        [Test]
	    public void TestSelectExprStdGroupBy() {
	        var stmtText = "select istream price from "+ typeof(SupportMarketDataBean).Name
	                +".std:groupwin(symbol).win:length(2)";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1.0, _listener.LastNewData[0].Get("price"));
	    }

        [Test]
	    public void TestSelectAvgExprStdGroupBy() {
	        var stmtText = "select istream avg(price) as aprice from "+ typeof(SupportMarketDataBean).Name
	                +".std:groupwin(symbol).win:length(2)";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aprice"));
	        SendEvent("B", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(2.0, _listener.LastNewData[0].Get("aprice"));
	    }

        [Test]
	    public void TestSelectAvgStdGroupByUni() {
	        var stmtText = "select istream average as aprice from "+ typeof(SupportMarketDataBean).Name
	                +".std:groupwin(symbol).win:length(2).stat:uni(price)";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aprice"));
	        SendEvent("B", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(3.0, _listener.LastNewData[0].Get("aprice"));
	        SendEvent("A", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(2.0, _listener.LastNewData[0].Get("aprice"));
	        SendEvent("A", 10);
	        SendEvent("A", 20);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(15.0, _listener.LastNewData[0].Get("aprice"));
	    }

        [Test]
	    public void TestSelectAvgExprGroupBy() {
	        var stmtText = "select istream avg(price) as aprice, symbol from "+ typeof(SupportMarketDataBean).Name
	                +".win:length(2) group by symbol";
	        var statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aprice"));
	        Assert.AreEqual("A", _listener.LastNewData[0].Get("symbol"));
	        SendEvent("B", 3);
	        //there is no A->1 as we already got it out
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(3.0, _listener.LastNewData[0].Get("aprice"));
	        Assert.AreEqual("B", _listener.LastNewData[0].Get("symbol"));
	        SendEvent("B", 5);
	        // there is NOW a A->null entry
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        Assert.AreEqual(null, _listener.LastNewData[0].Get("aprice"));
	        Assert.AreEqual(4.0, _listener.LastNewData[1].Get("aprice"));
	        Assert.AreEqual("B", _listener.LastNewData[1].Get("symbol"));
	        SendEvent("A", 10);
	        SendEvent("A", 20);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(2, _listener.LastNewData.Length);
	        Assert.AreEqual(15.0, _listener.LastNewData[0].Get("aprice"));//A
	        Assert.AreEqual(null, _listener.LastNewData[1].Get("aprice"));//B
	    }

	    private object SendEvent(string symbol, double price) {
	        object theEvent = new SupportMarketDataBean(symbol, price, null, null);
	        _epService.EPRuntime.SendEvent(theEvent);
	        return theEvent;
	    }

	    private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
	    {
	        var bean = new SupportBean();
	        bean.TheString = JOIN_KEY;
	        bean.LongBoxed = longBoxed;
	        bean.IntBoxed = intBoxed;
	        bean.ShortBoxed = shortBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendEvent(long longBoxed)
	    {
	        SendEvent(longBoxed, 0, (short)0);
	    }

	    private void SendEventInt(int intBoxed)
	    {
	        var bean = new SupportBean();
	        bean.IntBoxed = intBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendEventFloat(float floatBoxed)
	    {
	        var bean = new SupportBean();
	        bean.FloatBoxed = floatBoxed;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendTimerEvent(long msec)
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
