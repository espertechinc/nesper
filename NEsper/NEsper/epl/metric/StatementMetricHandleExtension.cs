///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using com.espertech.esper.compat;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

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
        private static extern bool GetThreadTimes(
            IntPtr hThread, 
            out FILETIME lpCreationTime,
            out FILETIME lpExitTime, 
            out FILETIME lpKernelTime, 
            out FILETIME lpUserTime);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();
#endif

        public static void ExecCpuBound(Func<long, long, bool> cpuAction)
        {
#if MONO
#else
            FILETIME lCreationTime;
            FILETIME lExitTime;
            FILETIME lUserTimeA;
            FILETIME lKernelTimeA;
            FILETIME lUserTimeB;
            FILETIME lKernelTimeB;

            IntPtr thread = GetCurrentThread();

            GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeA, out lKernelTimeA);

            // Calculate the time, not that the numbers represent 100-nanosecond intervals since
            // January 1, 601 (UTC).
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724284(v=vs.85).aspx

            var kernelTimeA = (((long)lKernelTimeA.dwHighDateTime) << 32) | ((long)lKernelTimeA.dwLowDateTime);
            var userTimeA = (((long)lUserTimeA.dwHighDateTime) << 32) | ((long)lUserTimeA.dwLowDateTime);
            var kernelTimeB = kernelTimeA;
            var userTimeB = userTimeA;
            var continuation = true;

            do
            {
                continuation = cpuAction.Invoke(
                    100L * (kernelTimeB - kernelTimeA),
                    100L * (userTimeB - userTimeA));

                GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeB, out lKernelTimeB);

                kernelTimeB = (((long)lKernelTimeB.dwHighDateTime) << 32) | ((long)lKernelTimeB.dwLowDateTime);
                userTimeB = (((long)lUserTimeB.dwHighDateTime) << 32) | ((long)lUserTimeB.dwLowDateTime);
            } while (continuation);
#endif
        }

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
                    metricValue,
                    100 * (lTimeB - lTimeA),
                    lWallTimeB - lWallTimeA);
#else

                FILETIME lCreationTime;
                FILETIME lExitTime;
                FILETIME lUserTimeA;
                FILETIME lKernelTimeA;
                FILETIME lUserTimeB;
                FILETIME lKernelTimeB;

                IntPtr thread = GetCurrentThread();
                long lWallTimeA = PerformanceObserverMono.NanoTime;

                GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeA, out lKernelTimeA);
                observableCall.Invoke();
                GetThreadTimes(thread, out lCreationTime, out lExitTime, out lUserTimeB, out lKernelTimeB);
                
                long lWallTimeB = PerformanceObserverMono.NanoTime;

                // Calculate the time, not that the numbers represent 100-nanosecond intervals since
                // January 1, 601 (UTC).
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724284(v=vs.85).aspx

                var kernelTimeA = (((long) lKernelTimeA.dwHighDateTime) << 32) | ((long) lKernelTimeA.dwLowDateTime);
                var kernelTimeB = (((long) lKernelTimeB.dwHighDateTime) << 32) | ((long) lKernelTimeB.dwLowDateTime);
                var userTimeA = (((long) lUserTimeA.dwHighDateTime) << 32) | ((long) lUserTimeA.dwLowDateTime);
                var userTimeB = (((long) lUserTimeB.dwHighDateTime) << 32) | ((long) lUserTimeB.dwLowDateTime);

                var execTimeNano = 100 * (userTimeB - userTimeA + kernelTimeB - kernelTimeA);

                perfCollector.Invoke(
                    statementMetricHandle,
                    execTimeNano,
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

    public delegate void PerformanceCollector(StatementMetricHandle statementMetricHandle, long cpuTime, long wallTime, int numInput);
}
