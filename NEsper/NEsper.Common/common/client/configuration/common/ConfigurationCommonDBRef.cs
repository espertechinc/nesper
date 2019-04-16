///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
            SqlTypesMapping = new Dictionary<int, string>();
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
        ///     The runtimerequires to retrieve result column names and types in order to build a resulting
        ///     event type and perform expression type checking.
        /// </summary>
        /// <returns>indication how to retrieve metadata</returns>
        public MetadataOriginEnum MetadataRetrievalEnum { get; set; }

        /// <summary>
        ///     Returns enum value determining how the runtimechanges case on output column names
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
        public int ConnectionTransactionIsolation {
            set => ConnectionSettings.TransactionIsolation = value;
        }

        /// <summary>
        ///     Sets the read-only flag on new connections created for this database.
        /// </summary>
        /// <value>is the read-only flag</value>
        public bool ConnectionReadOnly {
            set => ConnectionSettings.ReadOnly = value;
        }

        /// <summary>
        ///     Sets the catalog name for new connections created for this database.
        /// </summary>
        /// <value>is the catalog name</value>
        public string ConnectionCatalog {
            set => ConnectionSettings.Catalog = value;
        }

        /// <summary>
        ///     Returns the mapping of types that the runtimemust perform
        ///     when receiving output columns of that sql types.
        /// </summary>
        /// <value>map of &lt;seealso cref="java.sql.Types" /&gt; types to Java types</value>
        public IDictionary<int, string> SqlTypesMapping { get; }

        /// <summary>
        ///     Set the connection factory to use a factory class that provides an instance of
        ///     <seealso cref="javax.sql.DataSource" />.
        ///     <para />
        ///     This method is designed for use with Apache Commons DBCP and its BasicDataSourceFactory
        ///     but can also work for any application-provided factory for DataSource instances.
        ///     <para />
        ///     When using Apache DBCP, specify BasicDataSourceFactory.class.getName() as the class name
        ///     and populate all properties that Apache DBCP takes for connection pool configuration.
        ///     <para />
        ///     When using an application-provided data source factory, pass the class name of
        ///     a class that provides a public static method createDataSource(Properties properties) returning DataSource.
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
        ///     Sets the connection factory to use <seealso cref="javax.sql.DataSource" /> to obtain a
        ///     connection.
        /// </summary>
        /// <param name="contextLookupName">is the object name to look up via &lt;seealso cref="javax.naming.InitialContext" /&gt;</param>
        /// <param name="environmentProps">are the optional properties to pass to the context</param>
        public void SetDataSourceConnection(
            string contextLookupName,
            Properties environmentProps)
        {
            ConnectionFactoryDesc = new DataSourceConnection(contextLookupName, environmentProps);
        }

        /// <summary>
        ///     Sets the connection factory to use <seealso cref="java.sql.DriverManager" /> to obtain a
        ///     connection.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="url">is the URL</param>
        /// <param name="connectionArgs">are optional connection arguments</param>
        public void SetDriverManagerConnection(
            string className,
            string url,
            Properties connectionArgs)
        {
            ConnectionFactoryDesc = new DriverManagerConnection(className, url, connectionArgs);
        }

        /// <summary>
        ///     Sets the connection factory to use <seealso cref="java.sql.DriverManager" /> to obtain a
        ///     connection.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="url">is the URL</param>
        /// <param name="username">is the username to obtain a connection</param>
        /// <param name="password">is the password to obtain a connection</param>
        public void SetDriverManagerConnection(
            string className,
            string url,
            string username,
            string password)
        {
            ConnectionFactoryDesc = new DriverManagerConnection(className, url, username, password);
        }

        /// <summary>
        ///     Sets the connection factory to use <seealso cref="java.sql.DriverManager" /> to obtain a
        ///     connection.
        /// </summary>
        /// <param name="className">is the driver class name</param>
        /// <param name="url">is the URL</param>
        /// <param name="username">is the username to obtain a connection</param>
        /// <param name="password">is the password to obtain a connection</param>
        /// <param name="connectionArgs">are optional connection arguments</param>
        public void SetDriverManagerConnection(
            string className,
            string url,
            string username,
            string password,
            Properties connectionArgs)
        {
            ConnectionFactoryDesc = new DriverManagerConnection(className, url, username, password, connectionArgs);
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
                maxAgeSeconds, purgeIntervalSeconds, CacheReferenceType.DEFAULT);
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
                maxAgeSeconds, purgeIntervalSeconds, cacheReferenceType);
        }

        /// <summary>
        ///     Sets and indicator how the runtimeshould retrieve metadata about the columns
        ///     that a given SQL query returns.
        ///     <para />
        ///     The runtimerequires to retrieve result column names and types in order to build a resulting
        ///     event type and perform expression type checking.
        /// </summary>
        /// <param name="metadataOrigin">indication how to retrieve metadata</param>
        public void SetMetadataOrigin(MetadataOriginEnum metadataOrigin)
        {
            MetadataRetrievalEnum = metadataOrigin;
        }

        /// <summary>
        ///     Sets enum value determining how the runtimeshould change case on output column names
        ///     returned from statement or statement result set metadata.
        /// </summary>
        /// <param name="columnChangeCaseEnum">change case enums</param>
        public void SetColumnChangeCase(ColumnChangeCaseEnum columnChangeCaseEnum)
        {
            ColumnChangeCase = columnChangeCaseEnum;
        }

        /// <summary>
        ///     Adds a mapping of a sqlType to a type.
        ///     <para />
        ///     The mapping dictates to the runtimehow the output column should be
        ///     represented as an object.
        ///     <para />
        ///     Accepts a classname (fully-qualified or simple) or primitive type name
        ///     for the type parameter. See <seealso cref="DatabaseTypeEnum" /> for valid values for the type name.
        /// </summary>
        /// <param name="sqlType">is a sqlType constant, for which output columns are converted to type</param>
        /// <param name="typeName">is a type name</param>
        public void AddSqlTypesBinding(
            int sqlType,
            string typeName)
        {
            var typeEnum = DatabaseTypeEnum.GetEnum(typeName);
            if (typeEnum == null) {
                string supported = CompatExtensions.RenderAny(DatabaseTypeEnum.Values);
                throw new ConfigurationException(
                    "Unsupported type '" + typeName + "' when expecting any of: " + supported);
            }

            SqlTypesMapping.Put(sqlType, typeName);
        }

        /// <summary>
        ///     Adds a mapping of a java.sql.Types type to a type.
        ///     <para />
        ///     The mapping dictates to the runtimehow the output column should be
        ///     represented as an object.
        ///     <para />
        ///     Accepts a type for the type parameter. See <seealso cref="DatabaseTypeEnum" /> for valid values.
        /// </summary>
        /// <param name="sqlType">is a java.sql.Types constant, for which output columns are converted to java type</param>
        /// <param name="typeName">is a type</param>
        public void AddSqlTypesBinding(
            int sqlType,
            Type typeName)
        {
            AddSqlTypesBinding(sqlType, typeName.Name);
        }
    }
} // end of namespace