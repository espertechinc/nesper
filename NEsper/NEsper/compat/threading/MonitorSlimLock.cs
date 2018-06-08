///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public class MonitorSlimLock : ILockable
    {
        /// <summary>
        /// Gets the number of milliseconds until the lock acquisition fails.
        /// </summary>
        /// <value>The lock timeout.</value>

        public int LockTimeout
        {
            get { return _uLockTimeout; }
        }

        /// <summary>
        /// Uniquely identifies the lock.
        /// </summary>

        private readonly Guid _uLockId;

        /// <summary>
        /// Underlying object that is locked
        /// </summary>

        private readonly SlimLock _uLockObj;

        /// <summary>
        /// Number of milliseconds until the lock acquisition fails
        /// </summary>

        private readonly int _uLockTimeout;

        /// <summary>
        /// Owner of the lock.
        /// </summary>

        private Thread _uLockOwner;

        /// <summary>
        /// Used to track recursive locks.
        /// </summary>

        private int _uLockDepth;

        /// <summary>
        /// Gets the lock depth.
        /// </summary>
        /// <value>The lock depth.</value>

        public int LockDepth
        {
            get { return _uLockDepth; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorSlimLock"/> class.
        /// </summary>
        public MonitorSlimLock(int lockTimeout)
        {
            _uLockId = Guid.NewGuid();
            _uLockObj = new SlimLock();
            _uLockDepth = 0;
            _uLockTimeout = lockTimeout;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is held by current thread.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is held by current thread; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeldByCurrentThread
        {
            get { return _uLockOwner == Thread.CurrentThread; }
        }

        /// <summary>
        /// Acquires a lock against this instance.
        /// </summary>
        public IDisposable Acquire()
        {
            InternalAcquire(_uLockTimeout);
            return new TrackedDisposable(InternalRelease);
        }

        public IDisposable Acquire(long msec)
        {
            InternalAcquire((int) msec);
            return new TrackedDisposable(InternalRelease);
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
            InternalAcquire((int)(msec ?? _uLockTimeout));
            if (releaseLock)
                return new TrackedDisposable(InternalRelease);
            return new VoidDisposable();
        }

        /// <summary>
        /// Provides a temporary release of the lock if it is acquired.  When the
        /// disposable object that is returned is disposed, the lock is re-acquired.
        /// This method is effectively the opposite of acquire.
        /// </summary>
        /// <returns></returns>
        public IDisposable ReleaseAcquire()
        {
            InternalRelease();
            return new TrackedDisposable(() => InternalAcquire(_uLockTimeout));
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        public void Release()
        {
            InternalRelease();
        }

        /// <summary>
        /// Internally acquires the lock.
        /// </summary>
        private void InternalAcquire(int lockTimeout)
        {
            if ((_uLockOwner != null) && (_uLockOwner == Thread.CurrentThread))
            {
                // This condition is only true when the lock request
                // is nested.  The first time in, _uLockOwner is 0
                // because it is not owned and forces the caller to acquire
                // the spinlock to set the value; the nested call is true,
                // but only because its within an already locked scope.
                _uLockDepth++;
            }
            else
            {
                if (_uLockObj.Enter(lockTimeout))
                {
                    _uLockOwner = Thread.CurrentThread;
                    _uLockDepth = 1;
                }
                else {
                    throw new TimeoutException("Unable to obtain lock before timeout occurred");
                }
            }
        }

        /// <summary>
        /// Internally releases the lock.
        /// </summary>
        private void InternalRelease()
        {
            if (_uLockOwner != Thread.CurrentThread) {
                return;    
            }

            // Only called when you hold the lock
            --_uLockDepth;

            if (_uLockDepth == 0)
            {
                _uLockOwner = null;
                _uLockObj.Release();
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return "MonitorLock{" +
                   "_uLockId=" + _uLockId +
                   ";_uLockOwner=" + _uLockOwner +
                   "}";
        }
    }
}
