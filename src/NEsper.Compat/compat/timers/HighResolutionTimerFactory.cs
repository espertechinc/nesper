///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;

namespace com.espertech.esper.compat.timers
{
#if NETCOREAPP3_0_OR_GREATER
#else
    /// <summary>
    /// Implementation of the TimerFactory that uses the HighResolutionTimer.
    /// </summary>

    public class HighResolutionTimerFactory : ITimerFactory
    {
        /// <summary>
        /// Creates a timer.  The timer will begin after dueTime (in milliseconds)
        /// has passed and will occur at an interval specified by the period.
        /// </summary>
        /// <param name="timerCallback">the timer callback</param>
        /// <param name="offsetInMillis">offset in milliseconds</param>
        /// <param name="intervalInMillis">interval in milliseconds</param>
        /// <returns></returns>

        public ITimer CreateTimer(
            TimerCallback timerCallback,
            long offsetInMillis,
            long intervalInMillis)
        {
            return new HighResolutionTimer(
                timerCallback,
                null,
                offsetInMillis,
                intervalInMillis);
        }
    }
#endif
}
