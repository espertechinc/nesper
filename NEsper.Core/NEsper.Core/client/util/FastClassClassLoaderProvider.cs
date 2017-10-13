///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Type loader provider for use with FastClass-instance creation.
    /// </summary>
    public interface FastClassClassLoaderProvider
    {
        /// <summary>
        /// Returns the classloader to use.
        /// </summary>
        /// <param name="clazz">class to generate FastClass for</param>
        /// <returns>class loader</returns>
        ClassLoader Classloader(Type clazz);
    }

    public class FastClassClassLoaderProviderConstants
    {
        public const string NAME = "FastClassClassLoaderProvider";
    }

} // end of namespace
