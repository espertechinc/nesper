///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

using com.espertech.esper.client;
using com.espertech.esper.example.qos_sla.eventbean;

namespace com.espertech.esper.example.qos_sla.monitor
{
	[TestFixture]
	public class TestLatencySpikeMonitor : IDisposable
	{
	    private EPRuntime runtime;

	    [SetUp]
	    public void SetUp()
	    {
            Configuration configuration = new Configuration();
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

            EPServiceProviderManager.PurgeDefaultProvider();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(configuration);

	        LatencySpikeMonitor.Start();
	        runtime = epService.EPRuntime;
	    }

	    [Test]
	    public void TestLatencyAlert()
	    {
	        OperationMeasurement measurement = new OperationMeasurement("svc", "cust", 21000, true);
	        runtime.SendEvent(measurement);
	    }

	    public void Dispose()
	    {
	    }
	}
} // End of namespace
