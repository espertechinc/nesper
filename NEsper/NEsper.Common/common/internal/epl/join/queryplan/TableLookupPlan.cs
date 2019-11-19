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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Abstract specification on how to perform a table lookup.
    /// </summary>
    public abstract class TableLookupPlan
    {
        internal readonly int indexedStream;
        internal readonly TableLookupIndexReqKey[] indexNum;
        internal readonly int lookupStream;

        public TableLookupPlan(
            int lookupStream,
            int indexedStream,
            TableLookupIndexReqKey[] indexNum)
        {
            this.lookupStream = lookupStream;
            this.indexedStream = indexedStream;
            this.indexNum = indexNum;
        }

        public int LookupStream => lookupStream;

        public int IndexedStream => indexedStream;

        public TableLookupIndexReqKey[] IndexNum => indexNum;

        public ExprEvaluator[] VirtualDWHashEvals { get; set; }

        public Type[] VirtualDWHashTypes { get; set; }

        public QueryGraphValueEntryRange[] VirtualDWRangeEvals { get; set; }

        public Type[] VirtualDWRangeTypes { get; set; }

        protected abstract JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTables,
            EventType[] eventTypes);

        public JoinExecTableLookupStrategy MakeStrategy(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] eventTypes,
            VirtualDWView[] viewExternals)
        {
            var eventTables = new EventTable[indexNum.Length];
            for (var i = 0; i < indexNum.Length; i++) {
                eventTables[i] = indexesPerStream[indexedStream].Get(IndexNum[i]);
            }

            if (viewExternals[indexedStream] != null) {
                return viewExternals[indexedStream]
                    .GetJoinLookupStrategy(
                        this,
                        agentInstanceContext,
                        eventTables,
                        lookupStream);
            }

            return MakeStrategyInternal(eventTables, eventTypes);
        }
    }
} // end of namespace