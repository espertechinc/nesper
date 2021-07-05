///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

#if NETSTANDARD
using System.Runtime.Loader;
#endif

using Castle.MicroKernel.Registration;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.db.drivers;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.container
{
    /// <summary>
    /// Initializes a service container with a "default" set of services.
    /// </summary>
    public static class ContainerInitializer
    {
        private static void RegisterDatabaseDriver(this ContainerImpl container, Type driverType)
        {
            container.WindsorContainer.Register(
                Component.For(typeof(DbDriver))
                    .Named(driverType.FullName)
                    .ImplementedBy(driverType)
                    .LifestyleSingleton()
            );
        }

        public static IContainer RegisterDatabaseDriver(this IContainer container, Type driverType)
        {
            if (container is ContainerImpl containerImpl) {
                RegisterDatabaseDriver(containerImpl, driverType);
            }

            return container;
        }

        public static IContainer InitializeDatabaseDrivers(this IContainer container, bool scanAssembliesInAppDomain = false)
        {
            if (container.DoesNotHave<DbProviderFactoryManager>())
            {
                container.Register<DbProviderFactoryManager, DbProviderFactoryManagerDefault>(Lifespan.Singleton);
            }

            if (scanAssembliesInAppDomain) {
                if (container is ContainerImpl containerImpl) {
                    var types = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(TypeExtensions.IsImplementsInterface<DbDriver>);

                    foreach (var type in types) {
                        if ((type != typeof(DbDriver)) && (!type.IsAbstract)) {
                            RegisterDatabaseDriver(containerImpl, type);
                        }
                    }
                }
            }

            return container;
        }

        /// <summary>
        /// Initializes the default services.
        /// </summary>
        /// <param name="container">The service container.</param>
        public static IContainer InitializeDefaultServices(this IContainer container)
        {
            if (container.DoesNotHave<IContainer>())
                container.Register<IContainer>(
                    container, Lifespan.Singleton);
            if (container.DoesNotHave<ILockManager>())
                container.Register<ILockManager>(
                    ic => DefaultLockManager(), Lifespan.Singleton);
            if (container.DoesNotHave<IReaderWriterLockManager>())
                container.Register<IReaderWriterLockManager>(
                    ic => DefaultRWLockManager(), Lifespan.Singleton);
            if (container.DoesNotHave<IThreadLocalManager>())
                container.Register<IThreadLocalManager>(
                    ic => DefaultThreadLocalManager(), Lifespan.Singleton);
            if (container.DoesNotHave<IResourceManager>())
                container.Register<IResourceManager>(
                    ic => DefaultResourceManager(), Lifespan.Singleton);
            //if (container.DoesNotHave<IConfigurationParser>())
            //    container.Register<IConfigurationParser, ConfigurationParser>(
            //        Lifespan.Transient);
            if (container.DoesNotHave<ClassLoader>())
                container.Register<ClassLoader, ClassLoaderDefault>(
                    Lifespan.Singleton);
            if (container.DoesNotHave<ClassLoaderProvider>())
                container.Register<ClassLoaderProvider, ClassLoaderProviderDefault>(
                    Lifespan.Singleton);
            if (container.DoesNotHave<INamingContext>())
                container.Register<INamingContext, SimpleNamingContext>(
                    Lifespan.Singleton);
            if (container.DoesNotHave<IObjectCopier>()) {
                container.Register<IObjectCopier>(
                    ic => new SerializableObjectCopier(ic),
                    Lifespan.Singleton);
            }

            return container;
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
            return new DefaultThreadLocalManager(new SystemThreadLocalFactory());
        }

        private static IResourceManager DefaultResourceManager()
        {
            return new DefaultResourceManager(true);
        }
    }
}