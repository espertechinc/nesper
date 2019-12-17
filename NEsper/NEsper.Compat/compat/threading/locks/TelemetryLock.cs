///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace com.espertech.esper.compat.threading.locks
{
    public class TelemetryLock : ILockable
    {
        /// <summary>
        /// Unique identifier for the lock.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Lock that holds the real lock implementation.
        /// </summary>
        private readonly ILockable _subLock;

        /// <summary>
        /// Occurs when the lock is released.
        /// </summary>
        public event EventHandler<TelemetryEventArgs> LockReleased;

        /// <summary>
        /// Raises the <see cref="E:LockReleased"/> event.
        /// </summary>
        /// <param name="e">The <see cref="TelemetryEventArgs"/> instance containing the event data.</param>
        protected void OnLockReleased(TelemetryEventArgs e)
        {
            if ( LockReleased != null ) {
                LockReleased(this, e);
            }
        }

        /// <summary>
        /// Finishes tracking performance of a call sequence.
        /// </summary>
        /// <param name="timeLockRequest">The time the lock was requested.</param>
        /// <param name="timeLockAcquire">The time the lock was acquired.</param>
        private void FinishTrackingPerformance(long timeLockRequest, long timeLockAcquire)
        {
            var eventArgs = new TelemetryEventArgs();
            eventArgs.Id = _id;
            eventArgs.RequestTime = timeLockRequest;
            eventArgs.AcquireTime = timeLockAcquire;
            eventArgs.ReleaseTime = PerformanceObserver.MicroTime;
            eventArgs.StackTrace = new StackTrace();

            OnLockReleased(eventArgs);
        }

        /// <summary>
        /// Acquires the lock; the lock is released when the disposable
        /// object that was returned is disposed.
        /// </summary>
        /// <returns></returns>
        public IDisposable Acquire()
        {
            var timeLockRequested = PerformanceObserver.MicroTime;
            var disposableLock = _subLock.Acquire();
            var timeLockAcquired = PerformanceObserver.MicroTime;
            var disposableTrack = new TrackedDisposable(
                delegate
                    {
                        disposableLock.Dispose();
                        disposableLock = null;
                        FinishTrackingPerformance(timeLockRequested, timeLockAcquired);
                    });

            return disposableTrack;
        }

        public IDisposable Acquire(long msec)
        {
            var timeLockRequested = PerformanceObserver.MicroTime;
            var disposableLock = _subLock.Acquire(msec);
            var timeLockAcquired = PerformanceObserver.MicroTime;
            var disposableTrack = new TrackedDisposable(
                delegate
                {
                    disposableLock.Dispose();
                    disposableLock = null;
                    FinishTrackingPerformance(timeLockRequested, timeLockAcquired);
                });

            return disposableTrack;
        }

        public IDisposable Acquire(bool releaseLock, long? msec = null)
        {
            var timeLockRequested = PerformanceObserver.MicroTime;
            var disposableLock = _subLock.Acquire(releaseLock, msec: msec);
            var timeLockAcquired = PerformanceObserver.MicroTime;
            var disposableTrack = new TrackedDisposable(
                delegate
                {
                    disposableLock.Dispose();
                    disposableLock = null;
                    FinishTrackingPerformance(timeLockRequested, timeLockAcquired);
                });

            return disposableTrack;
        }

        /// <summary>
        /// Provides a temporary release of the lock if it is acquired.  When the
        /// disposable object that is returned is disposed, the lock is re-acquired.
        /// This method is effectively the opposite of acquire.
        /// </summary>
        /// <returns></returns>
        public IDisposable ReleaseAcquire()
        {
            var timeLockRequested = PerformanceObserver.MicroTime;
            var disposableLock = _subLock.ReleaseAcquire();
            var timeLockAcquired = PerformanceObserver.MicroTime;
            var disposableTrack = new TrackedDisposable(
                delegate
                {
                    disposableLock.Dispose();
                    disposableLock = null;
                    FinishTrackingPerformance(timeLockRequested, timeLockAcquired);
                });

            return disposableTrack;
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        public void Release()
        {
            _subLock.Release();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryLock"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="subLock">The sub lock.</param>
        public TelemetryLock(string id, ILockable subLock)
        {
            _id = id;
            _subLock = subLock;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryLock"/> class.
        /// </summary>
        /// <param name="subLock">The sub lock.</param>
        public TelemetryLock(ILockable subLock)
        {
            _id = Guid.NewGuid().ToString();
            _subLock = subLock;
        }
    }
}
