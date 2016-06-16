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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewTimeWin 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _testListener;

        [SetUp]
	    public void SetUp()
	    {
	        _testListener = new SupportUpdateListener();
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	    }

        [Test]
	    public void TestWinTimeSum()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        string sumTimeExpr = "select Symbol, Volume, sum(Price) as mySum " +
	                             "from " + typeof(SupportMarketDataBean).FullName + ".win:time(30)";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(sumTimeExpr);
	        selectTestView.AddListener(_testListener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestWinTimeSumGroupBy()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        string sumTimeUniExpr = "select Symbol, Volume, sum(Price) as mySum " +
	                             "from " + typeof(SupportMarketDataBean).FullName +
	                             ".win:time(30) group by Symbol";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
	        selectTestView.AddListener(_testListener);

	        RunGroupByAssertions(selectTestView);
	    }

        [Test]
	    public void TestWinTimeSumSingle()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        string sumTimeUniExpr = "select Symbol, Volume, sum(Price) as mySum " +
	                             "from " + typeof(SupportMarketDataBean).FullName +
	                             "(Symbol = 'IBM').win:time(30)";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(sumTimeUniExpr);
	        selectTestView.AddListener(_testListener);

	        RunSingleAssertion(selectTestView);
	    }

	    private void RunAssertion(EPStatement selectTestView)
	    {
	        AssertSelectResultType(selectTestView);

	        var currentTime = new CurrentTimeEvent(0);
	        _epService.EPRuntime.SendEvent(currentTime);

	        SendEvent(SYMBOL_DELL, 10000, 51);
	        AssertEvents(SYMBOL_DELL, 10000, 51,false);

	        SendEvent(SYMBOL_IBM, 20000, 52);
	        AssertEvents(SYMBOL_IBM, 20000, 103,false);

	        SendEvent(SYMBOL_DELL, 40000, 45);
	        AssertEvents(SYMBOL_DELL, 40000, 148,false);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));

	        //These events are out of the window and new sums are generated

	        SendEvent(SYMBOL_IBM, 30000, 70);
	        AssertEvents(SYMBOL_IBM, 30000,70,false);

	        SendEvent(SYMBOL_DELL, 10000, 20);
	        AssertEvents(SYMBOL_DELL, 10000, 90,false);

	    }

	    private void RunGroupByAssertions(EPStatement selectTestView)
	    {
	        AssertSelectResultType(selectTestView);

	        var currentTime = new CurrentTimeEvent(0);
	        _epService.EPRuntime.SendEvent(currentTime);

	        SendEvent(SYMBOL_DELL, 10000, 51);
	        AssertEvents(SYMBOL_DELL, 10000, 51,false);

	        SendEvent(SYMBOL_IBM, 30000, 70);
	        AssertEvents(SYMBOL_IBM, 30000, 70,false);

	        SendEvent(SYMBOL_DELL, 20000, 52);
	        AssertEvents(SYMBOL_DELL, 20000, 103,false);

	        SendEvent(SYMBOL_IBM, 30000, 70);
	        AssertEvents(SYMBOL_IBM, 30000, 140,false);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));

	        //These events are out of the window and new sums are generated
	        SendEvent(SYMBOL_DELL, 10000, 90);
	        AssertEvents(SYMBOL_DELL, 10000, 90,false);

	        SendEvent(SYMBOL_IBM, 30000, 120);
	        AssertEvents(SYMBOL_IBM, 30000, 120,false);

	        SendEvent(SYMBOL_DELL, 20000, 90);
	        AssertEvents(SYMBOL_DELL, 20000, 180,false);

	        SendEvent(SYMBOL_IBM, 30000, 120);
	        AssertEvents(SYMBOL_IBM, 30000, 240,false);
	     }

	    private void RunSingleAssertion(EPStatement selectTestView)
	    {
	        AssertSelectResultType(selectTestView);

	        var currentTime = new CurrentTimeEvent(0);
	        _epService.EPRuntime.SendEvent(currentTime);

	        SendEvent(SYMBOL_IBM, 20000, 52);
	        AssertEvents(SYMBOL_IBM, 20000, 52,false);

	        SendEvent(SYMBOL_IBM, 20000, 100);
	        AssertEvents(SYMBOL_IBM, 20000, 152,false);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(35000));

	        //These events are out of the window and new sums are generated
	        SendEvent(SYMBOL_IBM, 20000, 252);
	        AssertEvents(SYMBOL_IBM, 20000, 252,false);

	        SendEvent(SYMBOL_IBM, 20000, 100);
	        AssertEvents(SYMBOL_IBM, 20000, 352,false);
	    }

	    private void AssertEvents(string symbol, long volume, double sum,bool unique)
	    {
	        EventBean[] oldData = _testListener.LastOldData;
	        EventBean[] newData = _testListener.LastNewData;

	        if( ! unique)
	         Assert.IsNull(oldData);

	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(symbol, newData[0].Get("Symbol"));
	        Assert.AreEqual(volume, newData[0].Get("Volume"));
	        Assert.AreEqual(sum, newData[0].Get("mySum"));

	        _testListener.Reset();
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void AssertSelectResultType(EPStatement selectTestView)
	    {
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
	    }

	    private void SendEvent(string symbol, long volume, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        _epService.EPRuntime.SendEvent(bean);

	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
