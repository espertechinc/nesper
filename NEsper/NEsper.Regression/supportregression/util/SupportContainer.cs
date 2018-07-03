///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.db.drivers;
using com.espertech.esper.events;

namespace com.espertech.esper.supportregression.util
{
    public class SupportContainer
    {
        public static IContainer Instance;

        static SupportContainer()
        {
            Reset();
        }

        public static T Resolve<T>()
        {
            return Instance.Resolve<T>();
        }

        public static IContainer Reset()
        {
            Instance = CreateContainer();
            DbDriverPgSQL.Register(Instance);
            return Instance;
        }

        private static IContainer CreateContainer()
        {
            var container = ContainerExtensions.CreateDefaultContainer(false);
            container.Register<IResourceManager>(
                xx => new DefaultResourceManager(true,
                    @"..\..\..\etc",
                    @"..\..\..\..\etc",
                    @"..\..\..\..\..\etc"),
                Lifespan.Singleton);

            container.Register<EventAdapterService>(
                xx => SupportEventAdapterService.Allocate(
                    container, container.ClassLoaderProvider()),
                Lifespan.Singleton);

            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }
    }
}
