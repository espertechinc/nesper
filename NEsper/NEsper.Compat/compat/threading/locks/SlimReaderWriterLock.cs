///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
        private readonly Guid _id;
        private readonly int _lockTimeout;

#if MONO
        public const string ExceptionText = "ReaderWriterLockSlim is not supported on this platform";
#else
        private readonly ReaderWriterLockSlim _rwLock;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimReaderWriterLock"/> class.
        /// </summary>
        public SlimReaderWriterLock(int lockTimeout)
        {
            _id = Guid.NewGuid();
            _lockTimeout = lockTimeout;
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ReadLock = new CommonReadLock(this, _lockTimeout);
            WriteLock = new CommonWriteLock(this, _lockTimeout);

            _rDisposable = new TrackedDisposable(ReleaseReaderLock);
            _wDisposable = new TrackedDisposable(ReleaseWriterLock);
#endif
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

        private readonly IDisposable _rDisposable;
        private readonly IDisposable _wDisposable;

        public IDisposable AcquireReadLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireReadLock:IN:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
            if (_rwLock.TryEnterReadLock(_lockTimeout)) {
#if DIAGNOSTICS
                Console.WriteLine("{0}:AcquireReadLock:OUT:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
                return _rDisposable;
            }

#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireReadLock:ERR:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
        }

        public IDisposable AcquireWriteLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriteLock:IN:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
            if (_rwLock.TryEnterWriteLock(_lockTimeout)) {
#if DIAGNOSTICS
                Console.WriteLine("{0}:AcquireWriteLock:OUT:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
                return _wDisposable;
            }

#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriteLock:ERR:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, _lockTimeout);
#endif
            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriteLock:IN:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, lockWaitDuration);
#endif
            if (_rwLock.TryEnterWriteLock(lockWaitDuration)) {
#if DIAGNOSTICS
                Console.WriteLine("{0}:AcquireWriteLock:OUT:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, lockWaitDuration);
#endif
                return _wDisposable;
            }

#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriteLock:ERR:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, lockWaitDuration);
#endif
            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
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
        public bool IsWriterLockHeld
        {
            get { return _rwLock.IsWriteLockHeld; }
        }

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
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            
#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireReaderLock:IN:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
            if (_rwLock.TryEnterReadLock((int) timeout)) {
#if DIAGNOSTICS
                Console.WriteLine("{0}:AcquireReaderLock:OUT:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
                return;
            }

#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireReaderLock:ERR:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(long timeout)
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriterLock:IN:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
            if (_rwLock.TryEnterWriteLock((int) timeout)) {
#if DIAGNOSTICS
                Console.WriteLine("{0}:AcquireWriterLock:OUT:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
                return;
            }

#if DIAGNOSTICS
            Console.WriteLine("{0}:AcquireWriterLock:ERR:{1}: {2}", Thread.CurrentThread.ManagedThreadId, _id, timeout);
#endif
            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:ReleaseReaderLock:IN:{1}", Thread.CurrentThread.ManagedThreadId, _id);
#endif
            _rwLock.ExitReadLock();
#if DIAGNOSTICS
            Console.WriteLine("{0}:ReleaseReaderLock:OUT:{1}", Thread.CurrentThread.ManagedThreadId, _id);
#endif
#endif
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if DIAGNOSTICS
            Console.WriteLine("{0}:ReleaseWriterLock:IN:{1}", Thread.CurrentThread.ManagedThreadId, _id);
#endif
            _rwLock.ExitWriteLock();
#if DIAGNOSTICS
            Console.WriteLine("{0}:ReleaseWriterLock:OUT:{1}", Thread.CurrentThread.ManagedThreadId, _id);
#endif
#endif
        }
    }
}
