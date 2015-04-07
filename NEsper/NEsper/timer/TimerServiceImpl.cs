///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.timer
{
    /// <summary>
    /// Implementation of the internal clocking service interface.
    /// </summary>

    public class TimerServiceImpl : TimerService
    {
        private ITimer timer ;
        private TimerCallback timerCallback;
        private EPLTimerTask timerTask;
        private bool timerTaskCancelled;

        /// <summary>
        /// Set the callback method to invoke for clock ticks.
        /// </summary>
        /// <value></value>

        public TimerCallback Callback
        {
            set { this.timerCallback = value; }
        }

        /// <summary>
        /// Gets the msec timer resolution.
        /// </summary>
        /// <value>The msec timer resolution.</value>
        public long MsecTimerResolution { get; private set; }

        /// <summary>
        /// Returns a flag indicating whether statistics are enabled.
        /// </summary>

        public bool AreStatsEnabled
        {
            get { return timerTask.EnableStats; }
            set { timerTask.EnableStats = value; }
        }

        public long MaxDrift
        {
            get { return timerTask.MaxDrift; }
        }

        public long LastDrift
        {
            get { return timerTask.LastDrift; }
        }

        public long TotalDrift
        {
            get { return timerTask.TotalDrift; }
        }

        ///<summary>
        /// Gets the number of times the timer has been invoked.
        ///</summary>
        public long InvocationCount
        {
            get { return timerTask.InvocationCount; }
        }

        /// <summary>
        /// Gets the unique id for the timer.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the engine URI.
        /// </summary>
        /// <value>The engine URI.</value>
        public string EngineURI { get; private set; }

        /// <summary> Constructor.</summary>
        /// <param name="msecTimerResolution">is the millisecond resolution or interval the internal timer thread processes schedules</param>
        /// <param name="engineURI">engine URI</param>
        public TimerServiceImpl(String engineURI, long msecTimerResolution)
        {
            Id = Guid.NewGuid();
            EngineURI = engineURI;
            MsecTimerResolution = msecTimerResolution;
            timerTaskCancelled = false;
        }

        /// <summary>
        /// Handles the timer event
        /// </summary>
        /// <param name="state">The user state object.</param>

        private void OnTimerElapsed(Object state)
        {
            if (! timerTaskCancelled)
            {
                if (timerCallback != null)
                {
                    timerCallback();
                }
            }
        }

        /// <summary>
        /// Start clock expecting callbacks at regular intervals and a fixed rate.
        /// Catch-up callbacks are possible should the callback fall behind.
        /// </summary>
        public void StartInternalClock()
        {
            if (timer != null)
            {
                Log.Warn(".StartInternalClock Internal clock is already started, stop first before starting, operation not completed");
                return;
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".StartInternalClock Starting internal clock daemon thread, resolution=" + MsecTimerResolution);
            }

            if (timerCallback == null)
            {
                throw new IllegalStateException("Timer callback not set");
            }

            timerTask = new EPLTimerTask(timerCallback);

            timerTaskCancelled = false;
            timer = TimerFactory.DefaultTimerFactory.CreateTimer(
                OnTimerElapsed, MsecTimerResolution);
        }

        /// <summary>
        /// Stop internal clock.
        /// </summary>
        /// <param name="warnIfNotStarted">use true to indicate whether to warn if the clock is not Started, use false to not warn
        /// and expect the clock to be not Started.</param>
        public void StopInternalClock(bool warnIfNotStarted)
        {
            if (timer == null)
            {
                if (warnIfNotStarted)
                {
                    Log.Warn(".StopInternalClock Internal clock is already Stopped, Start first before Stopping, operation not completed");
                }
                return;
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".StopInternalClock Stopping internal clock daemon thread");
            }

            timerTaskCancelled = true;
            timer.Dispose();

            try
            {
                // Sleep for at least 100 ms to await the internal timer
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException)
            {
            }

            timer = null;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
