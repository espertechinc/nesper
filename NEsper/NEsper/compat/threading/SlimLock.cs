///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// Implements a simple spinLock algorithm.  The spinLock will attempt
    /// to exchange a value atomically.  If the exchange can not be done then
    /// the spinLock will enter a loop for a maximum amount of time as
    /// specified.  In the loop it will use a spinWait to allow the CPU to
    /// idle for a few cycles in an attempt to wait for the resource to be
    /// freed up.  If after a number of attempts the resource has not been
    /// freed, the spinLock will give up its quanta using a sleep.  The sleep
    /// will force the thread to yield and if all goes well releases the thread
    /// (which may be on the same processor) to release the critical resource.
    /// There's no reason to use this as a general purpose lock, monitors do
    /// just fine.
    /// </summary>

    public sealed class SlimLock
    {
        private int _myLockDepth;
        private Thread _myLockThread;

        /// <summary>
        /// Acquires the lock.  If the lock can be acquired immediately
        /// it does so.  In the event that the lock can not be acquired
        /// the lock will use a spin-lock algorithm to acquire the lock.
        /// </summary>

        public bool Enter(int timeoutInMillis)
        {
            Thread thread = Thread.CurrentThread;

            if (_myLockThread == thread)
            {
                _myLockDepth++;
                return true;
            }

            if (Interlocked.CompareExchange(ref _myLockThread, thread, null) == null)
            {
                _myLockDepth = 1;
                return true;
            }

            return EnterMyLockSpin(thread, timeoutInMillis);
        }

        /// <summary>
        /// Acquires the lock.  If the lock can be acquired immediately
        /// it does so.  In the event that the lock can not be acquired
        /// the lock will use a spin-lock algorithm to acquire the lock.
        /// </summary>

        public void Enter()
        {
            Thread thread = Thread.CurrentThread;
            if (_myLockThread == thread)
            {
                _myLockDepth++;
                return;
            }

            if (Interlocked.CompareExchange(ref _myLockThread, thread, null) == null)
            {
                _myLockDepth = 1;
                return;
            }

            EnterMyLockSpin(thread);
        }

        private void EnterMyLockSpin(Thread thread)
        {
            bool isMultiProcessor = IsMultiProcessor;

            for (int i = 0; ; i++)
            {
                if (i < 3 && isMultiProcessor)
                {
                    Thread.SpinWait(20);    // Wait a few dozen instructions to let another processor release lock. 
                }
                else
                {
                    Thread.Sleep(0);        // Give up my quantum.  
                }

                if (Interlocked.CompareExchange(ref _myLockThread, thread, null) == null)
                {
                    _myLockDepth = 1;
                    return;
                }
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>
        /// Enters the lock spin with a timeout.  Returns true if the
        /// lock was acquired within the time allotted.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="timeoutInMillis">The timeout in millis.</param>
        /// <returns></returns>

        private bool EnterMyLockSpin(Thread thread, int timeoutInMillis)
        {
            var isMultiProcessor = IsMultiProcessor;

            long time;

            QueryPerformanceCounter(out time);

            var factor = PerformanceObserverWin.MpMilli;
            var begTime = time;
            var endTime = begTime + timeoutInMillis / factor;

            if (isMultiProcessor)
            {
                if (EnterMyLockSpinWait(thread)) return true;
                if (EnterMyLockSpinWait(thread)) return true;
                if (EnterMyLockSpinWait(thread)) return true;

                QueryPerformanceCounter(out time);
                if (time > endTime) return false;
            }

            if (EnterMyLockSleep(thread, 0)) return true;
            if (EnterMyLockSleep(thread, 0)) return true;
            if (EnterMyLockSleep(thread, 0)) return true;

            QueryPerformanceCounter(out time);
            if (time > endTime) return false;

            while (true)
            {
                if (EnterMyLockSleep(thread, 10)) return true;

                QueryPerformanceCounter(out time);
                if (time > endTime) return false;
            }
        }

        private bool EnterMyLockSpinWait(Thread thread)
        {
            Thread.SpinWait(20);
            if (Interlocked.CompareExchange(ref _myLockThread, thread, null) == null)
            {
                _myLockDepth = 1;
                return true;
            }

            return false;
        }

        private bool EnterMyLockSleep(Thread thread, int sleep)
        {
            Thread.Sleep(sleep);
            if (Interlocked.CompareExchange(ref _myLockThread, thread, null) == null)
            {
                _myLockDepth = 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Releases the lock, allowing waiters to proceed.
        /// </summary>

        public void Release()
        {
            if (--_myLockDepth == 0)
            {
                Interlocked.Exchange(ref _myLockThread, null);
            }
        }

        public static readonly bool IsMultiProcessor;

        private static readonly int SpinWait0;
        private static readonly int SpinWait1;
        private static readonly int SpinWait2;
        private static readonly int SpinWait3;

        static SlimLock()
        {
            IsMultiProcessor = Environment.ProcessorCount > 1;

            SpinWait0 = PerformanceObserverWin.SpinIterationsPerMicro * 5;
            SpinWait1 = PerformanceObserverWin.SpinIterationsPerMicro * 10;
            SpinWait2 = PerformanceObserverWin.SpinIterationsPerMicro * 25;
            SpinWait3 = PerformanceObserverWin.SpinIterationsPerMicro * 50;
        }

        public static void SmartWait(int iter)
        {
            if (iter <= 10) Thread.SpinWait(SpinWait0);
            else if (iter <= 30) Thread.SpinWait(SpinWait1);
            else if (iter <= 60) Thread.SpinWait(SpinWait2);
            else if (iter <= 100) Thread.SpinWait(SpinWait3);
            else Thread.Sleep(0);
        }

        public static bool SmartWait(int iter, long timeEnd)
        {
            if (iter <= 10) Thread.SpinWait(SpinWait0);       // 0.05 ms
            else if (iter <= 30) Thread.SpinWait(SpinWait1);  // 0.35 ms
            else if (iter <= 60) Thread.SpinWait(SpinWait2);  // 1.85 ms
            else if ((iter % 10) == 0)
            {
                if (DateTimeHelper.CurrentTimeMillis > timeEnd)
                    return false;
            }
            else if (iter <= 100) Thread.SpinWait(SpinWait3);
            else Thread.Sleep(0);

            return true;
        }
    }
}
