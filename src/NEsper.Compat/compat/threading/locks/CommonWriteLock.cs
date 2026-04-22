
using System;

namespace com.espertech.esper.compat.threading.locks
{
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	public class CommonWriteLock : ILockable
	{
	    private readonly int _lockTimeout;
        private readonly IReaderWriterLockCommon _lockObj;

        public IDisposable Acquire()
        {
            _lockObj.AcquireWriterLock(_lockTimeout);
            return new TrackedDisposable(_lockObj.ReleaseWriterLock);
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockObj.AcquireWriterLock(msec);
            return new TrackedDisposable(_lockObj.ReleaseWriterLock);
        }

        public LockScope AcquireScope()
        {
            _lockObj.AcquireWriterLock(_lockTimeout);
            return new LockScope(this);
        }

        public LockScope AcquireScope(long msec)
        {
            _lockObj.AcquireWriterLock(msec);
            return new LockScope(this);
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

        public CommonWriteLock(IReaderWriterLockCommon lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
	
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	public class CommonWriteLock<T> : ILockable
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

        public LockScope AcquireScope()
        {
            _lockValue = _lockObj.AcquireWriterLock(_lockTimeout);
            return new LockScope(this);
        }

        public LockScope AcquireScope(long msec)
        {
            _lockValue = _lockObj.AcquireWriterLock(msec);
            return new LockScope(this);
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

        public CommonWriteLock(IReaderWriterLockCommon<T> lockObj, int lockTimeout)
        {
            _lockObj = lockObj;
            _lockTimeout = lockTimeout;
        }
	}
}
