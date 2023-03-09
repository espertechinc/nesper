///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// TypeResolver is similar to the ClassLoader of Java, but it's purpose is to provide
    /// resolution of types that may or may not be materialized into the process space.
    /// </summary>
    public interface TypeResolver
    {
        /// <summary>Gets the class; potentially resolving the class if it has not been loaded into materialized space.</summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="resolve">if true, the resolver should attempt to resolve the type if it is not loaded</param>
        /// <returns></returns>
        Type ResolveType(string typeName, bool resolve = false);
    }
}
