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
    /// ClassLoader provides a limited amount of cross-over functionality
    /// from the "Java" world.  In short, it exists to load classes.
    /// </summary>
    public interface ClassLoader
    {
        /// <summary>Gets the class.</summary>
        /// <param name="typeName">Name of the class.</param>
        /// <returns></returns>
        Type GetClass(string typeName);
    }
}
