///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Default class loader provider returns the current thread context classloader.
    /// </summary>
    public class TypeResolverProviderDefault : TypeResolverProvider
    {
        public const string NAME = "ClassLoaderProvider";

        private readonly TypeResolver typeResolver;

        public TypeResolverProviderDefault(TypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
        }

#if FALSE
        public ClassLoaderProviderDefault(IContainer container)
        {
            container.Register<ClassLoader, ClassLoaderDefault>(Lifespan.Singleton);
            _classLoader = container.Resolve<ClassLoader>();
        }
#endif

        public TypeResolver GetTypeResolver()
        {
            return typeResolver;
        }
    }
} // end of namespace