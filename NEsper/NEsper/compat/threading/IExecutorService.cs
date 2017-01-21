///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading
{
    public interface IExecutorService
    {
        /// <summary>
        /// Submits the specified runnable to the thread pool.
        /// </summary>
        /// <param name="runnable">The runnable.</param>
        /// <returns></returns>
        Future<Object> Submit(Action runnable);

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        Future<T> Submit<T>(ICallable<T> callable);

        /// <summary>
        /// Submits the specified callable to the thread pool.
        /// </summary>
        /// <param name="callable">The callable.</param>
        /// <returns></returns>
        Future<T> Submit<T>(Func<T> callable);

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Awaits the termination.
        /// </summary>
        void AwaitTermination();

        /// <summary>
        /// Awaits the termination.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void AwaitTermination(TimeSpan timeout);
    }
}
