///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Caches the Connection and PreparedStatement instance for reuse.
    /// </summary>
    public class ConnectionCacheImpl : ConnectionCache
    {
        private Pair<DbDriver, DbDriverCommand> cache;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="databaseConnectionFactory">connection factory</param>
        /// <param name="sql">statement sql</param>
        /// <param name="contextAttributes">statement contextual attributes</param>
        public ConnectionCacheImpl(
            DatabaseConnectionFactory databaseConnectionFactory,
            string sql,
            IEnumerable<Attribute> contextAttributes)
            : base(databaseConnectionFactory, sql, contextAttributes)
        {
        }

        public override Pair<DbDriver, DbDriverCommand> Connection {
            get {
                if (cache == null) {
                    cache = MakeNew();
                }

                return cache;
            }
        }

        public override void DoneWith(Pair<DbDriver, DbDriverCommand> pair)
        {
            // no need to implement
        }

        public override void Dispose()
        {
            if (cache != null) {
                Close(cache);
            }

            cache = null;
        }
    }
} // end of namespace