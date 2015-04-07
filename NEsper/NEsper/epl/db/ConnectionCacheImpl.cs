///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;


namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Caches the Connection and DbCommand instance for reuse.
    /// </summary>
    
    public class ConnectionCacheImpl : ConnectionCache
    {
        private Pair<DbDriver, DbDriverCommand> cache;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="databaseConnectionFactory">connection factory</param>
        /// <param name="sql">statement sql</param>
        /// <param name="contextAttributes">The context attributes.</param>

        public ConnectionCacheImpl(DatabaseConnectionFactory databaseConnectionFactory, String sql, IEnumerable<Attribute> contextAttributes)
            : base(databaseConnectionFactory, sql, contextAttributes)
        {
        }

        /// <summary>
        /// Returns a cached or new connection and statement pair.
        /// </summary>
        /// <returns>connection and statement pair</returns>
        public override Pair<DbDriver, DbDriverCommand> GetConnection()
        {
            if (cache == null)
            {
                cache = MakeNew();
            }
            return cache;
        }

        /// <summary>
        /// Indicate to return the connection and statement pair after use.
        /// </summary>
        /// <param name="pair">is the resources to return</param>
        public override void DoneWith(Pair<DbDriver, DbDriverCommand> pair)
        {
            // no need to implement
        }

        /// <summary>
        /// Destroys cache closing all resources cached, if any.
        /// </summary>
        public override void Dispose()
        {
            if (cache != null)
            {
                Close(cache);
            }
            cache = null;
        }
    }
}
