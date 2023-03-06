///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.timers
{
    public class PeriodicTimerFactory : ITimerFactory
    {
        public ITimer CreateTimer(
            TimerCallback timerCallback,
            long offsetInMillis,
            long intervalInMillis)
        {
            return new PeriodicTimeImpl(timerCallback, offsetInMillis, intervalInMillis);
        }

        private class PeriodicTimeImpl : ITimer
        {
            private PeriodicTimer _timer;
            private bool _isRunning;
            private readonly TimerCallback _timerCallback;
            private readonly TimeSpan _offset;
            private Task _task;

            internal PeriodicTimeImpl(
                TimerCallback timerCallback,
                long offsetInMillis,
                long intervalInMillis)
            {
                _isRunning = true;
                _timerCallback = timerCallback;
                _offset = TimeSpan.FromMilliseconds(offsetInMillis);
                _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalInMillis));
                _task = Task.Run(ExecuteTimer);
            }

            async void ExecuteTimer()
            {
                Console.WriteLine("Starting");
                
                if (_offset != TimeSpan.Zero) {
                    await Task.Delay(_offset);
                    Console.WriteLine("ExecuteTimer: Delay Fire: {0}", _offset);
                    _timerCallback(this);
                }

                while (_isRunning && await _timer.WaitForNextTickAsync()) {
                    Console.WriteLine("ExecuteTimer: Fire");
                    _timerCallback(this);
                }
            }

            public void Dispose()
            {
                Console.WriteLine("Dispose");
                
                _isRunning = false;
                _timer?.Dispose();
                _timer = null;
                _task?.Dispose();
                _task = null;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}