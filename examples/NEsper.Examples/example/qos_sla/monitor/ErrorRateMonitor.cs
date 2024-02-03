///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

using NEsper.Examples.QoS_SLA.eventbean;
using NEsper.Examples.Support;

namespace NEsper.Examples.QoS_SLA.monitor
{
    public class ErrorRateMonitor
    {
        public ErrorRateMonitor()
        {
            var runtime = EPRuntimeProvider.GetDefaultRuntime();

            var pattern = runtime.DeployStatement("every timer:at(*, *, *, *, *, */10)");
            var view = runtime.DeployStatement(
                "select count(*) as size from " + typeof(OperationMeasurement).FullName +
                    "(success=false)#time(10 min)#size()");

            pattern.Events +=
                delegate {
                    var count = (long) view.First()["size"];
                    Log.Info(".update Info, error rate in the last 10 minutes is " + count);
                };
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
