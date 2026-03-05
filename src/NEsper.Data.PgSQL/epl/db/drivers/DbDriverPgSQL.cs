///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.Common;

using com.espertech.esper.common.@internal.db.drivers;

using Npgsql;

namespace com.espertech.esper.epl.db.drivers {
    /// <summary>
    /// A database driver specific to the NPGSQL driver.
    /// </summary>
    public class DbDriverPgSQL : BaseDbDriver, IDisposable
    {
        /// <summary>
        /// Flag indicating whether to use internal connection pooling.
        /// </summary>
        private readonly bool _useConnectionPool;

        /// <summary>
        /// A connection pool for database connections.  Connections are returned to the pool when they are closed or disposed.
        /// Only the connection pool will completely close and dispose of the connection.
        /// </summary>
        private ConnectionPool _connectionPool;

        /// <summary>
        /// Initialize the <see cref="DbDriverPgSQL"/> class.
        /// </summary>
        public DbDriverPgSQL() : this(true) {
        }

        /// <summary>
        /// Initialize the <see cref="DbDriverPgSQL"/> class with a flag indicating whether to use connection pooling.
        /// </summary>
        /// <param name="useConnectionPool">Flag indicating whether to use connection pooling.</param>
        public DbDriverPgSQL(bool useConnectionPool) {
            _useConnectionPool = useConnectionPool;
        }

        /// <summary>
        /// Disposes the connection pool.
        /// </summary>
        public virtual void Dispose() {
            _connectionPool?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Factory method that is used to create instance of a connection.
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection() {
            DbConnection dbConnection;
            dbConnection = _connectionPool != null ? _connectionPool.Acquire() : new NpgsqlConnection(ConnectionString);
            dbConnection.Open();
            return dbConnection;
        }

        /// <summary>
        /// Gets a value indicating whether [use position parameters].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use position parameters]; otherwise, <c>false</c>.
        /// </value>
        protected override bool UsePositionalParameters => false;

        /// <summary>
        /// Gets the parameter prefix.
        /// </summary>
        /// <value>The param prefix.</value>
        protected override string ParamPrefix => ":";

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <returns></returns>
        protected override DbConnectionStringBuilder CreateConnectionStringBuilder() {
            return new NpgsqlConnectionStringBuilder();
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public override string ConnectionString {
            get => base.ConnectionString;
            set {
                // this logic only applies if the value being passed in is different from the current value.
                var current = base.ConnectionString;
                if (value != current) {
                    base.ConnectionString = value;
                    _connectionPool?.Dispose();
                    if (_useConnectionPool) {
                        _connectionPool = new ConnectionPool(value, _ => new NpgsqlConnection(_), 10);
                    }
                }
            }
        }
    }
}