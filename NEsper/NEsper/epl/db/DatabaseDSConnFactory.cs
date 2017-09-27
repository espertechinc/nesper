///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using java.sql;

using javax.naming;
using javax.sql;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Database connection factory using <seealso cref="InitialContext" /> and <seealso cref="DataSource" /> to obtain connections.
    /// </summary>
    public class DatabaseDSConnFactory : DatabaseConnectionFactory {
        private readonly ConfigurationDBRef.DataSourceConnection dsConfig;
        private readonly ConfigurationDBRef.ConnectionSettings connectionSettings;
    
        private DataSource dataSource;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dsConfig">is the datasource object name and initial context properties.</param>
        /// <param name="connectionSettings">are the connection-level settings</param>
        public DatabaseDSConnFactory(ConfigurationDBRef.DataSourceConnection dsConfig,
                                     ConfigurationDBRef.ConnectionSettings connectionSettings) {
            this.dsConfig = dsConfig;
            this.connectionSettings = connectionSettings;
        }
    
        public Connection GetConnection() {
            if (dataSource == null) {
                Properties envProps = dsConfig.EnvProperties;
                if (envProps == null) {
                    envProps = new Properties();
                }
    
                InitialContext ctx;
                try {
                    if (!envProps.IsEmpty()) {
                        ctx = new InitialContext(envProps);
                    } else {
                        ctx = new InitialContext();
                    }
                } catch (NamingException ex) {
                    throw new DatabaseConfigException("Error instantiating initial context", ex);
                }
    
                DataSource ds;
                string lookupName = dsConfig.ContextLookupName;
                try {
                    ds = (DataSource) ctx.Lookup(lookupName);
                } catch (NamingException ex) {
                    throw new DatabaseConfigException("Error looking up data source in context using name '" + lookupName + '\'', ex);
                }
    
                if (ds == null) {
                    throw new DatabaseConfigException("Null data source obtained through context using name '" + lookupName + '\'');
                }
    
                dataSource = ds;
            }
    
            Connection connection;
            try {
                connection = dataSource.Connection;
            } catch (SQLException ex) {
                string detail = "SQLException: " + ex.Message +
                        " SQLState: " + ex.SQLState +
                        " VendorError: " + ex.ErrorCode;
    
                throw new DatabaseConfigException("Error obtaining database connection using datasource " +
                        "with detail " + detail, ex);
            }
    
            DatabaseDMConnFactory.ConnectionOptions = connection, connectionSettings;
    
            return connection;
        }
    }
} // end of namespace
