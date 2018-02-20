
using System;

namespace com.espertech.esper.compat.threading
{
	/// <summary>
	/// Description of CommonReadLock.
	/// </summary>
	internal sealed class CommonReadLock 
        : ILockable
	{
	    private readonly int _lockTimeout;
        private readonly IReaderWriterLockCommon _lockObj;
	    private readonly IDisposable _disposableObj;

        public IDisposable Acquire()
        {
            _lockObj.AcquireReaderLock(_lockTimeout);
            return _disposableObj;
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockObj.AcquireReaderLock(msec);
            return _disposableObj;
        }

        public IDisposable Acquire(bool releaseLock, long? msec = null)
        {
            _lockObj.AcquireReaderLock(msec ?? _lockTimeout);
            if (releaseLock)
                return _disposableObj;
            return new VoidDisposable();
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

	    internal CommonReadLock(IReaderWriterLockCommon lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
            _disposableObj = new TrackedDisposable(_lockObj.ReleaseReaderLock);
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

	    public IDisposable Acquire(bool releaseLock, long? msec = null)
	    {
            _lockValue = _lockObj.AcquireReaderLock(msec ?? _lockTimeout);
            if (releaseLock)
                return new TrackedDisposable(() => _lockObj.ReleaseReaderLock(_lockValue));
	        return new VoidDisposable();
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

        internal CommonReadLock(IReaderWriterLockCommon<T> lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
}
