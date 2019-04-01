///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.epl.historical.datacache;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    ///     Service providing database connection factory and configuration information
    ///     for use with historical data polling.
    /// </summary>
    public interface DatabaseConfigServiceRuntime : DatabaseConfigService
    {
        /// <summary>
        ///     Returns a new cache implementation for this database.
        /// </summary>
        /// <param name="databaseName">is the name of the database to return a new cache implementation for for</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        /// <param name="streamNumber">stream number</param>
        /// <param name="scheduleCallbackId">callback id</param>
        /// <returns>cache implementation</returns>
        /// <throws>DatabaseConfigException is thrown to indicate database configuration errors</throws>
        HistoricalDataCache GetDataCache(
            string databaseName,
            AgentInstanceContext agentInstanceContext, 
            int streamNumber,
            int scheduleCallbackId);

        ConnectionCache GetConnectionCache(
            string databaseName,
            string preparedStatementText,
            IEnumerable<Attribute> contextAttributes);
    }
} // end of namespace