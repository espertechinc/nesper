///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.diagnostics
{
    public static class PerformanceMetricsHelper
    {
        public static void ExecCpuBound(Func<PerformanceExecutionContext, bool> cpuAction)
        {
            var processThread = ProcessThreadHelper.GetProcessThread();

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
            var processThread = ProcessThreadHelper.GetProcessThread();

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

        public static PerformanceMetrics GetCurrentMetricResult()
        {
            var processThread = ProcessThreadHelper.GetProcessThread();
            return new PerformanceMetrics(
                processThread.UserProcessorTime,
                processThread.PrivilegedProcessorTime,
                processThread.TotalProcessorTime,
                0);
        }
    }
}