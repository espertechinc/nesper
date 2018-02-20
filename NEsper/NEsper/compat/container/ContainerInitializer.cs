///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace com.espertech.esper.compat.container
{
    /// <summary>
    /// Initializes a service container with a "default" set of services.
    /// </summary>
    public class ContainerInitializer
    {
        /// <summary>
        /// Initializes the default services.
        /// </summary>
        /// <param name="container">The service container.</param>
        public static void InitializeDefaultServices(IContainer container)
        {
            container.RegisterSingleton<ILockManager>(
                ic => DefaultLockManager());
            container.RegisterSingleton<IReaderWriterLockManager>(
                ic => DefaultRWLockManager());
            container.RegisterSingleton<IThreadLocalManager>(
                ic => DefaultThreadLocalManager());
            container.RegisterSingleton<IResourceManager>(
                ic => DefaultResourceManager());
        }

        private static ILockManager DefaultLockManager()
        {
            return new DefaultLockManager(
                lockTimeout => new MonitorLock(lockTimeout));
        }

        private static IReaderWriterLockManager DefaultRWLockManager()
        {
            return new DefaultReaderWriterLockManager(
                lockTimeout => new StandardReaderWriterLock(lockTimeout));
        }

        private static IThreadLocalManager DefaultThreadLocalManager()
        {
            return new DefaultThreadLocalManager(new FastThreadLocalFactory());
        }

        private static IResourceManager DefaultResourceManager()
        {
            return new DefaultResourceManager(null, true);
        }
    }
}
