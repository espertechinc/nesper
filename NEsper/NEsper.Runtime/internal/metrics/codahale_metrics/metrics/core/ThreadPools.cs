///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A manager class for a set of named thread pools.
    /// </summary>
    internal class ThreadPools
    {
        private readonly IDictionary<string, IScheduledExecutorService> threadPools =
            new ConcurrentDictionary<string, IScheduledExecutorService>();

        /// <summary>
        /// Creates a new scheduled thread pool of a given size with the given name, or returns an
        /// existing thread pool if one was already created with the same name.
        /// </summary>
        /// <param name="poolSize">the number of threads to create</param>
        /// <param name="name">the name of the pool</param>
        /// <returns>a new <seealso cref="IScheduledExecutorService" /></returns>
        public IScheduledExecutorService NewScheduledThreadPool(int poolSize, string name)
        {
            var existing = threadPools.Get(name);
            if (IsValidExecutor(existing))
            {
                return existing;
            }
            else
            {
                // We lock here because executors are expensive to create. So
                // instead of just doing the usual putIfAbsent dance, we lock the
                // damn thing, check to see if anyone else put a thread pool in
                // there while we weren't watching.
                lock (this)
                {
                    var lastChance = threadPools.Get(name);
                    if (IsValidExecutor(lastChance))
                    {
                        return lastChance;
                    }
                    else
                    {
                        var service = Executors.DefaultScheduledExecutorService();
                        threadPools.Put(name, service);
                        return service;
                    }
                }
            }
        }

        private static bool IsValidExecutor(IExecutorService executor)
        {
            return executor != null && !executor.IsShutdown && !executor.IsTerminated;
        }

        /// <summary>
        /// Shuts down all thread pools created by this class in an orderly fashion.
        /// </summary>
        internal void Shutdown()
        {
            lock (this)
            {
                foreach (var executor in threadPools.Values)
                {
                    executor.Shutdown();
                }
                threadPools.Clear();
            }
        }
    }
} // end of namespace