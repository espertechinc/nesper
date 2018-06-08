///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.compat
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
    }
}
