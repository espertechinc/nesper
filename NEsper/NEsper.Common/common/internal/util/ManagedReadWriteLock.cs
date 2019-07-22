///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Simple read-write lock based on <seealso cref="IReaderWriterLock" /> that
    ///     associates a name with the lock and traces read/write locking and unlocking.
    /// </summary>
    public class ManagedReadWriteLock : IDisposable
    {
        /// <summary>
        ///     Acquire text.
        /// </summary>
        public const string ACQUIRE_TEXT = "Acquire ";

        /// <summary>
        ///     Acquired text.
        /// </summary>
        public const string ACQUIRED_TEXT = "Got     ";

        /// <summary>
        ///     Acquired text.
        /// </summary>
        public const string TRY_TEXT = "Trying  ";

        /// <summary>
        ///     Release text.
        /// </summary>
        public const string RELEASE_TEXT = "Release ";

        /// <summary>
        ///     Released text.
        /// </summary>
        public const string RELEASED_TEXT = "Freed   ";

        private static readonly ILog Log = LogManager.GetLogger(typeof(ManagedReadWriteLock));

        public IReaderWriterLock Lock { get; }

        private IDisposable _readerLock;
        private IDisposable _writerLock;

        private readonly string _name;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">of lock</param>
        /// <param name="isFair">true if a fair lock, false if not</param>
        public ManagedReadWriteLock(
            string name,
            bool isFair)
        {
            _name = name;
            Lock = isFair
                ? new FairReaderWriterLock(LockConstants.DefaultTimeout)
                : (IReaderWriterLock) new StandardReaderWriterLock(LockConstants.DefaultTimeout);
        }

        public void Dispose()
        {
            if (_writerLock != null) {
                _writerLock.Dispose();
                _writerLock = null;
            }

            if (_readerLock != null) {
                _readerLock.Dispose();
                _readerLock = null;
            }
        }

        public IDisposable AcquireDisposableWriteLock()
        {
            AcquireWriteLock();
            return new TrackedDisposable(ReleaseWriteLock);
        }

        /// <summary>
        ///     Lock write lock.
        /// </summary>
        private void AcquireWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write " + _name, Lock);
            }

            _writerLock = Lock.WriteLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write " + _name, Lock);
            }
        }

        /// <summary>
        ///     Try write lock with timeout, returning an indicator whether the lock was acquired or not.
        /// </summary>
        /// <param name="msec">number of milliseconds to wait for lock</param>
        /// <returns>indicator whether the lock could be acquired or not</returns>
        public bool TryWriteLock(long msec)
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + _name, Lock);
            }

            if (_writerLock != null) {
                throw new IllegalStateException("writer lock already acquired");
            }

            try {
                _writerLock = Lock.WriteLock.Acquire(msec);
            }
            catch (ThreadInterruptedException) {
                Log.Warn("Lock wait interrupted");
            }

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + _name, Lock);
            }

            return true;
        }

        /// <summary>
        ///     Unlock write lock.
        /// </summary>
        public void ReleaseWriteLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " write " + _name, Lock);
            }

            if (_writerLock == null) {
                throw new IllegalStateException("write lock not acquired");
            }

            _writerLock.Dispose();
            _writerLock = null;

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " write " + _name, Lock);
            }
        }

        public IDisposable AcquireDisposableReadLock()
        {
            AcquireReadLock();
            return new TrackedDisposable(ReleaseReadLock);
        }

        /// <summary>
        ///     Lock read lock.
        /// </summary>
        public void AcquireReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " read " + _name, Lock);
            }

            if (_readerLock != null) {
                throw new IllegalStateException("reader lock already acquired");
            }

            _readerLock = Lock.ReadLock.Acquire();

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " read " + _name, Lock);
            }
        }

        /// <summary>
        ///     Unlock read lock.
        /// </summary>
        public void ReleaseReadLock()
        {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " read " + _name, Lock);
            }

            if (_readerLock == null) {
                throw new IllegalStateException("reader lock not acquired");
            }

            _readerLock.Dispose();
            _readerLock = null;

            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " read " + _name, Lock);
            }
        }
    }
} // end of namespace