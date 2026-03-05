///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.db;

namespace com.espertech.esper.common.client.db
{
    /// <summary>
    /// Interface for resolving driver types to driver instances.
    /// </summary>
    public interface IDriverResolver
     {
        /// <summary>
        /// Resolves a driver type to a driver instance.
        /// </summary>
        /// <param name="type">The driver type to resolve.</param>
        /// <returns>The resolved driver instance.</returns>
        public DbDriver Resolve(Type type);
    }
}