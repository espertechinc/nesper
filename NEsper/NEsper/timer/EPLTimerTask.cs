///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Timers;
using com.espertech.esper.compat.logging;


namespace com.espertech.esper.timer
{
    /// <summary>
    /// Timer task to simply invoke the callback when triggered.
    /// </summary>

    sealed class EPLTimerTask
    {
        private readonly TimerCallback _timerCallback;
        private bool _isCancelled;

        internal bool EnableStats;
        internal long LastDrift;
        internal long MaxDrift;
        internal long TotalDrift;
        internal long InvocationCount;

        public bool Cancelled
        {
            set { _isCancelled = value; }
        }

        public EPLTimerTask(TimerCallback callback)
        {
            _timerCallback = callback;
            EnableStats = false;
            LastDrift = 0;
            MaxDrift = 0;
            TotalDrift = 0;
            InvocationCount = 0;
        }

        public void Run(object sender, ElapsedEventArgs e)
        {
            if (!_isCancelled)
            {
                if (EnableStats)
                {
                    // If we are called early, then delay will be positive. If we are called late, then the delay will be negative.
                    // NOTE: don't allow _enableStats to be set until future has been set
                    LastDrift = 0; // no drift detection
                    TotalDrift += LastDrift;
                    InvocationCount++;
                    if (LastDrift > MaxDrift)
                        MaxDrift = LastDrift;
                }

                try
                {
                    _timerCallback();
                }
                catch(Exception ex)
                {
                    Log.Error("Timer thread caught unhandled exception: " + ex.Message, ex);
                } 
                
            }
        }

        internal void ResetStats()
        {
            InvocationCount = 0;
            LastDrift = 0;
            TotalDrift = 0;
            MaxDrift = 0;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
