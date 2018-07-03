///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using NEsper.Examples.QoS_SLA.eventbean;

namespace NEsper.Examples.QoS_SLA.monitor
{
    public class DynaLatencySpikeMonitor
    {
        private static EPAdministrator _admin;

        private readonly EPStatement _spikeLatencyAlert;

        public static void Start()
        {
            _admin = EPServiceProviderManager.GetDefaultProvider().EPAdministrator;

            var eventName = typeof(LatencyLimit).FullName;
            var latencyAlert = _admin.CreatePattern("every newlimit=" + eventName);
            latencyAlert.Events +=
                (sender, e) => { new DynaLatencySpikeMonitor((LatencyLimit)e.NewEvents[0]["newlimit"]); };
        }

        public DynaLatencySpikeMonitor(LatencyLimit limit)
        {
            Log.Debug("New limit, for operation '" + limit.OperationName +
                    "' and customer '" + limit.CustomerId + "'" +
                    " setting threshold " + limit.LatencyThreshold);

            var filter = "operationName='" + limit.OperationName +
                         "',customerId='" + limit.CustomerId + "'";

            // Alert specific to operation and customer
            _spikeLatencyAlert = _admin.CreatePattern(
                "every alert=" + typeof (OperationMeasurement).FullName +
                "(" + filter + ", latency>" + limit.LatencyThreshold + ")");
            _spikeLatencyAlert.Events += LogLatencyEvent;

            // Stop pattern when the threshold changes
            var eventName = typeof(LatencyLimit).FullName;
            var stopPattern = _admin.CreatePattern(eventName + "(" + filter + ")");

            stopPattern.Events += ((newEvents, oldEvents) => _spikeLatencyAlert.Stop());
        }

        public void LogLatencyEvent(Object sender, UpdateEventArgs e)
        {
            var eventBean = (OperationMeasurement)e.NewEvents[0]["alert"];
            Log.Info("Alert, for operation '" + eventBean.OperationName +
                     "' and customer '" + eventBean.CustomerId + "'" +
                     " latency was " + eventBean.Latency);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
