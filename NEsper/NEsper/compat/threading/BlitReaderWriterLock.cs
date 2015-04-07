///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class BlitReaderWriterLock : IReaderWriterLock
    {
        private long _bitIndicator;

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        public ILockable ReadLock { get; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        public ILockable WriteLock { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlitReaderWriterLock"/> class.
        /// </summary>
        public BlitReaderWriterLock()
        {
            _bitIndicator = 0L;
            ReadLock = new ReaderLock(this);
            WriteLock = new WriterLock(this);
        }

        private const int MaxIterations = 20;

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        public void AcquireReaderLock(int millisecondsTimeout)
        {
            var timeA = Environment.TickCount;
            var timeB = timeA + millisecondsTimeout;

            int ii = 0;
            for (; Environment.TickCount < timeB; ii++)
            {
                long pBitField = _bitIndicator; // predictive read doesn't require interlocked
                long lBitField = pBitField >> 60; // upper 4-bits
                switch (lBitField)
                {
                    case 0L: // unlocked
                        {
                            long nBitField =
                                (1L << 60) | // reader-lock
                                (pBitField & 0x7fffffffffff0000) |
                                (1L);

                            long rBitField = Interlocked.CompareExchange(
                                    ref _bitIndicator,
                                    nBitField,
                                    pBitField);
                            if (rBitField == pBitField)
                            {
                                return;
                            }
                        }

                        if (ii < 5)
                        {
                            Thread.SpinWait(20);
                        }
                        else if (ii < 10)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 1L: // reader-lock
                        {
                            long nReaders =
                                (pBitField & 0xffff) + 1;
                            long nBitField =
                                (pBitField & 0x7fffffffffff0000) |
                                (nReaders);

                            long rBitField = Interlocked.CompareExchange(
                                    ref _bitIndicator,
                                    nBitField,
                                    pBitField);
                            if (rBitField == pBitField)
                            {
                                return;
                            }
                        }

                        if (ii < 5)
                        {
                            Thread.SpinWait(20);
                        }
                        else if (ii < 10)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                        break;

                    case 2L: // writer-lock
                        Thread.Sleep(0);
                        break;
                }
            }

            throw new TimeoutException("unable to acquire reader lock");
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            for (int ii = MaxIterations; ii > 0; ii--) {
                long pBitField = _bitIndicator;
                long nBitField = (pBitField & 0xffff);

                nBitField--;
                if (nBitField > 0) {
                    nBitField |= (pBitField & 0x7fffffffffff0000);
                }
                else {
                    nBitField |= (pBitField & 0x0fffffffffff0000);
                }

                long rBitField = Interlocked.CompareExchange(
                    ref _bitIndicator,
                    nBitField,
                    pBitField);
                if (rBitField == pBitField) {
                    return;
                }

                Thread.SpinWait(20);
            }

            throw new TimeoutException("unable to release reader lock");
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        public void AcquireWriterLock(int millisecondsTimeout)
        {
            var timeA = Environment.TickCount;
            var timeB = timeA + millisecondsTimeout;

            int ii = 0;
            for (; Environment.TickCount < timeB; ii++)
            {
                long pBitField = _bitIndicator;
                long lBitField = pBitField >> 60; // upper 4-bits
                switch (lBitField)
                {
                    case 0L: // unlocked
                        {
                            long nBitField =
                                (2L << 60) | // writer-lock
                                (pBitField & 0x7fffffffffff0000) |
                                (1L);

                            long rBitField = Interlocked.CompareExchange(
                                    ref _bitIndicator,
                                    nBitField,
                                    pBitField);
                            if (rBitField == pBitField)
                            {
                                return;
                            }
                        }

                        Thread.SpinWait(20);
                        break;

                    case 1L: // reader-lock
                        Thread.Sleep(0);
                        break;

                    case 2L: // writer-lock
                        Thread.Sleep(0);
                        break;
                }
            }

            throw new TimeoutException("unable to acquire writer lock");
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
            for (int ii = MaxIterations; ii > 0; ii--)
            {
                long pBitField = _bitIndicator;
                long nBitField = (pBitField & 0x0fffffffffff0000);

                long rBitField = Interlocked.CompareExchange(
                        ref _bitIndicator,
                        nBitField,
                        pBitField);

                if (rBitField == pBitField) {
                    return;
                }

                Thread.SpinWait(20);
            }

            throw new TimeoutException("unable to release writer lock");
        }

        /// <summary>
        /// Internal reader lock.
        /// </summary>
        internal class ReaderLock : ILockable
        {
            internal readonly BlitReaderWriterLock LockObj;
            internal readonly TrackedDisposable Disposable;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReaderLock"/> class.
            /// </summary>
            /// <param name="lockObj">The lock obj.</param>
            internal ReaderLock(BlitReaderWriterLock lockObj)
            {
                LockObj = lockObj;
                Disposable = new TrackedDisposable(LockObj.ReleaseReaderLock);
            }

            /// <summary>
            /// Acquires the lock; the lock is released when the disposable
            /// object that was returned is disposed.
            /// </summary>
            /// <returns></returns>
            public IDisposable Acquire()
            {
                LockObj.AcquireReaderLock(BaseLock.RLockTimeout);
                return Disposable;
            }

            /// <summary>
            /// Acquires the lock; the lock is released when the disposable
            /// object that was returned is disposed.
            /// </summary>
            /// <param name="msec">The msec.</param>
            /// <returns></returns>
            public IDisposable Acquire(int msec)
            {
                LockObj.AcquireReaderLock(msec);
                return Disposable;
            }

            public IDisposable ReleaseAcquire()
            {
                LockObj.ReleaseReaderLock();
                return new TrackedDisposable(() => LockObj.AcquireReaderLock(BaseLock.RLockTimeout));
            }
        }

        /// <summary>
        /// Internal writer lock.
        /// </summary>
        internal class WriterLock : ILockable
        {
            internal readonly BlitReaderWriterLock LockObj;
            internal readonly TrackedDisposable Disposable;

            /// <summary>
            /// Initializes a new instance of the <see cref="WriterLock"/> class.
            /// </summary>
            /// <param name="lockObj">The lock obj.</param>
            internal WriterLock(BlitReaderWriterLock lockObj)
            {
                LockObj = lockObj;
                Disposable = new TrackedDisposable(LockObj.ReleaseWriterLock);
            }

            /// <summary>
            /// Acquires the lock; the lock is released when the disposable
            /// object that was returned is disposed.
            /// </summary>
            /// <returns></returns>
            public IDisposable Acquire()
            {
                LockObj.AcquireWriterLock(BaseLock.WLockTimeout);
                return Disposable;
            }

            public IDisposable Acquire(int msec)
            {
                LockObj.AcquireWriterLock(msec);
                return Disposable;
            }

            public IDisposable ReleaseAcquire()
            {
                LockObj.ReleaseWriterLock();
                return new TrackedDisposable(() => LockObj.AcquireWriterLock(BaseLock.WLockTimeout));
            }
        }
    }
}
