///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmark.Server
{
    public class MultiQueueExecutor : Executor
    {
        private readonly Thread[] eventHandleThreads;
        private bool isHandlingEvents;
        private Queue<WaitCallback> eventQueue;
        private Object eventQueueLock;
        private readonly int maxQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueExecutor"/> class.
        /// </summary>
        /// <param name="numThreads">The num threads.</param>
        /// <param name="maxQueueSize">Size of the max queue.</param>
        public MultiQueueExecutor(int numThreads, int? maxQueueSize)
        {
            maxQueue = maxQueueSize ?? 128*1024;
            eventQueueLock = new object();
            eventQueue = new Queue<WaitCallback>();

            isHandlingEvents = true;
            eventHandleThreads = new Thread[numThreads];
            for (int ii = 0; ii < eventHandleThreads.Length; ii++)
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
            get
            {
                lock(eventQueueLock) {
                    return eventQueue.Count;
                }
            }
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute(WaitCallback waitCallback)
        {
            lock (eventQueueLock) {
                eventQueue.Enqueue(waitCallback);
                Monitor.Pulse(eventQueueLock);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            isHandlingEvents = false;

            for (int ii = 0; ii < eventHandleThreads.Length; ii++)
            {
                eventHandleThreads[ii].Join();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            for (int ii = 0; ii < eventHandleThreads.Length; ii++)
            {
                eventHandleThreads[ii].Start();
            }
        }

        /// <summary>
        /// Processes events on the event queue.
        /// </summary>
        private void ProcessEventQueue()
        {
            var myEventQueue = new Queue<WaitCallback>(maxQueue);

            while (isHandlingEvents)
            {
                lock( eventQueueLock ) {
                    while (eventQueue.Count == 0)
                        Monitor.Wait(eventQueueLock);

                    var tempQueue = eventQueue;
                    eventQueue = myEventQueue;
                    myEventQueue = tempQueue;
                    //myEventQueue = Interlocked.Exchange(ref eventQueue, myEventQueue);
                }

                foreach( WaitCallback qEvent in myEventQueue )
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

                myEventQueue.Clear();
            }
        }
    }
}
