///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.example.qos_sla.eventbean;

namespace com.espertech.esper.example.qos_sla.monitor
{
    public class AverageLatencyMonitor
    {
        public AverageLatencyMonitor()
        {
            EPAdministrator admin = EPServiceProviderManager.GetDefaultProvider().EPAdministrator;

            EPStatement statView = admin.CreateEPL(
                    "select * from " + typeof(OperationMeasurement).FullName +
                    ".std:groupby('CustomerId').std:groupby('OperationName')" +
                    ".win:length(100).stat:uni('Latency')");

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
