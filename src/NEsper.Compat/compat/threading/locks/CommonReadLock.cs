
using System;

namespace com.espertech.esper.compat.threading.locks
{
	/// <summary>
	/// Description of CommonReadLock.
	/// </summary>
	public sealed class CommonReadLock
        : ILockable
	{
	    private readonly int _lockTimeout;
        private readonly IReaderWriterLockCommon _lockObj;

        public IDisposable Acquire()
        {
            _lockObj.AcquireReaderLock(_lockTimeout);
            return new TrackedDisposable(_lockObj.ReleaseReaderLock);
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockObj.AcquireReaderLock(msec);
            return new TrackedDisposable(_lockObj.ReleaseReaderLock);
        }

        public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseReaderLock();
            return new TrackedDisposable(() => _lockObj.AcquireReaderLock(_lockTimeout));
        }

	    public void Release()
	    {
	        _lockObj.ReleaseReaderLock();
	    }

	    public CommonReadLock(IReaderWriterLockCommon lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
	
	/// <summary>
	/// Description of CommonReadLock.
	/// </summary>
    internal sealed class CommonReadLock<T> : ILockable
	{
	    private readonly int _lockTimeout;
	    private readonly IReaderWriterLockCommon<T> _lockObj;
        private T _lockValue;

        public IDisposable Acquire()
        {
            _lockValue = _lockObj.AcquireReaderLock(_lockTimeout);
            return new TrackedDisposable(() => _lockObj.ReleaseReaderLock(_lockValue));
        }

        public IDisposable Acquire(long msec)
        {
            _lockValue = _lockObj.AcquireReaderLock(msec);
            return new TrackedDisposable(() => _lockObj.ReleaseReaderLock(_lockValue));
        }

	    public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseReaderLock(_lockValue);
            _lockValue = default(T);
            return new TrackedDisposable(() => _lockValue = _lockObj.AcquireReaderLock(_lockTimeout));
        }

        public void Release()
        {
            _lockObj.ReleaseReaderLock(_lockValue);
            _lockValue = default(T);
        }

        public CommonReadLock(IReaderWriterLockCommon<T> lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
}
