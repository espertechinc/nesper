///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading.locks
{
    public sealed class StandardReaderWriterLock 
        : IReaderWriterLock
        , IReaderWriterLockCommon
    {
#if (DEBUG && LOCK_TRACING) 
        private readonly long _id = DebugId<StandardReaderWriterLock>.NewId();
#endif
        private readonly ReaderWriterLock _rwLock;
        private int _rLockCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardReaderWriterLock"/> class.
        /// </summary>
        public StandardReaderWriterLock(int lockTimeout)
        {
            _rLockCount = 0;
            _rwLock = new ReaderWriterLock();
            ReadLock = new CommonReadLock(this, lockTimeout);
            WriteLock = new CommonWriteLock(this, lockTimeout);
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get ; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get; }

        public IDisposable AcquireReadLock()
        {
            return ReadLock.Acquire();
        }

        public IDisposable AcquireWriteLock()
        {
            return WriteLock.Acquire();
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
            return WriteLock.Acquire((long) lockWaitDuration.TotalMilliseconds);
        }

        public void ReleaseWriteLock()
        {
            ReleaseWriterLock();
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StandardReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif

        public bool IsWriterLockHeld => _rwLock.IsWriterLockHeld;

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireReaderLock(long timeout)
        {
            try {
#if (DEBUG && LOCK_TRACING) 
                if (true) {
                    Console.WriteLine(
                        "{0}: AcquireReaderLock: {1} - Start ({2})",
                        Thread.CurrentThread.ManagedThreadId,
                        _id,
                        _rLockCount);
                }
#endif
                if (_rwLock.IsReaderLockHeld) {
                    _rLockCount++;
                } else if (_rwLock.IsWriterLockHeld) {
                    _rLockCount++;
                }
                else {
                    _rwLock.AcquireReaderLock((int)timeout);
                    _rLockCount = 1;
                }
#if (DEBUG && LOCK_TRACING) 
                if (true) {
                    Console.WriteLine(
                        "{0}: AcquireReaderLock: {1} - Acquired ({2})\n",
                        Thread.CurrentThread.ManagedThreadId,
                        _id,
                        _rLockCount);
                }
#endif
            }
            catch (ApplicationException)
            {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(long timeout)
        {
            try
            {
#if (DEBUG && LOCK_TRACING) 
                if (true) {
                    Console.WriteLine(
                        "{0}: AcquireWriterLock: {1} - Start",
                        Thread.CurrentThread.ManagedThreadId,
                        _id);
                }
#endif
                _rwLock.AcquireWriterLock((int)timeout);
#if (DEBUG && LOCK_TRACING) 
                if (true) {
                    Console.WriteLine(
                        "{0}: AcquireWriterLock: {1} - Acquired",
                        Thread.CurrentThread.ManagedThreadId,
                        _id);
                }
#endif
            }
            catch (ApplicationException)
            {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
#if (DEBUG && LOCK_TRACING) 
            if (true) {
                Console.WriteLine(
                    "{0}: ReleaseReaderLock: {1} - Start ({2})",
                    Thread.CurrentThread.ManagedThreadId,
                    _id,
                    _rLockCount);
            }
#endif
            if (_rwLock.IsWriterLockHeld) {
                // if the writer lock is held, then it means we acquired the reader lock under the
                // exclusion of the writer lock.  But if the rLockCount is zero, then it means we
                // are doing something wrong
                if (_rLockCount == 0) {
                    throw new LockException("cannot release writer lock through reader mechanism");
                }

                // Decrease the rLockCount.  There is no need to check the rLockCount because it
                // should never go below zero.  We might want to check the rLockCount on writer
                // lock release.
                _rLockCount--;
            } else if (_rwLock.IsReaderLockHeld) {
                if (--_rLockCount == 0) {
                    _rwLock.ReleaseReaderLock();
                }
            }
            else {
                throw new LockException("Attempt to release a lock that is not owned by the calling thread");
            }
#if (DEBUG && LOCK_TRACING) 
            if (true) {
                Console.WriteLine(
                    "{0}: ReleaseReaderLock: {1} - Released ({2})",
                    Thread.CurrentThread.ManagedThreadId,
                    _id,
                    _rLockCount);
            }
#endif
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
#if (DEBUG && LOCK_TRACING) 
            if (true) {
                Console.WriteLine(
                    "{0}: ReleaseWriterLock: {1} - Start",
                    Thread.CurrentThread.ManagedThreadId,
                    _id);
            }
#endif
            _rwLock.ReleaseWriterLock();
#if (DEBUG && LOCK_TRACING) 
            if (true) {
                Console.WriteLine(
                    "{0}: ReleaseWriterLock: {1} - Released",
                    Thread.CurrentThread.ManagedThreadId,
                    _id);
            }
#endif
        }
    }
}
