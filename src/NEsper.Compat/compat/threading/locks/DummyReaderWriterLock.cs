///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    ///     Uses a standard lock to model a reader-writer ... not for general use
    /// </summary>
    public class DummyReaderWriterLock
        : IReaderWriterLock
    {
        private static readonly VoidDisposable Disposable = new VoidDisposable();

        /// <summary>
        ///     Constructs a new instance of a DummyReaderWriterLock
        /// </summary>
        public DummyReaderWriterLock()
        {
            ReadLock = WriteLock = new MonitorSlimLock(60000);
        }

        /// <summary>
        ///     Gets the read-side lockable
        /// </summary>
        public ILockable ReadLock { get; }

        /// <summary>
        ///     Gets the write-side lockable
        /// </summary>
        public ILockable WriteLock { get; }

        public IDisposable AcquireReadLock()
        {
            return Disposable;
        }

        public IDisposable AcquireWriteLock()
        {
            return Disposable;
        }

        public IDisposable AcquireWriteLock(TimeSpan lockWaitDuration)
        {
            return Disposable;
        }

        /// <summary>
        /// Releases the write lock, canceling the lock semantics managed by any current holder.
        /// </summary>
        public void ReleaseWriteLock()
        {
        }

        /// <summary>
        ///     Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        ///     The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld => false;

#if DEBUG
        public bool Trace { get; set; }
#endif
    }
}