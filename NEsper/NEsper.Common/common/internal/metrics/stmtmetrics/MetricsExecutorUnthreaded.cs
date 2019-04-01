///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>Metrics executor executing in-thread. </summary>
    public class MetricsExecutorUnthreaded : MetricsExecutor
    {
        public void Execute(MetricExec execution, MetricExecutionContext executionContext)
        {
            execution.Execute(executionContext);
        }

        public void Dispose()
        {
            // no action required, nothing to stop
        }
    }
}