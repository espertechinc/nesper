///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.timers;
using com.espertech.esper.container;

namespace com.espertech.esper.runtime
{
    public class SupportContainer
    {
        public static IContainer CreateContainer()
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

            container.Register<ITimerFactory>(
                ic => new SimpleTimerFactory(),
                Lifespan.Singleton);

            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }
    }
}
