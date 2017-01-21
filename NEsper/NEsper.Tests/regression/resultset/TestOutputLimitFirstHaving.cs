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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitFirstHaving
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
        {
	        var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
	        config.EngineDefaults.LoggingConfig.IsEnableTimerDebug = false;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestHavingNoAvgOutputFirstEvents()
        {
	        var query = "select DoublePrimitive from SupportBean having DoublePrimitive > 1 output first every 2 events";
	        var statement = _epService.EPAdministrator.CreateEPL(query);
	        statement.AddListener(_listener);
	        RunAssertion2Events();
	        statement.Dispose();

	        // test joined
	        query = "select DoublePrimitive from SupportBean.std:lastevent(),SupportBean_ST0.std:lastevent() st0 having DoublePrimitive > 1 output first every 2 events";
	        statement = _epService.EPAdministrator.CreateEPL(query);
	        _epService.EPRuntime.SendEvent(new SupportBean_ST0("ID", 1));
	        statement.AddListener(_listener);
	        RunAssertion2Events();
	    }

        [Test]
	    public void TestHavingNoAvgOutputFirstMinutes()
        {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

	        var fields = "val0".Split(',');
	        var query = "select sum(DoublePrimitive) as val0 from SupportBean.win:length(5) having sum(DoublePrimitive) > 100 output first every 2 seconds";
	        var statement = _epService.EPAdministrator.CreateEPL(query);
	        statement.AddListener(_listener);

	        SendBeanEvent(10);
	        SendBeanEvent(80);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        SendBeanEvent(11);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{101d});

	        SendBeanEvent(1);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2999));
	        SendBeanEvent(1);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
	        SendBeanEvent(1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent(100);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{114d});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4999));
	        SendBeanEvent(0);
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
	        SendBeanEvent(0);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{102d});
	    }

        [Test]
	    public void TestHavingAvgOutputFirstEveryTwoMinutes()
	    {
	        var query = "select DoublePrimitive, avg(DoublePrimitive) from SupportBean having DoublePrimitive > 2*avg(DoublePrimitive) output first every 2 minutes";
	        var statement = _epService.EPAdministrator.CreateEPL(query);
	        statement.AddListener(_listener);

	        SendBeanEvent(1);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent(2);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendBeanEvent(9);
	        Assert.IsTrue(_listener.IsInvoked);
	     }

	    private void RunAssertion2Events()
        {
	        SendBeanEvent(1);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(2);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(9);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(1);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(1);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(2);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(1);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(2);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(2);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        SendBeanEvent(2);
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	    }

	    private void SendBeanEvent(double doublePrimitive)
        {
	        var b = new SupportBean();
	        b.DoublePrimitive = doublePrimitive;
	        _epService.EPRuntime.SendEvent(b);
	    }
	}

} // end of namespace
