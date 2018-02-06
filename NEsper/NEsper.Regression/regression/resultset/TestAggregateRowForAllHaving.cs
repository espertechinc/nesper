///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateRowForAllHaving 
	{
	    private const string JOIN_KEY = "KEY";

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
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
	        string viewExpr = "select irstream sum(longBoxed) as mySum " +
                              "from " + typeof(SupportBean).FullName + "#time(10 seconds) " +
	                          "having sum(longBoxed) > 10";
	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        RunAssert(selectTestView);
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        string viewExpr = "select irstream sum(longBoxed) as mySum " +
                              "from " + typeof(SupportBeanString).FullName + "#time(10 seconds) as one, " +
                                        typeof(SupportBean).FullName + "#time(10 seconds) as two " +
	                          "where one.theString = two.theString " +
	                          "having sum(longBoxed) > 10";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

	        RunAssert(selectTestView);
	    }

	    private void RunAssert(EPStatement selectTestView)
	    {
	        // assert select result type
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("mySum"));

	        SendTimerEvent(0);
	        SendEvent(10);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendTimerEvent(5000);
	        SendEvent(15);
	        Assert.AreEqual(25L, _listener.GetAndResetLastNewData()[0].Get("mySum"));

	        SendTimerEvent(8000);
	        SendEvent(-5);
	        Assert.AreEqual(20L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
	        Assert.IsNull(_listener.LastOldData);

	        SendTimerEvent(10000);
	        Assert.AreEqual(20L, _listener.LastOldData[0].Get("mySum"));
	        Assert.IsNull(_listener.GetAndResetLastNewData());
	    }

        [Test]
	    public void TestAvgGroupWindow()
	    {
	        //String stmtText = "select istream avg(price) as aprice from "+ typeof(SupportMarketDataBean).getName()
	        //        +"#groupwin(symbol)#length(1) having avg(price) <= 0";
            string stmtText = "select istream avg(price) as aprice from " + typeof(SupportMarketDataBean).FullName
	                +"#unique(symbol) having avg(price) <= 0";
	        EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
	        statement.AddListener(_listener);

	        SendEvent("A", -1);
	        Assert.AreEqual(-1.0d, _listener.LastNewData[0].Get("aprice"));
	        _listener.Reset();

	        SendEvent("A", 5);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("B", -6);
	        Assert.AreEqual(-.5d, _listener.LastNewData[0].Get("aprice"));
	        _listener.Reset();

	        SendEvent("C", 2);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("C", 3);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent("C", -2);
	        Assert.AreEqual(-1d, _listener.LastNewData[0].Get("aprice"));
	        _listener.Reset();
	    }

	    private object SendEvent(string symbol, double price)
        {
	        object theEvent = new SupportMarketDataBean(symbol, price, null, null);
	        _epService.EPRuntime.SendEvent(theEvent);
	        return theEvent;
	    }

	    private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
	    {
	        SupportBean bean = new SupportBean();
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

	    private void SendTimerEvent(long msec)
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
