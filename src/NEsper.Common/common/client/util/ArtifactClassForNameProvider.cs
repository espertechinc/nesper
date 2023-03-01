///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.client.util
{
#if DEPRECATED
    public class ArtifactClassForNameProvider : ClassForNameProvider
    {
        private readonly IContainer _container;
        private TypeResolver typeResolver;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public ArtifactClassForNameProvider(IContainer container)
        {
            _container = container;
        }

        public TypeResolver GetClassLoader()
        {
            lock (this) {
                if (typeResolver == null) {
                    var parentClassLoader = new TypeResolverDefault();
                    var defaultArtifactRepository = _container.ArtifactRepositoryManager().DefaultRepository;
                    typeResolver = new ArtifactTypeResolver(defaultArtifactRepository, parentClassLoader);
                }

                return typeResolver;
            }
        }

        public Type ClassForName(string className)
        {
#if false
            var simpleType = TypeHelper.GetTypeForSimpleName(className, false, false);
            if (simpleType != null) {
                return simpleType;
            }
#endif
            return GetClassLoader().ResolveType(className, true);
        }
    }
#endif
} // end of namespace