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
    /// Provider of a classloader.
    /// </summary>
    public interface TypeResolverProvider
    {
        /// <summary>
        /// Returns the classloader.
        /// </summary>
        /// <returns>classloader</returns>
        public TypeResolver TypeResolver { get; }
    }

    public class ClassLoaderProviderConstants
    {
        public const string NAME = "ClassLoaderProvider";
    }
} // end of namespace