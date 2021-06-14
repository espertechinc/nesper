///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Class that provides access to threadPool like services.  This class exists to
    /// provide an easier bridge between the CLR thread pool and the JVM thread pool
    /// mechanisms.
    /// </summary>

    public class MultiThreadedTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly Guid _id;
        private readonly List<Thread> _threads;
        private readonly LinkedList<Task> _tasks;
        private bool _isActive;
        private bool _isShutdown;
        private long _numExecuted;
        private long _numSubmitted;

        /// <summary>
        /// Gets the number of items executed.
        /// </summary>
        /// <value>The num executed.</value>
        public int NumExecuted => (int) Interlocked.Read(ref _numExecuted);

        public int NumSubmitted => (int) Interlocked.Read(ref _numSubmitted);

        public MultiThreadedTaskScheduler(
            int threadCount,
            ThreadFactory threadFactory)
        {
            _id = Guid.NewGuid();
            _tasks = new LinkedList<Task>();
            _isActive = true;
            _isShutdown = false;
            _numExecuted = 0;
            _numSubmitted = 0;
            _threads = ThreadRange(threadCount, threadFactory);
            _threads.ForEach(_ => _.Start());
        }

        public MultiThreadedTaskScheduler(int threadCount)
            : this(threadCount, DefaultThreadFactory())
        {
        }

        private static ThreadFactory DefaultThreadFactory()
        {
            return _ => new Thread(_)
            {
                IsBackground = true,
                Name = "MultiThreadedTaskScheduler-Thread-" + _
            };
        }

        private List<Thread> ThreadRange(
            int numThreads,
            ThreadFactory threadFactory)
        {
            return Enumerable.Range(0, numThreads)
                .Select(_ => threadFactory.Invoke(Run))
                .ToList();
        }

        public void Dispose()
        {
            Shutdown();
        }

        protected override void QueueTask(Task task)
        {
            if (_isShutdown)
            {
                throw new IllegalStateException("task scheduler is shutdown");
            }

            Interlocked.Increment(ref _numSubmitted);

            lock (_tasks)
            {
                _tasks.AddLast(task);
                Monitor.Pulse(_tasks);
            }
        }

        protected override bool TryExecuteTaskInline(
            Task task,
            bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (_tasks)
            {
                return new ReadOnlyCollection<Task>(_tasks);
            }
        }

        protected void Run()
        {
            while (_isActive)
            {
                try
                {
                    Task task;

                    lock (_tasks)
                    {
                        if (_tasks.Count == 0)
                        {
                            // if the task queue is drained and we are set to shutdown, then
                            // exit the loop
                            if (_isShutdown)
                            {
                                _isActive = false;
                                return;
                            }

                            // otherwise, sleep until there are some new tasks or until the allotted
                            // amount of time has elapsed.
                            Monitor.Wait(_tasks, 1000);
                            continue;
                        }

                        task = _tasks.First.Value;
                        _tasks.RemoveFirst();
                    }

                    try {
                        base.TryExecuteTask(task);
                    }
                    finally
                    {
                        Interlocked.Increment(ref _numExecuted);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Exception while processing on thread", e);
                }
            }
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

            _isShutdown = true;

            // Wait for the threads to shutdown
            for (int ii = 0; ii < _threads.Count; ii++) {
                var thread = _threads[ii];
                if (thread != null) {
                    _threads[ii] = null;
                    if (!thread.Join(TimeSpan.FromSeconds(10))) {
                        thread.Interrupt();
                        thread.Join(TimeSpan.FromSeconds(10));
                    }
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}