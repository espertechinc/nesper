///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public interface VirtualDWView
    {
        VirtualDataWindow VirtualDataWindow { get; }

        EventType EventType { get; }

        SubordTableLookupStrategy GetSubordinateLookupStrategy(
            SubordTableLookupStrategyFactoryVDW subordTableFactory,
            AgentInstanceContext agentInstanceContext);

        JoinExecTableLookupStrategy GetJoinLookupStrategy(
            TableLookupPlan tableLookupPlan,
            AgentInstanceContext agentInstanceContext,
            EventTable[] eventTables,
            int lookupStream);

        ICollection<EventBean> GetFireAndForgetData(
            EventTable eventTable,
            object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] annotations);

        void HandleStopIndex(
            string indexName,
            QueryPlanIndexItem explicitIndexDesc);

        void HandleStartIndex(
            string indexName,
            QueryPlanIndexItem explicitIndexDesc);

        void HandleDestroy(int agentInstanceId);

        void Destroy();
    }
} // end of namespace