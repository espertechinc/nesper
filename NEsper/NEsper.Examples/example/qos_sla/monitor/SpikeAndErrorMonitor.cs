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
    public class SpikeAndErrorMonitor
    {
        public SpikeAndErrorMonitor()
        {
            var admin = EPServiceProviderManager.GetDefaultProvider().EPAdministrator;
            var eventName = typeof(OperationMeasurement).FullName;
            var myPattern = admin.CreatePattern(
                    "every (spike=" + eventName + "(latency>20000) or error=" + eventName + "(success=false))");

            myPattern.Events +=
                delegate(Object sender, UpdateEventArgs e) {
                    var spike = (OperationMeasurement) e.NewEvents[0]["spike"];
                    var error = (OperationMeasurement) e.NewEvents[0]["error"];

                    if (spike != null) {
                        Log.Debug(".update spike={0}", spike);
                    }
                    if (error != null) {
                        Log.Debug(".update error={0}", error);
                    }
                };
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
