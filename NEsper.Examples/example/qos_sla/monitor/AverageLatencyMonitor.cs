///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

using NEsper.Examples.QoS_SLA.eventbean;
using NEsper.Examples.Support;

namespace NEsper.Examples.QoS_SLA.monitor
{
    public class AverageLatencyMonitor
    {
        public AverageLatencyMonitor()
        {
            var runtime = EPRuntimeProvider.GetDefaultRuntime();

            var statView = runtime.DeployStatement(
                    "select * from " + typeof(OperationMeasurement).FullName +
                    "#groupwin(CustomerId)" +
                    "#groupwin(OperationName)" +
                    "#length(100)" +
                    "#uni(Latency)");

            statView.Events += (sender, e) => MonitorLatency(e, 10000);
        }

        public void MonitorLatency(UpdateEventArgs updateEventArgs, int alertThreshold)
        {
            var newEvents = updateEventArgs.NewEvents;
            var count = (long)newEvents[0]["datapoints"];
            var avg = (double)newEvents[0]["average"];

            if ((count < 100) || (avg < alertThreshold))
            {
                return;
            }

            var operation = (String)newEvents[0]["operationName"];
            var customer = (String)newEvents[0]["customerId"];

            Log.Debug("Alert, for operation '{0}' and customer '{1}' average latency was {2}",
                      operation,
                      customer,
                      avg);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
