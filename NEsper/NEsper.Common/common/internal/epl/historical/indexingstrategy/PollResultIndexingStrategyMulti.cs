///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyMulti : PollResultIndexingStrategy
    {
        public int StreamNum { get; set; }

        public PollResultIndexingStrategy[] IndexingStrategies { get; set; }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext)
        {
            if (!isActiveCache) {
                return new EventTable[] {new UnindexedEventTableList(pollResult, StreamNum)};
            }

            var tables = new EventTable[IndexingStrategies.Length];
            for (var i = 0; i < IndexingStrategies.Length; i++) {
                tables[i] = IndexingStrategies[i].Index(pollResult, isActiveCache, agentInstanceContext)[0];
            }

            var organization = new EventTableOrganization(
                null, false, false, StreamNum, null, EventTableOrganizationType.MULTIINDEX);
            return new EventTable[] {new MultiIndexEventTable(tables, organization)};
        }
    }
} // end of namespace