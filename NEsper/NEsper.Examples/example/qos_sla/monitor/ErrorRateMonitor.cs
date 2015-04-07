///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.example.qos_sla.eventbean;

namespace com.espertech.esper.example.qos_sla.monitor
{
    public class ErrorRateMonitor
    {
        public ErrorRateMonitor()
        {
            EPAdministrator admin = EPServiceProviderManager.GetDefaultProvider().EPAdministrator;

            EPStatement pattern = admin.CreatePattern("every timer:at(*, *, *, *, *, */10)");
            EPStatement view = admin.CreateEPL("select count(*) as size from " + typeof(OperationMeasurement).FullName +
                    "(success=false).win:time(10 min).std:size()");

            pattern.Events +=
                delegate {
                    var count = (long) view.First()["size"];
                    Log.Info(".update Info, error rate in the last 10 minutes is " + count);
                };
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
