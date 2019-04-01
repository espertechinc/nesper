///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Implementation of a connection cache that simply doesn't cache but gets
	/// a new connection and statement every request, and closes these every time
	/// a client indicates done.
	/// </summary>
    
    public class ConnectionNoCacheImpl : ConnectionCache
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="databaseConnectionFactory">is the connection factory</param>
        /// <param name="sql">is the statement sql</param>
        /// <param name="contextAttributes">The context attributes.</param>
        public ConnectionNoCacheImpl(DatabaseConnectionFactory databaseConnectionFactory, String sql, IEnumerable<Attribute> contextAttributes)
            : base(databaseConnectionFactory, sql, contextAttributes)
        {
        }

        /// <summary>
        /// Returns a cached or new connection and statement pair.
        /// </summary>
        /// <returns>connection and statement pair</returns>
        public override Pair<DbDriver, DbDriverCommand> GetConnection()
        {
            return MakeNew();
        }

        /// <summary>
        /// Indicate to return the connection and statement pair after use.
        /// </summary>
        /// <param name="pair">is the resources to return</param>
        public override void DoneWith(Pair<DbDriver, DbDriverCommand> pair)
        {
            Close(pair);
        }

        /// <summary>
        /// Destroys cache closing all resources cached, if any.
        /// </summary>
        public override void Dispose()
        {
            // no resources held
        }
    }
}
