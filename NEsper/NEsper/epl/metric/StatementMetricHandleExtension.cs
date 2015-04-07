///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Runtime.InteropServices;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.metric
{
    public static class StatementMetricHandleExtension
    {
#if MONO
        [StructLayout(LayoutKind.Auto)]
        public struct Rusage
        {
            /* user time used */
            public long ru_utime_sec;
            public long ru_utime_usec;
            /* system time used */
            public long ru_stime_sec;
            public long ru_stime_usec;
            public long    ru_maxrss;              /* maximum resident set size */
            public long ru_ixrss;               /* integral shared memory size */
            public long ru_idrss;               /* integral unshared data size */
            public long ru_isrss;               /* integral unshared stack size */
            public long ru_minflt;              /* page reclaims */
            public long ru_majflt;              /* page faults */
            public long ru_nswap;               /* swaps */
            public long ru_inblock;             /* block input operations */
            public long ru_oublock;             /* block output operations */
            public long ru_msgsnd;              /* messages sent */
            public long ru_msgrcv;              /* messages received */
            public long ru_nsignals;            /* signals received */
            public long ru_nvcsw;               /* voluntary context switches */
            public long ru_nivcsw;              /* involuntary " */
        }

        public const int RUSAGE_SELF = 0;
        public const int RUSAGE_THREAD = 1;

        [DllImport("Kernel32.dll")]
        private static extern bool getrusage(int who, ref Rusage usage);
#else
        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern Int32 GetCurrentWin32ThreadId();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
           out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();
#endif

        /// <summary>
        /// Calls the specified observable call.
        /// </summary>
        /// <param name="statementMetricHandle">The statement metric handle.</param>
        /// <param name="perfCollector">The perf collector.</param>
        /// <param name="observableCall">The observable call.</param>
        /// <param name="numInput">The num input.</param>
        public static void Call(this StatementMetricHandle statementMetricHandle, PerformanceCollector perfCollector, Action observableCall, int numInput = 1)
        {
            if ((MetricReportingPath.IsMetricsEnabled) && statementMetricHandle.IsEnabled)
            {
#if MONO
                long lCreationTime, lExitTime;
                long lUserTimeA, lKernelTimeA;
                long lUserTimeB, lKernelTimeB;
                //IntPtr thread = GetCurrentThread();
                long lWallTimeA = PerformanceObserverMono.NanoTime;

                //GetThreadTimes(out lUserTimeA, out lKernelTimeA);
                observableCall.Invoke();
                //GetThreadTimes(out lUserTimeB, out lKernelTimeB);

                long lWallTimeB = PerformanceObserverMono.NanoTime;
                long lTimeA = 0; // lKernelTimeA + lUserTimeA
                long lTimeB = 0; // lKernelTimeB + lUserTimeB

                perfCollector.Invoke(
                    statementMetricHandle,
                    100 * (lTimeB - lTimeA),
                    lWallTimeB - lWallTimeA);
#else

                long lCreationTime, lExitTime;
                long lUserTimeA, lKernelTimeA;
                long lUserTimeB, lKernelTimeB;
                IntPtr thread = GetCurrentThread();
                long lWallTimeA = PerformanceObserverMono.NanoTime;

                GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeA, out lKernelTimeA);
                observableCall.Invoke();
                GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeB, out lKernelTimeB);

                long lWallTimeB = PerformanceObserverMono.NanoTime;
                long lTimeA = (lKernelTimeA + lUserTimeA);
                long lTimeB = (lKernelTimeB + lUserTimeB);

                perfCollector.Invoke(
                    statementMetricHandle,
                    100 * (lTimeB - lTimeA),
                    lWallTimeB - lWallTimeA,
                    numInput);
#endif
            }
            else
            {
                observableCall.Invoke();
            }
        }

    }
}
