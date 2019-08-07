///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.regressionlib.support.util
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
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                var container = ContainerExtensions.CreateDefaultContainer(false);
            container.Register<IResourceManager>(
                xx => new DefaultResourceManager(
                    true,
                    Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "etc")),
                    Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "etc")),
                    Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "etc"))),
                Lifespan.Singleton);

            //SupportEventTypeFactory.RegisterSingleton(container);
            //SupportExprNodeFactory.RegisterSingleton(container);
            //SupportDatabaseService.RegisterSingleton(container);

            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }
    }
}