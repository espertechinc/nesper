///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Metrics executor relying on a cached threadpool.
    /// </summary>
    public class MetricsExecutorThreaded : MetricsExecutor
    {
        private readonly IExecutorService _threadPool;

        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        public MetricsExecutorThreaded(string engineURI)
        {
            _threadPool = new DedicatedExecutorService("Metrics", 1);
        }

        public void Execute(
            MetricExec execution,
            MetricExecutionContext executionContext)
        {
            _threadPool.Submit(() => execution.Execute(executionContext));
        }

        public void Dispose()
        {
            _threadPool.Shutdown();
            _threadPool.AwaitTermination(new TimeSpan(0, 0, 10));
        }
    }
}