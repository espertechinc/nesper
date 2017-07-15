using System;

namespace com.espertech.esper.compat.threading
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
        /// Acquire the lock; the lock is released when the disposable
        /// object that was returned is disposed IF the releaseLock
        /// flag is set.
        /// </summary>
        /// <param name="releaseLock"></param>
        /// <param name="msec"></param>
        /// <returns></returns>
        public IDisposable Acquire(bool releaseLock, long? msec = null)
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

        /// <summary>
        /// Releases this instance.
        /// </summary>
        public void Release()
        {
        }
    }
}
