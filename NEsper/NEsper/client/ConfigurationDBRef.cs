///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Data;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Container for database configuration information, such as
    /// options around getting a database connection and options to control the lifecycle
    /// of connections and set connection parameters.
    /// </summary>

    [Serializable]
    public class ConfigurationDBRef
    {
        private ConnectionFactoryDesc _connectionFactoryDesc;
        private readonly ConnectionSettings _connectionSettings;
        private ConnectionLifecycleEnum _connectionLifecycleEnum;
        private ConfigurationDataCache _dataCacheDesc;
        private MetadataOriginEnum _metadataOrigin;
        private ColumnChangeCaseEnum _columnChangeCase;
        private readonly IDictionary<Type, Type> _dataTypeMapping;

        /// <summary>
        /// Gets or sets the auto-commit connection settings for new connections to this database.
        /// </summary>

        public virtual bool? ConnectionAutoCommit
        {
            get => _connectionSettings.AutoCommit;
            set => _connectionSettings.AutoCommit = value;
        }

        /// <summary>
        /// Gets or sets the transaction isolation level on new connections created for this database.
        /// </summary>

        public virtual IsolationLevel ConnectionTransactionIsolation
        {
            get => _connectionSettings.TransactionIsolation.GetValueOrDefault();
            set => _connectionSettings.TransactionIsolation = value;
        }

        /// <summary>
        /// Gets or sets the catalog name for new connections created for this database.
        /// </summary>

        public virtual String ConnectionCatalog
        {
            get => _connectionSettings.Catalog;
            set => _connectionSettings.Catalog = value;
        }

        /// <summary>
        /// Gets or sets the LRU cache to a given size for the database.
        /// </summary>

        public virtual int LRUCache
        {
            set => _dataCacheDesc = new ConfigurationLRUCache(value);
        }

        /// <summary>
        /// Ctor.
        /// </summary>

        public ConfigurationDBRef()
        {
            _connectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            _connectionSettings = new ConnectionSettings();
            _metadataOrigin = MetadataOriginEnum.DEFAULT;
            _columnChangeCase = ColumnChangeCaseEnum.NONE;
            _dataTypeMapping = new Dictionary<Type, Type>();
        }

        /// <summary>
        /// Sets the database provider connection.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="driverName">Name of the driver.</param>
        /// <param name="properties">The properties.</param>

        public void SetDatabaseDriver(IContainer container, String driverName, Properties properties)
        {
            var driver = DbDriverFactoryConnection.ResolveDriverFromName(
                container, driverName);
            driver.Properties = properties;

            _connectionFactoryDesc = new DbDriverFactoryConnection(driver);
        }

        /// <summary>
        /// Sets the database driver.
        /// </summary>
        /// <param name="dbDriverFactoryConnection">The db driver factory connection.</param>
        public void SetDatabaseDriver(DbDriverFactoryConnection dbDriverFactoryConnection)
        {
            _connectionFactoryDesc = dbDriverFactoryConnection;    
        }

        /// <summary>
        /// Sets the database driver.
        /// </summary>
        /// <param name="dbDriverFactoryConnection">The db driver factory connection.</param>
        /// <param name="properties">The properties.</param>
        public void SetDatabaseDriver(DbDriverFactoryConnection dbDriverFactoryConnection, Properties properties)
        {
            _connectionFactoryDesc = new DbDriverFactoryConnection(
                dbDriverFactoryConnection.Driver);
        }

        /// <summary>
        /// Sets the database driver.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="dbSpecification">The db specification.</param>
        public void SetDatabaseDriver(IContainer container, DbDriverConfiguration dbSpecification)
        {
            var driver = DbDriverFactoryConnection.ResolveDriverFromName(container, dbSpecification.DriverName);
            driver.Properties = dbSpecification.Properties;

            _connectionFactoryDesc = new DbDriverFactoryConnection(driver);
        }

        /// <summary> Returns the connection settings for this database.</summary>
        /// <returns> connection settings
        /// </returns>

        public virtual ConnectionSettings ConnectionSettings => _connectionSettings;

        /// <summary>
        /// Gets or sets the setting to control whether a new connection is obtained
        /// for each lookup, or connections are retained between lookups. Controls
        /// whether a new connection is obtained for each lookup, or connections
        /// are retained between lookups.
        /// </summary>

        public virtual ConnectionLifecycleEnum ConnectionLifecycle
        {
            get => _connectionLifecycleEnum;
            set => _connectionLifecycleEnum = value;
        }

        /// <summary>
        /// Gets the descriptor controlling connection creation settings.
        /// </summary>

        public virtual ConnectionFactoryDesc ConnectionFactoryDesc
        {
            get => _connectionFactoryDesc;
            set => _connectionFactoryDesc = value;
        }

        /// <summary>
        /// Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds.
        /// <para>
        /// Specifies the cache reference type to be weak references. Weak reference cache entries become
        /// eligible for garbage collection and are removed from cache when the garbage collection requires so.
        /// </para>
        /// </summary>
        /// <param name="maxAgeSeconds">is the maximum number of seconds before a query result is considered stale (also known as time-to-live)
        /// </param>
        /// <param name="purgeIntervalSeconds">is the interval at which the engine purges stale data from the cache
        /// </param>

        public virtual void SetExpiryTimeCache(double maxAgeSeconds, double purgeIntervalSeconds)
        {
            _dataCacheDesc = new ConfigurationExpiryTimeCache(maxAgeSeconds, purgeIntervalSeconds, ConfigurationCacheReferenceTypeHelper.GetDefault());
        }

        /// <summary>
        /// Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds. Also allows
        /// setting the reference type indicating whether garbage collection may remove entries from cache.
        /// </summary>
        /// <param name="maxAgeSeconds">the maximum number of seconds before a query result is considered stale (also known as time-to-live)</param>
        /// <param name="purgeIntervalSeconds">the interval at which the engine purges stale data from the cache.</param>
        /// <param name="cacheReferenceType">specifies the reference type to use</param>
        public virtual void SetExpiryTimeCache(double maxAgeSeconds, double purgeIntervalSeconds, ConfigurationCacheReferenceType cacheReferenceType)
        {
            _dataCacheDesc = new ConfigurationExpiryTimeCache(maxAgeSeconds, purgeIntervalSeconds, cacheReferenceType);
        }

        /// <summary>
        /// Gets a query result data cache descriptor.
        /// </summary>
        public virtual ConfigurationDataCache DataCacheDesc => _dataCacheDesc;

        /// <summary>
        /// Adds the SQL types binding.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <param name="desiredType">The desired type.</param>
        public void AddSqlTypeBinding(Type sqlType, Type desiredType)
        {
            DatabaseTypeEnum typeEnum = DatabaseTypeEnum.GetEnum(desiredType.FullName);
            if (typeEnum == null)
            {
                String supported = DatabaseTypeEnum.Values.Render();
                throw new ConfigurationException("Unsupported type '" + desiredType.FullName + "' when expecting any of: " + supported);
            }
            this._dataTypeMapping[sqlType] = desiredType;
        }

        /// <summary>
        /// Returns the mapping of types that the engine must perform
        /// when receiving output columns of that sql types.
        /// </summary>
        public IDictionary<Type, Type> DataTypeMapping => _dataTypeMapping;

        /// <summary>
        /// Returns an enumeration indicating how the engine retrieves metadata about the columns
        /// that a given SQL query returns.
        /// <para/>
        /// The engine requires to retrieve result column names and types in order to build a resulting
        /// event type and perform expression type checking.
        /// </summary>
        /// <returns>indication how to retrieve metadata</returns>
        public MetadataOriginEnum MetadataRetrievalEnum => _metadataOrigin;

        /// <summary>
        /// Gets and sets an indicator that indicates how the engine should retrieve
        /// metadata about the columns that a given SQL query returns.
        /// <para>
        /// The engine requires to retrieve result column names and types in order to build a resulting
        /// event type and perform expression type checking.
        /// </para>
        /// </summary>
        public MetadataOriginEnum MetadataOrigin
        {
            get => _metadataOrigin;
            set => _metadataOrigin = value;
        }

        /// <summary>
        /// Gets or sets an enum value determining how the engine changes case
        /// on output column names returned from statement or statement result
        /// set metadata.
        /// </summary>
        /// <returns>change case enums</returns>
        public ColumnChangeCaseEnum ColumnChangeCase
        {
            get => _columnChangeCase;
            set => _columnChangeCase = value;
        }

        /// <summary>
        /// Indicates how the engine retrieves metadata about a statement's output columns.
        /// </summary>
        [Serializable]
        public enum MetadataOriginEnum
        {
            /// <summary>
            /// By default, get output column metadata from the prepared statement., unless
            /// an Oracle connection class is used in which case the behavior is SAMPLE.
            /// </summary>
            DEFAULT,

            /// <summary>
            /// Always get output column metadata from the prepared statement regardless of what driver
            /// or connection is used.
            /// </summary>
            METADATA,

            /// <summary>
            /// Obtain output column metadata by executing a sample query statement at statement
            /// compilation time. The sample statement
            /// returns the same columns as the statement executed during event processing.
            /// See the documentation for the generation or specication of the sample query statement.
            /// </summary>
            SAMPLE
        }

        /// <summary>
        /// Controls how output column names get reflected in the event type.
        /// </summary>
        [Serializable]
        public enum ColumnChangeCaseEnum
        {
            /// <summary>
            /// Leave the column names the way the database driver represents the column.
            /// </summary>
            NONE,

            /// <summary>
            /// Change case to lowercase on any column names returned by statement metadata.
            /// </summary>
            LOWERCASE,

            /// <summary>
            /// Change case to uppercase on any column names returned by statement metadata.
            /// </summary>
            UPPERCASE
        }
    }
}
