///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.db;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    ///     Database connection factory to obtain connections.
    /// </summary>
    public class DatabaseDriverConnFactory : DatabaseConnectionFactory
    {
        /// <summary>Ctor.</summary>
        /// <param name="driverResolver">resolves a db driver from a type</param>
        /// <param name="config">is the database provider configuration</param>
        /// <param name="connectionSettings">are connection-level settings</param>
        /// <throws>  DatabaseConfigException thrown if the driver class cannot be loaded </throws>
        public DatabaseDriverConnFactory(
            Func<Type, DbDriver> driverResolver,
            DriverConnectionFactoryDesc config,
            ConnectionSettings connectionSettings)
        {
            Driver = DbDriverConnectionHelper.ResolveDriver(driverResolver, config);
            ConnectionSettings = connectionSettings;
        }

        /// <summary>Gets the connection settings.</summary>
        /// <value>The connection settings.</value>
        public ConnectionSettings ConnectionSettings { get; }

        /// <summary>
        ///     Gets the database driver.
        /// </summary>
        /// <value></value>
        public virtual DbDriver Driver { get; }
    }
} // end of namespace