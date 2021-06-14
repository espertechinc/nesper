///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// A Statement-lock implementation that doesn't lock.
    /// </summary>
    public class StatementAgentInstanceLockNoLockImpl : StatementAgentInstanceLock
    {
        private readonly string name;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">of lock</param>
        public StatementAgentInstanceLockNoLockImpl(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Lock write lock.
        /// </summary>
        public void AcquireWriteLock()
        {
        }

        /// <summary>
        /// Lock write lock.
        /// </summary>
        public bool AcquireWriteLock(long msecTimeout)
        {
            return true;
        }

        /// <summary>
        /// Unlock write lock.
        /// </summary>
        public void ReleaseWriteLock()
        {
        }

        /// <summary>
        /// Lock read lock.
        /// </summary>
        public void AcquireReadLock()
        {
        }

        /// <summary>
        /// Unlock read lock.
        /// </summary>
        public void ReleaseReadLock()
        {
        }

        public override string ToString()
        {
            return this.GetType().Name + " name=" + name;
        }

        public bool AddAcquiredLock(ILockable @lock)
        {
            return false;
        }
    }
} // end of namespace