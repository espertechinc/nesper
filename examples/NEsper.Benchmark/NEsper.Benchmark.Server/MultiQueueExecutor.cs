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

namespace NEsper.Benchmark.Server
{
    public class MultiQueueExecutor : Executor
    {
        private readonly Thread[] _eventHandleThreads;
        private bool _isHandlingEvents;
        private Queue<WaitCallback> _eventQueue;
        private object _eventQueueLock;
        private readonly int _maxQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueExecutor"/> class.
        /// </summary>
        /// <param name="numThreads">The num threads.</param>
        /// <param name="maxQueueSize">Size of the max queue.</param>
        public MultiQueueExecutor(int numThreads, int? maxQueueSize)
        {
            _maxQueue = maxQueueSize ?? 128*1024;
            _eventQueueLock = new object();
            _eventQueue = new Queue<WaitCallback>();

            _isHandlingEvents = true;
            _eventHandleThreads = new Thread[numThreads];
            for (var ii = 0; ii < _eventHandleThreads.Length; ii++)
            {
                _eventHandleThreads[ii] = new Thread(ProcessEventQueue);
                _eventHandleThreads[ii].Name = "EsperEventHandler-" + ii;
                _eventHandleThreads[ii].IsBackground = true;
                _eventHandleThreads[ii].Start();
            }
        }

        /// <summary>
        /// Gets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get { return _eventHandleThreads.Length; }
        }

        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>The queue depth.</value>
        public int QueueDepth
        {
            get
            {
                lock(_eventQueueLock) {
                    return _eventQueue.Count;
                }
            }
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute(WaitCallback waitCallback)
        {
            lock (_eventQueueLock) {
                _eventQueue.Enqueue(waitCallback);
                Monitor.Pulse(_eventQueueLock);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _isHandlingEvents = false;

            for (var ii = 0; ii < _eventHandleThreads.Length; ii++)
            {
                _eventHandleThreads[ii].Join();
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            for (var ii = 0; ii < _eventHandleThreads.Length; ii++)
            {
                _eventHandleThreads[ii].Start();
            }
        }

        /// <summary>
        /// Processes events on the event queue.
        /// </summary>
        private void ProcessEventQueue()
        {
            var myEventQueue = new Queue<WaitCallback>(_maxQueue);

            while (_isHandlingEvents)
            {
                lock( _eventQueueLock ) {
                    while (_eventQueue.Count == 0)
                        Monitor.Wait(_eventQueueLock);

                    var tempQueue = _eventQueue;
                    _eventQueue = myEventQueue;
                    myEventQueue = tempQueue;
                    //myEventQueue = Interlocked.Exchange(ref eventQueue, myEventQueue);
                }

                foreach( var qEvent in myEventQueue )
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
