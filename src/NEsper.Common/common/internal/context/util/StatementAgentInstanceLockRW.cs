///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.threading.locks;

using static com.espertech.esper.common.@internal.context.util.StatementAgentInstanceLockConstants;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    ///     Simple read-write @lock based on <seealso cref="IReaderWriterLock" /> that associates a
    ///     name with the @lock and traces read/write locking and unlocking.
    /// </summary>
    public class StatementAgentInstanceLockRW : StatementAgentInstanceLock
    {
        private readonly IReaderWriterLock _lock;
        private IDisposable _readLock;
        private IDisposable _writeLock;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isFair">true if a fair @lock, false if not</param>
        public StatementAgentInstanceLockRW(bool isFair)
        {
            if (isFair) {
                _lock = new SlimReaderWriterLock();
            }
            else {
                _lock = new FairReaderWriterLock();
            }
        }

        /// <summary>
        ///     Lock write @lock.
        /// </summary>
        public void AcquireWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write ", _lock);
            }

            _writeLock = _lock.WriteLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write ", _lock);
            }
        }

        public bool AcquireWriteLock(long msecTimeout)
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write ", _lock);
            }

            _writeLock = _lock.WriteLock.Acquire(msecTimeout);

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write ", _lock);
            }

            return true;
        }

        /// <summary>
        ///     Unlock write @lock.
        /// </summary>
        public void ReleaseWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " write ", _lock);
            }

            if (_writeLock == null) {
                throw new EPLockException("writeLock was not acquired");
            }

            _writeLock.Dispose();
            _writeLock = null;

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " write ", _lock);
            }
        }

        /// <summary>
        ///     Lock read @lock.
        /// </summary>
        public void AcquireReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " read ", _lock);
            }

            _readLock = _lock.ReadLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " read ", _lock);
            }
        }

        /// <summary>
        ///     Unlock read @lock.
        /// </summary>
        public void ReleaseReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " read ", _lock);
            }

            if (_readLock == null)
            {
                throw new EPLockException("readLock was not acquired");
            }

            _readLock.Dispose();
            _readLock = null;

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " read ", _lock);
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
} // end of namespace