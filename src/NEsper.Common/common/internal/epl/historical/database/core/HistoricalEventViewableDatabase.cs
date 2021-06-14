///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.execstrategy;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    public class HistoricalEventViewableDatabase : HistoricalEventViewableBase
    {
        public HistoricalEventViewableDatabase(
            HistoricalEventViewableDatabaseFactory factory,
            PollExecStrategy pollExecStrategy,
            AgentInstanceContext agentInstanceContext)
            : base(factory, pollExecStrategy, agentInstanceContext)
        {
            try {
                DataCache = agentInstanceContext.DatabaseConfigService.GetDataCache(
                    factory.DatabaseName,
                    agentInstanceContext,
                    factory.StreamNumber,
                    factory.ScheduleCallbackId);
            }
            catch (DatabaseConfigException e) {
                throw new EPException("Failed to obtain cache: " + e.Message, e);
            }
        }
    }
} // end of namespace