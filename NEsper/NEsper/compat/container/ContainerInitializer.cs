///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.db.drivers;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

using Castle.MicroKernel.Registration;

namespace com.espertech.esper.compat.container
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

        public static IContainer InitializeDatabaseDrivers(this IContainer container)
        {
            if (container.DoesNotHave<DbProviderFactoryManager>()) {
                container.Register<DbProviderFactoryManager, DbProviderFactoryManagerDefault>(Lifespan.Singleton);
            }

            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsImplementsInterface<DbDriver>());

            if (container is ContainerImpl containerImpl) {
                foreach (var type in types) {
                    if ((type != typeof(DbDriver)) && (!type.IsAbstract)) {
                        RegisterDatabaseDriver(containerImpl, type);
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
            if (container.DoesNotHave<IConfigurationParser>())
                container.Register<IConfigurationParser, ConfigurationParser>(
                    Lifespan.Transient);
            if (container.DoesNotHave<ClassLoader>())
                container.Register<ClassLoader, ClassLoaderDefault>(
                    Lifespan.Singleton);
            if (container.DoesNotHave<ClassLoaderProvider>())
                container.Register<ClassLoaderProvider, ClassLoaderProviderDefault>(
                    Lifespan.Singleton);
            if (container.DoesNotHave<Directory>())
                container.Register<Directory, SimpleServiceDirectory>(
                    Lifespan.Singleton);
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
