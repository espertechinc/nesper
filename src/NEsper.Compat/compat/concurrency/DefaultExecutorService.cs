///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Class that provides access to threadPool like services.  This class exists to
    /// provide an easier bridge between the CLR thread pool and the JVM thread pool
    /// mechanisms.
    /// </summary>

    public class DefaultExecutorService : IExecutorService, IDisposable
    {
        private readonly Guid _id;
        private readonly List<FutureBase> _futuresPending;
        private bool _isActive;
        private bool _isShutdown;
        private long _numExecuted;
        private long _numSubmitted;
        private long _numRecycled;

        private TaskFactory _taskFactory;

        /// <summary>
        /// Gets the number of items executed.
        /// </summary>
        /// <value>The num executed.</value>
        public int NumExecuted => (int) Interlocked.Read(ref _numExecuted);

        public int NumSubmitted => (int)Interlocked.Read(ref _numSubmitted);

        public bool IsShutdown => _isShutdown;

        public bool IsTerminated => _isShutdown && !_isActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExecutorService"/> class.
        /// </summary>
        /// <param name="taskFactory">The task factory.</param>
        public DefaultExecutorService(TaskFactory taskFactory)
        {
            _id = Guid.NewGuid();
            _futuresPending = new List<FutureBase>();
            _isActive = true;
            _isShutdown = false;
            _numExecuted = 0;
            _numSubmitted = 0;
            _numRecycled = 0;
            _taskFactory = taskFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExecutorService"/> class.
        /// </summary>
        public DefaultExecutorService(TaskScheduler taskScheduler)
        {
            _id = Guid.NewGuid();
            _futuresPending = new List<FutureBase>();
            _isActive = true;
            _isShutdown = false;
            _numExecuted = 0;
            _numSubmitted = 0;
            _numRecycled = 0;
            _taskFactory = new DisposableTaskFactory(taskScheduler);
        }

        public void Dispose()
        {
            if (_taskFactory is IDisposable disposableFactory) {
                disposableFactory.Dispose();
            }

            _taskFactory = null;
        }

        /// <summary>
        /// Dispatches the future.
        /// </summary>
        private void DispatchFuture(object userData)
        {
            var future = userData as FutureBase;

            try
            {
                if (_isActive)
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info(".DispatchFuture - Instance {0} dispatching item", _id);
                    }

                    if (future != null)
                    {
                        future.Invoke();
                        Interlocked.Increment(ref _numExecuted);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(".DispatchFuture - Instance {0} failed", _id);
                Log.Error(".DispatchFuture", e);
            }
            finally {
                Recycle(future);
            }
        }

        private void Recycle(FutureBase future)
        {
            Log.Info(".Recycle - Instance {0} starting", _id);
            lock (_futuresPending) {
                using (new Tracer(Log, "Recycle(lock)")) {
                    _futuresPending.Remove(future);

                    if (Log.IsInfoEnabled) {
                        Log.Info(
                            ".Recycle - Instance {0} done dispatching: {1} pending",
                            _id,
                            _futuresPending.Count);
                    }

                    if (_futuresPending.Count == 0) {
                        Monitor.PulseAll(_futuresPending);
                    }
                }
            }

            Interlocked.Increment(ref _numRecycled);
        }

        /// <summary>
        /// Submits the specified runnable to the thread pool.
        /// </summary>
        /// <param name="runnable">The runnable.</param>
        public IFuture<object> Submit(Action runnable)
        {
            Func<object> function =
                () => { runnable.Invoke(); return null; };
            return Submit(function);
        }

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        public IFuture<T> Submit<T>(ICallable<T> callable)
        {
            Func<T> function = callable.Call;
            return Submit(function);
        }

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        public IFuture<T> Submit<T>(Func<T> callable)
        {
            if (_isShutdown) {
                throw new IllegalStateException("ExecutorService is shutdown");
            }

            Interlocked.Increment(ref _numSubmitted);

            var future = new FutureImpl<T> {Callable = callable};

            lock (_futuresPending) {
                using (new Tracer(Log, "Submit(lock)")) {
                    _futuresPending.Add(future);

                    if (Log.IsInfoEnabled) {
                        Log.Info(
                            ".Submit - Instance {0} queued user work item: {1} pending",
                            _id,
                            _futuresPending.Count);
                    }
                }
            }

            Task task = _taskFactory.StartNew(() => DispatchFuture(future), TaskCreationOptions.None);
            Log.Info(".Submit - Queued task {0}", task);

            return future;
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info(".Shutdown - Marking instance {0} to avoid further queuing", _id);
            }

            // Mark the executor as inactive so that we don't take any new callables.
            _isShutdown = true;

            // Dispose the taskFactory
            if (_taskFactory?.Scheduler is IDisposable disposableScheduler) {
                disposableScheduler.Dispose();
            }

            _taskFactory = null;
        }

        /// <summary>
        /// Awaits the termination.
        /// </summary>
        public void AwaitTermination()
        {
            AwaitTermination(new TimeSpan(0, 0, 15));
        }

        public void AwaitTermination(int units, TimeUnit timeUnit)
        {
            AwaitTermination(TimeUnitHelper.ToTimeSpan(units, timeUnit));
        }

        /// <summary>
        /// Awaits the termination.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AwaitTermination(TimeSpan timeout)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info(
                    ".AwaitTermination - Instance {0} waiting for {1} tasks to complete",
                    _id, _futuresPending.Count);
            }

            var head = PerformanceObserver.MilliTime;
            var tail = head + (long) timeout.TotalMilliseconds;

            lock (_futuresPending) {
                using (new Tracer(Log, "AwaitTermination(lock)")) {
                    long remain = tail - PerformanceObserver.MilliTime;

                    while ((_futuresPending.Count != 0) && (remain > 0))
                    {
                        if (Log.IsInfoEnabled)
                        {
                            Log.Info(
                                ".AwaitTermination - Instance {0} entering wait",
                                _id,
                                _futuresPending.Count);
                        }


                        Monitor.Wait(_futuresPending, TimeSpan.FromMilliseconds(remain));
                        remain = tail - PerformanceObserver.MilliTime;
                    }

                    if (_futuresPending.Count != 0) {
                        _futuresPending.ForEach(
                            futureBase => {
                                Log.Warn(".AwaitTermination - Forceably terminating future");
                                futureBase.Kill();
                            });
                    }
                }
            }

            _isActive = false;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
