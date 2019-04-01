///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Obtains the write lock of the engine-wide event processing read-write lock by simply blocking until the lock was obtained.
    /// </summary>
    public class DeploymentLockStrategyNone : DeploymentLockStrategy
    {
        public static readonly DeploymentLockStrategyNone INSTANCE = new DeploymentLockStrategyNone();

        private DeploymentLockStrategyNone()
        {
        }

        public void Acquire(IReaderWriterLock engineWideLock)
        {
        }

        public void Release(IReaderWriterLock engineWideLock)
        {
        }
    }
} // end of namespace
