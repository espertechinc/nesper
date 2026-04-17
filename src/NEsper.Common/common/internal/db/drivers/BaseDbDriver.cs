///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

using com.espertech.esper.client.annotation;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.db;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.db.drivers
{
    /// <summary>
    /// An abstract base driver that provides some of the functionality
    /// that is common to all ADO.NET based drivers, but leaves the specifics
    /// of the database to the driver implementation.  ADO.NET leaves some
    /// wholes in its implementation and advises that for maximum performance
    /// that you use driver specific semantics.  This code exists to allow
    /// developers to integrate their own database models.
    /// </summary>
    public abstract class BaseDbDriver : DbDriver
    {
        private const string SAMPLE_WHERECLAUSE_PLACEHOLDER = "$ESPER-SAMPLE-WHERE";

        /// <summary>
        /// Connection name
        /// </summary>
        private string _name;

        /// <summary>
        /// Connection properties
        /// </summary>
        private Properties _connectionProperties;

        /// <summary>
        /// Connection string
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <returns></returns>
        protected abstract DbConnectionStringBuilder CreateConnectionStringBuilder();

        /// <summary>
        /// Gets the default meta origin policy.
        /// </summary>
        /// <value>The default meta origin policy.</value>
        public virtual MetadataOriginEnum DefaultMetaOriginPolicy => MetadataOriginEnum.DEFAULT;

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected abstract string ParamPrefix { get; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public virtual string ConnectionString {
            get => _connectionString;
            set => _connectionString = value;
        }

        protected BaseDbDriver()
        {
        }

        /// <summary>
        /// Gets a value indicating whether [use position parameters].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use position parameters]; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool UsePositionalParameters => false;

        #region "PositionalToTextConversion"

        /// <summary>
        /// Converts a positional parameter into text that can be embedded
        /// into the command text.
        /// </summary>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        protected delegate string PositionalToTextConverter(int parameterIndex);

        /// <summary>
        /// Gets the text for the parameter at the given index.
        /// </summary>
        /// <param name="parameterIndex">MapIndex of the parameter.</param>
        /// <returns></returns>
        protected string PositionalToNamedTextConverter(int parameterIndex)
        {
            return $"{ParamPrefix}arg{parameterIndex}";
        }

        /// <summary>
        /// Gets the text for the parameter at the given index.
        /// </summary>
        /// <param name="parameterIndex">MapIndex of the parameter.</param>
        /// <returns></returns>
        protected string PositionalToPositionalTextConverter(int parameterIndex)
        {
            return ParamPrefix;
        }

        /// <summary>
        /// Gets the positional to text converter.
        /// </summary>
        protected PositionalToTextConverter ParamToTextConverter {
            get {
                if (UsePositionalParameters) {
                    return PositionalToPositionalTextConverter;
                }
                else {
                    return PositionalToNamedTextConverter;
                }
            }
        }

        #endregion "PositionalToTextConversion"

        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public abstract DbConnection CreateConnection();

        /// <summary>
        /// Creates a connection using the internal mechanism.  Avoids having
        /// to make CreateConnection protected internal.  Its primary use is
        /// by the BaseDbDriverCommand.
        /// </summary>
        /// <returns></returns>
        protected internal DbConnection CreateConnectionInternal()
        {
            return CreateConnection();
        }

        /// <summary>
        /// Sets the transaction isolation.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="isolationLevel">The isolation level.</param>
        protected virtual void SetTransactionIsolation(
            DbConnection connection,
            IsolationLevel? isolationLevel)
        {
            try {
                if (isolationLevel != null) {
                    // Begin a transaction to provide the proper isolation.  Need to ensure
                    // that the transaction is properly committed upon completion since we
                    // do not have auto-commit handled.
                    connection.BeginTransaction(isolationLevel.Value);
                }
            }
            catch (DbException ex) {
                throw new DatabaseConfigException(
                    "Error setting transaction isolation level to " +
                    isolationLevel +
                    " on connection with detail " +
                    GetDetail(ex),
                    ex);
            }
        }

        /// <summary>
        /// Sets the catalog.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="catalog">The catalog.</param>
        protected virtual void SetCatalog(
            DbConnection connection,
            string catalog)
        {
            try {
                if (catalog != null) {
                    connection.ChangeDatabase(catalog);
                }
            }
            catch (DbException ex) {
                throw new DatabaseConfigException(
                    "Error setting catalog to '" +
                    catalog +
                    "' on connection with detail " +
                    GetDetail(ex),
                    ex);
            }
        }

        /// <summary>
        /// Sets the automatic commits.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="useAutoCommit">The use auto commit.</param>
        protected virtual void SetAutoCommit(
            DbConnection connection,
            bool? useAutoCommit)
        {
            try {
                if (useAutoCommit ?? false) {
                    throw new NotSupportedException("AutoCommit semantics not yet supported in this version");
                }
            }
            catch (DbException ex) {
                throw new DatabaseConfigException(
                    "Error setting auto-commit to " +
                    useAutoCommit +
                    " on connection with detail " +
                    GetDetail(ex),
                    ex);
            }
        }

        /// <summary> Method to set connection-level configuration settings.</summary>
        /// <param name="connection">is the connection to set on
        /// </param>
        /// <param name="connectionSettings">are the settings to apply
        /// </param>
        /// <throws>  DatabaseConfigException is thrown if an DbException is thrown </throws>
        protected virtual void ApplyConnectionOptions(
            DbConnection connection,
            ConnectionSettings connectionSettings)
        {
            SetTransactionIsolation(connection, connectionSettings.TransactionIsolation);
            SetCatalog(connection, connectionSettings.Catalog);
            SetAutoCommit(connection, connectionSettings.AutoCommit);
        }

        /// <summary>
        /// Sets the connection options using the default connection options.
        /// </summary>
        /// <param name="connection">The connection.</param>
        protected virtual void ApplyConnectionOptions(DbConnection connection)
        {
        }

        /// <summary>
        /// Gets the detail.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static string GetDetail(DbException ex)
        {
            return
                "DbException: " +
                ex.Message +
                " VendorError: " +
                ex.ErrorCode;
        }

        #region DbDriver Members

        /// <summary>
        /// Creates a database driver command from a collection of fragments.
        /// </summary>
        /// <param name="sqlFragments">The SQL fragments.</param>
        /// <param name="metadataSettings">The metadata settings.</param>
        /// <param name="contextAttributes">The context attributes.</param>
        /// <returns></returns>
        public virtual DbDriverCommand CreateCommand(
            IEnumerable<PlaceholderParser.Fragment> sqlFragments,
            ColumnSettings metadataSettings,
            IEnumerable<Attribute> contextAttributes)
        {
            int? dbCommandTimeout = null;

            // Determine if we have a SQLTimeoutAttribute specified within this context.  If so,
            // it must be applied to the command timeout.
            var timeoutAttribute = (TimeoutAttribute)contextAttributes?.FirstOrDefault(
                contextAttribute => contextAttribute is TimeoutAttribute);
            if (timeoutAttribute != null) {
                dbCommandTimeout = timeoutAttribute.Value;
            }

            // How do we convert from positional to underlying
            var paramConverter = ParamToTextConverter;
            // List of parameters
            var parameters = new List<string>();
            // Command text builder
            var buffer = new StringBuilder();
            // Counter for parameters
            var parameterCount = 0;
            foreach (var fragment in sqlFragments) {
                if (!fragment.IsParameter) {
                    buffer.Append(fragment.Value);
                }
                else if (fragment.Value == SAMPLE_WHERECLAUSE_PLACEHOLDER) {
                    continue;
                }
                else {
                    var parameter = paramConverter.Invoke(parameterCount++);
                    // Add the parameter to the parameter list
                    parameters.Add(parameter);
                    // Add the parameter to the command text
                    buffer.Append(parameter);
                }
            }

            var dbCommandText = buffer.ToString();
            return new BaseDbDriverCommand(
                this,
                sqlFragments,
                parameters,
                dbCommandText,
                dbCommandTimeout,
                metadataSettings);
        }

        /// <summary>
        /// Gets or sets the properties for the driver.
        /// </summary>
        /// <value>The properties.</value>
        public virtual Properties Properties {
            get => _connectionProperties;
            set {
                _connectionProperties = value;

                // Look for the term "connectionString" in the properties.  If it is not specified
                // then use the other items in the properties to drive a connection string builder.

                string propConnectionString;
                
                if (!_connectionProperties.TryGetValue(
                        DriverConfiguration.PROPERTY_CONNECTION_STRING,
                        out propConnectionString) &&
                    !_connectionProperties.TryGetValue(
                        DriverConfiguration.PROPERTY_CONNECTION_STRING_HYPHENATED,
                        out propConnectionString)) {
                    // Create the connection string; to do so, we require a connection
                    // string builder.  These are native to every connection class and
                    // ADO.NET providers will provide them to you natively.  We require
                    // that the implementation class provide us with one of these.
                    var builder = CreateConnectionStringBuilder();

                    foreach (var entry in _connectionProperties) {
                        var ekey = entry.Key;
                        var evalue = entry.Value;
                        if (string.Equals(ekey, "name", StringComparison.CurrentCultureIgnoreCase)) {
                            _name = evalue;
                        }
                        else {
                            builder.Add(ekey, evalue);
                        }
                    }

                    ConnectionString = builder.ConnectionString;
                }
                else {
                    ConnectionString = propConnectionString;
                }
            }
        }

        /// <summary>
        /// Connection name
        /// </summary>
        public virtual string Name => _name;

        #endregion DbDriver Members
    }
}