///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.historical.database.connection;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Implementation of a connection cache that simply doesn't cache but gets
    ///     a new connection and statement every request, and closes these every time
    ///     a client indicates done.
    /// </summary>
    public class ConnectionCacheNoCacheImpl : ConnectionCache
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="databaseConnectionFactory">is the connection factory</param>
        /// <param name="sql">is the statement sql</param>
        /// <param name="contextAttributes">statement contextual attributes</param>
        public ConnectionCacheNoCacheImpl(
            DatabaseConnectionFactory databaseConnectionFactory,
            string sql,
            IEnumerable<Attribute> contextAttributes)
            : base(databaseConnectionFactory, sql, contextAttributes)
        {
        }

        public override Pair<DbDriver, DbDriverCommand> Connection => MakeNew();

        public override void DoneWith(Pair<DbDriver, DbDriverCommand> pair)
        {
            Close(pair);
        }

        public override void Dispose()
        {
            // no resources held
        }
    }
} // end of namespace