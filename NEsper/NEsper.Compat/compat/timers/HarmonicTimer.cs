///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.timers
{
    /// <summary>
    /// Thread-based timer that used a harmonic algorithm to ensure that
    /// the thread clicks at a regular interval.
    /// </summary>

    public class HarmonicTimer : ITimer
    {
        private const int INTERNAL_CLOCK_SLIP = 100000;

        private readonly Guid _id;
        private Thread _thread;
        private readonly TimerCallback _timerCallback;
        private readonly long _tickAlign;
        private readonly long _tickPeriod;
        private bool _alive;

        /// <summary>
        /// Starts thread processing.
        /// </summary>

        private void Start()
        {
            Log.Debug(".Run - timer thread starting");

            long lTickAlign = _tickAlign;
            long lTickPeriod = _tickPeriod;

            try
            {
                Thread.BeginThreadAffinity();

                while (_alive)
                {
                    // Check the tickAlign to determine if we are here "too early"
                    // The CLR is a little sloppy in the way that thread timers are handled.
                    // In Java, when a timer is setup, the timer will adjust the interval
                    // up and down to match the interval set by the requester.  As a result,
                    // you will can easily see intervals between calls that look like 109ms,
                    // 94ms, 109ms, 94ms.  This is how the JVM ensures that the caller gets
                    // an average of 100ms.  The CLR however will provide you with 109ms,
                    // 109ms, 109ms, 109ms.  Eventually this leads to slip in the timer.
                    // To account for that we under allocate the timer interval by some
                    // small amount and allow the thread to sleep a wee-bit if the timer
                    // is too early to the next clock cycle.

                    long currTickCount = DateTime.Now.Ticks;
                    long currDelta = lTickAlign - currTickCount;

                    //Log.Info("Curr: {0} {1} {2}", currDelta, currTickCount, Environment.TickCount);

                    while (currDelta > INTERNAL_CLOCK_SLIP)
                    {
                        if (currDelta >= 600000)
                            Thread.Sleep(1); // force-yield quanta
                        else
                            Thread.SpinWait(20);

                        currTickCount = DateTime.Now.Ticks;
                        currDelta = lTickAlign - currTickCount;

                        //Log.Info("Curr: {0} {1} {2}", currDelta, currTickCount, Environment.TickCount);
                    }

                    lTickAlign += lTickPeriod;
                    _timerCallback(null);
                }
            }
            catch (ThreadInterruptedException)
            {
                Thread.EndThreadAffinity();
            }

            Log.Debug(".Run - timer thread stopping");
        }

        /// <summary>
        /// Creates the timer and wraps it
        /// </summary>
        /// <param name="timerCallback"></param>

        public HarmonicTimer(TimerCallback timerCallback)
        {
            _id = Guid.NewGuid();
            _alive = true;

            _timerCallback = timerCallback;

            _tickPeriod = 100000;
            _tickAlign = DateTime.Now.Ticks;

            _thread = new Thread(Start);
            _thread.Priority = ThreadPriority.AboveNormal;
            _thread.IsBackground = true;
            _thread.Name = "ThreadBasedTimer{" + _id + "}";
            _thread.Start();
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// </summary>

        ~HarmonicTimer()
        {
            Dispose();
        }

        /// <summary>
        /// Cleans up system resources
        /// </summary>

        public void Dispose()
        {
            _alive = false;

            if (_thread != null)
            {
                _thread.Interrupt();
                _thread = null;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}