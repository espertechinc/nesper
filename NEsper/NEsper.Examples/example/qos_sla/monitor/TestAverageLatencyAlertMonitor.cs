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
	public class TestAverageLatencyAlertMonitor : IDisposable
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

	        new AverageLatencyMonitor();
	        _runtime = epService.EPRuntime;
	    }

	    [Test]
	    public void TestLatencyAlert()
	    {
	        String[] services = {"s0", "s1", "s2"};
	        String[] customers = {"c0", "c1", "c2"};
	        OperationMeasurement measurement;

	        for (var i = 0; i < 100; i++)
	        {
	            for (var index = 0; index < services.Length; index++)
	            {
	                measurement = new OperationMeasurement(services[index], customers[index],
	                        9950 + i, true);
	                _runtime.SendEvent(measurement);
	            }
	        }

	        // This should generate an alert
	        measurement = new OperationMeasurement(services[0], customers[0], 10000, true);
	        _runtime.SendEvent(measurement);

	        // This should generate an alert
	        measurement = new OperationMeasurement(services[1], customers[1], 10001, true);
	        _runtime.SendEvent(measurement);

	        // This should not generate an alert
	        measurement = new OperationMeasurement(services[2], customers[2], 9999, true);
	        _runtime.SendEvent(measurement);
	    }

	    public void Dispose()
	    {
	    }
	}
} // End of namespace
