///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

using NEsper.Examples.QoS_SLA.eventbean;

using NUnit.Framework;

namespace NEsper.Examples.QoS_SLA.monitor
{
    [TestFixture]
    public class TestDynamicLatencyAlertMonitor : IDisposable
    {
        private EPRuntime _runtime;

        [SetUp]
        public void SetUp()
        {
            DynaLatencySpikeMonitor.Start();
            _runtime = EPServiceProviderManager.GetDefaultProvider().EPRuntime;
        }

        [Test]
        public void TestLatencyAlert()
        {
            var services = new[] { "s0", "s1", "s2" };
            var customers = new[] { "c0", "c1", "c2" };
            var limitSpike = new[] { 15000L, 10000L, 10040L };

            // Set up limits for 3 services/customer combinations
            for (var i = 0; i < services.Length; i++)
            {
                var limit = new LatencyLimit(services[i], customers[i], limitSpike[i]);
                _runtime.SendEvent(limit);
            }

            // Send events
            for (var i = 0; i < 100; i++)
            {
                for (var index = 0; index < services.Length; index++)
                {
                    var measurement = new OperationMeasurement(services[index], customers[index], 9950 + i, true);
                    _runtime.SendEvent(measurement);
                }
            }

            // Send a new limit
            var nlimit = new LatencyLimit(services[1], customers[1], 8000);
            _runtime.SendEvent(nlimit);

            // Send a new spike
            var nmeasurement = new OperationMeasurement(services[1], customers[1], 8001, true);
            _runtime.SendEvent(nmeasurement);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
} // End of namespace
