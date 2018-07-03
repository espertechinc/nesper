///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat.threading;

namespace NEsper.Benchmark.Server
{
    public class ThreadPoolExecutor : Executor
    {
        private int _eventQueueDepth;
        private readonly ILockable _eventMonitor;
        private readonly int _maxSize;
        private bool _isHandlingEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolExecutor"/> class.
        /// </summary>
        public ThreadPoolExecutor(int? maxQueueSize)
        {
            _maxSize = maxQueueSize ?? Int32.MaxValue;
            _eventQueueDepth = 0;
            _eventMonitor = new MonitorSpinLock(60000);
            _isHandlingEvents = true;
        }

        /// <summary>
        /// Gets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get
            {
                int workerThreads;
                int completionThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out completionThreads);
                return workerThreads;
            }
        }

        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>The queue depth.</value>
        public int QueueDepth
        {
            get { return _eventQueueDepth; }
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute( WaitCallback waitCallback ) 
        {
            using (_eventMonitor.Acquire())
            {
                while (_isHandlingEvents && (_eventQueueDepth > _maxSize))
                {
                    Thread.Sleep(1);
                }

                if (!_isHandlingEvents)
                {
                    return;
                }
            }

            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    try
                    {
                        waitCallback.Invoke(null);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("ThreadPoolExecutor: Event threw exception '{0}'", e.GetType());
                        Console.Error.WriteLine("ThreadPoolExecutor: Error message: {0}", e.Message);
                        Console.Error.WriteLine(e.StackTrace);
                    }
                    finally
                    {
                        using(_eventMonitor.Acquire())
                        {
                            _eventQueueDepth++;
                        }
                    }
                });
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _isHandlingEvents = false;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _isHandlingEvents = true;
        }
    }
}
