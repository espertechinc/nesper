///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;

using com.espertech.esper.common.client.db;

namespace com.espertech.esper.common.@internal.db
{
    public class StatefulDriverResolver : IDriverResolver
    {
        // Constructive drivers can be expensive.  If we are using activation, we only construct the
        // driver one time.
        private readonly ConcurrentDictionary<Type, DbDriver> _driverTable = new ConcurrentDictionary<Type, DbDriver>();

        /// <summary>
        /// Resolves a driver type to a driver instance.
        /// </summary>
        /// <param name="driverType">The driver type to resolve.</param>
        /// <returns>The resolved driver instance.</returns>
        public DbDriver Resolve(Type driverType)
        {
            ArgumentNullException.ThrowIfNull(driverType);
            return _driverTable.GetOrAdd(
                driverType,
                type => (DbDriver)Activator.CreateInstance(type)!
            );
        }
    }
}