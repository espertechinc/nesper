using System;

namespace com.espertech.esper.compat.threading.locks
{
    public class VoidLock : ILockable
    {
        private static VoidDisposable _singleton = new VoidDisposable();

        /// <summary>
        /// Acquires the lock; the lock is released when the disposable
        /// object that was returned is disposed.
        /// </summary>
        /// <returns></returns>
        public IDisposable Acquire()
        {
            return _singleton;
            //return new VoidDisposable();
        }

        /// <summary>
        /// Acquires the specified msec.
        /// </summary>
        /// <param name="msec">The msec.</param>
        /// <returns></returns>
        public IDisposable Acquire(long msec)
        {
            return _singleton;
            //return new VoidDisposable();
        }

        /// <summary>
        /// Provides a temporary release of the lock if it is acquired.  When the
        /// disposable object that is returned is disposed, the lock is re-acquired.
        /// This method is effectively the opposite of acquire.
        /// </summary>
        /// <returns></returns>
        public IDisposable ReleaseAcquire()
        {
            return _singleton;
            //return new VoidDisposable();
        }

        public LockScope AcquireScope()
        {
            return default;
        }

        public LockScope AcquireScope(long msec)
        {
            return default;
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        public void Release()
        {
        }
    }
}
