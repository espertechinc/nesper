///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.timer
{
	/// <summary>
	/// Service interface for repeated callbacks at regular intervals.
	/// </summary>

    public interface TimerService
    {
        /// <summary> Set the callback method to invoke for clock ticks.</summary>
        TimerCallback Callback { set; }

        /// <summary> Start clock expecting callbacks at regular intervals and a fixed rate.
        /// Catch-up callbacks are possible should the callback fall behind.
        /// </summary>
        void StartInternalClock();

        /// <summary> Stop internal clock.</summary>
        /// <param name="warnIfNotStarted">use true to indicate whether to warn if the clock is not Started, use false to not warn
        /// and expect the clock to be not Started. 
        /// </param>
        void StopInternalClock(bool warnIfNotStarted);

        /// <summary>
        /// Returns a flag indicating whether statistics are enabled.
        /// </summary>
        bool AreStatsEnabled { get; set; }

        /// <summary>
        /// Gets the maximum drift.
        /// </summary>
        long MaxDrift { get; }

        /// <summary>
        /// Gets the last drift.
        /// </summary>

        long LastDrift { get; }

        /// <summary>
        /// Gets the total drift.
        /// </summary>

        long TotalDrift { get; }

        ///<summary>
        /// Gets the number of times the timer has been invoked.
        ///</summary>

        long InvocationCount { get; }
    }
}
