///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Default class loader provider for use with FastClass-instance creation.
    /// </summary>
    public class FastClassClassLoaderProviderDefault : FastClassClassLoaderProvider
    {
        public const string NAME = "FastClassClassLoaderProvider";

        public static readonly FastClassClassLoaderProviderDefault INSTANCE = new FastClassClassLoaderProviderDefault();

        private FastClassClassLoaderProviderDefault()
        {
        }

        public ClassLoader Classloader(Type clazz)
        {
            throw new NotSupportedException();
        }
    }
} // end of namespace
