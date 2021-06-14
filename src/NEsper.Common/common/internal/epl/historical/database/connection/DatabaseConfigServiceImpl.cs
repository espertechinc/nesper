///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using ColumnSettings = com.espertech.esper.common.@internal.epl.historical.database.core.ColumnSettings;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    ///     Implementation provides database instance services such as connection factory and
    ///     connection settings.
    /// </summary>
    public class DatabaseConfigServiceImpl : DatabaseConfigServiceCompileTime,
        DatabaseConfigServiceRuntime
    {
        private readonly IContainer _container;
        private readonly ImportService _importService;
        private readonly IDictionary<string, DatabaseConnectionFactory> _connectionFactories;
        private readonly IDictionary<string, ConfigurationCommonDBRef> _mapDatabaseRef;

        /// <summary>Ctor.</summary>
        /// <param name="container">the container</param>
        /// <param name="mapDatabaseRef">is a map of database name and database configuration entries</param>
        /// <param name="importService">imports</param>
        public DatabaseConfigServiceImpl(
            IContainer container,
            IDictionary<string, ConfigurationCommonDBRef> mapDatabaseRef,
            ImportService importService)
        {
            _container = container;
            _mapDatabaseRef = mapDatabaseRef;
            _connectionFactories = new Dictionary<string, DatabaseConnectionFactory>();
            _importService = importService;
        }

        public DatabaseConnectionFactory GetConnectionFactory(string databaseName)
        {
            // check if we already have a reference
            var factory = _connectionFactories.Get(databaseName);
            if (factory != null) {
                return factory;
            }

            var config = _mapDatabaseRef.Get(databaseName);
            if (config == null) {
                throw new DatabaseConfigException(
                    "Cannot locate configuration information for database '" + databaseName + '\'');
            }

            var settings = config.ConnectionSettings;
            if (config.ConnectionFactoryDesc is DriverConnectionFactoryDesc dbConfig) {
                factory = new DatabaseDriverConnFactory(_container, dbConfig, settings);
            }
            else if (config.ConnectionFactoryDesc == null) {
                throw new DatabaseConfigException("No connection factory setting provided in configuration");
            }
            else {
                throw new DatabaseConfigException("Unknown connection factory setting provided in configuration");
            }

            _connectionFactories.Put(databaseName, factory);

            return factory;
        }

        public ColumnSettings GetQuerySetting(string databaseName)
        {
            var config = _mapDatabaseRef.Get(databaseName);
            if (config == null) {
                throw new DatabaseConfigException(
                    "Cannot locate configuration information for database '" + databaseName + '\'');
            }

            return new ColumnSettings(
                config.MetadataRetrievalEnum,
                config.ColumnChangeCase,
                config.DataTypesMapping);
        }

        public HistoricalDataCache GetDataCache(
            string databaseName,
            AgentInstanceContext agentInstanceContext,
            int streamNumber,
            int scheduleCallbackId)
        {
            var config = _mapDatabaseRef.Get(databaseName);
            if (config == null) {
                throw new DatabaseConfigException(
                    "Cannot locate configuration information for database '" + databaseName + '\'');
            }

            var dataCacheDesc = config.DataCacheDesc;
            return agentInstanceContext.HistoricalDataCacheFactory.GetDataCache(
                dataCacheDesc,
                agentInstanceContext,
                streamNumber,
                scheduleCallbackId);
        }

        public ConnectionCache GetConnectionCache(
            string databaseName,
            string preparedStatementText,
            IEnumerable<Attribute> contextAttributes)
        {
            var config = _mapDatabaseRef.Get(databaseName);
            if (config == null) {
                throw new DatabaseConfigException(
                    "Cannot locate configuration information for database '" + databaseName + '\'');
            }

            var connectionFactory = GetConnectionFactory(databaseName);

            var retain = config.ConnectionLifecycleEnum.Equals(ConnectionLifecycleEnum.RETAIN);
            if (retain) {
                return new ConnectionCacheImpl(connectionFactory, preparedStatementText, contextAttributes);
            }

            return new ConnectionCacheNoCacheImpl(connectionFactory, preparedStatementText, contextAttributes);
        }
    }
} // end of namespace