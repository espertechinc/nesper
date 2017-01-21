///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Implementation of the TimerFactory that uses the HighResolutionTimer.
    /// </summary>

    public class HighResolutionTimerFactory : ITimerFactory
    {
        /// <summary>
        /// Creates a timer.  The timer will begin after dueTime (in milliseconds)
        /// has passed and will occur at an interval specified by the period.
        /// </summary>
        /// <param name="timerCallback"></param>
        /// <param name="period"></param>
        /// <returns></returns>

        public ITimer CreateTimer(
            TimerCallback timerCallback,
            long period)
        {
            return new HighResolutionTimer(
                timerCallback,
                null,
                0,
                period);
        }
    }
}
