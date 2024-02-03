///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// Obtains the write lock of the runtime-wide event processing read-write lock by simply blocking until the lock was obtained.
    /// </summary>
    public class LockStrategyDefault : LockStrategy
    {
        /// <summary>
        /// The instance of the default lock strategy.
        /// </summary>
        public static readonly LockStrategyDefault INSTANCE = new LockStrategyDefault();

        private LockStrategyDefault()
        {
        }

        public IDisposable Acquire(IReaderWriterLock runtimeWideLock)
        {
            return runtimeWideLock.AcquireWriteLock();
        }
    }
} // end of namespace