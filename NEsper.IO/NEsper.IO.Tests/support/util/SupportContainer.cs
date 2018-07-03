///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

namespace com.espertech.esperio.support.util
{
    public class SupportContainer
    {
        public static IContainer Instance;

        static SupportContainer()
        {
            Instance = CreateContainer();
        }

        public static T Resolve<T>()
        {
            return Instance.Resolve<T>();
        }

        public static IContainer Reset()
        {
            return Instance = CreateContainer();
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

            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }
    }
}
