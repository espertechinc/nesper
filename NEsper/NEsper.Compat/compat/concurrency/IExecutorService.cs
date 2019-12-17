///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.concurrency
{
    public interface IExecutorService
    {
        /// <summary>
        /// Submits the specified runnable to the thread pool.
        /// </summary>
        /// <param name="runnable">The runnable.</param>
        /// <returns></returns>
        IFuture<object> Submit(Action runnable);

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        IFuture<T> Submit<T>(ICallable<T> callable);

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        IFuture<T> Submit<T>(Func<T> callable);

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Awaits termination.
        /// </summary>
        void AwaitTermination();

        /// <summary>
        /// Awaits termination.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void AwaitTermination(TimeSpan timeout);

        /// <summary>
        /// Gets a value indicating whether this instance is shutdown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shutdown; otherwise, <c>false</c>.
        /// </value>
        bool IsShutdown { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is terminated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is terminated; otherwise, <c>false</c>.
        /// </value>
        bool IsTerminated { get; }
    }
}
