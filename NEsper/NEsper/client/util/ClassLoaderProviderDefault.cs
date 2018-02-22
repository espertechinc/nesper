///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using Castle.MicroKernel.Registration;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Default class loader provider returns the current thread context classloader.
    /// </summary>
    public class ClassLoaderProviderDefault : ClassLoaderProvider
    {
        public const string NAME = "ClassLoaderProvider";

        private readonly ClassLoader _classLoader;

        public ClassLoaderProviderDefault(ClassLoader classLoader)
        {
            _classLoader = classLoader;
        }

#if FALSE
        public ClassLoaderProviderDefault(IContainer container)
        {
            container.Register<ClassLoader, ClassLoaderDefault>(Lifespan.Singleton);
            _classLoader = container.Resolve<ClassLoader>();
        }
#endif

        public ClassLoader GetClassLoader()
        {
            return _classLoader;
        }
    }
} // end of namespace
