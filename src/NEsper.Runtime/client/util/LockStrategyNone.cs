///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    /// Obtains the write lock of the runtime-wide event processing read-write lock by simply blocking until the lock was obtained.
    /// </summary>
    public class LockStrategyNone : LockStrategy
    {
        /// <summary>
        /// Instance of a lock strategy that does not obtain a lock.
        /// </summary>
        public static readonly LockStrategyNone INSTANCE = new LockStrategyNone();

        private LockStrategyNone()
        {
        }

        public IDisposable Acquire(IReaderWriterLock runtimeWideLock)
        {
            return new VoidDisposable();
        }

#if false
        public void Release(IReaderWriterLock runtimeWideLock)
        {
        }
#endif
    }
} // end of namespace