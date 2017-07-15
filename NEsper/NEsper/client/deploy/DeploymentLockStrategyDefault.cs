///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Obtains the write lock of the engine-wide event processing read-write lock by simply blocking until the lock was obtained.
    /// </summary>
    public class DeploymentLockStrategyDefault : DeploymentLockStrategy
    {
        public static readonly DeploymentLockStrategyDefault INSTANCE = new DeploymentLockStrategyDefault();
    
        private DeploymentLockStrategyDefault() {
        }
    
        public void Acquire(IReaderWriterLock engineWideLock)
        {
            engineWideLock.WriteLock.Acquire();
        }

        public void Release(IReaderWriterLock engineWideLock)
        {
            engineWideLock.WriteLock.Release();
        }
    }
} // end of namespace
