///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

namespace com.espertech.esper.compat.threading.locks
{
    public sealed class StandardReaderWriterLock
        : IReaderWriterLock,
            IReaderWriterLockCommon
    {
#if (DEBUG && LOCK_TRACING)
        private readonly long _id = DebugId<StandardReaderWriterLock>.NewId();
#endif
        private readonly ReaderWriterLock _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardReaderWriterLock"/> class.
        /// </summary>
        public StandardReaderWriterLock(int lockTimeout)
        {
            _rwLock = new ReaderWriterLock();
            ReadLock = new CommonReadLock(this, lockTimeout);
            WriteLock = new CommonWriteLock(this, lockTimeout);
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get; }

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
            return WriteLock.Acquire((long)lockWaitDuration.TotalMilliseconds);
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
                Debug.WriteLine(
                    "{0} : {1} : AcquireReaderLock: {2} - Start ({3}) / {4} / {5}",
                    Thread.CurrentThread.ManagedThreadId,
                    PerformanceObserver.MicroTime,
                    _id,
                    0,
                    _rwLock.IsReaderLockHeld,
                    _rwLock.IsWriterLockHeld);
#endif

                _rwLock.AcquireReaderLock((int)timeout);

#if (DEBUG && LOCK_TRACING)
                Debug.WriteLine(
                    "{0} : {1} : AcquireReaderLock: {2} - Acquired ({3}) / {4} / {5}\n",
                    Thread.CurrentThread.ManagedThreadId,
                    PerformanceObserver.MicroTime,
                    _id,
                    0,
                    _rwLock.IsReaderLockHeld,
                    _rwLock.IsWriterLockHeld);
#endif
            }
            catch (ApplicationException) {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(long timeout)
        {
            try {
#if (DEBUG && LOCK_TRACING)
                Debug.WriteLine(
                    "{0} : {1} : AcquireWriterLock: {2} - Start / {3} / {4} / {5}",
                    Thread.CurrentThread.ManagedThreadId,
                    PerformanceObserver.MicroTime,
                    _id,
                    _rwLock.IsReaderLockHeld,
                    _rwLock.IsWriterLockHeld,
                    _rwLock.WriterSeqNum);
#endif

                _rwLock.AcquireWriterLock((int)timeout);

#if (DEBUG && LOCK_TRACING)
                Debug.WriteLine(
                    "{0} : {1} : AcquireWriterLock: {2} - Acquired / {3} / {4}",
                    Thread.CurrentThread.ManagedThreadId,
                    PerformanceObserver.MicroTime,
                    _id,
                    _rwLock.IsReaderLockHeld,
                    _rwLock.IsWriterLockHeld);
#endif

            }
            catch (ApplicationException) {

#if (DEBUG && LOCK_TRACING)
                Debug.WriteLine(
                    "{0} : {1} : AcquireWriterLock: {2} - Timeout / {3} / {4}",
                    Thread.CurrentThread.ManagedThreadId,
                    PerformanceObserver.MicroTime,
                    _id,
                    _rwLock.IsReaderLockHeld,
                    _rwLock.IsWriterLockHeld);
#endif

                throw new TimeoutException("ReaderWriterLock timeout expired");
            }
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
#if (DEBUG && LOCK_TRACING)
            Debug.WriteLine(
                "{0} : {1} : ReleaseReaderLock: {2} - Start ({3}) / {4} / {5}",
                Thread.CurrentThread.ManagedThreadId,
                PerformanceObserver.MicroTime,
                _id,
                0,
                _rwLock.IsReaderLockHeld,
                _rwLock.IsWriterLockHeld);
#endif

            _rwLock.ReleaseReaderLock();

#if (DEBUG && LOCK_TRACING)
            Debug.WriteLine(
                "{0} : {1} : ReleaseReaderLock: {2} - Released ({3}) / {4} / {5}",
                Thread.CurrentThread.ManagedThreadId,
                PerformanceObserver.MicroTime,
                _id,
                0,
                _rwLock.IsReaderLockHeld,
                _rwLock.IsWriterLockHeld);
#endif
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
#if (DEBUG && LOCK_TRACING)
            Debug.WriteLine(
                "{0} : {1} : ReleaseWriterLock: {2} - Start / {3} / {4}",
                Thread.CurrentThread.ManagedThreadId,
                PerformanceObserver.MicroTime,
                _id,
                _rwLock.IsReaderLockHeld,
                _rwLock.IsWriterLockHeld);
#endif

            _rwLock.ReleaseWriterLock();

#if (DEBUG && LOCK_TRACING)
            Debug.WriteLine(
                "{0} : {1} : ReleaseWriterLock: {2} - Released / {3} / {4}",
                Thread.CurrentThread.ManagedThreadId,
                PerformanceObserver.MicroTime,
                _id,
                _rwLock.IsReaderLockHeld,
                _rwLock.IsWriterLockHeld);
#endif
        }
    }
}
