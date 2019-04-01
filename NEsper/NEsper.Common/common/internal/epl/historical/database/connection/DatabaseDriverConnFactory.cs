///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.db;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    ///     Database connection factory using <seealso cref="DriverManager" /> to obtain connections.
    /// </summary>
    public class DatabaseDriverConnFactory : DatabaseConnectionFactory
    {
        private readonly ConnectionSettings connectionSettings;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dbConfig">is the database provider configuration</param>
        /// <param name="connectionSettings">are connection-level settings</param>
        /// <throws>  DatabaseConfigException thrown if the driver class cannot be loaded </throws>
        public DatabaseDriverConnFactory(
            DbDriverFactoryConnection dbConfig,
            ConnectionSettings connectionSettings)
        {
            Driver = dbConfig.Driver;
            this.connectionSettings = connectionSettings;
        }

        /// <summary>
        ///     Gets the database driver.
        /// </summary>
        /// <value></value>
        public virtual DbDriver Driver { get; }

        /// <summary>Gets the connection settings.</summary>
        /// <value>The connection settings.</value>
        public ConnectionSettings ConnectionSettings => connectionSettings;
    }
} // end of namespace