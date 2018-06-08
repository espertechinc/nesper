///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class SlimReaderWriterLock
    	: IReaderWriterLock
    	, IReaderWriterLockCommon
    {
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
            if (_rwLock.TryEnterReadLock(_lockTimeout))
                return _rDisposable;

            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
        }

        public IDisposable AcquireWriteLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            if (_rwLock.TryEnterWriteLock(_lockTimeout))
                return _wDisposable;

            throw new TimeoutException("ReaderWriterLock timeout expired");
#endif
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
            if (_rwLock.TryEnterReadLock((int) timeout))
                return;

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
            if (_rwLock.TryEnterWriteLock((int) timeout))
                return;

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
            _rwLock.ExitReadLock();
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
            _rwLock.ExitWriteLock();
#endif
        }
    }
}
