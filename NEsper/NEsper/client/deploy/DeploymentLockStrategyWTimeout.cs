///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.util;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Obtains the write lock of the engine-wide event processing read-write lock by trying the lock
    /// waiting for the timeout and throwing an exception if the lock was not taken.
    /// </summary>
    public class DeploymentLockStrategyWTimeout : DeploymentLockStrategy
    {
        private TimeSpan _timeout;
        private IDisposable _currentLock;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="timeout">timeout value in the unit given</param>
        public DeploymentLockStrategyWTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            _currentLock = null;
        }

        public void Acquire(IReaderWriterLock engineWideLock)
        {
            try
            {
                var newLock = engineWideLock.WriteLock.Acquire((long) _timeout.TotalMilliseconds);
                // only assign the new lock if the current one is obtained... are we safe to assign
                // at this point?  I believe so because we have not returned control back to the
                // calling application.  As such, there can be no chance that the lock has been
                // released by the calling thread.  If it was released, its a defect because some
                // other thread called release while we were in acquire.
                _currentLock = newLock;
            }
            catch (TimeoutException)
            {
                throw new DeploymentLockException(
                    "Failed to obtain write lock of engine-wide processing read-write lock");
            }
        }

        public void Release(IReaderWriterLock engineWideLock)
        {
            var existingLock = Interlocked.Exchange(ref _currentLock, null);
            if (existingLock != null)
            {
                existingLock.Dispose();
            }
            
            //engineWideLock.WriteLock.Release();
        }
    }
} // end of namespace
