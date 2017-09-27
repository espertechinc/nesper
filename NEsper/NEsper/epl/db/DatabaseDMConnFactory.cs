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
using com.espertech.esper.epl.core;

using java.sql;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Database connection factory using <seealso cref="DriverManager" /> to obtain connections.
    /// </summary>
    public class DatabaseDMConnFactory : DatabaseConnectionFactory {
        private readonly ConfigurationDBRef.DriverManagerConnection driverConfig;
        private readonly ConfigurationDBRef.ConnectionSettings connectionSettings;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="driverConfig">is the driver manager configuration</param>
        /// <param name="connectionSettings">are connection-level settings</param>
        /// <param name="engineImportService">engine imports</param>
        /// <exception cref="DatabaseConfigException">thrown if the driver class cannot be loaded</exception>
        public DatabaseDMConnFactory(ConfigurationDBRef.DriverManagerConnection driverConfig,
                                     ConfigurationDBRef.ConnectionSettings connectionSettings,
                                     EngineImportService engineImportService)
                {
            this.driverConfig = driverConfig;
            this.connectionSettings = connectionSettings;
    
            // load driver class
            string driverClassName = driverConfig.ClassName;
            try {
                engineImportService.ClassForNameProvider.ClassForName(driverClassName);
            } catch (ClassNotFoundException ex) {
                throw new DatabaseConfigException("Error loading driver class '" + driverClassName + '\'', ex);
            } catch (RuntimeException ex) {
                throw new DatabaseConfigException("Error loading driver class '" + driverClassName + '\'', ex);
            }
        }
    
        /// <summary>
        /// Method to set connection-level configuration settings.
        /// </summary>
        /// <param name="connection">is the connection to set on</param>
        /// <param name="connectionSettings">are the settings to apply</param>
        /// <exception cref="DatabaseConfigException">is thrown if an SQLException is thrown</exception>
        protected static void SetConnectionOptions(Connection connection,
                                                   ConfigurationDBRef.ConnectionSettings connectionSettings)
                {
            try {
                if (connectionSettings.ReadOnly != null) {
                    connection.ReadOnly = connectionSettings.ReadOnly;
                }
            } catch (SQLException ex) {
                throw new DatabaseConfigException("Error setting read-only to " + connectionSettings.ReadOnly +
                        " on connection with detail " + GetDetail(ex), ex);
            }
    
            try {
                if (connectionSettings.TransactionIsolation != null) {
                    connection.TransactionIsolation = connectionSettings.TransactionIsolation;
                }
            } catch (SQLException ex) {
                throw new DatabaseConfigException("Error setting transaction isolation level to " +
                        connectionSettings.TransactionIsolation + " on connection with detail " + GetDetail(ex), ex);
            }
    
            try {
                if (connectionSettings.Catalog != null) {
                    connection.Catalog = connectionSettings.Catalog;
                }
            } catch (SQLException ex) {
                throw new DatabaseConfigException("Error setting catalog to '" + connectionSettings.Catalog +
                        "' on connection with detail " + GetDetail(ex), ex);
            }
    
            try {
                if (connectionSettings.AutoCommit != null) {
                    connection.Catalog = connectionSettings.Catalog;
                }
            } catch (SQLException ex) {
                throw new DatabaseConfigException("Error setting auto-commit to " + connectionSettings.AutoCommit +
                        " on connection with detail " + GetDetail(ex), ex);
            }
        }
    
        private static string GetDetail(SQLException ex) {
            return "SQLException: " + ex.Message +
                    " SQLState: " + ex.SQLState +
                    " VendorError: " + ex.ErrorCode;
        }
    
        public Connection GetConnection() {
            // use driver manager to get a connection
            Connection connection;
            string url = driverConfig.Url;
            Properties properties = driverConfig.OptionalProperties;
            if (properties == null) {
                properties = new Properties();
            }
            try {
                string user = driverConfig.OptionalUserName;
                string pwd = driverConfig.OptionalPassword;
                if ((user == null) && (pwd == null) && (properties.IsEmpty())) {
                    connection = DriverManager.GetConnection(url);
                } else if (!properties.IsEmpty()) {
                    connection = DriverManager.GetConnection(url, properties);
                } else {
                    connection = DriverManager.GetConnection(url, user, pwd);
                }
            } catch (SQLException ex) {
                string detail = "SQLException: " + ex.Message +
                        " SQLState: " + ex.SQLState +
                        " VendorError: " + ex.ErrorCode;
    
                throw new DatabaseConfigException("Error obtaining database connection using url '" + url +
                        "' with detail " + detail, ex);
            }
    
            SetConnectionOptions(connection, connectionSettings);
    
            return connection;
        }
    }
} // end of namespace
