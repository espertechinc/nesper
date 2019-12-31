///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.concurrency
{
    public interface IScheduledExecutorService : IExecutorService
    {
        /// <summary>
        /// Schedules a runnable task associated with an initial delay and periodicity.
        /// </summary>
        /// <param name="runnable">The runnable.</param>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="periodicity">The periodicity.</param>
        IScheduledFuture ScheduleWithFixedDelay(
            Runnable runnable,
            TimeSpan initialDelay,
            TimeSpan periodicity);

        IScheduledFuture ScheduleAtFixedRate(
            Runnable action,
            TimeSpan initialDelay,
            TimeSpan periodicity);
    }
}