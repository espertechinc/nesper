///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewSize 
	{
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
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestSize()
	    {
	        var statementText = "select irstream size from " + typeof(SupportMarketDataBean).FullName + "#size()";
	        var selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        selectTestView.AddListener(_listener);

	        SendEvent("DELL", 1L);
	        AssertSize(1, 0);

	        SendEvent("DELL", 1L);
	        AssertSize(2, 1);

	        selectTestView.Dispose();
	        statementText = "select size, Symbol, feed from " + typeof(SupportMarketDataBean).FullName + "#size(Symbol, feed)";
	        selectTestView = _epService.EPAdministrator.CreateEPL(statementText);
	        selectTestView.AddListener(_listener);
	        var fields = "size,Symbol,feed".Split(',');

	        SendEvent("DELL", 1L);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L, "DELL", "f1"});

	        SendEvent("DELL", 1L);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2L, "DELL", "f1"});
	    }

	    private void SendEvent(string symbol, long? volume)
	    {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, "f1");
	        _epService.EPRuntime.SendEvent(bean);
	    }

	    private void AssertSize(long newSize, long oldSize)
	    {
	        EPAssertionUtil.AssertPropsPerRow(_listener.AssertInvokedAndReset(), "size", new object[]{newSize}, new object[]{oldSize});
	    }
	}
} // end of namespace
