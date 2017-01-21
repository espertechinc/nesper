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
	public class TestGroupByEventPerGroupHaving 
	{
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
	    private SupportUpdateListener _testListener;

        [SetUp]
	    public void SetUp()
	    {
	        _testListener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        _testListener = null;
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	    }

        [Test]
	    public void TestHavingCount()
	    {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        var text = "select * from SupportBean(IntPrimitive = 3).win:length(10) as e1 group by TheString having count(*) > 2";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(text);
	        selectTestView.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
	        Assert.IsFalse(_testListener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new SupportBean("A1", 3));
	        Assert.IsTrue(_testListener.IsInvoked);
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        var viewExpr = "select irstream Symbol, sum(Price) as mySum " +
	                          "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
                              " " + typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE')" +
	                          "       and one.TheString = two.Symbol " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 100";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
	        _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

	        RunAssertion();
	    }

        [Test]
	    public void TestSumOneView()
	    {
	        var viewExpr = "select irstream Symbol, sum(Price) as mySum " +
                              "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 100";

	        var selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        RunAssertion();
	    }

	    private void RunAssertion()
	    {
	        SendEvent(SYMBOL_DELL, 10);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent(SYMBOL_DELL, 60);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent(SYMBOL_DELL, 30);
	        AssertNewEvent(SYMBOL_DELL, 100);

	        SendEvent(SYMBOL_IBM, 30);
	        AssertOldEvent(SYMBOL_DELL, 100);

	        SendEvent(SYMBOL_IBM, 80);
	        AssertNewEvent(SYMBOL_IBM, 110);
	    }

	    private void AssertNewEvent(string symbol, double newSum)
	    {
	        var oldData = _testListener.LastOldData;
	        var newData = _testListener.LastNewData;

	        Assert.IsNull(oldData);
	        Assert.AreEqual(1, newData.Length);

	        Assert.AreEqual(newSum, newData[0].Get("mySum"));
	        Assert.AreEqual(symbol, newData[0].Get("Symbol"));

	        _testListener.Reset();
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void AssertOldEvent(string symbol, double newSum)
	    {
	        var oldData = _testListener.LastOldData;
	        var newData = _testListener.LastNewData;

	        Assert.IsNull(newData);
	        Assert.AreEqual(1, oldData.Length);

	        Assert.AreEqual(newSum, oldData[0].Get("mySum"));
	        Assert.AreEqual(symbol, oldData[0].Get("Symbol"));

	        _testListener.Reset();
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void SendEvent(string symbol, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
