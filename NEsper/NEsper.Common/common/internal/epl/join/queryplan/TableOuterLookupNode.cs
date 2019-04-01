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
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    /// Specifies exection of a table lookup with outer join using the a specified lookup plan.
    /// </summary>
    public class TableOuterLookupNode : QueryPlanNode
    {
        private TableLookupPlan tableLookupPlan;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableLookupPlan">plan for performing lookup</param>
        public TableOuterLookupNode(TableLookupPlan tableLookupPlan)
        {
            this.tableLookupPlan = tableLookupPlan;
        }

        /// <summary>
        /// Returns lookup plan.
        /// </summary>
        /// <returns>lookup plan</returns>
        public TableLookupPlan LookupStrategySpec {
            get => tableLookupPlan;
        }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews, 
            VirtualDWView[] viewExternal, 
            ILockable[] tableSecondaryIndexLocks)
        {
            JoinExecTableLookupStrategy lookupStrategy = tableLookupPlan.MakeStrategy(
                agentInstanceContext, indexesPerStream, streamTypes, viewExternal);
            int indexedStream = tableLookupPlan.IndexedStream;
            if (tableSecondaryIndexLocks[indexedStream] != null) {
                return new TableOuterLookupExecNodeTableLocking(
                    indexedStream, lookupStrategy, tableSecondaryIndexLocks[indexedStream]);
            }

            return new TableOuterLookupExecNode(tableLookupPlan.IndexedStream, lookupStrategy);
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            usedIndexes.AddAll(Arrays.AsList(tableLookupPlan.IndexNum));
        }
    }
} // end of namespace