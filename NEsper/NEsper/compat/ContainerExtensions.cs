///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Castle.Windsor;

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat
{
    public static class ContainerExtensions
    {
        public static IDisposable Start(this IWindsorContainer container)
        {
            return ScopedInstance<IWindsorContainer>.Set(container);
        }

        public static IWindsorContainer CurrentContainer
        {
            get { return ScopedInstance<IWindsorContainer>.Current; }
        }

        public static T Get<T>(this IWindsorContainer container)
        {
            return container.Resolve<T>();
        }

        public static ILockManager LockManager(this IWindsorContainer container)
        {
            return container.Resolve<ILockManager>();
        }

        public static ILockManager LockManager()
        {
            return CurrentContainer.Resolve<ILockManager>();
        }

        public static IReaderWriterLockManager RWLockManager(this IWindsorContainer container)
        {
            return container.Resolve<IReaderWriterLockManager>();
        }

        public static IReaderWriterLockManager RWLockManager()
        {
            return CurrentContainer.Resolve<IReaderWriterLockManager>();
        }

        public static IThreadLocalManager ThreadLocalManager(this IWindsorContainer container)
        {
            return container.Resolve<IThreadLocalManager>();
        }

        public static IThreadLocalManager ThreadLocalManager()
        {
            return CurrentContainer.Resolve<IThreadLocalManager>();
        }

        public static ResourceManager ResourceManager(this IWindsorContainer container)
        {
            return container.Resolve<ResourceManager>();
        }

        public static ResourceManager ResourceManager()
        {
            return CurrentContainer.Resolve<ResourceManager>();
        }
    }
}
