///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.regression.client
{
    public class MyMetricFunctions
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static bool TakeCpuTime(long nanoSecTarget)
        {
            if (nanoSecTarget < 100)
            {
                throw new ArgumentException("CPU time wait nsec less then zero, was " + nanoSecTarget);
            }

            var current = PerformanceObserver.NanoTime;
            var before = current;
            var target = current+ nanoSecTarget;

            for (; current < target; current = PerformanceObserver.NanoTime)
            {
            }

            Log.Info("TakeCPUTime: {0} / {1}", nanoSecTarget, current - before);

            return true;
        }
    
        public static bool TakeWallTime(long msecTarget)
        {
            Thread.Sleep((int) msecTarget);
            return true;
        }
    }
}
