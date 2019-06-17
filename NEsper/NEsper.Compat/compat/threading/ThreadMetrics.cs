///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class ThreadMetrics
    {
        /// <summary>
        /// Continual counter that tracks the number of locks that have been
        /// acquired on the thread.
        /// </summary>

        [ThreadStatic] private static long _locksAcquired;

        /// <summary>
        /// Gets the # of times locks have been acquired by this thread.
        /// </summary>
        /// <value>The locks acquired.</value>
        public static long LocksAcquired
        {
            get { return _locksAcquired; }
        }

        /// <summary>
        /// Increments the # of times locks have been acquired.
        /// </summary>
        /// <returns></returns>
        public static long Increment()
        {
            return Interlocked.Increment(ref _locksAcquired);
        }
    }
}
