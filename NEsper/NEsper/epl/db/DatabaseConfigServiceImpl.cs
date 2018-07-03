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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.db
{
    /// <summary> Implementation provides database instance services such as connection factory and
    /// connection settings.
    /// </summary>

    public class DatabaseConfigServiceImpl : DatabaseConfigService
    {
        private readonly IDictionary<String, ConfigurationDBRef> _mapDatabaseRef;
        private readonly IDictionary<String, DatabaseConnectionFactory> _connectionFactories;
        private readonly SchedulingService _schedulingService;
        private readonly ScheduleBucket _scheduleBucket;
        private readonly EngineImportService _engineImportService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="mapDatabaseRef">is a map of database name and database configuration entries</param>
        /// <param name="schedulingService">is for scheduling callbacks for a cache</param>
        /// <param name="scheduleBucket">is a system bucket for all scheduling callbacks for caches</param>
        /// <param name="engineImportService">The engine import service.</param>
        public DatabaseConfigServiceImpl(
            IDictionary<String, ConfigurationDBRef> mapDatabaseRef,
            SchedulingService schedulingService,
            ScheduleBucket scheduleBucket,
            EngineImportService engineImportService)
        {
            _mapDatabaseRef = mapDatabaseRef;
            _connectionFactories = new Dictionary<String, DatabaseConnectionFactory>();
            _schedulingService = schedulingService;
            _scheduleBucket = scheduleBucket;
            _engineImportService = engineImportService;
        }

        /// <summary>
        /// Returns true to indicate a setting to retain connections between lookups.
        /// </summary>
        /// <param name="databaseName">is the name of the database</param>
        /// <param name="preparedStatementText">is the sql text</param>
        /// <param name="contextAttributes">The context attributes.</param>
        /// <returns>
        /// a cache implementation to cache connection and prepared statements
        /// </returns>
        /// <throws>  DatabaseConfigException is thrown to indicate database configuration errors </throws>
        public virtual ConnectionCache GetConnectionCache(String databaseName,
                                                          String preparedStatementText,
                                                          IEnumerable<Attribute> contextAttributes)
        {
            ConfigurationDBRef config;
            if (!_mapDatabaseRef.TryGetValue(databaseName, out config))
            {
                throw new DatabaseConfigException("Cannot locate configuration information for database '" + databaseName + "'");
            }

            DatabaseConnectionFactory connectionFactory = GetConnectionFactory(databaseName);

            if (config.ConnectionLifecycle == ConnectionLifecycleEnum.RETAIN)
            {
                return new ConnectionCacheImpl(connectionFactory, preparedStatementText, contextAttributes);
            }
            else
            {
                return new ConnectionNoCacheImpl(connectionFactory, preparedStatementText, contextAttributes);
            }
        }

        /// <summary>
        /// Returns a connection factory for a configured database.
        /// </summary>
        /// <param name="databaseName">is the name of the database</param>
        /// <returns>
        /// is a connection factory to use to get connections to the database
        /// </returns>
        /// <throws>  DatabaseConfigException is thrown to indicate database configuration errors </throws>
        public virtual DatabaseConnectionFactory GetConnectionFactory(String databaseName)
        {
            // check if we already have a reference
            DatabaseConnectionFactory factory;
            if (_connectionFactories.TryGetValue(databaseName, out factory))
            {
                return factory;
            }

            ConfigurationDBRef config;
            if (!_mapDatabaseRef.TryGetValue(databaseName, out config))
            {
                throw new DatabaseConfigException("Cannot locate configuration information for database '" + databaseName + "'");
            }

            ConnectionSettings settings = config.ConnectionSettings;
            if (config.ConnectionFactoryDesc is DbDriverFactoryConnection)
            {
                DbDriverFactoryConnection dbConfig = (DbDriverFactoryConnection)config.ConnectionFactoryDesc;
                factory = new DatabaseDriverConnFactory(dbConfig, settings);
            }
            else if (config.ConnectionFactoryDesc == null)
            {
                throw new DatabaseConfigException("No connection factory setting provided in configuration");
            }
            else
            {
                throw new DatabaseConfigException("Unknown connection factory setting provided in configuration");
            }

            _connectionFactories[databaseName] = factory;

            return factory;
        }

        /// <summary>
        /// Returns a new cache implementation for this database.
        /// </summary>
        /// <param name="databaseName">the name of the database to return a new cache implementation for for</param>
        /// <param name="statementContext">The statement context.</param>
        /// <param name="epStatementAgentInstanceHandle">is the statements-own handle for use in registering callbacks with services</param>
        /// <param name="dataCacheFactory">The data cache factory.</param>
        /// <param name="streamNumber">The stream number.</param>
        /// <returns>
        /// cache implementation
        /// </returns>
        /// <exception cref="DatabaseConfigException">Cannot locate configuration information for database '" + databaseName + "'</exception>
        /// <throws>  DatabaseConfigException is thrown to indicate database configuration errors </throws>
        public virtual DataCache GetDataCache(String databaseName, StatementContext statementContext, EPStatementAgentInstanceHandle epStatementAgentInstanceHandle, DataCacheFactory dataCacheFactory, int streamNumber)
        {
            ConfigurationDBRef config;
            if (!_mapDatabaseRef.TryGetValue(databaseName, out config))
            {
                throw new DatabaseConfigException("Cannot locate configuration information for database '" + databaseName + "'");
            }

            ConfigurationDataCache dataCacheDesc = config.DataCacheDesc;
            return dataCacheFactory.GetDataCache(dataCacheDesc, statementContext, epStatementAgentInstanceHandle, _schedulingService, _scheduleBucket, streamNumber);
        }

        public ColumnSettings GetQuerySetting(String databaseName)
        {
            ConfigurationDBRef config = _mapDatabaseRef.Get(databaseName);
            if (config == null)
            {
                throw new DatabaseConfigException("Cannot locate configuration information for database '" + databaseName + '\'');
            }
            return new ColumnSettings(
                config.MetadataRetrievalEnum,
                config.ColumnChangeCase,
                config.DataTypeMapping);
        }
    }
}
