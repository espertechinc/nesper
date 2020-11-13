///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace com.espertech.esper.compat.diagnostics
{
    public static class PerformanceMetricsHelper
    {
        public static ProcessThread GetProcessThread()
        {
#if NETCORE
            return null;
#else
            var processThread = ProcessThreadHelper.GetProcessThread();
            if (processThread != null) {
                throw new IllegalStateException("processThread could not be determined");
            }

            return processThread;
#endif
        }

        public static void ExecCpuBound(Func<PerformanceExecutionContext, bool> cpuAction)
        {
#if NETCORE
            throw new NotSupportedException("cpu bound execution not supported");
#else
            var processThread = GetProcessThread();

            var executionContext = new PerformanceExecutionContext();
            executionContext.InitialUserTime = processThread.UserProcessorTime;
            executionContext.InitialPrivTime = processThread.PrivilegedProcessorTime;
            executionContext.InitialTotalTime = processThread.TotalProcessorTime;
            executionContext.CurrentUserTime = executionContext.InitialUserTime;
            executionContext.CurrentPrivTime = executionContext.InitialPrivTime;
            executionContext.CurrentTotalTime = executionContext.InitialTotalTime;

            bool continuation = true;
            while (continuation)
            {
                continuation = cpuAction.Invoke(executionContext);
                executionContext.CurrentUserTime = processThread.UserProcessorTime;
                executionContext.CurrentPrivTime = processThread.PrivilegedProcessorTime;
                executionContext.CurrentTotalTime = processThread.TotalProcessorTime;
            }
#endif
        }

        /// <summary>
        /// Calls the specified observable call.
        /// </summary>
        /// <param name="observableCall">The observable call.</param>
        /// <param name="numInput">The num input.</param>
        public static PerformanceMetrics Call(
            Action observableCall,
            int numInput = 1)
        {
            var processThread = GetProcessThread();
            if (processThread != null) {
                var initialUserTime = processThread.UserProcessorTime;
                var initialPrivTime = processThread.PrivilegedProcessorTime;
                var initialTotalTime = processThread.TotalProcessorTime;

                observableCall.Invoke();

                var finalUserTime = processThread.UserProcessorTime;
                var finalPrivTime = processThread.PrivilegedProcessorTime;
                var finalTotalTime = processThread.TotalProcessorTime;

                return new PerformanceMetrics(
                    finalUserTime - initialUserTime,
                    finalPrivTime - initialPrivTime,
                    finalTotalTime - initialTotalTime,
                    numInput);
            }
            else {
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                observableCall.Invoke();
                stopwatch.Stop();

                var metric = stopwatch.Elapsed;
                return new PerformanceMetrics(
                    metric,
                    metric,
                    metric,
                    numInput);            }
        }

        public static PerformanceMetrics GetCurrentMetricResult()
        {
            var processThread = GetProcessThread();
            return new PerformanceMetrics(
                processThread.UserProcessorTime,
                processThread.PrivilegedProcessorTime,
                processThread.TotalProcessorTime,
                0);
        }
    }
}