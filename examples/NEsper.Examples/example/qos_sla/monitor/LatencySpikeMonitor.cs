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
    public class LatencySpikeMonitor
    {
        private LatencySpikeMonitor()
        {
        }

        public static void Start()
        {
            var runtime = EPRuntimeProvider.GetDefaultRuntime();
            var latencyAlert = runtime.DeployStatement(
                "every alert=" + typeof(OperationMeasurement).FullName + "(latency > 20000)");
            latencyAlert.Events += LogLatencyEvent;
        }

        public static void LogLatencyEvent(Object sender, UpdateEventArgs e)
        {
            var eventBean = (OperationMeasurement)e.NewEvents[0]["alert"];
            Log.Info("Alert, for operation '" + eventBean.OperationName +
                     "' and customer '" + eventBean.CustomerId + "'" +
                     " latency was " + eventBean.Latency);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
