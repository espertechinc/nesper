///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Implementation of the performance observer for use on .NET core.  Keep in mind
    /// that we will be relying on ticks to measure performance.  This means that our
    /// accuracy will be limited by the maximum resolution of ticks which is 1/10th of
    /// a microsecond.  Assuming its accurate.
    /// </summary>

    public class PerformanceObserverCore
    {
        public static long NanoTime => DateTime.Now.Ticks * 100;

        public static long MicroTime => DateTime.Now.Ticks / 10;

        public static long MilliTime => DateTime.Now.Ticks / 10000;

        public static long TimeNano(Runnable r)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            r.Invoke();
            stopWatch.Stop();
            return 100 * stopWatch.ElapsedTicks;
        }

        public static long TimeMicro(Runnable r)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            r.Invoke();
            stopWatch.Stop();
            return 100 * stopWatch.ElapsedTicks;
        }

        public static long TimeMillis(Runnable r)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            r.Invoke();
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
    }
}
