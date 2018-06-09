///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Data.Common;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// Allows the application to drive the database providers that are available to
    /// the process space.
    /// </summary>
    /// <seealso cref="com.espertech.esper.epl.db.drivers.DbProviderFactoryManager" />
    public class DbProviderFactoryManagerCustom : DbProviderFactoryManager
    {
        private readonly IDictionary<string, DbProviderFactory> _nameToProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProviderFactoryManagerCustom"/> class.
        /// </summary>
        public DbProviderFactoryManagerCustom()
        {
            _nameToProviderFactory = new Dictionary<string, DbProviderFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProviderFactoryManagerCustom"/> class.
        /// </summary>
        /// <param name="nameToProviderFactory">The name to provider factory.</param>
        public DbProviderFactoryManagerCustom(IDictionary<string, DbProviderFactory> nameToProviderFactory)
        {
            _nameToProviderFactory = nameToProviderFactory;
        }

        /// <summary>
        /// Adds the provider.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="providerFactory">The provider factory.</param>
        public void AddProvider(string name, DbProviderFactory providerFactory)
        {
            _nameToProviderFactory[name] = providerFactory;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _nameToProviderFactory.Clear();
        }

        /// <summary>
        /// Gets the factory associated with the name.
        /// </summary>
        /// <param name="factoryName">Name of the factory.</param>
        public DbProviderFactory GetFactory(string factoryName)
        {
            return _nameToProviderFactory.Get(factoryName);
        }
    }
}
