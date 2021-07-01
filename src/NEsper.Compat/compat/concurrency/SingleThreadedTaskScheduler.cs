///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Class that provides access to threadPool like services.  This class exists to
    /// provide an easier bridge between the CLR thread pool and the JVM thread pool
    /// mechanisms.
    /// </summary>

    public class SingleThreadedTaskScheduler : TaskScheduler
    {
        private readonly Guid _id;
        private readonly Thread _thread;
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

        public int NumSubmitted => (int)Interlocked.Read(ref _numSubmitted);

        public SingleThreadedTaskScheduler()
        {
            _id = Guid.NewGuid();
            _tasks = new LinkedList<Task>();
            _isActive = true;
            _isShutdown = false;
            _numExecuted = 0;
            _numSubmitted = 0;

            _thread = new Thread(this.Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void QueueTask(Task task)
        {
            if (_isShutdown) {
                throw new IllegalStateException("task scheduler is shutdown");
            }

            Interlocked.Increment(ref _numSubmitted);

            lock (_tasks) {
                if (!TryExecuteTaskInline(task, false)) {
                    _tasks.AddLast(task);
                    Monitor.Pulse(_tasks);
                }
            }
        }

        protected override bool TryExecuteTaskInline(
            Task task,
            bool taskWasPreviouslyQueued)
        {
            if (Thread.CurrentThread == _thread) {
                try {
                    task.RunSynchronously(this);
                }
                finally {
                    Interlocked.Increment(ref _numExecuted);
                }

                return true;
            }

            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (_tasks) {
                return new ReadOnlyCollection<Task>(_tasks);
            }
        }

        protected void Run()
        {
            while (_isActive) {
                try {
                    Task task;

                    lock (_tasks) {
                        if (_tasks.Count == 0) {
                            // if the task queue is drained and we are set to shutdown, then
                            // exit the loop
                            if (_isShutdown) {
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
                    finally {
                        Interlocked.Increment(ref _numExecuted);
                    }
                }
                catch (Exception e) {
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
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
