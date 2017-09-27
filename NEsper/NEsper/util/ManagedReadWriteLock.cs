///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using java.util.concurrent.locks;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Simple read-write lock based on <seealso cref="java.util.concurrent.locks.ReentrantReadWriteLock" /> that associates a
    /// name with the lock and traces read/write locking and unlocking.
    /// </summary>
    public class ManagedReadWriteLock {
        /// <summary>Acquire text.</summary>
        public static readonly string ACQUIRE_TEXT = "Acquire ";
        /// <summary>Acquired text.</summary>
        public static readonly string ACQUIRED_TEXT = "Got     ";
        /// <summary>Acquired text.</summary>
        public static readonly string TRY_TEXT = "Trying  ";
        /// <summary>Release text.</summary>
        public static readonly string RELEASE_TEXT = "Release ";
        /// <summary>Released text.</summary>
        public static readonly string RELEASED_TEXT = "Freed   ";
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ReentrantReadWriteLock lock;
        private readonly string name;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">of lock</param>
        /// <param name="isFair">true if a fair lock, false if not</param>
        public ManagedReadWriteLock(string name, bool isFair) {
            this.name = name;
            this.lock = new ReentrantReadWriteLock(isFair);
        }
    
        /// <summary>Lock write lock.</summary>
        public void AcquireWriteLock() {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " write " + name, lock);
            }
    
            lock.WriteLock().Lock();
    
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " write " + name, lock);
            }
        }
    
        /// <summary>
        /// Try write lock with timeout, returning an indicator whether the lock was acquired or not.
        /// </summary>
        /// <param name="msec">number of milliseconds to wait for lock</param>
        /// <returns>indicator whether the lock could be acquired or not</returns>
        public bool TryWriteLock(long msec) {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + name, lock);
            }
    
            bool result = false;
            try {
                result = lock.WriteLock().TryLock(msec, TimeUnit.MILLISECONDS);
            } catch (InterruptedException ex) {
                Log.Warn("Lock wait interupted");
            }
    
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(TRY_TEXT + " write " + name + " : " + result, lock);
            }
    
            return result;
        }
    
        /// <summary>Unlock write lock.</summary>
        public void ReleaseWriteLock() {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " write " + name, lock);
            }
    
            lock.WriteLock().Unlock();
    
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " write " + name, lock);
            }
        }
    
        /// <summary>Lock read lock.</summary>
        public void AcquireReadLock() {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRE_TEXT + " read " + name, lock);
            }
    
            lock.ReadLock().Lock();
    
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(ACQUIRED_TEXT + " read " + name, lock);
            }
        }
    
        /// <summary>Unlock read lock.</summary>
        public void ReleaseReadLock() {
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASE_TEXT + " read " + name, lock);
            }
    
            lock.ReadLock().Unlock();
    
            if (ThreadLogUtil.ENABLED_TRACE) {
                ThreadLogUtil.TraceLock(RELEASED_TEXT + " read " + name, lock);
            }
        }
    
        public ReentrantReadWriteLock GetLock() {
            return lock;
        }
    }
} // end of namespace
