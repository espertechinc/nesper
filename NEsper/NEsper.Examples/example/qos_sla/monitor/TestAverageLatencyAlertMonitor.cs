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
	public class TestAverageLatencyAlertMonitor : IDisposable
	{
	    private EPRuntime runtime;

	    [SetUp]
	    public void SetUp()
	    {
            Configuration configuration = new Configuration();
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

            EPServiceProviderManager.PurgeDefaultProvider();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(configuration);

	        new AverageLatencyMonitor();
	        runtime = epService.EPRuntime;
	    }

	    [Test]
	    public void TestLatencyAlert()
	    {
	        String[] services = {"s0", "s1", "s2"};
	        String[] customers = {"c0", "c1", "c2"};
	        OperationMeasurement measurement;

	        for (int i = 0; i < 100; i++)
	        {
	            for (int index = 0; index < services.Length; index++)
	            {
	                measurement = new OperationMeasurement(services[index], customers[index],
	                        9950 + i, true);
	                runtime.SendEvent(measurement);
	            }
	        }

	        // This should generate an alert
	        measurement = new OperationMeasurement(services[0], customers[0], 10000, true);
	        runtime.SendEvent(measurement);

	        // This should generate an alert
	        measurement = new OperationMeasurement(services[1], customers[1], 10001, true);
	        runtime.SendEvent(measurement);

	        // This should not generate an alert
	        measurement = new OperationMeasurement(services[2], customers[2], 9999, true);
	        runtime.SendEvent(measurement);
	    }

	    public void Dispose()
	    {
	    }
	}
} // End of namespace
