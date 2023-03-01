///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvidedImportClassLoaderFactory
    {
        public static TypeResolver GetClassLoader(
            IArtifactRepository artifactRepository,
            TypeResolver parentTypeResolver,
            PathRegistry<string, ClassProvided> classProvidedPathRegistry)
        {
            if (classProvidedPathRegistry.IsEmpty()) {
                return new ArtifactTypeResolver(artifactRepository, parentTypeResolver);
            }
            
            return new ClassProvidedImportTypeResolver(artifactRepository, parentTypeResolver, classProvidedPathRegistry);
        }
    }
} // end of namespace