///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Utility for CPU and wall time metrics.
    /// </summary>
    public class MetricUtil
    {
        [ThreadStatic] public static ProcessThread _currentProcessThread;

        /// <summary>
        ///     Gets the current process thread.
        /// </summary>
        /// <value>The current process thread.</value>
        public static ProcessThread CurrentProcessThread {
            get {
                if (_currentProcessThread == null) {
                    var id = Thread.CurrentThread.ManagedThreadId;

                    var process = Process.GetCurrentProcess();
                    foreach (ProcessThread processThread in process.Threads) {
                        if (processThread.Id == id) {
                            _currentProcessThread = processThread;
                            break;
                        }
                    }
                }

                return _currentProcessThread;
            }
        }

        /// <summary>
        ///     Gets the user processor time for the current thread.
        /// </summary>
        /// <value>The user processor time.</value>
        public static TimeSpan UserProcessorTime =>
            CurrentProcessThread?.UserProcessorTime ?? TimeSpan.Zero;

        /// <summary>
        ///     Gets the total processor time for the current thread.
        /// </summary>
        /// <value>The total processor time.</value>
        public static TimeSpan TotalProcessorTime =>
            CurrentProcessThread?.TotalProcessorTime ?? TimeSpan.Zero;
    }
}