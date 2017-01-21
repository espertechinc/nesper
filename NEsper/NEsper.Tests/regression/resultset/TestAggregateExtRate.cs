///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateExtRate
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

	    // rate implementation does not require a data window (may have one)
	    // advantage: not retaining events, only timestamp data points
	    // disadvantage: output rate limiting without snapshot may be less accurate rate
        [Test]
	    public void TestRateDataNonWindowed()
	    {
	        SendTimer(0);

	        var epl = "select rate(10) as myrate from SupportBean";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        RunAssertion();

	        stmt.Dispose();
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        stmt = _epService.EPAdministrator.Create(model);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(epl, model.ToEPL());

	        RunAssertion();

	        TryInvalid("select rate() from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'rate(*)': The rate aggregation function minimally requires a numeric constant or expression as a parameter. [select rate() from SupportBean]");
	        TryInvalid("select rate(true) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'rate(true)': The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation [select rate(true) from SupportBean]");
	    }

        [Test]
	    public void TestRateDataWindowed()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();

	        var fields = "myrate,myqtyrate".Split(',');
	        var viewExpr = "select RATE(longPrimitive) as myrate, RATE(longPrimitive, intPrimitive) as myqtyrate from SupportBean.win:length(3)";
	        var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
	        stmt.AddListener(_listener);

	        SendEvent(1000, 10);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});

	        SendEvent(1200, 0);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});

	        SendEvent(1300, 0);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null});

	        SendEvent(1500, 14);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3 * 1000 / 500d, 14 * 1000 / 500d});

	        SendEvent(2000, 11);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3 * 1000 / 800d, 25 * 1000 / 800d});

	        TryInvalid("select rate(longPrimitive) as myrate from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'rate(longPrimitive)': The rate aggregation function in the timestamp-property notation requires data windows [select rate(longPrimitive) as myrate from SupportBean]");
	        TryInvalid("select rate(current_timestamp) as myrate from SupportBean.win:time(20)",
	                "Error starting statement: Failed to validate select-clause expression 'rate(current_timestamp())': The rate aggregation function does not allow the current engine timestamp as a parameter [select rate(current_timestamp) as myrate from SupportBean.win:time(20)]");
	        TryInvalid("select rate(theString) as myrate from SupportBean.win:time(20)",
	                "Error starting statement: Failed to validate select-clause expression 'rate(theString)': The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation [select rate(theString) as myrate from SupportBean.win:time(20)]");
	    }

	    private void RunAssertion() {
	        var fields = "myrate".Split(',');

	        SendTimer(1000); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(1200); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(1600); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(1600); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(9000); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(9200); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(10999); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null});

	        SendTimer(11100); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.7});

	        SendTimer(11101); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.8});

	        SendTimer(11200); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.8});

	        SendTimer(11600); SendEvent();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{0.7});
	    }

	    private void TryInvalid(string epl, string message) {
	        try
	        {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private void SendTimer(long timeInMSec)
	    {
	        var theEvent = new CurrentTimeEvent(timeInMSec);
	        var runtime = _epService.EPRuntime;
	        runtime.SendEvent(theEvent);
	    }

	    private void SendEvent(long longPrimitive, int intPrimitive)
	    {
	        var bean = new SupportBean();
	        bean.LongPrimitive = longPrimitive;
	        bean.IntPrimitive = intPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void SendEvent()
	    {
	        var bean = new SupportBean();
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    public class RateSendRunnable
        {
	        private readonly EPRuntime _runtime;

	        public RateSendRunnable(EPRuntime runtime) {
	            _runtime = runtime;
	        }

	        public void Run() {
	            var bean = new SupportBean();
	            bean.LongPrimitive = DateTimeHelper.CurrentTimeMillis;
	            _runtime.SendEvent(bean);
	        }
	    }
	}
} // end of namespace
