///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    public class ClassLoaderDefault : ClassLoader
    {
        private IResourceManager _resourceManager;

        public ClassLoaderDefault(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public Stream GetResourceAsStream(string resourceName)
        {
            return _resourceManager.GetResourceAsStream(resourceName);
        }

        public Type GetClass(string typeName)
        {
            return TypeHelper.ResolveType(typeName, true);
        }
    }
}