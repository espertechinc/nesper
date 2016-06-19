///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestCount 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private EPStatement _selectTestView;

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
	    public void TestCountPlusStar()
	    {
	        // Test for ESPER-118
	        var statementText = "select *, count(*) as cnt from " + typeof(SupportMarketDataBean).Name;
	        _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        _selectTestView.AddListener(_listener);

	        SendEvent("S0", 1L);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(1L, _listener.LastNewData[0].Get("cnt"));
	        Assert.AreEqual("S0", _listener.LastNewData[0].Get("symbol"));

	        SendEvent("S1", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(2L, _listener.LastNewData[0].Get("cnt"));
	        Assert.AreEqual("S1", _listener.LastNewData[0].Get("symbol"));

	        SendEvent("S2", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(3L, _listener.LastNewData[0].Get("cnt"));
	        Assert.AreEqual("S2", _listener.LastNewData[0].Get("symbol"));
	    }

        [Test]
	    public void TestCountMain()
	    {
	    	var statementText = "select count(*) as cnt from " + typeof(SupportMarketDataBean).Name + ".win:time(1)";
	        _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        _selectTestView.AddListener(_listener);

	        SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(1L, _listener.LastNewData[0].Get("cnt"));

	        SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(2L, _listener.LastNewData[0].Get("cnt"));

	        SendEvent("DELL", 1L);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        Assert.AreEqual(3L, _listener.LastNewData[0].Get("cnt"));

	        // test invalid distinct
	        SupportMessageAssertUtil.TryInvalid(_epService, "select count(distinct *) from " + typeof(SupportMarketDataBean).Name,
	                "Error starting statement: Failed to validate select-clause expression 'count(distinct *)': Invalid use of the 'distinct' keyword with count and wildcard");
	    }

        [Test]
	    public void TestCountHaving()
	    {
	        var theEvent = typeof(SupportBean).Name;
	        var statementText = "select irstream sum(intPrimitive) as mysum from " + theEvent + " having sum(intPrimitive) = 2";
	        _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        _selectTestView.AddListener(_listener);

	        SendEvent();
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
	        SendEvent();
	        Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("mysum"));
	        SendEvent();
	        Assert.AreEqual(2, _listener.AssertOneGetOldAndReset().Get("mysum"));
	    }

        [Test]
	    public void TestSumHaving()
	    {
	        var theEvent = typeof(SupportBean).Name;
	        var statementText = "select irstream count(*) as mysum from " + theEvent + " having count(*) = 2";
	        _selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        _selectTestView.AddListener(_listener);

	        SendEvent();
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
	        SendEvent();
	        Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("mysum"));
	        SendEvent();
	        Assert.AreEqual(2L, _listener.AssertOneGetOldAndReset().Get("mysum"));
	    }

	    private void SendEvent(string symbol, long? volume)
	    {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendEvent()
	    {
	        var bean = new SupportBean("", 1);
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
