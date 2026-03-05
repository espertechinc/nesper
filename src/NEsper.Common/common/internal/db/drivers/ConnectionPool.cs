///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

namespace com.espertech.esper.common.@internal.db.drivers
{
        /// <summary>
        /// A connection pool for database connections.  Connections are returned to the pool when they are closed or disposed.
        /// Only the connection pool will completely close and dispose of the connection.
        /// </summary>
        public class ConnectionPool : IDisposable {
            /// <summary>
            /// The available unallocated connections in the pool.
            /// </summary>
            private readonly Queue<DbConnection> _availableConnections = new Queue<DbConnection>();

            /// <summary>
            /// The current size of the connection pool.
            /// </summary>
            private int _connectionPoolSize = 0;

            private readonly object _lock = new object();

            private readonly string _connectionString;

            private readonly int _maxPoolSize;

            private bool _disposed;

            private readonly Func<string, DbConnection> _connectionFactory;

            /// <summary>
            /// Initializes a new instance of the ConnectionPool class.
            /// </summary>
            /// <param name="connectionString">The connection string to use for the connections.</param>
            /// <param name="connectionFactory">The factory to create connections.</param>
            /// <param name="maxPoolSize">The maximum number of connections to keep in the pool.</param>
            public ConnectionPool(string connectionString, Func<string, DbConnection> connectionFactory, int maxPoolSize = 10) {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPoolSize);
                _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
                _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
                _maxPoolSize = maxPoolSize;
            }

            /// <summary>
            /// Acquires a connection from the pool.
            /// </summary>
            /// <returns></returns>
            public DbConnection Acquire() {
                lock (_lock) {
                    ObjectDisposedException.ThrowIf(_disposed, this);

                    while (_availableConnections.Count > 0) {
                        var connection = _availableConnections.Dequeue();
                        if (connection.State == System.Data.ConnectionState.Open) {
                            return new PooledConnection(this, connection);
                        }

                        connection.Dispose();
                        _connectionPoolSize--;
                    }

                    // there are no available connections, so we need to create a new one
                    // if we have not reached the maximum pool size
                    if (_connectionPoolSize < _maxPoolSize) {
                        var newConnection = _connectionFactory.Invoke(_connectionString);
                        _connectionPoolSize++;
                        return new PooledConnection(this, newConnection);
                    }

                    throw new InvalidOperationException(
                        $"Connection pool exhausted. Maximum pool size of {_maxPoolSize} reached.");
                }
            }


            /// <summary>
            /// Releases a connection back to the pool.
            /// </summary>
            /// <param name="connection"></param>
            public void Release(DbConnection connection) {
                if (connection == null) {
                    return;
                }

                lock (_lock) {
                    if (_disposed) {
                        connection.Dispose();
                        return;
                    }

                    _availableConnections.Enqueue(connection);
                }
            }

            /// <summary>
            /// Clears all available connections from the pool.
            /// </summary>
            public void Clear() {
                lock (_lock) {
                    while (_availableConnections.Count > 0) {
                        var connection = _availableConnections.Dequeue();
                        connection.Dispose();
                    }

                    _availableConnections.Clear();
                    _connectionPoolSize = 0;
                }
            }

            /// <summary>
            /// Releases all resources used by the ConnectionPool.
            /// </summary>
            public void Dispose() {
                lock (_lock) {
                    if (_disposed) {
                        return;
                    }

                    _disposed = true;
                    Clear();
                }
                GC.SuppressFinalize(this);
            }

        internal class PooledConnection : DbConnection {
            private readonly object _lock = new object();

            private ConnectionPool _pool;

            private DbConnection _connection;

            /// <summary>
            /// Initializes a new instance of the PooledConnection class.
            /// </summary>
            /// <param name="pool">The connection pool.</param>
            /// <param name="connection">The database connection.</param>
            public PooledConnection(ConnectionPool pool, DbConnection connection) {
                _pool = pool;
                _connection = connection;
            }

            /// <summary>
            /// Gets or sets the string used to open the connection.
            /// </summary>
            public override string ConnectionString {
                get => _connection?.ConnectionString ?? string.Empty;
                set {
                    lock(_lock) {
                        if (_connection != null) {
                            _connection.ConnectionString = value;
                        }
                    }
                }
            }

            public override string Database => _connection?.Database;
            public override string DataSource => _connection?.DataSource;
            public override string ServerVersion => _connection?.ServerVersion;
            public override System.Data.ConnectionState State => _connection?.State ?? System.Data.ConnectionState.Closed;

            /// <summary>
            /// Changes the current database for an open Connection object.
            /// </summary>
            /// <param name="databaseName">The name of the database to use instead of the current database.</param>
            public override void ChangeDatabase(string databaseName) {
                lock (_lock) {
                    ObjectDisposedException.ThrowIf(_connection is null, this);
                    _connection.ChangeDatabase(databaseName);
                }
            }

            /// <summary>
            /// Closes the connection to the database.  As this is a pooled connection, it is returned to the pool instead
            /// of being actually closed.
            /// </summary>
            public override void Close() {
                lock(_lock) {
                    _pool?.Release(_connection);
                    _pool = null;
                    _connection = null;
                }
            }

            /// <summary>
            /// Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.
            /// </summary>
            public override void Open()
            {
                lock (_lock) {
                    ObjectDisposedException.ThrowIf(_connection is null, this);
                    switch (_connection.State) {
                        case System.Data.ConnectionState.Closed:
                            _connection.Open();
                            break;

                        case System.Data.ConnectionState.Open:
                            // Connection is already open, do nothing
                            break;

                        default:
                            // For other states (Connecting, Executing, Fetching, etc.), we need to handle them
                            // This is a simplified approach - in a real implementation, you might want to 
                            // handle these states more explicitly
                            _connection.Open();
                            break;
                    }
                }
            }

            /// <summary>
            /// Creates and returns a Command object associated with the connection.
            /// </summary>
            /// <returns>A Command object associated with the connection.</returns>
            protected override DbCommand CreateDbCommand() {
                lock (_lock) {
                    ObjectDisposedException.ThrowIf(_connection is null, this);
                    return _connection.CreateCommand();
                }
            }

            /// <summary>
            /// Returns a string representation of the connection.
            /// </summary>
            /// <returns>A string representation of the connection.</returns>
            public override string ToString() {
                lock(_lock) {
                    return _connection?.ToString() ?? "Disposed PooledConnection";
                }
            }

            /// <summary>
            /// Releases the unmanaged resources used by the DbConnection and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing) {
                if (disposing) {
                    lock(_lock) {
                        _pool?.Release(_connection);
                        _pool = null;
                        _connection = null;
                    }
                }

                base.Dispose(disposing);
            }

            /// <summary>
            /// Begins a database transaction with the specified isolation level.
            /// </summary>
            /// <param name="isolationLevel">The isolation level under which the transaction should run.</param>
            /// <returns>A new transaction object.</returns>
            protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) {
                lock (_lock) {
                    ObjectDisposedException.ThrowIf(_connection is null, this);
                    return _connection.BeginTransaction(isolationLevel);
                }
            }
        }
    }
}