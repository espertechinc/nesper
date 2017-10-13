///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.virtualdw
{
    public interface VirtualDWView : IDisposable
    {
        VirtualDataWindow VirtualDataWindow { get; }

        Pair<IndexMultiKey, EventTable> GetSubordinateQueryDesc(
            Boolean unique,
            IList<IndexedPropDesc> hashedProps,
            IList<IndexedPropDesc> btreeProps);

        SubordTableLookupStrategy GetSubordinateLookupStrategy(
            string accessedByStatementName,
            int accessedByStatementId,
            Attribute[] accessedByStmtAnnotations,
            EventType[] outerStreamTypes,
            IList<SubordPropHashKey> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            IList<SubordPropRangeKey> rangeKeys,
            CoercionDesc rangeKeyCoercionTypes,
            bool nwOnTrigger,
            EventTable eventTable,
            SubordPropPlan joinDesc,
            bool forceTableScan);

        EventTable GetJoinIndexTable(QueryPlanIndexItem queryPlanIndexItem);

        JoinExecTableLookupStrategy GetJoinLookupStrategy(
            string accessedByStatementName,
            int accessedByStatementId,
            Attribute[] accessedByStmtAnnotations,
            EventTable[] eventTable,
            TableLookupKeyDesc keyDescriptor,
            int lookupStreamNum);

        Pair<IndexMultiKey, EventTable> GetFireAndForgetDesc(
            ISet<String> keysAvailable,
            ISet<String> rangesAvailable);

        ICollection<EventBean> GetFireAndForgetData(
            EventTable eventTable,
            Object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] accessedByStmtAnnotations);

        void HandleStartIndex(CreateIndexDesc spec);
        void HandleStopIndex(CreateIndexDesc spec);
        void HandleStopWindow();
    }
}