///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading.Tasks;

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.concurrency
{
    /// <summary>
    /// Class that provides access to threadPool like services.  This class exists to
    /// provide an easier bridge between the CLR thread pool and the JVM thread pool
    /// mechanisms.
    /// </summary>
    /// 
    public class Executors
    {
        /// <summary>
        /// Creates a new thread pool and returns the executor.  Ours does
        /// nothing as we use the CLR thread pool.
        /// </summary>
        /// <returns></returns>
        public static DefaultExecutorService DefaultExecutor()
        {
            return new DefaultExecutorService(Task.Factory);
        }

        /// <summary>
        /// Creates an executor tied to an explicit pool of threads.
        /// </summary>
        /// <returns></returns>
        public static DefaultExecutorService NewMultiThreadedExecutor(int numThreads)
        {
            return new DefaultExecutorService(new MultiThreadedTaskScheduler(numThreads));
        }

        /// <summary>
        /// Creates a new thread pool and returns the executor.
        /// </summary>
        /// <returns></returns>
        public static DefaultExecutorService NewSingleThreadExecutor()
        {
            return new DefaultExecutorService(new SingleThreadedTaskScheduler());
        }
        
        /// <summary>
        /// Creates a scheduled executor service that uses the thread pool for task execution.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static IScheduledExecutorService DefaultScheduledExecutorService()
        {
            return new DefaultScheduledExecutorService(DefaultExecutor());
        }

        /// <summary>
        /// Creates a fixed thread pool executor.
        /// </summary>
        /// <param name="numThreads">The number threads.</param>
        /// <param name="threadFactory">the thread factory.</param>
        /// <returns></returns>
        public static IExecutorService NewFixedThreadPool(int numThreads, ThreadFactory threadFactory)
        {
            return new DefaultExecutorService(
                    new MultiThreadedTaskScheduler(numThreads, threadFactory));
        }

        /// <summary>
        /// Returns (or creates) a cached thread pool executor.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static IExecutorService NewCachedThreadPool()
        {
            return DefaultExecutor();
        }
    }
}