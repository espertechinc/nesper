///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Data.Common;
using System.Linq;

using com.espertech.esper.compat.container;

namespace com.espertech.esper.epl.db.drivers
{
    public class DbProviderFactoryManagerDefault : DbProviderFactoryManager
    {
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProviderFactoryManagerDefault"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public DbProviderFactoryManagerDefault(IContainer container)
        {
            _container = container;
            RegisterDataProviders(container);
        }

        private static void RegisterDataProviders(IContainer container)
        {
#if NETFRAMEWORK
            foreach (DataRow dataTableRow in DbProviderFactories.GetFactoryClasses().Rows) {
                // Name
                // Description
                // InvariantName
                // AssemblyQualifiedName

                var dataProviderTypeName = (string) dataTableRow["InvariantName"];
                if (!container.Has(dataProviderTypeName)) {
                    container.Register<DbProviderFactory>(
                        xx => DbProviderFactories.GetFactory(dataTableRow),
                        Lifespan.Singleton,
                        dataProviderTypeName);
                }
            }
#endif

            var candidateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(DbProviderFactory)));
            foreach (var candidate in candidateTypes) {
                var dataProviderTypeName = candidate.FullName;
                if (!container.Has(dataProviderTypeName)) {
                    container.Register<DbProviderFactory>(
                        xx => CreateDataProvider(candidate),
                        Lifespan.Singleton,
                        dataProviderTypeName);
                }
            }
        }

        /// <summary>
        /// Crudely create a data provider.  If this is not sufficient for you, then
        /// simply register your data provider with the container.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        private static DbProviderFactory CreateDataProvider(Type candidate)
        {
            var constructor = candidate.GetConstructor(new Type[0]);
            var factory = (DbProviderFactory) constructor.Invoke(null);
            return factory;
        }

        /// <summary>
        /// Gets the factory associated with the name.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        public DbProviderFactory GetFactory(string factoryName)
        {
            return _container.Resolve<DbProviderFactory>(factoryName);
        }
    }
}
