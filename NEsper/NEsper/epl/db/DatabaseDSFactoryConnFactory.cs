///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.util;

using java.sql;

using javax.sql;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Database connection factory using <seealso cref="javax.naming.InitialContext" /> and <seealso cref="javax.sql.DataSource" /> to obtain connections.
    /// </summary>
    public class DatabaseDSFactoryConnFactory : DatabaseConnectionFactory {
        private readonly ConfigurationDBRef.ConnectionSettings connectionSettings;
        private DataSource dataSource;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dsConfig">is the datasource object name and initial context properties.</param>
        /// <param name="connectionSettings">are the connection-level settings</param>
        /// <param name="engineImportService">engine imports</param>
        /// <exception cref="DatabaseConfigException">when the factory cannot be configured</exception>
        public DatabaseDSFactoryConnFactory(ConfigurationDBRef.DataSourceFactory dsConfig,
                                            ConfigurationDBRef.ConnectionSettings connectionSettings,
                                            EngineImportService engineImportService)
                {
            this.connectionSettings = connectionSettings;
    
            Type clazz;
            try {
                clazz = engineImportService.ClassForNameProvider.ClassForName(dsConfig.FactoryClassname);
            } catch (ClassNotFoundException e) {
                throw new DatabaseConfigException("Type '" + dsConfig.FactoryClassname + "' cannot be loaded", e);
            }
    
            Object obj;
            try {
                obj = clazz.NewInstance();
            } catch (InstantiationException e) {
                throw new ConfigurationException("Type '" + clazz + "' cannot be instantiated", e);
            } catch (IllegalAccessException e) {
                throw new ConfigurationException("Illegal access instantiating class '" + clazz + "'", e);
            }
    
            // find method : static DataSource CreateDataSource(Properties properties)
            Method method;
            try {
                method = clazz.GetMethod("createDataSource", Typeof(Properties));
            } catch (NoSuchMethodException e) {
                throw new ConfigurationException("Type '" + clazz + "' does not provide a static method by name createDataSource accepting a single Properties object as parameter", e);
            }
            if (method == null) {
                throw new ConfigurationException("Type '" + clazz + "' does not provide a static method by name createDataSource accepting a single Properties object as parameter");
            }
            if (!JavaClassHelper.IsImplementsInterface(method.ReturnType, Typeof(DataSource))) {
                throw new ConfigurationException("On class '" + clazz + "' the static method by name createDataSource does not return a DataSource");
            }
    
            Object result;
            try {
                result = method.Invoke(obj, dsConfig.Properties);
            } catch (IllegalAccessException e) {
                throw new ConfigurationException("Type '" + clazz + "' failed in method createDataSource :" + e.Message, e);
            } catch (InvocationTargetException e) {
                throw new ConfigurationException("Type '" + clazz + "' failed in method createDataSource :" + e.Message, e);
            }
            if (result == null) {
                throw new ConfigurationException("Method createDataSource returned a null value for DataSource");
            }
    
            dataSource = (DataSource) result;
        }
    
        public Connection GetConnection() {
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
