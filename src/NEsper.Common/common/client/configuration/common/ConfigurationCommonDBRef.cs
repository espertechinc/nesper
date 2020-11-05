///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;

using com.espertech.esper.common.client.db;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Container for database configuration information, such as
    ///     options around getting a database connection and options to control the lifecycle
    ///     of connections and set connection parameters.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonDBRef
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonDBRef()
        {
            ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            ConnectionSettings = new ConnectionSettings();
            MetadataRetrievalEnum = MetadataOriginEnum.DEFAULT;
            ColumnChangeCase = ColumnChangeCaseEnum.NONE;
            DataTypesMapping = new Dictionary<Type, Type>();
        }

        /// <summary>
        ///     Returns the connection settings for this database.
        /// </summary>
        /// <returns>connection settings</returns>
        public ConnectionSettings ConnectionSettings { get; }

        /// <summary>
        ///     Returns the setting to control whether a new connection is obtained for each lookup,
        ///     or connections are retained between lookups.
        /// </summary>
        /// <returns>enum controlling connection allocation</returns>
        public ConnectionLifecycleEnum ConnectionLifecycleEnum { get; set; }

        /// <summary>
        ///     Returns the descriptor controlling connection creation settings.
        /// </summary>
        /// <returns>connection factory settings</returns>
        public ConnectionFactoryDesc ConnectionFactoryDesc { get; set; }

        /// <summary>
        ///     Return a query result data cache descriptor.
        /// </summary>
        /// <returns>cache descriptor</returns>
        public ConfigurationCommonCache DataCacheDesc { get; set; }

        /// <summary>
        ///     Returns an enumeration indicating how the runtime retrieves metadata about the columns
        ///     that a given SQL query returns.
        ///     <para />
        ///     The runtime requires to retrieve result column names and types in order to build a resulting
        ///     event type and perform expression type checking.
        /// </summary>
        /// <returns>indication how to retrieve metadata</returns>
        public MetadataOriginEnum MetadataRetrievalEnum { get; set; }

        /// <summary>
        ///     Returns enum value determining how the runtime changes case on output column names
        ///     returned from statement or statement result set metadata.
        /// </summary>
        /// <returns>change case enums</returns>
        public ColumnChangeCaseEnum ColumnChangeCase { get; set; }

        /// <summary>
        ///     Sets the auto-commit connection settings for new connections to this database.
        /// </summary>
        /// <value>
        ///     is true to set auto-commit to true, or false to set auto-commit to false, or null to accepts the
        ///     default
        /// </value>
        public bool ConnectionAutoCommit {
            set => ConnectionSettings.AutoCommit = value;
        }

        /// <summary>
        ///     Sets the transaction isolation level on new connections created for this database.
        /// </summary>
        /// <value>is the transaction isolation level</value>
        public IsolationLevel? ConnectionTransactionIsolation {
            get => ConnectionSettings.TransactionIsolation;
            set => ConnectionSettings.TransactionIsolation = value;
        }

        /// <summary>
        ///     Sets the read-only flag on new connections created for this database.
        /// </summary>
        /// <value>is the read-only flag</value>
        public bool ConnectionReadOnly {
            get => ConnectionSettings.ReadOnly;
            set => ConnectionSettings.ReadOnly = value;
        }

        /// <summary>
        ///     Sets the catalog name for new connections created for this database.
        /// </summary>
        /// <value>is the catalog name</value>
        public string ConnectionCatalog {
            get => ConnectionSettings.Catalog;
            set => ConnectionSettings.Catalog = value;
        }

        /// <summary>
        ///     Returns the mapping of types that the runtime must perform
        ///     when receiving output columns of that sql types.
        /// </summary>
        public IDictionary<Type, Type> DataTypesMapping { get; }

        /// <summary>
        ///     Sets and indicator how the runtime should retrieve metadata about the columns
        ///     that a given SQL query returns.
        ///     <para />
        ///     The runtime requires to retrieve result column names and types in order to build a resulting
        ///     event type and perform expression type checking.
        /// </summary>
        /// <value>indication how to retrieve metadata</value>
        public MetadataOriginEnum MetadataOrigin {
            set => MetadataRetrievalEnum = value;
        }

        /// <summary>
        ///     Set the connection factory to use a factory class.
        /// </summary>
        /// <param name="dataSourceFactoryClassName">the classname of the data source factory</param>
        /// <param name="properties">passed to the createDataSource method of the data source factory class</param>
        public void SetDataSourceFactory(
            Properties properties,
            string dataSourceFactoryClassName)
        {
            ConnectionFactoryDesc = new DataSourceFactory(properties, dataSourceFactoryClassName);
        }

        /// <summary>
        ///     Sets the connection factory to use to obtain a connection.
        /// </summary>
        /// <param name="contextLookupName">is the object name to look up</param>
        /// <param name="environmentProps">are the optional properties to pass to the context</param>
        public void SetDataSourceConnection(
            string contextLookupName,
            Properties environmentProps)
        {
            ConnectionFactoryDesc = new DataSourceConnection(contextLookupName, environmentProps);
        }

        /// <summary>
        /// Sets the database driver.
        /// </summary>
        /// <param name="driverConnectionFactoryDesc">The db driver factory connection.</param>
        public void SetDatabaseDriver(DriverConnectionFactoryDesc driverConnectionFactoryDesc)
        {
            ConnectionFactoryDesc = driverConnectionFactoryDesc;
        }

        /// <summary>Sets the database driver.</summary>
        /// <param name="driverName">Name of the driver.</param>
        /// <param name="connectionString">A specific connection string.</param>
        /// <param name="properties">The properties.</param>
        public void SetDatabaseDriver(
            string driverName,
            string connectionString,
            Properties properties)
        {
            if (!string.IsNullOrEmpty(connectionString)) {
                if (properties != null) {
                    properties = properties.Copy();
                }
                else {
                    properties = new Properties();
                }

                properties.Put(DriverConfiguration.PROPERTY_CONNECTION_STRING, connectionString);
            }

            ConnectionFactoryDesc = new DriverConnectionFactoryDesc(driverName, properties);
        }

        /// <summary>Sets the database driver.</summary>
        /// <param name="driverName">Name of the driver.</param>
        /// <param name="properties">The properties.</param>

        public void SetDatabaseDriver(
            string driverName,
            Properties properties)
        {
            ConnectionFactoryDesc = new DriverConnectionFactoryDesc(driverName, properties);
        }

        /// <summary>
        ///     Configures a LRU cache of the given size for the database.
        /// </summary>
        /// <param name="size">is the maximum number of entries before query results are evicted</param>
        public void SetLRUCache(int size)
        {
            DataCacheDesc = new ConfigurationCommonCacheLRU(size);
        }

        /// <summary>
        ///     Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds.
        ///     <para />
        ///     Specifies the cache reference type to be weak references. Weak reference cache entries become
        ///     eligible for garbage collection and are removed from cache when the garbage collection requires so.
        /// </summary>
        /// <param name="maxAgeSeconds">
        ///     is the maximum number of seconds before a query result is considered stale (also known as
        ///     time-to-live)
        /// </param>
        /// <param name="purgeIntervalSeconds">is the interval at which the runtime purges stale data from the cache</param>
        public void SetExpiryTimeCache(
            double maxAgeSeconds,
            double purgeIntervalSeconds)
        {
            DataCacheDesc = new ConfigurationCommonCacheExpiryTime(
                maxAgeSeconds,
                purgeIntervalSeconds,
                CacheReferenceType.DEFAULT);
        }

        /// <summary>
        ///     Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds. Also allows
        ///     setting the reference type indicating whether garbage collection may remove entries from cache.
        /// </summary>
        /// <param name="maxAgeSeconds">
        ///     is the maximum number of seconds before a query result is considered stale (also known as
        ///     time-to-live)
        /// </param>
        /// <param name="purgeIntervalSeconds">is the interval at which the runtime purges stale data from the cache</param>
        /// <param name="cacheReferenceType">specifies the reference type to use</param>
        public void SetExpiryTimeCache(
            double maxAgeSeconds,
            double purgeIntervalSeconds,
            CacheReferenceType cacheReferenceType)
        {
            DataCacheDesc = new ConfigurationCommonCacheExpiryTime(
                maxAgeSeconds,
                purgeIntervalSeconds,
                cacheReferenceType);
        }

        /// <summary>
        /// Adds the SQL types binding.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <param name="desiredType">The desired type.</param>
        public void AddTypeBinding(
            Type sqlType,
            Type desiredType)
        {
            try {
                DatabaseTypeEnumExtensions.GetEnum(desiredType.FullName);
                DataTypesMapping[sqlType] = desiredType;
            }
            catch (ArgumentException) {
                var supported = EnumHelper.GetValues<DatabaseTypeEnum>().RenderAny();
                throw new ConfigurationException(
                    "Unsupported type '" + desiredType.FullName + "' when expecting any of: " + supported);
            }
        }
    }
} // end of namespace