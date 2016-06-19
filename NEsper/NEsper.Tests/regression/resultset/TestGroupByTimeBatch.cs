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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestGroupByTimeBatch 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("MarketData", typeof(SupportMarketDataBean));
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestTimeBatchRowForAllNoJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream sum(Price) as sumPrice from MarketData.win:time_batch(1 sec)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], 45d);

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], 20d);

	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(1, oldEvents.Length);
	        AssertEvent(oldEvents[0], 45d);
	    }

        [Test]
	    public void TestTimeBatchRowForAllJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream sum(Price) as sumPrice from MarketData.win:time_batch(1 sec) as S0, SupportBean.win:keepall() as S1 where S0.Symbol = S1.TheString";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendSupportEvent("DELL");
	        SendSupportEvent("IBM");

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], 45d);

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], 20d);

	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(1, oldEvents.Length);
	        AssertEvent(oldEvents[0], 45d);
	    }

        [Test]
	    public void TestTimeBatchAggregateAllNoJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice from MarketData.win:time_batch(1 sec)";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(3, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", 45d);
	        AssertEvent(newEvents[1], "IBM", 45d);
	        AssertEvent(newEvents[2], "DELL", 45d);

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], "IBM", 20d);

	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(3, oldEvents.Length);
	        AssertEvent(oldEvents[0], "DELL", 20d);
	        AssertEvent(oldEvents[1], "IBM", 20d);
	        AssertEvent(oldEvents[2], "DELL", 20d);
	    }

        [Test]
	    public void TestTimeBatchAggregateAllJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice from MarketData.win:time_batch(1 sec) as S0, SupportBean.win:keepall() as S1 where S0.Symbol = S1.TheString";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendSupportEvent("DELL");
	        SendSupportEvent("IBM");

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(3, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", 45d);
	        AssertEvent(newEvents[1], "IBM", 45d);
	        AssertEvent(newEvents[2], "DELL", 45d);

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], "IBM", 20d);

	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(3, oldEvents.Length);
	        AssertEvent(oldEvents[0], "DELL", 20d);
	        AssertEvent(oldEvents[1], "IBM", 20d);
	        AssertEvent(oldEvents[2], "DELL", 20d);
	    }

        [Test]
	    public void TestTimeBatchRowPerGroupNoJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice from MarketData.win:time_batch(1 sec) group by Symbol order by Symbol asc";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(2, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", 30d);
	        AssertEvent(newEvents[1], "IBM", 15d);

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(2, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", null);
	        AssertEvent(newEvents[1], "IBM", 20d);

	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(2, oldEvents.Length);
	        AssertEvent(oldEvents[0], "DELL", 30d);
	        AssertEvent(oldEvents[1], "IBM", 15d);
	    }

        [Test]
	    public void TestTimeBatchRowPerGroupJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice " +
	                         " from MarketData.win:time_batch(1 sec) as S0, SupportBean.win:keepall() as S1" +
	                         " where S0.Symbol = S1.TheString " +
	                         " group by Symbol";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendSupportEvent("DELL");
	        SendSupportEvent("IBM");

	        // send first batch
	        SendMDEvent("DELL", 10, 0L);
	        SendMDEvent("IBM", 15, 0L);
	        SendMDEvent("DELL", 20, 0L);
	        SendTimer(1000);

	        var fields = "Symbol,sumPrice".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { "DELL", 30d }, new object[] { "IBM", 15d } });

	        // send second batch
	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);

            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastNewData, fields, new object[][] { new object[] { "DELL", null }, new object[] { "IBM", 20d } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastOldData(), fields, new object[][] { new object[] { "DELL", 30d }, new object[] { "IBM", 15d } });
	    }

        [Test]
	    public void TestTimeBatchAggrGroupedNoJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice, Volume from MarketData.win:time_batch(1 sec) group by Symbol";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendMDEvent("DELL", 10, 200L);
	        SendMDEvent("IBM", 15, 500L);
	        SendMDEvent("DELL", 20, 250L);

	        SendTimer(1000);
	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(3, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", 30d, 200L);
	        AssertEvent(newEvents[1], "IBM", 15d, 500L);
	        AssertEvent(newEvents[2], "DELL", 30d, 250L);

	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);
	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], "IBM", 20d, 600L);
	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(3, oldEvents.Length);
	        AssertEvent(oldEvents[0], "DELL", null, 200L);
	        AssertEvent(oldEvents[1], "IBM", 20d, 500L);
	        AssertEvent(oldEvents[2], "DELL", null, 250L);
	    }

        [Test]
	    public void TestTimeBatchAggrGroupedJoin()
	    {
	        SendTimer(0);
	        var stmtText = "select irstream Symbol, sum(Price) as sumPrice, Volume " +
	                          "from MarketData.win:time_batch(1 sec) as S0, SupportBean.win:keepall() as S1" +
	                          " where S0.Symbol = S1.TheString " +
	                          " group by Symbol";
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_listener);

	        SendSupportEvent("DELL");
	        SendSupportEvent("IBM");

	        SendMDEvent("DELL", 10, 200L);
	        SendMDEvent("IBM", 15, 500L);
	        SendMDEvent("DELL", 20, 250L);

	        SendTimer(1000);
	        var newEvents = _listener.LastNewData;
	        Assert.AreEqual(3, newEvents.Length);
	        AssertEvent(newEvents[0], "DELL", 30d, 200L);
	        AssertEvent(newEvents[1], "IBM", 15d, 500L);
	        AssertEvent(newEvents[2], "DELL", 30d, 250L);

	        SendMDEvent("IBM", 20, 600L);
	        SendTimer(2000);
	        newEvents = _listener.LastNewData;
	        Assert.AreEqual(1, newEvents.Length);
	        AssertEvent(newEvents[0], "IBM", 20d, 600L);
	        var oldEvents = _listener.LastOldData;
	        Assert.AreEqual(3, oldEvents.Length);
	        AssertEvent(oldEvents[0], "DELL", null, 200L);
	        AssertEvent(oldEvents[1], "IBM", 20d, 500L);
	        AssertEvent(oldEvents[2], "DELL", null, 250L);
	    }

	    private void SendSupportEvent(string theString)
	    {
	        _epService.EPRuntime.SendEvent(new SupportBean(theString, -1));
	    }

	    private void SendMDEvent(string symbol, double price, long? volume)
	    {
	        _epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, null));
	    }

	    private void AssertEvent(EventBean theEvent, string symbol, double? sumPrice, long? volume)
	    {
	        Assert.AreEqual(symbol, theEvent.Get("Symbol"));
	        Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
	        Assert.AreEqual(volume, theEvent.Get("Volume"));
	    }

        private void AssertEvent(EventBean theEvent, string symbol, double? sumPrice)
	    {
	        Assert.AreEqual(symbol, theEvent.Get("Symbol"));
	        Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
	    }

        private void AssertEvent(EventBean theEvent, double? sumPrice)
	    {
	        Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
	    }

	    private void SendTimer(long time)
	    {
	        var theEvent = new CurrentTimeEvent(time);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }
	}
} // end of namespace
