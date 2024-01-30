///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.regressionlib.support.client
{
    public class MyMetricFunctions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool TakeMillis(double milliSecTarget)
        {
            PerformanceMetricsHelper.ExecCpuBound(
                context => {
                    var currentMillis = (context.CurrentUserTime - context.InitialUserTime).TotalMilliseconds;
                    return milliSecTarget > currentMillis;
                });

            return true;
        }

        public static bool TakeNanos(long nanoSecTarget)
        {
            if (nanoSecTarget < 100) {
                throw new EPException("CPU time wait nsec less then zero, was " + nanoSecTarget);
            }

            PerformanceMetricsHelper.ExecCpuBound(
                context => {
                    var currentMillis = (context.CurrentUserTime - context.InitialUserTime).TotalMilliseconds;
                    var currentNanos = 1000000L * currentMillis;
                    return nanoSecTarget > currentNanos;
                });

            return true;
        }

        public static bool TakeWallTime(long msecTarget)
        {
            try {
                var currentTime = DateTime.UtcNow;
                var targetTime = currentTime + TimeSpan.FromMilliseconds(msecTarget);
                while (currentTime < targetTime) {
                    Thread.SpinWait(1000);
                    currentTime = DateTime.UtcNow;
                }
            }
            catch (ThreadInterruptedException e) {
                Log.Error("Unexpected exception", e);
            }

            return true;
        }
    }
} // end of namespace