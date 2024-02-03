///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies exection of a table lookup with outer join using the a specified lookup plan.
    /// </summary>
    public class TableOuterLookupNode : QueryPlanNode
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tableLookupPlan">plan for performing lookup</param>
        public TableOuterLookupNode(TableLookupPlan tableLookupPlan)
        {
            LookupStrategySpec = tableLookupPlan;
        }

        /// <summary>
        ///     Returns lookup plan.
        /// </summary>
        /// <returns>lookup plan</returns>
        public TableLookupPlan LookupStrategySpec { get; }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            var lookupStrategy = LookupStrategySpec.MakeStrategy(
                agentInstanceContext,
                indexesPerStream,
                streamTypes,
                viewExternal);
            var indexedStream = LookupStrategySpec.IndexedStream;
            if (tableSecondaryIndexLocks[indexedStream] != null) {
                return new TableOuterLookupExecNodeTableLocking(
                    indexedStream,
                    lookupStrategy,
                    tableSecondaryIndexLocks[indexedStream]);
            }

            return new TableOuterLookupExecNode(LookupStrategySpec.IndexedStream, lookupStrategy);
        }

        public void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes)
        {
            usedIndexes.AddAll(Arrays.AsList(LookupStrategySpec.IndexNum));
        }
    }
} // end of namespace