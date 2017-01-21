///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
	public class TestTimerScheduleObserverTimeZoneEST 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
	        config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
	        config.EngineDefaults.ExpressionConfig.TimeZone = TimeZoneHelper.GetTimeZoneInfo("GMT-4:00");
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _listener = new SupportUpdateListener();
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestCurrentTimeWTime() {
	        EPServiceProviderIsolated iso = _epService.GetEPServiceIsolated("E1");
	        SendCurrentTime(iso, "2012-10-01T8:59:00.000GMT-04:00");

	        string epl = "select * from pattern[timer:schedule(date: current_timestamp.withTime(9, 0, 0, 0))]";
	        iso.EPAdministrator.CreateEPL(epl, null, null).AddListener(_listener);

	        SendCurrentTime(iso, "2012-10-01T8:59:59.999GMT-4:00");
	        Assert.IsFalse(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-10-01T9:00:00.000GMT-4:00");
            Assert.IsTrue(_listener.IsInvokedAndReset());

	        SendCurrentTime(iso, "2012-10-03T9:00:00.000GMT-4:00");
            Assert.IsFalse(_listener.IsInvokedAndReset());

	        _epService.EPAdministrator.DestroyAllStatements();
	        iso.Dispose();
	    }

	    private void SendCurrentTime(EPServiceProviderIsolated iso, string time)
        {
            iso.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSecWZone(time)));
	    }
	}

} // end of namespace
