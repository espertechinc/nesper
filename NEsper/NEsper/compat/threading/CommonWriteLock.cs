
using System;

namespace com.espertech.esper.compat.threading
{
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	internal class CommonWriteLock : ILockable
	{
        private readonly IReaderWriterLockCommon _lockObj;

        public IDisposable Acquire()
        {
            _lockObj.AcquireWriterLock(BaseLock.WLockTimeout);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockObj.AcquireWriterLock(msec);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
        }

	    public IDisposable Acquire(bool releaseLock, long? msec = null)
	    {
            _lockObj.AcquireWriterLock(msec ?? BaseLock.WLockTimeout);
            if (releaseLock)
                return new TrackedDisposable(() => _lockObj.ReleaseWriterLock());
            return new VoidDisposable();
	    }

	    public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseWriterLock();
            return new TrackedDisposable(() => _lockObj.AcquireWriterLock(BaseLock.RLockTimeout));
        }

        public void Release()
        {
            _lockObj.ReleaseWriterLock();
        }

        internal CommonWriteLock(IReaderWriterLockCommon lockObj)
        {
            _lockObj = lockObj;
        }
	}
	
	/// <summary>
	/// Description of CommonWriteLock.
	/// </summary>
	internal class CommonWriteLock<T> : ILockable
	{
        private readonly IReaderWriterLockCommon<T> _lockObj;
        private T _lockValue;

        public IDisposable Acquire()
        {
            _lockValue = _lockObj.AcquireWriterLock(BaseLock.WLockTimeout);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
        }

	    public IDisposable Acquire(long msec)
	    {
            _lockValue = _lockObj.AcquireWriterLock(msec);
            return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
        }

        public IDisposable Acquire(bool releaseLock, long? msec = null)
        {
            _lockValue = _lockObj.AcquireWriterLock(msec ?? BaseLock.WLockTimeout);
            if (releaseLock)
                return new TrackedDisposable(() => _lockObj.ReleaseWriterLock(_lockValue));
            return new VoidDisposable();
        }

	    public IDisposable ReleaseAcquire()
        {
            _lockObj.ReleaseWriterLock(_lockValue);
            _lockValue = default(T);
            return new TrackedDisposable(() => _lockValue = _lockObj.AcquireWriterLock(BaseLock.RLockTimeout));
        }

        public void Release()
        {
            _lockObj.ReleaseWriterLock(_lockValue);
            _lockValue = default(T);
        }

        internal CommonWriteLock(IReaderWriterLockCommon<T> lockObj)
        {
            _lockObj = lockObj;
        }
	}
}
