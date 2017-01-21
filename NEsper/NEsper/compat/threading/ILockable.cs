///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// A simple locking mechanism
    /// </summary>

    public interface ILockable
    {
        /// <summary>
        /// Acquires the lock; the lock is released when the disposable
        /// object that was returned is disposed.
        /// </summary>
        /// <returns></returns>
        IDisposable Acquire();

        /// <summary>
        /// Acquire the lock; the lock is released when the disposable
        /// object that was returned is disposed IF the releaseLock
        /// flag is set.
        /// </summary>
        /// <param name="releaseLock"></param>
        /// <param name="msec"></param>
        /// <returns></returns>
        IDisposable Acquire(bool releaseLock, int? msec = null);

        /// <summary>
        /// Acquires the specified msec.
        /// </summary>
        /// <param name="msec">The msec.</param>
        /// <returns></returns>
        IDisposable Acquire(int msec);

        /// <summary>
        /// Provides a temporary release of the lock if it is acquired.  When the
        /// disposable object that is returned is disposed, the lock is re-acquired.
        /// This method is effectively the opposite of acquire.
        /// </summary>
        /// <returns></returns>
        IDisposable ReleaseAcquire();

        /// <summary>
        /// Releases this instance.
        /// </summary>
        void Release();
    }
}
