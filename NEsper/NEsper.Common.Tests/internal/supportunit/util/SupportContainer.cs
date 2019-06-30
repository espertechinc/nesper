///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.supportunit.db;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.util
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

            SupportEventTypeFactory.RegisterSingleton(container);
            SupportExprNodeFactory.RegisterSingleton(container);
            SupportDatabaseService.RegisterSingleton(container);
            SupportJoinResultNodeFactory.RegisterSingleton(container);

            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }
    }
}
