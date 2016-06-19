///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	public class TestGroupByEventPerRowHaving 
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
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _testListener = null;
	    }

        [Test]
	    public void TestGroupByHaving()
        {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));

	        RunAssertionGroupByHaving(false);
	        RunAssertionGroupByHaving(true);
	    }

	    private void RunAssertionGroupByHaving(bool join)
        {
	        string epl = !join ?
	                "select * from SupportBean.win:length_batch(3) group by TheString having count(*) > 1" :
	                "select TheString, IntPrimitive from SupportBean_S0.std:lastevent(), SupportBean.win:length_batch(3) group by TheString having count(*) > 1";
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));

	        EventBean[] received = _testListener.GetNewDataListFlattened();
	        EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive".Split(','),
	                new object[][] { new object[] {"E2", 20},  new object[] {"E2", 21}});
	        _testListener.Reset();
	        stmt.Dispose();
	    }

        [Test]
	    public void TestSumOneView()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        string viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
	                          "from " + typeof(SupportMarketDataBean).Name + ".win:length(3) " +
	                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 50";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        RunAssertion(selectTestView);
	    }

        [Test]
	    public void TestSumJoin()
	    {
	        // Every event generates a new row, this time we sum the Price by Symbol and output Volume
	        string viewExpr = "select irstream Symbol, Volume, sum(Price) as mySum " +
	                          "from " + typeof(SupportBeanString).Name + ".win:length(100) as one, " +
	                                    typeof(SupportMarketDataBean).Name + ".win:length(3) as two " +
	                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
	                          "  and one.TheString = two.Symbol " +
	                          "group by Symbol " +
	                          "having sum(Price) >= 50";

	        EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
	        selectTestView.AddListener(_testListener);

	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
	        _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));

	        RunAssertion(selectTestView);
	    }

	    private void RunAssertion(EPStatement selectTestView)
	    {
	        // assert select result type
	        Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
	        Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("Volume"));
	        Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));

	        string[] fields = "Symbol,Volume,mySum".Split(',');
	        SendEvent(SYMBOL_DELL, 10000, 49);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent(SYMBOL_DELL, 20000, 54);
	        EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(), fields, new object[]{SYMBOL_DELL, 20000L, 103d});

	        SendEvent(SYMBOL_IBM, 1000, 10);
	        Assert.IsFalse(_testListener.IsInvoked);

	        SendEvent(SYMBOL_IBM, 5000, 20);
	        EPAssertionUtil.AssertProps(_testListener.AssertOneGetOldAndReset(), fields, new object[]{SYMBOL_DELL, 10000L, 54d});

	        SendEvent(SYMBOL_IBM, 6000, 5);
	        Assert.IsFalse(_testListener.IsInvoked);
	    }

	    private void SendEvent(string symbol, long volume, double price)
	    {
	        var bean = new SupportMarketDataBean(symbol, price, volume, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
