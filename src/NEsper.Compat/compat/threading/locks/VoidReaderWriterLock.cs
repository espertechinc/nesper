using System;

namespace com.espertech.esper.compat.threading.locks
{
    public class VoidReaderWriterLock : IReaderWriterLock
    {
        private static readonly VoidLock Instance = new VoidLock();
        private static readonly VoidDisposable Disposable = new VoidDisposable();

        /// <summary>
        /// Initializes a new instance of the <see cref="VoidReaderWriterLock"/> class.
        /// </summary>
        public VoidReaderWriterLock()
        {
        }

        #region IReaderWriterLock Members

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        public ILockable ReadLock => Instance;

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        public ILockable WriteLock => Instance;

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

        public void ReleaseWriteLock()
        {
        }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld => false;

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VoidReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
        #endif

        #endregion
    }
}