///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    /// Implement this interface to provide a custom deployment lock strategy.
    /// The default lock strategy is <seealso cref="LockStrategyDefault" />.
    /// </summary>
    public interface LockStrategy
    {
        /// <summary>
        /// Acquire should acquire the write lock of the provided read-write lock and may retry and backoff or fail.
        /// </summary>
        /// <param name="runtimeWideLock">the runtime-wide event processing read-write lock</param>
        /// <throws>LockStrategyException to indicate lock attempt failed</throws>
        /// <throws>InterruptedException  when lock-taking is interrupted</throws>
        IDisposable Acquire(IReaderWriterLock runtimeWideLock);
    }
} // end of namespace