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
    public class DynaLatencySpikeMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static EPRuntime _runtime;
        
        private readonly EPDeployment _spikeLatencyDeployment;
        private readonly EPStatement _spikeLatencyAlert;

        public static void Start()
        {
            _runtime = EPRuntimeProvider.GetDefaultRuntime();

            var eventName = typeof(LatencyLimit).FullName;
            var latencyAlert = _runtime.DeployStatement("every newlimit=" + eventName);
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
            _spikeLatencyDeployment = _runtime.CompileDeploy(
                "every alert=" + typeof (OperationMeasurement).FullName +
                "(" + filter + ", latency>" + limit.LatencyThreshold + ")");
            _spikeLatencyAlert = _spikeLatencyDeployment.Statements[0];
            _spikeLatencyAlert.Events += LogLatencyEvent;

            // Stop pattern when the threshold changes
            var eventName = typeof(LatencyLimit).FullName;
            var stopPattern = _runtime.DeployStatement(eventName + "(" + filter + ")");

            stopPattern.Events += ((newEvents, oldEvents) => _runtime
                .DeploymentService.Undeploy(_spikeLatencyAlert.DeploymentId));
        }

        public void LogLatencyEvent(Object sender, UpdateEventArgs e)
        {
            var eventBean = (OperationMeasurement)e.NewEvents[0]["alert"];
            Log.Info("Alert, for operation '" + eventBean.OperationName +
                     "' and customer '" + eventBean.CustomerId + "'" +
                     " latency was " + eventBean.Latency);
        }
    }
}
