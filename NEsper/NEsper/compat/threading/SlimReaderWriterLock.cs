///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public sealed class SlimReaderWriterLock
    	: IReaderWriterLock
    	, IReaderWriterLockCommon
    {
#if MONO
        public const string ExceptionText = "ReaderWriterLockSlim is not supported on this platform";
#else
        private readonly ReaderWriterLockSlim _rwLock;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SlimReaderWriterLock"/> class.
        /// </summary>
        public SlimReaderWriterLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            ReadLock = new CommonReadLock(this);
            WriteLock = new CommonWriteLock(this);
#endif
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get ; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get;  private set; }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld
        {
            get { return _rwLock.IsWriteLockHeld; }
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SlimReaderWriterLock"/> is trace.
        /// </summary>
        /// <value><c>true</c> if trace; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif
        
        /// <summary>
        /// Acquires the reader lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireReaderLock(int timeout)
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if PERFORMANCE_TUNING
            var counterIn = PerformanceObserverWin.GetCounter();
            var hadWriteLock = HasWriteLock;
#endif

            if (!_rwLock.TryEnterReadLock(timeout))
            {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }

#if PERFORMANCE_TUNING
            var counter = PerformanceObserverWin.GetCounter() - counterIn;
            if (counter > 5000)
            {
                Console.WriteLine("!! {0} | {1}", counter, hadWriteLock);
            }

            HasReadLock = true;

            Interlocked.Increment(ref ReadAcquireCounter);
            Interlocked.Add(ref ReadAcquireCycles, counter);
#endif
#endif
        }

#if PERFORMANCE_TUNING
        public static long ReadAcquireCounter;
        public static long ReadAcquireCycles;
        public static long WriteAcquireCounter;
        public static long WriteAcquireCycles;

        public bool HasReadLock;
        public bool HasWriteLock;

        public long TimeAcquire;
        public long TimeRelease;
#endif

        /// <summary>
        /// Acquires the writer lock.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void AcquireWriterLock(int timeout)
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
#if PERFORMANCE_TUNING
            var counterIn = PerformanceObserverWin.GetCounter();
#endif

            if (!_rwLock.TryEnterWriteLock(timeout))
            {
                throw new TimeoutException("ReaderWriterLock timeout expired");
            }

#if PERFORMANCE_TUNING
            HasWriteLock = true;

            long counter = (TimeAcquire = PerformanceObserverWin.GetCounter()) - counterIn;
            Interlocked.Increment(ref WriteAcquireCounter);
            Interlocked.Add(ref WriteAcquireCycles, counter);
#endif
#endif
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            _rwLock.ExitReadLock();
#if PERFORMANCE_TUNING
            HasReadLock = false;
#endif
#endif
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock()
        {
#if MONO
            throw new NotSupportedException(ExceptionText);
#else
            _rwLock.ExitWriteLock();

#if PERFORMANCE_TUNING
            HasWriteLock = false;
            long counter = PerformanceObserverWin.GetCounter() - TimeAcquire;
            if (counter > 10000)
            {
                Console.WriteLine("%% => {0}", counter);
                Console.WriteLine(new StackTrace());
            }
#endif
#endif
        }
    }
}
