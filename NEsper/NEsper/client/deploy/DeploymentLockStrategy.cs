///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Implement this interface to provide a custom deployment lock strategy.
    /// The default lock strategy is <seealso cref="DeploymentLockStrategyDefault" />.
    /// </summary>
    public interface DeploymentLockStrategy
    {
        /// <summary>
        /// Acquire should acquire the write lock of the provided read-write lock and may retry and backoff or fail.
        /// </summary>
        /// <param name="engineWideLock">the engine-wide event processing read-write lock</param>
        /// <exception cref="DeploymentLockException">to indicate lock attempt failed</exception>
        /// <exception cref="ThreadInterruptedException">when lock-taking is interrupted</exception>
        void Acquire(IReaderWriterLock engineWideLock) ;
    
        /// <summary>
        /// Release should release the write lock of the provided read-write lock and should never fail.
        /// </summary>
        /// <param name="engineWideLock">the engine-wide event processing read-write lock</param>
        void Release(IReaderWriterLock engineWideLock);
    }
} // end of namespace
