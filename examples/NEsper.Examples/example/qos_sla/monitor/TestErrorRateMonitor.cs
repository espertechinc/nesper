///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.QoS_SLA.eventbean;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.QoS_SLA.monitor
{
	[TestFixture]
	public class TestErrorRateMonitor : IDisposable
	{
	    private EPRuntime _runtime;
	    private EventSender _sender;

	    [SetUp]
	    public void SetUp()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

            _runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
            _sender = _runtime.EventService.GetEventSender(typeof(OperationMeasurement).FullName);

	        new ErrorRateMonitor();
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
	        _sender.SendEvent(measurement);
	    }

	    public void Dispose()
	    {
	    }
	}
} // End of namespace
