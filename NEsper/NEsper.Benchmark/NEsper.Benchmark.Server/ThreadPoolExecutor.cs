///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;

namespace NEsper.Benchmark.Server
{
    public class ThreadPoolExecutor : Executor
    {
        private int eventQueueDepth;
        private readonly ILockable eventMonitor;
        private readonly int maxSize;
        private bool isHandlingEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolExecutor"/> class.
        /// </summary>
        public ThreadPoolExecutor(int? maxQueueSize)
        {
            maxSize = maxQueueSize ?? Int32.MaxValue;
            eventQueueDepth = 0;
            eventMonitor = new MonitorSpinLock();
            isHandlingEvents = true;
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
            get { return eventQueueDepth; }
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute( WaitCallback waitCallback ) 
        {
            using (eventMonitor.Acquire())
            {
                while (isHandlingEvents && (eventQueueDepth > maxSize))
                {
                    Thread.Sleep(1);
                }

                if (!isHandlingEvents)
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
                        using(eventMonitor.Acquire())
                        {
                            eventQueueDepth++;
                        }
                    }
                });
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            isHandlingEvents = false;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            isHandlingEvents = true;
        }
    }
}
