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
    public class ServiceHealthMonitor
    {
        public ServiceHealthMonitor()
        {
            var admin = EPServiceProviderManager.GetDefaultProvider().EPAdministrator;

            var eventBean = typeof(OperationMeasurement).FullName;

            var statView = admin.CreatePattern("every (" +
                    eventBean + "(success=false)->" +
                    eventBean + "(success=false)->" +
                    eventBean + "(success=false))");

            statView.Events += (newEvents, oldEvents) => 
                Log.Debug(".update Alert, detected 3 erros in a row");
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
