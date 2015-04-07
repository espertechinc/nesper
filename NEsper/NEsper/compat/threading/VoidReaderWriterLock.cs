namespace com.espertech.esper.compat.threading
{
    public class VoidReaderWriterLock : IReaderWriterLock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoidReaderWriterLock"/> class.
        /// </summary>
        public VoidReaderWriterLock()
        {
            ReadLock = new VoidLock();
            WriteLock = new VoidLock();
        }

        #region IReaderWriterLock Members

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        public ILockable ReadLock { get; set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        public ILockable WriteLock { get; set; }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld {
            get { return false; }
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VoidReaderWriterLock"/> is trace.
        /// </summary>
        /// <value><c>true</c> if trace; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
        #endif

        #endregion
    }
}