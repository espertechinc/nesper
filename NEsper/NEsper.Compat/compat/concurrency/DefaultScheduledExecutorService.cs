///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Timers;

namespace com.espertech.esper.compat.concurrency
{
    public class DefaultScheduledExecutorService : IScheduledExecutorService
    {
        private IExecutorService _dispatchExecutorService;

        public DefaultScheduledExecutorService()
        {
            _dispatchExecutorService = Executors.DefaultExecutor();
        }

        public IFuture<object> Submit(Action runnable)
        {
            return _dispatchExecutorService.Submit(runnable);
        }

        public IFuture<T> Submit<T>(ICallable<T> callable)
        {
            return _dispatchExecutorService.Submit(callable);
        }

        public IFuture<T> Submit<T>(Func<T> callable)
        {
            return _dispatchExecutorService.Submit(callable);
        }

        public void Shutdown()
        {
            _dispatchExecutorService.Shutdown();
        }

        public void AwaitTermination()
        {
            _dispatchExecutorService.AwaitTermination();
        }

        public void AwaitTermination(TimeSpan timeout)
        {
            _dispatchExecutorService.AwaitTermination(timeout);
        }

        public bool IsShutdown => _dispatchExecutorService.IsShutdown;
        public bool IsTerminated => _dispatchExecutorService.IsTerminated;

        public IScheduledFuture ScheduleWithFixedDelay(
            Runnable runnable,
            TimeSpan initialDelay,
            TimeSpan periodicity)
        {
            return new MyScheduledFuture(
                this, runnable.Invoke,
                CreateInitialTimer(initialDelay),
                CreateIntervalTimer(periodicity));
        }

        public IScheduledFuture ScheduleAtFixedRate(
            Runnable action,
            TimeSpan initialDelay,
            TimeSpan periodicity)
        {
            return new MyScheduledFuture(
                this, action.Invoke, 
                CreateInitialTimer(initialDelay),
                CreateIntervalTimer(periodicity));
        }

        private Timer CreateIntervalTimer(TimeSpan timerDelay)
        {
            if (timerDelay == TimeSpan.Zero) {
                return null;
            }

            return new Timer {
                Interval = timerDelay.TotalMilliseconds,
                AutoReset = true,
                Enabled = false
            };
        }

        private Timer CreateInitialTimer(TimeSpan timerDelay)
        {
            if (timerDelay == TimeSpan.Zero) {
                return null;
            }

            return new Timer {
                Interval = timerDelay.TotalMilliseconds,
                AutoReset = false,
                Enabled = false
            };
        }

        internal class MyScheduledFuture : FutureBase
            , IScheduledFuture
            , IDisposable
        {
            private readonly IExecutorService _executorService;
            private readonly Action _action;
            private Timer _initialTimer;
            private Timer _intervalTimer;

            /// <summary>
            /// Initializes a new instance of the <see cref="MyScheduledFuture"/> class.
            /// </summary>
            /// <param name="executorService">the executor service.</param>
            /// <param name="action">The action.</param>
            /// <param name="initialTimer"></param>
            /// <param name="intervalTimer"></param>
            internal MyScheduledFuture(
                IExecutorService executorService,
                Action action,
                Timer initialTimer,
                Timer intervalTimer)
            {
                _executorService = executorService;
                _action = action;

                _initialTimer = initialTimer;
                _initialTimer.Elapsed += OnInitialTimerElapsed;

                _intervalTimer = intervalTimer;
                _intervalTimer.Elapsed += OnIntervalTimerElapsed;

                if (initialTimer != null) {
                    _initialTimer.Enabled = true;
                }
                else {
                    _intervalTimer.Enabled = true;
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                DisposeTimers();
            }

            private void DisposeTimers()
            {
                if (_initialTimer != null) {
                    _initialTimer.Enabled = false;
                    _initialTimer.Dispose();
                    _initialTimer = null;
                }

                if (_intervalTimer != null) {
                    _intervalTimer.Enabled = false;
                    _intervalTimer.Dispose();
                    _intervalTimer = null;
                }
            }

            /// <summary>
            /// Called when the interval timer elapses.  The action is dispatched to the executor service.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
            private void OnInitialTimerElapsed(
                object sender,
                ElapsedEventArgs e)
            {
                if (IsCanceledOrFinished) {
                    Dispose();
                }
                else {
                    _initialTimer.Enabled = false;
                    // We enable the interval timer before dispatching the action to the
                    // executor.  This ensures that the interval timer is as close to accurate
                    // as we expect.  If we need better accuracy, we will need to rely on a
                    // technique like that of the HarmonicTimer to get a more accurate time-frame.
                    _intervalTimer.Enabled = true;
                    _executorService.Submit(_action);
                }
            }

            /// <summary>
            /// Called when the interval timer elapses.  The action is dispatched to the executor service.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
            private void OnIntervalTimerElapsed(
                object sender,
                ElapsedEventArgs e)
            {
                // As long as we are not canceled or finished we will queue the
                // elapsed timer.  If the future has been canceled, then disable the
                // timer and dispose.
                if (IsCanceledOrFinished) {
                    Dispose();
                }
                else {
                    _executorService.Submit(_action);
                }
            }

            /// <summary>
            /// Invokes the implementation.
            /// </summary>
            /// <exception cref="IllegalStateException">Invoke() should not be called directly</exception>
            protected override void InvokeImpl()
            {
                throw new IllegalStateException("Invoke() should not be called directly");
            }

            /// <summary>
            /// Gets a value indicating whether this instance has value.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
            /// </value>
            public bool HasValue => false;

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <param name="timeOut">The time out.</param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public object GetValue(TimeSpan timeOut)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the value.  If a value is not available this method throw a InvalidOperationException.
            /// </summary>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public object Get()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <param name="units">The units.</param>
            /// <param name="timeUnit">The time unit.</param>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            public object GetValue(
                int units,
                TimeUnit timeUnit)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the value or default.
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            public object GetValueOrDefault()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Attempts to cancel the future.
            /// </summary>
            /// <param name="force">if set to <c>true</c> [force].</param>
            /// <returns></returns>
            public override bool Cancel(bool force)
            {
                // No reason why "this" scheduled future cannot be canceled.  However, we want to
                // use this opportunity to close the other elements.
                Dispose();
                return base.Cancel(force);
            }
        }
    }
}
