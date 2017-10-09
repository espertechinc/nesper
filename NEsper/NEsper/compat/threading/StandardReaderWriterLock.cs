///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class StandardReaderWriterLock 
        : IReaderWriterLock
        , IReaderWriterLockCommon
    {
        private readonly ReaderWriterLock _rwLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardReaderWriterLock"/> class.
        /// </summary>
        public StandardReaderWriterLock()
        {
            _rwLock = new ReaderWriterLock();
            ReadLock = new CommonReadLock(this);
            WriteLock = new CommonWriteLock(this);
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
            return ReadLock.Acquire();
        }

        public IDisposable AcquireWriteLock()
        {
            return WriteLock.Acquire();
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="StandardReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif

        public bool IsWriterLockHeld
        {
            get { return _rwLock.IsWriterLockHeld; }
        }

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireReaderLock(long timeout)
        {
            try
            {
                _rwLock.AcquireReaderLock((int) timeout);
            }
            catch(ApplicationException)
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
                _rwLock.AcquireWriterLock((int) timeout);
            }
            catch(ApplicationException)
            {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            _rwLock.ReleaseReaderLock();
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
            _rwLock.ReleaseWriterLock();
        }
    }
}
