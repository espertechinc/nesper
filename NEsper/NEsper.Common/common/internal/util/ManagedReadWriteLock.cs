///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Simple read-write lock based on <seealso cref="java.util.concurrent.locks.ReentrantReadWriteLock" /> that associates a
    /// name with the lock and traces read/write locking and unlocking.
    /// </summary>
    public class ManagedReadWriteLock
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ManagedReadWriteLock));

        /// <summary>
        /// Acquire text.
        /// </summary>
        public const string ACQUIRE_TEXT = "Acquire ";

        /// <summary>
        /// Acquired text.
        /// </summary>
        public const string ACQUIRED_TEXT = "Got     ";

        /// <summary>
        /// Acquired text.
        /// </summary>
        public const string TRY_TEXT = "Trying  ";

        /// <summary>
        /// Release text.
        /// </summary>
        public const string RELEASE_TEXT = "Release ";

        /// <summary>
        /// Released text.
        /// </summary>
        public const string RELEASED_TEXT = "Freed   ";

        private readonly IReaderWriterLock _lock;
        private readonly string _name;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">of lock</param>
        /// <param name="isFair">true if a fair lock, false if not</param>
        public ManagedReadWriteLock(
            string name,
            bool isFair)
        {
            this._name = name;
            this._lock = isFair
                ? (IReaderWriterLock) new FairReaderWriterLock(10000)
                : (IReaderWriterLock) new StandardReaderWriterLock(10000);
        }

        /// <summary>
        /// Lock write lock.
        /// </summary>
        public void AcquireWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write " + _name, _lock);
            }

            _lock.WriteLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write " + _name, _lock);
            }
        }

        /// <summary>
        /// Try write lock with timeout, returning an indicator whether the lock was acquired or not.
        /// </summary>
        /// <param name="msec">number of milliseconds to wait for lock</param>
        /// <returns>indicator whether the lock could be acquired or not</returns>
        public bool TryWriteLock(long msec)
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + _name, _lock);
            }

            bool result = false;
            try {
                result = _lock.WriteLock.Acquire(msec);
            }
            catch (ThreadInterruptedException ex) {
                Log.Warn("Lock wait interupted");
            }

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + _name + " : " + result, _lock);
            }

            return result;
        }

        /// <summary>
        /// Unlock write lock.
        /// </summary>
        public void ReleaseWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " write " + _name, _lock);
            }

            _lock.WriteLock.Release();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " write " + _name, _lock);
            }
        }

        /// <summary>
        /// Lock read lock.
        /// </summary>
        public void AcquireReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " read " + _name, _lock);
            }

            _lock.ReadLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " read " + _name, _lock);
            }
        }

        /// <summary>
        /// Unlock read lock.
        /// </summary>
        public void ReleaseReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " read " + _name, _lock);
            }

            _lock.ReadLock.Release();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " read " + _name, _lock);
            }
        }

        public IReaderWriterLock Lock {
            get => _lock;
        }
    }
} // end of namespace