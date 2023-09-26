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
using com.espertech.esper.compat;
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

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isFair">true if a fair @lock, false if not</param>
        public StatementAgentInstanceLockRW(bool isFair)
        {
            if (isFair) {
                _lock = new FairReaderWriterLock();
            }
            else {
                _lock = new SlimReaderWriterLock();
            }
        }

        public bool IsWriterLockHeld {
            get => _lock.IsWriterLockHeld;
        }

        public bool Trace {
            get => _lock.Trace;
            set => _lock.Trace = value;
        }

        public IDisposable AcquireReadLock()
        {
            var lockDisposable = _lock.AcquireReadLock();
            return new TrackedDisposable(
                () => {
                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASE_TEXT + " read ", _lock);
                    }

                    lockDisposable.Dispose();
                    lockDisposable = null;
                    
                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASED_TEXT + " read ", _lock);
                    }
                }
            );
        }
        
        public IDisposable AcquireWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write ", _lock);
            }

            var lockDisposable = _lock.AcquireWriteLock();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write ", _lock);
            }
            
            return new TrackedDisposable(
                () => {
                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASE_TEXT + " write ", _lock);
                    }

                    lockDisposable.Dispose();
                    lockDisposable = null;

                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASED_TEXT + " write ", _lock);
                    }
                }
            );
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write ", _lock);
            }

            var lockDisposable = _lock.AcquireWriteLock(lockWaitDuration);

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write ", _lock);
            }
            
            return new TrackedDisposable(
                () => {
                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASE_TEXT + " write ", _lock);
                    }

                    lockDisposable.Dispose();
                    lockDisposable = null;

                    if (ThreadLogUtil.ENABLED_TRACE) {
                        ThreadLogUtil.TraceLock(RELEASED_TEXT + " write ", _lock);
                    }
                }
            );
        }

        public void ReleaseWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " write ", _lock);
            }

            _lock.ReleaseWriteLock();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " write ", _lock);
            }
        }

        public ILockable ReadLock => throw new NotSupportedException();
        public ILockable WriteLock => throw new NotSupportedException();
    }
} // end of namespace