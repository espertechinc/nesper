
namespace com.espertech.esper.compat.threading
{
	/// <summary>
	/// Uses a standard lock to model a reader-writer ... not for general use
	/// </summary>
	public class DummyReaderWriterLock
		: IReaderWriterLock
	{
		/// <summary>
		/// Constructs a new instance of a DummyReaderWriterLock
		/// </summary>
		public DummyReaderWriterLock()
		{
			ReadLock = WriteLock = LockManager.CreateDefaultLock();
		}
		
		/// <summary>
        /// Gets the read-side lockable
        /// </summary>
        public ILockable ReadLock { get; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        public ILockable WriteLock { get; private set; }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld
        {
            get { return false; }
        }


#if DEBUG
        public bool Trace { get; set; }
#endif
	}
}
