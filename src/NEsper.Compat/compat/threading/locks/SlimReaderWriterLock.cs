///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading.locks
{
    public sealed class SlimReaderWriterLock
        : IReaderWriterLock,
            IReaderWriterLockCommon
    {
        private readonly int _lockTimeout;
        private readonly bool _useUpgradeableLocks;

        private readonly ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimReaderWriterLock"/> class.
        /// </summary>
        public SlimReaderWriterLock(int lockTimeout, bool useUpgradeableLocks = false)
        {
            _lockTimeout = lockTimeout;
            _useUpgradeableLocks = useUpgradeableLocks;
            _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ReadLock = new CommonReadLock(this, _lockTimeout);
            WriteLock = new CommonWriteLock(this, _lockTimeout);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimReaderWriterLock"/> class.
        /// </summary>
        public SlimReaderWriterLock() : this(LockConstants.DefaultTimeout)
        {
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get ; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get;  private set; }

        public IDisposable AcquireReadLock()
        {
            if (_useUpgradeableLocks) {
                if (_rwLock.TryEnterUpgradeableReadLock(_lockTimeout)) {
                    return new TrackedDisposable(ReleaseReaderLock);
                }
            }
            else if (_rwLock.TryEnterReadLock(_lockTimeout)) {
                return new TrackedDisposable(ReleaseReaderLock);
            }

            throw new TimeoutException("ReaderWriterLock timeout expired");
        }

        public IDisposable AcquireWriteLock()
        {
            if (_rwLock.TryEnterWriteLock(_lockTimeout)) {
                return new TrackedDisposable(ReleaseWriterLock);
            }

            throw new TimeoutException("ReaderWriterLock timeout expired");
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
            if (_rwLock.TryEnterWriteLock(lockWaitDuration)) {
                return new TrackedDisposable(ReleaseWriterLock);
            }

            throw new TimeoutException("ReaderWriterLock timeout expired");
        }

        /// <summary>
        /// Releases the write lock, canceling the lock semantics managed by any current holder.
        /// </summary>
        public void ReleaseWriteLock()
        {
            ReleaseWriterLock();
        }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld => _rwLock.IsWriteLockHeld;

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SlimReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireReaderLock(long timeout)
        {
            if (_useUpgradeableLocks) {
                if (_rwLock.TryEnterUpgradeableReadLock((int)timeout)) {
                    return;
                }
            }
            else if (_rwLock.TryEnterReadLock((int)timeout)) {
                return;
            }
            throw new TimeoutException("ReaderWriterLock timeout expired");
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(long timeout)
        {
            if (_rwLock.TryEnterWriteLock((int) timeout)) {
                return;
            }
            throw new TimeoutException("ReaderWriterLock timeout expired");
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            if (_useUpgradeableLocks) {
                _rwLock.ExitUpgradeableReadLock();
            }
            else {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
            _rwLock.ExitWriteLock();
        }
    }
}
