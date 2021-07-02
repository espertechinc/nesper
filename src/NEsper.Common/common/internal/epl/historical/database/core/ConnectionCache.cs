///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.Common;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    /// Base class for a Connection and PreparedStatement cache.
    /// <para>Implementations control the lifecycle via lifecycle methods, or
    /// may simple obtain new resources and close new resources every time.
    /// </para>
    /// <para>
    /// This is not a pool - a cache is associated with one client class and that
    /// class is expected to use cache methods in well-defined order of get, done-with and destroy.
    /// </para>
    /// </summary>
    public abstract class ConnectionCache : IDisposable
    {
        private readonly DatabaseConnectionFactory _databaseConnectionFactory;
        private readonly string _sql;
        private readonly IList<PlaceholderParser.Fragment> _sqlFragments;
        private readonly IEnumerable<Attribute> _contextAttributes;

        /// <summary>
        /// Returns a cached or new connection and statement pair.
        /// </summary>
        /// <returns>connection and statement pair</returns>
        public abstract Pair<DbDriver, DbDriverCommand> Connection { get; }

        /// <summary>
        /// Indicate to return the connection and statement pair after use.
        /// </summary>
        /// <param name="pair">is the resources to return</param>
        public abstract void DoneWith(Pair<DbDriver, DbDriverCommand> pair);

        /// <summary>
        /// Destroys cache closing all resources cached, if any.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="databaseConnectionFactory">connection factory</param>
        /// <param name="sql">statement sql</param>
        /// <param name="contextAttributes">statement contextual attributes</param>
        protected ConnectionCache(
            DatabaseConnectionFactory databaseConnectionFactory,
            string sql,
            IEnumerable<Attribute> contextAttributes)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _sql = sql;
            _sqlFragments = PlaceholderParser.ParsePlaceholder(sql);
            _contextAttributes = contextAttributes;
        }

        /// <summary>
        /// Close resources.
        /// </summary>
        /// <param name="pair">is the resources to close.</param>
        protected internal static void Close(Pair<DbDriver, DbDriverCommand> pair)
        {
            Log.Info(".close Closing statement and connection");
            try {
                pair.Second.Dispose();
            }
            catch (DbException ex) {
                throw new EPException("Error closing statement", ex);
            }
        }

        /// <summary>
        /// Make a new pair of resources.
        /// </summary>
        /// <returns>pair of resources</returns>
        protected Pair<DbDriver, DbDriverCommand> MakeNew()
        {
            Log.Info(".MakeNew Obtaining new connection and statement");

            try {
                // Get the driver
                DbDriver dbDriver = _databaseConnectionFactory.Driver;
                // Get the command
                DbDriverCommand dbCommand = dbDriver.CreateCommand(_sqlFragments, null, _contextAttributes);

                return new Pair<DbDriver, DbDriverCommand>(dbDriver, dbCommand);
            }
            catch (DatabaseConfigException ex) {
                throw new EPException("Error obtaining connection", ex);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConnectionCache));
    }
} // end of namespace