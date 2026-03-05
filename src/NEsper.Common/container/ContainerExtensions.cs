///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using Castle.Windsor;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.container
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
                wrapper.InitializeDefaultServices();
            }

            return wrapper;
        }

        public static void CheckContainer(this IContainer container)
        {
            if (container == null) {
                throw new ArgumentException(
                    "container is null, please initialize",
                    nameof(container));
            }
        }

        [Obsolete("Use constructor injection instead")]
        public static ILockManager LockManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<ILockManager>();
        }

        [Obsolete("Use constructor injection instead")]
        public static IReaderWriterLockManager RWLockManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IReaderWriterLockManager>();
        }

        [Obsolete("Use constructor injection instead")]
        public static IThreadLocalManager ThreadLocalManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IThreadLocalManager>();
        }

        [Obsolete("Use constructor injection instead")]
        public static IResourceManager ResourceManager(this IContainer container)
        {
            container.CheckContainer();
            return container.Resolve<IResourceManager>();
        }
    }
}
