///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.db.drivers
{
    public class DbProviderFactoryManagerDefault : DbProviderFactoryManager
    {
        private readonly IDictionary<string, DbProviderFactory> _factories;

        public DbProviderFactoryManagerDefault()
        {
            _factories = RegisterDataProviders();
        }

        private static IDictionary<string, DbProviderFactory> RegisterDataProviders()
        {
            var factories = new Dictionary<string, DbProviderFactory>();
            var candidateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(DbProviderFactory)));
            foreach (var candidate in candidateTypes) {
                var dataProviderTypeName = candidate.FullName;
                if (!factories.ContainsKey(dataProviderTypeName)) {
                    factories[dataProviderTypeName] = CreateDataProvider(candidate);
                }
            }

            return factories;
        }

        /// <summary>
        /// Crudely create a data provider.  If this is not sufficient for you, then
        /// simply register your data provider with the container.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        private static DbProviderFactory CreateDataProvider(Type candidate)
        {
            var constructor = candidate.GetConstructor(Type.EmptyTypes);
            var factory = (DbProviderFactory)constructor.Invoke(null);
            return factory;
        }

        /// <summary>
        /// Gets the factory associated with the name.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        public DbProviderFactory GetFactory(string factoryName)
        {
            if (_factories.TryGetValue(factoryName, out var factory)) {
                return factory;
            }

            throw new EPException("DbProviderFactory not found for name '" + factoryName + "'");
        }
    }
}