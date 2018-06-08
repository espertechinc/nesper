///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.timers
{
    /// <summary>
    /// Implementation of the timer factory that uses the system timer.
    /// </summary>

    public class SystemTimerFactory : ITimerFactory
    {
        private readonly LinkedList<InternalTimer> _timerCallbackList = new LinkedList<InternalTimer>();
        private readonly Object _harmonicLock = new Object();
        private ITimer _harmonic;
        private long _currGeneration; // current generation
        private long _lastGeneration; // last generation that produced an event

        /// <summary>
        /// Disposable timer kept for internal purposes; cascades the timer effect.
        /// </summary>

        internal class InternalTimer : ITimer
        {
            internal TimerCallback TimerCallback;
            internal long NextTime;
            internal long Interval;
            internal SlimLock SlimLock;

            /// <summary>
            /// Initializes a new instance of the <see cref="InternalTimer"/> class.
            /// </summary>
            internal InternalTimer(long period, TimerCallback callback)
            {
                SlimLock = new SlimLock();
                TimerCallback = callback;
                Interval = period * 1000;
                NextTime = PerformanceObserver.MicroTime;
            }

            /// <summary>
            /// Called when [timer callback].
            /// </summary>
            /// <param name="currTime">The curr time.</param>
            internal void OnTimerCallback( long currTime )
            {
                if (!SlimLock.Enter(0)) {
                    return;
                }

                try
                {
                    while (true)
                    {
                        currTime = PerformanceObserver.MicroTime;
                        if (currTime < NextTime)
                        {
                            break;
                        }

                        NextTime += Interval;

                        var callback = TimerCallback;
                        if (callback != null) {
                            callback.Invoke(null);
                        }
                    }
                }
                finally {
                    SlimLock.Release();
                }
            }

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                TimerCallback = null;
            }

            #endregion
        }

        /// <summary>
        /// Creates the timer.
        /// </summary>
        private void CreateBaseTimer()
        {
            lock (_harmonicLock)
            {
                if (_harmonic == null)
                {
#if MONO
                    _harmonic = new HarmonicTimer(OnTimerEvent);
#else
                    _harmonic = new HighResolutionTimer(OnTimerEvent, null, 0, 10);
#endif
                }
            }
        }

        /// <summary>
        /// Determines whether the specified generations are idling.
        /// </summary>
        /// <param name="idleGenerations">The idle generations.</param>
        /// <returns>
        /// 	<c>true</c> if the specified idle generations is idling; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsIdling(long idleGenerations)
        {
            return idleGenerations == 1000;
        }

        /// <summary>
        /// Determines whether [is prune generation] [the specified generation].
        /// </summary>
        /// <param name="generation">The generation.</param>
        /// <returns>
        /// 	<c>true</c> if [is prune generation] [the specified generation]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPruneGeneration(long generation)
        {
            return generation%200 == 199;
        }

        /// <summary>
        /// Prunes dead callbacks.
        /// </summary>
        private void OnPruneCallbacks()
        {
            var deadList = new LinkedList<LinkedListNode<InternalTimer>>();
            var node = _timerCallbackList.First;
            while (node != null)
            {
                InternalTimer timer = node.Value;
                TimerCallback timerCallback = timer.TimerCallback;

                if (timerCallback == null)
                {
                    deadList.AddLast(node);
                }

                node = node.Next;
            }

            // Prune dead nodes
            foreach (LinkedListNode<InternalTimer> pnode in deadList)
            {
                _timerCallbackList.Remove(pnode);
            }      
        }

        /// <summary>
        /// Occurs when the timer event fires.
        /// </summary>
        /// <param name="userData">The user data.</param>
        private void OnTimerEvent(Object userData)
        {
            long curr = Interlocked.Increment(ref _currGeneration);

            LinkedListNode<InternalTimer> node = _timerCallbackList.First;
            if (node != null)
            {
                Interlocked.Exchange(ref _lastGeneration, curr);

                while (node != null)
                {
                    node.Value.OnTimerCallback(0);
                    //node.Value.OnTimerCallback(curr);
                    node = node.Next;
                }

                // Callbacks that are no longer in use are occassionally purged
                // from the list.  This is known as the prune cycle and occurs at
                // an interval determined by the IsPruneGeneration() method.
                if (IsPruneGeneration(curr))
                {
                    OnPruneCallbacks();
                }
            }
            else
            {
                long last = Interlocked.Read(ref _lastGeneration);

                // If the timer is running and doing nothing, we consider the
                // timer to be idling.  Idling only occurs when there are no callbacks,
                // so if the system is idling, we can actively shutdown the thread
                // timer.
                if (IsIdling(curr - last))
                {
                    lock (_harmonicLock)
                    {
                        if ( _harmonic != null )
                        {
                            _harmonic.Dispose();
                            _harmonic = null;
                        }
                    }
                }
            }
        }

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
            // Create a disposable timer that can be given back to the
            // caller.  The item is also used to track the lifetime of the
            // callback.
            var internalTimer = new InternalTimer(period, timerCallback);
            _timerCallbackList.AddLast(internalTimer);

            CreateBaseTimer();

            return internalTimer;
        }

        /// <summary>
        /// Thread-based timer that used a harmonic algorithm to ensure that
        /// the thread clicks at a regular interval.
        /// </summary>

        private class HarmonicTimer : ITimer
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
                        // up and down to match the interval set by the requestor.  As a result,
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

                Log.Debug( ".Run - timer thread stopping");
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
}
