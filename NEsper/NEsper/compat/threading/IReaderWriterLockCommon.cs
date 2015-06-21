namespace com.espertech.esper.compat.threading
{
	/// <summary>
	/// Simple boilerplate for common reader-writer lock implementations
	/// </summary>
	public interface IReaderWriterLockCommon
	{
		/// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void AcquireReaderLock(int timeout);

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void AcquireWriterLock(int timeout);

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        void ReleaseReaderLock();

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        void ReleaseWriterLock();
	}
	
	public interface IReaderWriterLockCommon<T>
	{
		/// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        T AcquireReaderLock(int timeout);

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        T AcquireWriterLock(int timeout);

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        void ReleaseReaderLock(T value);

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        void ReleaseWriterLock(T value);
	}
}
