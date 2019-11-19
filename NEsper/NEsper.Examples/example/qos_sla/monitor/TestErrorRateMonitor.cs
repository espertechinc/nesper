///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using NEsper.Examples.QoS_SLA.eventbean;

using NUnit.Framework;

namespace NEsper.Examples.QoS_SLA.monitor
{
	[TestFixture]
	public class TestErrorRateMonitor : IDisposable
	{
	    private EPRuntime _runtime;

	    [SetUp]
	    public void SetUp()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

            EPServiceProviderManager.PurgeDefaultProvider();
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);

	        new ErrorRateMonitor();
	        _runtime = epService.EPRuntime;
	    }

	    [Test]
	    public void TestAlert()
	    {
	        for (var i= 0; i < 5; i++)
	        {
	            SendEvent(false);
	        }

	        //sleep(11000);

	        for (var i= 0; i < 4; i++)
	        {
	            SendEvent(false);
	        }

	        //sleep(11000);
	        //sleep(11000);
	    }

	    private void SendEvent(bool success)
	    {
	        var measurement = new OperationMeasurement("myService", "myCustomer", 10000, success);
	        _runtime.SendEvent(measurement);
	    }

	    public void Dispose()
	    {
	    }
	}
} // End of namespace
