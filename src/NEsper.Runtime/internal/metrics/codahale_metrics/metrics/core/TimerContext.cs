///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A timing context.
    /// </summary>
    /// <unknown>@see Timer#time()</unknown>
    public class TimerContext
    {
        private readonly Clock clock;
        private readonly long startTime;
        private readonly Timer timer;

        /// <summary>
        ///     Creates a new <seealso cref="TimerContext" /> with the current time as its starting value and with the
        ///     given <seealso cref="Timer" />.
        /// </summary>
        /// <param name="timer">the <seealso cref="Timer" /> to report the elapsed time to</param>
        /// <param name="clock">the clock</param>
        internal TimerContext(
            Timer timer,
            Clock clock)
        {
            this.timer = timer;
            this.clock = clock;
            startTime = clock.Tick;
        }

        /// <summary>
        ///     Stops recording the elapsed time and updates the timer.
        /// </summary>
        public void Stop()
        {
            timer.Update(clock.Tick - startTime, TimeUnit.NANOSECONDS);
        }
    }
} // end of namespace