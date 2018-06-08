
using System;

namespace com.espertech.esper.compat.threading
{
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	internal class CommonWriteLock : ILockable
	{
	    private readonly int _lockTimeout;
        private readonly IReaderWriterLockCommon _lockObj;

        public IDisposable Acquire()
        {
            _lockObj.AcquireWriterLock(_lockTimeout);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockObj.AcquireWriterLock(msec);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
        }

	    public IDisposable Acquire(bool releaseLock, long? msec = null)
	    {
            _lockObj.AcquireWriterLock(msec ?? _lockTimeout);
            if (releaseLock)
                return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
            return new VoidDisposable();
	    }

	    public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseWriterLock();
            return new TrackedDisposable(() => _lockObj.AcquireWriterLock(_lockTimeout));
        }

        public void Release()
        {
            _lockObj.ReleaseWriterLock();
        }

        internal CommonWriteLock(IReaderWriterLockCommon lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
	
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	internal class CommonWriteLock<T> : ILockable
	{
	    private readonly int _lockTimeout;
        private readonly IReaderWriterLockCommon<T> _lockObj;
        private T _lockValue;

        public IDisposable Acquire()
        {
            _lockValue = _lockObj.AcquireWriterLock(_lockTimeout);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockValue = _lockObj.AcquireWriterLock(msec);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
        }

        public IDisposable Acquire(bool releaseLock, long? msec = null)
        {
            _lockValue = _lockObj.AcquireWriterLock(msec ?? _lockTimeout);
            if (releaseLock)
                return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
            return new VoidDisposable();
        }

	    public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseWriterLock(_lockValue);
            _lockValue = default(T);
            return new TrackedDisposable(() => _lockValue = _lockObj.AcquireWriterLock(_lockTimeout));
        }

        public void Release()
        {
            _lockObj.ReleaseWriterLock(_lockValue);
            _lockValue = default(T);
        }

        internal CommonWriteLock(IReaderWriterLockCommon<T> lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
}
