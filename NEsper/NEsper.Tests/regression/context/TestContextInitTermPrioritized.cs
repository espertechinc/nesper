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

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
	public class TestContextInitTermPrioritized
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        configuration.EngineDefaults.ExecutionConfig.IsPrioritized = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestAtNowWithSelectedEventEnding()
        {
	        var fields = "theString".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Priority(1) create context C1 start @now end SupportBean");
	        var stmt = _epService.EPAdministrator.CreateEPL("@Priority(0) context C1 select * from SupportBean");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1"});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2"});
	    }
	}
} // end of namespace
