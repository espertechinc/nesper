///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;

namespace NEsper.Benchmark.Perforator
{
    public class ThreadLocal
    {
        private const int TestIterations = 1000000;
        private const int TestCycles = 5;

        private static void MeasurePerformance(string caption, IThreadLocalFactory threadLocalFactory)
        {
            for (int cycle = 0; cycle < TestCycles; cycle++)
            {
                Console.Out.Write("{0,-20} ... ", caption);
                Console.Out.Flush();

                var threadDispatchQueue = threadLocalFactory.CreateThreadLocal(
                    () => new Queue<Runnable>());

                // Check access time - when not set
                long timeA = PerformanceObserver.MicroTime;
                for (int ii = 0; ii < TestIterations; ii++) {
                    var value = threadDispatchQueue.Value;
                }

                // Check access time - with get or create
                long timeB = PerformanceObserver.MicroTime;
                for (int ii = 0; ii < TestIterations; ii++) {
                    threadDispatchQueue.GetOrCreate();
                }

                long timeC = PerformanceObserver.MicroTime;
                Console.WriteLine("{0} -> {1} us -> {2} us", timeA, timeB - timeA, timeC - timeB);
            }
        }

        public static void MeasureSlimThreadLocal() { MeasurePerformance("SlimThreadLocal", new SlimThreadLocalFactory()); }
        public static void MeasureXperThreadLocal() { MeasurePerformance("XperThreadLocal", new XFastThreadLocalFactory()); }
        public static void MeasureFastThreadLocal() { MeasurePerformance("FastThreadLocal", new FastThreadLocalFactory()); }
        public static void MeasureSystemThreadLocal() { MeasurePerformance("SystemThreadLocal", new SystemThreadLocalFactory()); }
    }
}
