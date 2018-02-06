using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public class FairReaderWriterLock 
    	: IReaderWriterLock
    	, IReaderWriterLockCommon
    {
        /// <summary>
        /// Main lock
        /// </summary>
        private SpinLock _uMainLock;
        
        private LockFlags _uLockFlags;

        private int _uLockOwner;

        private int _uSharedCount;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FairReaderWriterLock"/> class.
        /// </summary>
        public FairReaderWriterLock(int lockTimeout)
        {
            _uMainLock = new SpinLock(true);
            _uLockFlags = LockFlags.None;
            _uSharedCount = 0;

            ReadLock = new CommonReadLock(this, lockTimeout);
            WriteLock = new CommonWriteLock(this, lockTimeout);
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get; private set; }

        public IDisposable AcquireReadLock()
        {
            return ReadLock.Acquire();
        }

        public IDisposable AcquireWriteLock()
        {
            return WriteLock.Acquire();
        }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld
        {
            get
            {
                var hasWriteLock = new bool[] { false };

                WithMainLock(
                    () =>
                    {
                        hasWriteLock[0] =
                            (_uLockOwner == System.Threading.Thread.CurrentThread.ManagedThreadId) &&
                            ((_uLockFlags & LockFlags.Exclusive) == LockFlags.Exclusive);
                    });

                return hasWriteLock[0];
            }
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FairReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif

        /// <summary>
        /// Executes the action within the mainlock.
        /// </summary>
        private void WithMainLock(Action action)
        {
            var lockTaken = false;

            try
            {
                _uMainLock.Enter(ref lockTaken);
                if (lockTaken)
                {
                    action.Invoke();
                    return;
                }

                throw new TimeoutException("unable to secure main lock");
            }
            finally
            {
                if (lockTaken)
                {
                    _uMainLock.Exit();
                }
            }
        }

        /// <summary>
        /// Executes the action within the mainlock.  An end time is provided to this
        /// call to set the last millisecond in which this lock must be obtained.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeCur">The current time.</param>
        /// <param name="timeEnd">The end time.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private T WithMainLock<T> (long timeCur, long timeEnd, Func<T> action)
		{
            var timeOut = timeEnd - timeCur;
            if (timeOut > 0)
            {
                var lockTaken = false;

                try
                {
                    _uMainLock.TryEnter((int) timeOut, ref lockTaken);
                    if (lockTaken)
                    {
                        return action.Invoke();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _uMainLock.Exit();
                    }
                }
            }

            throw new TimeoutException("unable to secure main lock");
        }

        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireReaderLock(long timeout)
        {
            var timeCur = DateTimeHelper.CurrentTimeMillis;
            var timeEnd = timeCur + timeout;
            var ii = 0;

            for (;;)
            {
                var result = WithMainLock(
                    timeCur,
                    timeEnd,
                    () =>
                    {
                        if ((_uLockFlags == LockFlags.None) ||
                            (_uLockFlags == LockFlags.Shared))
                        {
                            _uSharedCount++;
                            return true;
                        }

                        return false;
                    });
                if (result)
                {
                    return;
                }

                SlimLock.SmartWait(++ii);

                timeCur = DateTimeHelper.CurrentTimeMillis;
            }
        }

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(long timeout)
        {
            var timeCur = DateTimeHelper.CurrentTimeMillis;
            var timeEnd = timeCur + timeout;
            var ii = 0;

            var upgrade = new bool[] {false};

            try
            {
                for (;;)
                {
                    var result = WithMainLock(
                        timeCur,
                        timeEnd,
                        () =>
                        {
                            if (_uLockFlags == LockFlags.None)
                            {
                                _uLockFlags = LockFlags.Exclusive;
                                _uLockOwner = System.Threading.Thread.CurrentThread.ManagedThreadId;
                                return true;
                            }
                            else if (_uLockFlags == LockFlags.Shared)
                            {
                                _uLockFlags |= LockFlags.ExclusiveUpgrade;
                                upgrade[0] = true;
                                return false;
                            }
                            else if (_uLockFlags == LockFlags.ExclusiveUpgrade)
                            {
                                // shared flag has been cleared
                                upgrade[0] = false;
                                _uLockFlags = LockFlags.Exclusive;
                                _uLockOwner = System.Threading.Thread.CurrentThread.ManagedThreadId;
                                return true;
                            }
                            // Exclusive - wait

                            return false;
                        });
                    if (result)
                    {
                        return;
                    }

                    SlimLock.SmartWait(++ii);

                    timeCur = DateTimeHelper.CurrentTimeMillis;
                }
            }
            finally
            {
                if (upgrade[0])
                {
                    WithMainLock(() => _uLockFlags &= ~LockFlags.ExclusiveUpgrade);
                }
            }
        }

        private void ReleaseReaderInternal()
        {
            if (--_uSharedCount == 0)
            {
                _uLockFlags &= ~LockFlags.Shared;
            }
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
            WithMainLock(ReleaseReaderInternal);
        }

        private void ReleaseWriterInternal()
        {
            _uLockFlags &= ~LockFlags.Exclusive;
        }
        
        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
            WithMainLock(ReleaseWriterInternal);
        }

        [Flags]
        internal enum LockFlags
        {
        	/// <summary>
        	/// No flags are set
        	/// </summary>
			None = 0x0000,
			/// <summary>
			/// Lock is in a shared state ...
			/// </summary>
			Shared = 0x0001,
			/// <summary>
			/// Lock is in an exclusive state ...
			/// </summary>
			Exclusive = 0x0002,
			/// <summary>
			/// Exclusive upgrade pending ...
			/// </summary>
			ExclusiveUpgrade = 0x0004,
        }
    }
}
