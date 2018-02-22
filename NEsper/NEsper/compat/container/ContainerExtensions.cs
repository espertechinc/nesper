///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.util;
using com.espertech.esper.compat.threading;

using Castle.Windsor;

namespace com.espertech.esper.compat.container
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Creates the default service collection.
        /// </summary>
        /// <returns></returns>
        public static IContainer CreateDefaultContainer(bool initialize = true)
        {
            var wrapper = new ContainerImpl(new WindsorContainer());
            if (initialize) {
                ContainerInitializer.InitializeDefaultServices(wrapper);
            }

            return wrapper;
        }

        public static ILockManager LockManager(this IContainer container)
        {
            return container.Resolve<ILockManager>();
        }

        public static IReaderWriterLockManager RWLockManager(this IContainer container)
        {
            return container.Resolve<IReaderWriterLockManager>();
        }

        public static IThreadLocalManager ThreadLocalManager(this IContainer container)
        {
            return container.Resolve<IThreadLocalManager>();
        }

        public static IResourceManager ResourceManager(this IContainer container)
        {
            return container.Resolve<IResourceManager>();
        }

        public static ClassLoaderProvider ClassLoaderProvider(this IContainer container)
        {
            return container.Resolve<ClassLoaderProvider>();
        }
    }
}
