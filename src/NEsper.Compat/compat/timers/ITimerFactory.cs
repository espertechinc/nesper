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
    /// <summary>
    /// Factory object that creates timers.
    /// </summary>

    public interface ITimerFactory
    {
        /// <summary>
        /// Creates a timer.  The timer will begin after dueTime (in milliseconds)
        /// has passed and will occur at an interval specified by the period.
        /// </summary>
        /// <param name="timerCallback">timer callback</param>
        /// <param name="offsetInMillis">timer offset in milliseconds</param>
        /// <param name="intervalInMillis">interval in milliseconds</param>
        /// <returns></returns>

        ITimer CreateTimer(
            TimerCallback timerCallback,
            long offsetInMillis,
            long intervalInMillis);
    }
}
