///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

namespace NEsper.Benchmark.Server
{
    public class InlineExecutor : Executor
    {
        /// <summary>
        /// Gets the thread count.
        /// </summary>
        /// <value>The thread count.</value>
        public int ThreadCount
        {
            get { return 0; }
        }
        
        /// <summary>
        /// Gets the queue depth.
        /// </summary>
        /// <value>The queue depth.</value>
        public int QueueDepth
        {
            get { return 0; }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Gets the executor.
        /// </summary>
        /// <value>The executor.</value>
        public void Execute( WaitCallback waitCallback )
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
        }
    }
}
