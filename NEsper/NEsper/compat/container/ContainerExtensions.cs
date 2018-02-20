///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.schedule;

using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace com.espertech.esper.compat.container
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Creates the default service collection.
        /// </summary>
        /// <returns></returns>
        public static IContainer CreateDefaultContainer()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(
                Component.For<EPServiceProvider>()
                    .ImplementedBy<EPServiceProviderImpl>()
                    .LifeStyle.Transient
            );
            container.Register(
                Component.For<EPServiceProviderSPI>()
                    .ImplementedBy<EPServiceProviderImpl>()
                    .LifeStyle.Transient
            );
            container.Register(
                Component.For<Directory>()
                    .ImplementedBy<SimpleServiceDirectory>()
                    .LifeStyle.Singleton
            );
            container.Register(
                Component.For<StatementEventTypeRef>()
                    .ImplementedBy<StatementEventTypeRefImpl>()
                    .LifeStyle.Transient
            );
            container.Register(
                Component.For<StatementVariableRef>()
                    .ImplementedBy<StatementVariableRefImpl>()
                    .LifeStyle.Transient
            );
            container.Register(
                Component.For<TableService>()
                    .ImplementedBy<TableServiceImpl>()
                    .LifeStyle.BoundTo<EPServiceProviderSPI>()
            );
            container.Register(
                Component.For<VariableService>()
                    .ImplementedBy<VariableServiceImpl>()
                    .LifeStyle.BoundTo<EPServiceProviderSPI>()
            );
            container.Register(
                Component.For<StatementLockFactory>()
                    .ImplementedBy<StatementLockFactoryImpl>()
                    .LifeStyle.Transient
            );
            container.Register(
                Component.For<SchedulingServiceSPI>()
                    .ImplementedBy<SchedulingServiceImpl>()
                    .LifeStyle.BoundTo<EPServiceProviderSPI>()
            );

            IContainer wrapper = new ContainerImplWindsor(container);
            ContainerInitializer.InitializeDefaultServices(wrapper);
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
