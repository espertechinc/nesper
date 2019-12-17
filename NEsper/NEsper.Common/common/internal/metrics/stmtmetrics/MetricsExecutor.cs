///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>Executor for metrics executions. </summary>
    public interface MetricsExecutor : IDisposable
    {
        /// <summary>Execute a metrics execution. </summary>
        /// <param name="execution">to execute</param>
        /// <param name="executionContext">context in which to execute</param>
        void Execute(
            MetricExec execution,
            MetricExecutionContext executionContext);
    }
}