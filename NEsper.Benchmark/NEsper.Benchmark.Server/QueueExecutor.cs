///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

using com.espertech.esper.compat.collections;

namespace NEsper.Benchmark.Server
{
    public class QueueExecutor : Executor
    {
        private readonly Thread[] eventHandleThreads;
        private bool isHandlingEvents;
        private readonly IBlockingQueue<WaitCallback> eventQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueExecutor"/> class.
        /// </summary>
        /// <param name="numThreads">The num threads.</param>
        public QueueExecutor(int numThreads)
        {
            eventQueue = new ImperfectBlockingQueue<WaitCallback>();

            isHandlingEvents = true;
            eventHandleThreads = new Thread[numThreads];
            for (var ii = 0; ii < eventHandleThreads.Length; ii++)
            {
                eventHandleThreads[ii] = new Thread(ProcessEventQueue);
                eventHandleThreads[ii].Name = "EsperEventHandler-" + ii;
                eventHandleThreads[ii].IsBackground = true;
                eventHandleThreads[ii].Start();
            }
        }

        /// <summary>
        /// Gets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get { return eventHandleThreads.Length; }
        }

        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>The queue depth.</value>
        public int QueueDepth
        {
            get { return eventQueue.Count; }
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute(WaitCallback waitCallback)
        {
            eventQueue.Push(waitCallback);
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            isHandlingEvents = false;

            for (var ii = 0; ii < eventHandleThreads.Length; ii++)
            {
                eventHandleThreads[ii].Join();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            for (var ii = 0; ii < eventHandleThreads.Length; ii++)
            {
                eventHandleThreads[ii].Start();
            }
        }

        /// <summary>
        /// Processes events on the event queue.
        /// </summary>
        private void ProcessEventQueue()
        {
            while (isHandlingEvents)
            {
                WaitCallback qEvent;
                if (eventQueue.Pop(100, out qEvent))
                {
                    try
                    {
                        qEvent.Invoke(null);
                    }
                    catch( Exception e )
                    {
                        Console.Error.WriteLine("QueueExecutor: Event threw exception '{0}'", e.GetType());
                        Console.Error.WriteLine("QueueExecutor: Error message: {0}", e.Message);
                        Console.Error.WriteLine(e.StackTrace);
                    }
                }
            }
        }
    }
}
